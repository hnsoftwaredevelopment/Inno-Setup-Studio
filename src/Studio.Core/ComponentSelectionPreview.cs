using System.ComponentModel;
namespace Studio.Core;
public sealed record ComponentPreviewType(string Id,string Name);
public sealed class ComponentPreviewItem(ComponentSelectionPreview owner,ComponentRule rule,int index) : INotifyPropertyChanged
{
    internal bool Checked;
    public string Name=>new string(' ',rule.Level*3)+rule.DescriptionFor(owner.LanguageId);
    public bool IsChecked { get=>Checked; set=>owner.Toggle(index,value); }
    public bool? CheckState { get=>Checked && owner.IsPartial(index) ? null : Checked; set=>owner.Toggle(index,value ?? true); }
    public bool CanToggle=>!rule.Fixed && owner.HasCustomType;
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()=>PropertyChanged?.Invoke(this,new(string.Empty));
}
public sealed class ComponentSelectionPreview : INotifyPropertyChanged
{
    private readonly StudioProject _project;
    private readonly Func<string> _language;
    private readonly Func<string,string> _message;
    private readonly IReadOnlyList<InstallType> _types;
    private string _typeId;
    private string _initialTypeId;
    private bool[] _initial=[];
    public ComponentSelectionPreview(StudioProject project,Func<string> language,Func<string,string> message)
    {
        _project=project; _language=language;_message=message;_types=ComponentRule.AvailableTypes(project);
        _typeId=_initialTypeId=_types[0].Name;
        Items=project.Components.Select((c,i)=>new ComponentPreviewItem(this,c,i)).ToArray(); ApplyType();
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public string LanguageId=>_language();
    public IReadOnlyList<ComponentPreviewItem> Items { get; }
    public IReadOnlyList<ComponentPreviewType> Types=>_types.Select(t=>new ComponentPreviewType(t.Name,_project.InstallTypes.Count>0 ? t.DescriptionFor(LanguageId) : _message(t.Name switch { "full"=>"FullInstallation","compact"=>"CompactInstallation",_=>"CustomInstallation" }))).ToArray();
    public bool ShowTypePicker=>_types.Count>1 || !HasCustomType;
    public bool HasCustomType=>_types.Any(t=>t.IsCustom);
    public string TypeId
    {
        get=>_typeId;
        set { var type=_types.FirstOrDefault(t=>t.Name==value);if(type is null || value==_typeId) return;_typeId=value;ApplyType();Refresh(); }
    }
    private void ApplyType()
    {
        var custom=_types.Single(t=>t.Name==_typeId).IsCustom;
        for(var i=0;i<Items.Count;i++) if(!custom || _project.Components[i].Fixed)
            Items[i].Checked=_project.Components[i].Types.Contains(_typeId,StringComparer.OrdinalIgnoreCase);
        Normalize(); _initialTypeId=_typeId;_initial=Items.Select(i=>i.Checked).ToArray();
    }
    private int End(int index) {var next=index+1;while(next<Items.Count && _project.Components[next].Level>_project.Components[index].Level) next++;return next;}
    internal bool IsPartial(int index)=>Enumerable.Range(index+1,End(index)-index-1).Any(i=>!Items[i].Checked);
    public void Toggle(int index,bool value)
    {
        if(index<0 || index>=Items.Count || !Items[index].CanToggle || Items[index].Checked==value && !IsPartial(index)) return;
        Items[index].Checked=value;
        for(var i=index+1;i<End(index);i++) if(!_project.Components[i].Fixed) Items[i].Checked=value;
        if(value)
            for(var i=index-1;i>=0;i--) if(_project.Components[index].Name.StartsWith(_project.Components[i].Name+"\\",StringComparison.OrdinalIgnoreCase)) Items[i].Checked=true;
        Normalize();
        _typeId=Items.Select(i=>i.Checked).SequenceEqual(_initial) ? _initialTypeId : _types.First(t=>t.IsCustom).Name; Refresh();
    }
    private void Normalize()
    {
        for(var i=Items.Count-1;i>=0;i--)
        {
            var end=End(i);if(end==i+1) continue;
            var any=Enumerable.Range(i+1,end-i-1).Any(j=>Items[j].Checked);
            if(any) Items[i].Checked=true; else if(!_project.Components[i].CheckableAlone) Items[i].Checked=false;
        }
    }
    public string Summary=>string.Join("\n",Items.Where(i=>i.Checked).Select(i=>i.Name.Trim()));
    public void Refresh() { foreach(var item in Items) item.Refresh();PropertyChanged?.Invoke(this,new(string.Empty)); }
}
