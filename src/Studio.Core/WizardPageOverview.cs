namespace Studio.Core;

public enum WizardPageState { Enabled, Disabled, Conditional, Runtime }

public sealed record WizardPageSummary(int Order, string Id, string Name, WizardPageState State,
    string Status, string Reason, int? DesignIndex)
{
    public WizardDocumentKind? DocumentKind => WizardPageOverview.DocumentKindForPage(Id);
    public PageSettingsSection? SettingsSection => Id switch { "UserInfo" => PageSettingsSection.UserInfo, "ProgramGroup" => PageSettingsSection.ProgramGroup, _ => null };
    public bool CanDesign => DesignIndex is not null;
    public bool CanToggle => DocumentKind is not null || SettingsSection == PageSettingsSection.UserInfo;
    public bool IsOn => State is WizardPageState.Enabled or WizardPageState.Runtime;
}

/// <summary>The regular native flow for the current export. Not a list of editable project pages.</summary>
public static class WizardPageOverview
{
    internal static WizardDocumentKind? DocumentKindForPage(string id) => id switch
    {
        "License" => WizardDocumentKind.LicenseFile,
        "InfoBefore" => WizardDocumentKind.InfoBeforeFile,
        "InfoAfter" => WizardDocumentKind.InfoAfterFile,
        _ => null
    };
    public static IReadOnlyList<WizardPageSummary> Create(StudioLocalizer text, StudioProject? project = null)
    {
        // Until later slices add page settings, visibility follows InnoScript's fixed setup directives.
        (string Id, WizardPageState State, int? DesignIndex)[] pages =
        [
            ("Welcome", WizardPageState.Enabled, 1), ("License", WizardPageState.Disabled, null),
            ("Password", WizardPageState.Disabled, null), ("InfoBefore", WizardPageState.Disabled, null),
            ("UserInfo", WizardPageState.Disabled, null), ("Destination", WizardPageState.Enabled, 2),
            ("Components", WizardPageState.Disabled, null), ("ProgramGroup", WizardPageState.Disabled, null),
            ("Tasks", WizardPageState.Disabled, null), ("Ready", WizardPageState.Enabled, null),
            ("Preparing", WizardPageState.Conditional, null), ("Installing", WizardPageState.Runtime, null),
            ("InfoAfter", WizardPageState.Disabled, null), ("Finished", WizardPageState.Enabled, null)
        ];
        if (project is not null)
            for (var i = 0; i < pages.Length; i++)
            {
                var kind = DocumentKindForPage(pages[i].Id);
                if (pages[i].Id == "UserInfo" && project.PageSettings.UserInfoEnabled) pages[i].State = WizardPageState.Enabled;
                if (kind is { } document && WizardDocuments.IsActive(project.Documents[document]))
                    pages[i].State = WizardPageState.Enabled;
            }
        string ReasonKey(string id, WizardPageState state)
        {
            if (project is not null && DocumentKindForPage(id) is { } kind && !project.Documents[kind].Enabled
                && !string.IsNullOrWhiteSpace(project.Documents[kind].Source)) return "DocumentDisabled";
            return (id, state) switch
            {
                ("UserInfo", WizardPageState.Enabled) => "UserInfoConfigured",
                ("License" or "InfoBefore" or "InfoAfter", WizardPageState.Enabled) => "DocumentConfigured",
                _ => "PageReason" + id
            };
        }
        return pages.Select((page, index) => new WizardPageSummary(index + 1, page.Id,
            text["PageOverview" + page.Id], page.State, text["PageState" + page.State],
            text[ReasonKey(page.Id, page.State)], page.DesignIndex)).ToArray();
    }
}
