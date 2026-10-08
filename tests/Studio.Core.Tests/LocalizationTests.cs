using System.Globalization;
using System.Resources;
using System.Text.Json;
using Studio.Core;

namespace Studio.Core.Tests;

public class LocalizationTests
{
    [Theory]
    [InlineData("nl", "Basisontwerp")]
    [InlineData("en", "Base design")]
    [InlineData("de", "Basisentwurf")]
    [InlineData("fr", "Basisontwerp")]
    public void LanguageSelectionUsesDutchFallback(string language, string expected)
    {
        var text = new StudioLocalizer(language);
        Assert.Equal(expected, text["BaseDesign"]);
    }

    [Fact]
    public void EveryTranslationContainsAllNeutralKeys()
    {
        var resources = new ResourceManager("Studio.Core.Resources.Strings", typeof(StudioLocalizer).Assembly);
        var neutral = resources.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        foreach (var language in new[] { "en", "de" })
        {
            var translated = resources.GetResourceSet(CultureInfo.GetCultureInfo(language), true, false)!;
            Assert.NotNull(translated);
            foreach (System.Collections.DictionaryEntry entry in neutral)
                Assert.False(string.IsNullOrWhiteSpace(translated.GetString((string)entry.Key)), $"{language}: {entry.Key}");
        }
    }

    [Fact]
    public void LanguageChangePreservesProjectSelectionAndTrialState()
    {
        var text = new StudioLocalizer();
        using var editor = new EditorSession(StudioProject.CreateExample(), text);
        editor.PageIndex = 2;
        editor.SelectedElement = ElementKind.Browse;
        editor.PropertyText = "Mijn eigen knop";
        var before = JsonSerializer.Serialize(editor.Project);
        var choices = editor.PageChoices;
        editor.IsPreview = true;
        editor.SetTrialDirectory(@"C:\Test");
        text.Language = "de";
        Assert.Same(choices, editor.PageChoices);
        Assert.Equal("Installationsordner", editor.PageName);
        Assert.Equal("Durchsuchen", editor.SelectedElementName);
        Assert.Equal(@"C:\Test", editor.DirectoryText);
        Assert.True(editor.IsDirty);
        Assert.Equal(before, JsonSerializer.Serialize(editor.Project));
        editor.IsPreview = false;
        Assert.Equal(2, editor.PageIndex);
        Assert.Equal(ElementKind.Browse, editor.SelectedElement);
        Assert.Equal("Mijn eigen knop", editor.BrowseCaption);
    }

    [Fact]
    public void LanguageChangeDoesNotMakeCleanProjectDirtyAndTranslatesStatus()
    {
        var text = new StudioLocalizer();
        using var editor = new EditorSession(StudioProject.CreateExample(), text);
        editor.MarkSaved();
        text.Language = "en";
        Assert.Equal("Project saved.", editor.Status);
        Assert.False(editor.IsDirty);
        Assert.Equal("Page 01", editor.PageChoices[1].Description);
    }

    [Fact]
    public void BindingFeedbackDoesNotRefreshAnUnchangedSelection()
    {
        using var editor = new EditorSession(StudioProject.CreateExample());
        editor.PageIndex = 2;
        var notifications = 0;
        editor.PropertyChanged += (_, _) =>
        {
            notifications++;
            if (notifications < 5) editor.SelectedElement = editor.SelectedElement;
        };
        editor.SelectedElement = ElementKind.Directory;
        Assert.Equal(1, notifications);
        var elements = editor.Elements;
        editor.SelectedElement = ElementKind.Browse;
        Assert.Same(elements, editor.Elements);
    }

    [Fact]
    public async Task ProjectValidationErrorsHaveTranslatableMessages()
    {
        var project = StudioProject.CreateExample();
        project.Pages[1].Destination!.DefaultDirectory = "";
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => ProjectFile.SaveAsync("unused.issstudio", project));
        Assert.Equal("InvalidDirectory", ProjectFile.ErrorResourceKey(error));
        var text = new StudioLocalizer("en");
        Assert.StartsWith("Enter a default installation folder", text[ProjectFile.ErrorResourceKey(error)!]);
    }

    [Fact]
    public void ReplacedSessionUnsubscribesFromLanguageChanges()
    {
        var text = new StudioLocalizer();
        var editor = new EditorSession(StudioProject.CreateExample(), text);
        var notifications = 0;
        editor.PropertyChanged += (_, _) => notifications++;
        editor.Dispose();
        text.Language = "de";
        Assert.Equal(0, notifications);
    }

    [Fact]
    public void PreferencesRoundTripAndInvalidDataFallBackToDutch()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var path = Path.Combine(folder, "settings.json");
        try
        {
            Assert.Equal("nl", StudioPreferences.LoadLanguage(path));
            StudioPreferences.SaveLanguage(path, "de");
            Assert.Equal("de", StudioPreferences.LoadLanguage(path));
            File.WriteAllText(path, "{\"Language\":\"fr\"}");
            Assert.Equal("nl", StudioPreferences.LoadLanguage(path));
            File.WriteAllText(path, "broken");
            Assert.Equal("nl", StudioPreferences.LoadLanguage(path));
            File.WriteAllText(path, "null");
            Assert.Equal("nl", StudioPreferences.LoadLanguage(path));
            File.WriteAllText(path, new string(' ', 16_385));
            Assert.Equal("nl", StudioPreferences.LoadLanguage(path));
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }
}
