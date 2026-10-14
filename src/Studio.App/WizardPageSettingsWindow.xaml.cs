using System.Windows;
using Studio.Core;
namespace Studio.App;
public partial class WizardPageSettingsWindow : Window
{
    public WizardPageSettingsWindow(EditorSession editor, PageSettingsSection section)
    {
        InitializeComponent(); DataContext = new WizardPageSettingsEditor(editor, section);
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
