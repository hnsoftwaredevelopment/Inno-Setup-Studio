using System.Diagnostics;
using Studio.Core;

namespace Studio.Core.Tests;

public sealed class InnoCompilerFactAttribute : FactAttribute
{
    public static string CompilerPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Inno Setup 7", "ISCC.exe");
    public InnoCompilerFactAttribute()
    {
        if (!File.Exists(CompilerPath)) Skip = "Inno Setup 7 compiler is not installed at the standard location.";
    }
}

public class InnoCompilerTests
{
    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task SuppliedExampleExportsAndCompilesWithoutStudio()
    {
        var sample = Path.Combine(AppContext.BaseDirectory, "Fixtures", "FirstInstaller");
        var project = await ProjectFile.LoadAsync(Path.Combine(sample, "Eerste installer.issstudio"));
        var folder = Path.Combine(Path.GetTempPath(), "Studio-sample-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var script = Path.Combine(folder, "installer.iss");
            await InnoScript.ExportAsync(script, project, sample);
            var start = new ProcessStartInfo(InnoCompilerFactAttribute.CompilerPath)
            {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
            };
            start.ArgumentList.Add("/Q");
            start.ArgumentList.Add(script);
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var errors = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); throw; }
            Assert.True(process.ExitCode == 0, await output + await errors);
            Assert.True(File.Exists(Path.Combine(folder, "output", "setup.exe")));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task ExportCompilesOutsideStudioWithSpecialCharactersAndCustomWizardText()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-compiler-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var source = Path.Combine(folder, "résumé; {edition}.txt");
            await File.WriteAllTextAsync(source, "Small test payload");
            var project = StudioProject.CreateExample();
            project.Name = " Product \"Edition\" {2026} ";
            project.AppVersion = "2026.10-beta";
            project.InstallFile.Source = source;
            project.InstallFile.Destination = @"{app}\Données; 2026";
            project.Pages[0].Title = "Welkom O'Brien — {edition}";
            project.Pages[0].Body = "Eerste regel\r\nTweede regel\n{#1+1} 50% [name] 'tekst'; end;";
            project.Pages[0].ButtonOverrides[ElementKind.Next] = "&Start >";
            project.Pages[1].Title = "Données: kies een map";
            project.Pages[1].Body = "Regel één\nRegel twee";
            project.Pages[1].ButtonOverrides[ElementKind.Cancel] = "Stop";
            project.Pages[1].Destination!.BrowseCaption = "&Kiezen...";
            project.Pages[1].Destination!.BrowseTooltip = "# Kies O'Brien {#1+1}\nEen andere map";
            var exportDirectory = Directory.CreateDirectory(Path.Combine(folder, "export elsewhere")).FullName;
            var script = Path.Combine(exportDirectory, "test.iss");
            await InnoScript.ExportAsync(script, project, folder);
            var start = new ProcessStartInfo(InnoCompilerFactAttribute.CompilerPath)
            {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true,
                CreateNoWindow = true, WorkingDirectory = folder
            };
            start.ArgumentList.Add("/Q");
            start.ArgumentList.Add(script);
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var errors = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); throw; }
            Assert.True(process.ExitCode == 0, await output + await errors);
            Assert.True(File.Exists(Path.Combine(exportDirectory, "output", "setup.exe")));
            Assert.Equal("Small test payload", await File.ReadAllTextAsync(source));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
