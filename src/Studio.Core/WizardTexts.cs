namespace Studio.Core;

public enum WizardTextSlot
{
    SharedBack, SharedNext, SharedCancel, WelcomeTitle, WelcomeBody,
    WelcomeBack, WelcomeNext, WelcomeCancel, DestinationTitle, DestinationBody,
    DestinationBrowse, DestinationTooltip, DestinationBack, DestinationNext, DestinationCancel
}

public enum WizardTextOrigin { Shared, Local, Standard, Fallback, BuiltIn }
public sealed record ResolvedWizardText(string RawText, string Text, WizardTextOrigin Origin, string LanguageId, bool Missing);

/// <summary>One resolution rule for the editor, translation status and script generation.</summary>
public static class WizardTexts
{
    public static WizardTextSlot Slot(PageKind? page, ElementKind element) => (page, element) switch
    {
        (_, ElementKind.Title) => page == PageKind.Welcome ? WizardTextSlot.WelcomeTitle : WizardTextSlot.DestinationTitle,
        (_, ElementKind.Body) => page == PageKind.Welcome ? WizardTextSlot.WelcomeBody : WizardTextSlot.DestinationBody,
        (_, ElementKind.Browse) => WizardTextSlot.DestinationBrowse,
        _ => Enum.Parse<WizardTextSlot>((page?.ToString() ?? "Shared") + element)
    };

    private static WizardTextSlot? Shared(WizardTextSlot slot) => slot switch
    {
        WizardTextSlot.WelcomeBack or WizardTextSlot.DestinationBack => WizardTextSlot.SharedBack,
        WizardTextSlot.WelcomeNext or WizardTextSlot.DestinationNext => WizardTextSlot.SharedNext,
        WizardTextSlot.WelcomeCancel or WizardTextSlot.DestinationCancel => WizardTextSlot.SharedCancel,
        _ => null
    };

    private static string? NativeKey(WizardTextSlot slot) => slot switch
    {
        WizardTextSlot.WelcomeTitle => "WelcomeLabel1", WizardTextSlot.WelcomeBody => "WelcomeLabel2",
        WizardTextSlot.DestinationTitle => "WizardSelectDir", WizardTextSlot.DestinationBody => "SelectDirLabel3",
        WizardTextSlot.DestinationBrowse => "ButtonBrowse", WizardTextSlot.DestinationTooltip => null,
        WizardTextSlot.SharedBack or WizardTextSlot.WelcomeBack or WizardTextSlot.DestinationBack => "ButtonBack",
        WizardTextSlot.SharedNext or WizardTextSlot.WelcomeNext or WizardTextSlot.DestinationNext => "ButtonNext",
        _ => "ButtonCancel"
    };

    public static string? Own(StudioProject project, InstallerLanguage language, WizardTextSlot slot)
    {
        if (language.TextOverrides.TryGetValue(slot, out var text)) return text;
        if (language.Id != project.TextLanguageId || language.StandardTexts.Contains(slot)) return null;
        var welcome = project.Pages[0];
        var destination = project.Pages[1];
        return slot switch
        {
            WizardTextSlot.WelcomeTitle => welcome.Title, WizardTextSlot.WelcomeBody => welcome.Body,
            WizardTextSlot.DestinationTitle => destination.Title, WizardTextSlot.DestinationBody => destination.Body,
            WizardTextSlot.DestinationBrowse => destination.Destination!.BrowseCaption,
            WizardTextSlot.DestinationTooltip => destination.Destination!.BrowseTooltip,
            WizardTextSlot.SharedBack => project.Buttons[ElementKind.Back],
            WizardTextSlot.SharedNext => project.Buttons[ElementKind.Next],
            WizardTextSlot.SharedCancel => project.Buttons[ElementKind.Cancel],
            _ => (slot.ToString().StartsWith("Welcome", StringComparison.Ordinal) ? welcome : destination)
                .ButtonOverrides.GetValueOrDefault(Enum.Parse<ElementKind>(slot.ToString().Replace("Welcome", "", StringComparison.Ordinal)
                    .Replace("Destination", "", StringComparison.Ordinal)))
        };
    }

