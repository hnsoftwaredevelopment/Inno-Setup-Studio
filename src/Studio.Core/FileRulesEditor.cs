using System.ComponentModel;
using System.Collections.ObjectModel;
namespace Studio.Core;

public sealed class FileRuleChoice(InstallFileSettings rule,int number,StudioLocalizer text) : INotifyPropertyChanged
{
    public InstallFileSettings Rule { get; }=rule;
    public string Name => number+". "+(string.IsNullOrWhiteSpace(Rule.Source) ? text["FilesEmptyRule"] : Rule.Source);
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Name)));
}
public sealed class FileRulesEditor : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    private int _index;
    private string _excludeText="";
    public FileRulesEditor(EditorSession editor) { _editor=editor; Rebuild(); _excludeText=string.Join(Environment.NewLine,Selected.Excludes); }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<FileRuleChoice> Rules { get; }=[];
    public int SelectedIndex { get=>_index; set { if(value<0 || value>=Rules.Count || value==_index) return; _index=value; _excludeText=string.Join(Environment.NewLine,Selected.Excludes); Refresh(); } }
    private InstallFileSettings Selected => Rules[_index].Rule;
    public string Source { get=>Selected.Source; set { if(!_editor.IsDesign || value==Source) return; Selected.Source=value; Changed(); } }
    public string Destination { get=>Selected.Destination; set { if(!_editor.IsDesign || value==Destination) return; Selected.Destination=value; Changed(); } }
    public bool RecurseSubdirectories { get=>Selected.RecurseSubdirectories; set { if(!_editor.IsDesign || value==RecurseSubdirectories) return; Selected.RecurseSubdirectories=value; Changed(); } }
    public string Excludes { get=>_excludeText; set
    {
        if(!_editor.IsDesign || value==Excludes) return;
        _excludeText=value;
        Selected.Excludes=value.Split(["\r\n","\n"],StringSplitOptions.None).Where(s=>!string.IsNullOrWhiteSpace(s)).ToList(); Changed();
    } }
    public bool CanAdd=>_editor.IsDesign && Rules.Count<1000;
    public bool CanRemove=>_editor.IsDesign && Rules.Count>1;
    public void Add()
    {
        if(!CanAdd) return;
        _editor.Project.AdditionalFiles.Add(new()); Rebuild(); _index=Rules.Count-1; _excludeText=""; Changed();
    }
    public void Remove()
    {
        if(!CanRemove) return;
        if(_index==0) { _editor.Project.InstallFile=_editor.Project.AdditionalFiles[0]; _editor.Project.AdditionalFiles.RemoveAt(0); }
        else _editor.Project.AdditionalFiles.RemoveAt(_index-1);
        _index=Math.Min(_index,Rules.Count-2); Rebuild(); _excludeText=string.Join(Environment.NewLine,Selected.Excludes); Changed();
    }
    public InstallFileSettings Snapshot() => new() { Source=Source,Destination=Destination,RecurseSubdirectories=RecurseSubdirectories,Excludes=[..Selected.Excludes] };
    private void Rebuild() { Rules.Clear(); var number=0; foreach(var rule in FileCollections.Rules(_editor.Project)) Rules.Add(new(rule,++number,_editor.Text)); }
    private void Changed() { _editor.FileRulesChanged(); Rules[_index].Refresh(); Refresh(); }
    private void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(string.Empty));
}
