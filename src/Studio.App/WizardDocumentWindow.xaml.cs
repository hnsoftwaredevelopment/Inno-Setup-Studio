using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;
using Studio.Core;
namespace Studio.App;
public partial class WizardDocumentWindow : Window
{
    private readonly EditorSession _editor;
    private readonly WizardDocumentKind _kind;
    private readonly string? _directory;
    private bool _loading = true;
    public WizardDocumentWindow(EditorSession editor, WizardDocumentKind kind, string? directory)
    {
        _editor=editor; _kind=kind; _directory=directory;
        InitializeComponent();
        Heading.Text=editor.Text[kind switch { WizardDocumentKind.LicenseFile=>"PageOverviewLicense", WizardDocumentKind.InfoBeforeFile=>"PageOverviewInfoBefore", _=>"PageOverviewInfoAfter" }];
        GeneralSource.Text=editor.Project.Documents[kind].Source;
        Languages.ItemsSource=editor.Project.Languages.Select(l=>new { l.Id, Name=l.DisplayName(editor.Text) }).ToArray();
        Languages.SelectedValue=editor.PreviewLanguageId;
        _loading=false; UpdateVariant();
    }
    private void UpdateVariant()
    {
        if (Languages.SelectedValue is not string id) return;
        Preview.FlowDirection = _editor.Project.Languages.Single(l => l.Id == id).RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        _loading=true; VariantSource.Text=_editor.Project.Documents[_kind].Translations.GetValueOrDefault(id,""); _loading=false;
        Preview.Document.Blocks.Clear();
    }
    private void Language_Changed(object sender,SelectionChangedEventArgs e) { if (!_loading) UpdateVariant(); }
    private void GeneralSource_Changed(object sender,TextChangedEventArgs e)
    {
        if (_loading) return;
        _editor.SetDocumentSource(_kind,null,GeneralSource.Text); Preview.Document.Blocks.Clear();
    }
    private void VariantSource_Changed(object sender,TextChangedEventArgs e)
    {
        if (_loading || Languages.SelectedValue is not string id) return;
        _editor.SetDocumentSource(_kind,id,VariantSource.Text); Preview.Document.Blocks.Clear();
    }
    private string? Choose()
    {
        var dialog=new OpenFileDialog { Filter=_editor.Text["DocumentFilter"],CheckFileExists=true,Title=Heading.Text };
        if (dialog.ShowDialog(this)!=true) return null;
        return _directory is null ? dialog.FileName : Path.GetRelativePath(_directory,dialog.FileName);
    }
    private void GeneralChoose_Click(object sender,RoutedEventArgs e) { var path=Choose(); if(path is not null) GeneralSource.Text=path; }
    private void VariantChoose_Click(object sender,RoutedEventArgs e) { var path=Choose(); if(path is not null) VariantSource.Text=path; }
    private async void Preview_Click(object sender,RoutedEventArgs e)
    {
        if (Languages.SelectedValue is not string id) return;
        try
        {
            var source=WizardDocuments.Source(_editor.Project,_kind,id);
            if(string.IsNullOrWhiteSpace(source)) { ShowText(_editor.Text["DocumentInactive"]); return; }
            var path=WizardDocuments.Resolve(source,_directory);
            await using var stream=File.OpenRead(path);
            if(stream.Length>4_194_304) { ShowText(_editor.Text["DocumentPreviewLarge"]); return; }
            var bytes=new byte[(int)stream.Length]; await stream.ReadExactlyAsync(bytes);
            if (source != WizardDocuments.Source(_editor.Project,_kind,id) || Languages.SelectedValue as string != id) return;
            if(Path.GetExtension(path).Equals(".rtf",StringComparison.OrdinalIgnoreCase))
                new TextRange(Preview.Document.ContentStart,Preview.Document.ContentEnd).Load(new MemoryStream(bytes),DataFormats.Rtf);
            else
            {
                using var reader=new StreamReader(new MemoryStream(bytes),new System.Text.UTF8Encoding(false,true),true);
                ShowText(reader.ReadToEnd());
            }
        }
        catch(Exception error) when(error is IOException or UnauthorizedAccessException or ArgumentException or System.Text.DecoderFallbackException)
        { ShowText(_editor.Text[ProjectFile.ErrorResourceKey(error) ?? "DocumentPreviewFailed"]); }
    }
    private void ShowText(string text) { Preview.Document.Blocks.Clear(); Preview.Document.Blocks.Add(new Paragraph(new Run(text))); }
    private void Close_Click(object sender,RoutedEventArgs e) => Close();
}
