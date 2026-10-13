using System.ComponentModel;

namespace Studio.Core;

public sealed class ElementChoice(ElementKind kind, string key, StudioLocalizer text) : INotifyPropertyChanged
{
    public ElementKind Kind => kind;
    public string Name => text[key];
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}

public sealed class PageChoice(int index, string nameKey, StudioLocalizer text) : INotifyPropertyChanged
{
    public int Index => index;
    public string Name => text[nameKey];
    public string Description => Index == 0 ? text["BaseShared"] : text.Format("PageNumber", Index);
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}

public sealed class EditorSession : INotifyPropertyChanged, IDisposable
{
    private int _pageIndex;
    private int _designPageIndex;
    private ElementKind _designElement;
    private string? _trialDirectory;
    private ElementKind _selectedElement = ElementKind.Next;
    private bool _isPreview;
    private double _zoom = 1;
    private string _previewLanguageId;
    private IReadOnlyList<InstallerLanguageChoice> _previewLanguages = [];
    private Dictionary<string, int> _translationCounts = [];

    public EditorSession(StudioProject project, StudioLocalizer? text = null)
    {
        Project = project;
        _previewLanguageId = project.Languages.First(l => l.Enabled).Id;
        IsDirty = project.WasMigrated;
        if (project.WasMigrated) _statusKey = "StatusMigrated";
        Text = text ?? new StudioLocalizer();
        UpdateTranslationCounts();
        RebuildPreviewLanguages();
        PageChoices = [new(0, "BaseDesign", Text),
            .. project.Pages.Select((p, i) => new PageChoice(i + 1,
                p.Kind == PageKind.Welcome ? "PageWelcome" : "ElementDirectory", Text))];
        ElementKind[] order = [ElementKind.Title, ElementKind.Body, ElementKind.Directory, ElementKind.Browse,
            ElementKind.Back, ElementKind.Next, ElementKind.Cancel];
        AllElements = order.Select(kind => new ElementChoice(kind, "Element" + kind, Text)).ToArray();
        _baseElements = AllElements.Where(e => e.Kind is ElementKind.Back or ElementKind.Next or ElementKind.Cancel).ToArray();
        _welcomeElements = AllElements.Where(e => e.Kind is not (ElementKind.Directory or ElementKind.Browse)).ToArray();
        Text.PropertyChanged += LanguageChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public StudioProject Project { get; }
    public StudioLocalizer Text { get; }
    private string _statusKey = "StatusSelect";
    public IReadOnlyList<PageChoice> PageChoices { get; }
    public StudioPage? CurrentPage => IsBase ? null : Project.Pages[PageIndex - 1];
    public bool IsDirty { get; private set; }
    public string Status => Text[_statusKey];
    public bool IsBase => PageIndex == 0;
    public bool IsDestination => CurrentPage?.Kind == PageKind.Destination;
    public bool IsDirectorySelected => SelectedElement == ElementKind.Directory;
    public bool IsBrowseSelected => SelectedElement == ElementKind.Browse;
    public bool ShowInheritance => !IsDirectorySelected;
    public bool IsDesign => !IsPreview;
    public bool CanEdit => IsDesign && (!IsBase || IsButton);
    public bool IsButton => SelectedElement is ElementKind.Back or ElementKind.Next or ElementKind.Cancel;
    public bool CanReset => CanEdit && !IsDirectorySelected && WizardTexts.Own(Project, PreviewLanguage, SelectedSlot) is not null;
    public bool ShowTextWatermark => ShowInheritance && string.IsNullOrEmpty(PropertyText);
    public bool ShowCopyText => ShowInheritance && string.IsNullOrEmpty(PropertyText);
    public bool ShowResetText => ShowInheritance && !string.IsNullOrEmpty(PropertyText);
    public bool CanGoBack => IsDesign || PageIndex > 1;
    public string PageName => PageChoices[PageIndex].Name;
    public string PageDescription => IsBase ? Text["BaseDescription"] : Text["PageDescription"];
    public string PreviewTitle => IsBase ? Text["BaseTitle"] : Resolve(ElementKind.Title).Text;
    public string PreviewBody => IsBase ? Text["BaseBody"] : Resolve(ElementKind.Body).Text;
    public string BackCaption => Caption(ElementKind.Back);
    public string NextCaption => Caption(ElementKind.Next);
    public string CancelCaption => Caption(ElementKind.Cancel);
    private string Caption(ElementKind kind) => Resolve(kind).Text;
    private ResolvedWizardText Resolve(ElementKind kind) => WizardTexts.Resolve(Project, PreviewLanguageId, WizardTexts.Slot(CurrentPage?.Kind, kind));
    private WizardTextSlot SelectedSlot => WizardTexts.Slot(CurrentPage?.Kind, SelectedElement);
    private ResolvedWizardText SelectedText => WizardTexts.Resolve(Project, PreviewLanguageId, SelectedSlot);
    public bool SelectedTextMissing => !IsDirectorySelected && SelectedText.Missing;
    public string EffectiveText => IsDirectorySelected ? DirectoryText : SelectedText.Text;
    public string EditingLanguageName => PreviewLanguage.DisplayName(Text);
    public string DefaultLanguageId => Project.Languages.First(l => l.Enabled).Id;
    public string DefaultLanguageName => Project.Languages.First(l => l.Enabled).DisplayName(Text);
    public int MissingTranslationCount => _translationCounts.Values.Sum();
    internal int TranslationCount(string id) => _translationCounts.GetValueOrDefault(id);
    public bool HasMissingTranslations => MissingTranslationCount > 0;
    public string TranslationSummary => HasMissingTranslations ? Text.Format("TranslationSummaryMissing", MissingTranslationCount, DefaultLanguageName)
        : Text.Format("TranslationSummaryComplete", DefaultLanguageName);
    private string Native(string key) => WizardTexts.NativeMessage(Project, PreviewLanguageId, key).Text;
    private InstallerLanguage PreviewLanguage => Project.Languages.Single(l => l.Id == PreviewLanguageId);
    public bool IsTextLanguage => PreviewLanguageId == Project.TextLanguageId;
    public string PreviewWindowTitle => Native("SetupWindowTitle");
    public string PreviewDirectoryLabel => Native("WizardSelectDir");
    public string PreviewLanguageHint => Text.Format("TranslationLanguageStatus", TranslationCount(PreviewLanguageId))
        + (PreviewLanguage.RightToLeft ? "\n" + Text["PreviewRtlLimitation"] : "");
    public IReadOnlyList<InstallerLanguageChoice> PreviewLanguages => _previewLanguages;
    public string PreviewLanguageId
    {
        get => _previewLanguageId;
        set { if (value == _previewLanguageId || !Project.Languages.Any(l => l.Id == value && l.Enabled)) return; _previewLanguageId = value; Refresh(); }
    }
    public void AddLanguage(InstallerLanguage language)
    {
        if (!IsDesign || Project.Languages.Count >= 100 || Project.Languages.Any(l => l.Id == language.Id
            || string.Equals(l.Source, language.Source, StringComparison.OrdinalIgnoreCase))) return;
        Project.Languages.Add(language); RebuildPreviewLanguages(); IsDirty = true; Refresh();
    }
    public void SetLanguageEnabled(string id, bool enabled)
    {
        var language = Project.Languages.SingleOrDefault(l => l.Id == id);
        if (!IsDesign || language is null || language.Enabled == enabled || (!enabled && Project.Languages.Count(l => l.Enabled) == 1)) return;
        language.Enabled = enabled;
        if (!Project.Languages.Any(l => l.Id == _previewLanguageId && l.Enabled)) _previewLanguageId = Project.Languages.First(l => l.Enabled).Id;
        RebuildPreviewLanguages(); IsDirty = true; Refresh();
    }
    public void MoveLanguage(string id, int direction)
    {
        var index = Project.Languages.FindIndex(l => l.Id == id);
        var target = index + direction;
        if (!IsDesign || index < 0 || direction is not (-1 or 1) || target < 0 || target >= Project.Languages.Count) return;
        (Project.Languages[index], Project.Languages[target]) = (Project.Languages[target], Project.Languages[index]);
        RebuildPreviewLanguages(); IsDirty = true; Refresh();
    }
    private void RebuildPreviewLanguages()
    {
        var existing = _previewLanguages.ToDictionary(l => l.Id);
        _previewLanguages = Project.Languages.Where(l => l.Enabled)
            .Select(l => existing.GetValueOrDefault(l.Id) ?? new InstallerLanguageChoice(l, this)).ToArray();
    }
    public string DirectoryText => IsPreview && _trialDirectory is not null ? _trialDirectory : CurrentPage?.Destination?.DefaultDirectory ?? "";
    public string TrialDirectory { get => DirectoryText; set => SetTrialDirectory(value); }
    public string BrowseCaption => WizardTexts.Resolve(Project, PreviewLanguageId, WizardTextSlot.DestinationBrowse).Text;
    public string PropertyLabel => IsDirectorySelected ? Text["DefaultDirectoryLabel"] : Text["OwnTranslationText"];
    public string PropertyHint => IsDirectorySelected
        ? Text["DirectoryHint"]
        : IsBrowseSelected ? Text["BrowseHint"]
        : Text["TextHint"];
    public bool AllowsMultiline => SelectedElement == ElementKind.Body;
    public string SelectedElementName => AllElements.First(e => e.Kind == SelectedElement).Name;
    public string Origin => IsDirectorySelected ? Text["OriginPage"] : DescribeOrigin(SelectedText);
    private string DescribeOrigin(ResolvedWizardText resolved) => resolved.Origin switch
    {
        WizardTextOrigin.Local => IsButton ? Text["OriginOverride"] : Text["OriginPage"],
        WizardTextOrigin.Shared => IsBase ? Text["OriginBase"] : Text["OriginInherited"],
        WizardTextOrigin.Fallback => Text.Format("TranslationOriginFallback", DefaultLanguageName),
        WizardTextOrigin.BuiltIn => Text["TranslationOriginBuiltIn"],
        _ => Text["TranslationOriginNative"]
    };
    public string SelectedTranslationHint => SelectedTextMissing ? Text.Format("TranslationMissingField", DefaultLanguageName) : Text["TranslationEditHint"];
    public string DocumentState => IsDirty ? Text["Unsaved"] : Text["Unchanged"];

    public string ProductName
    {
        get => Project.Name;
        set { if (!IsDesign || value == Project.Name) return; Project.Name = value; IsDirty = true; Refresh(); }
    }

    public string ApplicationId
    {
        get => Project.AppId;
        set { if (!IsDesign || value == Project.AppId) return; Project.AppId = value; IsDirty = true; Refresh(); }
    }

    public string ApplicationVersion
    {
        get => Project.AppVersion;
        set { if (!IsDesign || value == Project.AppVersion) return; Project.AppVersion = value; IsDirty = true; Refresh(); }
    }

    public string OutputBaseFileName
    {
        get => Project.OutputBaseFileName;
        set { if (!IsDesign || value == OutputBaseFileName) return; Project.OutputBaseFileName = value; IsDirty = true; Refresh(); }
    }
    public bool HasSetupNameWarning => InstallerOutput.IsSetup(OutputBaseFileName);

    public string SourceFile
    {
        get => Project.InstallFile.Source;
        set { if (!IsDesign || value == SourceFile) return; Project.InstallFile.Source = value; IsDirty = true; Refresh(); }
    }

    public string FileDestination
    {
        get => Project.InstallFile.Destination;
        set { if (!IsDesign || value == FileDestination) return; Project.InstallFile.Destination = value; IsDirty = true; Refresh(); }
    }

    public IReadOnlyList<ElementChoice> AllElements { get; }
    private readonly IReadOnlyList<ElementChoice> _baseElements;
    private readonly IReadOnlyList<ElementChoice> _welcomeElements;
    public IReadOnlyList<ElementChoice> Elements => IsBase ? _baseElements : IsDestination ? AllElements : _welcomeElements;

    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (value < 0 || value >= PageChoices.Count || (IsPreview && value == 0) || value == _pageIndex) return;
            _pageIndex = value;
            if (!Elements.Any(e => e.Kind == _selectedElement)) _selectedElement = ElementKind.Next;
            Refresh();
        }
    }

    public ElementKind SelectedElement
    {
        get => _selectedElement;
        set
        {
            if (_selectedElement == value || IsPreview || !Elements.Any(e => e.Kind == value)) return;
            _selectedElement = value;
            Refresh();
        }
    }

    public bool IsPreview
    {
        get => _isPreview;
        set
        {
            if (_isPreview == value) return;
            _isPreview = value;
            if (value)
            {
                _designPageIndex = _pageIndex;
                _designElement = _selectedElement;
                _trialDirectory = null;
                if (IsBase) _pageIndex = 1;
                _statusKey = "StatusTryOut";
            }
            else
            {
                _pageIndex = _designPageIndex;
                _selectedElement = _designElement;
                _trialDirectory = null;
                _statusKey = "StatusDesign";
            }
            Refresh();
        }
    }

    public double Zoom
    {
        get => _zoom;
        set { _zoom = Math.Clamp(value, 0.5, 1.5); Refresh(); }
    }

    public string PropertyText
    {
        get => IsDirectorySelected ? CurrentPage?.Destination?.DefaultDirectory ?? ""
            : WizardTexts.Own(Project, PreviewLanguage, SelectedSlot) ?? "";
        set
        {
            if (!CanEdit || PropertyText == value) return;
            if (IsDirectorySelected) CurrentPage!.Destination!.DefaultDirectory = value;
            else WizardTexts.Set(Project, PreviewLanguage, SelectedSlot, value);
            IsDirty = true;
            Refresh();
        }
    }

    public string BrowseTooltip
    {
        get => WizardTexts.Own(Project, PreviewLanguage, WizardTextSlot.DestinationTooltip) ?? "";
        set
        {
            if (!CanEdit || !IsBrowseSelected || BrowseTooltip == value) return;
            WizardTexts.Set(Project, PreviewLanguage, WizardTextSlot.DestinationTooltip, value);
            IsDirty = true;
            Refresh();
        }
    }
    public string EffectiveBrowseTooltip => WizardTexts.Resolve(Project, PreviewLanguageId, WizardTextSlot.DestinationTooltip).Text;
    public string BrowseTooltipOrigin => DescribeOrigin(WizardTexts.Resolve(Project, PreviewLanguageId, WizardTextSlot.DestinationTooltip));
    public bool CanResetTooltip => CanEdit && IsBrowseSelected && WizardTexts.Own(Project, PreviewLanguage, WizardTextSlot.DestinationTooltip) is not null;
    public bool ShowTooltipWatermark => string.IsNullOrEmpty(BrowseTooltip);
    public bool ShowCopyTooltip => string.IsNullOrEmpty(BrowseTooltip);
    public bool ShowResetTooltip => !string.IsNullOrEmpty(BrowseTooltip);
    public bool CanCopyTooltip => CanEdit && IsBrowseSelected && ShowCopyTooltip && !string.IsNullOrEmpty(EffectiveBrowseTooltip);
    public void UseEffectiveTooltip()
    {
        if (!CanCopyTooltip) return;
        var value = WizardTexts.Resolve(Project, PreviewLanguageId, WizardTextSlot.DestinationTooltip).RawText;
        WizardTexts.Set(Project, PreviewLanguage, WizardTextSlot.DestinationTooltip, value); IsDirty = true; Refresh();
    }
    public void ResetTooltip()
    {
        if (!CanResetTooltip) return;
        WizardTexts.Reset(Project, PreviewLanguage, WizardTextSlot.DestinationTooltip); IsDirty = true; Refresh();
    }
    public void UseEffectiveText()
    {
        if (!CanEdit || !ShowCopyText) return;
        if (WizardTexts.Own(Project, PreviewLanguage, SelectedSlot) == SelectedText.RawText) return;
        WizardTexts.Set(Project, PreviewLanguage, SelectedSlot, SelectedText.RawText); IsDirty = true; Refresh();
    }

    public void SetTrialDirectory(string path)
    {
        if (!IsPreview || !IsDestination) return;
        _trialDirectory = path;
        SetStatus("StatusTrialDirectory");
    }

    public void ResetProperty()
    {
        if (!CanReset) return;
        WizardTexts.Reset(Project, PreviewLanguage, SelectedSlot);
        IsDirty = true;
        Refresh();
    }

    public void Activate(ElementKind element)
    {
        if (IsDesign) { SelectedElement = element; return; }
        switch (element)
        {
            case ElementKind.Back when PageIndex > 1: PageIndex--; break;
            case ElementKind.Next when PageIndex < Project.Pages.Count: PageIndex++; break;
            case ElementKind.Next: SetStatus("StatusEnd"); break;
            case ElementKind.Cancel: IsPreview = false; break;
        }
    }

    public void MarkSaved() { Project.WasMigrated = false; IsDirty = false; SetStatus("StatusSaved"); }
    public void SetStatus(string resourceKey) { _statusKey = resourceKey; Refresh(); }
    private void LanguageChanged(object? sender, PropertyChangedEventArgs e)
    {
        foreach (var page in PageChoices) page.Refresh();
        foreach (var element in AllElements) element.Refresh();
        Refresh();
    }
    public void Dispose() => Text.PropertyChanged -= LanguageChanged;
    private void UpdateTranslationCounts() => _translationCounts = Project.Languages.Where(l => l.Enabled)
        .ToDictionary(l => l.Id, l => WizardTexts.MissingCount(Project, l.Id));
    private void Refresh()
    {
        UpdateTranslationCounts();
        foreach (var language in _previewLanguages) language.Refresh();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}

public sealed class InstallerLanguageChoice(InstallerLanguage language, EditorSession editor) : INotifyPropertyChanged
{
    public string Id => language.Id;
    public string Name => language.DisplayName(editor.Text);
    public string StatusName => Name + (editor.TranslationCount(Id) > 0 ? " (" + editor.TranslationCount(Id) + ")" : "");
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}
