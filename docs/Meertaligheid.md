# Meertaligheid van Studio

Dit document beschrijft de Studio-localisatie en de huidige basis voor projecttalen. De volledige installerlocalisatie is gespecificeerd in [Productspecificatie, FR-08](Productspecificatie.md#11-studio-taal-en-installertalen--fr-08) en gepland in [M02](Milestones.md#m02--m02-installertalen).

## Twee onafhankelijke talen

De Studio-taal bepaalt navigatie, eigenschappen, toelichtingen, meldingen en door Studio ingestelde dialoogtitels. Beschikbaar zijn Nederlands (`nl`), Engels (`en`) en Duits (`de`). Nederlands is de standaard en terugvaltaal.

Installertalen zijn aparte projectinstellingen. Een taalwissel van Studio verandert geen titel, beschrijving, knoptekst, tooltip, pad, pagina-afwijking of projectnaam. Nieuwe projecten beginnen Engels; oude projecten behouden hun Nederlandse teksten. Voorbeeldtaal bevat alleen actieve projecttalen en wordt niet in het project opgeslagen. Ook de venstertitel en maplabel in de preview volgen deze taalkeuze.

De standaardnamen van pagina's in de Studio-navigatie worden vertaald op basis van hun paginatype. Opgeslagen projectgegevens blijven ongewijzigd. Hulpteksten in het Basisontwerp en tijdelijke afbeeldinglabels zijn Studio-teksten.

## Vertalingen beheren in Visual Studio

De bestanden staan in `src/Studio.Core/Resources/`:

| Bestand | Taal |
|---|---|
| `Strings.resx` | Nederlands; neutrale bron en terugval |
| `Strings.en.resx` | Engels |
| `Strings.de.resx` | Duits |

Open de solution in Visual Studio en open ResXManager. De drie bestanden vormen één resourcegroep. `ResXManager.config.xml` in de solutionmap stelt de neutrale taal in op `nl`; dezelfde waarde staat als `NeutralLanguage` in het Core-project.

Gebruik dezelfde sleutel in alle drie de bestanden. Sleutels beschrijven het doel, bijvoorbeeld `SaveChanges` of `DirectoryHint`. Laat opmaakplaatsen zoals `{0:00}` en Inno Setup-constanten zoals `{autopf}` intact. Een underscore in een knop- of labeltekst markeert een toegangstoets.

Er is geen handgeschreven `Strings.Designer.cs` nodig. De .NET-build verwerkt de resources; `StudioLocalizer` leest ze met `ResourceManager`. Nieuwe sleutels kunnen direct via de indexer worden gebruikt.

## Aansluiten van nieuwe schermteksten

In WPF:

```xml
xmlns:loc="clr-namespace:Studio.App"
<Button Content="{loc:Loc Save}" />
```

De markup-extensie maakt een binding die automatisch ververst bij een taalwissel. Gebruik haar uitsluitend voor de Studio-interface, nooit voor waarden die de gebruiker als installerinhoud bewerkt.

In de editorlogica:

```csharp
Text["StatusSaved"]
Text.Format("PageNumber", 1)
```

Statusmeldingen bewaren hun resourcesleutel, zodat ook een al zichtbare melding mee vertaalt. Pagina- en elementkeuzes behouden hun objectidentiteit; verversen mag de selectie niet wijzigen. Een vervangen `EditorSession` wordt afgemeld met `Dispose()`.

De service gebruikt expliciet de geselecteerde cultuur. De algemene procescultuur wordt niet gewijzigd: projectopslag en toekomstige scriptgeneratie mogen niet afhangen van de taal van de werkplek.

## Instelling en terugval

De keuze wordt direct opgeslagen in:

```text
%LocalAppData%\HNSoftwareDevelopment\Inno Setup Studio\settings.json
```

Dit bestand hoort bij de nieuwe Studio en staat los van `.issstudio`-projecten en de oude InnoSetupStudio. Ontbrekende, ongeldige of te grote instellingen vallen terug op Nederlands. Als opslaan mislukt, blijft de gekozen taal actief en meldt Studio dat de keuze niet kon worden onthouden.

Standaard Windows-onderdelen van bestands- en mapdialogen en knoppen van Windows-meldingsvensters volgen de Windows-taal. De door Studio geleverde titels en berichten volgen wel de Studio-taal.

## Controle

Tests controleren de drie talen, volledige resourcegroepen, terugval, instellingsopslag, vertaalbare validatiefouten en taalwissels tijdens bewerken en uitproberen. Het geserialiseerde installerproject moet daarbij exact gelijk blijven en de bestaande wijzigingsstatus en selectie moeten behouden blijven.

Tijdens visuele controle is een herhaalde selectienotificatie gevonden die een stack overflow kon veroorzaken. De selectie negeert nu ongewijzigde waarden en de elementenlijsten zijn stabiel. Een regressietest controleert de terugkoppeling van bindings.

## Start van M02

M02-S01 voegt een alleen-lezen overzicht van Inno Setup-taalbronnen toe. Dat staat los van Studio-resx. [Bronoverzicht, grenzen en teststappen](slices/M02-S01-Beschikbare-taalbronnen.md).

M02-S02 voegt Projecttalen, volgorde, inschakelen en een aparte Voorbeeldtaal toe. De bestaande eigen teksten blijven in hun oorspronkelijke taal bewerkbaar; aanvullende talen tonen voorlopig alleen standaardteksten uit hun bron. Eigen vertalingen volgen in S03. Meertalige export wordt tot S05 met uitleg geblokkeerd; uitsluitend Engels of de bestaande Nederlandse teksttaal kan wel worden gebouwd. [Opslag, beperkingen en teststappen](slices/M02-S02-Projecttalen-en-previewkeuze.md). M02-S02 is op 13 oktober 2026 door de gebruiker geaccepteerd.

## Eigen teksten vanaf M02-S03

Voorbeeldtaal bepaalt ook de taal van de eigen tekstbewerking. Er staat één taal tegelijk in het eigenschappenpaneel; de wizard blijft daarnaast zichtbaar. Een leeg eigen invoerveld toont de effectieve wizardtekst als grijze watermark. Zodra er invoer is, verdwijnt de watermark. Daaronder staat alleen Overnemen bij een leeg veld, of Herstellen bij eigen invoer. Tekst overnemen maakt pas na een gebruikersactie een eigen tekst van een standaard-/terugvaltekst.

De eerste actieve projecttaal is de standaardtaal. Een ontbrekende of lege eigen vertaling gebruikt eerst het standaardbericht uit het gekozen taalbestand. Ontbreekt dat, dan volgt de toepasselijke eigen tekst uit de standaardtaal, daarna het bronbericht uit die taal. Een ontbrekende eigen vertaling blijft meetellen, ook als een standaardbericht in de gekozen taal beschikbaar is. Een lege vereiste tekst gebruikt uiteindelijk een native of ingebouwde standaardtekst; een lege optionele tooltip is toegestaan. De waarschuwing vermeldt ontbrekende vertalingen voor actieve talen; de taalkeuzelijst toont aantallen per taal. Uitschakelen bewaart eigen teksten en haalt die taal uit de teller.

Herstellen werkt voor de gekozen taal en scope. Tooltip heeft een eigen herstelactie. {AppName} en {AppVersion} in eigen teksten verwijzen naar de applicatiegegevens; productwaarden worden daarbij eenmaal als letterlijke gegevens ingevuld. [Volledige volgorde, migratie en gebruikerscontrole](slices/M02-S03-Eigen-teksten-per-taal.md). S03 is geaccepteerd. De volledige taalexport blijft voor S05.

## Vertaaloverzicht en controles — M02-S04

Vertaaloverzicht opent via de knop naast Voorbeeldtaal een afzonderlijk venster met alle projecttalen naast elkaar in projectvolgorde. De eerste kolom beschrijft scherm en tekstveld en blijft staan bij horizontaal scrollen. Uitgeschakelde talen blijven zichtbaar en bewerkbaar; hun problemen tellen niet mee in de actieve totalen. Eigen teksten worden direct gewijzigd in het geopende project. Sluiten annuleert niets; Bewaren blijft nodig om wijzigingen op schijf vast te leggen.

Lege velden tonen de effectieve tekst als watermark. Overnemen en Herstellen werken per taal en scope met dezelfde regels als het eigenschappenpaneel. Meldingen signaleren onbekende of onvolledige Studio-verwijzingen, en {AppName}/{AppVersion} die in een eigen vertaling ontbreken ten opzichte van de toepasselijke eigen standaardtekst. Meldingen herschrijven geen tekst en verhinderen opslaan niet. Standaardberichten en letterlijke applicatiegegevens worden niet als Studio-invoersyntax gecontroleerd. Regelovergangen blijven behouden.

Taal verwijderen in Projecttalen vraagt bevestiging met het aantal eigen tekstvelden. Uitschakelen bewaart deze velden. De laatste actieve taal kan niet worden verwijderd. Bij verwijderen van de oorspronkelijke opslagtaal worden uitsluitend de eigen waarden van een overblijvende taal in de bestaande opslagvelden ondergebracht; terugvalteksten worden niet als vertaling gekopieerd. Formaat 6 blijft gelijk. [Scope, grenzen en gebruikerscontrole](slices/M02-S04-Vertaaloverzicht.md).
