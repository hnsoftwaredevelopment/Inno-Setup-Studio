using System.Diagnostics;
using System.Text.Json.Nodes;
using Studio.Core;
namespace Studio.Core.Tests;
public class ShortcutTests : IDisposable
{
    private readonly string _folder=Path.Combine(Path.GetTempPath(),"Studio-icons-"+Guid.NewGuid().ToString("N"));
    public ShortcutTests()=>Directory.CreateDirectory(_folder);
    public void Dispose()=>Directory.Delete(_folder,true);
    [Fact] public void ExpandedShortcutNameIsBounded()
    {
        var project=Project(); project.Name=new string('x',4097);
        Assert.Throws<InvalidDataException>(()=>ShortcutRule.Validate(new() { Target=@"{app}\demo.exe" },project));
    }
    [Fact] public void EditingAndReferenceRefreshPreserveStateAndAllowRemovingAllShortcuts()
    {
        using var editor=new EditorSession(Project()); var rules=new ShortcutRulesEditor(editor);
        rules.SetTargets([@"{app}\demo.exe"]); Assert.False(editor.IsDirty);
        rules.Add(); rules.Target=@"{app}\demo.exe"; Assert.Equal("",rules.Error);
        rules.Add(); rules.Target=@"{app}\demo.exe"; Assert.NotEmpty(rules.Error);
        rules.Name="Second"; Assert.Equal("",rules.Error); editor.MarkSaved();
        rules.SetTargets([]); Assert.NotEmpty(rules.Error); Assert.False(editor.IsDirty);
        rules.SetTargets([@"{app}\demo.exe"]); Assert.Equal("",rules.Error);
        editor.IsPreview=true; rules.Name="Changed"; rules.Remove(); rules.Add(); Assert.Equal("Second",rules.Name); Assert.Equal(2,rules.Rules.Count);
        editor.IsPreview=false; rules.Remove(); rules.Remove(); Assert.False(rules.HasSelection); Assert.Empty(editor.Project.Shortcuts);
    }
    [Fact] public void TargetSelectionSurvivesListResetAndSupportsDocuments()
    {
        using var editor=new EditorSession(Project()); var rules=new ShortcutRulesEditor(editor);
        rules.SetTargets([@"{app}\demo.exe",@"{app}\Manual.pdf"]); rules.Add();
        Assert.True(rules.IsStartMenu); Assert.False(rules.IsDesktop);
        rules.Folder=ShortcutFolder.Desktop; Assert.True(rules.IsDesktop); Assert.False(rules.IsStartMenu);
        rules.SelectedTarget=@"{app}\Manual.pdf"; Assert.Equal(@"{app}\Manual.pdf",rules.Target);
        editor.MarkSaved(); rules.SetTargets([]); rules.SelectedTarget=null;
        Assert.Equal(@"{app}\Manual.pdf",rules.Target); Assert.Null(rules.SelectedTarget); Assert.False(editor.IsDirty);
        rules.SetTargets([@"{app}\Manual.pdf"]); Assert.Equal(rules.Target,rules.SelectedTarget); Assert.Equal("",rules.Error);
        rules.SelectedTarget=@"{app}\excluded.exe"; Assert.Equal(@"{app}\Manual.pdf",rules.Target);
    }
    private StudioProject Project()
    {
        var path=Path.Combine(_folder,"demo.exe"); File.WriteAllText(path,"Test payload, never executed");
        var project=StudioProject.CreateNew(); project.InstallFile.Source=path; return project;
    }
    [Fact] public void ExportResolvesNamesAndReferencesAndSafelyQuotesParameters()
    {
        var project=Project(); project.Shortcuts.Add(new() { Name="{AppName}",Target=@"{app}\demo.exe",Subfolder="Tools",Parameters="--title \"Demo\"",WorkingDirectory="{app}" });
        var script=InnoScript.Generate(project,null);
        Assert.Contains("Name: \"{group}\\Tools\\My application\"; Filename: \"{app}\\demo.exe\"; Parameters: \"--title \"\"Demo\"\"\"; WorkingDir: \"{app}\"",script);
        project.Shortcuts[0].Target=@"{app}\missing.exe";
        var error=Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(project,null));
        Assert.Equal("ShortcutTargetMissing",ProjectFile.ErrorResourceKey(error)); Assert.Equal(@"{app}\missing.exe",error.Data["Studio.ShortcutTarget"]);
    }
    [Theory]
    [InlineData("Bad/Name")][InlineData("..")][InlineData("{#Injected}")]
    public async Task InvalidShortcutCannotOverwriteProject(string name)
    {
        var project=Project(); var path=Path.Combine(_folder,"project.issstudio"); await ProjectFile.SaveAsync(path,project);
        var before=await File.ReadAllBytesAsync(path); project.Shortcuts.Add(new() { Name=name,Target=@"{app}\demo.exe" });
        await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,project)); Assert.Equal(before,await File.ReadAllBytesAsync(path));
    }
    [Fact] public async Task RoundTripSaveAsAndVersionElevenMigrationRetainRules()
    {
        var project=Project(); project.Shortcuts.Add(new() { Name="Demo",Target=@"{app}\demo.exe",Folder=ShortcutFolder.Desktop });
        project.DirectoryRules.Add(new() { Name=@"{app}\Empty" });
        var path=Path.Combine(_folder,"project.issstudio"); await ProjectFile.SaveAsync(path,project);
        var loaded=await ProjectFile.LoadAsync(path); Assert.Equal(ShortcutFolder.Desktop,Assert.Single(loaded.Shortcuts).Folder);
        var other=Path.Combine(_folder,"other"); Directory.CreateDirectory(other); var copy=Path.Combine(other,"copy.issstudio");
        await ProjectFile.SaveAsync(copy,loaded,_folder); Assert.Equal(@"{app}\demo.exe",Assert.Single((await ProjectFile.LoadAsync(copy)).Shortcuts).Target);
        var legacy=JsonNode.Parse(await File.ReadAllTextAsync(path))!; legacy["Version"]=11; legacy.AsObject().Remove("Shortcuts");
        await File.WriteAllTextAsync(path,legacy.ToJsonString()); loaded=await ProjectFile.LoadAsync(path);
        Assert.True(loaded.WasMigrated); Assert.Empty(loaded.Shortcuts); Assert.Single(loaded.DirectoryRules); Assert.Equal(project.AppId,loaded.AppId);
    }
    [Fact] public void GroupPageFollowsNativeHasIconsAndTrialGroupIsTemporary()
    {
        using var editor=new EditorSession(Project()); editor.Project.Shortcuts.Add(new() { Name="Demo",Target=@"{app}\demo.exe",Folder=ShortcutFolder.Desktop });
        Assert.Equal(WizardPageState.Conditional,WizardPageOverview.Create(editor.Text,editor.Project).Single(p=>p.Id=="ProgramGroup").State);
        editor.Project.PageSettings.GroupMode=ProgramGroupPageMode.Show; editor.Project.PageSettings.AllowNoIcons=true; editor.MarkSaved();
        using(var trial=new WizardTrialPreview(editor))
        {
            trial.Next(); trial.Next(); Assert.True(trial.IsProgramGroup);
            trial.Group="Trial folder"; trial.NoIcons=true; trial.Next(); Assert.True(trial.IsReady);
        }
        Assert.Equal("",editor.Project.PageSettings.DefaultGroup); Assert.False(editor.IsDirty);
        editor.Project.PageSettings.GroupMode=ProgramGroupPageMode.Hide;
        Assert.Equal(WizardPageState.Disabled,WizardPageOverview.Create(editor.Text,editor.Project).Single(p=>p.Id=="ProgramGroup").State);
    }
    [Fact] public async Task NativeCompilerAcceptsBothLocationsAndEscapedParameters()
    {
        var project=Project(); project.Name="Demo {literal}";
        project.InstallFile.Source=Path.Combine(_folder,"demo{literal}.exe"); await File.WriteAllTextAsync(project.InstallFile.Source,"Not executed");
        project.Shortcuts.Add(new() { Name="{AppName}",Target=@"{app}\demo{literal}.exe",Parameters="--title \"Demo\"",WorkingDirectory="{app}" });
        project.Shortcuts.Add(new() { Name="Demo desktop",Target=@"{app}\demo{literal}.exe",Folder=ShortcutFolder.Desktop });
        project.PageSettings.DefaultGroup="{AppName}";
        var path=Path.Combine(_folder,"icons.iss"); await File.WriteAllTextAsync(path,InnoScript.Generate(project,null));
        var start=new ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=_folder };
        start.ArgumentList.Add("/Qp"); start.ArgumentList.Add(path);
        using var process=Process.Start(start)!; var output=process.StandardOutput.ReadToEndAsync(); var errors=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(); Assert.True(process.ExitCode==0,await output+await errors);
    }
}
