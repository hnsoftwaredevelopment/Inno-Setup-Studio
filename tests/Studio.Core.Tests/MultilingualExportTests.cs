using System.Diagnostics;
using System.Text.Json;
using Studio.Core;

namespace Studio.Core.Tests;

public class MultilingualExportTests
{
    private static EditorSession Bilingual()
    {
        var editor = new EditorSession(StudioProject.CreateNew());
        editor.Project.InstallFile.Source = typeof(MultilingualExportTests).Assembly.Location;
        editor.AddLanguage(InstallerLanguage.Dutch());
        return editor;
    }

    [Fact]
    public void ActiveLanguageBranchesContainEachScopesResolvedTextAndKeepNativeButtonRoles()
    {
        using var editor = Bilingual();
        editor.SetTranslation("english", WizardTextSlot.WelcomeTitle, "English {AppName}");
        editor.SetTranslation("dutch", WizardTextSlot.WelcomeTitle, "Nederlands {AppName}");
        editor.SetTranslation("dutch", WizardTextSlot.SharedCancel, "Gedeeld");
        editor.SetTranslation("dutch", WizardTextSlot.WelcomeCancel, "Lokaal");
        editor.SetTranslation("dutch", WizardTextSlot.DestinationTooltip, "Map {AppVersion}");
        var script = InnoScript.Generate(editor.Project, null);
        Assert.Contains("if ActiveLanguage = 'english' then", script);
        Assert.Contains("if ActiveLanguage = 'dutch' then", script);
        Assert.Contains("'English My application'", script);
        Assert.Contains("'Nederlands My application'", script);
        Assert.Contains("'Gedeeld'", script);
        Assert.Contains("'Lokaal'", script);
        Assert.Contains("WizardForm.DirBrowseButton.Hint := 'Map 1.0';", script);
        Assert.DoesNotContain("wpReady", script);
        Assert.DoesNotContain("wpFinished", script);
        Assert.DoesNotContain("DirEdit.Text :=", script);
    }

    [Fact]
    public void OrderAndEnabledStateDriveExportButStudioAndPreviewSelectionDoNot()
    {
        using var editor = Bilingual();
        var projectSnapshot = JsonSerializer.Serialize(editor.Project);
        var initial = InnoScript.Generate(editor.Project, null);
        editor.PreviewLanguageId = "dutch";
        editor.Text.Language = "de";
        Assert.Equal(initial, InnoScript.Generate(editor.Project, null));
        Assert.Equal(projectSnapshot, JsonSerializer.Serialize(editor.Project));
        editor.MoveLanguage("dutch", -1);
        var ordered = InnoScript.Generate(editor.Project, null);
        Assert.True(ordered.IndexOf("Name: \"dutch\";", StringComparison.Ordinal) < ordered.IndexOf("Name: \"english\";", StringComparison.Ordinal));
        editor.SetLanguageEnabled("english", false);
        Assert.DoesNotContain("Name: \"english\";", InnoScript.Generate(editor.Project, null));
    }

    [Fact]
    public void NativeChosenLanguageWinsBeforeDefaultOwnTextAndMissingNativeUsesDefaultOwnText()
    {
        using var editor = Bilingual();
        editor.SetTranslation("english", WizardTextSlot.WelcomeTitle, "Default custom");
        var expectedNative = editor.Project.Languages[1].Message("WelcomeLabel1", editor.Project);
        Assert.Contains("'" + expectedNative + "'", InnoScript.Generate(editor.Project, null));
        editor.Project.Languages[1].Messages.Remove("WelcomeLabel1");
        var script = InnoScript.Generate(editor.Project, null);
        Assert.Equal(2, script.Split("WizardForm.WelcomeLabel1.Caption := 'Default custom';").Length - 1);
    }

    [Fact]
    public void ArbitraryIdsCannotBecomeLanguageOrPascalSyntax()
    {
        using var editor = Bilingual();
        var language = editor.Project.Languages[1];
        language.Id = "not valid'; begin {#bad}";
        var script = InnoScript.Generate(editor.Project, null);
        Assert.Contains("Name: \"studio_language_2\";", script);
        Assert.Contains("ActiveLanguage = 'studio_language_2'", script);
        Assert.DoesNotContain("not valid", script);
        Assert.DoesNotContain("{#", script);
    }

