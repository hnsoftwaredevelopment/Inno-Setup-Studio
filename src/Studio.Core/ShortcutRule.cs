namespace Studio.Core;
public enum ShortcutFolder { StartMenu,Desktop }
public sealed class ShortcutRule
{
    public string Name { get; set; }="{AppName}";
    public ShortcutFolder Folder { get; set; }
    public string Subfolder { get; set; }="";
    public string Target { get; set; }="";
    public string Parameters { get; set; }="";
    public string WorkingDirectory { get; set; }="";
    private static bool Segment(string value)=>!string.IsNullOrWhiteSpace(value) && value is not ("." or "..")
        && !value.EndsWith('.') && !value.EndsWith(' ') && value.IndexOfAny(['\\','/',':','*','?','"','<','>','|'])<0;
    public static string TargetPath(string value,bool allowRoot=false)
    {
        value=value.Replace('/','\\');
        if(value.Contains("{#",StringComparison.Ordinal) || !allowRoot && value=="{app}") throw InnoScript.Error("ShortcutInvalid");
        if(!(value=="{app}" || value.StartsWith(@"{app}\",StringComparison.Ordinal))) throw InnoScript.Error("ShortcutInvalid");
        _=FileCollections.Destination(value[..5]+value[5..].Replace('{','_').Replace('}','_'));
        return value;
    }
    public static void Validate(ShortcutRule? rule,StudioProject project)
    {
        if(rule is null || !Enum.IsDefined(rule.Folder) || new[]{rule.Name,rule.Subfolder,rule.Target,rule.Parameters,rule.WorkingDirectory}
            .Any(v=>v is null || v.Length>4096 || v.Any(char.IsControl) || v.Contains("{#",StringComparison.Ordinal))) throw InnoScript.Error("ShortcutInvalid");
        var name=WizardTexts.Expand(rule.Name,project);
        if(name.Length>4096 || !Segment(name) || rule.Subfolder.Length>0 && rule.Subfolder.Replace('/','\\').Split('\\').Any(p=>!Segment(p))) throw InnoScript.Error("ShortcutInvalid");
        try { _=TargetPath(rule.Target); if(rule.WorkingDirectory.Length>0) _=TargetPath(rule.WorkingDirectory,true); }
        catch(InvalidDataException) { throw InnoScript.Error("ShortcutInvalid"); }
    }
    internal string IconName(StudioProject project)
    {
        var literal=(Subfolder.Length==0 ? "" : Subfolder.Replace('/','\\')+"\\")+WizardTexts.Expand(Name,project);
        return (Folder==ShortcutFolder.StartMenu ? "{group}\\" : "{autodesktop}\\")+InnoScript.Literal(literal);
    }
    private static string NativePath(string path)=>"{app}"+InnoScript.Literal(TargetPath(path,true)[5..]);
    internal string Script(StudioProject project)
    {
        Validate(this,project);
        var line="Name: "+InnoScript.Parameter(IconName(project))+"; Filename: "+InnoScript.Parameter(NativePath(Target));
        if(Parameters.Length>0)
        {
            var parameters=System.Text.RegularExpressions.Regex.Replace(Parameters,@"\{AppName\}|\{AppVersion\}",m=>InnoScript.Literal(m.Value=="{AppName}" ? project.Name : project.AppVersion));
            line+="; Parameters: "+InnoScript.Parameter(parameters);
        }
        if(WorkingDirectory.Length>0) line+="; WorkingDir: "+InnoScript.Parameter(NativePath(WorkingDirectory));
        return line;
    }
    public static void CheckTarget(ShortcutRule rule,ISet<string> targets)
    {
        if(targets.Contains(TargetPath(rule.Target))) return;
        var error=InnoScript.Error("ShortcutTargetMissing"); error.Data["Studio.ShortcutTarget"]=rule.Target; error.Data["Studio.ShortcutName"]=rule.Name; throw error;
    }
    public static void CheckIndexSize(int count) { if(count>100_000) throw InnoScript.Error("ShortcutIndexTooLarge"); }
}
