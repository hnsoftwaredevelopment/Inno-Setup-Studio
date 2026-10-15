using System.Text.Json;
using Studio.Core;
namespace Studio.Core.Tests;
public class WizardTrialTests
{
    [Fact] public async Task CancelPreservesUnsavedEditsExportAndSourceWhileNativeRolesStaySeparate()
    {
        using var editor=new EditorSession(StudioProject.CreateNew());
        editor.ProductName="Unsaved product";
        editor.Project.InstallFile.Source=typeof(WizardTrialTests).Assembly.Location;
        editor.SetTranslation("english",WizardTextSlot.SharedNext,"Custom next");
        var before=JsonSerializer.Serialize(editor.Project); var script=InnoScript.Generate(editor.Project,null);
        var source=@"C:\Program Files\Inno Setup 7\Default.isl"; var bytes=await File.ReadAllBytesAsync(source);
        using(var trial=new WizardTrialPreview(editor))
        {
            await trial.LoadLanguageAsync("english",Path.GetDirectoryName(source),null);
            Assert.False(trial.ShowBack); Assert.Equal("Custom next",trial.NextCaption);
            trial.Next(); trial.Directory="C:\\Trial"; trial.Back(); trial.Next(); Assert.Equal("C:\\Trial",trial.Directory);
            trial.Next(); Assert.Equal("Install",trial.NextCaption); trial.Next();
            Assert.False(trial.ShowBack); Assert.False(trial.ShowNext);
            trial.Progress=100; trial.CompleteInstallation(); Assert.Equal("Finish",trial.NextCaption);
            Assert.False(trial.ShowCancel); trial.Cancel(); Assert.True(trial.ExitRequested);
        }
        Assert.True(editor.IsDirty); Assert.Equal(before,JsonSerializer.Serialize(editor.Project));
        Assert.Equal(script,InnoScript.Generate(editor.Project,null)); Assert.Equal(bytes,await File.ReadAllBytesAsync(source));
    }
    [Fact] public void RouteIncludesActivePagesAndTrialValuesNeverChangeProject()
    {
        using var editor=new EditorSession(StudioProject.CreateNew());
        foreach(var document in editor.Project.Documents.Values) document.Source="test.txt";
        editor.Project.PageSettings.UserInfoEnabled=true;
        editor.PageIndex=2; editor.SelectedElement=ElementKind.Browse; editor.MarkSaved();
        var before=JsonSerializer.Serialize(editor.Project);
        using(var trial=new WizardTrialPreview(editor))
        {
            Assert.Equal(new[]{"Welcome","License","InfoBefore","UserInfo","Destination","Ready","Installing","InfoAfter","Finished"},trial.Route.Select(p=>p.Id));
            trial.Next(); Assert.Equal("License",trial.PageId); Assert.False(trial.CanNext);
            trial.DocumentAvailable=true; trial.LicenseAccepted=true; trial.Next();
            trial.DocumentAvailable=true; trial.Next(); Assert.Equal("UserInfo",trial.PageId);
            trial.UserName=""; Assert.False(trial.CanNext); trial.UserName="Trial user";
            trial.Organization="Trial org"; trial.Next(); trial.Directory="C:\\Trial"; trial.Next();
            Assert.Contains("C:\\Trial",trial.Summary); Assert.Contains("Trial user",trial.Summary);
            trial.Next(); Assert.Equal("Installing",trial.PageId); Assert.False(trial.CanBack);
            trial.Progress=100; trial.CompleteInstallation(); Assert.Equal("InfoAfter",trial.PageId);
            trial.DocumentAvailable=true; trial.Next(); Assert.Equal("Finished",trial.PageId);
            trial.Next(); Assert.True(trial.ExitRequested);
        }
        Assert.Equal(before,JsonSerializer.Serialize(editor.Project)); Assert.False(editor.IsDirty);
        Assert.False(editor.IsPreview); Assert.Equal(2,editor.PageIndex); Assert.Equal(ElementKind.Browse,editor.SelectedElement);
    }
    [Fact] public void DisabledPagesAreSkippedAndPreparationIsConditional()
    {
        using var editor=new EditorSession(StudioProject.CreateNew());
        editor.Project.Documents[WizardDocumentKind.LicenseFile].Source="stored.txt";
        editor.Project.Documents[WizardDocumentKind.LicenseFile].Enabled=false;
        using var trial=new WizardTrialPreview(editor);
        Assert.Equal(new[]{"Welcome","Destination","Ready","Installing","Finished"},trial.Route.Select(p=>p.Id));
        trial.Next(); trial.Next(); trial.PreparationProblem=true; trial.Next();
        Assert.Equal("Preparing",trial.PageId); trial.Back(); Assert.Equal("Ready",trial.PageId);
        trial.Next(); trial.ResolvePreparation(); Assert.Equal("Installing",trial.PageId);
        trial.CompleteInstallation(); Assert.Equal("Installing",trial.PageId);
    }
    [Fact] public async Task LanguageSwitchResetsAcceptanceAndResolvesSelectedDocumentAndButtonScope()
    {
        using var editor=new EditorSession(StudioProject.CreateNew()); editor.AddLanguage(InstallerLanguage.Dutch());
        editor.Project.Documents[WizardDocumentKind.LicenseFile].Source="default.txt";
        editor.Project.Documents[WizardDocumentKind.LicenseFile].Translations["dutch"]="dutch.txt";
        editor.SetTranslation("dutch",WizardTextSlot.SharedNext,"Samen verder");
        editor.SetTranslation("dutch",WizardTextSlot.WelcomeNext,"Welkom verder"); editor.MarkSaved();
        using var trial=new WizardTrialPreview(editor);
        await trial.LoadLanguageAsync("dutch",@"C:\Program Files\Inno Setup 7",null);
        Assert.Equal("Welkom verder",trial.NextCaption); trial.Next(); Assert.Equal("Samen verder",trial.NextCaption);
        Assert.Equal("dutch.txt",trial.DocumentSource); trial.DocumentAvailable=true; trial.LicenseAccepted=true;
        await trial.LoadLanguageAsync("english",@"C:\Program Files\Inno Setup 7",null);
        Assert.Equal("default.txt",trial.DocumentSource); Assert.False(trial.LicenseAccepted); Assert.False(trial.CanNext);
        Assert.Equal("english",editor.PreviewLanguageId); Assert.False(editor.IsDirty);
    }
}
