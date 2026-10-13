using System.Windows;
using System.Windows.Controls;
using Studio.Core;

namespace Studio.App;

public partial class ProjectLanguagesWindow : Window
{
    private readonly EditorSession _editor;
    private readonly string? _compiler;
    private readonly List<string> _additionalPaths;
    public ProjectLanguagesWindow(EditorSession editor, string? compiler, List<string> additionalPaths)
    {
        InitializeComponent();
        _editor = editor; _compiler = compiler; _additionalPaths = additionalPaths;
        Refresh();
    }
    private sealed record Choice(InstallerLanguage Language, EditorSession Editor)
    {
        public string Id => Language.Id;
        public string Name => Language.DisplayName(Editor.Text);
        public bool Enabled => Language.Enabled;
        public string Source => Language.Source;
        public string TextOrigin => (Id == Editor.DefaultLanguageId ? Editor.Text["DefaultInstallerLanguage"] + ". " : "")
            + Editor.Text.Format("TranslationLanguageStatus", WizardTexts.MissingCount(Editor.Project, Id));
    }
    private void Refresh(string? id = null)
    {
        id ??= (Languages.SelectedItem as Choice)?.Id;
        var choices = _editor.Project.Languages.Select(l => new Choice(l, _editor)).ToArray();
        Languages.ItemsSource = choices;
        Languages.SelectedItem = choices.FirstOrDefault(l => l.Id == id) ?? choices.FirstOrDefault();
    }
    private void Enabled_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: Choice choice } check)
        { _editor.SetLanguageEnabled(choice.Id, check.IsChecked == true); Refresh(choice.Id); }
    }
    private void Up_Click(object sender, RoutedEventArgs e) => Move(-1);
    private void Down_Click(object sender, RoutedEventArgs e) => Move(1);
    private void Move(int direction)
    {
        if (Languages.SelectedItem is not Choice choice) return;
        _editor.MoveLanguage(choice.Id, direction); Refresh(choice.Id);
    }
    private void Add_Click(object sender, RoutedEventArgs e)
    {
        new LanguageSourcesWindow(_compiler, _additionalPaths, _editor) { Owner = this }.ShowDialog();
        Refresh();
    }
}
