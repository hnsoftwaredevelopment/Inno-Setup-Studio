namespace Studio.Core;
[Flags] public enum DirectoryRuleAttributes { None=0,ReadOnly=1,Hidden=2,System=4,NotContentIndexed=8 }
public enum DirectoryUninstall { NativeDefault,Keep,RemoveExistingEmpty }
public enum DirectoryCompression { Unchanged,Enable,Disable }
public sealed class DirectoryRule
{
    public string Name { get; set; }=@"{app}\Data";
    public DirectoryRuleAttributes Attributes { get; set; }
    public DirectoryUninstall Uninstall { get; set; }
    public DirectoryCompression Compression { get; set; }
    public bool DeleteAfterInstall { get; set; }
    public static void Validate(DirectoryRule? rule)
    {
        if(rule is null || rule.Name is null || rule.Name.Length>4096 || !Enum.IsDefined(rule.Uninstall) || !Enum.IsDefined(rule.Compression)
            || (rule.Attributes&~(DirectoryRuleAttributes)15)!=0) throw InnoScript.Error("DirectoryRuleInvalid");
        try { _=FileCollections.Destination(rule.Name); }
        catch(InvalidDataException) { throw InnoScript.Error("DirectoryRuleInvalid"); }
    }
    internal string Script()
    {
        Validate(this);
        var line="Name: "+InnoScript.Parameter(FileCollections.Destination(Name));
        var attributes=new List<string>();
        foreach(var (flag,name) in new[]{(DirectoryRuleAttributes.ReadOnly,"readonly"),(DirectoryRuleAttributes.Hidden,"hidden"),
            (DirectoryRuleAttributes.System,"system"),(DirectoryRuleAttributes.NotContentIndexed,"notcontentindexed")})
            if(Attributes.HasFlag(flag)) attributes.Add(name);
        if(attributes.Count>0) line+="; Attribs: "+string.Join(" ",attributes);
        var flags=new List<string>();
        if(DeleteAfterInstall) flags.Add("deleteafterinstall");
        if(Compression==DirectoryCompression.Enable) flags.Add("setntfscompression");
        if(Compression==DirectoryCompression.Disable) flags.Add("unsetntfscompression");
        if(Uninstall==DirectoryUninstall.Keep) flags.Add("uninsneveruninstall");
        if(Uninstall==DirectoryUninstall.RemoveExistingEmpty) flags.Add("uninsalwaysuninstall");
        if(flags.Count>0) line+="; Flags: "+string.Join(" ",flags);
        return line;
    }
}
