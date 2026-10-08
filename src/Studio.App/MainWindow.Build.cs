using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Studio.Core;

namespace Studio.App;

public partial class MainWindow
{
    private string? _compilerPath = CompilerService.LoadLocation(CompilerService.SettingsPath) ?? CompilerService.Discover();
    private string? _compilerVersion;
    private string? _buildDirectory;
    private string _compilerStateKey = "CompilerUnchecked";
    private CancellationTokenSource? _buildCancellation;
    private BuildOutputWindow? _buildOutput;
    private readonly List<CompilerMessage> _buildMessages = [];
    private string _buildStateKey = "BuildNoMessages";
    private readonly List<string> _additionalLanguageSources = [];
    private void LanguageSources_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        new LanguageSourcesWindow(_compilerPath, _additionalLanguageSources) { Owner = this }.ShowDialog();
    }

    private void UpdateCompilerState()
    {
        CompilerLocation.Text = _compilerPath ?? "";
        CompilerState.Text = Text[_compilerStateKey] + (_compilerVersion is null ? "" : " " + _compilerVersion);
        _buildOutput?.SetState(Text[_buildStateKey], _buildCancellation is null ? null : () => _buildCancellation?.Cancel());
    }

    private void ClearBuildResult()
    {
        _buildDirectory = null;
        BuildResult.Text = "";
        OpenBuildFolder.IsEnabled = false;
        _buildMessages.Clear();
        _buildStateKey = "BuildNoMessages";
        _buildOutput?.Reset();
        _buildOutput?.SetState(Text[_buildStateKey], null);
    }

    private async Task RefreshCompilerAsync()
    {
        if (_busy) return;
        SetBusy(true);
        _compilerVersion = null;
        _compilerStateKey = "CompilerMissing";
        try
        {
            if (_compilerPath is not null)
            {
                _compilerVersion = await CompilerService.CheckVersionAsync(_compilerPath);
                _compilerStateKey = "CompilerReady";
            }
        }
        catch (Exception error) when (IsCompilerError(error))
        {
            _compilerStateKey = "CompilerInvalid";
            _compilerVersion = error.Data["CompilerVersion"] as string;
        }
        finally { UpdateCompilerState(); SetBusy(false); }
    }

    private async void CheckCompiler_Click(object sender, RoutedEventArgs e) => await RefreshCompilerAsync();

    private async void ChooseCompiler_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        var dialog = new OpenFileDialog { Title = Text["ChooseCompiler"], Filter = "Inno Setup compiler (ISCC.exe)|ISCC.exe", CheckFileExists = true };
        if (dialog.ShowDialog(this) != true) return;
        _compilerPath = dialog.FileName;
        ClearBuildResult();
        await RefreshCompilerAsync();
        try { CompilerService.SaveLocation(CompilerService.SettingsPath, _compilerPath); }
        catch (Exception error) when (IsFileError(error))
        {
            MessageBox.Show(this, Text["PreferencesFailed"], Text["CompilerHeading"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Build_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || !_editor.IsDesign) return;
        ClearBuildResult();
        if (_compilerPath is null)
        {
            MessageBox.Show(this, Text["CompilerMissing"], Text["CompilerHeading"], MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if ((_path is null || _editor.IsDirty) && !await SaveAsync(false)) return;
        var directory = Path.GetDirectoryName(_path!)!;
        SetBusy(true);
        BuildResult.Text = Text["BuildRunning"];
        _buildStateKey = "BuildRunning";
        using var cancellation = new CancellationTokenSource();
        _buildCancellation = cancellation;
        ShowBuildMessages();
        try
        {
            // Generate synchronously before awaiting: the workspace is locked for this saved snapshot.
            var script = InnoScript.Generate(_editor.Project, directory);
            var progress = new Progress<CompilerMessage>(message =>
            {
                if (!ReferenceEquals(_buildCancellation, cancellation)) return;
                _buildMessages.Add(message);
                _buildOutput?.Append(message);
            });
            var result = await CompilerService.BuildAsync(_compilerPath, script, directory, _editor.OutputBaseFileName, progress, cancellation.Token);
            _compilerVersion = result.Version;
            _compilerStateKey = "CompilerReady";
            _buildDirectory = result.Directory;
            OpenBuildFolder.IsEnabled = true;
            _buildStateKey = result.Canceled ? "BuildCanceled" : result.InstallerPath is null ? "BuildFailed"
                : result.Messages.Any(m => m.Severity == CompilerMessageSeverity.Warning) ? "BuildSucceededWithWarnings" : "BuildSucceeded";
            BuildResult.Text = Text[_buildStateKey]
                + Environment.NewLine + (_path ?? "") + Environment.NewLine
                + (result.InstallerPath ?? result.ScriptPath);
            _buildMessages.Clear();
            _buildMessages.AddRange(result.Messages);
            _buildOutput?.Reset();
            foreach (var message in _buildMessages) _buildOutput?.Append(message);
        }
        catch (OperationCanceledException)
        {
            _buildStateKey = "BuildCanceled";
            BuildResult.Text = Text[_buildStateKey];
        }
        catch (Exception error) when (IsCompilerError(error))
        {
            _buildStateKey = "BuildFailed";
            BuildResult.Text = Text["BuildFailed"];
            var key = ProjectFile.ErrorResourceKey(error);
            var message = new CompilerMessage(CompilerMessageSeverity.Error,
                key is null ? Text["CompilerBuildError"] + Environment.NewLine + error.Message : Text[key], null, null, error.Message);
            _buildMessages.Add(message);
            _buildOutput?.Append(message);
        }
        finally { _buildCancellation = null; UpdateCompilerState(); SetBusy(false); }
    }

    private void BuildMessages_Click(object sender, RoutedEventArgs e) => ShowBuildMessages();
    private void ShowBuildMessages()
    {
        if (_buildOutput is null)
        {
            _buildOutput = new BuildOutputWindow { Owner = this };
            _buildOutput.Closed += (_, _) => _buildOutput = null;
            foreach (var message in _buildMessages) _buildOutput.Append(message);
            _buildOutput.Show();
        }
        _buildOutput.SetState(Text[_buildStateKey], _buildCancellation is null ? null : () => _buildCancellation?.Cancel());
        _buildOutput.Activate();
    }

    private void OpenBuildFolder_Click(object sender, RoutedEventArgs e)
    {
        if (_buildDirectory is null) return;
        try
        {
            var start = new ProcessStartInfo("explorer.exe") { UseShellExecute = false };
            start.ArgumentList.Add(_buildDirectory);
            Process.Start(start)?.Dispose();
        }
        catch (Exception error) when (IsCompilerError(error))
        {
            MessageBox.Show(this, Text["FileError"], Text["OpenBuildFolder"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static bool IsCompilerError(Exception error) => IsFileError(error) || error is Win32Exception or TimeoutException or InvalidOperationException;
}
