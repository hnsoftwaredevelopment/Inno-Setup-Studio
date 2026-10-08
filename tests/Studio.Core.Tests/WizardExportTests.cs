using Studio.Core;

namespace Studio.Core.Tests;

public class WizardExportTests
{
    [Fact]
    public void LocalCaptionsAreScopedAfterSharedResetAndEmptyOverrideIsPreserved()
    {
        var project = StudioProject.CreateExample();
        project.InstallFile.Source = typeof(WizardExportTests).Assembly.Location;
        project.Buttons[ElementKind.Cancel] = "Basis stop";
        project.Pages[0].ButtonOverrides[ElementKind.Cancel] = "Welkom stop";
        project.Pages[1].ButtonOverrides[ElementKind.Next] = "";
        var script = InnoScript.Generate(project, null);
        var reset = script.IndexOf("WizardForm.CancelButton.Caption := 'Basis stop';", StringComparison.Ordinal);
        var welcome = script.IndexOf("if CurPageID = wpWelcome then", StringComparison.Ordinal);
        var destination = script.IndexOf("if CurPageID = wpSelectDir then", StringComparison.Ordinal);
        Assert.True(reset >= 0 && reset < welcome && welcome < destination);
        Assert.Contains("WizardForm.CancelButton.Caption := 'Welkom stop';", script[welcome..destination]);
        Assert.Contains("WizardForm.CancelButton.Caption := 'Basis stop';", script[destination..]);
        Assert.Contains("WizardForm.NextButton.Caption := '';", script[destination..]);
    }

    [Fact]
    public void ExportUsesNativePagesAndKeepsInstallationButtonRoles()
    {
        var project = StudioProject.CreateExample();
        project.InstallFile.Source = typeof(WizardExportTests).Assembly.Location;
        project.Pages[0].ButtonOverrides[ElementKind.Next] = "Welkom verder";
        project.Pages[1].ButtonOverrides[ElementKind.Back] = "Map terug";
        var script = InnoScript.Generate(project, null);
        Assert.Contains("DisableWelcomePage=no", script);
        Assert.Contains("DisableDirPage=no", script);
        Assert.Contains("WizardForm.WelcomeLabel1.Caption :=", script);
        Assert.Contains("WizardForm.SelectDirLabel.Caption :=", script);
        Assert.Contains("WizardForm.DirBrowseButton.Hint :=", script);
        Assert.Contains("if CurPageID = wpWelcome then", script);
        Assert.Contains("if CurPageID = wpSelectDir then", script);
        Assert.Contains("WizardForm.NextButton.Caption := 'Welkom verder';", script);
        Assert.Contains("WizardForm.BackButton.Caption := 'Map terug';", script);
        Assert.DoesNotContain("DirEdit.Text :=", script);
        Assert.DoesNotContain("wpReady", script);
        Assert.DoesNotContain("wpFinished", script);
        Assert.DoesNotContain("OnClick", script);
    }

    [Fact]
    public void WizardTextIsLiteralAndCannotBecomePascalOrPreprocessorCode()
    {
        var project = StudioProject.CreateExample();
        project.InstallFile.Source = typeof(WizardExportTests).Assembly.Location;
        project.Pages[0].Body = "O'Brien\n{#1+1} 50% [name]";
        project.Pages[1].Destination!.BrowseTooltip = "";
        var script = InnoScript.Generate(project, null);
        Assert.Contains("'O''Brien' + #13 + #10 + '{' + #35 + '1+1} 50% [name]'", script);
        Assert.DoesNotContain("{#", script);
        Assert.Contains("WizardForm.DirBrowseButton.ShowHint := False;", script);
        Assert.Equal("O'Brien\n{#1+1} 50% [name]", project.Pages[0].Body);
    }
}
