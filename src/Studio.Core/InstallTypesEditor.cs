using System.Collections.ObjectModel;
using System.ComponentModel;
namespace Studio.Core;
public sealed class InstallTypeChoice(InstallType type) : INotifyPropertyChanged
{
    public string Name=>type.Name+" — "+type.Description;
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()=>PropertyChanged?.Invoke(this,new(nameof(Name)));
}
public sealed record InstallTypeLanguageChoice(string Id,string Name);
public sealed class InstallTypesEditor : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    private int _index;
    private string _languageId;
    public InstallTypesEditor(EditorSession editor)
    {
        _editor=editor; _index=editor.Project.InstallTypes.FindIndex(t=>t.Name==editor.Project.SelectedInstallTypeId);
        if(_index<0 && editor.Project.InstallTypes.Count>0) _index=0;
        Languages=editor.Project.Languages.Where(l=>l.Enabled).Select(l=>new InstallTypeLanguageChoice(l.Id,l.DisplayName(editor.Text))).ToArray();
        _languageId=Languages.FirstOrDefault(l=>l.Id==editor.PreviewLanguageId)?.Id ?? Languages[0].Id; Rebuild();
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<InstallTypeChoice> Types { get; }=[];
    public IReadOnlyList<InstallTypeLanguageChoice> Languages { get; }
    private InstallType? Selected=>HasSelection ? _editor.Project.InstallTypes[_index] : null;
    public bool HasSelection=>_index>=0 && _index<_editor.Project.InstallTypes.Count;
    public bool CanAdd=>_editor.IsDesign && Types.Count<1000;
    public bool CanRemove=>_editor.IsDesign && HasSelection;
    public bool CanMoveUp=>CanRemove && _index>0;
    public bool CanMoveDown=>CanRemove && _index<Types.Count-1;
    public int SelectedIndex
    {
        get=>_index;
        set { if(!_editor.IsDesign || value<0 || value>=Types.Count || value==_index) return; _index=value; Changed(); }
    }
    public string Name { get=>Selected?.Name ?? ""; set { if(!CanRemove || Name==value) return; Selected!.Name=value; Changed(); } }
    public string Description { get=>Selected?.Description ?? ""; set { if(!CanRemove || Description==value) return; Selected!.Description=value; Changed(); } }
    public bool IsCustom { get=>Selected?.IsCustom ?? false; set { if(!CanRemove || IsCustom==value) return; Selected!.IsCustom=value; Changed(); } }
    public string LanguageId { get=>_languageId; set { if(value is null || !Languages.Any(l=>l.Id==value) || value==_languageId) return; _languageId=value; Refresh(); } }
    public string Translation
    {
        get=>Selected?.Translations.GetValueOrDefault(_languageId) ?? "";
        set { if(!CanRemove || Translation==value) return; if(string.IsNullOrWhiteSpace(value)) Selected!.Translations.Remove(_languageId); else Selected!.Translations[_languageId]=value; Changed(); }
    }
    public string EffectiveDescription=>Selected?.DescriptionFor(_languageId) ?? "";
    public string Error
    {
        get { try { InstallType.ValidateProject(_editor.Project); return ""; } catch(InvalidDataException error) { return _editor.Text[ProjectFile.ErrorResourceKey(error)!]; } }
    }
    public void Add()
    {
        if(!CanAdd) return;
        var number=1; while(_editor.Project.InstallTypes.Any(t=>t.Name.Equals("type"+number,StringComparison.OrdinalIgnoreCase))) number++;
        _editor.Project.InstallTypes.Add(new() { Name="type"+number,Description=_editor.Text["InstallTypeNew"] });
        _index=_editor.Project.InstallTypes.Count-1; Rebuild(); Changed();
    }
    public void Remove()
    {
        if(!CanRemove) return;
        _editor.Project.InstallTypes.RemoveAt(_index); _index=Math.Min(_index,_editor.Project.InstallTypes.Count-1); Rebuild(); Changed();
    }
    public void Move(int direction)
    {
        if(!CanRemove || direction is not (-1 or 1) || _index+direction<0 || _index+direction>=Types.Count) return;
        var target=_index+direction; var list=_editor.Project.InstallTypes;
        (list[_index],list[target])=(list[target],list[_index]); _index=target; Rebuild(); Changed();
    }
    private void Rebuild() { Types.Clear(); foreach(var type in _editor.Project.InstallTypes) Types.Add(new(type)); }
    private void Changed()
    {
        _editor.Project.SelectedInstallTypeId=Selected?.Name ?? "";
        if(HasSelection) Types[_index].Refresh(); _editor.FileRulesChanged(); Refresh();
    }
    private void Refresh()=>PropertyChanged?.Invoke(this,new(string.Empty));
}
