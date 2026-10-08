using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Studio.Core;

public sealed record StudioLanguage(string Code, string Name);

/// <summary>Localizes the editor only. Installer content belongs to StudioProject.</summary>
public sealed class StudioLocalizer : INotifyPropertyChanged
{
    private static readonly ResourceManager Resources = new("Studio.Core.Resources.Strings", typeof(StudioLocalizer).Assembly);
    private string _language;

    public StudioLocalizer(string? language = null) => _language = NormalizeLanguage(language);
    public static IReadOnlyList<StudioLanguage> Languages { get; } =
        [new("nl", "Nederlands"), new("en", "English"), new("de", "Deutsch")];
    public event PropertyChangedEventHandler? PropertyChanged;
    public CultureInfo Culture => CultureInfo.GetCultureInfo(Language);
    public string Language
    {
        get => _language;
        set
        {
            var language = NormalizeLanguage(value);
            if (_language == language) return;
            _language = language;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
    public string this[string key] => Resources.GetString(key, Culture) ?? $"[{key}]";
    public string Format(string key, params object[] args) => string.Format(Culture, this[key], args);
    public static string NormalizeLanguage(string? language) => language is "en" or "de" ? language : "nl";
}
