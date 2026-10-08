using System.Text.Json;

namespace Studio.Core;

public enum CompilerMessageSeverity { Information, Warning, Error }

public sealed record CompilerMessage(CompilerMessageSeverity Severity, string Message, string? FileName, int? Line, string Raw)
{
    public static CompilerMessage Parse(string text)
    {
        try
        {
            using var json = JsonDocument.Parse(text);
            var root = json.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String && root.TryGetProperty("severity", out var severity)
                && severity.ValueKind == JsonValueKind.String)
            {
                var kind = severity.GetString() switch { "warning" => CompilerMessageSeverity.Warning,
                    "error" => CompilerMessageSeverity.Error, _ => CompilerMessageSeverity.Information };
                var file = root.TryGetProperty("filename", out var filename) && filename.ValueKind == JsonValueKind.String ? filename.GetString() : null;
                int? number = root.TryGetProperty("line", out var line) && line.ValueKind == JsonValueKind.Number && line.TryGetInt32(out var parsed) ? parsed : null;
                return new(kind, message.GetString()!, file, number, text);
            }
        }
        catch (JsonException) { }
        var fallback = text.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ? CompilerMessageSeverity.Error
            : text.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase) ? CompilerMessageSeverity.Warning : CompilerMessageSeverity.Information;
        return new(fallback, text, null, null, text);
    }
}
