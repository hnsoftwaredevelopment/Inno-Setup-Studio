namespace Studio.Core;

/// <summary>Only properties with a proven native translation are offered for editing.</summary>
public static class WizardPropertyScope
{
    public static bool CanEdit(PageKind? page, ElementKind element)
    {
        if (!Enum.IsDefined(element) || page is { } kind && !Enum.IsDefined(kind)) return false;
        var button = element is ElementKind.Back or ElementKind.Next or ElementKind.Cancel;
        return page switch
        {
            null => button,
            PageKind.Welcome => button || element is ElementKind.Title or ElementKind.Body,
            PageKind.Destination => true,
            _ => false
        };
    }

    public static string Describe(StudioLocalizer text, PageKind? page, ElementKind element)
    {
        if (!CanEdit(page, element)) return text["ScopeUnavailable"];
        return text[(page, element) switch
        {
            (null, ElementKind.Next) => "ScopeSharedNext",
            (null, _) => "ScopeSharedBackCancel",
            (_, ElementKind.Back or ElementKind.Next or ElementKind.Cancel) => "ScopeLocalButton",
            (_, ElementKind.Directory) => "ScopeDirectory",
            (_, ElementKind.Browse) => "ScopeBrowse",
            _ => "ScopePageText"
        }];
    }
}
