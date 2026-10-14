using System.Windows;
using System.Windows.Controls;
using Studio.Core;

namespace Studio.App;

public partial class WizardPageOverviewWindow : Window
{
    private readonly EditorSession _editor;
    private readonly string? _directory;
    public WizardPageOverviewWindow(EditorSession editor, string? directory)
    {
        _editor = editor; _directory = directory;
        InitializeComponent();
        DataContext = WizardPageOverview.Create(editor.Text, editor.Project);
        Pages.SelectedIndex = 0;
    }
    private void Pages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var canDesign = Pages.SelectedItem is WizardPageSummary row && (row.CanDesign || row.DocumentKind is not null) && _editor.IsDesign;
        Design.Content = _editor.Text[Pages.SelectedItem is WizardPageSummary { DocumentKind: not null } ? "DocumentConfigure" : "PageOverviewDesign"];
        Design.Visibility = canDesign ? Visibility.Visible : Visibility.Collapsed;
        Unavailable.Visibility = canDesign ? Visibility.Collapsed : Visibility.Visible;
    }
    private void Design_Click(object sender, RoutedEventArgs e)
    {
        if (!_editor.IsDesign) return;
        if (Pages.SelectedItem is WizardPageSummary { DocumentKind: { } kind })
        {
            var selected = Pages.SelectedIndex;
            new WizardDocumentWindow(_editor, kind, _directory) { Owner = this }.ShowDialog();
            DataContext = WizardPageOverview.Create(_editor.Text, _editor.Project);
            Pages.SelectedIndex = selected;
            return;
        }
        if (Pages.SelectedItem is not WizardPageSummary { DesignIndex: int index }) return;
        _editor.PageIndex = index;
        Close();
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
