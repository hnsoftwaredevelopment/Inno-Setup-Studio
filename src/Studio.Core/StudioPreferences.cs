using System.Text.Json;

namespace Studio.Core;

public sealed record StudioPreferences(string Language)
{
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HNSoftwareDevelopment", "Inno Setup Studio", "settings.json");

    public static string LoadLanguage(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            if (stream.Length > 16_384) return "nl";
            return StudioLocalizer.NormalizeLanguage(JsonSerializer.Deserialize<StudioPreferences>(stream)?.Language);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        {
            return "nl";
        }
    }

    public static void SaveLanguage(string path, string language)
    {
        path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + $".{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(new StudioPreferences(StudioLocalizer.NormalizeLanguage(language))));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
    }
}
