using System.IO;
using System.Windows;
using Studio.Core;
namespace Studio.App;
public partial class ShortcutRulesWindow : Window
{
    private readonly EditorSession _editor;
    private readonly ShortcutRulesEditor _rules;
    private readonly string? _directory;
    private CancellationTokenSource? _cancellation;
    private int _request;
    private bool _closed;
    public ShortcutRulesWindow(EditorSession editor,string? directory)
    {
        _editor=editor; _directory=directory; _rules=new(editor); InitializeComponent(); DataContext=_rules;
        Loaded+=async (_,_)=>await LoadTargetsAsync();
        Closed+=(_,_)=> { _closed=true; _request++; _cancellation?.Cancel(); _cancellation?.Dispose(); };
    }
    private async Task LoadTargetsAsync()
    {
        var request=++_request; _cancellation?.Cancel(); _cancellation?.Dispose(); _cancellation=new(); var cancellation=_cancellation.Token;
        var snapshots=FileCollections.Rules(_editor.Project).Select(r=>new InstallFileSettings { Source=r.Source,Destination=r.Destination,RecurseSubdirectories=r.RecurseSubdirectories,Excludes=[..r.Excludes] }).ToArray();
        try
        {
            var targets=await Task.Run(()=>
            {
                var result=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach(var rule in snapshots) foreach(var file in FileCollections.Preview(rule,_directory,cancellation))
                { result.Add(file.Destination); ShortcutRule.CheckIndexSize(result.Count); }
                return result;
            },cancellation);
            if(!_closed && request==_request) _rules.SetTargets(targets);
        }
        catch(OperationCanceledException) { }
        catch(Exception error) when(error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { if(!_closed && request==_request) _rules.SetTargets([],ProjectFile.ErrorResourceKey(error) ?? "FilesPreviewFailed"); }
    }
    private void Add_Click(object sender,RoutedEventArgs e)=>_rules.Add();
    private void Remove_Click(object sender,RoutedEventArgs e)=>_rules.Remove();
    private async void Refresh_Click(object sender,RoutedEventArgs e)=>await LoadTargetsAsync();
    private void Group_Click(object sender,RoutedEventArgs e)
    {
        new WizardPageSettingsWindow(_editor,PageSettingsSection.ProgramGroup) { Owner=this }.ShowDialog(); _rules.RefreshSettings();
    }
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
}
