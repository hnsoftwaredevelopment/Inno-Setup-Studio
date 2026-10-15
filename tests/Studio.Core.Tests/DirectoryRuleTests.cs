using System.Diagnostics;
using System.Text.Json.Nodes;
using Studio.Core;
namespace Studio.Core.Tests;
public class DirectoryRuleTests : IDisposable
{
    private readonly string _folder=Path.Combine(Path.GetTempPath(),"Studio-dirs-"+Guid.NewGuid().ToString("N"));
    public DirectoryRuleTests()=>Directory.CreateDirectory(_folder);
    public void Dispose()=>Directory.Delete(_folder,true);
    private static StudioProject Project()
    {
        var project=StudioProject.CreateNew(); project.InstallFile.Source=typeof(DirectoryRuleTests).Assembly.Location; return project;
    }
    [Fact] public async Task InvalidOptionsAndMissingCurrentSchemaCollectionAreRejected()
    {
        foreach(var rule in new[]{new DirectoryRule { Attributes=(DirectoryRuleAttributes)16 },new DirectoryRule { Uninstall=(DirectoryUninstall)999 },new DirectoryRule { Compression=(DirectoryCompression)999 }})
        {
            var project=Project(); project.DirectoryRules.Add(rule);
            var error=await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(Path.Combine(_folder,"invalid.issstudio"),project));
            Assert.Equal("DirectoryRuleInvalid",ProjectFile.ErrorResourceKey(error));
        }
        var path=Path.Combine(_folder,"schema.issstudio"); await ProjectFile.SaveAsync(path,Project());
        var json=JsonNode.Parse(await File.ReadAllTextAsync(path))!; json.AsObject().Remove("DirectoryRules");
        await File.WriteAllTextAsync(path,json.ToJsonString());
        var missing=await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.LoadAsync(path));
        Assert.Equal("DirectoryRuleInvalid",ProjectFile.ErrorResourceKey(missing));
    }
    [Fact] public void ExportUsesNativeAttributesAndMutuallyExclusivePolicies()
    {
        var project=Project();
        project.DirectoryRules.Add(new() { Name=@"{app}\Data; archive",Attributes=DirectoryRuleAttributes.Hidden|DirectoryRuleAttributes.NotContentIndexed,
            Compression=DirectoryCompression.Enable,Uninstall=DirectoryUninstall.Keep,DeleteAfterInstall=true });
        var script=InnoScript.Generate(project,null);
        Assert.Contains("[Dirs]",script);
        Assert.Contains("Name: \"{app}\\Data; archive\"; Attribs: hidden notcontentindexed; Flags: deleteafterinstall setntfscompression uninsneveruninstall",script);
        Assert.DoesNotContain("uninsalwaysuninstall",script); Assert.DoesNotContain("unsetntfscompression",script);
    }
    [Theory]
    [InlineData(@"{sys}")][InlineData(@"{app}\..\Outside")][InlineData(@"{app}\{#x}")][InlineData("{app}\\Data\n[Run]")]
    public async Task InvalidNameCannotOverwriteSavedProject(string name)
    {
        var project=Project(); var path=Path.Combine(_folder,"project.issstudio"); await ProjectFile.SaveAsync(path,project);
        var before=await File.ReadAllBytesAsync(path); project.DirectoryRules.Add(new() { Name=name });
        var error=await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,project));
        Assert.Equal("DirectoryRuleInvalid",ProjectFile.ErrorResourceKey(error)); Assert.Equal(before,await File.ReadAllBytesAsync(path));
    }
    [Fact] public async Task RoundTripSaveAsAndFormatTenMigrationPreserveOtherSettings()
    {
        var project=Project(); project.DirectoryRules.Add(new() { Name=@"{app}\Logs",Uninstall=DirectoryUninstall.Keep });
        project.InstallFile.RecurseSubdirectories=true; project.InstallFile.Excludes=["*.pdb"];
        var path=Path.Combine(_folder,"project.issstudio"); await ProjectFile.SaveAsync(path,project);
        var loaded=await ProjectFile.LoadAsync(path); Assert.Equal(DirectoryUninstall.Keep,Assert.Single(loaded.DirectoryRules).Uninstall);
        var elsewhere=Path.Combine(_folder,"other"); Directory.CreateDirectory(elsewhere);
        var other=Path.Combine(elsewhere,"copy.issstudio"); await ProjectFile.SaveAsync(other,loaded,_folder);
        Assert.Equal(@"{app}\Logs",Assert.Single((await ProjectFile.LoadAsync(other)).DirectoryRules).Name);
        var legacy=JsonNode.Parse(await File.ReadAllTextAsync(path))!; legacy["Version"]=10; legacy.AsObject().Remove("DirectoryRules");
        await File.WriteAllTextAsync(path,legacy.ToJsonString()); loaded=await ProjectFile.LoadAsync(path);
        Assert.True(loaded.WasMigrated); Assert.Empty(loaded.DirectoryRules); Assert.Equal(project.AppId,loaded.AppId);
        Assert.True(loaded.InstallFile.RecurseSubdirectories); Assert.Equal("*.pdb",Assert.Single(loaded.InstallFile.Excludes));
    }
    [Fact] public void EditingCanReturnToZeroRulesAndTrialCannotChangeRules()
    {
        using var editor=new EditorSession(Project()); var rules=new DirectoryRulesEditor(editor);
        Assert.False(editor.IsDirty); Assert.False(rules.HasSelection);
        rules.Add(); rules.Name=@"{app}\Empty"; rules.Hidden=true; rules.Uninstall=DirectoryUninstall.Keep;
        Assert.True(editor.IsDirty); Assert.True(rules.Hidden); Assert.Equal("",rules.Error);
        editor.MarkSaved(); rules.SelectedIndex=0; Assert.False(editor.IsDirty);
        rules.Name=@"{app}\..\Outside"; Assert.NotEmpty(rules.Error);
        rules.Name=@"{app}\Empty"; editor.IsPreview=true; rules.Name=@"{app}\Changed"; rules.Add(); rules.Remove();
        Assert.Single(rules.Rules); Assert.Equal(@"{app}\Empty",rules.Name);
        editor.IsPreview=false; rules.Remove(); Assert.Empty(editor.Project.DirectoryRules); Assert.False(rules.HasSelection);
    }
    [Fact] public async Task NativeCompilerAcceptsAllPoliciesAndAttributes()
    {
        var project=Project();
        var policies=Enum.GetValues<DirectoryUninstall>(); var compression=Enum.GetValues<DirectoryCompression>();
        for(var i=0;i<policies.Length;i++) project.DirectoryRules.Add(new() { Name=@"{app}\Empty"+i,Uninstall=policies[i],Compression=compression[i],
            Attributes=DirectoryRuleAttributes.ReadOnly|DirectoryRuleAttributes.Hidden|DirectoryRuleAttributes.System|DirectoryRuleAttributes.NotContentIndexed,DeleteAfterInstall=i==0 });
        var path=Path.Combine(_folder,"dirs.iss"); await File.WriteAllTextAsync(path,InnoScript.Generate(project,null));
        var start=new ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=_folder };
        start.ArgumentList.Add("/Qp"); start.ArgumentList.Add(path);
        using var process=Process.Start(start)!; var output=process.StandardOutput.ReadToEndAsync(); var errors=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(); Assert.True(process.ExitCode==0,await output+await errors);
    }
}
