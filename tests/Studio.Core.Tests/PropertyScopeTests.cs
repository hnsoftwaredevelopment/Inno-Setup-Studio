using Studio.Core;
using System.Diagnostics;
namespace Studio.Core.Tests;
public class PropertyScopeTests
{
 [Fact] public async Task NativeCompilerAcceptsSharedAndLocalCaptionsWithDocumentAndUserInfoPages()
 {
  var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
  try
  {
   using var editor=new EditorSession(StudioProject.CreateNew()); editor.AddLanguage(InstallerLanguage.Dutch());
   editor.Project.InstallFile.Source=typeof(PropertyScopeTests).Assembly.Location;
   editor.Project.PageSettings.UserInfoEnabled=true;
   var document=Path.Combine(folder,"document.txt"); await File.WriteAllTextAsync(document,"Native document page");
   foreach(var kind in Enum.GetValues<WizardDocumentKind>()) editor.Project.Documents[kind].Source=document;
   foreach(var language in new[]{"english","dutch"})
   {
    editor.SetTranslation(language,WizardTextSlot.SharedNext,language+" shared & next");
    editor.SetTranslation(language,WizardTextSlot.WelcomeNext,language+" welcome");
    editor.SetTranslation(language,WizardTextSlot.DestinationNext,language+" directory");
   }
   var path=Path.Combine(folder,"scope.iss"); await File.WriteAllTextAsync(path,InnoScript.Generate(editor.Project,folder));
   var start=new ProcessStartInfo(@"C:\Program Files\Inno Setup 7\ISCC.exe") { UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=folder };
   start.ArgumentList.Add("/Qp"); start.ArgumentList.Add(path);
   using var process=Process.Start(start)!;
   var output=process.StandardOutput.ReadToEndAsync(); var errors=process.StandardError.ReadToEndAsync();
   await process.WaitForExitAsync(); Assert.True(process.ExitCode==0,await output+await errors);
   Assert.Equal("Native document page",await File.ReadAllTextAsync(document));
  }
  finally { Directory.Delete(folder,true); }
 }
 [Fact] public void SharedNextTargetsOnlyOrdinaryNativePagesAndLocalOverridesFollowReset()
 {
  using var editor=new EditorSession(StudioProject.CreateNew());
  editor.Project.InstallFile.Source=typeof(PropertyScopeTests).Assembly.Location;
  editor.SetTranslation("english",WizardTextSlot.SharedNext,"Shared next");
  editor.SetTranslation("english",WizardTextSlot.WelcomeNext,"Welcome next");
  editor.SetTranslation("english",WizardTextSlot.DestinationNext,"Directory next");
  var script=InnoScript.Generate(editor.Project,null);
  Assert.Contains("if CurPageID in [wpWelcome, wpLicense, wpPassword, wpInfoBefore, wpUserInfo, wpSelectDir, wpSelectComponents, wpSelectProgramGroup, wpSelectTasks, wpInfoAfter] then",script);
  var shared=script.IndexOf("NextButton.Caption := 'Shared next';",StringComparison.Ordinal);
  var welcome=script.IndexOf("if CurPageID = wpWelcome then",StringComparison.Ordinal);
  Assert.True(shared>=0 && shared<welcome);
  Assert.DoesNotContain("wpReady",script); Assert.DoesNotContain("wpFinished",script); Assert.DoesNotContain("wpPreparing",script);
 }
 [Theory]
 [InlineData(null,ElementKind.Title,false)]
 [InlineData(null,ElementKind.Next,true)]
 [InlineData(PageKind.Welcome,ElementKind.Directory,false)]
 [InlineData(PageKind.Welcome,ElementKind.Cancel,true)]
 [InlineData(PageKind.Destination,ElementKind.Browse,true)]
 [InlineData(PageKind.Destination,(ElementKind)999,false)]
 public void OnlyProvenPropertiesCanBeEdited(PageKind? page,ElementKind element,bool expected)
 { Assert.Equal(expected,WizardPropertyScope.CanEdit(page,element)); }
 [Fact] public void ResetIsBoundToTheSelectedLanguageAndPageAndBackNavigationRestoresValues()
 {
  using var editor=new EditorSession(StudioProject.CreateNew()); editor.AddLanguage(InstallerLanguage.Dutch());
  editor.SetTranslation("english",WizardTextSlot.SharedNext,"EN shared");
  editor.SetTranslation("dutch",WizardTextSlot.SharedNext,"NL shared");
  editor.PageIndex=1; editor.SelectedElement=ElementKind.Next; editor.PropertyText="EN welcome";
  editor.PreviewLanguageId="dutch"; editor.PropertyText="NL welcome";
  editor.PageIndex=2; Assert.Equal("NL shared",editor.NextCaption); editor.PropertyText="NL directory";
  editor.PageIndex=1; Assert.Equal("NL welcome",editor.NextCaption); editor.ResetProperty();
  Assert.Equal("NL shared",editor.NextCaption); editor.PageIndex=2; Assert.Equal("NL directory",editor.NextCaption);
  editor.PreviewLanguageId="english"; editor.PageIndex=1; Assert.Equal("EN welcome",editor.NextCaption);
 }
}
