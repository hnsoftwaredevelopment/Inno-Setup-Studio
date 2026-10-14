namespace Studio.Core;

public enum WizardPageState { Enabled, Disabled, Conditional, Runtime }

public sealed record WizardPageSummary(int Order, string Id, string Name, WizardPageState State,
    string Status, string Reason, int? DesignIndex)
{
    public WizardDocumentKind? DocumentKind => Id switch { "License" => WizardDocumentKind.LicenseFile, "InfoBefore" => WizardDocumentKind.InfoBeforeFile, "InfoAfter" => WizardDocumentKind.InfoAfterFile, _ => null };
    public bool CanDesign => DesignIndex is not null;
}

/// <summary>The regular native flow for the current export. Not a list of editable project pages.</summary>
public static class WizardPageOverview
{
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
                var kind = pages[i].Id switch { "License" => (WizardDocumentKind?)WizardDocumentKind.LicenseFile,
                    "InfoBefore" => WizardDocumentKind.InfoBeforeFile, "InfoAfter" => WizardDocumentKind.InfoAfterFile, _ => null };
                if (kind is { } document && !string.IsNullOrWhiteSpace(project.Documents[document].Source))
                    pages[i].State = WizardPageState.Enabled;
            }
        return pages.Select((page, index) => new WizardPageSummary(index + 1, page.Id,
            text["PageOverview" + page.Id], page.State, text["PageState" + page.State],
            text[page.State == WizardPageState.Enabled && page.Id is "License" or "InfoBefore" or "InfoAfter" ? "DocumentConfigured" : "PageReason" + page.Id], page.DesignIndex)).ToArray();
    }
}
