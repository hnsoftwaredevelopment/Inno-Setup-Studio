using System.Text.Json;
using Studio.Core;

namespace Studio.Core.Tests;

public class TranslationOverviewTests
{
    private static EditorSession Bilingual()
    {
        var editor = new EditorSession(StudioProject.CreateNew());
        editor.AddLanguage(InstallerLanguage.Dutch());
        editor.MarkSaved();
        return editor;
    }

    [Fact]
    public void OverviewEditsTwoLanguagesWithoutChangingPreviewSelection()
    {
        using var editor = Bilingual();
        editor.PageIndex = 2;
        editor.SelectedElement = ElementKind.Browse;
        using var overview = new TranslationOverview(editor);
        var row = overview.Rows.Single(r => r.Slot == WizardTextSlot.WelcomeTitle);
        row.Cells[0].Text = "English title";
        row.Cells[1].Text = "Nederlandse titel";
        Assert.Equal("english", editor.PreviewLanguageId);
        Assert.Equal(2, editor.PageIndex);
        Assert.Equal(ElementKind.Browse, editor.SelectedElement);
        Assert.Equal("English title", WizardTexts.Resolve(editor.Project, "english", row.Slot).Text);
        Assert.Equal("Nederlandse titel", WizardTexts.Resolve(editor.Project, "dutch", row.Slot).Text);
        Assert.True(editor.IsDirty);
    }

    [Fact]
    public void CopyResetAndSharedScopeUseTheExistingResolver()
    {
        using var editor = Bilingual();
        using var overview = new TranslationOverview(editor);
        var shared = overview.Rows.Single(r => r.Slot == WizardTextSlot.SharedNext).Cells[1];
        var local = overview.Rows.Single(r => r.Slot == WizardTextSlot.WelcomeNext).Cells[1];
        shared.Text = "Doorgaan";
        Assert.Equal("Doorgaan", local.EffectiveText);
        Assert.True(local.ShowCopy);
        local.Copy();
        Assert.Equal("Doorgaan", local.Text);
        Assert.True(local.ShowReset);
        shared.Text = "Verder";
        Assert.Equal("Doorgaan", local.EffectiveText);
        local.Reset();
        Assert.Equal("", local.Text);
        Assert.Equal("Verder", local.EffectiveText);
        Assert.True(local.ShowCopy);
        Assert.False(local.ShowReset);
    }

    [Fact]
    public void DisabledLanguageRemainsEditableButItsIssuesAreExcludedFromActiveSummary()
    {
        using var editor = Bilingual();
        editor.SetLanguageEnabled("dutch", false);
        using var overview = new TranslationOverview(editor);
        var cell = overview.Rows[0].Cells[1];
        cell.Text = "{AppNmae}";
        Assert.True(cell.CanEdit);
        Assert.NotEmpty(cell.Issues);
        Assert.Equal(0, overview.ActiveIssueCount);
        editor.SetLanguageEnabled("dutch", true);
        Assert.Equal(1, overview.ActiveIssueCount);
        Assert.Equal("{AppNmae}", cell.Text);
    }

    [Fact]
    public void OverviewObservesMainEditorChangesAndLocalizationDoesNotDirtyTheProject()
    {
        using var editor = Bilingual();
        using var overview = new TranslationOverview(editor);
        var rows = overview.Rows;
        editor.PageIndex = 1;
        editor.SelectedElement = ElementKind.Title;
        editor.PropertyText = "Title";
        var cell = rows.Single(r => r.Slot == WizardTextSlot.WelcomeTitle).Cells[0];
        Assert.Equal("Title", cell.Text);
        editor.MarkSaved();
        var snapshot = JsonSerializer.Serialize(editor.Project);
        editor.Text.Language = "de";
        Assert.Same(rows, overview.Rows);
        Assert.Contains("Willkommen", rows.Single(r => r.Slot == WizardTextSlot.WelcomeTitle).Name);
        Assert.Equal(snapshot, JsonSerializer.Serialize(editor.Project));
        Assert.False(editor.IsDirty);
    }

