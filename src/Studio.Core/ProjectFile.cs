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
                || version is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or StudioProject.CurrentVersion))
                throw InvalidProject("UnsupportedProject");
            if (version >= 2
                && (!root.TryGetProperty("AppId", out _) || !root.TryGetProperty("AppVersion", out _)))
                throw InvalidProject("InvalidApplicationDetails");
            if (version >= 3 && !root.TryGetProperty("InstallFile", out _))
                throw InvalidProject("InvalidFileRule");
            if (version < 3 && root.TryGetProperty("InstallFile", out _))
                throw InvalidProject("UnsupportedProject");
            if(version>=10 && !root.TryGetProperty("AdditionalFiles",out _)) throw InvalidProject("InvalidFileRule");
            if(version<10 && root.TryGetProperty("AdditionalFiles",out _)) throw InvalidProject("UnsupportedProject");
            if (version >= 4 && !root.TryGetProperty("OutputBaseFileName", out _))
                throw InvalidProject("InvalidOutputName");
            if (version < 4 && root.TryGetProperty("OutputBaseFileName", out _))
                throw InvalidProject("UnsupportedProject");
            if (version >= 5 && (!root.TryGetProperty("Languages", out _) || !root.TryGetProperty("TextLanguageId", out _)))
                throw InvalidProject("InvalidProjectLanguages");
            if (version < 5 && (root.TryGetProperty("Languages", out _) || root.TryGetProperty("TextLanguageId", out _)))
                throw InvalidProject("UnsupportedProject");
            if (version >= 6 && root.GetProperty("Languages").ValueKind == JsonValueKind.Array
                && root.GetProperty("Languages").EnumerateArray().Any(l => l.ValueKind != JsonValueKind.Object
                    || !l.TryGetProperty("TextOverrides", out _) || !l.TryGetProperty("StandardTexts", out _)))
                throw InvalidProject("InvalidProjectLanguages");
            if (version == 5 && root.GetProperty("Languages").ValueKind == JsonValueKind.Array
                && root.GetProperty("Languages").EnumerateArray().Any(l => l.ValueKind == JsonValueKind.Object
                    && (l.TryGetProperty("TextOverrides", out _) || l.TryGetProperty("StandardTexts", out _))))
                throw InvalidProject("UnsupportedProject");
            if (version >= 7 && !root.TryGetProperty("Documents", out _)) throw InvalidProject("DocumentInvalid");
            if (version < 7 && root.TryGetProperty("Documents", out _)) throw InvalidProject("UnsupportedProject");
            if (version >= 8 && !root.TryGetProperty("PageSettings", out _)) throw InvalidProject("PageSettingsInvalid");
            if (version < 8 && root.TryGetProperty("PageSettings", out _)) throw InvalidProject("UnsupportedProject");
            if (version >= 9 && root.GetProperty("Documents").ValueKind == JsonValueKind.Object
                && root.GetProperty("Documents").EnumerateObject().Any(d => d.Value.ValueKind != JsonValueKind.Object
                    || !d.Value.TryGetProperty("Enabled", out _))) throw InvalidProject("DocumentInvalid");
            var project = root.Deserialize<StudioProject>(Options)!;
            if (version == 1)
            {
                // Derive identity from legacy content so reopening an unsaved migration is stable.
                var hash = System.Security.Cryptography.SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(root));
                project.AppId = new Guid(hash.AsSpan(0, 16)).ToString("B");
                project.AppVersion = "1.0";
            }
            if (version < StudioProject.CurrentVersion)
            {
                if (version < 3) project.InstallFile = new();
                if (version < 4) project.OutputBaseFileName = "setup";
                if (version < 5)
                {
                    project.TextLanguageId = "dutch";
                    project.Languages = [InstallerLanguage.Dutch()];
                }
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

    public static async Task SaveAsync(string path, StudioProject project, string? originalProjectDirectory = null)
    {
        string Rebase(string value) => !string.IsNullOrWhiteSpace(value) && !Path.IsPathRooted(value) && originalProjectDirectory is not null
            ? Path.GetRelativePath(Path.GetDirectoryName(Path.GetFullPath(path))!, Path.GetFullPath(value, originalProjectDirectory)) : value;
        Validate(project);
        var source = project.InstallFile.Source;
        var storedSource = source;
        if (!string.IsNullOrWhiteSpace(source) && !Path.IsPathRooted(source) && originalProjectDirectory is not null)
        {
            var originalSource = Path.GetFullPath(source, originalProjectDirectory);
            storedSource = Path.GetRelativePath(Path.GetDirectoryName(Path.GetFullPath(path))!, originalSource);
        }
        var json = JsonSerializer.SerializeToNode(project, Options)!;
        json["InstallFile"]!["Source"] = storedSource;
        var additional=project.AdditionalFiles.Select(f=>new InstallFileSettings
        {
            Source=Rebase(f.Source),Destination=f.Destination,RecurseSubdirectories=f.RecurseSubdirectories,Excludes=[..f.Excludes]
        }).ToList();
        json["AdditionalFiles"]=JsonSerializer.SerializeToNode(additional,Options);
        var documents = project.Documents.ToDictionary(pair => pair.Key, pair => new WizardDocumentSource
        {
            Enabled = pair.Value.Enabled,
            Source = Rebase(pair.Value.Source),
            Translations = pair.Value.Translations.ToDictionary(v => v.Key, v => Rebase(v.Value))
        });
        json["Documents"] = JsonSerializer.SerializeToNode(documents, Options);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(json, Options);
        if (bytes.Length > MaxBytes) throw InvalidProject("ProjectTooLarge");
        var temporaryPath = path + $".{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes);
            File.Move(temporaryPath, path, overwrite: true);
            project.InstallFile.Source = storedSource;
            project.AdditionalFiles=additional;
            project.Documents = documents;
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

    internal static void Validate(StudioProject? project)
    {
        if (project is null || project.Format != StudioProject.FormatId || project.Version != StudioProject.CurrentVersion)
            throw InvalidProject("UnsupportedProject");
        if (!IsSingleLine(project.Name) || !IsSingleLine(project.AppId) || project.AppId.Length > 127
            || !IsSingleLine(project.AppVersion))
            throw InvalidProject("InvalidApplicationDetails");
        if (!InstallerOutput.IsValid(project.OutputBaseFileName)) throw InvalidProject("InvalidOutputName");
        if (project.Languages is null || project.Languages.Count is < 1 or > 100
            || project.Languages.Any(l => l is null || !IsSingleLine(l.Id) || !IsSingleLine(l.Name)
                || !IsSingleLine(l.Source) || l.LanguageId is < 0 or > 65535 || l.Messages is null
                || l.TextOverrides is null || l.StandardTexts is null
                || l.TextOverrides.Any(p => !Enum.IsDefined(p.Key) || p.Value is null)
                || l.StandardTexts.Any(s => !Enum.IsDefined(s))
                || l.Messages.Any(p => !InstallerLanguage.PreviewKeys.Contains(p.Key) || p.Value is null))
            || project.Languages.Select(l => l.Id).Distinct(StringComparer.Ordinal).Count() != project.Languages.Count
            || !project.Languages.Any(l => l.Enabled) || !project.Languages.Any(l => l.Id == project.TextLanguageId))
            throw InvalidProject("InvalidProjectLanguages");
        if (project.PageSettings is not { } pageSettings || !Enum.IsDefined(pageSettings.GroupMode)
            || new[] { pageSettings.DefaultName, pageSettings.DefaultOrganization, pageSettings.DefaultGroup }
                .Any(value => value is null || value.Length > 4096 || value.Any(char.IsControl)))
            throw InvalidProject("PageSettingsInvalid");
        if (project.Documents is null || project.Documents.Count != 3
            || Enum.GetValues<WizardDocumentKind>().Any(kind => !project.Documents.ContainsKey(kind))
            || project.Documents.Values.Any(d => d is null || d.Source is null || d.Source.Any(char.IsControl)
                || d.Translations is null || d.Translations.Any(v => !project.Languages.Any(l => l.Id == v.Key)
                    || v.Value is null || v.Value.Any(char.IsControl)))) throw InvalidProject("DocumentInvalid");
        if(project.AdditionalFiles is null || project.AdditionalFiles.Count>999) throw InvalidProject("InvalidFileRule");
        foreach(var file in FileCollections.Rules(project)) FileCollections.Validate(file);
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
