using System.ComponentModel;
using System.Windows;
using Studio.Core;

namespace Studio.App;

public partial class BuildOutputWindow : Window
{
    private Action? _cancel;
    public BuildOutputWindow() => InitializeComponent();
    public void Reset() => Messages.Clear();
    public void SetState(string text, Action? cancel)
    {
        BuildState.Text = text;
        _cancel = cancel;
        CancelBuild.IsEnabled = cancel is not null;
    }
    public void Append(CompilerMessage message)
    {
        var text = Localization.Current;
        var prefix = message.Severity == CompilerMessageSeverity.Information ? "" : text["Compiler" + message.Severity] + ": ";
        var location = message.FileName is null ? "" : message.FileName + (message.Line is null ? "" : ":" + message.Line) + " — ";
        // Keep the live view bounded; compiler.log retains the full native output.
        if (Messages.Text.Length > 1_000_000) Messages.Clear();
        Messages.AppendText(prefix + location + message.Message + Environment.NewLine);
        Messages.ScrollToEnd();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _cancel?.Invoke();
        CancelBuild.IsEnabled = false;
        BuildState.Text = Localization.Current["BuildCanceling"];
    }
    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_cancel is null) return;
        e.Cancel = true;
        Cancel_Click(this, new RoutedEventArgs());
    }
}
