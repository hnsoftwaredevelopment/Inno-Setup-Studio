using System.Text.RegularExpressions;
namespace Studio.Core;

public enum TaskInitialSelection { Checked, Unchecked, CheckedOnce }

public sealed class TaskRule
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Dictionary<string,string> Translations { get; set; } = [];
    public TaskInitialSelection InitialSelection { get; set; }
    public bool Restart { get; set; }
    public string DescriptionFor(string id) => Translations.TryGetValue(id,out var text) && !string.IsNullOrWhiteSpace(text)
        ? text.Trim() : Description.Trim();

    internal static void ValidateProject(StudioProject project)
    {
        if(project.Tasks is null || project.Tasks.Count>1000) throw InnoScript.Error("TaskInvalid");
        var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var languages=project.Languages.Select(l=>l.Id).ToHashSet(StringComparer.Ordinal);
        foreach(var task in project.Tasks)
        {
            if(task is null || task.Name is null || task.Name.Length>128
                || !Regex.IsMatch(task.Name,@"\A[A-Za-z_][A-Za-z0-9_]*\z")
                || new[]{"and","or","not"}.Contains(task.Name,StringComparer.OrdinalIgnoreCase))
                throw InnoScript.Error("TaskNameInvalid");
            if(!names.Add(task.Name)) throw InnoScript.Error("TaskDuplicate");
            if(!Enum.IsDefined(task.InitialSelection)) throw InnoScript.Error("TaskInvalid");
            if(!ValidText(task.Description) || string.IsNullOrWhiteSpace(task.Description) || task.Translations is null
                || task.Translations.Any(t=>!languages.Contains(t.Key) || !ValidText(t.Value)))
                throw InnoScript.Error("TaskDescriptionInvalid");
        }
    }
    private static bool ValidText(string? value) => value is not null && value.Length<=4096
        && !value.Any(char.IsControl) && !value.Contains("{#",StringComparison.Ordinal);

    internal static IEnumerable<string> Script(StudioProject project,IReadOnlyList<ExportLanguage> languages)
    {
        if(project.Tasks.Count==0) yield break;
        yield return "[CustomMessages]";
        for(var i=0;i<project.Tasks.Count;i++)
            foreach(var language in languages)
                yield return language.Name+".StudioTask"+(i+1)+"="+project.Tasks[i].DescriptionFor(language.Language.Id).Replace("%","%%",StringComparison.Ordinal);
        yield return "";
        yield return "[Tasks]";
        for(var i=0;i<project.Tasks.Count;i++)
        {
            var task=project.Tasks[i];
            var flags=new List<string>();
            if(task.InitialSelection==TaskInitialSelection.Unchecked) flags.Add("unchecked");
            if(task.InitialSelection==TaskInitialSelection.CheckedOnce) flags.Add("checkedonce");
            if(task.Restart) flags.Add("restart");
            yield return "Name: "+InnoScript.Parameter(task.Name)+"; Description: "+InnoScript.Parameter("{cm:StudioTask"+(i+1)+"}")
                +(flags.Count>0 ? "; Flags: "+string.Join(" ",flags) : "");
        }
        yield return "";
    }
}
