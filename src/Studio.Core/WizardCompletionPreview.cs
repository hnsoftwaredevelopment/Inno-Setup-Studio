using System.ComponentModel;
namespace Studio.Core;
public enum CompletionPhase { Ready, Preparing, Installing, Finished }

/// <summary>A local simulation: only reads the project and native language files.</summary>
public sealed class WizardCompletionPreview : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    private string _languageId;
    private int _progress, _request;
    private bool _problem, _loading;
    private IReadOnlyDictionary<string,string> _messages = new Dictionary<string,string>();
    private static readonly Dictionary<string,string> English = new()
    {
        ["ButtonInstall"]="&Install", ["ButtonFinish"]="&Finish", ["WizardReady"]="Ready to Install",
        ["ReadyLabel1"]="Setup is now ready to begin installing [name] on your computer.",
        ["ReadyLabel2b"]="Click Install to continue with the installation.", ["ReadyMemoDir"]="Destination location:",
        ["ReadyMemoUserInfo"]="User information:", ["WizardPreparing"]="Preparing to Install",
        ["PreparingDesc"]="Setup is preparing to install [name] on your computer.", ["WizardInstalling"]="Installing",
        ["InstallingLabel"]="Please wait while Setup installs [name] on your computer.",
        ["FinishedHeadingLabel"]="Completing the [name] Setup Wizard",
        ["FinishedLabelNoIcons"]="Setup has finished installing [name] on your computer."
    };
    public WizardCompletionPreview(EditorSession editor, CompletionPhase phase)
    {
        _editor=editor; _languageId=editor.PreviewLanguageId; Phase=phase;
        _problem=phase==CompletionPhase.Preparing;
        if(phase==CompletionPhase.Finished) _progress=100;
        Languages=editor.Project.Languages.Where(l=>l.Enabled).Select(l=>new InstallerLanguageChoice(l,editor)).ToArray();
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public IReadOnlyList<InstallerLanguageChoice> Languages { get; }
    public string LanguageId => _languageId;
    public CompletionPhase Phase { get; private set; }
    public bool ExitRequested { get; private set; }
    public bool CanInteract => !_loading;
    public bool IsReady => Phase==CompletionPhase.Ready;
    public bool IsPreparing => Phase==CompletionPhase.Preparing;
    public bool IsInstalling => Phase==CompletionPhase.Installing;
    public bool IsFinished => Phase==CompletionPhase.Finished;
    public bool HasFallback => English.Keys.Any(key=>!_messages.TryGetValue(key,out var value)||string.IsNullOrWhiteSpace(value));
    private string SourceStatus => _loading ? _editor.Text["CompletionLoading"] : HasFallback ? _editor.Text["CompletionSourceFallback"] : _editor.Text["CompletionNativeSource"];
    public string SourceHint => SourceStatus + (_editor.Project.Languages.Single(l => l.Id == LanguageId).RightToLeft ? "\n" + _editor.Text["PreviewRtlLimitation"] : "");
    public string WindowTitle => WizardTexts.NativeMessage(_editor.Project,LanguageId,"SetupWindowTitle").Text;
    public string Title => Native(Phase switch { CompletionPhase.Ready=>"WizardReady", CompletionPhase.Preparing=>"WizardPreparing", CompletionPhase.Installing=>"WizardInstalling", _=>"FinishedHeadingLabel" });
    public string Body => Native(Phase switch { CompletionPhase.Ready=>"ReadyLabel1", CompletionPhase.Preparing=>"PreparingDesc", CompletionPhase.Installing=>"InstallingLabel", _=>"FinishedLabelNoIcons" }) + (IsReady ? "\n\n"+Native("ReadyLabel2b") : "");
    public string Summary
    {
        get
        {
            var lines=new List<string> { Native("ReadyMemoDir"), _editor.Project.Pages[1].Destination!.DefaultDirectory };
            if(_editor.Project.PageSettings.UserInfoEnabled)
                lines.AddRange(["",Native("ReadyMemoUserInfo"),WizardTexts.Expand(_editor.Project.PageSettings.DefaultName,_editor.Project),WizardTexts.Expand(_editor.Project.PageSettings.DefaultOrganization,_editor.Project)]);
            return string.Join("\n",lines);
        }
    }
    public string BackCaption => WizardTexts.Resolve(_editor.Project,LanguageId,WizardTextSlot.SharedBack).Text;
    public string CancelCaption => WizardTexts.Resolve(_editor.Project,LanguageId,WizardTextSlot.SharedCancel).Text;
    public string NextCaption => IsReady ? Native("ButtonInstall") : IsFinished ? Native("ButtonFinish") : WizardTexts.NativeMessage(_editor.Project,LanguageId,"ButtonNext").Text;
    public bool ShowNext => !IsInstalling;
    public bool ShowBack => !IsFinished;
    public bool CanBack => IsPreparing;
    public bool ShowCancel => !IsFinished;
    public bool CanNext => CanInteract && (IsReady || IsFinished);
    public bool CanComplete => CanInteract && IsInstalling && Progress>=100;
    public bool PreparationProblem { get=>_problem; set { _problem=value; Refresh(); } }
    public int Progress { get=>_progress; set { _progress=Math.Clamp(value,0,100); Refresh(); } }
    public void Next()
    {
        if(!CanNext) return;
        if(IsFinished) ExitRequested=true;
        else { Phase=PreparationProblem ? CompletionPhase.Preparing : CompletionPhase.Installing; _progress=0; }
        Refresh();
    }
    public void Back() { if(CanInteract && CanBack) { Phase=CompletionPhase.Ready; Refresh(); } }
    public void Cancel() { ExitRequested=true; Refresh(); }
    public void ResolvePreparation() { if(CanInteract && IsPreparing) { _problem=false; Phase=CompletionPhase.Installing; _progress=0; Refresh(); } }
    public void CompleteInstallation() { if(CanComplete) { Phase=CompletionPhase.Finished; Refresh(); } }
    public async Task LoadLanguageAsync(string id,string? compilerDirectory,string? projectDirectory)
    {
        var language=_editor.Project.Languages.SingleOrDefault(l=>l.Enabled && l.Id==id);
        if(language is null) return;
        var request=++_request; _languageId=id; _loading=true; Refresh();
        IReadOnlyDictionary<string,string> messages=new Dictionary<string,string>();
        try
        {
            var path=LanguageScript.Source(language.Source,projectDirectory);
            if(path.StartsWith("compiler:",StringComparison.OrdinalIgnoreCase))
            {
                if(compilerDirectory is null) throw new FileNotFoundException();
                path=Path.Combine(compilerDirectory,path[9..]);
            }
            messages=(await LanguageSourceFile.LoadAsync(path)).Messages;
        }
        catch(Exception error) when(error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) { }
        if(request!=_request) return;
        _messages=messages; _loading=false; Refresh();
    }
    private string Native(string key)
        => Message(key, English[key]);
    public bool HasNativeMessage(string key) => _messages.TryGetValue(key,out var value) && !string.IsNullOrWhiteSpace(value);
    public string Message(string key, string fallback)
    {
        var raw=HasNativeMessage(key) ? _messages[key] : fallback;
        return new InstallerLanguage { Messages=new Dictionary<string,string> { [key]=raw } }.Message(key,_editor.Project);
    }
    private void Refresh() => PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(string.Empty));
}
