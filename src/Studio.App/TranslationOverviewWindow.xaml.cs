using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Studio.Core;

namespace Studio.App;

public partial class TranslationOverviewWindow : Window
{
    private readonly TranslationOverview _overview;
    public TranslationOverviewWindow(EditorSession editor)
    {
        InitializeComponent();
        _overview = new TranslationOverview(editor);
        DataContext = _overview;
        for (var index = 0; index < editor.Project.Languages.Count; index++)
        {
            // The index is an internal matrix coordinate; language text is never parsed as XAML.
            var content = new FrameworkElementFactory(typeof(ContentControl));
            content.SetBinding(ContentControl.ContentProperty, new Binding($"Cells[{index}]"));
            content.SetValue(ContentControl.ContentTemplateProperty, Resources["TranslationCellTemplate"]);
            content.SetValue(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            content.SetValue(Control.VerticalContentAlignmentProperty, VerticalAlignment.Top);
            Translations.Columns.Add(new DataGridTemplateColumn
            {
                Header = _overview.Rows[0].Cells[index], HeaderTemplate = (DataTemplate)Resources["LanguageHeader"],
                Width = 280, CellTemplate = new DataTemplate { VisualTree = content }
            });
        }
        Closed += (_, _) => _overview.Dispose();
    }
    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TranslationCell cell }) cell.Copy();
    }
    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TranslationCell cell }) cell.Reset();
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
