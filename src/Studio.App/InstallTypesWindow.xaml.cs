using System.Windows;
using Studio.Core;
namespace Studio.App;
public partial class InstallTypesWindow : Window
{
    private InstallTypesEditor Types=>(InstallTypesEditor)DataContext;
    public InstallTypesWindow(EditorSession editor) { InitializeComponent(); DataContext=new InstallTypesEditor(editor); }
    private void Add_Click(object sender,RoutedEventArgs e)=>Types.Add();
    private void Remove_Click(object sender,RoutedEventArgs e)=>Types.Remove();
    private void Up_Click(object sender,RoutedEventArgs e)=>Types.Move(-1);
    private void Down_Click(object sender,RoutedEventArgs e)=>Types.Move(1);
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
}
