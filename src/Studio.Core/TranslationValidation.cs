using System.Text.RegularExpressions;

namespace Studio.Core;

public sealed record TranslationIssue(string ResourceKey, string Argument)
{
    public string Describe(StudioLocalizer text) => text.Format(ResourceKey, Argument);
}

/// <summary>Non-destructive checks for Studio references in raw custom text only.</summary>
public static class TranslationValidation
{
    private static readonly Regex References = new(@"\{AppName\}|\{AppVersion\}", RegexOptions.CultureInvariant);

    public static IReadOnlyList<TranslationIssue> Check(StudioProject project, string languageId, WizardTextSlot slot)
    {
        var language = project.Languages.Single(l => l.Id == languageId);
        var own = WizardTexts.Own(project, language, slot);
        if (string.IsNullOrWhiteSpace(own)) return [];
        var issues = new List<TranslationIssue>();
        var opening = -1;
        for (var i = 0; i < own.Length; i++)
        {
            switch (own[i])
            {
                case '{':
                    if (opening >= 0) Malformed(own[opening..i]);
                    opening = i;
                    break;
                case '}':
                    if (opening < 0) Malformed("}");
                    else
                    {
                        var token = own[opening..(i + 1)];
                        if (token is not ("{AppName}" or "{AppVersion}"))
                            issues.Add(new("TranslationReferenceUnknown", Short(token)));
                    }
                    opening = -1;
                    break;
                case '\r' or '\n' when opening >= 0:
                    Malformed(own[opening..i]);
                    opening = -1;
                    break;
            }
        }
        if (opening >= 0) Malformed(own[opening..]);
        var fallback = project.Languages.First(l => l.Enabled);
        if (fallback.Id != languageId)
        {
            var reference = WizardTexts.Own(project, fallback, slot);
            if (string.IsNullOrWhiteSpace(reference) && WizardTexts.Shared(slot) is { } shared)
                reference = WizardTexts.Own(project, fallback, shared);
            var used = References.Matches(own).Select(m => m.Value).ToHashSet(StringComparer.Ordinal);
            foreach (var required in References.Matches(reference ?? "").Select(m => m.Value).Distinct())
                if (!used.Contains(required)) issues.Add(new("TranslationReferenceMissing", required));
        }
        return issues.Distinct().ToArray();

        void Malformed(string fragment) => issues.Add(new("TranslationReferenceMalformed", Short(fragment)));
    }

    private static string Short(string value) => value.Length > 64 ? value[..64] + "…" : value;
}
