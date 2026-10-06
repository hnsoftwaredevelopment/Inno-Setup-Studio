using System.ComponentModel;

namespace Studio.Core;

public sealed record ElementChoice(ElementKind Kind, string Name);
public sealed record PageChoice(int Index, string Name, string Description);

public sealed class EditorSession : INotifyPropertyChanged
{
    private int _pageIndex;
    private int _designPageIndex;
    private ElementKind _designElement;
    private string? _trialDirectory;
    private ElementKind _selectedElement = ElementKind.Next;
    private bool _isPreview;
    private double _zoom = 1;

    public EditorSession(StudioProject project)
    {
        Project = project;
        PageChoices = [new(0, "Basisontwerp", "Gedeeld op alle pagina’s"),
            .. project.Pages.Select((p, i) => new PageChoice(i + 1, p.Name, $"Pagina {i + 1:00}"))];
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public StudioProject Project { get; }
    public IReadOnlyList<PageChoice> PageChoices { get; }
    public StudioPage? CurrentPage => IsBase ? null : Project.Pages[PageIndex - 1];
    public bool IsDirty { get; private set; }
    public string Status { get; private set; } = "Selecteer een element op het ontwerpvlak of in de elementenlijst.";
    public bool IsBase => PageIndex == 0;
    public bool IsDestination => CurrentPage?.Kind == PageKind.Destination;
    public bool IsDirectorySelected => SelectedElement == ElementKind.Directory;
    public bool IsBrowseSelected => SelectedElement == ElementKind.Browse;
    public bool ShowInheritance => IsButton;
    public bool IsDesign => !IsPreview;
    public bool CanEdit => IsDesign && (!IsBase || IsButton);
    public bool IsButton => SelectedElement is ElementKind.Back or ElementKind.Next or ElementKind.Cancel;
    public bool CanReset => CanEdit && !IsBase && IsButton && CurrentPage!.ButtonOverrides.ContainsKey(SelectedElement);
    public bool CanGoBack => IsDesign || PageIndex > 1;
    public string PageName => PageChoices[PageIndex].Name;
    public string PageDescription => IsBase ? "Gedeelde knoppen · wijzigingen werken door op alle pagina’s" : "Pagina-inhoud en eventuele afwijkingen van het basisontwerp";
    public string PreviewTitle => CurrentPage?.Title ?? "Uw gedeelde basisontwerp";
    public string PreviewBody => CurrentPage?.Body ?? "Hier verschijnt de inhoud van elke pagina.\n\nSelecteer hieronder een navigatieknop om de gedeelde tekst aan te passen.";
    public string BackCaption => Project.Caption(CurrentPage, ElementKind.Back);
    public string NextCaption => Project.Caption(CurrentPage, ElementKind.Next);
    public string CancelCaption => Project.Caption(CurrentPage, ElementKind.Cancel);
    public string DirectoryText => IsPreview && _trialDirectory is not null ? _trialDirectory : CurrentPage?.Destination?.DefaultDirectory ?? "";
    public string TrialDirectory { get => DirectoryText; set => SetTrialDirectory(value); }
    public string BrowseCaption => CurrentPage?.Destination?.BrowseCaption ?? "";
    public string PropertyLabel => IsDirectorySelected ? "_Standaard installatiemap" : "_Tekst";
    public string PropertyHint => IsDirectorySelected
        ? "Bijvoorbeeld {autopf}\\Mijn applicatie. Inno Setup bepaalt deze locatie op de doelcomputer. Een gekozen map in Uitproberen verandert dit standaardpad niet."
        : IsBrowseSelected ? "Deze knop hoort bij de pagina Installatiemap. In Uitproberen opent hij de mapkeuze."
        : "De tekst wordt direct in het ontwerpvoorbeeld bijgewerkt.";
    public bool AllowsMultiline => SelectedElement == ElementKind.Body;
    public string SelectedElementName => AllElements.First(e => e.Kind == SelectedElement).Name;
    public string Origin => IsBase ? "Basisontwerp · geldt voor alle pagina’s"
        : !IsButton ? "Inhoud van deze pagina"
        : CurrentPage!.ButtonOverrides.ContainsKey(SelectedElement) ? "Aangepast voor deze pagina" : "Overgenomen uit Basisontwerp";
    public string DocumentState => IsDirty ? "Niet-opgeslagen wijzigingen" : "Geen wijzigingen";

    public static IReadOnlyList<ElementChoice> AllElements { get; } =
    [new(ElementKind.Title, "Titel"), new(ElementKind.Body, "Beschrijving"),
     new(ElementKind.Directory, "Installatiemap"), new(ElementKind.Browse, "Bladeren"),
     new(ElementKind.Back, "Terug"), new(ElementKind.Next, "Verder"), new(ElementKind.Cancel, "Annuleren")];
    public IEnumerable<ElementChoice> Elements => AllElements.Where(e => IsBase
        ? e.Kind is ElementKind.Back or ElementKind.Next or ElementKind.Cancel
        : IsDestination || e.Kind is not (ElementKind.Directory or ElementKind.Browse));

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
            if (IsPreview || !Elements.Any(e => e.Kind == value)) return;
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
                Status = "Uitproberen: navigeer met de knoppen. Er wordt niets geïnstalleerd.";
            }
            else
            {
                _pageIndex = _designPageIndex;
                _selectedElement = _designElement;
                _trialDirectory = null;
                Status = "Ontwerpen: klik op een element om het te bewerken.";
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
            _ => Project.Caption(CurrentPage, SelectedElement)
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
        get => CurrentPage?.Destination?.BrowseTooltip ?? "";
        set
        {
            if (!IsDesign || !IsBrowseSelected || CurrentPage?.Destination is not { } settings || settings.BrowseTooltip == value) return;
            settings.BrowseTooltip = value;
            IsDirty = true;
            Refresh();
        }
    }

    public void SetTrialDirectory(string path)
    {
        if (!IsPreview || !IsDestination) return;
        _trialDirectory = path;
        SetStatus("Map gekozen voor deze uitprobeersessie. Het standaardpad is ongewijzigd.");
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
            case ElementKind.Next: SetStatus("Einde van het ontwerpvoorbeeld. Er wordt niets geïnstalleerd."); break;
            case ElementKind.Cancel: IsPreview = false; break;
        }
    }

    public void MarkSaved() { IsDirty = false; SetStatus("Project opgeslagen."); }
    public void SetStatus(string message) { Status = message; Refresh(); }
    private void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}