    public static ResolvedWizardText Resolve(StudioProject project, string languageId, WizardTextSlot slot)
    {
        var language = project.Languages.Single(l => l.Id == languageId);
        var fallback = project.Languages.First(l => l.Enabled);
        var own = Own(project, language, slot);
        var sharedSlot = Shared(slot);
        var shared = sharedSlot is { } s ? Own(project, language, s) : null;
        if (!string.IsNullOrWhiteSpace(own)) return Result(own, sharedSlot is null && slot.ToString().StartsWith("Shared", StringComparison.Ordinal)
            ? WizardTextOrigin.Shared : WizardTextOrigin.Local, language.Id, false);
        if (!string.IsNullOrWhiteSpace(shared)) return Result(shared, WizardTextOrigin.Shared, language.Id, own is not null);
        var defaultOwn = Own(project, fallback, slot);
        var defaultShared = sharedSlot is { } key ? Own(project, fallback, key) : null;
        var missingOwn = own is not null || shared is not null || (language.Id != fallback.Id
            && (!string.IsNullOrWhiteSpace(defaultOwn) || !string.IsNullOrWhiteSpace(defaultShared)));
        var nativeKey = NativeKey(slot);
        if (nativeKey is not null && language.Messages.ContainsKey(nativeKey)
            && !string.IsNullOrWhiteSpace(language.Message(nativeKey, project)))
            return NativeMessage(project, languageId, nativeKey) with { Missing = missingOwn };
        if (language.Id != fallback.Id)
        {
            if (!string.IsNullOrWhiteSpace(defaultOwn)) return Result(defaultOwn, WizardTextOrigin.Fallback, fallback.Id, true);
            if (!string.IsNullOrWhiteSpace(defaultShared)) return Result(defaultShared, WizardTextOrigin.Fallback, fallback.Id, true);
        }
        // No tooltip is a supported optional state; visible labels and buttons always need text.
        if (nativeKey is null) return Result("", WizardTextOrigin.Standard, language.Id, false);
        var resolved = NativeMessage(project, languageId, nativeKey);
        return resolved with { Missing = resolved.Missing || own is not null || shared is not null };
        ResolvedWizardText Result(string raw, WizardTextOrigin origin, string sourceId, bool missing) =>
            new(raw, Expand(raw, project), origin, sourceId, missing);
    }

    public static ResolvedWizardText NativeMessage(StudioProject project, string languageId, string key)
    {
        var language = project.Languages.Single(l => l.Id == languageId);
        var fallback = project.Languages.First(l => l.Enabled);
        if (HasVisibleText(language))
            return Result(language, WizardTextOrigin.Standard, false);
        if (HasVisibleText(fallback))
            return Result(fallback, WizardTextOrigin.Fallback, true);
        return Result(InstallerLanguage.English(), WizardTextOrigin.BuiltIn, true);

        bool HasVisibleText(InstallerLanguage source) => source.Messages.ContainsKey(key)
            && !string.IsNullOrWhiteSpace(source.Message(key, project));

        ResolvedWizardText Result(InstallerLanguage source, WizardTextOrigin origin, bool missing)
        {
            var text = source.Message(key, project);
            return new(text, text, origin, source.Id, missing);
        }
    }

    public static string Expand(string text, StudioProject project) =>
        System.Text.RegularExpressions.Regex.Replace(text, @"\{AppName\}|\{AppVersion\}",
            match => match.Value == "{AppName}" ? project.Name : project.AppVersion);

    public static int MissingCount(StudioProject project, string languageId)
    {
        var language = project.Languages.Single(l => l.Id == languageId);
        var fallback = project.Languages.First(l => l.Enabled);
        return (NativeMessage(project, languageId, "SetupWindowTitle").Missing ? 1 : 0) + Enum.GetValues<WizardTextSlot>().Count(slot =>
            (Shared(slot) is null || Own(project, language, slot) is not null || Own(project, fallback, slot) is not null)
            && Resolve(project, languageId, slot).Missing);
    }

    public static void Set(StudioProject project, InstallerLanguage language, WizardTextSlot slot, string value)
    {
        language.StandardTexts.Remove(slot);
        if (language.Id != project.TextLanguageId) { language.TextOverrides[slot] = value; return; }
        language.TextOverrides.Remove(slot);
        var welcome = project.Pages[0];
        var destination = project.Pages[1];
        switch (slot)
        {
            case WizardTextSlot.WelcomeTitle: welcome.Title = value; break;
            case WizardTextSlot.WelcomeBody: welcome.Body = value; break;
            case WizardTextSlot.DestinationTitle: destination.Title = value; break;
            case WizardTextSlot.DestinationBody: destination.Body = value; break;
            case WizardTextSlot.DestinationBrowse: destination.Destination!.BrowseCaption = value; break;
            case WizardTextSlot.DestinationTooltip: destination.Destination!.BrowseTooltip = value; break;
            case WizardTextSlot.SharedBack: project.Buttons[ElementKind.Back] = value; break;
            case WizardTextSlot.SharedNext: project.Buttons[ElementKind.Next] = value; break;
            case WizardTextSlot.SharedCancel: project.Buttons[ElementKind.Cancel] = value; break;
            default:
                var page = slot.ToString().StartsWith("Welcome", StringComparison.Ordinal) ? welcome : destination;
                page.ButtonOverrides[Enum.Parse<ElementKind>(slot.ToString().Replace(page.Kind.ToString(), "", StringComparison.Ordinal))] = value;
                break;
        }
    }

    public static void Reset(StudioProject project, InstallerLanguage language, WizardTextSlot slot)
    {
        language.TextOverrides.Remove(slot);
        if (language.Id != project.TextLanguageId) return;
        if (Shared(slot) is not null)
        {
            var page = slot.ToString().StartsWith("Welcome", StringComparison.Ordinal) ? project.Pages[0] : project.Pages[1];
            page.ButtonOverrides.Remove(Enum.Parse<ElementKind>(slot.ToString().Replace(page.Kind.ToString(), "", StringComparison.Ordinal)));
        }
        else language.StandardTexts.Add(slot);
    }
}
