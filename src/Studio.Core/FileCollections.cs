using System.IO.Enumeration;
namespace Studio.Core;
public sealed record InstallFilePreview(string Source,string Destination);

public static class FileCollections
{
    public static IEnumerable<InstallFileSettings> Rules(StudioProject project) => new[]{project.InstallFile}.Concat(project.AdditionalFiles);
    public static void Validate(InstallFileSettings? rule)
    {
        if(rule is null || rule.Source is null || rule.Source.Length>4096 || rule.Source.Any(char.IsControl)
            || rule.Excludes is null || rule.Excludes.Count>100 || rule.Excludes.Any(p=>string.IsNullOrWhiteSpace(p) || p.Length>256
                || p.Any(char.IsControl) || p.IndexOfAny(['"',',','/','{','}',':'])>=0)) throw InnoScript.Error("InvalidFileRule");
        Destination(rule.Destination);
    }
    public static string Destination(string? value)
    {
        var destination=value?.Replace('/','\\') ?? "";
        if(!(destination=="{app}" || destination.StartsWith(@"{app}\",StringComparison.Ordinal))
            || destination[5..].IndexOfAny(['{','}','"','*','?','<','>','|',':'])>=0 || destination.Any(char.IsControl)
            || destination[5..].Split('\\').Any(p=>p is "." or ".." || p.EndsWith('.') || p.EndsWith(' ')))
            throw InnoScript.Error("ExportDestinationInvalid");
        return destination;
    }
    public static string ResolvePattern(InstallFileSettings rule,string? projectDirectory)
    {
        Validate(rule); var source=rule.Source;
        if(source.Contains("{#",StringComparison.Ordinal)) throw InnoScript.Error("ExportPreprocessorUnsupported");
        if(string.IsNullOrWhiteSpace(source) || source.IndexOfAny(['"','<','>','|'])>=0
            || Path.IsPathRooted(source) && !Path.IsPathFullyQualified(source)) throw InnoScript.Error("ExportSourceInvalid");
        if(!Path.IsPathFullyQualified(source) && string.IsNullOrWhiteSpace(projectDirectory)) throw InnoScript.Error("ExportSourceNeedsProject");
        var full=Path.IsPathFullyQualified(source) ? Path.GetFullPath(source) : Path.GetFullPath(source,projectDirectory!);
        var parent=Path.GetDirectoryName(full)!;
        if(parent.IndexOfAny(['*','?'])>=0) throw InnoScript.Error("ExportSourceInvalid");
        if(!Directory.Exists(parent)) throw InnoScript.Error("ExportSourceMissing");
        return full;
    }
    public static IReadOnlyList<InstallFilePreview> Preview(InstallFileSettings rule,string? projectDirectory,CancellationToken cancellationToken=default)
    {
        var source=ResolvePattern(rule,projectDirectory); var root=Path.GetDirectoryName(source)!; var pattern=Path.GetFileName(source);
        var wildcard=pattern.IndexOfAny(['*','?'])>=0; var destination=Destination(rule.Destination).TrimEnd('\\');
        var options=new EnumerationOptions { MatchType=MatchType.Win32,MatchCasing=MatchCasing.CaseInsensitive,AttributesToSkip=0,IgnoreInaccessible=false };
        var pending=new Stack<string>(); pending.Push(root); var files=new List<InstallFilePreview>(); var visited=0;
        while(pending.TryPop(out var directory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach(var path in Directory.EnumerateFiles(directory,pattern,options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative=Path.GetRelativePath(root,path);
                if(wildcard && (File.GetAttributes(path)&FileAttributes.Hidden)!=0 || Excluded(relative,rule.Excludes)) continue;
                if(files.Count>=10_000) throw InnoScript.Error("FilesPreviewTooLarge");
                var subdirectory=Path.GetRelativePath(root,directory);
                var target=wildcard ? relative : subdirectory=="." ? pattern : Path.Combine(subdirectory,pattern);
                files.Add(new(path,destination+"\\"+target));
            }
            if(!rule.RecurseSubdirectories) continue;
            foreach(var child in Directory.EnumerateDirectories(directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var attributes=File.GetAttributes(child);
                if((attributes&FileAttributes.Hidden)!=0 || Excluded(Path.GetRelativePath(root,child),rule.Excludes)) continue;
                if((attributes&FileAttributes.ReparsePoint)!=0) throw InnoScript.Error("FilesLinkedDirectory");
                if(++visited>50_000) throw InnoScript.Error("FilesPreviewTooLarge");
                pending.Push(child);
            }
        }
        if(files.Count==0) throw InnoScript.Error("ExportSourceMissing");
        return files.OrderBy(f=>f.Source,StringComparer.OrdinalIgnoreCase).ToArray();
    }
    private static bool Excluded(string path,IEnumerable<string> patterns)
    {
        var parts=path.Split('\\');
        foreach(var pattern in patterns)
        {
            var front=pattern.StartsWith('\\'); var value=front ? pattern[1..] : pattern;
            var count=value.Count(c=>c=='\\')+1;
            if(parts.Length<count) continue;
            var candidate=string.Join("\\",front ? parts.Take(count) : parts.TakeLast(count));
            if(SimpleMatch(value,candidate)) return true;
        }
        return false;
    }
    // Native Excludes treats backslashes literally, unlike FileSystemName's escape syntax.
    private static bool SimpleMatch(string pattern,string text)
    {
        var p=0; var t=0; var star=-1; var retry=0;
        while(t<text.Length)
        {
            if(p<pattern.Length && (pattern[p]=='?' || char.ToUpperInvariant(pattern[p])==char.ToUpperInvariant(text[t]))) { p++; t++; }
            else if(p<pattern.Length && pattern[p]=='*') { star=p++; retry=t; }
            else if(star>=0) { p=star+1; t=++retry; }
            else return false;
        }
        while(p<pattern.Length && pattern[p]=='*') p++;
        return p==pattern.Length;
    }
}
