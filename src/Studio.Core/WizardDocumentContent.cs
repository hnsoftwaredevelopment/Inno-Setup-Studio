using System.Text;
namespace Studio.Core;

/// <summary>Bounded, read-only document input shared by configuration and trial previews.</summary>
public sealed record WizardDocumentContent(byte[] Bytes, string? Text, bool IsRtf)
{
    public static async Task<WizardDocumentContent> LoadAsync(string source,string? directory,CancellationToken cancellationToken=default)
    {
        var path=WizardDocuments.Resolve(source,directory);
        await using var stream=File.OpenRead(path);
        if(stream.Length>4_194_304) throw InnoScript.Error("DocumentPreviewLarge");
        var bytes=new byte[(int)stream.Length]; await stream.ReadExactlyAsync(bytes,cancellationToken);
        if(Path.GetExtension(path).Equals(".rtf",StringComparison.OrdinalIgnoreCase))
        {
            var header=Encoding.ASCII.GetString(bytes,0,Math.Min(bytes.Length,64)).TrimStart();
            if(!header.StartsWith(@"{\rtf",StringComparison.Ordinal) || header.Length<6 || !char.IsAsciiDigit(header[5]))
                throw InnoScript.Error("DocumentPreviewFailed");
            return new(bytes,null,true);
        }
        try
        {
            using var reader=new StreamReader(new MemoryStream(bytes),new UTF8Encoding(false,true),true);
            var text=await reader.ReadToEndAsync(cancellationToken);
            if(text.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t' or '\f'))) throw InnoScript.Error("DocumentPreviewFailed");
            return new(bytes,text,false);
        }
        catch(DecoderFallbackException) { throw InnoScript.Error("DocumentPreviewFailed"); }
    }
}
