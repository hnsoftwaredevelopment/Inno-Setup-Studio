using System.Windows;
using System.Windows.Controls;
using Studio.Core;

namespace Studio.App;

public partial class WizardPageOverviewWindow : Window
{
    private readonly EditorSession _editor;
    public WizardPageOverviewWindow(EditorSession editor)
    {
        _editor = editor;
        InitializeComponent();
        DataContext = WizardPageOverview.Create(editor.Text);
        Pages.SelectedIndex = 0;
    }
    private void Pages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var canDesign = Pages.SelectedItem is WizardPageSummary { CanDesign: true } && _editor.IsDesign;
        Design.Visibility = canDesign ? Visibility.Visible : Visibility.Collapsed;
        Unavailable.Visibility = canDesign ? Visibility.Collapsed : Visibility.Visible;
    }
    private void Design_Click(object sender, RoutedEventArgs e)
    {
        if (!_editor.IsDesign || Pages.SelectedItem is not WizardPageSummary { DesignIndex: int index }) return;
        _editor.PageIndex = index;
        Close();
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
