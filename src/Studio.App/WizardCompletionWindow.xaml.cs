using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Studio.Core;
namespace Studio.App;
public partial class WizardCompletionWindow : Window
{
    private readonly WizardCompletionPreview _preview;
    private readonly string? _compilerDirectory, _projectDirectory;
    private bool _ready;
    public WizardCompletionWindow(EditorSession editor, CompletionPhase phase, string? compilerDirectory, string? projectDirectory)
    {
        _compilerDirectory=compilerDirectory; _projectDirectory=projectDirectory;
        _preview=new WizardCompletionPreview(editor,phase);
        InitializeComponent(); DataContext=_preview;
        _preview.PropertyChanged+=Preview_Changed;
        Closed+=(_,_)=>_preview.PropertyChanged-=Preview_Changed;
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
    private void Preview_Changed(object? sender,PropertyChangedEventArgs e) { if(_preview.ExitRequested) Close(); }
    private void Next_Click(object sender,RoutedEventArgs e)=>_preview.Next();
    private void Back_Click(object sender,RoutedEventArgs e)=>_preview.Back();
    private void Cancel_Click(object sender,RoutedEventArgs e)=>_preview.Cancel();
    private void Resolve_Click(object sender,RoutedEventArgs e)=>_preview.ResolvePreparation();
    private void Complete_Click(object sender,RoutedEventArgs e)=>_preview.CompleteInstallation();
    private void Close_Click(object sender,RoutedEventArgs e)=>Close();
}
