using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Studio.Core;

namespace Studio.App;

public partial class MainWindow : Window
{
    private EditorSession _editor = null!;
    private string? _path;
    private bool _busy;
    private bool _closeApproved;
    private bool _closing;
    private static StudioLocalizer Text => Localization.Current;

    public MainWindow()
    {
        InitializeComponent();
        SetProject(StudioProject.CreateNew(), null);
        Loaded += async (_, _) => await RefreshCompilerAsync();
        Text.PropertyChanged += LanguageChanged;
        Closed += (_, _) => { Text.PropertyChanged -= LanguageChanged; _editor.Dispose(); };
    }

    private void SetProject(StudioProject project, string? path)
    {
        if (_editor is not null) { _editor.PropertyChanged -= EditorChanged; _editor.Dispose(); }
        _path = path;
        _editor = new EditorSession(project, Text);
        _editor.PropertyChanged += EditorChanged;
        DataContext = _editor;
        ClearBuildResult();
        UpdateTitle();
    }

    private void LanguageChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateCompilerState();
        try { StudioPreferences.SaveLanguage(StudioPreferences.DefaultPath, Text.Language); }
        catch (Exception error) when (IsFileError(error))
        {
            _editor.SetStatus("PreferencesFailed");
            MessageBox.Show(this, Text["PreferencesFailed"], Text["LanguageLabel"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void EditorChanged(object? sender, PropertyChangedEventArgs e) => UpdateTitle();
    private void UpdateTitle() => Title = $"{(_editor.IsDirty ? "• " : "")}{(_path is null ? Text["NewProject"] : Path.GetFileName(_path))} — Inno Setup Studio";
    private void SetBusy(bool busy) { _busy = busy; Workspace.IsEnabled = !busy; }
    private void Design_Checked(object sender, RoutedEventArgs e) { if (_editor is not null) _editor.IsPreview = false; }
    private void Preview_Checked(object sender, RoutedEventArgs e)
    {
        if (_editor is null || _busy || _editor.IsPreview) return;
        try
        {
            new WizardTrialWindow(_editor, _compilerPath is null ? null : Path.GetDirectoryName(_compilerPath),
                _path is null ? null : Path.GetDirectoryName(_path)) { Owner = this }.ShowDialog();
        }
        finally { _editor.IsPreview = false; }
    }
    private void WizardPageOverview_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || !_editor.IsDesign) return;
        new WizardPageOverviewWindow(_editor, _path is null ? null : Path.GetDirectoryName(_path),
            _compilerPath is null ? null : Path.GetDirectoryName(_compilerPath)) { Owner = this }.ShowDialog();
    }
    private void Reset_Click(object sender, RoutedEventArgs e) => _editor.ResetProperty();
    private void ResetTooltip_Click(object sender, RoutedEventArgs e) => _editor.ResetTooltip();
    private void UseEffectiveText_Click(object sender, RoutedEventArgs e) => _editor.UseEffectiveText();
    private void UseEffectiveTooltip_Click(object sender, RoutedEventArgs e) => _editor.UseEffectiveTooltip();
    private void ChooseDirectory_Click(object sender, RoutedEventArgs e)
    {
        if (!_editor.CanEdit || !_editor.IsDirectorySelected) return;
        var path = FolderPicker.Pick(this, _editor.DirectoryText, Text["DefaultFolderTitle"]);
        if (path is not null) _editor.PropertyText = path;
    }

    private async void New_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_busy || !await CanReplaceProjectAsync()) return;
        SetProject(StudioProject.CreateNew(), null);
    }

    private async void Open_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_busy || !await CanReplaceProjectAsync()) return;
        var dialog = new OpenFileDialog { Filter = Text["ProjectFilter"], Title = Text["OpenTitle"], CheckFileExists = true };
        if (dialog.ShowDialog(this) != true) return;
        SetBusy(true);
        try { SetProject(await ProjectFile.LoadAsync(dialog.FileName), dialog.FileName); }
        catch (Exception error) when (IsFileError(error)) { ShowFileError("Open", error); }
        finally { SetBusy(false); }
    }

    private async void Save_Executed(object sender, ExecutedRoutedEventArgs e) { if (!_busy) await SaveAsync(false); }
    private async void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e) { if (!_busy) await SaveAsync(true); }

    internal void ManageFileRules()
    {
        if(_busy || !_editor.IsDesign) return;
        new FileRulesWindow(_editor,_path is null ? null : Path.GetDirectoryName(_path)) { Owner=this }.ShowDialog();
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || !_editor.IsDesign) return;
        var projectDirectory = _path is null ? null : Path.GetDirectoryName(_path);
        // Validate before asking where to write, so missing input cannot replace existing output.
        try { _ = InnoScript.Generate(_editor.Project, projectDirectory); }
        catch (Exception error) when (IsFileError(error)) { ShowFileError("Export", error); return; }
        var dialog = new SaveFileDialog { Title = Text["ExportScript"], Filter = Text["ScriptFilter"],
            DefaultExt = ".iss", AddExtension = true, OverwritePrompt = false, FileName = "installer.iss",
            InitialDirectory = projectDirectory ?? "" };
        if (dialog.ShowDialog(this) != true) return;
        SetBusy(true);
        try
        {
            byte[]? confirmed = null;
            if (File.Exists(dialog.FileName))
            {
                // Capture the existing content before asking; later edits invalidate this confirmation.
                confirmed = await InnoScript.ReadExportFingerprintAsync(dialog.FileName);
                if (MessageBox.Show(this, Text.Format("ExportOverwriteConfirm", dialog.FileName), Text["ExportScript"],
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;
            }
            await InnoScript.ExportAsync(dialog.FileName, _editor.Project, projectDirectory, confirmed);
            _editor.SetStatus("StatusExported");
        }
        catch (Exception error) when (IsFileError(error)) { ShowFileError("Export", error); }
        finally { SetBusy(false); }
    }

    private async Task<bool> SaveAsync(bool saveAs)
    {
        var path = _path;
        if (path is null || saveAs)
        {
            var dialog = new SaveFileDialog { Filter = Text["ProjectFilter"], Title = Text["SaveTitle"], DefaultExt = ".issstudio", AddExtension = true, OverwritePrompt = true,
                FileName = path is null ? SuggestedFileName(_editor.Project.Name) : Path.GetFileName(path) };
            if (dialog.ShowDialog(this) != true) return false;
            path = dialog.FileName;
        }
        if (!string.Equals(Path.GetExtension(path), ".issstudio", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, Text["ExtensionHint"], Text["SaveTitle"], MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }
        SetBusy(true);
        try
        {
            await ProjectFile.SaveAsync(path, _editor.Project, _path is null ? null : Path.GetDirectoryName(_path));
            _path = path;
            _editor.MarkSaved();
            return true;
        }
        catch (Exception error) when (IsFileError(error)) { ShowFileError("Save", error); return false; }
        finally { SetBusy(false); }
    }

    private async Task<bool> CanReplaceProjectAsync()
    {
        if (!_editor.IsDirty) return true;
        var result = MessageBox.Show(this, Text["SaveChanges"], Text["Unsaved"],
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        return result == MessageBoxResult.No || result == MessageBoxResult.Yes && await SaveAsync(false);
    }

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_closeApproved) return;
        if (_busy || _closing) { e.Cancel = true; return; }
        if (!_editor.IsDirty) return;
        e.Cancel = true;
        _closing = true;
        var canClose = await CanReplaceProjectAsync();
        _closing = false;
        if (!canClose) return;
        _closeApproved = true;
        // Defer Close until the current Closing event has returned, even when no await yielded.
        _ = Dispatcher.BeginInvoke(new Action(Close));
    }

    private static bool IsFileError(Exception error) => error is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or NotSupportedException;
    private static string SuggestedFileName(string productName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var name = new string(productName.Take(120).Select(c => invalid.Contains(c) ? '_' : c).ToArray()).TrimEnd(' ', '.');
        return (string.IsNullOrWhiteSpace(name) ? "Project" : name) + ".issstudio";
    }
    private void ShowFileError(string action, Exception error)
    {
        _editor.SetStatus("Status" + action + "Failed");
        var key = ProjectFile.ErrorResourceKey(error) ?? (error switch
        {
            FileNotFoundException or DirectoryNotFoundException => "FileMissing",
            UnauthorizedAccessException => "AccessDenied",
            _ => "FileError"
        });
        MessageBox.Show(this, FileErrorDescription(error, key), Text[action + "Failed"], MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    private string FileErrorDescription(Exception error, string key)
    {
        var message = Text[key];
        if (error.Data["Studio.LanguageId"] is string id && _editor.Project.Languages.FirstOrDefault(language => language.Id == id) is { } language)
            message += Environment.NewLine + Environment.NewLine + language.DisplayName(Text) + Environment.NewLine + language.Source;
        if (error.Data["Studio.DocumentPath"] is string documentPath) message += Environment.NewLine + documentPath;
        return message;
    }
}
