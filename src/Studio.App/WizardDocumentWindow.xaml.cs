using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Microsoft.Win32;
using Studio.Core;
namespace Studio.App;
public partial class WizardDocumentWindow : Window
{
    private readonly EditorSession _editor;
    private readonly WizardDocumentKind _kind;
    private readonly string? _directory;
    private bool _loading = true;
    private bool _ready, _closed;
    private int _request;
    private CancellationTokenSource? _readCancellation;
    private readonly DispatcherTimer _refreshTimer=new() { Interval=TimeSpan.FromMilliseconds(250) };
    public WizardDocumentWindow(EditorSession editor, WizardDocumentKind kind, string? directory)
    {
        _editor=editor; _kind=kind; _directory=directory;
        InitializeComponent();
        Heading.Text=editor.Text[kind switch { WizardDocumentKind.LicenseFile=>"PageOverviewLicense", WizardDocumentKind.InfoBeforeFile=>"PageOverviewInfoBefore", _=>"PageOverviewInfoAfter" }];
        GeneralSource.Text=editor.Project.Documents[kind].Source;
        Languages.ItemsSource=editor.Project.Languages.Select(l=>new { l.Id, Name=l.DisplayName(editor.Text) }).ToArray();
        Languages.SelectedValue=editor.PreviewLanguageId;
        _loading=false; UpdateVariant();
        _refreshTimer.Tick+=async (_,_)=> { _refreshTimer.Stop(); await RefreshPreviewAsync(); };
        Loaded+=async (_,_)=> { _ready=true; await RefreshPreviewAsync(); };
        Closed+=(_,_)=> { _closed=true; _request++; _refreshTimer.Stop(); _readCancellation?.Cancel(); _readCancellation?.Dispose(); };
    }
    private void UpdateVariant()
    {
        if (Languages.SelectedValue is not string id) return;
        Preview.FlowDirection = _editor.Project.Languages.Single(l => l.Id == id).RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        _loading=true; VariantSource.Text=_editor.Project.Documents[_kind].Translations.GetValueOrDefault(id,""); _loading=false;
        QueuePreview();
    }
    private void Language_Changed(object sender,SelectionChangedEventArgs e) { if (!_loading) UpdateVariant(); }
    private void GeneralSource_Changed(object sender,TextChangedEventArgs e)
    {
        if (_loading) return;
        _editor.SetDocumentSource(_kind,null,GeneralSource.Text); QueuePreview();
    }
    private void VariantSource_Changed(object sender,TextChangedEventArgs e)
    {
        if (_loading || Languages.SelectedValue is not string id) return;
        _editor.SetDocumentSource(_kind,id,VariantSource.Text); QueuePreview();
    }
    private string? Choose()
    {
        var dialog=new OpenFileDialog { Filter=_editor.Text["DocumentFilter"],CheckFileExists=true,Title=Heading.Text };
        if (dialog.ShowDialog(this)!=true) return null;
        return _directory is null ? dialog.FileName : Path.GetRelativePath(_directory,dialog.FileName);
    }
    private void GeneralChoose_Click(object sender,RoutedEventArgs e) { var path=Choose(); if(path is not null) GeneralSource.Text=path; }
    private void VariantChoose_Click(object sender,RoutedEventArgs e) { var path=Choose(); if(path is not null) VariantSource.Text=path; }
    private void QueuePreview()
    {
        _request++; _readCancellation?.Cancel(); _refreshTimer.Stop();
        Preview.Document.Blocks.Clear(); PreviewError.Text="";
        PreviewStatus.Text=_editor.Text["TrialDocumentLoading"];
        if(_ready && !_closed) _refreshTimer.Start();
    }
    private async void Preview_Click(object sender,RoutedEventArgs e) { _refreshTimer.Stop(); await RefreshPreviewAsync(); }
    private async Task RefreshPreviewAsync()
    {
        if(_closed || Languages.SelectedValue is not string id) return;
        var request=++_request;
        _readCancellation?.Cancel(); _readCancellation?.Dispose();
        _readCancellation=new CancellationTokenSource(); var cancellation=_readCancellation.Token;
        Preview.Document.Blocks.Clear(); PreviewError.Text=""; PreviewStatus.Text=_editor.Text["TrialDocumentLoading"];
        var document=_editor.Project.Documents[_kind];
        var selectedSource=WizardDocuments.Source(_editor.Project,_kind,id);
        var sources=new List<(string Label,string Source)>();
        if(!string.IsNullOrWhiteSpace(document.Source)) sources.Add((_editor.Text["DocumentGeneral"],document.Source));
        sources.AddRange(document.Translations.Where(p=>!string.IsNullOrWhiteSpace(p.Value))
            .Select(p=>(_editor.Project.Languages.Single(l=>l.Id==p.Key).DisplayName(_editor.Text),p.Value)));
        var errors=new List<string>(); FlowDocument? selectedDocument=null;
        foreach(var (label,source) in sources)
        {
            try
            {
                var content=await WizardDocumentContent.LoadAsync(source,_directory,cancellation);
                if(_closed || request!=_request) return;
                var preview=new FlowDocument();
                if(content.IsRtf) new TextRange(preview.ContentStart,preview.ContentEnd).Load(new MemoryStream(content.Bytes),DataFormats.Rtf);
                else preview.Blocks.Add(new Paragraph(new Run(content.Text)));
                if(source==selectedSource) selectedDocument=preview;
            }
            catch(OperationCanceledException) { return; }
            catch(Exception error) when(error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or FormatException)
            { errors.Add($"{label} — {source}: {_editor.Text[ProjectFile.ErrorResourceKey(error) ?? "DocumentPreviewFailed"]}"); }
        }
        if(_closed || request!=_request) return;
        PreviewError.Text=errors.Count==0 ? "" : _editor.Text["DocumentError"]+"\n"+string.Join("\n",errors);
        PreviewStatus.Text=string.IsNullOrWhiteSpace(selectedSource) ? _editor.Text["DocumentInactive"] : selectedDocument is null ? "" : selectedSource;
        if(selectedDocument is not null) Preview.Document=selectedDocument;
    }
    private void Close_Click(object sender,RoutedEventArgs e) => Close();
}
