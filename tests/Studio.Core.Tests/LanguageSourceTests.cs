using System.Text;
using Studio.Core;

namespace Studio.Core.Tests;

public class LanguageSourceTests : IDisposable
{
    private readonly string _folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Studio-language-" + Guid.NewGuid().ToString("N"))).FullName;
    private const string Source = "[LangOptions]\r\nLanguageName=Deutsch\r\nLanguageID=$0407\r\nLanguageCodePage=1252\r\n[Messages]\r\nWelcomeLabel1=Hallo [name]\r\nWelcomeLabel2=Zeile %n Nummer %1; Text=bleibt\r\nEmpty=\r\n[CustomMessages]\r\nMine=Grüße";

    [Fact]
    public async Task SourcesAreReadOnlyAndMessagesKeepNativePlaceholders()
    {
        var path = Path.Combine(_folder, "German.isl");
        await File.WriteAllTextAsync(path, Source, new UTF8Encoding(true));
        var before = await File.ReadAllBytesAsync(path);
        var source = await LanguageSourceFile.LoadAsync(path);
        Assert.Equal("Deutsch", source.Name);
        Assert.Equal(0x407, source.LanguageId);
        Assert.Equal("Zeile %n Nummer %1; Text=bleibt", source.Messages["WelcomeLabel2"]);
        Assert.Equal("", source.Messages["Empty"]);
        Assert.Equal("Grüße", source.CustomMessages["Mine"]);
        Assert.Equal(before, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task ExplicitLegacyCodePageIsSupported()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var path = Path.Combine(_folder, "German.isl");
        await File.WriteAllBytesAsync(path, Encoding.GetEncoding(1252).GetBytes(Source));
        Assert.Equal("Grüße", (await LanguageSourceFile.LoadAsync(path)).CustomMessages["Mine"]);
    }

    [Theory]
    [InlineData("[LangOptions]\nLanguageName=Test\nLanguageID=not valid\n[Messages]\nText=x")]
    [InlineData("[Messages]\nText=x")]
    [InlineData("#include \"Other.isl\"\n" + Source)]
    [InlineData("[Setup]\nAppName=Not a language")]
    public async Task InvalidOrExecutableInputIsRejected(string content)
    {
        var path = Path.Combine(_folder, "bad.isl");
        await File.WriteAllTextAsync(path, content);
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => LanguageSourceFile.LoadAsync(path));
        Assert.NotNull(ProjectFile.ErrorResourceKey(error));
    }

    [Fact]
    public async Task CatalogKeepsGoodSourcesAndReportsMissingOrBrokenFilesSeparately()
    {
        await File.WriteAllTextAsync(Path.Combine(_folder, "Default.isl"), Source.Replace("Deutsch", "English"));
        var languages = Directory.CreateDirectory(Path.Combine(_folder, "Languages")).FullName;
        await File.WriteAllTextAsync(Path.Combine(languages, "German.isl"), Source);
        await File.WriteAllTextAsync(Path.Combine(languages, "Broken.isl"), "broken");
        var catalog = await LanguageSourceFile.DiscoverAsync(_folder);
        Assert.Equal(2, catalog.Sources.Count);
        Assert.Single(catalog.Problems);
        File.Delete(Path.Combine(_folder, "Default.isl"));
        var changed = await LanguageSourceFile.DiscoverAsync(_folder);
        Assert.Single(changed.Sources);
        Assert.Equal(2, changed.Problems.Count);
    }

    [Fact]
    public async Task OversizedFileIsRejected()
    {
        var path = Path.Combine(_folder, "large.isl");
        await File.WriteAllBytesAsync(path, new byte[1_048_577]);
        Assert.Equal("LanguageSourceTooLarge", ProjectFile.ErrorResourceKey(await Assert.ThrowsAsync<InvalidDataException>(() => LanguageSourceFile.LoadAsync(path))));
    }

    [InnoCompilerFact]
    public async Task InstalledCompilerLanguagesAreAllReadableAndRemainUnchanged()
    {
        var folder = Path.GetDirectoryName(InnoCompilerFactAttribute.CompilerPath)!;
        var paths = Directory.GetFiles(Path.Combine(folder, "Languages"), "*.isl").Prepend(Path.Combine(folder, "Default.isl")).ToArray();
        var before = paths.ToDictionary(p => p, p => System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(p)));
        var catalog = await LanguageSourceFile.DiscoverAsync(folder);
        Assert.Empty(catalog.Problems);
        Assert.Equal(paths.Length, catalog.Sources.Count);
        Assert.Contains(catalog.Sources, s => s.Name == "English");
        Assert.Contains(catalog.Sources, s => s.Name == "Nederlands");
        Assert.Contains(catalog.Sources, s => s.Name == "Deutsch");
        foreach (var path in paths) Assert.Equal(before[path], System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)));
    }

    public void Dispose() => Directory.Delete(_folder, true);
}
