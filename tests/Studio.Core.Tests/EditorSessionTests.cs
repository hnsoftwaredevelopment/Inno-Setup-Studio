using Studio.Core;

namespace Studio.Core.Tests;

public class EditorSessionTests
{
    [Fact]
    public void DesignClickSelectsButPreviewClickNavigatesWithoutChangingProject()
    {
        var editor = new EditorSession(StudioProject.CreateExample());
        editor.PageIndex = 1;
        editor.Activate(ElementKind.Next);
        Assert.Equal(ElementKind.Next, editor.SelectedElement);
        Assert.Equal(1, editor.PageIndex);
        editor.IsPreview = true;
        editor.Activate(ElementKind.Next);
        Assert.Equal(2, editor.PageIndex);
        Assert.False(editor.IsDirty);
        editor.IsPreview = false;
        Assert.Equal(1, editor.PageIndex);
        Assert.Equal(ElementKind.Next, editor.SelectedElement);
    }

    [Fact]
    public void EditingAndResettingCaptionUpdatesEffectiveValueAndOrigin()
    {
        var editor = new EditorSession(StudioProject.CreateExample());
        editor.PageIndex = 1;
        editor.Activate(ElementKind.Next);
        Assert.False(editor.CanReset);
        editor.PropertyText = "Doorgaan";
        Assert.True(editor.CanReset);
        Assert.Equal("Doorgaan", editor.NextCaption);
        Assert.True(editor.IsDirty);
        editor.ResetProperty();
        Assert.False(editor.CanReset);
        Assert.Equal("Verder >", editor.NextCaption);
        editor.PageIndex = 0;
        editor.PropertyText = "Volgende";
        editor.PageIndex = 2;
        Assert.Equal("Volgende", editor.NextCaption);
    }

    [Fact]
    public void PreviewCannotMutatePropertiesAndCancelReturnsToDesign()
    {
        var editor = new EditorSession(StudioProject.CreateExample());
        editor.IsPreview = true;
        editor.PropertyText = "Onbedoelde wijziging";
        editor.ResetProperty();
        editor.Activate(ElementKind.Cancel);
        Assert.False(editor.IsPreview);
        Assert.False(editor.IsDirty);
        Assert.Equal(0, editor.PageIndex);
    }

    [Fact]
    public void PreviewStopsAtEndAndDoesNotPretendToInstall()
    {
        var editor = new EditorSession(StudioProject.CreateExample());
        editor.PageIndex = 2;
        editor.IsPreview = true;
        editor.Activate(ElementKind.Next);
        Assert.Equal(2, editor.PageIndex);
        Assert.Contains("Einde", editor.Status);
        Assert.False(editor.IsDirty);
    }
}
