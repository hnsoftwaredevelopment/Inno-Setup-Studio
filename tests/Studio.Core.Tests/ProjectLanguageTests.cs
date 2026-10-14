using System.Text.Json.Nodes;
using Studio.Core;

namespace Studio.Core.Tests;

public class ProjectLanguageTests
{
    [Fact]
    public void NativeMessageFormattingDoesNotInterpretProductNameAsMessageSyntax()
    {
        var project = StudioProject.CreateNew();
        project.Name = "Rock & Roll %n [name/ver] %1";
        Assert.Equal("Setup - " + project.Name, InstallerLanguage.English().Message("SetupWindowTitle", project));
        Assert.Equal("Welcome to the " + project.Name + " Setup Wizard", InstallerLanguage.English().Message("WelcomeLabel1", project));
    }
    [Fact]
    public void ExportUsesAllEnabledLanguagesIncludingDutchWithoutOriginalEnglish()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        try
        {
            File.WriteAllText(path, "payload");
            var project = StudioProject.CreateNew();
            project.InstallFile.Source = path;
            Assert.Contains("Name: \"english\"; MessagesFile: \"compiler:Default.isl\"", InnoScript.Generate(project, null));
            using var editor = new EditorSession(project);
            editor.AddLanguage(InstallerLanguage.Dutch());
            Assert.Contains("Name: \"dutch\"; MessagesFile: \"compiler:Languages\\Dutch.isl\"", InnoScript.Generate(project, null));
            editor.SetLanguageEnabled("english", false);
            var dutch = InnoScript.Generate(project, null);
            Assert.Contains("Name: \"dutch\";", dutch);
            Assert.DoesNotContain("Name: \"english\";", dutch);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("duplicate")]
    [InlineData("disabled")]
    [InlineData("null")]
    public async Task InvalidLanguageStateCannotReplaceSavedProject(string kind)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            var project = StudioProject.CreateNew();
            await ProjectFile.SaveAsync(path, project);
            var before = await File.ReadAllBytesAsync(path);
            switch (kind)
            {
                case "missing": project.TextLanguageId = "absent"; break;
                case "duplicate": project.Languages.Add(InstallerLanguage.English()); break;
                case "disabled": project.Languages[0].Enabled = false; break;
                case "null": project.Languages[0].Messages = null!; break;
            }
            var error = await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.SaveAsync(path, project));
            Assert.Equal("InvalidProjectLanguages", ProjectFile.ErrorResourceKey(error));
            Assert.Equal(before, await File.ReadAllBytesAsync(path));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Languages")]
    [InlineData("TextLanguageId")]
    public async Task CurrentFormatRequiresExplicitLanguageFields(string field)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            await ProjectFile.SaveAsync(path, StudioProject.CreateNew());
            var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            json.Remove(field);
            await File.WriteAllTextAsync(path, json.ToJsonString());
            var error = await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.LoadAsync(path));
            Assert.Equal("InvalidProjectLanguages", ProjectFile.ErrorResourceKey(error));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task ImportedMessagesRemainAvailableAndUnchangedAfterReopeningWithoutSource()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-language-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        var sourcePath = Path.Combine(folder, "Custom.isl");
        var projectPath = Path.Combine(folder, "Project.issstudio");
        try
        {
            var content = "[LangOptions]\nLanguageName=Custom\nLanguageID=0\n[Messages]\nWelcomeLabel1=Hello [name]\nWelcomeLabel2=One%nTwo\nButtonNext=&Continue >\n";
            await File.WriteAllTextAsync(sourcePath, content);
            var source = await LanguageSourceFile.LoadAsync(sourcePath);
            var language = InstallerLanguage.FromSource(source, null);
            var project = StudioProject.CreateNew();
            using var editor = new EditorSession(project);
            editor.AddLanguage(language);
            editor.AddLanguage(InstallerLanguage.FromSource(source, null));
            Assert.Equal(2, project.Languages.Count);
            await ProjectFile.SaveAsync(projectPath, project);
            Assert.Equal(content, await File.ReadAllTextAsync(sourcePath));
            File.Delete(sourcePath);
            var restored = await ProjectFile.LoadAsync(projectPath);
            using var reopened = new EditorSession(restored);
            reopened.PreviewLanguageId = language.Id;
            reopened.PageIndex = 1;
            Assert.Equal("Hello My application", reopened.PreviewTitle);
            Assert.Equal("One\nTwo", reopened.PreviewBody);
            Assert.Equal("Continue >", reopened.NextCaption);
            Assert.True(reopened.CanEdit);
            Assert.False(reopened.IsDirty);
        }
        finally { Directory.Delete(folder, true); }
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task NewEnglishProjectCompilesWithNativeDefaultLanguage()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-English-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            var project = StudioProject.CreateNew();
            project.InstallFile.Source = Path.Combine(folder, "payload.txt");
            await File.WriteAllTextAsync(project.InstallFile.Source, "payload");
            var result = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath,
                InnoScript.Generate(project, folder), folder, project.OutputBaseFileName);
            Assert.NotNull(result.InstallerPath);
            Assert.DoesNotContain(result.Messages, m => m.Severity == CompilerMessageSeverity.Error);
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public void NewProjectsStartEnglishRegardlessOfStudioLanguage()
    {
        var project = StudioProject.CreateNew();
        using var editor = new EditorSession(project, new StudioLocalizer("de"));
        editor.PageIndex = 1;
        Assert.Equal("english", project.TextLanguageId);
        Assert.Contains("Welcome", editor.PreviewTitle);
        Assert.False(editor.IsDirty);
        var choices = editor.PreviewLanguages;
        editor.Zoom = 0.8;
        Assert.Same(choices, editor.PreviewLanguages);
        editor.Text.Language = "nl";
        Assert.Same(choices, editor.PreviewLanguages);
        Assert.Contains("Welcome", editor.PreviewTitle);
    }

    [Fact]
    public async Task FormatFourMigrationKeepsDutchTextOutputAndIdentity()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            var original = StudioProject.CreateExample();
            original.OutputBaseFileName = "viewer-install";
            original.Pages[0].Title = "Mijn eigen welkom";
            await ProjectFile.SaveAsync(path, original);
            var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            json["Version"] = 4;
            json.Remove("Documents"); json.Remove("PageSettings");
            json.Remove("Languages");
            json.Remove("TextLanguageId");
            await File.WriteAllTextAsync(path, json.ToJsonString());
            var migrated = await ProjectFile.LoadAsync(path);
            Assert.Equal("dutch", migrated.TextLanguageId);
            Assert.Equal("Mijn eigen welkom", migrated.Pages[0].Title);
            Assert.Equal("viewer-install", migrated.OutputBaseFileName);
            Assert.Equal(original.AppId, migrated.AppId);
            Assert.True(migrated.WasMigrated);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task EnabledLanguagesAndOrderRoundTripWithoutSavingPreviewSelection()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            var project = StudioProject.CreateNew();
            using var editor = new EditorSession(project);
            editor.AddLanguage(InstallerLanguage.Dutch());
            editor.MoveLanguage("dutch", -1);
            editor.PreviewLanguageId = "english";
            await ProjectFile.SaveAsync(path, project);
            var loaded = await ProjectFile.LoadAsync(path);
            Assert.Equal(new[] { "dutch", "english" }, loaded.Languages.Select(l => l.Id));
            Assert.Equal("english", loaded.TextLanguageId);
            using var reopened = new EditorSession(loaded);
            Assert.Equal("dutch", reopened.PreviewLanguageId);
            reopened.SetLanguageEnabled("dutch", false);
            Assert.Equal("english", reopened.PreviewLanguageId);
            Assert.Equal(2, loaded.Languages.Count);
            reopened.SetLanguageEnabled("english", false);
            Assert.True(loaded.Languages.Single(l => l.Id == "english").Enabled);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void PreviewSwitchPreservesTrialDirectoryAndDoesNotEditDutchTexts()
    {
        var project = StudioProject.CreateExample();
        using var editor = new EditorSession(project);
        editor.AddLanguage(InstallerLanguage.English());
        editor.MarkSaved();
        editor.PageIndex = 2;
        editor.IsPreview = true;
        editor.TrialDirectory = @"C:\Trial";
        editor.PreviewLanguageId = "english";
        Assert.Equal(@"C:\Trial", editor.DirectoryText);
        Assert.Equal(project.Languages[1].Message("WizardSelectDir", project), editor.PreviewTitle);
        Assert.True(editor.SelectedTextMissing);
        Assert.False(editor.IsDirty);
        editor.Text.Language = "de";
        Assert.Equal("english", editor.PreviewLanguageId);
        editor.IsPreview = false;
        Assert.True(editor.CanEdit);
        editor.PropertyText = "Must not overwrite";
        Assert.Equal("Kies de installatiemap", project.Pages[1].Title);
        editor.PreviewLanguageId = "dutch";
        Assert.True(editor.CanEdit);
    }
}
