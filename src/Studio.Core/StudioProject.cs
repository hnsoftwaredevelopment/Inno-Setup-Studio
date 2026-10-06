namespace Studio.Core;

public enum ElementKind { Title, Body, Back, Next, Cancel }
public enum PageKind { Welcome, Destination }

public sealed class StudioPage
{
    public PageKind Kind { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public Dictionary<ElementKind, string> ButtonOverrides { get; set; } = [];
}

public sealed class StudioProject
{
    public const string FormatId = "HN.InnoSetupStudio.VisualProject";
    public string Format { get; set; } = FormatId;
    public int Version { get; set; } = 1;
    public string Name { get; set; } = "Mijn applicatie";
    public Dictionary<ElementKind, string> Buttons { get; set; } = new()
    {
        [ElementKind.Back] = "< Terug", [ElementKind.Next] = "Verder >", [ElementKind.Cancel] = "Annuleren"
    };
    public List<StudioPage> Pages { get; set; } = [];

    public string Caption(StudioPage? page, ElementKind element) =>
        page is not null && page.ButtonOverrides.TryGetValue(element, out var local)
            ? local : Buttons[element];

    public static StudioProject CreateExample() => new()
    {
        Pages =
        [
            new() { Kind = PageKind.Welcome, Name = "Welkom", Title = "Welkom bij de installatie van Mijn applicatie",
                Body = "Deze wizard helpt u Mijn applicatie op uw computer te installeren.\n\nKlik op Verder om door te gaan." },
            new() { Kind = PageKind.Destination, Name = "Installatiemap", Title = "Kies de installatiemap",
                Body = "Waar wilt u Mijn applicatie installeren?" }
        ]
    };
}
