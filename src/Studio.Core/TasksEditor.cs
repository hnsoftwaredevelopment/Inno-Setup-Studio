using System.Collections.ObjectModel;
using System.ComponentModel;
namespace Studio.Core;
public sealed class TaskChoice(TaskRule type) : INotifyPropertyChanged
{
    public string Name=>type.Name+" — "+type.Description;
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()=>PropertyChanged?.Invoke(this,new(nameof(Name)));
}
public sealed record TaskSelectionChoice(TaskInitialSelection Value,string Name);
public sealed class TasksEditor : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    private int _index;
    private string _languageId;
    public TasksEditor(EditorSession editor)
    {
        _editor=editor; _index=editor.Project.Tasks.Count>0 ? 0 : -1;
        InitialChoices=Enum.GetValues<TaskInitialSelection>().Select(v=>new TaskSelectionChoice(v,editor.Text["TaskInitial"+v])).ToArray();
        Languages=editor.Project.Languages.Where(l=>l.Enabled).Select(l=>new InstallTypeLanguageChoice(l.Id,l.DisplayName(editor.Text))).ToArray();
        _languageId=Languages.FirstOrDefault(l=>l.Id==editor.PreviewLanguageId)?.Id ?? Languages[0].Id; Rebuild();
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<TaskChoice> Tasks { get; }=[];
    public IReadOnlyList<InstallTypeLanguageChoice> Languages { get; }
    private TaskRule? Selected=>HasSelection ? _editor.Project.Tasks[_index] : null;
    public bool HasSelection=>_index>=0 && _index<_editor.Project.Tasks.Count;
    public bool CanAdd=>_editor.IsDesign && Tasks.Count<1000;
    public bool CanEdit=>_editor.IsDesign && HasSelection;
    public bool CanRemove=>CanEdit;
    public IReadOnlyList<TaskSelectionChoice> InitialChoices { get; }
    public bool CanMoveUp=>CanEdit && _index>0;
    public bool CanMoveDown=>CanEdit && _index<Tasks.Count-1;
    public int SelectedIndex
    {
        get=>_index;
        set { if(!_editor.IsDesign || value<0 || value>=Tasks.Count || value==_index) return; _index=value; Refresh(); }
    }
    public string Name { get=>Selected?.Name ?? ""; set { if(!CanEdit || Name==value) return; Selected!.Name=value; Changed(); } }
    public string Description { get=>Selected?.Description ?? ""; set { if(!CanEdit || Description==value) return; Selected!.Description=value; Changed(); } }
    public TaskInitialSelection InitialSelection { get=>Selected?.InitialSelection ?? TaskInitialSelection.Checked; set { if(!CanEdit || InitialSelection==value) return; Selected!.InitialSelection=value; Changed(); } }
    public bool Restart { get=>Selected?.Restart ?? false; set { if(!CanEdit || Restart==value) return; Selected!.Restart=value; Changed(); } }
    public string LanguageId { get=>_languageId; set { if(value is null || !Languages.Any(l=>l.Id==value) || value==_languageId) return; _languageId=value; Refresh(); } }
    public string Translation
    {
        get=>Selected?.Translations.GetValueOrDefault(_languageId) ?? "";
        set { if(!CanEdit || Translation==value) return; if(string.IsNullOrWhiteSpace(value)) Selected!.Translations.Remove(_languageId); else Selected!.Translations[_languageId]=value; Changed(); }
    }
    public string EffectiveDescription=>Selected?.DescriptionFor(_languageId) ?? "";
    public string Error
    {
        get { try { TaskRule.ValidateProject(_editor.Project); return ""; } catch(InvalidDataException error) { return _editor.Text[ProjectFile.ErrorResourceKey(error)!]; } }
    }
    public void Add()
    {
        if(!CanAdd) return;
        var number=1; while(_editor.Project.Tasks.Any(t=>t.Name.Equals("task"+number,StringComparison.OrdinalIgnoreCase))) number++;
        _editor.Project.Tasks.Add(new() { Name="task"+number,Description=_editor.Text["TaskNew"] });
        _index=_editor.Project.Tasks.Count-1; Rebuild(); Changed();
    }
    public void Remove()
    {
        if(!CanRemove) return;
        _editor.Project.Tasks.RemoveAt(_index); _index=Math.Min(_index,_editor.Project.Tasks.Count-1); Rebuild(); Changed();
    }
    public void Move(int direction)
    {
        if(!CanEdit || direction is not (-1 or 1) || _index+direction<0 || _index+direction>=Tasks.Count) return;
        var target=_index+direction; var list=_editor.Project.Tasks;
        (list[_index],list[target])=(list[target],list[_index]); _index=target; Rebuild(); Changed();
    }
    private void Rebuild() { Tasks.Clear(); foreach(var type in _editor.Project.Tasks) Tasks.Add(new(type)); }
    private void Changed()
    {
        if(HasSelection) Tasks[_index].Refresh(); _editor.FileRulesChanged(); Refresh();
    }
    private void Refresh()=>PropertyChanged?.Invoke(this,new(string.Empty));
}
