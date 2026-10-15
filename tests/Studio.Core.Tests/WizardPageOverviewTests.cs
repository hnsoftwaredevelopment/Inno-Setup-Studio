using System.Text.Json;
using Studio.Core;
namespace Studio.Core.Tests;

public class WizardPageOverviewTests
{
    [Fact]
    public void OverviewFollowsNativeOrderAndDistinguishesCurrentVisibility()
    {
        var rows = WizardPageOverview.Create(new StudioLocalizer());
        Assert.Equal(new[] { "Welcome", "License", "Password", "InfoBefore", "UserInfo", "Destination", "Components",
            "ProgramGroup", "Tasks", "Ready", "Preparing", "Installing", "InfoAfter", "Finished" }, rows.Select(r => r.Id));
        Assert.Equal(new[] { "Welcome", "Destination", "Ready", "Finished" }, rows.Where(r => r.State == WizardPageState.Enabled).Select(r => r.Id));
        Assert.Equal(WizardPageState.Conditional, rows.Single(r => r.Id == "Preparing").State);
        Assert.Equal(WizardPageState.Runtime, rows.Single(r => r.Id == "Installing").State);
        Assert.Equal(8, rows.Count(r => r.State == WizardPageState.Disabled));
        Assert.Equal(Enumerable.Range(1, 14), rows.Select(r => r.Order));
    }

    [Fact]
    public void CurrentStatusesMatchNativeExportSettings()
    {
        var project = StudioProject.CreateNew();
        project.InstallFile.Source = typeof(WizardPageOverviewTests).Assembly.Location;
        var script = InnoScript.Generate(project, null);
        Assert.Contains("DisableWelcomePage=no", script);
        Assert.Contains("DisableDirPage=no", script);
        foreach (var setting in new[] { "LicenseFile=", "Password=", "InfoBeforeFile=", "UserInfoPage=yes",
            "InfoAfterFile=", "DisableReadyPage=yes", "DisableFinishedPage=yes", "[Components]", "[Tasks]", "[Icons]" })
            Assert.DoesNotContain(setting, script);
    }

    [Theory]
    [InlineData("nl")]
    [InlineData("en")]
    [InlineData("de")]
    public void OverviewContainsTranslatedReasonsAndOnlyExistingDesignTargets(string language)
    {
        var rows = WizardPageOverview.Create(new StudioLocalizer(language));
        Assert.All(rows, r => { Assert.False(string.IsNullOrWhiteSpace(r.Name)); Assert.False(string.IsNullOrWhiteSpace(r.Reason)); Assert.DoesNotContain("PageOverview", r.Reason); });
        Assert.Equal(new int?[] { 1, 2 }, rows.Where(r => r.CanDesign).Select(r => r.DesignIndex));
        Assert.All(rows.Where(r => !r.CanDesign), r => Assert.Null(r.DesignIndex));
    }

    [Fact]
    public void InspectingOverviewDoesNotModifyProjectOrSelection()
    {
        using var editor = new EditorSession(StudioProject.CreateNew());
        editor.PageIndex = 2;
        editor.SelectedElement = ElementKind.Browse;
        var before = JsonSerializer.Serialize(editor.Project);
        _ = WizardPageOverview.Create(editor.Text);
        Assert.Equal(before, JsonSerializer.Serialize(editor.Project));
        Assert.Equal(2, editor.PageIndex);
        Assert.Equal(ElementKind.Browse, editor.SelectedElement);
        Assert.False(editor.IsDirty);
        Assert.Equal("english", editor.PreviewLanguageId);
    }
}
