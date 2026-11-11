using System.Windows;
using Studio.Core;
namespace Studio.App;
public partial class ComponentsWindow : Window
{
    private readonly EditorSession _editor;
    private ComponentsEditor Components=>(ComponentsEditor)DataContext;
    public ComponentsWindow(EditorSession editor) { _editor=editor; InitializeComponent(); DataContext=new ComponentsEditor(editor); }
    private void Add_Click(object sender,RoutedEventArgs e)=>Components.Add();
    private void AddChild_Click(object sender,RoutedEventArgs e)=>Components.Add(true);
    private void Remove_Click(object sender,RoutedEventArgs e)=>Components.Remove();
    private void Up_Click(object sender,RoutedEventArgs e)=>Components.Move(-1);
    private void Down_Click(object sender,RoutedEventArgs e)=>Components.Move(1);
    private void Types_Click(object sender,RoutedEventArgs e) { new InstallTypesWindow(_editor){Owner=this}.ShowDialog(); Components.RefreshSettings(); }
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
}
