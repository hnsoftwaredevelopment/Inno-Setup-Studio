using Studio.Core;
namespace Studio.Core.Tests;
public class PageToggleTests
{
 [Fact] public void ToggleUserInformationPreservesSettingsAndOnlySupportedPagesCanToggle()
 {
  using var editor=new EditorSession(StudioProject.CreateNew());
  var form=new WizardPageSettingsEditor(editor,PageSettingsSection.UserInfo); form.DefaultName="Mijn naam";
  Assert.True(editor.SetWizardPageEnabled("UserInfo",true));
  Assert.True(editor.Project.PageSettings.UserInfoEnabled);
  Assert.True(editor.SetWizardPageEnabled("UserInfo",false)); Assert.Equal("Mijn naam",form.DefaultName);
  var rows=WizardPageOverview.Create(editor.Text,editor.Project);
  Assert.Equal(new[]{"License","InfoBefore","UserInfo","InfoAfter"},rows.Where(r=>r.CanToggle).Select(r=>r.Id));
  Assert.False(editor.SetWizardPageEnabled("Installing",false));
  Assert.False(rows.Single(r=>r.Id=="Installing").CanToggle);
 }
 [Fact] public async Task DisabledDocumentRetainsGeneralSourceAndVariantsAcrossSaving()
 {
  var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
  try
  {
   using var editor=new EditorSession(StudioProject.CreateNew()); editor.AddLanguage(InstallerLanguage.Dutch());
   editor.Project.InstallFile.Source=typeof(PageToggleTests).Assembly.Location;
   editor.SetDocumentSource(WizardDocumentKind.LicenseFile,null,"missing.txt");
   editor.SetDocumentSource(WizardDocumentKind.LicenseFile,"dutch","missing-nl.txt");
   Assert.True(editor.SetWizardPageEnabled("License",false));
   Assert.DoesNotContain("LicenseFile",InnoScript.Generate(editor.Project,folder));
   var path=Path.Combine(folder,"project.issstudio"); await ProjectFile.SaveAsync(path,editor.Project);
   var loaded=await ProjectFile.LoadAsync(path);
   Assert.False(loaded.Documents[WizardDocumentKind.LicenseFile].Enabled);
   Assert.Equal("missing.txt",loaded.Documents[WizardDocumentKind.LicenseFile].Source);
   Assert.Equal("missing-nl.txt",loaded.Documents[WizardDocumentKind.LicenseFile].Translations["dutch"]);
   Assert.True(editor.SetWizardPageEnabled("License",true));
   Assert.Equal("missing-nl.txt",WizardDocuments.Source(editor.Project,WizardDocumentKind.LicenseFile,"dutch"));
   editor.IsPreview=true; Assert.False(editor.SetWizardPageEnabled("License",false));
  }
  finally { Directory.Delete(folder,true); }
 }
 [Fact] public async Task VersionEightDocumentsMigrateAsEnabledAndKeepExistingSources()
 {
  var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
  try
  {
   var project=StudioProject.CreateNew(); project.Documents[WizardDocumentKind.LicenseFile].Source="license.txt";
   var path=Path.Combine(folder,"old.issstudio"); await ProjectFile.SaveAsync(path,project);
   var json=System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(path))!; json["Version"]=8; json.AsObject().Remove("AdditionalFiles");
   foreach(var entry in json["Documents"]!.AsObject()) entry.Value!.AsObject().Remove("Enabled");
   await File.WriteAllTextAsync(path,json.ToJsonString());
   var migrated=await ProjectFile.LoadAsync(path);
   Assert.True(migrated.WasMigrated); Assert.True(migrated.Documents[WizardDocumentKind.LicenseFile].Enabled);
   Assert.Equal("license.txt",migrated.Documents[WizardDocumentKind.LicenseFile].Source);
   Assert.True(WizardPageOverview.Create(new StudioLocalizer(),migrated).Single(r=>r.Id=="License").IsOn);
  }
  finally { Directory.Delete(folder,true); }
 }
}
