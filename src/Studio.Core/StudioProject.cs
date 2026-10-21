namespace Studio.Core;

public enum ElementKind { Title, Body, Back, Next, Cancel, Directory, Browse }
public enum PageKind { Welcome, Destination }

public sealed class StudioPage
{
    public PageKind Kind { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public Dictionary<ElementKind, string> ButtonOverrides { get; set; } = [];
    public DestinationSettings? Destination { get; set; }
}

public sealed class DestinationSettings
{
    public string DefaultDirectory { get; set; } = @"{autopf}\Mijn applicatie";
    public string BrowseCaption { get; set; } = "Bladeren...";
    public string BrowseTooltip { get; set; } = "Kies een andere installatiemap";
}

public sealed class StudioProject
{
    public const string FormatId = "HN.InnoSetupStudio.VisualProject";
    public string Format { get; set; } = FormatId;
    public const int CurrentVersion = 13;
    public int Version { get; set; } = CurrentVersion;
    public string Name { get; set; } = "Mijn applicatie";
    public string AppId { get; set; } = Guid.NewGuid().ToString("B");
    public string AppVersion { get; set; } = "1.0";
    public string OutputBaseFileName { get; set; } = "mysetup";
    public Dictionary<WizardDocumentKind, WizardDocumentSource> Documents { get; set; } = WizardDocuments.Empty();
    public WizardPageSettings PageSettings { get; set; } = new();
    public InstallFileSettings InstallFile { get; set; } = new();
    public List<InstallFileSettings> AdditionalFiles { get; set; } = [];
    public List<DirectoryRule> DirectoryRules { get; set; } = [];
    public List<ShortcutRule> Shortcuts { get; set; } = [];
    public List<InstallType> InstallTypes { get; set; } = [];
    public string SelectedInstallTypeId { get; set; } = "";
    public string TextLanguageId { get; set; } = "dutch";
    public List<InstallerLanguage> Languages { get; set; } = [InstallerLanguage.Dutch()];
    [System.Text.Json.Serialization.JsonIgnore]
    public bool WasMigrated { get; internal set; }
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
                Body = "Waar wilt u Mijn applicatie installeren?", Destination = new() }
        ]
    };

    public static StudioProject CreateNew()
    {
        var project = CreateExample();
        var english = InstallerLanguage.English();
        english.StandardTexts = Enum.GetValues<WizardTextSlot>().ToHashSet();
        project.Name = "My application";
        project.TextLanguageId = english.Id;
        project.Languages = [english];
        foreach (var kind in project.Buttons.Keys.ToArray())
            project.Buttons[kind] = english.Message("Button" + kind, project);
        foreach (var page in project.Pages)
        {
            page.Title = english.Message(page.Kind == PageKind.Welcome ? "WelcomeLabel1" : "WizardSelectDir", project);
            page.Body = english.Message(page.Kind == PageKind.Welcome ? "WelcomeLabel2" : "SelectDirLabel3", project);
        }
        project.Pages[1].Destination = new() { DefaultDirectory = @"{autopf}\My application",
            BrowseCaption = english.Message("ButtonBrowse", project), BrowseTooltip = "Choose another destination folder" };
        return project;
    }
}

public sealed class InstallFileSettings
{
    public string Source { get; set; } = "";
    public string Destination { get; set; } = "{app}";
    public bool RecurseSubdirectories { get; set; }
    public List<string> Excludes { get; set; } = [];
}
