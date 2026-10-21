using System.Text.RegularExpressions;
namespace Studio.Core;
public sealed class InstallType
{
    public string Name { get; set; }="";
    public string Description { get; set; }="";
    public Dictionary<string,string> Translations { get; set; }=[];
    public bool IsCustom { get; set; }
    public string DescriptionFor(string languageId)=>Translations.TryGetValue(languageId,out var text) && !string.IsNullOrWhiteSpace(text) ? text.Trim() : Description.Trim();
    internal static void ValidateProject(StudioProject project)
    {
        if(project.InstallTypes is null || project.InstallTypes.Count>1000) throw InnoScript.Error("InstallTypeInvalid");
        foreach(var type in project.InstallTypes)
        {
            if(type is null || type.Name is null || type.Name.Length>128 || (!Regex.IsMatch(type.Name,@"\A[A-Za-z_][A-Za-z0-9_]*\z") || new[]{"and","or","not"}.Contains(type.Name,StringComparer.OrdinalIgnoreCase))) throw InnoScript.Error("InstallTypeNameInvalid");
            if(!Text(type.Description) || string.IsNullOrWhiteSpace(type.Description) || type.Translations is null
                || type.Translations.Any(t=>!project.Languages.Any(l=>l.Id==t.Key) || !Text(t.Value))) throw InnoScript.Error("InstallTypeDescriptionInvalid");
        }
        if(project.InstallTypes.Select(t=>t.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=project.InstallTypes.Count) throw InnoScript.Error("InstallTypeDuplicate");
        if(project.InstallTypes.Count(t=>t.IsCustom)>1) throw InnoScript.Error("InstallTypeCustomDuplicate");
        if(project.SelectedInstallTypeId is null || project.SelectedInstallTypeId.Length>0 && !project.InstallTypes.Any(t=>t.Name==project.SelectedInstallTypeId)) throw InnoScript.Error("InstallTypeInvalid");
    }
    private static bool Text(string? value)=>value is not null && value.Length<=4096 && !value.Any(char.IsControl) && !value.Contains("{#",StringComparison.Ordinal);
    internal static IEnumerable<string> Script(StudioProject project,IReadOnlyList<ExportLanguage> languages)
    {
        if(project.InstallTypes.Count==0) yield break;
        yield return "[CustomMessages]";
        for(var i=0;i<project.InstallTypes.Count;i++)
            foreach(var language in languages)
                yield return language.Name+".StudioType"+(i+1)+"="+project.InstallTypes[i].DescriptionFor(language.Language.Id).Replace("%","%%",StringComparison.Ordinal);
        yield return ""; yield return "[Types]";
        for(var i=0;i<project.InstallTypes.Count;i++)
        {
            var type=project.InstallTypes[i];
            yield return "Name: "+InnoScript.Parameter(type.Name)+"; Description: "+InnoScript.Parameter("{cm:StudioType"+(i+1)+"}")+(type.IsCustom ? "; Flags: iscustom" : "");
        }
        yield return "";
    }
}