    [Theory]
    [InlineData("compiler:..\\Dutch.isl")]
    [InlineData("compiler:Languages\\Dutch.isl,compiler:Default.isl")]
    [InlineData("compiler:{#bad}.isl")]
    public void InvalidLanguageSourceCannotBecomeCompilerInput(string source)
    {
        using var editor = Bilingual();
        editor.Project.Languages[1].Source = source;
        var error = Assert.Throws<InvalidDataException>(() => InnoScript.Generate(editor.Project, null));
        Assert.Equal("ExportLanguageSourceInvalid", ProjectFile.ErrorResourceKey(error));
        Assert.Equal("dutch", error.Data["Studio.LanguageId"]);
    }

    [Fact]
    public void ExternalSourcesAreResolvedFromProjectAndMissingSourcesHaveLanguageSpecificErrors()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-language-source-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            using var editor = Bilingual();
            var source = Path.Combine(folder, "Eigen taal.isl");
            File.WriteAllText(source, "[LangOptions]\nLanguageName=Eigen\nLanguageID=0\n[Messages]\nButtonNext=Next\n");
            editor.Project.Languages[1].Source = Path.GetFileName(source);
            Assert.Contains("MessagesFile: \"" + source + "\"", InnoScript.Generate(editor.Project, folder));
            Assert.Equal("ExportLanguageSourceNeedsProject", ProjectFile.ErrorResourceKey(
                Assert.Throws<InvalidDataException>(() => InnoScript.Generate(editor.Project, null))));
            File.Delete(source);
            Assert.Equal("ExportLanguageSourceMissing", ProjectFile.ErrorResourceKey(
                Assert.Throws<InvalidDataException>(() => InnoScript.Generate(editor.Project, folder))));
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public void AmpersandsInLiteralButtonTextAreEscapedButTooltipRemainsLiteral()
    {
        using var editor = Bilingual();
        editor.SetLanguageEnabled("dutch", false);
        editor.SetTranslation("english", WizardTextSlot.DestinationBrowse, "R&D");
        editor.SetTranslation("english", WizardTextSlot.DestinationTooltip, "R&D");
        var script = InnoScript.Generate(editor.Project, null);
        Assert.Contains("WizardForm.DirBrowseButton.Caption := 'R&&D';", script);
        Assert.Contains("WizardForm.DirBrowseButton.Hint := 'R&D';", script);
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task MultipleLanguagesIncludingRtlAndDutchOnlyCompileWithoutChangingSources()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-multilingual-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        var compilerDirectory = Path.GetDirectoryName(InnoCompilerFactAttribute.CompilerPath)!;
        var paths = new[] { Path.Combine(compilerDirectory, "Default.isl"), Path.Combine(compilerDirectory, "Languages", "Dutch.isl"),
            Path.Combine(compilerDirectory, "Languages", "German.isl"), Path.Combine(compilerDirectory, "Languages", "Hebrew.isl") };
        var originals = paths.ToDictionary(path => path, File.ReadAllBytes);
        try
        {
            using var editor = Bilingual();
            var german = InstallerLanguage.FromSource(await LanguageSourceFile.LoadAsync(paths[2]), compilerDirectory);
            var hebrew = InstallerLanguage.FromSource(await LanguageSourceFile.LoadAsync(paths[3]), compilerDirectory);
            editor.AddLanguage(german); editor.AddLanguage(hebrew);
            editor.ProductName = "O'Brien & {edition}";
            editor.SetTranslation("english", WizardTextSlot.WelcomeTitle, "Install {AppName} {AppVersion}");
            editor.SetTranslation("dutch", WizardTextSlot.WelcomeTitle, "Installeer {AppName} {AppVersion}");
            editor.SetTranslation(german.Id, WizardTextSlot.WelcomeTitle, "Installation — {AppName}");
            editor.SetTranslation(hebrew.Id, WizardTextSlot.WelcomeTitle, "התקנת {AppName}");
            editor.SetTranslation("dutch", WizardTextSlot.DestinationBody, "Regel 1\nRegel 2");
            var payload = Path.Combine(folder, "payload.txt");
            await File.WriteAllTextAsync(payload, "payload");
            editor.Project.InstallFile.Source = payload;
            var multilingual = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath,
                InnoScript.Generate(editor.Project, folder), folder);
            Assert.NotNull(multilingual.InstallerPath);
            Assert.DoesNotContain(multilingual.Messages, message => message.Severity == CompilerMessageSeverity.Error);
            foreach (var language in editor.Project.Languages.Where(language => language.Id != "dutch").ToArray())
                editor.SetLanguageEnabled(language.Id, false);
            var dutch = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath,
                InnoScript.Generate(editor.Project, folder), folder);
            Assert.NotNull(dutch.InstallerPath);
            foreach (var path in paths) Assert.Equal(originals[path], await File.ReadAllBytesAsync(path));
        }
        finally { Directory.Delete(folder, true); }
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task ExportedScriptWithRelativeExternalDutchSourceCompilesInAnotherDirectory()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-external-language-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            var compilerDirectory = Path.GetDirectoryName(InnoCompilerFactAttribute.CompilerPath)!;
            var languagePath = Path.Combine(folder, "Eigen taal; snapshot.isl");
            File.Copy(Path.Combine(compilerDirectory, "Languages", "Dutch.isl"), languagePath);
            var original = await File.ReadAllBytesAsync(languagePath);
            var source = await LanguageSourceFile.LoadAsync(languagePath);
            using var editor = new EditorSession(StudioProject.CreateNew());
            var language = InstallerLanguage.FromSource(source, compilerDirectory);
            language.Source = Path.GetFileName(languagePath);
            editor.AddLanguage(language);
            editor.SetLanguageEnabled("english", false);
            editor.SetTranslation(language.Id, WizardTextSlot.WelcomeTitle, "Eigen titel {AppName}");
            editor.Project.InstallFile.Source = "payload.txt";
            await File.WriteAllTextAsync(Path.Combine(folder, "payload.txt"), "payload");
            var exportFolder = Directory.CreateDirectory(Path.Combine(folder, "export elsewhere")).FullName;
            var scriptPath = Path.Combine(exportFolder, "installer.iss");
            await InnoScript.ExportAsync(scriptPath, editor.Project, folder);
            var script = await File.ReadAllTextAsync(scriptPath);
            Assert.Contains(languagePath, script);
            Assert.Contains("'Eigen titel My application'", script);
            var start = new ProcessStartInfo(InnoCompilerFactAttribute.CompilerPath)
            {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
            };
            start.ArgumentList.Add("/Q"); start.ArgumentList.Add(scriptPath);
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var errors = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); throw; }
            Assert.True(process.ExitCode == 0, await output + await errors);
            Assert.True(File.Exists(Path.Combine(exportFolder, "output", editor.OutputBaseFileName + ".exe")));
            Assert.Equal(original, await File.ReadAllBytesAsync(languagePath));
        }
        finally { Directory.Delete(folder, true); }
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task IncompleteCustomLanguageCompilesWithNativeWarningsAndProjectFallbackText()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-partial-language-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            var languagePath = Path.Combine(folder, "Partial.isl");
            await File.WriteAllTextAsync(languagePath, "[LangOptions]\nLanguageName=Custom\nLanguageID=0\nLanguageCodePage=0\n[Messages]\nButtonNext=Continue >\n");
            using var editor = new EditorSession(StudioProject.CreateNew());
            var language = InstallerLanguage.FromSource(await LanguageSourceFile.LoadAsync(languagePath), null);
            editor.AddLanguage(language);
            editor.SetTranslation("english", WizardTextSlot.WelcomeTitle, "Default fallback {AppName}");
            editor.Project.InstallFile.Source = Path.Combine(folder, "payload.txt");
            await File.WriteAllTextAsync(editor.Project.InstallFile.Source, "payload");
            var script = InnoScript.Generate(editor.Project, folder);
            Assert.Equal("Default fallback My application", WizardTexts.Resolve(editor.Project, language.Id, WizardTextSlot.WelcomeTitle).Text);
            Assert.Equal(2, script.Split("WizardForm.WelcomeLabel1.Caption := 'Default fallback My application';").Length - 1);
            var result = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath, script, folder);
            Assert.NotNull(result.InstallerPath);
            Assert.Contains(result.Messages, message => message.Severity == CompilerMessageSeverity.Warning);
            Assert.DoesNotContain(result.Messages, message => message.Severity == CompilerMessageSeverity.Error);
        }
        finally { Directory.Delete(folder, true); }
    }
}
