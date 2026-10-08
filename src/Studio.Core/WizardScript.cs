using System.Globalization;
using System.Text;

namespace Studio.Core;

/// <summary>Uses documented wizard controls without replacing their native navigation.</summary>
internal static class WizardScript
{
    public static string Generate(StudioProject project)
    {
        var welcome = project.Pages.Single(p => p.Kind == PageKind.Welcome);
        var destination = project.Pages.Single(p => p.Kind == PageKind.Destination);
        var settings = destination.Destination!;
        var lines = new List<string>
        {
            "", "[Code]",
            "procedure InitializeWizard;",
            "var", "  HeightChange: Integer;", "begin",
            Assign("WelcomeLabel1", "Caption", welcome.Title),
            "  HeightChange := WizardForm.AdjustLabelHeight(WizardForm.WelcomeLabel1);",
            "  WizardForm.IncTopDecHeight(WizardForm.WelcomeLabel2, HeightChange);",
            Assign("WelcomeLabel2", "Caption", welcome.Body),
            Assign("SelectDirLabel", "Caption", destination.Body),
            "  HeightChange := WizardForm.AdjustLabelHeight(WizardForm.SelectDirLabel);",
            "  if HeightChange < 0 then HeightChange := 0;",
            "  WizardForm.SelectDirBrowseLabel.Top := WizardForm.SelectDirBrowseLabel.Top + HeightChange;",
            "  WizardForm.DirEdit.Top := WizardForm.DirEdit.Top + HeightChange;",
            "  WizardForm.DirBrowseButton.Top := WizardForm.DirBrowseButton.Top + HeightChange;",
            Assign("DirBrowseButton", "Caption", settings.BrowseCaption),
            Assign("DirBrowseButton", "Hint", settings.BrowseTooltip),
            "  WizardForm.DirBrowseButton.ShowHint := " + (settings.BrowseTooltip.Length == 0 ? "False" : "True") + ";",
            "end;", "",
            "procedure CurPageChanged(CurPageID: Integer);", "begin",
            // Native page changes reset captions first. Reset our shared values before local overrides.
            Assign("BackButton", "Caption", project.Buttons[ElementKind.Back]),
            Assign("CancelButton", "Caption", project.Buttons[ElementKind.Cancel])
        };
        AddPage(welcome, "wpWelcome");
        AddPage(destination, "wpSelectDir");
        lines.Add("end;");
        lines.Add("");
        return string.Join("\r\n", lines);

        void AddPage(StudioPage page, string id)
        {
            lines.Add("  if CurPageID = " + id + " then");
            lines.Add("  begin");
            if (page.Kind == PageKind.Destination)
                lines.Add("  " + Assign("PageNameLabel", "Caption", page.Title));
            foreach (var button in new[] { ElementKind.Back, ElementKind.Next, ElementKind.Cancel })
                lines.Add("  " + Assign(button + "Button", "Caption", project.Caption(page, button)));
            lines.Add("  end;");
        }
    }

    private static string Assign(string control, string property, string value) =>
        "  WizardForm." + control + "." + property + " := " + Text(value) + ";";

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
