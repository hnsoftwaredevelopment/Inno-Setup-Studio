using System.ComponentModel;

namespace Studio.Core;

/// <summary>A stable matrix over the editor's live project; no separate translation draft.</summary>
public sealed class TranslationOverview : INotifyPropertyChanged, IDisposable
{
    private readonly EditorSession _editor;
    public TranslationOverview(EditorSession editor)
    {
        _editor = editor;
        Rows = Enum.GetValues<WizardTextSlot>().Select(slot => new TranslationRow(editor, slot)).ToArray();
        _editor.PropertyChanged += EditorChanged;
    }
    public IReadOnlyList<TranslationRow> Rows { get; }
    public int ActiveIssueCount => Rows.SelectMany(row => row.Cells).Where(cell => cell.Enabled).Sum(cell => cell.Issues.Count);
    public string Summary => _editor.TranslationSummary + "\n" + _editor.Text.Format("TranslationOverviewIssues", ActiveIssueCount);
    public event PropertyChangedEventHandler? PropertyChanged;
    private void EditorChanged(object? sender, PropertyChangedEventArgs e)
    {
        foreach (var row in Rows) row.Refresh();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
    public void Dispose() => _editor.PropertyChanged -= EditorChanged;
}

public sealed class TranslationRow : INotifyPropertyChanged
{
    private readonly EditorSession _editor;
    public TranslationRow(EditorSession editor, WizardTextSlot slot)
    {
        _editor = editor; Slot = slot;
        Cells = editor.Project.Languages.Select(language => new TranslationCell(editor, language, slot)).ToArray();
    }
    public WizardTextSlot Slot { get; }
    public IReadOnlyList<TranslationCell> Cells { get; }
    public string Name => DescribeSlot(_editor.Text, Slot);
    internal static string DescribeSlot(StudioLocalizer text, WizardTextSlot slot)
    {
        var name = slot.ToString();
        var scope = name.StartsWith("Shared", StringComparison.Ordinal) ? "BaseDesign"
            : name.StartsWith("Welcome", StringComparison.Ordinal) ? "PageWelcome" : "ElementDirectory";
        var element = name.Replace("Shared", "", StringComparison.Ordinal).Replace("Welcome", "", StringComparison.Ordinal)
            .Replace("Destination", "", StringComparison.Ordinal);
        return text[scope] + "\n" + text[element == "Tooltip" ? "TooltipLabel" : "Element" + element];
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh()
    {
        foreach (var cell in Cells) cell.Refresh();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}

public sealed class TranslationCell(EditorSession editor, InstallerLanguage language, WizardTextSlot slot) : INotifyPropertyChanged
{
    public string LanguageId => language.Id;
    public bool Enabled => language.Enabled;
    public bool CanEdit => editor.IsDesign;
    public bool AllowsMultiline => slot is WizardTextSlot.WelcomeBody or WizardTextSlot.DestinationBody or WizardTextSlot.DestinationTooltip;
    public string Header => language.DisplayName(editor.Text) + (language.Id == editor.DefaultLanguageId
        ? "\n" + editor.Text["DefaultInstallerLanguage"] : !Enabled ? "\n" + editor.Text["TranslationDisabled"] : "");
    public string Text
    {
        get => WizardTexts.Own(editor.Project, language, slot) ?? "";
        set => editor.SetTranslation(language.Id, slot, value);
    }
    private ResolvedWizardText Resolved => WizardTexts.Resolve(editor.Project, language.Id, slot);
    public string EffectiveText => Resolved.Text;
    public bool ShowWatermark => string.IsNullOrEmpty(Text);
    public bool ShowCopy => string.IsNullOrEmpty(Text);
    public bool ShowReset => !string.IsNullOrEmpty(Text);
    public bool CanCopy => CanEdit && ShowCopy && !string.IsNullOrEmpty(EffectiveText);
    public IReadOnlyList<TranslationIssue> Issues => TranslationValidation.Check(editor.Project, language.Id, slot);
    public string Warnings => string.Join("\n", Issues.Select(issue => issue.Describe(editor.Text)));
    public bool HasWarnings => Issues.Count > 0;
    public string Status
    {
        get
        {
            var resolved = Resolved;
            var origin = resolved.Origin switch
            {
                WizardTextOrigin.Local => editor.Text["TranslationCellCustom"],
                WizardTextOrigin.Shared => editor.Text["OriginInherited"],
                WizardTextOrigin.Fallback => editor.Text.Format("TranslationOriginFallback", editor.DefaultLanguageName),
                WizardTextOrigin.BuiltIn => editor.Text["TranslationOriginBuiltIn"],
                _ => editor.Text["TranslationOriginNative"]
            };
            return (resolved.Missing ? editor.Text["TranslationCellMissing"] + " · " : "") + origin;
        }
    }
    public string AccessibleName => Header + " · " + TranslationRow.DescribeSlot(editor.Text, slot);
    public void Copy()
    {
        if (CanCopy) editor.SetTranslation(language.Id, slot, Resolved.RawText);
    }
    public void Reset() => editor.ResetTranslation(language.Id, slot);
    public event PropertyChangedEventHandler? PropertyChanged;
    internal void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
}
