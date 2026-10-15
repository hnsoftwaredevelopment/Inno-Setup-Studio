using System.ComponentModel;
using System.Collections.ObjectModel;
namespace Studio.Core;
public sealed record DirectoryPolicyChoice<T>(T Id,string Name);
public sealed class DirectoryRuleChoice(DirectoryRule rule,int number) : INotifyPropertyChanged
{
    public string Name=>number+". "+rule.Name;
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Name)));
}
public sealed class DirectoryRulesEditor : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    private int _index;
    public DirectoryRulesEditor(EditorSession editor)
    {
        _editor=editor; _index=editor.Project.DirectoryRules.Count>0 ? 0 : -1; Rebuild();
        UninstallChoices=Enum.GetValues<DirectoryUninstall>().Select(v=>new DirectoryPolicyChoice<DirectoryUninstall>(v,editor.Text["DirectoryUninstall"+v])).ToArray();
        CompressionChoices=Enum.GetValues<DirectoryCompression>().Select(v=>new DirectoryPolicyChoice<DirectoryCompression>(v,editor.Text["DirectoryCompression"+v])).ToArray();
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<DirectoryRuleChoice> Rules { get; }=[];
    public IReadOnlyList<DirectoryPolicyChoice<DirectoryUninstall>> UninstallChoices { get; }
    public IReadOnlyList<DirectoryPolicyChoice<DirectoryCompression>> CompressionChoices { get; }
    public int SelectedIndex { get=>_index; set { if(value<0 || value>=_editor.Project.DirectoryRules.Count || value==_index) return; _index=value; Refresh(); } }
    public bool HasSelection=>_index>=0 && _index<_editor.Project.DirectoryRules.Count;
    public bool CanAdd=>_editor.IsDesign && Rules.Count<1000;
    public bool CanRemove=>_editor.IsDesign && HasSelection;
    private DirectoryRule? Selected=>HasSelection ? _editor.Project.DirectoryRules[_index] : null;
    public string Name { get=>Selected?.Name ?? ""; set { if(!_editor.IsDesign || Selected is null || Name==value) return; Selected.Name=value; Changed(); } }
    public DirectoryUninstall Uninstall { get=>Selected?.Uninstall ?? DirectoryUninstall.NativeDefault; set { if(!_editor.IsDesign || Selected is null || Uninstall==value) return; Selected.Uninstall=value; Changed(); } }
    public DirectoryCompression Compression { get=>Selected?.Compression ?? DirectoryCompression.Unchanged; set { if(!_editor.IsDesign || Selected is null || Compression==value) return; Selected.Compression=value; Changed(); } }
    public bool DeleteAfterInstall { get=>Selected?.DeleteAfterInstall ?? false; set { if(!_editor.IsDesign || Selected is null || DeleteAfterInstall==value) return; Selected.DeleteAfterInstall=value; Changed(); } }
    private bool Attribute(DirectoryRuleAttributes flag)=>Selected?.Attributes.HasFlag(flag) ?? false;
    private void Attribute(DirectoryRuleAttributes flag,bool value)
    {
        if(!_editor.IsDesign || Selected is null || Attribute(flag)==value) return;
        Selected.Attributes=value ? Selected.Attributes|flag : Selected.Attributes&~flag; Changed();
    }
    public bool ReadOnly { get=>Attribute(DirectoryRuleAttributes.ReadOnly); set=>Attribute(DirectoryRuleAttributes.ReadOnly,value); }
    public bool Hidden { get=>Attribute(DirectoryRuleAttributes.Hidden); set=>Attribute(DirectoryRuleAttributes.Hidden,value); }
    public bool System { get=>Attribute(DirectoryRuleAttributes.System); set=>Attribute(DirectoryRuleAttributes.System,value); }
    public bool NotContentIndexed { get=>Attribute(DirectoryRuleAttributes.NotContentIndexed); set=>Attribute(DirectoryRuleAttributes.NotContentIndexed,value); }
    public string Error
    {
        get
        {
            if(Selected is null) return "";
            try { DirectoryRule.Validate(Selected); return ""; }
            catch(InvalidDataException error) { return _editor.Text[ProjectFile.ErrorResourceKey(error)!]; }
        }
    }
    public string UninstallHint=>_editor.Text["DirectoryUninstallHint"+Uninstall];
    public void Add()
    {
        if(!CanAdd) return;
        _editor.Project.DirectoryRules.Add(new()); Rebuild(); _index=Rules.Count-1; Changed();
    }
    public void Remove()
    {
        if(!CanRemove) return;
        _editor.Project.DirectoryRules.RemoveAt(_index); _index=Math.Min(_index,_editor.Project.DirectoryRules.Count-1); Rebuild(); Changed();
    }
    private void Rebuild() { Rules.Clear(); var number=0; foreach(var rule in _editor.Project.DirectoryRules) Rules.Add(new(rule,++number)); }
    private void Changed() { if(HasSelection) Rules[_index].Refresh(); _editor.FileRulesChanged(); Refresh(); }
    private void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(string.Empty));
}
