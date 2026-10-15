namespace Studio.Core;

internal sealed record ExportLanguage(InstallerLanguage Language, string Name, string MessagesFile);

/// <summary>Exports only enabled sources; project IDs never become script syntax.</summary>
internal static class LanguageScript
{
    public static IReadOnlyList<ExportLanguage> Create(StudioProject project, string? projectDirectory) =>
        project.Languages.Where(language => language.Enabled).Select((language, index) =>
        {
            try
            {
                return new ExportLanguage(language, language.Id is "english" or "dutch" ? language.Id : "studio_language_" + (index + 1),
                    Source(language.Source, projectDirectory));
            }
            catch (InvalidDataException error)
            {
                error.Data["Studio.LanguageId"] = language.Id;
                throw;
            }
        }).ToArray();

    internal static string Source(string source, string? projectDirectory)
    {
        if (source.Any(char.IsControl) || source.IndexOfAny(['"', ',', '{', '}', '*', '?', '<', '>', '|']) >= 0
            || !string.Equals(Path.GetExtension(source), ".isl", StringComparison.OrdinalIgnoreCase))
            throw InnoScript.Error("ExportLanguageSourceInvalid");
        if (source.StartsWith("compiler:", StringComparison.OrdinalIgnoreCase))
        {
            var relative = source[9..].Replace('/', '\\');
            if (Path.IsPathRooted(relative) || relative.Contains(':') || relative.Split('\\').Any(part => part is ".." or "." or ""))
                throw InnoScript.Error("ExportLanguageSourceInvalid");
            return "compiler:" + relative;
        }
        if (Path.IsPathRooted(source) && !Path.IsPathFullyQualified(source)) throw InnoScript.Error("ExportLanguageSourceInvalid");
        if (!Path.IsPathFullyQualified(source) && string.IsNullOrWhiteSpace(projectDirectory))
            throw InnoScript.Error("ExportLanguageSourceNeedsProject");
        string resolved;
        try
        {
            resolved = Path.IsPathFullyQualified(source) ? Path.GetFullPath(source) : Path.GetFullPath(source, projectDirectory!);
        }
        catch (ArgumentException) { throw InnoScript.Error("ExportLanguageSourceInvalid"); }
        if (!File.Exists(resolved)) throw InnoScript.Error("ExportLanguageSourceMissing");
        return resolved;
    }
}
