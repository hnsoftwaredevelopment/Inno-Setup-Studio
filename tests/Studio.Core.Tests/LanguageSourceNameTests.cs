using Studio.Core;

namespace Studio.Core.Tests;

public class LanguageSourceNameTests
{
    private static LanguageSource Source(int id, string native) => new("test.isl", native, id, false,
        new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());

    [Theory]
    [InlineData("nl", "Koreaans")]
    [InlineData("en", "Korean")]
    [InlineData("de", "Koreanisch")]
    public void KoreanNameFollowsStudioLanguage(string language, string expected) =>
        Assert.Equal(expected, LanguageSourceNames.GetName(Source(0x412, "한국어"), new StudioLocalizer(language)));

    [Fact]
    public void RegionalVariantsAreDistinctAndUnknownLanguagesKeepNativeName()
    {
        var text = new StudioLocalizer("nl");
        Assert.NotEqual(LanguageSourceNames.GetName(Source(0x804, "简体中文"), text), LanguageSourceNames.GetName(Source(0x404, "繁體中文"), text));
        Assert.Equal("Eigen taal", LanguageSourceNames.GetName(Source(0, "Eigen taal"), text));
    }

    [InnoCompilerFact]
    public async Task EveryInstalledLanguageHasNamesInAllStudioLanguages()
    {
        var catalog = await LanguageSourceFile.DiscoverAsync(Path.GetDirectoryName(InnoCompilerFactAttribute.CompilerPath)!);
        foreach (var language in new[] { "nl", "en", "de" })
            foreach (var source in catalog.Sources)
                Assert.False(new StudioLocalizer(language)[LanguageSourceNames.ResourceKey(source.LanguageId)].StartsWith('['));
    }

    [Fact]
    public void ListUsesTranslatedAlphabeticalOrder()
    {
        var german = Source(0x407, "Deutsch");
        var english = Source(0x409, "English");
        var dutch = Source(0x413, "Nederlands");
        var input = new[] { dutch, english, german };
        Assert.Equal(new[] { german, english, dutch }, LanguageSourceNames.PrepareList(input, new StudioLocalizer("nl")));
        Assert.Equal(new[] { dutch, english, german }, LanguageSourceNames.PrepareList(input, new StudioLocalizer("en")));
    }

    [Fact]
    public void IdenticalCopyIsHiddenButModifiedLanguageRemainsSeparate()
    {
        var original = Source(0x413, "Nederlands");
        var copy = original with { FilePath = "elsewhere/Dutch.isl" };
        var changed = copy with { Name = "Mijn taal", LanguageId = 0 };
        var changedText = copy with { Messages = new Dictionary<string, string> { ["WelcomeLabel1"] = "Eigen tekst" } };
        var list = LanguageSourceNames.PrepareList(new[] { original, copy, changed, changedText }, new StudioLocalizer("nl"));
        Assert.Equal(3, list.Count);
        Assert.Contains(original, list);
        Assert.Contains(changed, list);
        Assert.Contains(changedText, list);
        Assert.DoesNotContain(copy, list);
    }
}
