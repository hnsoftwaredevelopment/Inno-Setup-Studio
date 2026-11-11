using System.Collections.ObjectModel;
using System.ComponentModel;
namespace Studio.Core;
public sealed class ComponentChoice(ComponentRule rule) : INotifyPropertyChanged
{
    public string Name=>new string(' ',rule.Level*2)+rule.Name+" — "+rule.Description;
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()=>PropertyChanged?.Invoke(this,new(nameof(Name)));
}
public sealed class ComponentTypeChoice(string id,string name,Func<bool> get,Action<bool> set) : INotifyPropertyChanged
{
    public string Name=>name;
    public string Id=>id;
    public bool IsChecked { get=>get(); set { if(value==get()) return; set(value); PropertyChanged?.Invoke(this,new(nameof(IsChecked))); } }
    public event PropertyChangedEventHandler? PropertyChanged;
}
public sealed class ComponentsEditor : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    private int _index;
    private string _languageId;
    public ComponentsEditor(EditorSession editor)
    {
        _editor=editor; _index=editor.Project.Components.Count>0 ? 0 : -1;
        Languages=editor.Project.Languages.Where(l=>l.Enabled).Select(l=>new InstallTypeLanguageChoice(l.Id,l.DisplayName(editor.Text))).ToArray();
        _languageId=Languages.FirstOrDefault(l=>l.Id==editor.PreviewLanguageId)?.Id ?? Languages[0].Id; Rebuild();
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ComponentChoice> Components { get; }=[];
    public IReadOnlyList<InstallTypeLanguageChoice> Languages { get; }
    public IReadOnlyList<ComponentTypeChoice> TypeChoices=>ComponentRule.AvailableTypes(_editor.Project).Select(t=>new ComponentTypeChoice(t.Name,t.Description,()=>Selected?.Types.Contains(t.Name,StringComparer.OrdinalIgnoreCase) ?? false,value=>
    { if(!CanEdit) return; Selected!.Types.RemoveAll(n=>n.Equals(t.Name,StringComparison.OrdinalIgnoreCase)); if(value) Selected.Types.Add(t.Name); Changed(); })).ToArray();
    private ComponentRule? Selected=>HasSelection ? _editor.Project.Components[_index] : null;
    public bool HasSelection=>_index>=0 && _index<_editor.Project.Components.Count;
    public bool CanEdit=>_editor.IsDesign && HasSelection;
    public bool CanAdd=>_editor.IsDesign && Components.Count<1000;
    public bool CanAddChild=>CanAdd && HasSelection && Selected!.Level<255;
    public bool CanRemove=>CanEdit && End(_index)==_index+1;
    public bool NameReadOnly=>!CanRemove;
    public bool CanMoveUp=>CanEdit && Sibling(-1)>=0;
    public bool CanMoveDown=>CanEdit && Sibling(1)>=0;
    public string ReferenceHint=>CanEdit && !CanRemove ? _editor.Text["ComponentHasChildren"] : "";
    public int SelectedIndex { get=>_index; set { if(value<0 || value>=Components.Count || value==_index) return; _index=value; Refresh(); } }
    public string Name { get=>Selected?.Name ?? ""; set { if(!CanRemove || Name==value) return; Selected!.Name=value; Changed(); } }
    public string Description { get=>Selected?.Description ?? ""; set { if(!CanEdit || Description==value) return; Selected!.Description=value; Changed(); } }
    public bool Fixed { get=>Selected?.Fixed ?? false; set { if(!CanEdit || Fixed==value) return; Selected!.Fixed=value; Changed(); } }
    public bool CheckableAlone { get=>Selected?.CheckableAlone ?? false; set { if(!CanEdit || CheckableAlone==value) return; Selected!.CheckableAlone=value; Changed(); } }
    public string LanguageId { get=>_languageId; set { if(value is null || !Languages.Any(l=>l.Id==value) || value==_languageId) return; _languageId=value; Refresh(); } }
    public string Translation
    {
        get=>Selected?.Translations.GetValueOrDefault(_languageId) ?? "";
        set { if(!CanEdit || Translation==value) return; if(string.IsNullOrWhiteSpace(value)) Selected!.Translations.Remove(_languageId); else Selected!.Translations[_languageId]=value; Changed(); }
    }
    public string EffectiveDescription=>Selected?.DescriptionFor(_languageId) ?? "";
    public string Error { get { try { ComponentRule.ValidateProject(_editor.Project); return ""; } catch(InvalidDataException e) { return _editor.Text[ProjectFile.ErrorResourceKey(e)!]; } } }
    public void Add(bool child=false)
    {
        if(!CanAdd || child && !CanAddChild) return;
        var prefix=child ? Name+"\\" : ""; var number=1;
        while(_editor.Project.Components.Any(c=>c.Name.Equals(prefix+"component"+number,StringComparison.OrdinalIgnoreCase))) number++;
        var position=child ? End(_index) : Components.Count;
        _editor.Project.Components.Insert(position,new(){Name=prefix+"component"+number,Description=_editor.Text["ComponentNew"],Types=[ComponentRule.AvailableTypes(_editor.Project)[0].Name]});
        _index=position; Rebuild(); Changed();
    }
    public void Remove() { if(!CanRemove) return; _editor.Project.Components.RemoveAt(_index); _index=Math.Min(_index,Components.Count-2); Rebuild(); Changed(); }
    private int End(int index) { var list=_editor.Project.Components;var next=index+1;while(next<list.Count && list[next].Level>list[index].Level) next++;return next; }
    private int Sibling(int direction)
    {
        if(!HasSelection) return -1;
        var list=_editor.Project.Components; var level=Selected!.Level;
        var other=direction>0 ? End(_index) : _index-1;
        if(direction<0) while(other>=0 && list[other].Level>level) other--;
        return other>=0 && other<list.Count && list[other].Level==level ? other : -1;
    }
    public void Move(int direction)
    {
        if(!CanEdit || direction is not (-1 or 1)) return; var other=Sibling(direction); if(other<0) return;
        var first=Math.Min(_index,other);var second=Math.Max(_index,other);var end=End(second);var list=_editor.Project.Components;
        var a=list.GetRange(first,second-first);var b=list.GetRange(second,end-second);
        list.RemoveRange(first,end-first);list.InsertRange(first,b.Concat(a));_index=direction<0 ? first : first+b.Count; Rebuild(); Changed();
    }
    public void RefreshSettings()=>Refresh();
    private void Rebuild() { Components.Clear(); foreach(var c in _editor.Project.Components) Components.Add(new(c)); }
    private void Changed() { if(HasSelection) Components[_index].Refresh(); _editor.FileRulesChanged(); Refresh(); }
    private void Refresh()=>PropertyChanged?.Invoke(this,new(string.Empty));
}
