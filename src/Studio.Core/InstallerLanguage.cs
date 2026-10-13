namespace Studio.Core;

/// <summary>A selected source and the native messages needed by the current preview.</summary>
public sealed class InstallerLanguage
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int LanguageId { get; set; }
    public string Source { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public bool RightToLeft { get; set; }
    public Dictionary<string, string> Messages { get; set; } = [];
    public Dictionary<WizardTextSlot, string> TextOverrides { get; set; } = [];
    public HashSet<WizardTextSlot> StandardTexts { get; set; } = [];

    internal static readonly string[] PreviewKeys = ["SetupWindowTitle", "WelcomeLabel1", "WelcomeLabel2",
        "WizardSelectDir", "SelectDirLabel3", "ButtonBack", "ButtonNext", "ButtonCancel", "ButtonBrowse"];

    public static InstallerLanguage FromSource(LanguageSource source, string? compilerDirectory)
    {
        var reference = source.FilePath;
        if (compilerDirectory is not null)
        {
            var relative = Path.GetRelativePath(compilerDirectory, source.FilePath);
            if (!Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar))
                reference = "compiler:" + relative;
        }
        return new()
        {
            Id = Guid.NewGuid().ToString("N"), Name = source.Name, LanguageId = source.LanguageId,
            Source = reference, RightToLeft = source.RightToLeft,
            Messages = PreviewKeys.Where(source.Messages.ContainsKey).ToDictionary(k => k, k => source.Messages[k])
        };
    }

    public string DisplayName(StudioLocalizer text)
    {
        var key = LanguageSourceNames.ResourceKey(LanguageId);
        var name = text[key];
        return name == "[" + key + "]" ? Name : name;
    }

    public string Message(string key, StudioProject project)
    {
        if (!Messages.TryGetValue(key, out var value)) return "[" + key + "]";
        value = value.Replace("%n", "\n", StringComparison.Ordinal);
        value = System.Text.RegularExpressions.Regex.Replace(value, "&&|&", match => match.Value == "&&" ? "&" : "");
        // Substitute once, after native formatting: product names are literal data, not message syntax.
        return System.Text.RegularExpressions.Regex.Replace(value, @"\[name/ver\]|\[name\]|%1(?![0-9])",
            match => match.Value == "[name/ver]" ? project.Name + " " + project.AppVersion : project.Name);
    }

    public static InstallerLanguage English() => new()
    {
        Id = "english", Name = "English", LanguageId = 0x0409, Source = "compiler:Default.isl",
        Messages = new() { ["SetupWindowTitle"] = "Setup - %1", ["WelcomeLabel1"] = "Welcome to the [name] Setup Wizard",
            ["WelcomeLabel2"] = "This will install [name/ver] on your computer.%n%nIt is recommended that you close all other applications before continuing.",
            ["WizardSelectDir"] = "Select Destination Location", ["SelectDirLabel3"] = "Setup will install [name] into the following folder.",
            ["ButtonBack"] = "< &Back", ["ButtonNext"] = "&Next >", ["ButtonCancel"] = "Cancel", ["ButtonBrowse"] = "&Browse..." }
    };

    public static InstallerLanguage Dutch() => new()
    {
        Id = "dutch", Name = "Nederlands", LanguageId = 0x0413, Source = @"compiler:Languages\Dutch.isl",
        Messages = new() { ["SetupWindowTitle"] = "Installatie - %1", ["WelcomeLabel1"] = "Welkom bij de installatie van [name]",
            ["WelcomeLabel2"] = "Deze wizard helpt u [name] op uw computer te installeren.",
            ["WizardSelectDir"] = "Kies de installatiemap", ["SelectDirLabel3"] = "Waar wilt u [name] installeren?",
            ["ButtonBack"] = "< Terug", ["ButtonNext"] = "Verder >", ["ButtonCancel"] = "Annuleren", ["ButtonBrowse"] = "Bladeren..." }
    };
}
