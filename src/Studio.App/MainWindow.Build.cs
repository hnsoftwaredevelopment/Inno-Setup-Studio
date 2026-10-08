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

    private void UpdateCompilerState()
    {
        CompilerLocation.Text = _compilerPath ?? "";
        CompilerState.Text = Text[_compilerStateKey] + (_compilerVersion is null ? "" : " " + _compilerVersion);
    }

    private void ClearBuildResult()
    {
        _buildDirectory = null;
        BuildResult.Text = "";
        OpenBuildFolder.IsEnabled = false;
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
        try
        {
            // Generate synchronously before awaiting: the workspace is locked for this saved snapshot.
            var script = InnoScript.Generate(_editor.Project, directory);
            var result = await CompilerService.BuildAsync(_compilerPath, script, directory);
            _compilerVersion = result.Version;
            _compilerStateKey = "CompilerReady";
            _buildDirectory = result.Directory;
            OpenBuildFolder.IsEnabled = true;
            BuildResult.Text = Text[result.InstallerPath is null ? "BuildFailed" : "BuildSucceeded"]
                + Environment.NewLine + (_path ?? "") + Environment.NewLine
                + (result.InstallerPath ?? result.ScriptPath);
            if (result.InstallerPath is null)
                MessageBox.Show(this, Text["BuildFailed"] + Environment.NewLine + result.Log,
                    Text["BuildInstaller"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception error) when (IsCompilerError(error))
        {
            BuildResult.Text = Text["BuildFailed"];
            var key = ProjectFile.ErrorResourceKey(error);
            MessageBox.Show(this, key is null ? Text["CompilerBuildError"] + Environment.NewLine + error.Message : Text[key],
                Text["BuildInstaller"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { UpdateCompilerState(); SetBusy(false); }
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
