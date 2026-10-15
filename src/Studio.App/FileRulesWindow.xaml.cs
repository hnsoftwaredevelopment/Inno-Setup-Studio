using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using Studio.Core;
namespace Studio.App;
public partial class FileRulesWindow : Window
{
    private readonly FileRulesEditor _rules;
    private readonly EditorSession _editor;
    private readonly string? _directory;
    private readonly DispatcherTimer _timer=new() { Interval=TimeSpan.FromMilliseconds(350) };
    private CancellationTokenSource? _cancellation;
    private int _request;
    private bool _closed;
    public FileRulesWindow(EditorSession editor,string? directory)
    {
        _editor=editor; _directory=directory; _rules=new(editor);
        InitializeComponent(); DataContext=_rules;
        _rules.PropertyChanged+=RulesChanged;
        _timer.Tick+=async (_,_)=> { _timer.Stop(); await RefreshPreviewAsync(); };
        Loaded+=async (_,_)=>await RefreshPreviewAsync();
        Closed+=(_,_)=> { _closed=true; _request++; _timer.Stop(); _cancellation?.Cancel(); _cancellation?.Dispose(); _rules.PropertyChanged-=RulesChanged; };
    }
    private void RulesChanged(object? sender,PropertyChangedEventArgs e)
    {
        _request++; _cancellation?.Cancel(); _timer.Stop(); FilePreview.ItemsSource=null; PreviewError.Text="";
        PreviewStatus.Text=_editor.Text["FilesLoading"]; _timer.Start();
    }
    private async Task RefreshPreviewAsync()
    {
        if(_closed) return;
        var request=++_request; _cancellation?.Cancel(); _cancellation?.Dispose(); _cancellation=new();
        var cancellation=_cancellation.Token; var rule=_rules.Snapshot();
        FilePreview.ItemsSource=null; PreviewError.Text=""; PreviewStatus.Text=_editor.Text["FilesLoading"];
        try
        {
            var files=await Task.Run(()=>FileCollections.Preview(rule,_directory,cancellation),cancellation);
            if(_closed || request!=_request) return;
            FilePreview.ItemsSource=files; PreviewStatus.Text=_editor.Text.Format("FilesCount",files.Count);
        }
        catch(OperationCanceledException) { }
        catch(Exception error) when(error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            if(_closed || request!=_request) return;
            PreviewStatus.Text=""; PreviewError.Text=_editor.Text[ProjectFile.ErrorResourceKey(error) ?? "FilesPreviewFailed"];
        }
    }
    private string StoredPath(string path)=>_directory is null ? path : Path.GetRelativePath(_directory,path);
    private void ChooseFile_Click(object sender,RoutedEventArgs e)
    {
        var dialog=new OpenFileDialog { Title=_editor.Text["ChooseSourceFile"],CheckFileExists=true };
        if(dialog.ShowDialog(this)==true) _rules.Source=StoredPath(dialog.FileName);
    }
    private void ChooseFolder_Click(object sender,RoutedEventArgs e)
    {
        var path=FolderPicker.Pick(this,"",_editor.Text["FilesChooseFolder"]);
        if(path is not null) _rules.Source=StoredPath(Path.Combine(path,"*"));
    }
    private void Add_Click(object sender,RoutedEventArgs e)=>_rules.Add();
    private void Remove_Click(object sender,RoutedEventArgs e)=>_rules.Remove();
    private async void Refresh_Click(object sender,RoutedEventArgs e) { _timer.Stop(); await RefreshPreviewAsync(); }
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
}
