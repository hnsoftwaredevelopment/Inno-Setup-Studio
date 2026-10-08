using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Studio.Core;

namespace Studio.App;

public partial class LanguageSourcesWindow : Window
{
    private readonly string? _compiler;
    private readonly List<string> _additionalPaths;
    private bool _loading;
    private static StudioLocalizer Text => Localization.Current;

    public LanguageSourcesWindow(string? compiler, List<string> additionalPaths)
    {
        InitializeComponent();
        _compiler = compiler;
        _additionalPaths = additionalPaths;
        Loaded += async (_, _) => await ReloadAsync();
    }

    private async void Reload_Click(object sender, RoutedEventArgs e) => await ReloadAsync();
    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        var dialog = new OpenFileDialog { Title = Text["AddLanguageSource"], Filter = "Inno Setup (*.isl)|*.isl", CheckFileExists = true, Multiselect = true };
        if (dialog.ShowDialog(this) != true) return;
        foreach (var path in dialog.FileNames)
            if (!_additionalPaths.Contains(path, StringComparer.OrdinalIgnoreCase)) _additionalPaths.Add(path);
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (_loading) return;
        _loading = true;
        Actions.IsEnabled = false;
        Problems.Text = Text["LanguageSourcesLoading"];
        var selectedPath = (Sources.SelectedItem as SourceChoice)?.Source.FilePath;
        var sources = new List<LanguageSource>();
        var problems = new List<string>();
        try
        {
            if (_compiler is null) problems.Add(Text["CompilerMissing"]);
            else
            {
                try
                {
                    await CompilerService.CheckVersionAsync(_compiler);
                    var catalog = await LanguageSourceFile.DiscoverAsync(Path.GetDirectoryName(_compiler)!);
                    sources.AddRange(catalog.Sources);
                    problems.AddRange(catalog.Problems.Select(p => p.FilePath + Environment.NewLine + Text[p.ResourceKey]));
                }
                catch (Exception error) when (LanguageSourceFile.IsReadError(error) || error is System.ComponentModel.Win32Exception or TimeoutException)
                { problems.Add(Text["CompilerInvalid"]); }
            }
            foreach (var path in _additionalPaths)
            {
                if (sources.Any(s => string.Equals(s.FilePath, path, StringComparison.OrdinalIgnoreCase))) continue;
                try { sources.Add(await LanguageSourceFile.LoadAsync(path)); }
                catch (Exception error) when (LanguageSourceFile.IsReadError(error))
                { problems.Add(path + Environment.NewLine + Text[LanguageSourceFile.ErrorKey(error)]); }
            }
            var choices = LanguageSourceNames.PrepareList(sources, Text)
                .Select(source => new SourceChoice(source, LanguageSourceNames.GetName(source, Text))).ToArray();
            Sources.ItemsSource = choices;
            Sources.SelectedItem = choices.FirstOrDefault(s => s.Source.FilePath == selectedPath) ?? choices.FirstOrDefault();
            Problems.Text = problems.Count == 0 ? Text.Format("LanguageSourcesLoaded", choices.Length)
                : string.Join(Environment.NewLine + Environment.NewLine, problems);
        }
        finally { Actions.IsEnabled = true; _loading = false; }
    }

    private sealed record Entry(string Section, string Key, string Value);
    private sealed record SourceChoice(LanguageSource Source, string DisplayName)
    {
        public string NativeName => DisplayName == Source.Name ? "" : Source.Name;
    }
    private void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        var source = (Sources.SelectedItem as SourceChoice)?.Source;
        SourcePath.Text = source?.FilePath ?? "";
        SourceDetails.Text = source is null ? "" : Text.Format("LanguageSourceDetails", source.LanguageId.ToString("X4"),
            source.RightToLeft ? Text["LanguageSourceYes"] : Text["LanguageSourceNo"], source.Messages.Count, source.CustomMessages.Count);
        Entries.ItemsSource = source is null ? Array.Empty<Entry>() :
            source.Options.Select(p => new Entry("LangOptions", p.Key, p.Value))
                .Concat(source.Messages.Select(p => new Entry("Messages", p.Key, p.Value)))
                .Concat(source.CustomMessages.Select(p => new Entry("CustomMessages", p.Key, p.Value))).ToArray();
    }
}
