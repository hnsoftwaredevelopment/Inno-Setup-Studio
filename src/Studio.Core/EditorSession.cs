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

    public EditorSession(StudioProject project, StudioLocalizer? text = null)
    {
        Project = project;
        _previewLanguageId = project.Languages.First(l => l.Enabled).Id;
        IsDirty = project.WasMigrated;
        if (project.WasMigrated) _statusKey = "StatusMigrated";
        Text = text ?? new StudioLocalizer();
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
    public bool ShowInheritance => IsButton;
    public bool IsDesign => !IsPreview;
    public bool CanEdit => IsDesign && (IsTextLanguage || IsDirectorySelected) && (!IsBase || IsButton);
    public bool IsButton => SelectedElement is ElementKind.Back or ElementKind.Next or ElementKind.Cancel;
    public bool CanReset => CanEdit && !IsBase && IsButton && CurrentPage!.ButtonOverrides.ContainsKey(SelectedElement);
    public bool CanGoBack => IsDesign || PageIndex > 1;
    public string PageName => PageChoices[PageIndex].Name;
    public string PageDescription => IsBase ? Text["BaseDescription"] : Text["PageDescription"];
    public string PreviewTitle => IsBase ? Text["BaseTitle"] : IsTextLanguage ? CurrentPage!.Title : Native(CurrentPage!.Kind == PageKind.Welcome ? "WelcomeLabel1" : "WizardSelectDir");
    public string PreviewBody => IsBase ? Text["BaseBody"] : IsTextLanguage ? CurrentPage!.Body : Native(CurrentPage!.Kind == PageKind.Welcome ? "WelcomeLabel2" : "SelectDirLabel3");
    public string BackCaption => Caption(ElementKind.Back);
    public string NextCaption => Caption(ElementKind.Next);
    public string CancelCaption => Caption(ElementKind.Cancel);
    private string Caption(ElementKind kind) => IsTextLanguage ? Project.Caption(CurrentPage, kind) : Native("Button" + kind);
    private string Native(string key) => PreviewLanguage.Message(key, Project);
    private InstallerLanguage PreviewLanguage => Project.Languages.Single(l => l.Id == PreviewLanguageId);
    public bool IsTextLanguage => PreviewLanguageId == Project.TextLanguageId;
    public string PreviewWindowTitle => Native("SetupWindowTitle");
    public string PreviewDirectoryLabel => Native("WizardSelectDir");
    public string PreviewLanguageHint => Text[IsTextLanguage ? "PreviewOwnTexts" : "PreviewNativeTexts"]
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
            .Select(l => existing.GetValueOrDefault(l.Id) ?? new InstallerLanguageChoice(l, Text)).ToArray();
    }
    public string DirectoryText => IsPreview && _trialDirectory is not null ? _trialDirectory : CurrentPage?.Destination?.DefaultDirectory ?? "";
    public string TrialDirectory { get => DirectoryText; set => SetTrialDirectory(value); }
    public string BrowseCaption => IsTextLanguage ? CurrentPage?.Destination?.BrowseCaption ?? "" : Native("ButtonBrowse");
    public string PropertyLabel => IsDirectorySelected ? Text["DefaultDirectoryLabel"] : Text["TextLabel"];
    public string PropertyHint => IsDirectorySelected
        ? Text["DirectoryHint"]
        : IsBrowseSelected ? Text["BrowseHint"]
        : Text["TextHint"];
    public bool AllowsMultiline => SelectedElement == ElementKind.Body;
    public string SelectedElementName => AllElements.First(e => e.Kind == SelectedElement).Name;
    public string Origin => !IsTextLanguage && !IsDirectorySelected ? Text["PreviewNativeTexts"] : IsBase ? Text["OriginBase"]
        : !IsButton ? Text["OriginPage"]
        : CurrentPage!.ButtonOverrides.ContainsKey(SelectedElement) ? Text["OriginOverride"] : Text["OriginInherited"];
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
        get => SelectedElement switch
        {
            ElementKind.Title => PreviewTitle,
            ElementKind.Body => PreviewBody,
            ElementKind.Directory => CurrentPage?.Destination?.DefaultDirectory ?? "",
            ElementKind.Browse => BrowseCaption,
            _ => Caption(SelectedElement)
        };
        set
        {
            if (!CanEdit || PropertyText == value) return;
            switch (SelectedElement)
            {
                case ElementKind.Title: CurrentPage!.Title = value; break;
                case ElementKind.Body: CurrentPage!.Body = value; break;
                case ElementKind.Directory: CurrentPage!.Destination!.DefaultDirectory = value; break;
                case ElementKind.Browse: CurrentPage!.Destination!.BrowseCaption = value; break;
                default:
                    if (IsBase) Project.Buttons[SelectedElement] = value;
                    else CurrentPage!.ButtonOverrides[SelectedElement] = value;
                    break;
            }
            IsDirty = true;
            Refresh();
        }
    }

    public string BrowseTooltip
    {
        get => IsTextLanguage ? CurrentPage?.Destination?.BrowseTooltip ?? "" : "";
        set
        {
            if (!CanEdit || !IsBrowseSelected || CurrentPage?.Destination is not { } settings || settings.BrowseTooltip == value) return;
            settings.BrowseTooltip = value;
            IsDirty = true;
            Refresh();
        }
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
        CurrentPage!.ButtonOverrides.Remove(SelectedElement);
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
        foreach (var language in _previewLanguages) language.Refresh();
        Refresh();
    }
    public void Dispose() => Text.PropertyChanged -= LanguageChanged;
    private void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}

public sealed class InstallerLanguageChoice(InstallerLanguage language, StudioLocalizer text) : INotifyPropertyChanged
{
    public string Id => language.Id;
    public string Name => language.DisplayName(text);
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}
