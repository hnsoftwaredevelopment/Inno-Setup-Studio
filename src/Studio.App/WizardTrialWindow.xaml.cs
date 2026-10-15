using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Studio.Core;
namespace Studio.App;
public partial class WizardTrialWindow : Window
{
    private readonly WizardTrialPreview _preview;
    private readonly EditorSession _editor;
    private readonly string? _compilerDirectory, _projectDirectory;
    private bool _ready, _closed;
    private string _documentKey="";
    private int _documentRequest;
    public WizardTrialWindow(EditorSession editor,string? compilerDirectory,string? projectDirectory)
    {
        _editor=editor; _compilerDirectory=compilerDirectory; _projectDirectory=projectDirectory;
        InitializeComponent();
        _preview=new WizardTrialPreview(editor); DataContext=_preview;
        _preview.PropertyChanged+=Preview_Changed;
        Closed+=(_,_)=> { _closed=true; _documentRequest++; _preview.PropertyChanged-=Preview_Changed; _preview.Dispose(); };
    }
    private async void Window_Loaded(object sender,RoutedEventArgs e)
    {
        _ready=true; await _preview.LoadLanguageAsync(_preview.LanguageId,_compilerDirectory,_projectDirectory);
    }
    private async void Language_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(_ready && LanguagePicker.SelectedValue is string id && id!=_preview.LanguageId)
            await _preview.LoadLanguageAsync(id,_compilerDirectory,_projectDirectory);
    }
    private async void Preview_Changed(object? sender,PropertyChangedEventArgs e)
    {
        if(_closed) return;
        if(_preview.ExitRequested) { Close(); return; }
        var key=_preview.PageId+"\n"+_preview.LanguageId+"\n"+_preview.DocumentSource;
        if(key==_documentKey) return;
        _documentKey=key; var request=++_documentRequest;
        _preview.DocumentAvailable=false; DocumentPreview.Document.Blocks.Clear(); DocumentStatus.Text="";
        DocumentStatus.Foreground=(Brush)FindResource("Muted");
        if(!_preview.IsDocument) return;
        DocumentStatus.Text=_editor.Text["TrialDocumentLoading"];
        try
        {
            var content=await WizardDocumentContent.LoadAsync(_preview.DocumentSource,_projectDirectory);
            if(_closed || request!=_documentRequest) return;
            if(content.IsRtf)
                new TextRange(DocumentPreview.Document.ContentStart,DocumentPreview.Document.ContentEnd).Load(new MemoryStream(content.Bytes),DataFormats.Rtf);
            else
            {
                DocumentPreview.Document.Blocks.Add(new Paragraph(new Run(content.Text)));
            }
            DocumentStatus.Text=_preview.DocumentSource; _preview.DocumentAvailable=true;
        }
        catch(Exception error) when(error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or FormatException)
        {
            if(!_closed && request==_documentRequest)
            {
                DocumentPreview.Document.Blocks.Clear();
                DocumentStatus.Foreground=new SolidColorBrush(Color.FromRgb(180,35,24));
                DocumentStatus.Text=_editor.Text["DocumentError"]+" — "+_editor.Text[ProjectFile.ErrorResourceKey(error) ?? "DocumentPreviewFailed"];
            }
        }
    }
    private void Next_Click(object sender,RoutedEventArgs e)=>_preview.Next();
    private void Back_Click(object sender,RoutedEventArgs e)=>_preview.Back();
    private void Cancel_Click(object sender,RoutedEventArgs e)=>_preview.Cancel();
    private void Resolve_Click(object sender,RoutedEventArgs e)=>_preview.ResolvePreparation();
    private void Complete_Click(object sender,RoutedEventArgs e)=>_preview.CompleteInstallation();
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
    private void Browse_Click(object sender,RoutedEventArgs e)
    {
        var path=FolderPicker.Pick(this,_preview.Directory,_editor.Text["TrialFolderTitle"]);
        if(path is not null) _preview.Directory=path;
    }
}