    [Theory]
    [InlineData("{AppName} / {AppVersion}", 0)]
    [InlineData("{AppNmae}", 1)]
    [InlineData("{appname}", 1)]
    [InlineData("{AppName", 1)]
    [InlineData("AppName}", 1)]
    [InlineData("{AppName} {AppName}", 0)]
    public void ReferencesReportUnknownOrMalformedTokensWithoutChangingText(string value, int count)
    {
        using var editor = Bilingual();
        editor.SetTranslation("english", WizardTextSlot.WelcomeTitle, value);
        Assert.Equal(count, TranslationValidation.Check(editor.Project, "english", WizardTextSlot.WelcomeTitle).Count);
        Assert.Equal(value, WizardTexts.Own(editor.Project, editor.Project.Languages[0], WizardTextSlot.WelcomeTitle));
    }

    [Fact]
    public void MissingReferenceUsesTheApplicableDefaultScopeAndNeverInspectsProductValues()
    {
        using var editor = Bilingual();
        editor.ProductName = "literal {unknown}";
        editor.SetTranslation("english", WizardTextSlot.SharedNext, "Continue {AppName} {AppVersion}");
        editor.SetTranslation("dutch", WizardTextSlot.WelcomeNext, "Verder {AppName}");
        var issues = TranslationValidation.Check(editor.Project, "dutch", WizardTextSlot.WelcomeNext);
        Assert.Equal("TranslationReferenceMissing", Assert.Single(issues).ResourceKey);
        Assert.Equal("{AppVersion}", issues[0].Argument);
        editor.SetTranslation("dutch", WizardTextSlot.WelcomeNext, "Verder {AppName} {AppVersion}");
        Assert.Empty(TranslationValidation.Check(editor.Project, "dutch", WizardTextSlot.WelcomeNext));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemovingStorageLanguagePreservesRemainingOwnAndNativeTexts(bool legacy)
    {
        using var editor = new EditorSession(legacy ? StudioProject.CreateExample() : StudioProject.CreateNew());
        var removedId = editor.Project.TextLanguageId;
        var remaining = legacy ? InstallerLanguage.English() : InstallerLanguage.Dutch();
        editor.AddLanguage(remaining);
        editor.SetTranslation(remaining.Id, WizardTextSlot.WelcomeTitle, "Kept {AppName}");
        editor.SetTranslation(remaining.Id, WizardTextSlot.DestinationBody, "");
        editor.SetTranslation(remaining.Id, WizardTextSlot.SharedNext, "Shared");
        editor.SetTranslation(remaining.Id, WizardTextSlot.DestinationNext, "Local");
        var before = Enum.GetValues<WizardTextSlot>().ToDictionary(s => s, s => WizardTexts.Own(editor.Project, remaining, s));
        Assert.True(editor.RemoveLanguage(removedId));
        Assert.Equal(remaining.Id, editor.Project.TextLanguageId);
        Assert.Equal(remaining.Id, editor.PreviewLanguageId);
        foreach (var slot in before.Keys) Assert.Equal(before[slot], WizardTexts.Own(editor.Project, remaining, slot));
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            await ProjectFile.SaveAsync(path, editor.Project);
            var restored = await ProjectFile.LoadAsync(path);
            foreach (var slot in before.Keys) Assert.Equal(before[slot], WizardTexts.Own(restored, restored.Languages[0], slot));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void LastEnabledLanguageAndPreviewModeCannotBeRemovedOrEdited()
    {
        using var editor = Bilingual();
        editor.SetLanguageEnabled("dutch", false);
        Assert.False(editor.RemoveLanguage("english"));
        Assert.True(editor.RemoveLanguage("dutch"));
        editor.MarkSaved();
        editor.IsPreview = true;
        using var overview = new TranslationOverview(editor);
        overview.Rows[0].Cells[0].Text = "Disallowed";
        Assert.False(editor.RemoveLanguage("english"));
        Assert.False(editor.IsDirty);
        Assert.Equal("", overview.Rows[0].Cells[0].Text);
    }

    [Theory]
    [InlineData("nl")]
    [InlineData("en")]
    [InlineData("de")]
    public void ReferenceMessagesAreLocalizedAndShownByBothEditors(string studioLanguage)
    {
        using var editor = Bilingual();
        editor.Text.Language = studioLanguage;
        editor.PageIndex = 1;
        editor.SelectedElement = ElementKind.Title;
        editor.PropertyText = "{AppNmae}";
        using var overview = new TranslationOverview(editor);
        var cell = overview.Rows.Single(row => row.Slot == WizardTextSlot.WelcomeTitle).Cells[0];
        Assert.True(editor.HasSelectedReferenceWarnings);
        Assert.Equal(editor.SelectedReferenceWarnings, cell.Warnings);
        Assert.Contains("{AppNmae}", cell.Warnings);
        Assert.Contains("{AppName}", cell.Warnings);
        Assert.DoesNotContain("[Translation", cell.Warnings);
        Assert.Contains("1", overview.Summary);
        editor.PropertyText = "{AppName}";
        Assert.False(editor.HasSelectedReferenceWarnings);
        Assert.False(cell.HasWarnings);
    }

    [Fact]
    public async Task DisablingSavingAndReopeningKeepsEveryTranslationIncludingLineBreaks()
    {
        using var editor = Bilingual();
        using var overview = new TranslationOverview(editor);
        var body = overview.Rows.Single(row => row.Slot == WizardTextSlot.WelcomeBody);
        body.Cells[0].Text = "One\nTwo {AppName}";
        body.Cells[1].Text = "Een\nTwee {AppName}";
        editor.SetLanguageEnabled("dutch", false);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            await ProjectFile.SaveAsync(path, editor.Project);
            using var restored = new EditorSession(await ProjectFile.LoadAsync(path));
            using var restoredOverview = new TranslationOverview(restored);
            var restoredBody = restoredOverview.Rows.Single(row => row.Slot == WizardTextSlot.WelcomeBody);
            Assert.Equal("One\nTwo {AppName}", restoredBody.Cells[0].Text);
            Assert.Equal("Een\nTwee {AppName}", restoredBody.Cells[1].Text);
            Assert.False(restoredBody.Cells[1].Enabled);
            Assert.True(restoredBody.Cells[1].CanEdit);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RemovingStorageLanguageDoesNotTouchDisabledThirdLanguage()
    {
        using var editor = Bilingual();
        var third = InstallerLanguage.English();
        third.Id = "third"; third.Source = "third.isl";
        editor.AddLanguage(third);
        editor.SetTranslation("third", WizardTextSlot.WelcomeTitle, "Third title");
        editor.SetTranslation("third", WizardTextSlot.WelcomeNext, "Third next");
        editor.SetLanguageEnabled("third", false);
        var snapshot = JsonSerializer.Serialize(third);
        Assert.True(editor.RemoveLanguage("english"));
        Assert.Equal(snapshot, JsonSerializer.Serialize(third));
        Assert.Equal("dutch", editor.Project.TextLanguageId);
    }

    [Fact]
    public void NativeTextAndInheritedSharedWarningsAreNotReportedAsInvalidOwnText()
    {
        using var editor = Bilingual();
        editor.Project.Languages[0].Messages["WelcomeLabel1"] = "Native {not a Studio reference}";
        Assert.Empty(TranslationValidation.Check(editor.Project, "english", WizardTextSlot.WelcomeTitle));
        editor.SetTranslation("english", WizardTextSlot.SharedNext, "{unknown}");
        using var overview = new TranslationOverview(editor);
        Assert.Equal(1, overview.ActiveIssueCount);
        Assert.Empty(overview.Rows.Single(row => row.Slot == WizardTextSlot.WelcomeNext).Cells[0].Issues);
    }
}
