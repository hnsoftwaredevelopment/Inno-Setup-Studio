using System.Text.Json;
using Studio.Core;
namespace Studio.Core.Tests;
public class CompletionPreviewTests
{
 [Fact] public void RolesAndTrialProgressCannotModifyProjectOrSharedNextCaption()
 {
  using var editor=new EditorSession(StudioProject.CreateNew()); editor.PageIndex=2;
  editor.Project.Buttons[ElementKind.Next]="Eigen Verder";
  editor.Project.Languages[0].StandardTexts.Remove(WizardTextSlot.SharedNext);
  editor.MarkSaved(); var before=JsonSerializer.Serialize(editor.Project);
  var preview=new WizardCompletionPreview(editor,CompletionPhase.Ready);
  Assert.Equal("Install",preview.NextCaption); Assert.DoesNotContain("Eigen Verder",preview.NextCaption);
  Assert.Contains(editor.Project.Pages[1].Destination!.DefaultDirectory,preview.Summary);
  preview.Next(); Assert.Equal(CompletionPhase.Installing,preview.Phase); Assert.False(preview.ShowNext);
  Assert.False(preview.CanComplete); preview.Progress=100; preview.CompleteInstallation();
  Assert.Equal(CompletionPhase.Finished,preview.Phase); Assert.Equal("Finish",preview.NextCaption);
  preview.Next(); Assert.True(preview.ExitRequested);
  Assert.Equal(before,JsonSerializer.Serialize(editor.Project)); Assert.Equal(2,editor.PageIndex);
  Assert.Equal("english",editor.PreviewLanguageId); Assert.False(editor.IsDirty);
 }
 [Fact] public void PreparationProblemBlocksProgressUntilExplicitlyResolved()
 {
  using var editor=new EditorSession(StudioProject.CreateNew());
  var preview=new WizardCompletionPreview(editor,CompletionPhase.Ready) { PreparationProblem=true };
  preview.Next(); Assert.Equal(CompletionPhase.Preparing,preview.Phase); Assert.False(preview.CanNext);
  preview.Next(); Assert.Equal(CompletionPhase.Preparing,preview.Phase);
  preview.ResolvePreparation(); Assert.Equal(CompletionPhase.Installing,preview.Phase);
  preview.Progress=-1; Assert.Equal(0,preview.Progress);
  preview.Progress=150; Assert.Equal(100,preview.Progress);
 }
 [Fact] public async Task NativeLanguageSwitchPreservesEditorStateExportAndSourceBytes()
 {
  var compilerDirectory=@"C:\Program Files\Inno Setup 7";
  var sourcePath=Path.Combine(compilerDirectory,"Languages","German.isl");
  var bytes=await File.ReadAllBytesAsync(sourcePath);
  var source=await LanguageSourceFile.LoadAsync(sourcePath);
  using var editor=new EditorSession(StudioProject.CreateNew());
  var german=InstallerLanguage.FromSource(source,compilerDirectory); editor.AddLanguage(german);
  editor.Project.InstallFile.Source=typeof(CompletionPreviewTests).Assembly.Location;
  editor.MarkSaved(); var before=JsonSerializer.Serialize(editor.Project); var script=InnoScript.Generate(editor.Project,null);
  var preview=new WizardCompletionPreview(editor,CompletionPhase.Ready);
  await preview.LoadLanguageAsync(german.Id,compilerDirectory,null);
  Assert.Equal("Installieren",preview.NextCaption); Assert.False(preview.HasFallback);
  preview.Next(); preview.Progress=100; preview.CompleteInstallation(); Assert.Equal("Fertigstellen",preview.NextCaption);
  Assert.Equal("english",editor.PreviewLanguageId); Assert.False(editor.IsDirty);
  Assert.Equal(before,JsonSerializer.Serialize(editor.Project)); Assert.Equal(script,InnoScript.Generate(editor.Project,null));
  Assert.Equal(bytes,await File.ReadAllBytesAsync(sourcePath));
 }
 [Fact] public async Task MissingSourceUsesVisibleEnglishFallbackAndCancelDoesNotDiscardUnsavedEdits()
 {
  using var editor=new EditorSession(StudioProject.CreateNew()); editor.ProductName="Nog niet opgeslagen";
  var preview=new WizardCompletionPreview(editor,CompletionPhase.Finished);
  await preview.LoadLanguageAsync("english",null,null);
  Assert.True(preview.HasFallback); Assert.Equal("Finish",preview.NextCaption);
  Assert.False(preview.ShowBack); Assert.False(preview.ShowCancel);
  preview.Cancel(); Assert.True(preview.ExitRequested); Assert.True(editor.IsDirty);
  Assert.Equal("Nog niet opgeslagen",editor.ProductName);
 }
 [Fact] public async Task PartialCustomSourceUsesOwnRoleAndFallbackForOtherLabels()
 {
  var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
  try
  {
   var path=Path.Combine(folder,"custom.isl");
   await File.WriteAllTextAsync(path,"[LangOptions]\nLanguageName=Custom\nLanguageID=0\n[Messages]\nButtonInstall=&Custom install\n");
   using var editor=new EditorSession(StudioProject.CreateNew());
   var custom=InstallerLanguage.FromSource(await LanguageSourceFile.LoadAsync(path),null); editor.AddLanguage(custom);
   var preview=new WizardCompletionPreview(editor,CompletionPhase.Ready);
   await preview.LoadLanguageAsync(custom.Id,null,null);
   Assert.Equal("Custom install",preview.NextCaption); Assert.True(preview.HasFallback);
   Assert.DoesNotContain("[WizardReady]",preview.Title);
  }
  finally { Directory.Delete(folder,true); }
 }
}
