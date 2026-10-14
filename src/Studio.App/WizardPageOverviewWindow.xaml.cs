using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        var canDesign = Pages.SelectedItem is WizardPageSummary row && (row.CanDesign || row.SettingsSection == PageSettingsSection.ProgramGroup) && _editor.IsDesign;
        Design.Content = _editor.Text[Pages.SelectedItem is WizardPageSummary { DocumentKind: not null } ? "DocumentConfigure"
            : Pages.SelectedItem is WizardPageSummary { SettingsSection: not null } ? "PageConfigure" : "PageOverviewDesign"];
        Design.Visibility = canDesign ? Visibility.Visible : Visibility.Collapsed;
        Unavailable.Visibility = canDesign || Pages.SelectedItem is WizardPageSummary { CanToggle: true } ? Visibility.Collapsed : Visibility.Visible;
    }
    private void PageToggle_Click(object sender, RoutedEventArgs e)
    {
        if (!_editor.IsDesign || sender is not CheckBox { DataContext: WizardPageSummary row } toggle || !row.CanToggle) return;
        Pages.SelectedItem = row;
        var enabled = toggle.IsChecked == true;
        _editor.SetWizardPageEnabled(row.Id, enabled);
        if (enabled) OpenSettings(row);
        RefreshRows(row.Order - 1);
        e.Handled = true;
    }
    private void Pages_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock { DataContext: WizardPageSummary { CanToggle: true, IsOn: true } row })
        {
            OpenSettings(row); RefreshRows(row.Order - 1); e.Handled = true;
        }
    }
    private void Pages_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2 && Pages.SelectedItem is WizardPageSummary { CanToggle: true, IsOn: true } row)
        {
            OpenSettings(row); RefreshRows(row.Order - 1); e.Handled = true;
        }
    }
    private void OpenSettings(WizardPageSummary row)
    {
        if (!_editor.IsDesign) return;
        if (row.DocumentKind is { } kind)
            new WizardDocumentWindow(_editor, kind, _directory) { Owner = this }.ShowDialog();
        else if (row.SettingsSection is { } section)
            new WizardPageSettingsWindow(_editor, section) { Owner = this }.ShowDialog();
    }
    private void RefreshRows(int selected)
    {
        var scroll = FindScrollViewer(Pages);
        var offset = scroll?.VerticalOffset ?? 0;
        DataContext = WizardPageOverview.Create(_editor.Text, _editor.Project);
        Pages.SelectedIndex = selected;
        Pages.UpdateLayout();
        scroll?.ScrollToVerticalOffset(offset);
    }
    private static ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        if (parent is ScrollViewer scroll) return scroll;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            if (FindScrollViewer(VisualTreeHelper.GetChild(parent, index)) is { } child) return child;
        return null;
    }
    private void Design_Click(object sender, RoutedEventArgs e)
    {
        if (!_editor.IsDesign) return;
        if (Pages.SelectedItem is WizardPageSummary { SettingsSection: PageSettingsSection.ProgramGroup } row)
        {
            OpenSettings(row); RefreshRows(row.Order - 1);
            return;
        }
        if (Pages.SelectedItem is not WizardPageSummary { DesignIndex: int index }) return;
        _editor.PageIndex = index;
        Close();
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
