using Studio.Core;

namespace Studio.Core.Tests;

public class ProjectTests
{
    [Fact]
    public void SharedChangeFlowsToPagesWithoutOverwritingLocalCaption()
    {
        var project = StudioProject.CreateExample();
        project.Pages[1].ButtonOverrides[ElementKind.Next] = "Kies deze map";
        project.Buttons[ElementKind.Next] = "Doorgaan";
        Assert.Equal("Doorgaan", project.Caption(project.Pages[0], ElementKind.Next));
        Assert.Equal("Kies deze map", project.Caption(project.Pages[1], ElementKind.Next));
    }

    [Fact]
    public void EmptyOverrideIsDifferentFromInheritanceAndCanBeReset()
    {
        var project = StudioProject.CreateExample();
        var page = project.Pages[0];
        page.ButtonOverrides[ElementKind.Next] = "";
        Assert.Equal("", project.Caption(page, ElementKind.Next));
        page.ButtonOverrides.Remove(ElementKind.Next);
        Assert.Equal("Verder >", project.Caption(page, ElementKind.Next));
    }
}
