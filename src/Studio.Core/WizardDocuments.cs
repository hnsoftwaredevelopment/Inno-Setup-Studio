namespace Studio.Core;
public enum WizardDocumentKind { LicenseFile, InfoBeforeFile, InfoAfterFile }
public sealed class WizardDocumentSource
{
    public string Source { get; set; } = "";
    public Dictionary<string,string> Translations { get; set; } = [];
}
public static class WizardDocuments
{
    public static Dictionary<WizardDocumentKind,WizardDocumentSource> Empty() => Enum.GetValues<WizardDocumentKind>().ToDictionary(k=>k,_=>new WizardDocumentSource());
    public static string Source(StudioProject project,WizardDocumentKind kind,string languageId)
    {
        var document=project.Documents[kind];
        return string.IsNullOrWhiteSpace(document.Source) ? "" : document.Translations.TryGetValue(languageId,out var variant) && !string.IsNullOrWhiteSpace(variant) ? variant : document.Source;
    }
    public static string Resolve(string source,string? directory)
    {
        if (source.IndexOfAny(['{','}','"'])>=0 || source.Any(char.IsControl)
            || Path.GetExtension(source).ToLowerInvariant() is not (".txt" or ".rtf")) throw InnoScript.Error("DocumentInvalid");
        try { return InnoScript.ResolveSource(source,directory); }
        catch (InvalidDataException error) { error.Data["Studio.DocumentPath"] = source; throw; }
    }
}
