namespace Studio.Core;

public static class InstallerOutput
{
    public static bool IsValid(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120 || name != name.Trim()
            || name.EndsWith('.') || name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            || name.Any(char.IsControl) || name.IndexOfAny(['<', '>', ':', '"', '/', '\\', '|', '?', '*', '{', '}']) >= 0)
            return false;
        var stem = name.Split('.')[0].TrimEnd(' ').ToUpperInvariant();
        return stem is not ("CON" or "PRN" or "AUX" or "NUL")
            && !(stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal))
                && (stem[3] is >= '1' and <= '9' || stem[3] is '¹' or '²' or '³'));
    }

    public static bool IsSetup(string name) => string.Equals(name, "setup", StringComparison.OrdinalIgnoreCase);
}
