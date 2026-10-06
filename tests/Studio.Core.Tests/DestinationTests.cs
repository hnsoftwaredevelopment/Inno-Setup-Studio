using Studio.Core;

namespace Studio.Core.Tests;

public class DestinationTests
{
    [Fact]
    public void DestinationElementsEditTheirOwnSettings()
    {
        var editor = new EditorSession(StudioProject.CreateExample()) { PageIndex = 2 };
        editor.Activate(ElementKind.Directory);
        editor.PropertyText = @"{autopf}\HN Software\Mijn applicatie";
        Assert.Equal(editor.PropertyText, editor.DirectoryText);
        editor.Activate(ElementKind.Browse);
        editor.PropertyText = "Andere map...";
        editor.BrowseTooltip = "Kies waar de applicatie wordt geïnstalleerd";
        Assert.Equal("Andere map...", editor.BrowseCaption);
        Assert.False(editor.CanReset);
        Assert.True(editor.IsDirty);
        Assert.Equal("Verder >", editor.NextCaption);
    }

    [Fact]
    public void TrialDirectoryDoesNotChangeProjectAndIsResetBetweenRuns()
    {
        var editor = new EditorSession(StudioProject.CreateExample()) { PageIndex = 2 };
        editor.Activate(ElementKind.Directory);
        var original = editor.PropertyText;
        editor.IsPreview = true;
        editor.SetTrialDirectory(@"C:\Testmap");
        Assert.Equal(@"C:\Testmap", editor.DirectoryText);
        Assert.False(editor.IsDirty);
        editor.Activate(ElementKind.Back);
        editor.Activate(ElementKind.Next);
        Assert.Equal(@"C:\Testmap", editor.DirectoryText);
        editor.IsPreview = false;
        Assert.Equal(ElementKind.Directory, editor.SelectedElement);
        Assert.Equal(original, editor.DirectoryText);
        editor.IsPreview = true;
        Assert.Equal(original, editor.DirectoryText);
    }

    [Fact]
    public void DestinationElementsAreUnavailableOnOtherPages()
    {
        var editor = new EditorSession(StudioProject.CreateExample()) { PageIndex = 2 };
        editor.Activate(ElementKind.Browse);
        editor.PageIndex = 1;
        Assert.DoesNotContain(editor.Elements, e => e.Kind == ElementKind.Browse);
        Assert.Equal(ElementKind.Next, editor.SelectedElement);
        editor.Activate(ElementKind.Directory);
        Assert.Equal(ElementKind.Next, editor.SelectedElement);
        editor.PageIndex = 0;
        Assert.Equal(3, editor.Elements.Count());
    }

    [Fact]
    public async Task DestinationSettingsSurviveSaveAndLoad()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var editor = new EditorSession(StudioProject.CreateExample()) { PageIndex = 2 };
            editor.Activate(ElementKind.Directory);
            editor.PropertyText = @"{autopf}\HN Software";
            editor.Activate(ElementKind.Browse);
            editor.PropertyText = "Kies map";
            editor.BrowseTooltip = "Andere map selecteren";
            await ProjectFile.SaveAsync(path, editor.Project);
            var restored = new EditorSession(await ProjectFile.LoadAsync(path)) { PageIndex = 2 };
            Assert.Equal(@"{autopf}\HN Software", restored.DirectoryText);
            Assert.Equal("Kies map", restored.BrowseCaption);
            Assert.Equal("Andere map selecteren", restored.BrowseTooltip);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void PreviewStillReportsLocalCaptionOrigin()
    {
        var editor = new EditorSession(StudioProject.CreateExample()) { PageIndex = 1 };
        editor.PropertyText = "Beginnen";
        editor.IsPreview = true;
        Assert.Equal("Aangepast voor deze pagina", editor.Origin);
        Assert.False(editor.CanReset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("C:\\Applicatie\nOnjuist")]
    public async Task InvalidDefaultDirectoryCannotBeSaved(string directory)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var project = StudioProject.CreateExample();
            project.Pages[1].Destination!.DefaultDirectory = directory;
            await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.SaveAsync(path, project));
            Assert.False(File.Exists(path));
        }
        finally { File.Delete(path); }
    }
}
