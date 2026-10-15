using Studio.Core;
namespace Studio.Core.Tests;
public class DocumentContentTests
{
    [Fact] public async Task TextReaderAcceptsBomAndRejectsBinaryAndOversizedInput()
    {
        var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            var path=Path.Combine(folder,"document.txt");
            await File.WriteAllTextAsync(path,"Nederlandse tekst",System.Text.Encoding.Unicode);
            Assert.Equal("Nederlandse tekst",(await WizardDocumentContent.LoadAsync(path,null)).Text);
            await File.WriteAllBytesAsync(path,[65,0,66]);
            var binary=await Assert.ThrowsAsync<InvalidDataException>(()=>WizardDocumentContent.LoadAsync(path,null));
            Assert.Equal("DocumentPreviewFailed",ProjectFile.ErrorResourceKey(binary));
            await File.WriteAllBytesAsync(path,new byte[4_194_305]);
            var large=await Assert.ThrowsAsync<InvalidDataException>(()=>WizardDocumentContent.LoadAsync(path,null));
            Assert.Equal("DocumentPreviewLarge",ProjectFile.ErrorResourceKey(large));
        }
        finally { Directory.Delete(folder,true); }
    }
    [Fact] public async Task MissingAndUnsupportedSourcesFailAndRelativeRtfBytesArePreserved()
    {
        var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(()=>WizardDocumentContent.LoadAsync("missing.txt",folder));
            await Assert.ThrowsAsync<InvalidDataException>(()=>WizardDocumentContent.LoadAsync("image.png",folder));
            var bytes=System.Text.Encoding.ASCII.GetBytes(@"{\rtf1 Example}");
            await File.WriteAllBytesAsync(Path.Combine(folder,"example.rtf"),bytes);
            var content=await WizardDocumentContent.LoadAsync("example.rtf",folder);
            Assert.True(content.IsRtf); Assert.Equal(bytes,content.Bytes); Assert.Null(content.Text);
            await File.WriteAllTextAsync(Path.Combine(folder,"invalid.rtf"),"This is not RTF data");
            var invalid=await Assert.ThrowsAsync<InvalidDataException>(()=>WizardDocumentContent.LoadAsync("invalid.rtf",folder));
            Assert.Equal("DocumentPreviewFailed",ProjectFile.ErrorResourceKey(invalid));
        }
        finally { Directory.Delete(folder,true); }
    }
}
