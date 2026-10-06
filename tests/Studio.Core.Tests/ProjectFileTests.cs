using Studio.Core;

namespace Studio.Core.Tests;

public class ProjectFileTests
{
    [Fact]
    public async Task SaveAndLoadPreserveSharedAndExplicitlyEmptyLocalValues()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var project = StudioProject.CreateExample();
            project.Buttons[ElementKind.Next] = "Volgende";
            project.Pages[1].ButtonOverrides[ElementKind.Next] = "";
            await ProjectFile.SaveAsync(path, project);
            var restored = await ProjectFile.LoadAsync(path);
            Assert.Equal("Volgende", restored.Caption(restored.Pages[0], ElementKind.Next));
            Assert.Equal("", restored.Caption(restored.Pages[1], ElementKind.Next));
            project.Pages[0].Title = "Nieuwe titel";
            await ProjectFile.SaveAsync(path, project);
            Assert.Equal("Nieuwe titel", (await ProjectFile.LoadAsync(path)).Pages[0].Title);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"Format\":\"HN.InnoSetupStudio.VisualProject\",\"Version\":999}")]
    [InlineData("{\"Format\":\"HN.InnoSetupStudio.VisualProject\",\"Version\":1,\"Pages\":null}")]
    public async Task InvalidOrForeignDocumentsAreRejected(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            await File.WriteAllTextAsync(path, content);
            await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.LoadAsync(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task InvalidProjectCannotReplaceExistingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issstudio");
        try
        {
            var project = StudioProject.CreateExample();
            await ProjectFile.SaveAsync(path, project);
            project.Buttons.Remove(ElementKind.Next);
            await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.SaveAsync(path, project));
            Assert.Equal("Verder >", (await ProjectFile.LoadAsync(path)).Buttons[ElementKind.Next]);
        }
        finally { File.Delete(path); }
    }
}
