using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Studio.Core;

namespace Studio.Core.Tests;

public sealed class ScriptExportTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "Studio-export-" + Guid.NewGuid().ToString("N"));
    public ScriptExportTests() => Directory.CreateDirectory(_folder);
    public void Dispose() => Directory.Delete(_folder, recursive: true);

    private StudioProject ProjectWithFile()
    {
        File.WriteAllText(Path.Combine(_folder, "résumé; data.txt"), "Test payload");
        var project = StudioProject.CreateExample();
        project.InstallFile.Source = "résumé; data.txt";
        project.InstallFile.Destination = @"{app}\Data";
        return project;
    }

    [Fact]
    public async Task ExportPreservesIdentityAndResolvesRelativeSourceFromProjectNotExportDirectory()
    {
        var project = ProjectWithFile();
        project.Name = " Product \"Special\" {édition} ";
        project.AppVersion = "2026.10-beta";
        var script = InnoScript.Generate(project, _folder);
        Assert.Contains("AppId=\"" + project.AppId.Replace("{", "{{") + "\"", script);
        Assert.Contains("AppName=\" Product \"Special\" {{édition} \"", script);
        Assert.Contains("AppVersion=\"2026.10-beta\"", script);
        Assert.Contains("Source: \"" + Path.Combine(_folder, "résumé; data.txt") + "\"; DestDir: \"{app}\\Data\"", script);
        Assert.Contains("compiler:Languages\\Dutch.isl", script);
        Assert.Equal(script, InnoScript.Generate(project, _folder));
        var path = Path.Combine(_folder, "project.issstudio");
        await ProjectFile.SaveAsync(path, project);
        var restored = await ProjectFile.LoadAsync(path);
        Assert.Equal(project.InstallFile.Source, restored.InstallFile.Source);
        Assert.Equal(project.InstallFile.Destination, restored.InstallFile.Destination);
        Assert.Equal(script, InnoScript.Generate(restored, _folder));
    }

    [Fact]
    public async Task MissingSourceCannotReplaceExistingExport()
    {
        var project = ProjectWithFile();
        var path = Path.Combine(_folder, "installer.iss");
        await InnoScript.ExportAsync(path, project, _folder);
        var original = await File.ReadAllBytesAsync(path);
        File.Delete(Path.Combine(_folder, project.InstallFile.Source));
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => InnoScript.ExportAsync(path, project, _folder));
        Assert.Equal("ExportSourceMissing", ProjectFile.ErrorResourceKey(error));
        Assert.Equal(original, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public void RelativeSourceNeedsProjectLocationAndPreviewCannotEditFileRule()
    {
        var project = ProjectWithFile();
        Assert.Throws<InvalidDataException>(() => InnoScript.Generate(project, null));
        using var editor = new EditorSession(project);
        editor.SourceFile = editor.SourceFile;
        Assert.False(editor.IsDirty);
        editor.FileDestination = @"{app}\Documents";
        Assert.True(editor.IsDirty);
        editor.MarkSaved();
        editor.IsPreview = true;
        editor.SourceFile = "other.txt";
        editor.FileDestination = @"{app}\Other";
        Assert.Equal("résumé; data.txt", project.InstallFile.Source);
        Assert.Equal(@"{app}\Documents", project.InstallFile.Destination);
        Assert.False(editor.IsDirty);
    }

    [Theory]
    [InlineData("Name", "Injected\n[Code]")]
    [InlineData("Name", "{#1+1}")]
    [InlineData("AppId", "{#1+1}")]
    [InlineData("AppVersion", "{#1+1}")]
    public void ExportRejectsScriptAndPreprocessorInjection(string property, string value)
    {
        var project = ProjectWithFile();
        typeof(StudioProject).GetProperty(property)!.SetValue(project, value);
        Assert.Throws<InvalidDataException>(() => InnoScript.Generate(project, _folder));
    }

    [Theory]
    [InlineData("*")]
    [InlineData("missing.txt")]
    [InlineData("C:relative.txt")]
    public void ExportRejectsWildcardMissingAndDriveRelativeSources(string source)
    {
        var project = ProjectWithFile();
        project.InstallFile.Source = source;
        Assert.Throws<InvalidDataException>(() => InnoScript.Generate(project, _folder));
    }

    [Fact]
    public async Task FormatTwoMigratesWithoutChangingIdentityOrOriginalFile()
    {
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var originalProject = StudioProject.CreateExample();
        originalProject.AppVersion = "9.2-beta";
        originalProject.Buttons[ElementKind.Next] = "Doorgaan";
        var legacy = JsonNode.Parse(JsonSerializer.Serialize(originalProject, options))!.AsObject();
        legacy["Version"] = 2;
        legacy.Remove("InstallFile");
        var path = Path.Combine(_folder, "legacy.issstudio");
        var content = legacy.ToJsonString();
        await File.WriteAllTextAsync(path, content);
        var migrated = await ProjectFile.LoadAsync(path);
        Assert.Equal(originalProject.AppId, migrated.AppId);
        Assert.Equal("9.2-beta", migrated.AppVersion);
        Assert.Equal("Doorgaan", migrated.Buttons[ElementKind.Next]);
        Assert.Equal(StudioProject.CurrentVersion, migrated.Version);
        Assert.True(migrated.WasMigrated);
        Assert.Equal("", migrated.InstallFile.Source);
        Assert.Equal("{app}", migrated.InstallFile.Destination);
        Assert.Equal(content, await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task SaveAsInAnotherFolderKeepsTheSameSourceAndIdentity()
    {
        var project = ProjectWithFile();
        var other = Directory.CreateDirectory(Path.Combine(_folder, "Other")).FullName;
        var path = Path.Combine(other, "copy.issstudio");
        await ProjectFile.SaveAsync(path, project, _folder);
        var restored = await ProjectFile.LoadAsync(path);
        Assert.Equal(project.AppId, restored.AppId);
        Assert.Equal(Path.Combine(_folder, "résumé; data.txt"), InnoScript.ResolveSource(restored.InstallFile.Source, other));
        Assert.Equal(restored.InstallFile.Source, project.InstallFile.Source);
    }

    [Fact]
    public async Task FailedSaveAsDoesNotRebaseTheInMemorySource()
    {
        var project = ProjectWithFile();
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => ProjectFile.SaveAsync(Path.Combine(_folder, "missing", "copy.issstudio"), project, _folder));
        Assert.Equal("résumé; data.txt", project.InstallFile.Source);
    }

    [Theory]
    [InlineData("{app}\\..\\Other")]
    [InlineData("{app}\\../Other")]
    [InlineData("{app}\\.. \\Other")]
    [InlineData("{app}\\Documents.")]
    [InlineData("{sys}")]
    [InlineData("{app}\\Data\n[Code]")]
    [InlineData("{app}\\\"; Flags: external")]
    [InlineData("{app}\\{#1+1}")]
    public void InvalidDestinationsAreRejected(string destination)
    {
        var project = ProjectWithFile();
        project.InstallFile.Destination = destination;
        Assert.Throws<InvalidDataException>(() => InnoScript.Generate(project, _folder));
    }

    [Fact]
    public async Task ExportCannotOverwriteTheSelectedSourceOrAcceptAnUnrelatedExtension()
    {
        var project = ProjectWithFile();
        var path = Path.Combine(_folder, "payload.iss");
        await File.WriteAllTextAsync(path, "Original payload");
        project.InstallFile.Source = path;
        await Assert.ThrowsAsync<InvalidDataException>(() => InnoScript.ExportAsync(path, project, _folder));
        Assert.Equal("Original payload", await File.ReadAllTextAsync(path));
        await Assert.ThrowsAsync<InvalidDataException>(() => InnoScript.ExportAsync(Path.Combine(_folder, "project.issstudio"), project, _folder));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CurrentFormatRequiresAnExplicitNonNullFileRule(bool nullRule)
    {
        var project = ProjectWithFile();
        var path = Path.Combine(_folder, "project.issstudio");
        await ProjectFile.SaveAsync(path, project);
        var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
        if (nullRule) json["InstallFile"] = null;
        else json.Remove("InstallFile");
        await File.WriteAllTextAsync(path, json.ToJsonString());
        await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.LoadAsync(path));
    }

    [Fact]
    public async Task IncorrectOldFormatCannotSilentlyDiscardAFileRule()
    {
        var project = ProjectWithFile();
        var path = Path.Combine(_folder, "project.issstudio");
        await ProjectFile.SaveAsync(path, project);
        var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
        json["Version"] = 2;
        await File.WriteAllTextAsync(path, json.ToJsonString());
        await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.LoadAsync(path));
    }
}
