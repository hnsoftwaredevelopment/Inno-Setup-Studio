# Ondersteuningsmatrix — Inno Setup 7.1

Status: geaccepteerde scope en verificatiekader, 8 oktober 2026. Doelversie: 7.1; lokaal vastgesteld met ISCC --version: 7.1.0.

## Betekenis van ondersteuning

De matrix maakt onderscheid tussen een Studio-functie en bewezen installerondersteuning. Het bestaan van een invoerveld of preview is onvoldoende.

| Niveau | Betekenis |
|---|---|
| Gepland | In scope; nog geen bewezen export |
| Studio-basis | Editor/opslag bestaat, export nog niet bewezen |
| Compiler gecontroleerd | Export met doelcompiler getest |
| Runtime gecontroleerd | Relevante werking in echte installer door gebruiker gecontroleerd |
| Uitgesloten | Buiten afgesproken productgrens |

M01-S02 bevat de eerste scriptgenerator. Applicatiegegevens, het standaardpad, een vaste Nederlandse taalbron en één lokale bestandsregel zijn met ISCC 7.1.0 gecompileerd en door de gebruiker getest en geaccepteerd. De gemelde waarschuwing over setup.exe is nu verwerkt bij M01-S05; runtimebewijs voor de huidige wizardteksten is vastgelegd bij M01-S03. Zie [detailregistratie en bewijs](slices/M01-S02-Bestandsregel-en-export.md).

M01-S03 voegt native tekstexport voor Welkom en Installatiemap toe, met gedeelde en lokale knopteksten en Bladeren-tooltip. Deze eigenschappen zijn Runtime gecontroleerd: de gebruiker heeft Studio en de gecompileerde, uitgevoerde installer getest en M01-S03 geaccepteerd. Zie [propertycatalogus, bereik en grenzen](slices/M01-S03-Wizardontwerp-export.md).

M01-S04 voegt ontdekken/kiezen van ISCC, versiecontrole en bouwen van een opgeslagen projectstand toe. Procesargumenten en afzonderlijke uitvoermappen zijn met ISCC 7.1.0 getest. [Detailregistratie en teststappen](slices/M01-S04-Compiler-kiezen-en-bouwen.md).

M01-S05 voegt een opgeslagen OutputBaseFilename, native JSONL-bouwmeldingen en annuleren toe. De koppelingen zijn met ISCC 7.1.0 getest; de gebruiker heeft de handmatige tests uitgevoerd en M01-S05 geaccepteerd. [Detailregistratie en teststappen](slices/M01-S05-Bouwmeldingen-en-annuleren.md).

M01-S06 voegt exportbescherming en de lokale SVG-eindproef toe. Zelfstandige compilatie met 7.1.0 is geslaagd; de gebruiker heeft de eindproef afgerond en verse installatie/deïnstallatie binnen de M01-scope geaccepteerd. [Eindproef en beperkingen](slices/M01-S06-Eenvoudige-installer-afronden.md).

M02-S01 leest de taalbronnen bij ISCC 7.1.0 en aanvullende zelfstandige .isl-bestanden. Alle lokale compilerbronnen zijn ingelezen zonder wijziging; de broninspectie en aangepaste taallijst zijn door de gebruiker geaccepteerd. Alleen broninspectie is gebouwd; projecttalen, vertalingen en previewkeuze blijven gepland. [Detailregistratie](slices/M02-S01-Beschikbare-taalbronnen.md).

## Functionele dekking

V = visuele bediening; G = gecontroleerde geavanceerde opties; S = officiële scripting. De aangegeven bediening is beoogd, niet allemaal al gerealiseerd.

