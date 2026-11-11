using Studio.Core;
namespace Studio.Core.Tests;
public class ComponentTests : IDisposable
{
    readonly string _folder=Path.Combine(Path.GetTempPath(),"Studio-components-"+Guid.NewGuid().ToString("N"));
    public ComponentTests()=>Directory.CreateDirectory(_folder);
    public void Dispose()=>Directory.Delete(_folder,true);
    StudioProject Project()
    {
        var p=StudioProject.CreateNew(); p.InstallFile.Source=Path.Combine(_folder,"payload.txt"); File.WriteAllText(p.InstallFile.Source,"Payload");
        p.InstallTypes.AddRange([new(){Name="full",Description="Full"},new(){Name="compact",Description="Compact"},new(){Name="custom",Description="Custom",IsCustom=true}]);
        p.Components.Add(new(){Name="main",Description="Program",Types=["full","compact","custom"],Fixed=true});
        p.Components.Add(new(){Name="help",Description="Help",Types=["full"]}); return p;
    }
    [Fact] public void ExportAndTrialFollowTypesWithoutChangingTheProject()
    {
        var p=Project(); var script=InnoScript.Generate(p,null); Assert.Contains("[Components]",script); Assert.Contains("Types: full compact custom; Flags: fixed",script);
        using var editor=new EditorSession(p); editor.MarkSaved();
        using(var trial=new WizardTrialPreview(editor))
        {
            Assert.Contains(trial.Route,r=>r.Id=="Components");
            trial.Next(); trial.Next(); Assert.True(trial.IsComponents);
            trial.ComponentSelection!.TypeId="compact"; Assert.True(trial.ComponentSelection.Items[0].IsChecked); Assert.False(trial.ComponentSelection.Items[1].IsChecked);
            trial.ComponentSelection.TypeId="custom"; trial.ComponentSelection.Items[1].IsChecked=true;
            Assert.True(trial.ComponentSelection.Items[1].IsChecked); Assert.False(trial.ComponentSelection.Items[0].CanToggle);
        }
        Assert.False(editor.IsDirty); Assert.False(editor.IsPreview);
    }
    [Fact] public void InvalidHierarchyAndMissingTypeAreRejected()
    {
        var p=Project(); p.Components.Add(new(){Name=@"missing\child",Description="Child"});
        Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(p,null));
        p.Components.RemoveAt(2); p.Components[1].Types=["unknown"]; Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(p,null));
        p.Components[1].Types=[]; p.Components[1].Name="MAIN"; Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(p,null));
    }
    [Fact] public void EditorMovesWholeGroupsAndGuardsUsedTypes()
    {
        var p=Project(); using var editor=new EditorSession(p); var components=new ComponentsEditor(editor);
        components.SelectedIndex=1; components.Add(true); components.Name=@"help\dutch"; components.Description="Dutch help";
        components.SelectedIndex=1; Assert.False(components.CanRemove); components.Name="renamed"; Assert.Equal("help",components.Name);
        components.Move(-1); Assert.Equal(new[]{"help",@"help\dutch","main"},p.Components.Select(c=>c.Name)); Assert.Equal("",components.Error);
        var types=new InstallTypesEditor(editor); Assert.True(types.NameReadOnly); Assert.False(types.CanRemove);types.Remove();types.Name="renamed";
        Assert.Equal("full",p.InstallTypes[0].Name); Assert.Contains("main",types.ReferenceHint);
        components.SelectedIndex=1;components.Remove();components.SelectedIndex=0;components.Remove(); Assert.Single(p.Components);
    }
    [Fact] public void DefaultTypesAndHierarchySelectionMatchNativeConstraints()
    {
        var p=Project(); p.InstallTypes.Clear(); p.Components.Add(new(){Name=@"help\dutch",Description="Dutch help",Types=["full"]});
        var selection=new ComponentSelectionPreview(p,()=>"english",key=>key); Assert.Equal(3,selection.Types.Count);
        Assert.True(selection.Items[1].IsChecked);Assert.True(selection.Items[2].IsChecked);
        selection.Items[2].IsChecked=false;Assert.False(selection.Items[1].IsChecked);Assert.Equal("custom",selection.TypeId);
        selection.Items[2].IsChecked=true;Assert.True(selection.Items[1].IsChecked);
        p.InstallTypes.Add(new(){Name="full",Description="Full"});
        var locked=new ComponentSelectionPreview(p,()=>"english",key=>key); Assert.False(locked.HasCustomType);Assert.False(locked.Items[1].CanToggle);
    }
    [Fact] public async Task PersistenceMigrationAndNativeCompilerRetainDescriptionsAndLinks()
    {
        var p=Project();p.Languages.Add(InstallerLanguage.Dutch());
        p.Components[1].Translations["dutch"]="Handleiding";p.Components[1].Description="Help 100% %n {literal}";
        p.Components.Add(new(){Name=@"help\dutch",Description="Dutch",Types=["full"],CheckableAlone=true});
        var path=Path.Combine(_folder,"project.issstudio"); await ProjectFile.SaveAsync(path,p);
        var loaded=await ProjectFile.LoadAsync(path);Assert.Equal("Handleiding",loaded.Components[1].DescriptionFor("dutch"));Assert.Equal("full",loaded.Components[2].Types[0]);
        var other=Path.Combine(_folder,"other");Directory.CreateDirectory(other);await ProjectFile.SaveAsync(Path.Combine(other,"copy.issstudio"),loaded,_folder);
        Assert.Equal(3,(await ProjectFile.LoadAsync(Path.Combine(other,"copy.issstudio"))).Components.Count);
        var script=InnoScript.Generate(p,null); Assert.Contains("dutch.StudioComponent2=Handleiding",script);Assert.Contains("english.StudioComponent2=Help 100%% %%n {literal}",script);
        var iss=Path.Combine(_folder,"components.iss");await File.WriteAllTextAsync(iss,script);
        var start=new System.Diagnostics.ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=_folder};
        start.ArgumentList.Add("/Qp");start.ArgumentList.Add(iss);using var process=System.Diagnostics.Process.Start(start)!;
        var output=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();await process.WaitForExitAsync();Assert.True(process.ExitCode==0,await output+await errors);
        var json=System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(path))!;json["Version"]=13;json.AsObject().Remove("Components"); json.AsObject().Remove("Tasks");await File.WriteAllTextAsync(path,json.ToJsonString());
        loaded=await ProjectFile.LoadAsync(path);Assert.True(loaded.WasMigrated);Assert.Empty(loaded.Components);Assert.Equal(3,loaded.InstallTypes.Count);
    }
    [Fact] public async Task InvalidInputCannotOverwriteSavedComponents()
    {
        var p=Project();var path=Path.Combine(_folder,"safe.issstudio");await ProjectFile.SaveAsync(path,p);var before=await File.ReadAllBytesAsync(path);
        p.Components[0].Translations["unknown"]="Text";await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,p));Assert.Equal(before,await File.ReadAllBytesAsync(path));
        p.Components[0].Translations.Clear();p.Components[0].Description="{#Injected}";await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,p));
    }
    [Fact] public void PartialParentCanSelectItsChildrenWithoutChangingFixedChildren()
    {
        var p=Project();p.Components[1].CheckableAlone=true;
        p.Components.Add(new(){Name=@"help\one",Description="One",Types=["full"]});
        p.Components.Add(new(){Name=@"help\two",Description="Two"});
        var preview=new ComponentSelectionPreview(p,()=>"english",key=>key);
        Assert.Null(preview.Items[1].CheckState);preview.Items[1].CheckState=true;
        Assert.True(preview.Items[2].IsChecked);Assert.True(preview.Items[3].IsChecked);Assert.True(preview.Items[1].CheckState);
        preview.Items[3].IsChecked=false;Assert.Null(preview.Items[1].CheckState);
        p.Components[2].Fixed=true;preview.Items[1].IsChecked=false;
        Assert.True(preview.Items[2].IsChecked);Assert.True(preview.Items[1].IsChecked);
    }
    [Fact] public void LanguageRemovalAndDisablePreserveOtherComponentDescriptions()
    {
        var p=Project();p.Languages.Add(InstallerLanguage.Dutch());p.Components[1].Translations["dutch"]="Handleiding";
        using var editor=new EditorSession(p);Assert.Equal(1,editor.OwnTextCount("dutch"));
        editor.SetLanguageEnabled("dutch",false);Assert.Equal("Handleiding",p.Components[1].DescriptionFor("dutch"));
        Assert.DoesNotContain("dutch.StudioComponent2",InnoScript.Generate(p,null));
        editor.SetLanguageEnabled("dutch",true);Assert.True(editor.RemoveLanguage("dutch"));Assert.Empty(p.Components[1].Translations);Assert.Equal("Help",p.Components[1].DescriptionFor("english"));
    }
    [Fact] public async Task NativeCompilerGeneratesDefaultTypesWhenNoTypesAreDefined()
    {
        var p=Project();p.InstallTypes.Clear();var script=InnoScript.Generate(p,null);Assert.DoesNotContain("[Types]",script);
        var path=Path.Combine(_folder,"defaulttypes.iss");await File.WriteAllTextAsync(path,script);
        var start=new System.Diagnostics.ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe"){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=_folder};
        start.ArgumentList.Add("/Qp");start.ArgumentList.Add(path);using var process=System.Diagnostics.Process.Start(start)!;
        var output=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();await process.WaitForExitAsync();Assert.True(process.ExitCode==0,await output+await errors);
    }
}
