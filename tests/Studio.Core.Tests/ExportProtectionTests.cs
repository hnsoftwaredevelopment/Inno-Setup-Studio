using Studio.Core;

namespace Studio.Core.Tests;

public class ExportProtectionTests
{
    [Fact]
    public async Task ReexportPreservesManualChangesAndIdenticalExportPreservesBytes()
    {
        var folder = Path.Combine(Path.GetTempPath(), "Studio-export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var project = StudioProject.CreateExample();
            project.InstallFile.Source = typeof(ExportProtectionTests).Assembly.Location;
            var path = Path.Combine(folder, "installer.iss");
            await InnoScript.ExportAsync(path, project, folder);
            var bytes = await File.ReadAllBytesAsync(path);
            await InnoScript.ExportAsync(path, project, folder);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(path));
            await File.AppendAllTextAsync(path, "\r\n; manual edit");
            var changed = await File.ReadAllBytesAsync(path);
            var error = await Assert.ThrowsAsync<InvalidDataException>(() => InnoScript.ExportAsync(path, project, folder));
            Assert.Equal("ExportExistingProtected", ProjectFile.ErrorResourceKey(error));
            Assert.Equal(changed, await File.ReadAllBytesAsync(path));
            Assert.Empty(Directory.GetFiles(folder, "*.tmp"));
        }
        finally { Directory.Delete(folder, true); }
    }
}
