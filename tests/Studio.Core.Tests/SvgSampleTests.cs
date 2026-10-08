using System.Diagnostics;
using Studio.Core;

namespace Studio.Core.Tests;

public sealed class SvgSampleFactAttribute : FactAttribute
{
    public static string SampleDirectory
    {
        get
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "Inno Setup Studio.slnx"))) folder = folder.Parent;
            return folder is null ? "" : Path.Combine(folder.FullName, "samples", "SVGViewerDemo");
        }
    }
    public SvgSampleFactAttribute()
    {
        if (!File.Exists(InnoCompilerFactAttribute.CompilerPath) || !File.Exists(Path.Combine(SampleDirectory, "Source", "SVGViewer.exe")))
            Skip = "Local SVGViewer payload and Inno Setup 7 compiler are required.";
    }
}

public class SvgSampleTests
{
    [SvgSampleFact]
    [Trait("Category", "InnoCompiler")]
    public async Task SvgProjectExportsOutsideStudioAndCompilesWithNativeCompiler()
    {
        var sample = SvgSampleFactAttribute.SampleDirectory;
        var project = await ProjectFile.LoadAsync(Path.Combine(sample, "SVG Viewer M01-proef.issstudio"));
        var folder = Path.Combine(Path.GetTempPath(), "Studio-SVG-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var script = Path.Combine(folder, "external.iss");
            await InnoScript.ExportAsync(script, project, sample);
            var start = new ProcessStartInfo(InnoCompilerFactAttribute.CompilerPath)
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add("/Q");
            start.ArgumentList.Add(script);
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var errors = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { if (!process.HasExited) process.Kill(true); await process.WaitForExitAsync(); throw; }
            Assert.True(process.ExitCode == 0, await output + await errors);
            Assert.True(File.Exists(Path.Combine(folder, "output", project.OutputBaseFileName + ".exe")));
            Assert.Contains("AppId=\"{{a7a61c46-37b5-43a0-9947-e11e03e23d83}\"", await File.ReadAllTextAsync(script));
        }
        finally { Directory.Delete(folder, true); }
    }
}
