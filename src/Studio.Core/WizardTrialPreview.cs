using System.ComponentModel;
namespace Studio.Core;

/// <summary>One native-order route with temporary input; never executes installation actions.</summary>
public sealed class WizardTrialPreview : INotifyPropertyChanged, IDisposable
{
    private readonly EditorSession _editor;
    private readonly WizardCompletionPreview _completion;
    private int _index;
    private bool _accepted, _documentAvailable, _disposed;
    private string _directory, _name, _organization;
    private static readonly Dictionary<string,string> English = new()
    {
        ["WizardLicense"]="License Agreement", ["LicenseLabel3"]="Please read the following License Agreement. You must accept the terms of this agreement before continuing with the installation.",
        ["LicenseAccepted"]="I accept the agreement", ["LicenseNotAccepted"]="I do not accept the agreement",
        ["WizardInfoBefore"]="Information", ["InfoBeforeLabel"]="Please read the following important information before continuing.",
        ["WizardInfoAfter"]="Information", ["InfoAfterLabel"]="Please read the following important information before continuing.",
        ["WizardUserInfo"]="User Information", ["UserInfoDesc"]="Please enter your information.",
        ["UserInfoName"]="User Name:", ["UserInfoOrg"]="Organization:"
    };
    public WizardTrialPreview(EditorSession editor)
    {
        if(editor.IsPreview) throw new InvalidOperationException("A trial is already active.");
        _editor=editor;
        Route=WizardPageOverview.Create(editor.Text,editor.Project).Where(p=>p.State is WizardPageState.Enabled or WizardPageState.Runtime).ToArray();
        _directory=WizardTexts.Expand(editor.Project.Pages[1].Destination!.DefaultDirectory,editor.Project);
        _name=WizardTexts.Expand(editor.Project.PageSettings.DefaultName,editor.Project);
        _organization=WizardTexts.Expand(editor.Project.PageSettings.DefaultOrganization,editor.Project);
        _completion=new WizardCompletionPreview(editor,CompletionPhase.Ready);
        _completion.PropertyChanged+=CompletionChanged;
        editor.IsPreview=true;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public IReadOnlyList<WizardPageSummary> Route { get; }
    public IReadOnlyList<InstallerLanguageChoice> Languages => _completion.Languages;
    public string LanguageId => _completion.LanguageId;
    public string PageId => _completion.IsPreparing ? "Preparing" : Route[_index].Id;
    public string PageName => _editor.Text["PageOverview"+PageId];
    public string RouteHint => string.Join(" → ",Route.Select(p=>_editor.Text["PageOverview"+p.Id]));
    public bool ExitRequested { get; private set; }
    public bool CanInteract => !_disposed && _completion.CanInteract;
    public bool IsDocument => DocumentKind is not null;
    public bool IsLicense => PageId=="License";
    public bool IsUserInfo => PageId=="UserInfo";
    public bool IsDestination => PageId=="Destination";
    public bool IsReady => PageId=="Ready";
    public bool IsPreparing => PageId=="Preparing";
    public bool IsInstalling => PageId=="Installing";
    public bool IsFinished => PageId=="Finished";
    public WizardDocumentKind? DocumentKind => PageId switch
    {
        "License"=>WizardDocumentKind.LicenseFile,"InfoBefore"=>WizardDocumentKind.InfoBeforeFile,
        "InfoAfter"=>WizardDocumentKind.InfoAfterFile,_=>null
    };
    public string DocumentSource => DocumentKind is { } kind ? WizardDocuments.Source(_editor.Project,kind,LanguageId) : "";
    public bool DocumentAvailable { get=>_documentAvailable; set { if(_documentAvailable==value) return; _documentAvailable=value; Refresh(); } }
    public bool LicenseAccepted { get=>_accepted; set { if(_accepted==value) return; _accepted=value; Refresh(); } }
    public bool LicenseDeclined { get=>!_accepted; set { if(value) LicenseAccepted=false; } }
    public string Directory { get=>_directory; set { if(_directory==value) return; _directory=value; Refresh(); } }
    public string UserName { get=>_name; set { if(_name==value) return; _name=value; Refresh(); } }
    public string Organization { get=>_organization; set { if(_organization==value) return; _organization=value; Refresh(); } }
    public int Progress { get=>_completion.Progress; set=>_completion.Progress=value; }
    public bool PreparationProblem { get=>_completion.PreparationProblem; set=>_completion.PreparationProblem=value; }
    public string WindowTitle => _completion.WindowTitle;
    public string SourceHint => _completion.SourceHint + (English.Keys.Any(k=>!_completion.HasNativeMessage(k)) ? "\n"+_editor.Text["TrialMessageFallback"] : "");
    private string Native(string key) => _completion.Message(key,English[key]);
    private PageKind? DesignPage => PageId switch { "Welcome"=>PageKind.Welcome,"Destination"=>PageKind.Destination,_=>null };
    private string Resolve(ElementKind element) => WizardTexts.Resolve(_editor.Project,LanguageId,WizardTexts.Slot(DesignPage,element)).Text;
    public string Title => PageId switch
    {
        "Welcome" or "Destination"=>Resolve(ElementKind.Title),
        "License"=>Native("WizardLicense"),"InfoBefore"=>Native("WizardInfoBefore"),"InfoAfter"=>Native("WizardInfoAfter"),
        "UserInfo"=>Native("WizardUserInfo"),_=>_completion.Title
    };
    public string Body => PageId switch
    {
        "Welcome" or "Destination"=>Resolve(ElementKind.Body),"License"=>Native("LicenseLabel3"),
        "InfoBefore"=>Native("InfoBeforeLabel"),"InfoAfter"=>Native("InfoAfterLabel"),"UserInfo"=>Native("UserInfoDesc"),_=>_completion.Body
    };
    public string LicenseAcceptCaption => Native("LicenseAccepted");
    public string LicenseDeclineCaption => Native("LicenseNotAccepted");
    public string UserNameCaption => Native("UserInfoName");
    public string OrganizationCaption => Native("UserInfoOrg");
    public string BrowseCaption => WizardTexts.Resolve(_editor.Project,LanguageId,WizardTextSlot.DestinationBrowse).Text;
    public string BrowseTooltip => WizardTexts.Resolve(_editor.Project,LanguageId,WizardTextSlot.DestinationTooltip).Text;
    public string BackCaption => Resolve(ElementKind.Back);
    public string CancelCaption => Resolve(ElementKind.Cancel);
    public string NextCaption => IsReady || IsFinished || IsPreparing ? _completion.NextCaption : Resolve(ElementKind.Next);
    public bool ShowNext => !IsInstalling;
    public bool ShowBack => IsPreparing || _index>0 && !IsInstalling && PageId!="InfoAfter" && !IsFinished;
    public bool ShowCancel => !IsFinished && PageId!="InfoAfter";
    public bool CanBack => CanInteract && (IsPreparing || _index>0 && !IsInstalling && PageId!="InfoAfter" && !IsFinished);
    public bool CanNext => CanInteract && !IsInstalling && !IsPreparing
        && (!IsDocument || DocumentAvailable) && (!IsLicense || LicenseAccepted)
        && (!IsUserInfo || !string.IsNullOrWhiteSpace(UserName)) && (!IsDestination || !string.IsNullOrWhiteSpace(Directory));
    public bool CanComplete => CanInteract && IsInstalling && _completion.CanComplete;
    public string Summary => string.Join("\n",new[]{_completion.Message("ReadyMemoDir","Destination location:"),Directory}
        .Concat(_editor.Project.PageSettings.UserInfoEnabled ? new[]{"",_completion.Message("ReadyMemoUserInfo","User information:"),UserName,Organization} : []));
    public void Next()
    {
        if(!CanNext) return;
        if(IsFinished) { ExitRequested=true; Refresh(); return; }
        if(IsReady) { _completion.Next(); if(_completion.IsPreparing) { Refresh(); return; } }
        Move(1);
    }
    public void Back()
    {
        if(!CanBack) return;
        if(IsPreparing) { _completion.Back(); Refresh(); }
        else Move(-1);
    }
    public void Cancel() { ExitRequested=true; Refresh(); }
    public void ResolvePreparation()
    {
        if(!CanInteract || !IsPreparing) return;
        _completion.ResolvePreparation(); Move(1);
    }
    public void CompleteInstallation()
    {
        if(!CanComplete) return;
        _completion.CompleteInstallation(); Move(1);
    }
    private void Move(int direction)
    {
        _index=Math.Clamp(_index+direction,0,Route.Count-1); _documentAvailable=false; Refresh();
    }
    public async Task LoadLanguageAsync(string id,string? compilerDirectory,string? projectDirectory)
    {
        if(!Languages.Any(l=>l.Id==id) || _disposed) return;
        _accepted=false; _documentAvailable=false;
        await _completion.LoadLanguageAsync(id,compilerDirectory,projectDirectory); Refresh();
    }
    private void CompletionChanged(object? sender,PropertyChangedEventArgs e)=>Refresh();
    private void Refresh() => PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(string.Empty));
    public void Dispose()
    {
        if(_disposed) return;
        _disposed=true; _completion.PropertyChanged-=CompletionChanged; _editor.IsPreview=false;
    }
}
