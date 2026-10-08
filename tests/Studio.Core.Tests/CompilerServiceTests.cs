using Studio.Core;

namespace Studio.Core.Tests;

public class CompilerServiceTests
{
    [Fact]
    public void CompilerChoiceSurvivesRestartAndDoesNotChangeLanguagePreference()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-preferences-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var language = Path.Combine(folder, "settings.json");
            var compiler = Path.Combine(folder, "compiler.json");
            StudioPreferences.SaveLanguage(language, "de");
            var original = File.ReadAllText(language);
            var chosen = Path.Combine(folder, "custom tools", "ISCC.exe");
            CompilerService.SaveLocation(compiler, chosen);
            Assert.Equal(chosen, CompilerService.LoadLocation(compiler));
            Assert.Equal(original, File.ReadAllText(language));
            File.WriteAllText(compiler, "broken json");
            Assert.Null(CompilerService.LoadLocation(compiler));
        }
        finally { Directory.Delete(folder, true); }
    }

    [Theory]
    [InlineData("7.1.0", true)]
    [InlineData("7.1.2", true)]
    [InlineData("7.0.0", false)]
    [InlineData("7.2.0", false)]
    [InlineData("not a version", false)]
    public void OnlyTargetCompilerFamilyIsSupported(string version, bool expected) =>
        Assert.Equal(expected, CompilerService.IsSupported(version));

    [Fact]
    public async Task MissingCompilerCannotCreateBuildOutput()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-build-" + Guid.NewGuid().ToString("N"));
        await Assert.ThrowsAsync<FileNotFoundException>(() => CompilerService.BuildAsync(
            Path.Combine(folder, "ISCC.exe"), "invalid script", folder));
        Assert.False(Directory.Exists(folder));
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task BuildCompilesSnapshotIntoSeparateRunsAndDoesNotChangeSource()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-build-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var source = Path.Combine(folder, "payload.txt");
            await File.WriteAllTextAsync(source, "Payload");
            var project = StudioProject.CreateExample();
            project.InstallFile.Source = source;
            var script = InnoScript.Generate(project, folder);
            var first = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath, script, folder);
            var second = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath, script, folder);
            Assert.NotEqual(first.Directory, second.Directory);
            Assert.True(File.Exists(first.InstallerPath));
            Assert.True(File.Exists(second.InstallerPath));
            Assert.Equal(script, await File.ReadAllTextAsync(first.ScriptPath));
            Assert.Equal("Payload", await File.ReadAllTextAsync(source));
            var failed = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath, "[Setup]\r\nNotADirective=yes", folder);
            Assert.Null(failed.InstallerPath);
            Assert.NotEqual(0, failed.ExitCode);
            Assert.True(File.Exists(first.InstallerPath));
        }
        finally { Directory.Delete(folder, true); }
    }
}
