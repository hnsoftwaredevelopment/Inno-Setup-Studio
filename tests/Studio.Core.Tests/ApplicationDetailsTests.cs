using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Studio.Core;

namespace Studio.Core.Tests;

public class ApplicationDetailsTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    [Fact]
    public async Task IdentitySurvivesRenameVersionChangeLanguageSwitchAndSaveAs()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        var copyPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var project = StudioProject.CreateExample();
            var identity = project.AppId;
            Assert.True(Guid.TryParse(identity, out _));
            Assert.NotEqual(identity, StudioProject.CreateExample().AppId);
            using var editor = new EditorSession(project);
            editor.ProductName = "Mijn product";
            editor.ApplicationVersion = "2026.10-beta";
            editor.Text.Language = "de";
            Assert.True(editor.IsDirty);
            Assert.Equal(identity, editor.ApplicationId);
            await ProjectFile.SaveAsync(path, project);
            var restored = await ProjectFile.LoadAsync(path);
            Assert.Equal("Mijn product", restored.Name);
            Assert.Equal("2026.10-beta", restored.AppVersion);
            Assert.Equal(identity, restored.AppId);
            Assert.Equal("Welkom bij de installatie van Mijn applicatie", restored.Pages[0].Title);
            await ProjectFile.SaveAsync(copyPath, restored);
            Assert.Equal(identity, (await ProjectFile.LoadAsync(copyPath)).AppId);
        }
        finally { File.Delete(path); File.Delete(copyPath); }
    }

    [Fact]
    public void PreviewAndUnchangedBindingsCannotModifyApplicationDetails()
    {
        using var editor = new EditorSession(StudioProject.CreateExample());
        editor.ProductName = editor.ProductName;
        editor.ApplicationId = editor.ApplicationId;
        editor.ApplicationVersion = editor.ApplicationVersion;
        Assert.False(editor.IsDirty);
        var identity = editor.ApplicationId;
        editor.IsPreview = true;
        editor.ProductName = "Onbedoeld";
        editor.ApplicationId = "Andere identiteit";
        editor.ApplicationVersion = "2.0";
        Assert.Equal("Mijn applicatie", editor.ProductName);
        Assert.Equal(identity, editor.ApplicationId);
        Assert.Equal("1.0", editor.ApplicationVersion);
        Assert.False(editor.IsDirty);
    }

    [Fact]
    public async Task LegacyProjectMigratesWithoutOverwritingAndKeepsIdentityWhenReopened()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var original = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "LegacyProject.issstudio"));
            await File.WriteAllTextAsync(path, original);
            var project = await ProjectFile.LoadAsync(path);
            Assert.Equal(StudioProject.CurrentVersion, project.Version);
            Assert.Equal("1.0", project.AppVersion);
            Assert.Equal(project.AppId, (await ProjectFile.LoadAsync(path)).AppId);
            Assert.Equal(original, await File.ReadAllTextAsync(path));
            using var editor = new EditorSession(project);
            Assert.True(editor.IsDirty);
            await ProjectFile.SaveAsync(path, project);
            editor.MarkSaved();
            var restored = await ProjectFile.LoadAsync(path);
            Assert.Equal(project.AppId, restored.AppId);
            Assert.Equal(project.Pages[1].Destination!.DefaultDirectory, restored.Pages[1].Destination!.DefaultDirectory);
            Assert.Equal(project.Buttons, restored.Buttons);
            var saved = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
            saved["Version"] = 1;
            saved.Remove("AppId");
            saved.Remove("AppVersion");
            saved.Remove("InstallFile");
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(original), saved));
            using var reopened = new EditorSession(restored);
            Assert.False(reopened.IsDirty);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(127, true)]
    [InlineData(128, false)]
    public async Task CustomApplicationIdHonoursLengthLimitAndIsPreserved(int length, bool valid)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            using var editor = new EditorSession(StudioProject.CreateExample());
            editor.ApplicationId = new string('a', length);
            Assert.True(editor.IsDirty);
            if (valid)
            {
                await ProjectFile.SaveAsync(path, editor.Project);
                Assert.Equal(editor.ApplicationId, (await ProjectFile.LoadAsync(path)).AppId);
            }
            else
            {
                var error = await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.SaveAsync(path, editor.Project));
                Assert.Equal("InvalidApplicationDetails", ProjectFile.ErrorResourceKey(error));
                Assert.False(File.Exists(path));
            }
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Name", "")]
    [InlineData("AppId", " ")]
    [InlineData("AppId", "product\nander")]
    [InlineData("AppVersion", "")]
    [InlineData("AppVersion", "1.0\nAppId=ander")]
    public async Task InvalidMetadataCannotOverwriteSavedProject(string property, string value)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var project = StudioProject.CreateExample();
            await ProjectFile.SaveAsync(path, project);
            var original = await File.ReadAllTextAsync(path);
            typeof(StudioProject).GetProperty(property)!.SetValue(project, value);
            await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.SaveAsync(path, project));
            Assert.Equal(original, await File.ReadAllTextAsync(path));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("AppId")]
    [InlineData("AppVersion")]
    public async Task CurrentFormatRequiresExplicitMetadata(string property)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var json = JsonNode.Parse(JsonSerializer.Serialize(StudioProject.CreateExample(), Options))!.AsObject();
            json.Remove(property);
            await File.WriteAllTextAsync(path, json.ToJsonString());
            await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.LoadAsync(path));
        }
        finally { File.Delete(path); }
    }
}
