using System.Windows.Data;
using System.Windows.Markup;
using Studio.Core;

namespace Studio.App;

public static class Localization
{
    public static StudioLocalizer Current { get; } = new(StudioPreferences.LoadLanguage(StudioPreferences.DefaultPath));
}

/// <summary>A live WPF binding to a Studio resource; never used for installer content.</summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class LocExtension(string key) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new Binding($"[{key}]") { Source = Localization.Current, Mode = BindingMode.OneWay }.ProvideValue(serviceProvider);
}
