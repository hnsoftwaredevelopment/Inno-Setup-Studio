using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Studio.App;

internal static class FolderPicker
{
    public static string? Pick(Window owner, string currentPath, string title)
    {
        var dialog = new OpenFolderDialog { Title = title, Multiselect = false };
        // Inno Setup-constants horen bij de doelcomputer en zijn geen lokale directory.
        if (!currentPath.Contains('{') && Directory.Exists(currentPath)) dialog.InitialDirectory = currentPath;
        return dialog.ShowDialog(owner) == true ? dialog.FolderName : null;
    }
}
