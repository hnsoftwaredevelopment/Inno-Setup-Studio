using System.Text.Json.Nodes;
using Studio.Core;

namespace Studio.Core.Tests;

public class TranslationTests
{
    [Theory]
    [InlineData("&")]
    [InlineData("%n%n")]
    public void FormattingOnlyNativeMessagesUseVisibleFallback(string value)
    {
        using var editor = Bilingual();
        editor.Project.Languages[0].Messages["WelcomeLabel1"] = value;
        editor.Project.Languages[0].Messages["SetupWindowTitle"] = value;
        Assert.Contains("Welcome", editor.PreviewTitle);
        Assert.Contains("Setup", editor.PreviewWindowTitle);
    }

    [Fact]
    public void CopyingEffectiveTextIsExplicitAndPreviewCannotChangeTranslations()
    {
        using var editor = Bilingual();
        editor.PropertyText = "Hello {AppName}";
        editor.PreviewLanguageId = "dutch";
        var native = editor.Project.Languages[1].Message("WelcomeLabel1", editor.Project);
        editor.UseEffectiveText();
        Assert.Equal(native, editor.PropertyText);
        Assert.Equal(0, editor.MissingTranslationCount);
        editor.MarkSaved();
        editor.UseEffectiveText();
        Assert.False(editor.IsDirty);
        var choices = editor.PreviewLanguages;
        editor.IsPreview = true;
        editor.PropertyText = "Overwrite";
        editor.ResetProperty();
        editor.UseEffectiveText();
        editor.Text.Language = "de";
        editor.PreviewLanguageId = "english";
        Assert.Same(choices, editor.PreviewLanguages);
        Assert.False(editor.IsDirty);
        Assert.Equal("Hello {AppName}", editor.PropertyText);
    }

    [Fact]
    public void TooltipHasIndependentFallbackAndReset()
    {
        using var editor = Bilingual();
        editor.PageIndex = 2;
        editor.SelectedElement = ElementKind.Browse;
        editor.BrowseTooltip = "Choose a folder for {AppName}";
        editor.PreviewLanguageId = "dutch";
        Assert.Equal("Choose a folder for My application", editor.EffectiveBrowseTooltip);
        Assert.Equal("", editor.BrowseTooltip);
        Assert.True(editor.ShowTooltipWatermark);
        Assert.True(editor.ShowCopyTooltip);
        Assert.False(editor.ShowResetTooltip);
        editor.UseEffectiveTooltip();
        Assert.Equal("Choose a folder for {AppName}", editor.BrowseTooltip);
        Assert.False(editor.ShowCopyTooltip);
        Assert.True(editor.ShowResetTooltip);
        editor.BrowseTooltip = "Kies een map voor {AppName}";
        editor.PropertyText = "Kies...";
        editor.ResetTooltip();
        Assert.False(editor.CanResetTooltip);
        Assert.True(editor.ShowCopyTooltip);
        Assert.Equal("Choose a folder for My application", editor.EffectiveBrowseTooltip);
        Assert.Equal("Kies...", editor.BrowseCaption);
        editor.PreviewLanguageId = "english";
        editor.ResetTooltip();
        Assert.Equal("", editor.EffectiveBrowseTooltip);
        Assert.Equal(0, editor.MissingTranslationCount);
    }

