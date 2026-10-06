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
    private const string ProjectFilter = "Inno Setup Studio-project (*.issstudio)|*.issstudio";

    public MainWindow()
    {
        InitializeComponent();
        SetProject(StudioProject.CreateExample(), null);
    }

    private void SetProject(StudioProject project, string? path)
    {
        if (_editor is not null) _editor.PropertyChanged -= EditorChanged;
        _path = path;
        _editor = new EditorSession(project);
        _editor.PropertyChanged += EditorChanged;
        DataContext = _editor;
        UpdateTitle();
    }

    private void EditorChanged(object? sender, PropertyChangedEventArgs e) => UpdateTitle();
    private void UpdateTitle() => Title = $"{(_editor.IsDirty ? "• " : "")}{(_path is null ? "Nieuw project" : Path.GetFileName(_path))} — Inno Setup Studio";
    private void SetBusy(bool busy) { _busy = busy; Workspace.IsEnabled = !busy; }
    private void Design_Checked(object sender, RoutedEventArgs e) { if (_editor is not null) _editor.IsPreview = false; }
    private void Preview_Checked(object sender, RoutedEventArgs e) { if (_editor is not null) _editor.IsPreview = true; }
    private void Reset_Click(object sender, RoutedEventArgs e) => _editor.ResetProperty();
    private void ChooseDirectory_Click(object sender, RoutedEventArgs e)
    {
        if (!_editor.CanEdit || !_editor.IsDirectorySelected) return;
        var path = FolderPicker.Pick(this, _editor.DirectoryText, "Standaard installatiemap kiezen");
        if (path is not null) _editor.PropertyText = path;
    }

    private async void New_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_busy || !await CanReplaceProjectAsync()) return;
        SetProject(StudioProject.CreateExample(), null);
    }

    private async void Open_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_busy || !await CanReplaceProjectAsync()) return;
        var dialog = new OpenFileDialog { Filter = ProjectFilter, CheckFileExists = true };
        if (dialog.ShowDialog(this) != true) return;
        SetBusy(true);
        try { SetProject(await ProjectFile.LoadAsync(dialog.FileName), dialog.FileName); }
        catch (Exception error) when (IsFileError(error)) { ShowFileError("Openen", error); }
        finally { SetBusy(false); }
    }

    private async void Save_Executed(object sender, ExecutedRoutedEventArgs e) { if (!_busy) await SaveAsync(false); }
    private async void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e) { if (!_busy) await SaveAsync(true); }

    private async Task<bool> SaveAsync(bool saveAs)
    {
        var path = _path;
        if (path is null || saveAs)
        {
            var dialog = new SaveFileDialog { Filter = ProjectFilter, DefaultExt = ".issstudio", AddExtension = true,
                FileName = path is null ? "Mijn applicatie.issstudio" : Path.GetFileName(path) };
            if (dialog.ShowDialog(this) != true) return false;
            path = dialog.FileName;
        }
        if (!string.Equals(Path.GetExtension(path), ".issstudio", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "Gebruik de extensie .issstudio voor deze nieuwe variant.", "Project opslaan", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }
        SetBusy(true);
        try
        {
            await ProjectFile.SaveAsync(path, _editor.Project);
            _path = path;
            _editor.MarkSaved();
            return true;
        }
        catch (Exception error) when (IsFileError(error)) { ShowFileError("Opslaan", error); return false; }
        finally { SetBusy(false); }
    }

    private async Task<bool> CanReplaceProjectAsync()
    {
        if (!_editor.IsDirty) return true;
        var result = MessageBox.Show(this, "Wilt u de wijzigingen in dit project opslaan?", "Niet-opgeslagen wijzigingen",
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
    private void ShowFileError(string action, Exception error)
    {
        _editor.SetStatus($"{action} is niet gelukt. Het huidige project blijft beschikbaar.");
        MessageBox.Show(this, error.Message, $"{action} mislukt", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
