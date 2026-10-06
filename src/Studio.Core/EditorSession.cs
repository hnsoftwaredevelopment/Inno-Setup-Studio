using System.ComponentModel;

namespace Studio.Core;

public sealed record ElementChoice(ElementKind Kind, string Name);
public sealed record PageChoice(int Index, string Name, string Description);

public sealed class EditorSession : INotifyPropertyChanged
{
    private int _pageIndex;
    private int _designPageIndex;
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
    public string SelectedElementName => AllElements.First(e => e.Kind == SelectedElement).Name;
    public string Origin => IsBase ? "Basisontwerp · geldt voor alle pagina’s"
        : !IsButton ? "Inhoud van deze pagina"
        : CanReset ? "Aangepast voor deze pagina" : "Overgenomen uit Basisontwerp";
    public string DocumentState => IsDirty ? "Niet-opgeslagen wijzigingen" : "Geen wijzigingen";

    public static IReadOnlyList<ElementChoice> AllElements { get; } =
    [new(ElementKind.Title, "Titel"), new(ElementKind.Body, "Beschrijving"),
     new(ElementKind.Back, "Terug"), new(ElementKind.Next, "Verder"), new(ElementKind.Cancel, "Annuleren")];
    public IEnumerable<ElementChoice> Elements => IsBase ? AllElements.Where(e => e.Kind >= ElementKind.Back) : AllElements;

    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (value < 0 || value >= PageChoices.Count || (IsPreview && value == 0) || value == _pageIndex) return;
            _pageIndex = value;
            if (IsBase && !IsButton) _selectedElement = ElementKind.Next;
            Refresh();
        }
    }

    public ElementKind SelectedElement
    {
        get => _selectedElement;
        set
        {
            if (IsPreview || !Enum.IsDefined(value) || (IsBase && value < ElementKind.Back)) return;
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
                if (IsBase) _pageIndex = 1;
                Status = "Uitproberen: navigeer met de knoppen. Er wordt niets geïnstalleerd.";
            }
            else
            {
                _pageIndex = _designPageIndex;
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
            _ => Project.Caption(CurrentPage, SelectedElement)
        };
        set
        {
            if (!CanEdit || PropertyText == value) return;
            switch (SelectedElement)
            {
                case ElementKind.Title: CurrentPage!.Title = value; break;
                case ElementKind.Body: CurrentPage!.Body = value; break;
                default:
                    if (IsBase) Project.Buttons[SelectedElement] = value;
                    else CurrentPage!.ButtonOverrides[SelectedElement] = value;
                    break;
            }
            IsDirty = true;
            Refresh();
        }
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
