using System.Text.Json;
using System.Text.Json.Serialization;

namespace Studio.Core;

public static class ProjectFile
{
    private const int MaxBytes = 1_048_576;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public static async Task<StudioProject> LoadAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        if (stream.Length > MaxBytes) throw InvalidProject("FileTooLarge");
        try
        {
            using var json = await JsonDocument.ParseAsync(stream);
            if (json.RootElement.ValueKind != JsonValueKind.Object
                || !json.RootElement.TryGetProperty("Format", out _)
                || !json.RootElement.TryGetProperty("Version", out _))
                throw InvalidProject("NotStudioProject");
            var root = json.RootElement;
            if (root.GetProperty("Version").ValueKind != JsonValueKind.Number
                || !root.GetProperty("Version").TryGetInt32(out var version)
                || version is not (1 or StudioProject.CurrentVersion))
                throw InvalidProject("UnsupportedProject");
            if (version == StudioProject.CurrentVersion
                && (!root.TryGetProperty("AppId", out _) || !root.TryGetProperty("AppVersion", out _)))
                throw InvalidProject("InvalidApplicationDetails");
            var project = root.Deserialize<StudioProject>(Options)!;
            if (version == 1)
            {
                // Derive identity from legacy content so reopening an unsaved migration is stable.
                var hash = System.Security.Cryptography.SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(root));
                project.AppId = new Guid(hash.AsSpan(0, 16)).ToString("B");
                project.AppVersion = "1.0";
                project.Version = StudioProject.CurrentVersion;
                project.WasMigrated = true;
            }
            Validate(project);
            return project;
        }
        catch (JsonException error)
        {
            throw InvalidProject("InvalidProject", error);
        }
    }

    public static async Task SaveAsync(string path, StudioProject project)
    {
        Validate(project);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(project, Options);
        if (bytes.Length > MaxBytes) throw InvalidProject("ProjectTooLarge");
        var temporaryPath = path + $".{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    // Keep a stable resource key alongside the diagnostic message for localized presentation.
    public static string? ErrorResourceKey(Exception error) => error.Data["Studio.ResourceKey"] as string;
    private static InvalidDataException InvalidProject(string key, Exception? inner = null)
    {
        var error = new InvalidDataException(new StudioLocalizer()[key], inner);
        error.Data["Studio.ResourceKey"] = key;
        return error;
    }

    private static void Validate(StudioProject? project)
    {
        if (project is null || project.Format != StudioProject.FormatId || project.Version != StudioProject.CurrentVersion)
            throw InvalidProject("UnsupportedProject");
        if (!IsSingleLine(project.Name) || !IsSingleLine(project.AppId) || project.AppId.Length > 127
            || !IsSingleLine(project.AppVersion))
            throw InvalidProject("InvalidApplicationDetails");
        if (string.IsNullOrWhiteSpace(project.Name) || project.Buttons is null || project.Buttons.Count != 3
            || new[] { ElementKind.Back, ElementKind.Next, ElementKind.Cancel }.Any(k => !project.Buttons.ContainsKey(k))
            || project.Buttons.Values.Any(v => v is null)
            || project.Pages is null || project.Pages.Count != 2)
            throw InvalidProject("MissingProjectData");
        for (var i = 0; i < project.Pages.Count; i++)
        {
            var page = project.Pages[i];
            if (page is null || page.Kind != (i == 0 ? PageKind.Welcome : PageKind.Destination)
                || string.IsNullOrWhiteSpace(page.Name) || page.Title is null || page.Body is null
                || page.ButtonOverrides is null
                || page.ButtonOverrides.Any(p => p.Key is not (ElementKind.Back or ElementKind.Next or ElementKind.Cancel) || p.Value is null))
                throw InvalidProject("InvalidPage");
            if (page.Kind == PageKind.Destination
                && (page.Destination is not { } settings || string.IsNullOrWhiteSpace(settings.DefaultDirectory)
                    || settings.DefaultDirectory.Any(char.IsControl) || settings.BrowseCaption is null || settings.BrowseTooltip is null))
                throw InvalidProject("InvalidDirectory");
        }
    }

    private static bool IsSingleLine(string? value) =>
        !string.IsNullOrWhiteSpace(value) && !value.Any(char.IsControl);
}
