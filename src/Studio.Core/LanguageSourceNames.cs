namespace Studio.Core;

/// <summary>Editor-only names; native LanguageName and installer messages remain untouched.</summary>
public static class LanguageSourceNames
{
    public static string ResourceKey(int languageId) => "InstallerLanguageName_" + languageId.ToString("X4", System.Globalization.CultureInfo.InvariantCulture);
    public static string GetName(LanguageSource source, StudioLocalizer text)
    {
        var key = ResourceKey(source.LanguageId);
        var translated = text[key];
        return translated == "[" + key + "]" ? source.Name : translated;
    }

    public static IReadOnlyList<LanguageSource> PrepareList(IEnumerable<LanguageSource> sources, StudioLocalizer text)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var comparer = StringComparer.Create(text.Culture, ignoreCase: true);
        return sources.Where(source => seen.Add(ContentKey(source)))
            .OrderBy(source => GetName(source, text), comparer)
            .ThenBy(source => source.Name, comparer)
            .ThenBy(source => source.FilePath, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string ContentKey(LanguageSource source)
    {
        // Compare parsed content, so copies with another encoding or line ending are also identical.
        static object Entries(IReadOnlyDictionary<string, string> values) => values
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new { Key = pair.Key.ToUpperInvariant(), pair.Value }).ToArray();
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            Options = Entries(source.Options), Messages = Entries(source.Messages), CustomMessages = Entries(source.CustomMessages),
            source.Name, source.LanguageId, source.RightToLeft
        });
    }
}