    [Theory]
    [InlineData("TextOverrides")]
    [InlineData("StandardTexts")]
    public async Task CurrentFormatRequiresTranslationState(string field)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            await ProjectFile.SaveAsync(path, StudioProject.CreateNew());
            var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            json["Languages"]![0]!.AsObject().Remove(field);
            await File.WriteAllTextAsync(path, json.ToJsonString());
            var error = await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.LoadAsync(path));
            Assert.Equal("InvalidProjectLanguages", ProjectFile.ErrorResourceKey(error));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task DisabledTranslationsAreSavedAndRetainedAfterReopening()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            using var editor = Bilingual();
            editor.PreviewLanguageId = "dutch";
            editor.PropertyText = "Hallo";
            editor.SetLanguageEnabled("dutch", false);
            await ProjectFile.SaveAsync(path, editor.Project);
            using var restored = new EditorSession(await ProjectFile.LoadAsync(path));
            restored.SetLanguageEnabled("dutch", true);
            restored.PreviewLanguageId = "dutch";
            restored.PageIndex = 1;
            restored.SelectedElement = ElementKind.Title;
            Assert.Equal("Hallo", restored.PropertyText);
        }
        finally { File.Delete(path); }
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task ReferencesAndEmptyRequiredTextCompileWithTheSameResolvedValuesAsPreview()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-texts-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            using var editor = new EditorSession(StudioProject.CreateNew()) { PageIndex = 1, SelectedElement = ElementKind.Title };
            editor.ProductName = "O'Brien & {AppVersion}";
            editor.ApplicationVersion = "2.3";
            editor.PropertyText = "Install {AppName} {AppVersion}";
            editor.SelectedElement = ElementKind.Body;
            editor.PropertyText = " ";
            editor.Project.InstallFile.Source = Path.Combine(folder, "payload.txt");
            await File.WriteAllTextAsync(editor.Project.InstallFile.Source, "payload");
            var script = InnoScript.Generate(editor.Project, folder);
            Assert.Equal("Install O'Brien & {AppVersion} 2.3", editor.PreviewTitle);
            Assert.Contains("'Install O''Brien & {AppVersion} 2.3'", script);
            Assert.False(string.IsNullOrWhiteSpace(editor.PreviewBody));
            Assert.DoesNotContain("WizardForm.WelcomeLabel2.Caption := ' ';", script);
            var result = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath, script, folder);
            Assert.NotNull(result.InstallerPath);
            Assert.DoesNotContain(result.Messages, m => m.Severity == CompilerMessageSeverity.Error);
        }
        finally { Directory.Delete(folder, true); }
    }

    private static EditorSession Bilingual(bool legacy = false)
    {
        var editor = new EditorSession(legacy ? StudioProject.CreateExample() : StudioProject.CreateNew());
        editor.AddLanguage(legacy ? InstallerLanguage.English() : InstallerLanguage.Dutch());
        editor.PageIndex = 1;
        editor.SelectedElement = ElementKind.Title;
        editor.MarkSaved();
        return editor;
    }

    [Fact]
    public void NativeTextsNeedNoTranslationButCustomDefaultTextDoes()
    {
        using var editor = Bilingual();
        Assert.Equal(0, editor.MissingTranslationCount);
        editor.PropertyText = "Hello {AppName}";
        editor.PreviewLanguageId = "dutch";
        var native = editor.Project.Languages[1].Message("WelcomeLabel1", editor.Project);
        Assert.Equal(native, editor.PreviewTitle);
        Assert.True(editor.SelectedTextMissing);
        Assert.Equal(1, editor.MissingTranslationCount);
        Assert.Equal("", editor.PropertyText);
        editor.PropertyText = "Hallo {AppName}";
        Assert.Equal("Hallo My application", editor.PreviewTitle);
        Assert.False(editor.SelectedTextMissing);
        Assert.Equal(0, editor.MissingTranslationCount);
        editor.PropertyText = "  ";
        Assert.Equal("  ", editor.PropertyText);
        Assert.Equal(native, editor.PreviewTitle);
        Assert.True(editor.SelectedTextMissing);
        editor.ResetProperty();
        Assert.False(editor.CanReset);
        Assert.Equal(native, editor.PreviewTitle);
    }

    [Fact]
    public void MissingNativeTextUsesDefaultCustomThenDefaultNativeThenBuiltIn()
    {
        using var editor = Bilingual();
        editor.PropertyText = "Hello {AppName}";
        editor.PreviewLanguageId = "dutch";
        editor.Project.Languages[1].Messages.Remove("WelcomeLabel1");
        Assert.Equal("Hello My application", editor.PreviewTitle);
        Assert.True(editor.SelectedTextMissing);
        editor.UseEffectiveText();
        Assert.Equal("Hello {AppName}", editor.PropertyText);
        editor.ResetProperty();
        editor.PreviewLanguageId = "english";
        editor.ResetProperty();
        editor.PreviewLanguageId = "dutch";
        Assert.Equal(editor.Project.Languages[0].Message("WelcomeLabel1", editor.Project), editor.PreviewTitle);
        editor.Project.Languages[0].Messages.Remove("WelcomeLabel1");
        Assert.Equal(InstallerLanguage.English().Message("WelcomeLabel1", editor.Project), editor.PreviewTitle);
    }

    [Fact]
    public void CopyAndResetActionsAreExclusiveAndNeverReplaceExistingOwnText()
    {
        using var editor = Bilingual();
        Assert.True(editor.ShowTextWatermark);
        Assert.True(editor.ShowCopyText);
        Assert.False(editor.ShowResetText);
        var native = editor.EffectiveText;
        editor.UseEffectiveText();
        Assert.Equal(native, editor.PropertyText);
        Assert.False(editor.ShowTextWatermark);
        Assert.False(editor.ShowCopyText);
        Assert.True(editor.ShowResetText);
        editor.PropertyText = "My own title";
        editor.UseEffectiveText();
        Assert.Equal("My own title", editor.PropertyText);
        editor.ResetProperty();
        Assert.Equal("", editor.PropertyText);
        Assert.True(editor.ShowTextWatermark);
        Assert.True(editor.ShowCopyText);
        Assert.False(editor.ShowResetText);
        editor.PageIndex = 2;
        editor.SelectedElement = ElementKind.Directory;
        Assert.False(editor.ShowTextWatermark);
        Assert.False(editor.ShowCopyText);
        Assert.False(editor.ShowResetText);
    }

    [Fact]
    public void ReorderingChangesDefaultWithoutCopyingTranslationsAndDisablingPreservesThem()
    {
        using var editor = Bilingual();
        editor.PropertyText = "English";
        editor.PreviewLanguageId = "dutch";
        editor.PropertyText = "Nederlands";
        editor.MoveLanguage("dutch", -1);
        Assert.Equal("dutch", editor.DefaultLanguageId);
        Assert.Equal("Nederlands", editor.PreviewTitle);
        editor.SetLanguageEnabled("dutch", false);
        Assert.Equal("english", editor.DefaultLanguageId);
        Assert.Equal("English", editor.PreviewTitle);
        editor.SetLanguageEnabled("dutch", true);
        editor.PreviewLanguageId = "dutch";
        Assert.Equal("Nederlands", editor.PropertyText);
    }

    [Fact]
    public void SharedAndLocalTranslationsResetOnlySelectedScopeAndLanguage()
    {
        using var editor = Bilingual();
        editor.PageIndex = 0;
        editor.SelectedElement = ElementKind.Next;
        editor.PropertyText = "Proceed";
        Assert.Equal(1, editor.MissingTranslationCount);
        editor.PreviewLanguageId = "dutch";
        editor.PropertyText = "Doorgaan";
        editor.PageIndex = 1;
        Assert.Equal("Doorgaan", editor.NextCaption);
        Assert.False(editor.CanReset);
        editor.PropertyText = "Beginnen";
        Assert.Equal("Beginnen", editor.NextCaption);
        editor.ResetProperty();
        Assert.Equal("Doorgaan", editor.NextCaption);
        editor.PageIndex = 2;
        Assert.Equal("Doorgaan", editor.NextCaption);
        editor.PreviewLanguageId = "english";
        Assert.Equal("Proceed", editor.NextCaption);
        editor.PageIndex = 0;
        editor.ResetProperty();
        Assert.Equal("Next >", editor.NextCaption);
        editor.PreviewLanguageId = "dutch";
        Assert.Equal("Doorgaan", editor.NextCaption);
    }

    [Fact]
    public void BlankDefaultAndMissingNativeSourcesCannotEraseRequiredText()
    {
        using var editor = Bilingual();
        editor.PropertyText = "";
        editor.Project.Languages[0].Messages.Clear();
        editor.Project.Languages[1].Messages.Clear();
        Assert.False(string.IsNullOrWhiteSpace(editor.PreviewTitle));
        editor.PreviewLanguageId = "dutch";
        Assert.False(string.IsNullOrWhiteSpace(editor.PreviewTitle));
        Assert.True(editor.MissingTranslationCount > 0);
    }

    [Fact]
    public void ReferencesExpandOnceWithoutInterpretingProductValuesAsSyntax()
    {
        using var editor = Bilingual();
        editor.PropertyText = "{AppName} / {AppVersion} / {unknown}";
        editor.ProductName = "O'Brien & {AppVersion}";
        editor.ApplicationVersion = "2.0";
        Assert.Equal("O'Brien & {AppVersion} / 2.0 / {unknown}", editor.PreviewTitle);
        Assert.Equal("{AppName} / {AppVersion} / {unknown}", editor.PropertyText);
    }

    [Fact]
    public async Task TranslationsAndRestoredNativeFieldsRoundTrip()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            using var editor = Bilingual();
            editor.PropertyText = "English custom";
            editor.ResetProperty();
            editor.PreviewLanguageId = "dutch";
            editor.PropertyText = "Nederlandse titel";
            await ProjectFile.SaveAsync(path, editor.Project);
            using var reopened = new EditorSession(await ProjectFile.LoadAsync(path));
            reopened.PageIndex = 1;
            reopened.SelectedElement = ElementKind.Title;
            Assert.Contains("Welcome", reopened.PreviewTitle);
            reopened.PreviewLanguageId = "dutch";
            Assert.Equal("Nederlandse titel", reopened.PropertyText);
            Assert.Equal(0, reopened.MissingTranslationCount);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task FormatFiveMigrationPreservesLanguagesAndLegacyFieldsExactly()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            using var editor = Bilingual(true);
            editor.Project.OutputBaseFileName = "custom-installer";
            editor.MoveLanguage("english", -1);
            await ProjectFile.SaveAsync(path, editor.Project);
            var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            json["Version"] = 5;
            json.AsObject().Remove("Documents"); json.AsObject().Remove("PageSettings");
            foreach (var language in json["Languages"]!.AsArray())
            { language!.AsObject().Remove("TextOverrides"); language.AsObject().Remove("StandardTexts"); }
            var content = json.ToJsonString();
            await File.WriteAllTextAsync(path, content);
            var restored = await ProjectFile.LoadAsync(path);
            Assert.True(restored.WasMigrated);
            Assert.Equal("dutch", restored.TextLanguageId);
            Assert.Equal(new[] { "english", "dutch" }, restored.Languages.Select(l => l.Id));
            Assert.Equal("custom-installer", restored.OutputBaseFileName);
            Assert.Equal(editor.Project.Pages[0].Title, restored.Pages[0].Title);
            Assert.Equal(content, await File.ReadAllTextAsync(path));
        }
        finally { File.Delete(path); }
    }
}
