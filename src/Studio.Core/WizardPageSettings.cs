using System.ComponentModel;
namespace Studio.Core;

public enum ProgramGroupPageMode { Auto, Show, Hide }
public enum PageSettingsSection { UserInfo, ProgramGroup }
public sealed class WizardPageSettings
{
    public bool UserInfoEnabled { get; set; }
    public string DefaultName { get; set; } = "{sysuserinfoname}";
    public string DefaultOrganization { get; set; } = "{sysuserinfoorg}";
    public bool UsePreviousUserInfo { get; set; } = true;
    public ProgramGroupPageMode GroupMode { get; set; } = ProgramGroupPageMode.Auto;
    public string DefaultGroup { get; set; } = "";
    public bool AllowNoIcons { get; set; }
    public bool UsePreviousGroup { get; set; } = true;
}
public sealed record GroupModeChoice(ProgramGroupPageMode Mode, string Name);

/// <summary>Settings are project data; trial input belongs only to this open form.</summary>
public sealed class WizardPageSettingsEditor(EditorSession editor, PageSettingsSection section) : INotifyPropertyChanged
{
    private WizardPageSettings Settings => editor.Project.PageSettings;
    private string? _trialName, _trialOrganization, _trialGroup;
    public StudioLocalizer Text => editor.Text;
    public bool IsUserInfo => section == PageSettingsSection.UserInfo;
    public bool IsProgramGroup => !IsUserInfo;
    public string Title => Text[IsUserInfo ? "PageOverviewUserInfo" : "PageOverviewProgramGroup"];
    public IReadOnlyList<GroupModeChoice> GroupModes { get; } = Enum.GetValues<ProgramGroupPageMode>()
        .Select(mode => new GroupModeChoice(mode, editor.Text["GroupMode" + mode])).ToArray();
    public bool UserInfoEnabled { get => Settings.UserInfoEnabled; set => Set(Settings.UserInfoEnabled, value, v => Settings.UserInfoEnabled = v); }
    public string DefaultName { get => Settings.DefaultName; set { if(Set(Settings.DefaultName,value,v=>Settings.DefaultName=v)) { _trialName=null; Refresh(); } } }
    public string DefaultOrganization { get => Settings.DefaultOrganization; set { if(Set(Settings.DefaultOrganization,value,v=>Settings.DefaultOrganization=v)) { _trialOrganization=null; Refresh(); } } }
    public bool UsePreviousUserInfo { get => Settings.UsePreviousUserInfo; set => Set(Settings.UsePreviousUserInfo,value,v=>Settings.UsePreviousUserInfo=v); }
    public ProgramGroupPageMode GroupMode { get => Settings.GroupMode; set { if(Enum.IsDefined(value)) Set(Settings.GroupMode,value,v=>Settings.GroupMode=v); } }
    public string DefaultGroup { get => Settings.DefaultGroup; set { if(Set(Settings.DefaultGroup,value,v=>Settings.DefaultGroup=v)) { _trialGroup=null; Refresh(); } } }
    public bool AllowNoIcons { get => Settings.AllowNoIcons; set => Set(Settings.AllowNoIcons,value,v=>Settings.AllowNoIcons=v); }
    public bool UsePreviousGroup { get => Settings.UsePreviousGroup; set => Set(Settings.UsePreviousGroup,value,v=>Settings.UsePreviousGroup=v); }
    public string TrialName { get => _trialName ?? WizardTexts.Expand(DefaultName,editor.Project); set { _trialName=value; Refresh(); } }
    public string TrialOrganization { get => _trialOrganization ?? WizardTexts.Expand(DefaultOrganization,editor.Project); set { _trialOrganization=value; Refresh(); } }
    public string TrialGroup { get => _trialGroup ?? WizardTexts.Expand(DefaultGroup,editor.Project); set { _trialGroup=value; Refresh(); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(T current,T value,Action<T> assign)
    {
        if(!editor.IsDesign || value is null || EqualityComparer<T>.Default.Equals(current,value)) return false;
        assign(value); editor.PageSettingsChanged(); Refresh(); return true;
    }
    private void Refresh() => PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(string.Empty));
}
