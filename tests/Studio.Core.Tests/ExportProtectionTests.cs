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
    [Fact]
    public async Task ExplicitConfirmationCanReplaceAnOlderOrManuallyEditedExport()
    {
        var folder=Path.Combine(Path.GetTempPath(),"Studio-export-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var project=StudioProject.CreateNew(); project.InstallFile.Source=typeof(ExportProtectionTests).Assembly.Location;
            var path=Path.Combine(folder,"alternate.iss"); await File.WriteAllTextAsync(path,"; previous installer and manual changes");
            var confirmed=await InnoScript.ReadExportFingerprintAsync(path);
            await InnoScript.ExportAsync(path,project,folder,confirmed);
            Assert.Equal(InnoScript.Generate(project,folder),await File.ReadAllTextAsync(path));
            Assert.Empty(Directory.GetFiles(folder,"*.tmp"));
        }
        finally { Directory.Delete(folder,true); }
    }
    [Fact]
    public async Task ChangesAfterConfirmationArePreserved()
    {
        var folder=Path.Combine(Path.GetTempPath(),"Studio-export-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var project=StudioProject.CreateNew(); project.InstallFile.Source=typeof(ExportProtectionTests).Assembly.Location;
            var path=Path.Combine(folder,"alternate.iss"); await File.WriteAllTextAsync(path,"; old");
            var confirmed=await InnoScript.ReadExportFingerprintAsync(path);
            await File.AppendAllTextAsync(path,"; changed during confirmation"); var changed=await File.ReadAllBytesAsync(path);
            var error=await Assert.ThrowsAsync<InvalidDataException>(()=>InnoScript.ExportAsync(path,project,folder,confirmed));
            Assert.Equal("ExportChangedSinceConfirmation",ProjectFile.ErrorResourceKey(error));
            Assert.Equal(changed,await File.ReadAllBytesAsync(path)); Assert.Empty(Directory.GetFiles(folder,"*.tmp"));
        }
        finally { Directory.Delete(folder,true); }
    }
    [Fact]
    public async Task ProjectSaveAsCanReplaceAnExistingAlternativeProject()
    {
        var folder=Path.Combine(Path.GetTempPath(),"Studio-project-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var path=Path.Combine(folder,"alternate.issstudio"); var first=StudioProject.CreateNew(); first.Name="Old project";
            await ProjectFile.SaveAsync(path,first);
            var replacement=StudioProject.CreateNew(); replacement.Name="Replacement";
            await ProjectFile.SaveAsync(path,replacement,folder);
            var reopened=await ProjectFile.LoadAsync(path);
            Assert.Equal("Replacement",reopened.Name); Assert.Equal(replacement.AppId,reopened.AppId);
            Assert.Empty(Directory.GetFiles(folder,"*.tmp"));
        }
        finally { Directory.Delete(folder,true); }
    }
    [Fact]
    public async Task OverwriteConfirmationNeverAllowsReplacingThePayloadSource()
    {
        var folder=Path.Combine(Path.GetTempPath(),"Studio-export-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            var path=Path.Combine(folder,"source.iss"); await File.WriteAllTextAsync(path,"source bytes");
            var project=StudioProject.CreateNew(); project.InstallFile.Source=path;
            var confirmed=await InnoScript.ReadExportFingerprintAsync(path);
            var error=await Assert.ThrowsAsync<InvalidDataException>(()=>InnoScript.ExportAsync(path,project,folder,confirmed));
            Assert.Equal("ExportCannotReplaceSource",ProjectFile.ErrorResourceKey(error));
            Assert.Equal("source bytes",await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(folder,true); }
    }
    [Fact]
    public async Task RemovedConfirmedFileIsNotSilentlyRecreated()
    {
        var folder=Path.Combine(Path.GetTempPath(),"Studio-export-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            var path=Path.Combine(folder,"alternate.iss"); await File.WriteAllTextAsync(path,"old script");
            var project=StudioProject.CreateNew(); project.InstallFile.Source=typeof(ExportProtectionTests).Assembly.Location;
            var confirmed=await InnoScript.ReadExportFingerprintAsync(path); File.Delete(path);
            var error=await Assert.ThrowsAsync<InvalidDataException>(()=>InnoScript.ExportAsync(path,project,folder,confirmed));
            Assert.Equal("ExportChangedSinceConfirmation",ProjectFile.ErrorResourceKey(error)); Assert.False(File.Exists(path));
            Assert.Empty(Directory.GetFiles(folder,"*.tmp"));
        }
        finally { Directory.Delete(folder,true); }
    }
}