| Bereik | Beoogde bediening | Inno Setup-koppeling en bron | Milestone | Nu |
|---|---|---|---|---|
| Productidentiteit en versie | V | [AppName](https://jrsoftware.org/ishelp/topic_setup_appname.htm), [AppId](https://jrsoftware.org/ishelp/topic_setup_appid.htm), [AppVersion](https://jrsoftware.org/ishelp/topic_setup_appversion.htm) | M01 | Compiler gecontroleerd; editor geaccepteerd in M01-S01 |
| Uitgever en overige productmetadata | V, G | [Setup](https://jrsoftware.org/ishelp/topic_setupsection.htm) | M07 | Gepland |
| Bestanden en bestandsopties | V, G | [Files](https://jrsoftware.org/ishelp/topic_filessection.htm) | M01, M04, M07 | Compiler gecontroleerd voor één lokaal bestand binnen {app}; overige opties gepland |
| Mapregels en mapopties | V, G | [Dirs](https://jrsoftware.org/ishelp/topic_dirssection.htm) | M04, M07 | Gepland |
| Snelkoppelingen en metadata | V, G | [Icons](https://jrsoftware.org/ishelp/topic_iconssection.htm) | M04, M07 | Gepland |
| Installatietypen | V, G | [Types](https://jrsoftware.org/ishelp/topic_typessection.htm) | M04 | Gepland |
| Componenten | V, G | [Components](https://jrsoftware.org/ishelp/topic_componentssection.htm) | M04 | Gepland |
| Taken | V, G | [Tasks](https://jrsoftware.org/ishelp/topic_taskssection.htm) | M04 | Gepland |
| Register | V, G | [Registry](https://jrsoftware.org/ishelp/topic_registrysection.htm) | M05, M07 | Gepland |
| INI-configuratie | V, G | [INI](https://jrsoftware.org/ishelp/topic_inisection.htm) | M05 | Gepland |
| Programma-acties | V, G | [Run / UninstallRun](https://jrsoftware.org/ishelp/topic_runsection.htm) | M05 | Gepland |
| Opruimregels | V, G | [InstallDelete](https://jrsoftware.org/ishelp/topic_installdeletesection.htm), [UninstallDelete](https://jrsoftware.org/ishelp/topic_uninstalldeletesection.htm) | M05 | Gepland |
| Doelarchitectuur en installatiemodus | V, G | [ArchitecturesAllowed](https://jrsoftware.org/ishelp/topic_setup_architecturesallowed.htm), [ArchitecturesInstallIn64BitMode](https://jrsoftware.org/ishelp/topic_setup_architecturesinstallin64bitmode.htm) | M05 | Gepland |
| Setup-architectuur | V, G | [SetupArchitecture](https://jrsoftware.org/ishelp/topic_setup_setuparchitecture.htm) | M05 | Gepland |
| Installatierechten | V, G | [PrivilegesRequired](https://jrsoftware.org/ishelp/topic_setup_privilegesrequired.htm) | M01, M05 | Gepland |
| Ingeschakelde installertalen | V | [Languages](https://jrsoftware.org/ishelp/topic_languagessection.htm) | M02 | Gepland |
| Standaardtekstafwijkingen | V, G | [Messages](https://jrsoftware.org/ishelp/topic_messagessection.htm) | M01, M02, M03 | Studio-basis |
| Eigen vertaalbare teksten | V, G | [CustomMessages](https://jrsoftware.org/ishelp/topic_custommessagessection.htm) | M02, M08 | Gepland |
| Taalgedrag en lettertypen | V, G | [LangOptions](https://jrsoftware.org/ishelp/topic_langoptionssection.htm) | M02, M07 | Gepland |
| Standaardwizard en zichtbaarheid | V | [Wizard Pages](https://jrsoftware.org/ishelp/topic_wizardpages.htm) | M03, M04, M05 | Studio-basis voor twee pagina's |
| Stijlen en wizardafbeeldingen | V, G | [WizardStyle](https://jrsoftware.org/ishelp/topic_setup_wizardstyle.htm) | M06 | Gepland |
| Pagina-afwijkingen via native eigenschappen | V, S | [Support Classes](https://jrsoftware.org/ishelp/topic_scriptclasses.htm), [Event Functions](https://jrsoftware.org/ishelp/topic_scriptevents.htm) | M01, M03, M06 | Runtime gecontroleerd voor de teksten van Welkom en Installatiemap; zie M01-S03 |
| Eigen standaardinvoerpagina's | V, S | [Custom Wizard Pages](https://jrsoftware.org/ishelp/topic_scriptpages.htm) | M08 | Gepland |
| Native bestandsdownload en archiefextractie | G | [Files-opties](https://jrsoftware.org/ishelp/topic_filessection.htm) | M07 | Gepland |
| Publieke verificatiesleutels | G | [ISSigKeys](https://jrsoftware.org/ishelp/topic_issigkeyssection.htm) | M07 | Gepland |
| Ondertekening | G | [SignTool](https://jrsoftware.org/ishelp/topic_setup_signtool.htm) | M07 | Gepland |
| Eigen runtimecode | S | [Pascal Script](https://jrsoftware.org/ishelp/topic_scriptintro.htm) | M08 | Gepland |
| Eigen compileertijdinvoer | S | [Preprocessor](https://jrsoftware.org/ishelp/topic_isppoverview.htm) | M08 | Gepland |
| Genereren en ISCC-proces | V | [Compiler command line](https://jrsoftware.org/ishelp/topic_compilercmdline.htm) | M01 | Compiler gecontroleerd: export en bouwen vanuit Studio met 7.1-versiegate; M01-S04 geaccepteerd; JSONL-meldingen en annuleren compiler gecontroleerd en gebruikerscontrole geaccepteerd in M01-S05 |

De reguliere secties hierboven vormen de eindscope. Niet-verouderde parameters die geen apart visueel veld krijgen worden in M07 via een gecontroleerde geavanceerde weergave bereikbaar. Een afzonderlijke catalogus legt vóór implementatie de concrete 7.1-dekking vast. Het doel is geen oncontroleerbaar vrij tekstvak dat iedere sleutel automatisch als ondersteund bestempelt.

## Verplichte registratie per nieuwe instelling

Bij een slice wordt deze matrix aangevuld of gekoppeld aan een detailcatalogus met:

| Veld | Vereiste inhoud |
|---|---|
| Studio-instelling | Eenduidige identifier, gebruikersnaam en betrokken FR |
| Vertaling | Sectie/richtlijn/parameter of gedocumenteerde native API |
| Versie | Getoetste compiler en relevante versiebeperking |
| Bereik | Project, taal, pagina, knoprol of regel |
| Waardetype | Toegestane waarden, standaard, lege waarde en beperkingen |
| Preview | Getrouw, benadering, voorbeeldwaarden of niet gesimuleerd |
| Verificatie | Test, voorbeeldproject en resultaat van ISCC |
| Gebruikerstest | Scenario in echte installer en acceptatie |
| Oplevering | Slice en geaccepteerde commit wanneer beschikbaar |

Geen validatiebewijs wordt verzonnen: onbekend blijft onbekend totdat de betreffende slice het onderzoekt.

## Belangrijke grenzen voor de ontwerper

- Standaardpagina's houden hun vaste volgorde; instellingen en projectinhoud bepalen wanneer ze zichtbaar zijn.
- Sommige instellingen gelden voor de hele wizard. Studio biedt alleen lokale afwijkingen met bewezen native vertaling.
- Een globale message-override is geen pagina-override. Per-paginagedrag vereist afzonderlijk gecontroleerd event-/controlgebruik.
- De tekst en werking van de volgende-knop veranderen met de stap. Verder, Installeren en Voltooien mogen niet worden samengevoegd.
- Stijlen kunnen de zichtbare uitkomst van controlkleuren bepalen. Een property in een API is op zichzelf geen bewijs dat elke stijl haar zichtbaar toepast.
- Beveiligingsdialoog, taaldialoog en andere systeemvensters zijn geen vrij opmaakbare wizardpagina's.

Deze grenzen volgen uit de [wizardbeschrijving](https://jrsoftware.org/ishelp/topic_wizardpages.htm), [message-scope](https://jrsoftware.org/ishelp/topic_messagessection.htm) en [stijlregels](https://jrsoftware.org/ishelp/topic_setup_wizardstyle.htm). Iedere concrete ontwerpinstelling wordt alsnog tegen 7.1 gecontroleerd.

## Uitgesloten technieken

| Mogelijkheid | Beleid |
|---|---|
| Willekeurige tekstkleur van standaardbuttons via vervangende controls of API-trucs | Uitgesloten |
| Eigen WPF-/web-installerinterface boven een verborgen installer | Uitgesloten |
| Extra DLL-framework voor schermen of afhankelijke installaties | Uitgesloten |
| Native functie gebruiken via officieel Pascal Script | Mogelijk binnen scope; documentatie en verificatie vereist |
| Willekeurig eigen script volledig visueel teruglezen of simuleren | Geen productbelofte |
| Verouderde richtlijn toevoegen aan nieuwe scripts | Niet aanbieden als nieuwe productinstelling |
| Nieuwe optie uitsluitend uit een latere compilerhandleiding | Niet aanbieden voor 7.1 zonder versiecontrole |

## Bronbeleid

Online Inno Setup-help kan inmiddels latere versies beschrijven. De links zijn naslag; de doelversie wordt getoetst met de lokale 7.1-help, de meegeleverde voorbeelden en ISCC 7.1.0.

De huidige installatie bevat onder meer AllPagesExample.iss, Languages.iss, Components.iss, CodeDlg.iss en architectuurvoorbeelden. We gebruiken gecontroleerde eigen voorbeeldprojecten voor regressies, zodat Studio niet afhankelijk wordt van een hardcoded compilerinstallatiepad.

De geïnstalleerde Inno Setup-bestanden worden gelezen als bron en blijven ongewijzigd.
