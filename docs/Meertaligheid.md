# Meertaligheid van Studio

Dit document beschrijft de huidige Studio-implementatie. De toekomstige installertalen en aparte Voorbeeldtaal zijn gespecificeerd in [Productspecificatie, FR-08](Productspecificatie.md#11-studio-taal-en-installertalen--fr-08) en gepland in [M02](Milestones.md#m02--m02-installertalen). Ze zijn nog niet gebouwd.

## Twee onafhankelijke talen

De Studio-taal bepaalt navigatie, eigenschappen, toelichtingen, meldingen en door Studio ingestelde dialoogtitels. Beschikbaar zijn Nederlands (`nl`), Engels (`en`) en Duits (`de`). Nederlands is de standaard en terugvaltaal.

De installertaal is een aparte toekomstige projectinstelling. Een taalwissel van Studio verandert geen titel, beschrijving, knoptekst, tooltip, pad, pagina-afwijking of projectnaam. De huidige voorbeeldinstaller bevat Nederlandse teksten. Ook de vaste previewteksten “Installatie” en “Installatiemap” blijven voorlopig Nederlands; die horen later bij de installerlocalisatie met Inno Setup-taalbestanden.

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

M02-S01 voegt een alleen-lezen overzicht van Inno Setup-taalbronnen toe. Dat staat los van Studio-resx en verandert nog geen project- of previewtaal. [Bronoverzicht, grenzen en teststappen](slices/M02-S01-Beschikbare-taalbronnen.md). Projecttalen en de aparte Voorbeeldtaal volgen in M02-S02.
