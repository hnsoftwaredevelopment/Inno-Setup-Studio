using Studio.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
namespace Studio.Core.Tests;
public class PageSettingsTests
{
 [Fact] public void UserInfoSettingsChangeStatusAndExportButTrialValuesDoNotChangeProject()
 {
  using var editor=new EditorSession(StudioProject.CreateNew());
  var form=new WizardPageSettingsEditor(editor,PageSettingsSection.UserInfo);
  form.UserInfoEnabled=true; form.DefaultName="Test gebruiker"; form.DefaultOrganization="Test organisatie";
  form.UsePreviousUserInfo=false;
  editor.Project.InstallFile.Source=typeof(PageSettingsTests).Assembly.Location;
  Assert.Equal(WizardPageState.Enabled,WizardPageOverview.Create(editor.Text,editor.Project).Single(p=>p.Id=="UserInfo").State);
  var script=InnoScript.Generate(editor.Project,null);
  Assert.Contains("UserInfoPage=yes",script); Assert.Contains("DefaultUserInfoName=\"Test gebruiker\"",script);
  Assert.Contains("UsePreviousUserInfo=no",script);
  editor.MarkSaved();
  form.TrialName="Proef"; form.TrialOrganization="Tijdelijk";
  Assert.False(editor.IsDirty); Assert.Equal(script,InnoScript.Generate(editor.Project,null));
  form.UserInfoEnabled=false;
  Assert.DoesNotContain("UserInfoPage=yes",InnoScript.Generate(editor.Project,null));
  Assert.Equal("Test gebruiker",form.DefaultName);
 }
 [Fact] public void GroupPolicyIsRetainedButNoIconsMeansNoGroupPage()
 {
  using var editor=new EditorSession(StudioProject.CreateNew());
  var form=new WizardPageSettingsEditor(editor,PageSettingsSection.ProgramGroup);
  form.GroupMode=ProgramGroupPageMode.Show; form.DefaultGroup="{AppName}"; form.AllowNoIcons=true; form.UsePreviousGroup=false;
  editor.Project.InstallFile.Source=typeof(PageSettingsTests).Assembly.Location;
  var script=InnoScript.Generate(editor.Project,null);
  Assert.Contains("DisableProgramGroupPage=no",script); Assert.Contains("DefaultGroupName=\"My application\"",script);
  Assert.Contains("AllowNoIcons=yes",script); Assert.Contains("UsePreviousGroup=no",script);
  Assert.DoesNotContain("[Icons]",script);
  Assert.Equal(WizardPageState.Disabled,WizardPageOverview.Create(editor.Text,editor.Project).Single(p=>p.Id=="ProgramGroup").State);
  editor.MarkSaved(); form.TrialGroup="Andere map"; Assert.False(editor.IsDirty);
 }
 [Fact] public async Task SettingsRoundTripAndVersionSevenMigrationRetainDocuments()
 {
  var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
  try
  {
   using var editor=new EditorSession(StudioProject.CreateNew());
   var form=new WizardPageSettingsEditor(editor,PageSettingsSection.UserInfo);
   form.UserInfoEnabled=true; form.DefaultName="Naam"; form.DefaultOrganization="Organisatie";
   form.GroupMode=ProgramGroupPageMode.Hide; form.DefaultGroup="Testmap"; form.AllowNoIcons=true;
   editor.Project.Documents[WizardDocumentKind.LicenseFile].Source="license.txt";
   var path=Path.Combine(folder,"test.issstudio"); await ProjectFile.SaveAsync(path,editor.Project);
   var loaded=await ProjectFile.LoadAsync(path);
   Assert.True(loaded.PageSettings.UserInfoEnabled); Assert.Equal("Naam",loaded.PageSettings.DefaultName);
   Assert.Equal(ProgramGroupPageMode.Hide,loaded.PageSettings.GroupMode); Assert.True(loaded.PageSettings.AllowNoIcons);
   var json=System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(path))!;
   json["Version"]=7; json.AsObject().Remove("PageSettings"); await File.WriteAllTextAsync(path,json.ToJsonString());
   loaded=await ProjectFile.LoadAsync(path); Assert.True(loaded.WasMigrated); Assert.False(loaded.PageSettings.UserInfoEnabled);
   Assert.Equal("license.txt",loaded.Documents[WizardDocumentKind.LicenseFile].Source);
  }
  finally { Directory.Delete(folder,true); }
 }
 [Theory]
 [InlineData("nl")][InlineData("en")][InlineData("de")]
 public void OpeningFormAndChangingTrialValuesPreserveCleanProjectAndPreviewLanguage(string language)
 {
  using var editor=new EditorSession(StudioProject.CreateNew(),new StudioLocalizer(language));
  var before=JsonSerializer.Serialize(editor.Project);
  var form=new WizardPageSettingsEditor(editor,PageSettingsSection.UserInfo);
  Assert.All(form.GroupModes,choice=>Assert.DoesNotContain("GroupMode",choice.Name));
  form.TrialName="Proef"; form.TrialOrganization="Proeforganisatie"; form.TrialGroup="Proefmap";
  Assert.Equal(before,JsonSerializer.Serialize(editor.Project)); Assert.False(editor.IsDirty);
  Assert.Equal("english",editor.PreviewLanguageId);
 }
 [Fact] public async Task InvalidSettingsAreRejectedBeforeSaving()
 {
  var project=StudioProject.CreateNew(); project.PageSettings.DefaultName="Naam\nUserInfoPage=yes";
  var error=await Assert.ThrowsAsync<InvalidDataException>(()=>ProjectFile.SaveAsync("unused.issstudio",project));
  Assert.Equal("PageSettingsInvalid",ProjectFile.ErrorResourceKey(error));
 }
 [Fact] public async Task NativeCompilerAcceptsUserInformationAndAllGroupPolicies()
 {
  var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
  try
  {
   using var editor=new EditorSession(StudioProject.CreateNew());
   editor.Project.InstallFile.Source=typeof(PageSettingsTests).Assembly.Location;
   var form=new WizardPageSettingsEditor(editor,PageSettingsSection.UserInfo);
   form.UserInfoEnabled=true; form.DefaultName="Test gebruiker"; form.DefaultOrganization="Test organisatie";
   form.UsePreviousUserInfo=false; form.DefaultGroup="{AppName}"; form.AllowNoIcons=true; form.UsePreviousGroup=false;
   foreach(var mode in Enum.GetValues<ProgramGroupPageMode>())
   {
    form.GroupMode=mode;
    var script=InnoScript.Generate(editor.Project,null);
    // Test-only shortcut makes the native group page applicable; never added to the user's project.
    script+="\r\n[Icons]\r\nName: \"{group}\\Test\"; Filename: \"{app}\\Studio.Core.Tests.dll\"\r\n";
    var path=Path.Combine(folder,mode+".iss"); await File.WriteAllTextAsync(path,script);
    var start=new ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=folder };
    start.ArgumentList.Add("/Qp"); start.ArgumentList.Add(path);
    using var process=Process.Start(start)!; var output=process.StandardOutput.ReadToEndAsync(); var errors=process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync(); Assert.True(process.ExitCode==0,await output+await errors);
   }
  }
  finally { Directory.Delete(folder,true); }
 }
}
