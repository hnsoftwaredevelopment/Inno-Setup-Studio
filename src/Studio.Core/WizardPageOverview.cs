namespace Studio.Core;

public enum WizardPageState { Enabled, Disabled, Conditional, Runtime }

public sealed record WizardPageSummary(int Order, string Id, string Name, WizardPageState State,
    string Status, string Reason, int? DesignIndex)
{
    public bool CanDesign => DesignIndex is not null;
}

/// <summary>The regular native flow for the current export. Not a list of editable project pages.</summary>
public static class WizardPageOverview
{
    public static IReadOnlyList<WizardPageSummary> Create(StudioLocalizer text)
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
        return pages.Select((page, index) => new WizardPageSummary(index + 1, page.Id,
            text["PageOverview" + page.Id], page.State, text["PageState" + page.State],
            text["PageReason" + page.Id], page.DesignIndex)).ToArray();
    }
}
