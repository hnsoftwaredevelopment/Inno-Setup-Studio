using System.Text.Json.Nodes;
using Studio.Core;

namespace Studio.Core.Tests;

public class BuildDiagnosticsTests
{
    [Theory]
    [InlineData("mysetup", true)]
    [InlineData("Mijn product-1.0", true)]
    [InlineData("setup", true)]
    [InlineData("../setup", false)]
    [InlineData("CON.txt", false)]
    [InlineData("CON .txt", false)]
    [InlineData("COM¹", false)]
    [InlineData("setup.exe", false)]
    [InlineData("x{#1}", false)]
    [InlineData("name.", false)]
    [InlineData("", false)]
    public void OutputNameIsAPlainWindowsBaseFilename(string name, bool valid) =>
        Assert.Equal(valid, InstallerOutput.IsValid(name));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CurrentFormatRequiresOutputNameAndOlderFormatsCannotSilentlyDiscardIt(bool older)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            await ProjectFile.SaveAsync(path, StudioProject.CreateExample());
            var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            if (older) json["Version"] = 3;
            else json.Remove("OutputBaseFileName");
            await File.WriteAllTextAsync(path, json.ToJsonString());
            await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.LoadAsync(path));
        }
        finally { File.Delete(path); }
    }

    [InnoCompilerFact]
    public async Task CancellationBeforeStartDoesNotCreateAnOutputDirectory()
    {
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        var folder = Path.Combine(Path.GetTempPath(), "Studio-cancel-" + Guid.NewGuid().ToString("N"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CompilerService.BuildAsync(
            InnoCompilerFactAttribute.CompilerPath, "ignored", folder, cancellationToken: cancel.Token));
        Assert.False(Directory.Exists(folder));
    }

    [Fact]
    public async Task FormatThreeMigrationPreservesSourceAndIdentityAndAddsPreviousOutputName()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".issstudio");
        try
        {
            var original = StudioProject.CreateExample();
            original.InstallFile.Source = "payload.txt";
            await ProjectFile.SaveAsync(path, original);
            var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            json["Version"] = 3;
            json.Remove("OutputBaseFileName");
            var content = json.ToJsonString();
            await File.WriteAllTextAsync(path, content);
            var restored = await ProjectFile.LoadAsync(path);
            Assert.Equal("setup", restored.OutputBaseFileName);
            Assert.Equal(original.AppId, restored.AppId);
            Assert.Equal(original.InstallFile.Source, restored.InstallFile.Source);
            Assert.True(restored.WasMigrated);
            Assert.Equal(content, await File.ReadAllTextAsync(path));
            using var editor = new EditorSession(restored);
            editor.OutputBaseFileName = "Mijn installer";
            Assert.True(editor.IsDirty);
            await ProjectFile.SaveAsync(path, restored);
            Assert.Equal("Mijn installer", (await ProjectFile.LoadAsync(path)).OutputBaseFileName);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("{\"severity\":\"warning\",\"message\":\"native warning\"}", CompilerMessageSeverity.Warning)]
    [InlineData("{\"severity\":\"error\",\"message\":\"native error\",\"filename\":\"installer.iss\",\"line\":12}", CompilerMessageSeverity.Error)]
    [InlineData("Error: plain text from 7.1.0", CompilerMessageSeverity.Error)]
    [InlineData("Compiling...", CompilerMessageSeverity.Information)]
    [InlineData("{malformed", CompilerMessageSeverity.Information)]
    [InlineData("{\"severity\":\"error\",\"message\":\"native\",\"line\":\"invalid\"}", CompilerMessageSeverity.Error)]
    public void NativeDiagnosticsAndFallbackTextArePreserved(string line, CompilerMessageSeverity severity)
    {
        var message = CompilerMessage.Parse(line);
        Assert.Equal(severity, message.Severity);
        Assert.Equal(line, message.Raw);
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task SetupWarningAndCompileErrorAreStructuredAndLogged()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-diagnostics-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var project = StudioProject.CreateExample();
            project.InstallFile.Source = Path.Combine(folder, "payload.txt");
            await File.WriteAllTextAsync(project.InstallFile.Source, "payload");
            project.OutputBaseFileName = "setup";
            var result = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath,
                InnoScript.Generate(project, folder), folder, "setup");
            Assert.NotNull(result.InstallerPath);
            Assert.Contains(result.Messages, m => m.Severity == CompilerMessageSeverity.Warning && m.Message.Contains("OutputBaseFileName"));
            Assert.True(File.Exists(Path.Combine(result.Directory, "compiler.log")));
            var failed = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath, "[Setup]\r\nWrongDirective=yes", folder);
            Assert.Null(failed.InstallerPath);
            Assert.Contains(failed.Messages, m => m.Severity == CompilerMessageSeverity.Error && m.Line == 2);
        }
        finally { Directory.Delete(folder, true); }
    }

    [InnoCompilerFact]
    [Trait("Category", "InnoCompiler")]
    public async Task CancellationAfterCompilerStartsStopsRunAndNeverReportsAnInstaller()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-cancel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            using var cancel = new CancellationTokenSource();
            var progress = new CancelOnFirstLine(cancel);
            var project = StudioProject.CreateExample();
            project.InstallFile.Source = Path.Combine(folder, "payload.dat");
            var payload = new byte[4 * 1024 * 1024];
            new Random(1).NextBytes(payload);
            await File.WriteAllBytesAsync(project.InstallFile.Source, payload);
            var result = await CompilerService.BuildAsync(InnoCompilerFactAttribute.CompilerPath,
                InnoScript.Generate(project, folder), folder,
                progress: progress, cancellationToken: cancel.Token);
            Assert.True(result.Canceled);
            Assert.Null(result.InstallerPath);
            Assert.True(File.Exists(Path.Combine(result.Directory, "compiler.log")));
        }
        finally { Directory.Delete(folder, true); }
    }

    private sealed class CancelOnFirstLine(CancellationTokenSource cancel) : IProgress<CompilerMessage>
    {
        public void Report(CompilerMessage value) => cancel.Cancel();
    }
}
