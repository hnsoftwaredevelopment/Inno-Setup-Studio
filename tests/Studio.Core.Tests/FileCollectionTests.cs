using System.Diagnostics;
using System.Text.Json.Nodes;
using Studio.Core;
namespace Studio.Core.Tests;
public class FileCollectionTests : IDisposable
{
    private readonly string _folder=Path.Combine(Path.GetTempPath(),"Studio-files-"+Guid.NewGuid().ToString("N"));
    public FileCollectionTests()
    {
        Directory.CreateDirectory(Path.Combine(_folder,"sub")); Directory.CreateDirectory(Path.Combine(_folder,"Temp"));
        File.WriteAllText(Path.Combine(_folder,"a.txt"),"A"); File.WriteAllText(Path.Combine(_folder,"README"),"R");
        File.WriteAllText(Path.Combine(_folder,"sub","b.txt"),"B"); File.WriteAllText(Path.Combine(_folder,"Temp","c.txt"),"C");
    }
    public void Dispose()=>Directory.Delete(_folder,true);
    [Fact] public async Task AdditionalPayloadCannotBeOverwrittenByExportEvenWithConfirmation()
    {
        var source=Path.Combine(_folder,"payload.iss"); await File.WriteAllTextAsync(source,"Payload content");
        var project=StudioProject.CreateNew(); project.InstallFile.Source="a.txt";
        project.AdditionalFiles.Add(new() { Source="payload.iss" });
        var hash=await InnoScript.ReadExportFingerprintAsync(source);
        var error=await Assert.ThrowsAsync<InvalidDataException>(()=>InnoScript.ExportAsync(source,project,_folder,hash));
        Assert.Equal("ExportCannotReplaceSource",ProjectFile.ErrorResourceKey(error));
        Assert.Equal("Payload content",await File.ReadAllTextAsync(source));
    }
    [Theory]
    [InlineData(@"{app}\..\outside")]
    [InlineData(@"{app}\Data\..")]
    [InlineData(@"{sys}")]
    public void DestinationCannotEscapeApplicationFolder(string destination)
    {
        Assert.Throws<InvalidDataException>(()=>FileCollections.Preview(new() { Source="a.txt",Destination=destination },_folder));
    }
    [Fact] public void RuleEditingPreservesMultilineDraftAndRemovalKeepsRemainingRule()
    {
        using var editor=new EditorSession(StudioProject.CreateNew()); var rules=new FileRulesEditor(editor);
        Assert.False(editor.IsDirty); rules.Source="a.txt"; rules.Excludes="*.pdb\r\n";
        Assert.Equal("*.pdb\r\n",rules.Excludes); Assert.Single(editor.Project.InstallFile.Excludes);
        rules.Add(); rules.Source="*.txt"; rules.RecurseSubdirectories=true; rules.SelectedIndex=0;
        rules.Remove(); Assert.Equal("*.txt",rules.Source); Assert.True(rules.RecurseSubdirectories);
        Assert.False(rules.CanRemove); Assert.Empty(editor.Project.AdditionalFiles); Assert.True(editor.IsDirty);
        editor.MarkSaved(); _=FileCollections.Preview(rules.Snapshot(),_folder); Assert.False(editor.IsDirty);
    }
    [Fact] public void WildcardsRecursionAndExclusionsProduceNativeRelativeDestinations()
    {
        var rule=new InstallFileSettings { Source="*.*",Destination=@"{app}\Data",RecurseSubdirectories=true,Excludes=[@"\Temp",@"sub\*.txt"] };
        var files=FileCollections.Preview(rule,_folder);
        Assert.Equal(new[]{"a.txt","README"},files.Select(f=>Path.GetFileName(f.Source)).Order(StringComparer.OrdinalIgnoreCase));
        Assert.Contains(files,f=>f.Destination==@"{app}\Data\README");
        rule.Excludes=["*.*"]; Assert.Single(FileCollections.Preview(rule,_folder));
    }
    [Fact] public async Task MultipleRulesAndSaveAsPreserveSourcesAndVersionNineMigrates()
    {
        var project=StudioProject.CreateNew(); project.InstallFile.Source="a.txt";
        project.AdditionalFiles.Add(new() { Source=@"sub\*.txt",Destination=@"{app}\Data",RecurseSubdirectories=true,Excludes=["*.pdb"] });
        var path=Path.Combine(_folder,"project.issstudio"); await ProjectFile.SaveAsync(path,project);
        var loaded=await ProjectFile.LoadAsync(path); Assert.Single(loaded.AdditionalFiles);
        Assert.Contains("Excludes: \"*.pdb\"",InnoScript.Generate(loaded,_folder));
        var elsewhere=Path.Combine(_folder,"elsewhere"); Directory.CreateDirectory(elsewhere);
        var saveAs=Path.Combine(elsewhere,"copy.issstudio"); await ProjectFile.SaveAsync(saveAs,loaded,_folder);
        Assert.Equal(InnoScript.Generate(project,_folder),InnoScript.Generate(await ProjectFile.LoadAsync(saveAs),elsewhere));
        var legacy=JsonNode.Parse(await File.ReadAllTextAsync(path))!;
        legacy["Version"]=9; legacy.AsObject().Remove("AdditionalFiles"); legacy.AsObject().Remove("DirectoryRules"); legacy.AsObject().Remove("Shortcuts"); legacy.AsObject().Remove("InstallTypes"); legacy.AsObject().Remove("SelectedInstallTypeId");
        legacy["InstallFile"]!.AsObject().Remove("RecurseSubdirectories"); legacy["InstallFile"]!.AsObject().Remove("Excludes");
        await File.WriteAllTextAsync(path,legacy.ToJsonString()); var migrated=await ProjectFile.LoadAsync(path);
        Assert.True(migrated.WasMigrated); Assert.Empty(migrated.AdditionalFiles); Assert.Equal("a.txt",migrated.InstallFile.Source);
        Assert.False(migrated.InstallFile.RecurseSubdirectories); Assert.Empty(migrated.InstallFile.Excludes);
    }
    [Fact] public async Task NativeCompilerAcceptsMultipleWildcardRules()
    {
        var project=StudioProject.CreateNew(); project.InstallFile.Source="*.txt"; project.InstallFile.RecurseSubdirectories=true;
        project.InstallFile.Excludes=[@"\Temp"];
        project.AdditionalFiles.Add(new() { Source="README",Destination=@"{app}\Docs" });
        var path=Path.Combine(_folder,"test.iss"); await File.WriteAllTextAsync(path,InnoScript.Generate(project,_folder));
        var start=new ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=_folder };
        start.ArgumentList.Add(path);
        using var process=Process.Start(start)!; var output=process.StandardOutput.ReadToEndAsync(); var error=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(); var log=await output+await error; Assert.True(process.ExitCode==0,log);
        Assert.Contains(Path.Combine(_folder,"sub","b.txt"),log,StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.Combine(_folder,"Temp","c.txt"),log,StringComparison.OrdinalIgnoreCase);
    }
}
