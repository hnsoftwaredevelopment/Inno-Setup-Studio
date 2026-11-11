using System.Text.RegularExpressions;
namespace Studio.Core;
public sealed class ComponentRule
{
    public string Name { get; set; }="";
    public string Description { get; set; }="";
    public Dictionary<string,string> Translations { get; set; }=[];
    public List<string> Types { get; set; }=[];
    public bool Fixed { get; set; }
    public bool CheckableAlone { get; set; }
    public int Level=>Name.Count(c=>c=='\\');
    public string DescriptionFor(string id)=>Translations.TryGetValue(id,out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : Description.Trim();
    internal static IReadOnlyList<InstallType> AvailableTypes(StudioProject project)=>project.InstallTypes.Count>0 ? project.InstallTypes :
        [new(){Name="full",Description="Full installation"},new(){Name="compact",Description="Compact installation"},new(){Name="custom",Description="Custom installation",IsCustom=true}];
    internal static void ValidateProject(StudioProject project)
    {
        if(project.Components is null || project.Components.Count>1000) throw InnoScript.Error("ComponentInvalid");
        var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase); var stack=new List<string>();
        var types=AvailableTypes(project).Select(t=>t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach(var c in project.Components)
        {
            if(c is null || c.Name is null || c.Name.Length>4096 || c.Name.Split('\\').Any(s=>s.Length>128 || !Regex.IsMatch(s,@"\A[A-Za-z_][A-Za-z0-9_]*\z") || new[]{"and","or","not"}.Contains(s,StringComparer.OrdinalIgnoreCase))) throw InnoScript.Error("ComponentNameInvalid");
            if(!names.Add(c.Name)) throw InnoScript.Error("ComponentDuplicate");
            var level=c.Level;
            if(level>255 || level>stack.Count || level>0 && !c.Name[..c.Name.LastIndexOf('\\')].Equals(stack[level-1],StringComparison.OrdinalIgnoreCase)) throw InnoScript.Error("ComponentHierarchyInvalid");
            if(stack.Count>level) stack.RemoveRange(level,stack.Count-level); stack.Add(c.Name);
            if(!Text(c.Description) || string.IsNullOrWhiteSpace(c.Description) || c.Translations is null || c.Translations.Any(t=>!project.Languages.Any(l=>l.Id==t.Key) || !Text(t.Value))) throw InnoScript.Error("ComponentDescriptionInvalid");
            if(c.Types is null || c.Types.Count>1000 || c.Types.Any(t=>t is null || !types.Contains(t)) || c.Types.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=c.Types.Count) throw InnoScript.Error("ComponentTypeMissing");
        }
    }
    private static bool Text(string? value)=>value is not null && value.Length<=4096 && !value.Any(char.IsControl) && !value.Contains("{#",StringComparison.Ordinal);
    internal static IEnumerable<string> Script(StudioProject project,IReadOnlyList<ExportLanguage> languages)
    {
        if(project.Components.Count==0) yield break;
        yield return "[CustomMessages]";
        for(var i=0;i<project.Components.Count;i++) foreach(var language in languages)
            yield return language.Name+".StudioComponent"+(i+1)+"="+project.Components[i].DescriptionFor(language.Language.Id).Replace("%","%%",StringComparison.Ordinal);
        yield return ""; yield return "[Components]";
        for(var i=0;i<project.Components.Count;i++)
        {
            var c=project.Components[i]; var flags=new List<string>(); if(c.Fixed) flags.Add("fixed"); if(c.CheckableAlone) flags.Add("checkablealone");
            yield return "Name: "+InnoScript.Parameter(c.Name)+"; Description: "+InnoScript.Parameter("{cm:StudioComponent"+(i+1)+"}")
                +(c.Types.Count>0 ? "; Types: "+string.Join(" ",c.Types) : "")+(flags.Count>0 ? "; Flags: "+string.Join(" ",flags) : "");
        }
        yield return "";
    }
}
