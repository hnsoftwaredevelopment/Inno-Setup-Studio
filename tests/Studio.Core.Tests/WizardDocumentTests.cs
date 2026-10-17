using System.Text.Json;
using Studio.Core;
namespace Studio.Core.Tests;
public class WizardDocumentTests
{
    [Fact]
    public void SourcesDriveOverviewAndExportWithNativeLanguageOverrides()
    {
        using var editor = new EditorSession(StudioProject.CreateNew());
        editor.AddLanguage(InstallerLanguage.Dutch());
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder,"base.txt"),"General agreement");
            File.WriteAllText(Path.Combine(folder,"nl.rtf"),@"{\rtf1 Nederlandse licentie}");
            editor.SetDocumentSource(WizardDocumentKind.LicenseFile, null, "base.txt");
            editor.SetDocumentSource(WizardDocumentKind.LicenseFile, "dutch", "nl.rtf");
            editor.Project.InstallFile.Source = typeof(WizardDocumentTests).Assembly.Location;
            var script = InnoScript.Generate(editor.Project,folder);
            Assert.Contains("LicenseFile=\""+Path.Combine(folder,"base.txt")+"\"",script);
            Assert.Contains("LicenseFile: \""+Path.Combine(folder,"nl.rtf")+"\"",script);
            Assert.Equal("nl.rtf",WizardDocuments.Source(editor.Project,WizardDocumentKind.LicenseFile,"dutch"));
            Assert.Equal("base.txt",WizardDocuments.Source(editor.Project,WizardDocumentKind.LicenseFile,"english"));
            Assert.Equal(WizardPageState.Enabled,WizardPageOverview.Create(editor.Text,editor.Project).Single(r=>r.Id=="License").State);
            editor.SetDocumentSource(WizardDocumentKind.LicenseFile,null,"");
            Assert.DoesNotContain("LicenseFile",InnoScript.Generate(editor.Project,folder));
            Assert.Equal("nl.rtf",editor.Project.Documents[WizardDocumentKind.LicenseFile].Translations["dutch"]);
        }
        finally { Directory.Delete(folder,true); }
    }
    [Fact]
    public async Task NativeCompilerAcceptsAllThreeDocumentsAndTranslatedLicense()
    {
        var compiler=@"C:\Program Files\Inno Setup 7\ISCC.exe";
        Assert.True(File.Exists(compiler));
        var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            using var editor=new EditorSession(StudioProject.CreateNew());
            editor.AddLanguage(InstallerLanguage.Dutch());
            editor.Project.InstallFile.Source=typeof(WizardDocumentTests).Assembly.Location;
            File.WriteAllText(Path.Combine(folder,"base.txt"),"General document");
            File.WriteAllText(Path.Combine(folder,"nl.rtf"),@"{\rtf1 Nederlandse licentie}");
            foreach(var kind in Enum.GetValues<WizardDocumentKind>()) editor.SetDocumentSource(kind,null,"base.txt");
            editor.SetDocumentSource(WizardDocumentKind.LicenseFile,"dutch","nl.rtf");
            var bytes=File.ReadAllBytes(Path.Combine(folder,"nl.rtf"));
            var script=Path.Combine(folder,"test.iss");
            await File.WriteAllTextAsync(script,InnoScript.Generate(editor.Project,folder));
            var start=new System.Diagnostics.ProcessStartInfo(compiler) { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=folder };
            start.ArgumentList.Add("/Qp"); start.ArgumentList.Add(script);
            using var process=System.Diagnostics.Process.Start(start)!;
            var output=process.StandardOutput.ReadToEndAsync(); var errors=process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            Assert.True(process.ExitCode==0,await output+await errors);
            Assert.Equal(bytes,File.ReadAllBytes(Path.Combine(folder,"nl.rtf")));
        }
        finally { Directory.Delete(folder,true); }
    }

    [Fact]
    public void RemovingLanguageRetainsOtherDocumentVariantsAndTrialDoesNotEditSources()
    {
        using var editor=new EditorSession(StudioProject.CreateNew());
        editor.AddLanguage(InstallerLanguage.Dutch());
        editor.SetDocumentSource(WizardDocumentKind.LicenseFile,null,"base.txt");
        editor.SetDocumentSource(WizardDocumentKind.LicenseFile,"dutch","nl.txt");
        editor.SetDocumentSource(WizardDocumentKind.LicenseFile,"english","en.txt");
        editor.IsPreview=true;
        editor.SetDocumentSource(WizardDocumentKind.LicenseFile,null,"trial.txt");
        Assert.Equal("base.txt",editor.Project.Documents[WizardDocumentKind.LicenseFile].Source);
        editor.IsPreview=false;
        Assert.True(editor.RemoveLanguage("dutch"));
        Assert.Equal("en.txt",editor.Project.Documents[WizardDocumentKind.LicenseFile].Translations["english"]);
        Assert.False(editor.Project.Documents[WizardDocumentKind.LicenseFile].Translations.ContainsKey("dutch"));
    }

    [Theory]
    [InlineData("missing.txt")]
    [InlineData("test.exe")]
    [InlineData("{#bad}.txt")]
    public void InvalidActiveSourceBlocksExportButDormantSourcesDoNot(string source)
    {
        var project=StudioProject.CreateNew();
        project.InstallFile.Source=typeof(WizardDocumentTests).Assembly.Location;
        project.Documents[WizardDocumentKind.LicenseFile].Source=source;
        Assert.Throws<InvalidDataException>(()=>InnoScript.Generate(project,Path.GetTempPath()));
        project.Documents[WizardDocumentKind.LicenseFile].Source="";
        project.Documents[WizardDocumentKind.LicenseFile].Translations["english"]=source;
        Assert.DoesNotContain("LicenseFile",InnoScript.Generate(project,Path.GetTempPath()));
    }

    [Fact]
    public async Task SaveAsPreservesRelativeDocumentSourcesAndLegacyProjectsMigrate()
    {
        var folder = Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(folder,"other"));
        try
        {
            var project=StudioProject.CreateNew();
            project.Documents[WizardDocumentKind.InfoBeforeFile].Source="readme.txt";
            await ProjectFile.SaveAsync(Path.Combine(folder,"other","project.issstudio"),project,folder);
            var reopened=await ProjectFile.LoadAsync(Path.Combine(folder,"other","project.issstudio"));
            Assert.Equal(Path.Combine(folder,"readme.txt"),Path.GetFullPath(reopened.Documents[WizardDocumentKind.InfoBeforeFile].Source,Path.Combine(folder,"other")));
            var json=System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(StudioProject.CreateNew(),new JsonSerializerOptions { Converters={new System.Text.Json.Serialization.JsonStringEnumConverter()} }))!;
            json["Version"]=6; json.AsObject().Remove("AdditionalFiles"); json.AsObject().Remove("DirectoryRules"); json.AsObject().Remove("Shortcuts"); json.AsObject().Remove("Documents"); json.AsObject().Remove("PageSettings");
            await File.WriteAllTextAsync(Path.Combine(folder,"old.issstudio"),json.ToJsonString());
            var migrated=await ProjectFile.LoadAsync(Path.Combine(folder,"old.issstudio"));
            Assert.True(migrated.WasMigrated);
            Assert.All(migrated.Documents.Values,d=>Assert.Equal("",d.Source));
        }
        finally { Directory.Delete(folder,true); }
    }
}
