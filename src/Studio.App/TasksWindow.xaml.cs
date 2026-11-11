using System.Windows;
using Studio.Core;
namespace Studio.App;
public partial class TasksWindow : Window
{
    private TasksEditor Tasks=>(TasksEditor)DataContext;
    public TasksWindow(EditorSession editor) { InitializeComponent(); DataContext=new TasksEditor(editor); }
    private void Add_Click(object sender,RoutedEventArgs e)=>Tasks.Add();
    private void Remove_Click(object sender,RoutedEventArgs e)=>Tasks.Remove();
    private void Up_Click(object sender,RoutedEventArgs e)=>Tasks.Move(-1);
    private void Down_Click(object sender,RoutedEventArgs e)=>Tasks.Move(1);
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
}
