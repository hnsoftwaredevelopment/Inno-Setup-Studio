using System.Windows;
using Studio.Core;
namespace Studio.App;
public partial class DirectoryRulesWindow : Window
{
    public DirectoryRulesWindow(EditorSession editor) { InitializeComponent(); DataContext=new DirectoryRulesEditor(editor); }
    private void Add_Click(object sender,RoutedEventArgs e)=>((DirectoryRulesEditor)DataContext).Add();
    private void Remove_Click(object sender,RoutedEventArgs e)=>((DirectoryRulesEditor)DataContext).Remove();
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
}
