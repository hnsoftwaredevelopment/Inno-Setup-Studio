using System.Windows;
using System.Windows.Controls;

namespace Studio.App.Views;

public partial class InstallFileDetails : UserControl
{
    public InstallFileDetails() => InitializeComponent();
    private void ChooseSource_Click(object sender, RoutedEventArgs e) =>
        ((MainWindow)Window.GetWindow(this)).ChooseSourceFile();
}
