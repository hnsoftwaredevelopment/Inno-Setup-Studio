using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Studio.Core;

public sealed record LanguageSource(string FilePath, string Name, int LanguageId, bool RightToLeft,
    IReadOnlyDictionary<string, string> Options, IReadOnlyDictionary<string, string> Messages,
    IReadOnlyDictionary<string, string> CustomMessages);
public sealed record LanguageSourceProblem(string FilePath, string ResourceKey);
public sealed record LanguageSourceCatalog(IReadOnlyList<LanguageSource> Sources, IReadOnlyList<LanguageSourceProblem> Problems);

public static class LanguageSourceFile
{
    public static async Task<LanguageSource> LoadAsync(string path)
    {
        if (!string.Equals(Path.GetExtension(path), ".isl", StringComparison.OrdinalIgnoreCase)) throw Error("LanguageSourceInvalid");
        await using var stream = File.OpenRead(path);
        if (stream.Length > 1_048_576) throw Error("LanguageSourceTooLarge");
        var bytes = new byte[checked((int)stream.Length)];
        await stream.ReadExactlyAsync(bytes);
        if (stream.ReadByte() != -1) throw Error("LanguageSourceTooLarge");
        string content;
        try { content = new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF'); }
        catch (DecoderFallbackException)
        {
            // Legacy ANSI files need an explicit code page; never guess the machine's encoding.
            var ascii = Encoding.ASCII.GetString(bytes);
            var options = Parse(ascii).Options;
            if (!options.TryGetValue("LanguageCodePage", out var value) || !int.TryParse(value, out var codePage) || codePage <= 0)
                throw Error("LanguageSourceEncoding");
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                content = Encoding.GetEncoding(codePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback).GetString(bytes);
            }
            catch (Exception error) when (error is ArgumentException or NotSupportedException) { throw Error("LanguageSourceEncoding"); }
        }
        var parsed = Parse(content);
        if (parsed.Options.TryGetValue("LanguageCodePage", out var page) && (!int.TryParse(page, out var codePageValue) || codePageValue < 0))
            throw Error("LanguageSourceInvalid");
        if (!parsed.Options.TryGetValue("LanguageName", out var name) || string.IsNullOrWhiteSpace(name)
            || !parsed.Options.TryGetValue("LanguageID", out var id) || !TryLanguageId(id, out var languageId)
            || parsed.Messages.Count == 0) throw Error("LanguageSourceInvalid");
        var rtl = parsed.Options.GetValueOrDefault("RightToLeft", "no");
        if (!string.Equals(rtl, "yes", StringComparison.OrdinalIgnoreCase) && !string.Equals(rtl, "no", StringComparison.OrdinalIgnoreCase))
            throw Error("LanguageSourceInvalid");
        return new(Path.GetFullPath(path), name, languageId, rtl.Equals("yes", StringComparison.OrdinalIgnoreCase),
            new ReadOnlyDictionary<string, string>(parsed.Options), new ReadOnlyDictionary<string, string>(parsed.Messages),
            new ReadOnlyDictionary<string, string>(parsed.CustomMessages));
    }

    public static async Task<LanguageSourceCatalog> DiscoverAsync(string compilerDirectory)
    {
        var sources = new List<LanguageSource>();
        var problems = new List<LanguageSourceProblem>();
        await AddAsync(Path.Combine(compilerDirectory, "Default.isl"));
        var languages = Path.Combine(compilerDirectory, "Languages");
        try
        {
            foreach (var path in Directory.EnumerateFiles(languages, "*.isl").Order(StringComparer.OrdinalIgnoreCase)) await AddAsync(path);
        }
        catch (Exception error) when (IsReadError(error)) { problems.Add(new(languages, ErrorKey(error))); }
        return new(sources.AsReadOnly(), problems.AsReadOnly());

        async Task AddAsync(string path)
        {
            try { sources.Add(await LoadAsync(path)); }
            catch (Exception error) when (IsReadError(error)) { problems.Add(new(path, ErrorKey(error))); }
        }
    }

    public static bool IsReadError(Exception error) => error is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or NotSupportedException;
    public static string ErrorKey(Exception error) => ProjectFile.ErrorResourceKey(error) ??
        (error is FileNotFoundException or DirectoryNotFoundException ? "LanguageSourceMissing" : "LanguageSourceUnreadable");

    private static bool TryLanguageId(string value, out int id) =>
        int.TryParse(value.TrimStart('$'), value.StartsWith('$') ? NumberStyles.AllowHexSpecifier : NumberStyles.None,
            CultureInfo.InvariantCulture, out id) && id is >= 0 and <= 65535;

    private static (Dictionary<string, string> Options, Dictionary<string, string> Messages, Dictionary<string, string> CustomMessages) Parse(string content)
    {
        var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["LangOptions"] = new(StringComparer.OrdinalIgnoreCase), ["Messages"] = new(StringComparer.OrdinalIgnoreCase),
            ["CustomMessages"] = new(StringComparer.OrdinalIgnoreCase)
        };
        Dictionary<string, string>? current = null;
        foreach (var raw in content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';')) continue;
            if (line.StartsWith('#') || line.Contains("{#", StringComparison.Ordinal)) throw Error("LanguageSourceScriptUnsupported");
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                if (!sections.TryGetValue(line[1..^1], out current)) throw Error("LanguageSourceInvalid");
                continue;
            }
            var separator = line.IndexOf('=');
            if (current is null || separator <= 0) throw Error("LanguageSourceInvalid");
            var key = line[..separator].Trim();
            if (key.Length == 0 || key.Any(char.IsWhiteSpace)) throw Error("LanguageSourceInvalid");
            current[key] = line[(separator + 1)..].Trim();
        }
        return (sections["LangOptions"], sections["Messages"], sections["CustomMessages"]);
    }

    private static InvalidDataException Error(string key)
    {
        var error = new InvalidDataException(new StudioLocalizer()[key]);
        error.Data["Studio.ResourceKey"] = key;
        return error;
    }
}
