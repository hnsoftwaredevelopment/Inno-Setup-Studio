using System.ComponentModel;
using System.Collections.ObjectModel;
namespace Studio.Core;
public sealed record ShortcutFolderChoice(ShortcutFolder Id,string Name);
public sealed class ShortcutChoice(ShortcutRule rule,int number) : INotifyPropertyChanged
{
    public string Name=>number+". "+rule.Name;
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Name)));
}
public sealed class ShortcutRulesEditor : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    private int _index;
    private HashSet<string> _targets=new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;
    private string _referenceError="";
    public ShortcutRulesEditor(EditorSession editor)
    {
        _editor=editor; _index=editor.Project.Shortcuts.Count>0 ? 0 : -1; Rebuild();
        Folders=Enum.GetValues<ShortcutFolder>().Select(v=>new ShortcutFolderChoice(v,editor.Text["ShortcutFolder"+v])).ToArray();
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ShortcutChoice> Rules { get; }=[];
    public IReadOnlyList<ShortcutFolderChoice> Folders { get; }
    public IReadOnlyList<string> Targets { get; private set; }=[];
    public bool HasSelection=>_index>=0 && _index<_editor.Project.Shortcuts.Count;
    private ShortcutRule? Selected=>HasSelection ? _editor.Project.Shortcuts[_index] : null;
    public int SelectedIndex { get=>_index; set { if(value<0 || value>=_editor.Project.Shortcuts.Count || value==_index) return; _index=value; Refresh(); } }
    public bool CanAdd=>_editor.IsDesign && Rules.Count<1000;
    public bool IsStartMenu=>HasSelection && Folder==ShortcutFolder.StartMenu;
    public bool IsDesktop=>HasSelection && Folder==ShortcutFolder.Desktop;
    public bool CanRemove=>_editor.IsDesign && HasSelection;
    public string Name { get=>Selected?.Name ?? ""; set { if(!_editor.IsDesign || Selected is null || Name==value) return; Selected.Name=value; Changed(); } }
    public ShortcutFolder Folder { get=>Selected?.Folder ?? ShortcutFolder.StartMenu; set { if(!_editor.IsDesign || Selected is null || Folder==value) return; Selected.Folder=value; Changed(); } }
    public string Subfolder { get=>Selected?.Subfolder ?? ""; set { if(!_editor.IsDesign || Selected is null || Subfolder==value) return; Selected.Subfolder=value; Changed(); } }
    public string Target { get=>Selected?.Target ?? ""; set { if(!_editor.IsDesign || Selected is null || Target==value) return; Selected.Target=value; Changed(); } }
    // Clearing a ComboBox during an index refresh must never clear the stored target.
    public string? SelectedTarget
    {
        get=>Targets.FirstOrDefault(t=>t.Equals(Target.Replace('/','\\'),StringComparison.OrdinalIgnoreCase));
        set { if(value is not null && _targets.Contains(value)) Target=value; }
    }
    public string Parameters { get=>Selected?.Parameters ?? ""; set { if(!_editor.IsDesign || Selected is null || Parameters==value) return; Selected.Parameters=value; Changed(); } }
    public string WorkingDirectory { get=>Selected?.WorkingDirectory ?? ""; set { if(!_editor.IsDesign || Selected is null || WorkingDirectory==value) return; Selected.WorkingDirectory=value; Changed(); } }
    public string ReferenceError=>_referenceError;
    public string ReferenceStatus=>!_loaded ? _editor.Text["ShortcutLoading"] : _editor.Text.Format("ShortcutReferenceCount",Targets.Count,
        _editor.Project.Shortcuts.Count(s=>!_targets.Contains(s.Target.Replace('/','\\'))));
    public string GroupHint=>_editor.Text.Format("ShortcutGroupDefault",_editor.Project.PageSettings.DefaultGroup.Length==0 ? "(Default)" : WizardTexts.Expand(_editor.Project.PageSettings.DefaultGroup,_editor.Project));
    public string Error
    {
        get
        {
            if(Selected is null) return "";
            if(string.IsNullOrWhiteSpace(Target)) return _editor.Text["ShortcutChooseTarget"];
            try
            {
                ShortcutRule.Validate(Selected,_editor.Project);
                if(_editor.Project.Shortcuts.Count(s=>s.IconName(_editor.Project).Equals(Selected.IconName(_editor.Project),StringComparison.OrdinalIgnoreCase))>1) return _editor.Text["ShortcutDuplicate"];
                if(_loaded && _referenceError.Length==0) ShortcutRule.CheckTarget(Selected,_targets);
                return "";
            }
            catch(InvalidDataException error) { return _editor.Text[ProjectFile.ErrorResourceKey(error)!]; }
        }
    }
    public void SetTargets(IEnumerable<string> targets,string? errorKey=null)
    {
        _targets=new(targets,StringComparer.OrdinalIgnoreCase); Targets=_targets.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        _referenceError=errorKey is null ? "" : _editor.Text[errorKey]; _loaded=true; Refresh();
    }
    public void RefreshSettings()=>Refresh();
    public void Add() { if(!CanAdd) return; _editor.Project.Shortcuts.Add(new()); Rebuild(); _index=Rules.Count-1; Changed(); }
    public void Remove() { if(!CanRemove) return; _editor.Project.Shortcuts.RemoveAt(_index); _index=Math.Min(_index,_editor.Project.Shortcuts.Count-1); Rebuild(); Changed(); }
    private void Rebuild() { Rules.Clear(); var number=0; foreach(var rule in _editor.Project.Shortcuts) Rules.Add(new(rule,++number)); }
    private void Changed() { if(HasSelection) Rules[_index].Refresh(); _editor.FileRulesChanged(); Refresh(); }
    private void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(string.Empty));
}
