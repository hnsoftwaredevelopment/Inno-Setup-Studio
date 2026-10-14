using System.Globalization;
using System.Text;

namespace Studio.Core;

/// <summary>Uses documented wizard controls without replacing their native navigation.</summary>
internal static class WizardScript
{
    public static string Generate(StudioProject project, IReadOnlyList<ExportLanguage> languages)
    {
        var welcome = project.Pages.Single(p => p.Kind == PageKind.Welcome);
        var destination = project.Pages.Single(p => p.Kind == PageKind.Destination);
        string Resolve(ExportLanguage language, PageKind? page, ElementKind element) =>
            WizardTexts.Resolve(project, language.Language.Id, WizardTexts.Slot(page, element)).Text;
        var lines = new List<string>
        {
            "", "[Code]",
            "procedure InitializeWizard;",
            "var", "  HeightChange: Integer;", "begin"
        };
        ForEachLanguage(language =>
        {
            var tooltip = WizardTexts.Resolve(project, language.Language.Id, WizardTextSlot.DestinationTooltip).Text;
            lines.Add("  WizardForm.Caption := " + Text(WizardTexts.NativeMessage(project, language.Language.Id, "SetupWindowTitle").Text) + ";");
            lines.Add(Assign("WelcomeLabel1", "Caption", Resolve(language, PageKind.Welcome, ElementKind.Title)));
            lines.Add(Assign("WelcomeLabel2", "Caption", Resolve(language, PageKind.Welcome, ElementKind.Body)));
            lines.Add(Assign("SelectDirLabel", "Caption", Resolve(language, PageKind.Destination, ElementKind.Body)));
            lines.Add(Assign("DirBrowseButton", "Caption", Resolve(language, PageKind.Destination, ElementKind.Browse)));
            lines.Add(Assign("DirBrowseButton", "Hint", tooltip));
            lines.Add("  WizardForm.DirBrowseButton.ShowHint := " + (tooltip.Length == 0 ? "False" : "True") + ";");
        });
        lines.AddRange([
            "  HeightChange := WizardForm.AdjustLabelHeight(WizardForm.WelcomeLabel1);",
            "  WizardForm.IncTopDecHeight(WizardForm.WelcomeLabel2, HeightChange);",
            "  HeightChange := WizardForm.AdjustLabelHeight(WizardForm.SelectDirLabel);",
            "  if HeightChange < 0 then HeightChange := 0;",
            "  WizardForm.SelectDirBrowseLabel.Top := WizardForm.SelectDirBrowseLabel.Top + HeightChange;",
            "  WizardForm.DirEdit.Top := WizardForm.DirEdit.Top + HeightChange;",
            "  WizardForm.DirBrowseButton.Top := WizardForm.DirBrowseButton.Top + HeightChange;",
            "end;", "",
            "procedure CurPageChanged(CurPageID: Integer);", "begin"
        ]);
        ForEachLanguage(language =>
        {
            // Native page changes reset captions first. Reset our shared values before local overrides.
            lines.Add(Assign("BackButton", "Caption", Resolve(language, null, ElementKind.Back)));
            lines.Add(Assign("CancelButton", "Caption", Resolve(language, null, ElementKind.Cancel)));
            AddPage(language, welcome, "wpWelcome");
            AddPage(language, destination, "wpSelectDir");
        });
        lines.Add("end;");
        lines.Add("");
        return string.Join("\r\n", lines);

        void ForEachLanguage(Action<ExportLanguage> write)
        {
            foreach (var language in languages)
            {
                if (languages.Count > 1)
                {
                    lines.Add("  if ActiveLanguage = " + Text(language.Name) + " then");
                    lines.Add("  begin");
                }
                write(language);
                if (languages.Count > 1) lines.Add("  end;");
            }
        }

        void AddPage(ExportLanguage language, StudioPage page, string id)
        {
            lines.Add("  if CurPageID = " + id + " then");
            lines.Add("  begin");
            if (page.Kind == PageKind.Destination)
                lines.Add("  " + Assign("PageNameLabel", "Caption", Resolve(language, page.Kind, ElementKind.Title)));
            foreach (var button in new[] { ElementKind.Back, ElementKind.Next, ElementKind.Cancel })
                lines.Add("  " + Assign(button + "Button", "Caption", Resolve(language, page.Kind, button)));
            lines.Add("  end;");
        }
    }

    private static string Assign(string control, string property, string value)
    {
        // Button captions interpret '&' as an accelerator; Studio text is literal.
        if (property == "Caption" && control.EndsWith("Button", StringComparison.Ordinal)) value = value.Replace("&", "&&", StringComparison.Ordinal);
        return "  WizardForm." + control + "." + property + " := " + Text(value) + ";";
    }

    private static string Text(string value)
    {
        var tokens = new List<string>();
        var chunk = new StringBuilder();
        // Encode # as a character code so even literal {#...} cannot invoke ISPP.
        foreach (var character in value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'))
        {
            if (character == '#' || char.IsControl(character))
            {
                Flush();
                if (tokens.Count == 0) tokens.Add("''");
                if (character == '\n') tokens.Add("#13");
                tokens.Add("#" + ((int)character).ToString(CultureInfo.InvariantCulture));
            }
            else chunk.Append(character == '\'' ? "''" : character.ToString());
        }
        Flush();
        return tokens.Count == 0 ? "''" : string.Join(" + ", tokens);

        void Flush()
        {
            if (chunk.Length == 0) return;
            tokens.Add("'" + chunk + "'");
            chunk.Clear();
        }
    }
}
