using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Studio.Core;

namespace Studio.App.Views;

public partial class WizardPreview : UserControl
{
    public WizardPreview() => InitializeComponent();
    private void Element_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditorSession editor && sender is Button { Tag: ElementKind element })
            editor.Activate(element);
    }
}

public sealed class SelectionOutlineConverter : IMultiValueConverter
{
    private static readonly Brush Selected = new SolidColorBrush(Color.FromRgb(23, 104, 94));
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        values.Length == 3 && values[0] is ElementKind selected && values[1] is ElementKind element
        && values[2] is true && selected == element ? Selected : Brushes.Transparent;
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
