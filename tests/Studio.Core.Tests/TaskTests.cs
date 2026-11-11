using System.Diagnostics;
using System.Text.Json.Nodes;
using Studio.Core;

namespace Studio.Core.Tests;
public sealed class TaskTests : IDisposable
{
    private readonly string _folder=Path.Combine(Path.GetTempPath(),"Studio-tasks-"+Guid.NewGuid().ToString("N"));
    public TaskTests()=>Directory.CreateDirectory(_folder);
    public void Dispose()=>Directory.Delete(_folder,true);

    private StudioProject Project()
    {
        var project=StudioProject.CreateNew();
        project.InstallFile.Source=Path.Combine(_folder,"app.txt");
        File.WriteAllText(project.InstallFile.Source,"Test payload");
        project.Languages.Add(InstallerLanguage.Dutch());
        project.Tasks.Add(new(){Name="desktopicon",Description="Desktop shortcut",InitialSelection=TaskInitialSelection.Unchecked,
            Translations=new(){["dutch"]="Bureaubladsnelkoppeling"}});
        project.Tasks.Add(new(){Name="firstlaunch",Description="First run 100% %n",InitialSelection=TaskInitialSelection.CheckedOnce,Restart=true});
        project.Tasks.Add(new(){Name="extras",Description="Extras"});
        return project;
    }

    [Fact] public async Task TrialKeepsTemporarySelectionsAcrossLanguageAndNavigation()
    {
        var project=Project();using var editor=new EditorSession(project);
        editor.MarkSaved();
        using var trial=new WizardTrialPreview(editor);
        trial.Next();trial.Next();Assert.True(trial.IsTasks);
        Assert.False(trial.TaskChoices[0].IsSelected);Assert.True(trial.TaskChoices[1].IsSelected);Assert.True(trial.TaskChoices[2].IsSelected);
        trial.TaskChoices[0].IsSelected=true;
        trial.TaskChoices[1].IsSelected=false;
        await trial.LoadLanguageAsync("dutch",null,null);
        Assert.Equal("Bureaubladsnelkoppeling",trial.TaskChoices[0].Description);
        Assert.Equal("First run 100% %n",trial.TaskChoices[1].Description);
        trial.Next();Assert.Contains("Bureaubladsnelkoppeling",trial.Summary);Assert.DoesNotContain("First run",trial.Summary);
        trial.Back();Assert.True(trial.IsTasks);Assert.True(trial.TaskChoices[0].IsSelected);
        Assert.Equal(TaskInitialSelection.Unchecked,project.Tasks[0].InitialSelection);Assert.False(editor.IsDirty);
    }

    [Fact] public async Task TasksRoundTripAndOlderProjectsHaveNoTaskPage()
    {
        var project=Project();using var editor=new EditorSession(project);var tasks=new TasksEditor(editor);
        tasks.SelectedIndex=1;tasks.Move(-1);tasks.LanguageId="dutch";tasks.Translation="Eerste start";
        var path=Path.Combine(_folder,"project.issstudio");
        await ProjectFile.SaveAsync(path,project);
        var loaded=await ProjectFile.LoadAsync(path);
        Assert.Equal("firstlaunch",loaded.Tasks[0].Name);
        Assert.Equal("Eerste start",loaded.Tasks[0].DescriptionFor("dutch"));
        Assert.True(loaded.Tasks[0].Restart);
        var json=JsonNode.Parse(await File.ReadAllTextAsync(path))!;
        json["Version"]=14;json.AsObject().Remove("Tasks");
        await File.WriteAllTextAsync(path,json.ToJsonString());
        loaded=await ProjectFile.LoadAsync(path);
        Assert.True(loaded.WasMigrated);Assert.Empty(loaded.Tasks);
        Assert.Equal(WizardPageState.Disabled,WizardPageOverview.Create(editor.Text,loaded).Single(p=>p.Id=="Tasks").State);
    }

    [Fact] public async Task InvalidTasksCannotOverwriteAnExistingProject()
    {
        var project=Project();var path=Path.Combine(_folder,"project.issstudio");
        await ProjectFile.SaveAsync(path,project);var original=await File.ReadAllBytesAsync(path);
        project.Tasks[1].Name="DESKTOPICON";
        await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,project));
        project.Tasks[1].Name="firstlaunch";project.Tasks[1].Description="{#Injected}";
        await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,project));
        Assert.Equal(original,await File.ReadAllBytesAsync(path));
    }

    [Fact] public void LanguageRemovalClearsOnlyThatLanguagesTaskDescriptions()
    {
        var project=Project();using var editor=new EditorSession(project);
        editor.SetLanguageEnabled("dutch",false);
        Assert.DoesNotContain("dutch.StudioTask1",InnoScript.Generate(project,null));
        Assert.Equal("Bureaubladsnelkoppeling",project.Tasks[0].DescriptionFor("dutch"));
        Assert.True(editor.RemoveLanguage("dutch"));Assert.Empty(project.Tasks[0].Translations);
        Assert.Equal("Desktop shortcut",project.Tasks[0].DescriptionFor("english"));
    }

    [Fact] public async Task NativeCompilerAcceptsTranslatedTaskOptions()
    {
        var script=InnoScript.Generate(Project(),null);
        Assert.Contains("[Tasks]",script);Assert.Contains("Flags: unchecked",script);Assert.Contains("Flags: checkedonce restart",script);
        Assert.Contains("dutch.StudioTask1=Bureaubladsnelkoppeling",script);Assert.Contains("dutch.StudioTask2=First run 100%% %%n",script);
        var path=Path.Combine(_folder,"tasks.iss");await File.WriteAllTextAsync(path,script);
        var start=new ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=_folder};
        start.ArgumentList.Add("/Qp");start.ArgumentList.Add(path);using var process=Process.Start(start)!;
        var output=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();Assert.True(process.ExitCode==0,await output+await errors);
    }
}
