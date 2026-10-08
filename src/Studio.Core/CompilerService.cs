using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Studio.Core;

public sealed record CompilerBuildResult(string Directory, string ScriptPath, string? InstallerPath,
    int ExitCode, string Log, string Version);

public static class CompilerService
{
    public static string SettingsPath => Path.Combine(Path.GetDirectoryName(StudioPreferences.DefaultPath)!, "compiler.json");

    public static string? Discover()
    {
        foreach (var root in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs") })
        {
            var candidate = Path.Combine(root, "Inno Setup 7", "ISCC.exe");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    public static string? LoadLocation(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return stream.Length > 16_384 ? null : JsonSerializer.Deserialize<string>(stream);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) { return null; }
    }

    public static void SaveLocation(string path, string compiler)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(Path.GetFullPath(compiler)));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    public static bool IsSupported(string version) => System.Version.TryParse(version.Trim(), out var parsed)
        && parsed.Major == 7 && parsed.Minor == 1;

    public static async Task<string> CheckVersionAsync(string compiler)
    {
        var result = await RunAsync(compiler, ["--version"], TimeSpan.FromSeconds(10));
        var version = result.Output.Trim();
        if (result.ExitCode != 0 || !IsSupported(version))
        {
            var error = new InvalidDataException(new StudioLocalizer()["CompilerInvalid"]);
            error.Data["Studio.ResourceKey"] = "CompilerInvalid";
            error.Data["CompilerVersion"] = version;
            throw error;
        }
        return version;
    }

    public static async Task<CompilerBuildResult> BuildAsync(string compiler, string script, string projectDirectory)
    {
        var version = await CheckVersionAsync(compiler);
        var directory = Path.Combine(Path.GetFullPath(projectDirectory), ".studio-builds",
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var scriptPath = Path.Combine(directory, "installer.iss");
        await File.WriteAllTextAsync(scriptPath, script, new UTF8Encoding(true));
        var result = await RunAsync(compiler, ["/O" + directory, scriptPath], TimeSpan.FromMinutes(30));
        var installer = Path.Combine(directory, "setup.exe");
        return new(directory, scriptPath, result.ExitCode == 0 && File.Exists(installer) ? installer : null,
            result.ExitCode, result.Output, version);
    }

    private static async Task<(int ExitCode, string Output)> RunAsync(string compiler, string[] arguments, TimeSpan limit)
    {
        if (!Path.IsPathFullyQualified(compiler) || !File.Exists(compiler))
            throw new FileNotFoundException("Compiler not found.", compiler);
        var start = new ProcessStartInfo(compiler)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(compiler)!
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var errors = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(limit);
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            await Task.WhenAll(output, errors);
            throw new TimeoutException("Compiler timed out.");
        }
        return (process.ExitCode, await output + await errors);
    }
}
