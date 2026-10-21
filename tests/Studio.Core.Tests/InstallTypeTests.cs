using System.Text.Json.Nodes;
using Studio.Core;
namespace Studio.Core.Tests;
public class InstallTypeTests : IDisposable
{
    private readonly string _folder=Path.Combine(Path.GetTempPath(),"Studio-types-"+Guid.NewGuid().ToString("N"));
    public InstallTypeTests()=>Directory.CreateDirectory(_folder);
    public void Dispose()=>Directory.Delete(_folder,true);
    private StudioProject Project()
    {
        var project=StudioProject.CreateNew(); project.InstallFile.Source=Path.Combine(_folder,"payload.txt");
        File.WriteAllText(project.InstallFile.Source,"Payload"); return project;
    }
    [Fact] public void ExportTranslatesDescriptionsWithFallbackAndEscapesMessageSyntax()
    {
        var project=Project(); project.Languages.Add(InstallerLanguage.Dutch());
        project.InstallTypes.Add(new() { Name="full",Description="Full 100% %n {literal}",Translations=new() { ["dutch"]="Volledig" } });
        project.InstallTypes.Add(new() { Name="custom",Description="Custom",IsCustom=true });
        var script=InnoScript.Generate(project,null);
        Assert.Contains("[Types]",script); Assert.Contains("Name: \"full\"; Description: \"{cm:StudioType1}\"",script);
        Assert.Contains("english.StudioType1=Full 100%% %%n {literal}",script);
        Assert.Contains("dutch.StudioType1=Volledig",script); Assert.Contains("dutch.StudioType2=Custom",script);
        Assert.Contains("Flags: iscustom",script);
    }
    [Theory]
    [InlineData("two words")][InlineData("../x")][InlineData("{#input}")][InlineData("")][InlineData("1name")][InlineData("AND")]
    public void InvalidNamesAreRejected(string name)
    {
        var project=Project(); project.InstallTypes.Add(new() { Name=name,Description="Valid" });
        Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(project,null));
    }
    [Fact] public void DuplicateNamesMultipleCustomTypesAndBlankDescriptionsAreRejected()
    {
        var project=Project(); project.InstallTypes.Add(new() { Name="full",Description="Full",IsCustom=true });
        project.InstallTypes.Add(new() { Name="FULL",Description="Other" });
        Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(project,null));
        project.InstallTypes[1].Name="other"; project.InstallTypes[1].IsCustom=true;
        Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(project,null));
        project.InstallTypes[1].IsCustom=false; project.InstallTypes[1].Description=" ";
        Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(project,null));
    }
    [Fact] public async Task NativeCompilerAcceptsTranslatedTypesWithoutInventedComponents()
    {
        var project=Project(); project.Languages.Add(InstallerLanguage.Dutch());
        project.InstallTypes.Add(new() { Name="full",Description="Full 100% %n {literal} \"quoted\"",Translations=new() { ["dutch"]="Volledig" } });
        project.InstallTypes.Add(new() { Name="custom",Description="Custom",IsCustom=true });
        var script=InnoScript.Generate(project,null); Assert.DoesNotContain("[Components]",script);
        var path=Path.Combine(_folder,"types.iss"); await File.WriteAllTextAsync(path,script);
        var start=new System.Diagnostics.ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=_folder };
        start.ArgumentList.Add("/Qp"); start.ArgumentList.Add(path);
        using var process=System.Diagnostics.Process.Start(start)!; var output=process.StandardOutput.ReadToEndAsync(); var errors=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(); Assert.True(process.ExitCode==0,await output+await errors);
    }
    [Fact] public void RemovingLanguageCountsAndRemovesOnlyItsTypeTranslations()
    {
        var project=Project(); project.Languages.Add(InstallerLanguage.Dutch());
        project.InstallTypes.Add(new() { Name="full",Description="Full",Translations=new() { ["dutch"]="Volledig",["english"]="Complete" } });
        using var editor=new EditorSession(project); Assert.Equal(1,editor.OwnTextCount("dutch"));
        Assert.True(editor.RemoveLanguage("dutch")); Assert.False(project.InstallTypes[0].Translations.ContainsKey("dutch"));
        Assert.Equal("Complete",project.InstallTypes[0].DescriptionFor("english"));
    }
    [Fact] public async Task InvalidDescriptionsCannotOverwriteSavedProject()
    {
        var project=Project(); project.InstallTypes.Add(new() { Name="full",Description="Full" });
        var path=Path.Combine(_folder,"safe.issstudio"); await ProjectFile.SaveAsync(path,project); var original=await File.ReadAllBytesAsync(path);
        foreach(var invalid in new[]{" ","line\nline","{#Injected}",new string('x',4097)})
        {
            project.InstallTypes[0].Description=invalid;
            await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,project));
            Assert.Equal(original,await File.ReadAllBytesAsync(path));
        }
        project.InstallTypes[0].Description="Full"; project.InstallTypes[0].Translations["unknown"]="Text";
        await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,project));
        project.InstallTypes[0].Translations.Clear(); project.SelectedInstallTypeId="missing";
        await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync(path,project));
    }
    [Fact] public void DisabledLanguagesKeepTranslationsAndPreviewModePreventsChanges()
    {
        var project=Project(); project.Languages.Add(InstallerLanguage.Dutch());
        project.InstallTypes.Add(new() { Name="full",Description="Full",Translations=new() { ["dutch"]="Volledig" } });
        using var editor=new EditorSession(project); editor.SetLanguageEnabled("dutch",false);
        var script=InnoScript.Generate(project,null); Assert.DoesNotContain("dutch.StudioType1",script);
        Assert.Equal("Volledig",project.InstallTypes[0].Translations["dutch"]);
        var types=new InstallTypesEditor(editor); editor.IsPreview=true; types.Name="changed"; types.Add(); types.Remove(); types.Move(1);
        Assert.Equal("full",Assert.Single(project.InstallTypes).Name);
        editor.IsPreview=false; editor.SetLanguageEnabled("dutch",true); Assert.Contains("dutch.StudioType1=Volledig",InnoScript.Generate(project,null));
    }
    [Fact] public async Task OrderSelectionTranslationsAndVersionTwelveMigrationArePreserved()
    {
        using var editor=new EditorSession(Project()); var types=new InstallTypesEditor(editor);
        Assert.False(editor.IsDirty); types.Add(); types.Name="full"; types.Description="Full"; types.LanguageId="english"; types.Translation="Complete";
        types.Add(); types.Name="custom"; types.Description="Custom"; types.IsCustom=true; types.Move(-1);
        var path=Path.Combine(_folder,"types.issstudio"); await ProjectFile.SaveAsync(path,editor.Project);
        var loaded=await ProjectFile.LoadAsync(path); Assert.Equal("custom",loaded.InstallTypes[0].Name); Assert.Equal("custom",loaded.SelectedInstallTypeId);
        Assert.Equal("Complete",loaded.InstallTypes[1].DescriptionFor("english"));
        var other=Path.Combine(_folder,"copy"); Directory.CreateDirectory(other);
        await ProjectFile.SaveAsync(Path.Combine(other,"copy.issstudio"),loaded,_folder);
        Assert.Equal("custom",(await ProjectFile.LoadAsync(Path.Combine(other,"copy.issstudio"))).SelectedInstallTypeId);
        var json=JsonNode.Parse(await File.ReadAllTextAsync(path))!; json["Version"]=12;
        json.AsObject().Remove("InstallTypes"); json.AsObject().Remove("SelectedInstallTypeId"); await File.WriteAllTextAsync(path,json.ToJsonString());
        loaded=await ProjectFile.LoadAsync(path); Assert.True(loaded.WasMigrated); Assert.Empty(loaded.InstallTypes); Assert.Equal("",loaded.SelectedInstallTypeId);
        types.Remove(); types.Remove(); Assert.Empty(editor.Project.InstallTypes); Assert.Equal("",editor.Project.SelectedInstallTypeId);
    }
}
