using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace Studio.Core;

public sealed record CompilerBuildResult(string Directory, string ScriptPath, string? InstallerPath,
    int ExitCode, string Log, string Version)
{
    public bool Canceled { get; init; }
    public IReadOnlyList<CompilerMessage> Messages { get; init; } = [];
}

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

    public static async Task<string> CheckVersionAsync(string compiler, CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(compiler, ["--version"], TimeSpan.FromSeconds(10), null, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
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

    public static async Task<CompilerBuildResult> BuildAsync(string compiler, string script, string projectDirectory,
        string outputBaseName = "mysetup", IProgress<CompilerMessage>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!InstallerOutput.IsValid(outputBaseName)) throw new ArgumentException("Invalid installer output name.", nameof(outputBaseName));
        var version = await CheckVersionAsync(compiler, cancellationToken);
        var directory = Path.Combine(Path.GetFullPath(projectDirectory), ".studio-builds",
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var scriptPath = Path.Combine(directory, "installer.iss");
        await File.WriteAllTextAsync(scriptPath, script, new UTF8Encoding(true));
        var result = await RunAsync(compiler, ["/MJ", "/O" + directory, "/F" + outputBaseName, scriptPath], TimeSpan.FromMinutes(30), progress, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(directory, "compiler.log"), result.Output, new UTF8Encoding(true));
        var installer = Path.Combine(directory, outputBaseName + ".exe");
        return new(directory, scriptPath, !result.Canceled && result.ExitCode == 0 && File.Exists(installer) ? installer : null,
            result.ExitCode, result.Output, version) { Canceled = result.Canceled, Messages = result.Messages };
    }

    private sealed record ProcessResult(int ExitCode, string Output, bool Canceled, IReadOnlyList<CompilerMessage> Messages);

    private static async Task<ProcessResult> RunAsync(string compiler, string[] arguments, TimeSpan limit,
        IProgress<CompilerMessage>? progress, CancellationToken cancellationToken)
    {
        if (!Path.IsPathFullyQualified(compiler) || !File.Exists(compiler))
            throw new FileNotFoundException("Compiler not found.", compiler);
        var start = new ProcessStartInfo(compiler)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(compiler)!
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        cancellationToken.ThrowIfCancellationRequested();
        using var process = Process.Start(start)!;
        var messages = new ConcurrentQueue<CompilerMessage>();
        var output = ReadAsync(process.StandardOutput);
        var errors = ReadAsync(process.StandardError);
        using var timeout = new CancellationTokenSource(limit);
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
        var canceled = false;
        try { await process.WaitForExitAsync(combined.Token); }
        catch (OperationCanceledException)
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
            await process.WaitForExitAsync();
            await Task.WhenAll(output, errors);
            if (!cancellationToken.IsCancellationRequested) throw new TimeoutException("Compiler timed out.");
            canceled = true;
        }
        await Task.WhenAll(output, errors);
        canceled |= cancellationToken.IsCancellationRequested;
        var captured = messages.ToArray();
        return new(process.ExitCode, string.Join(Environment.NewLine, captured.Select(m => m.Raw)), canceled, captured);

        async Task ReadAsync(StreamReader stream)
        {
            while (await stream.ReadLineAsync() is { } line)
            {
                var message = CompilerMessage.Parse(line);
                messages.Enqueue(message);
                progress?.Report(message);
            }
        }
    }
}
