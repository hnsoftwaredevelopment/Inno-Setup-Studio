# Ontwikkelplan — Inno Setup Studio

Status: geaccepteerd op 8 oktober 2026. Dit plan vervangt de eerste proefplanning. Die is bewaard in [Proefversie](../docs/historie/Proefversie.md).

## Productbasis

De [productspecificatie](../docs/Productspecificatie.md) beschrijft het eindproduct. De [ondersteuningsmatrix](../docs/InnoSetup-ondersteuning.md) begrenst functies tot gecontroleerde Inno Setup 7.1-mogelijkheden. De [werkwijze](../docs/Werkwijze.md) legt acceptatie, commits, pushes, branches en merges vast.

Huidige architectuur: Studio.Core bevat projectmodel, editorstatus, opslag en lokalisatie; Studio.App bevat WPF-werkplek en preview; Studio.Core.Tests bevat xUnit-tests. De bestaande UI blijft het vertrekpunt.

## Technische richting

- Projectmodel is de bron voor visueel beheerde gegevens. Gegevens blijven onafhankelijk van Studio-taal, previewkeuzes en lokale compilerlocatie.
- Opslag groeit met gecontroleerde formaatversies en migraties; bestaande proefprojecten zijn regressievoorbeelden.
- Een geordende generator vertaalt projectwaarden naar reguliere secties en gedocumenteerde native scriptfragmenten. Eigen code heeft een herkenbare bron en eigenaarschap.
- De export is reproduceerbaar en leesbaar. Studio leest willekeurige uitvoerwijzigingen niet automatisch terug.
- Compilerkoppeling beheert proces, argumenten, werkmap, uitvoer en projectstand. Uitproberen blijft WPF-simulatie zonder installatiehandelingen.
- Nieuwe projecten/assemblies worden alleen toegevoegd als ze een echte modulegrens verduidelijken. Een apart InnoSetup-project is een kandidaat bij M01, geen verplichte architectuurverbouwing.
- Native installerondersteuning krijgt een catalogus van versie, vertaling, bereik en bewijs. Het bestaan van een WPF-property bepaalt geen installerfunctionaliteit.

Beleidsgrens: [ADR-001](../docs/decisions/ADR-001-InnoSetup-als-productgrens.md). Project-/scriptbeheer: [ADR-002](../docs/decisions/ADR-002-Project-en-scriptbeheer.md), voorstel ter review.

## Afhankelijkheden

~~~mermaid
flowchart TD
    M00["M00 Product en werkwijze"] --> M01["M01 Eerste echte installer"]
    M01 --> M02["M02 Installertalen"]
    M02 --> M03["M03 Wizardpagina's"]
    M03 --> M04["M04 Installatiekeuzes"]
    M04 --> M05["M05 Systeemintegratie"]
    M03 --> M06["M06 Vormgeving"]
    M05 --> M07["M07 Geavanceerde opties"]
    M06 --> M07
    M07 --> M08["M08 Scripting"]
    M02 --> M08
    M08 --> M09["M09 Productoplevering"]
~~~

Voorgestelde werkvolgorde is M00 tot en met M09. M06 heeft technisch minder afhankelijkheden, maar staat later zodat een echte installer vroeg wordt bewezen. Er wordt één milestone en één slice tegelijk uitgewerkt.

## Voorbereiding per milestone

1. Controleer actuele main, remote en gebruikerswijzigingen; maak de branch met de milestonenaam.
2. Lees relevante producteisen, matrixregels en reeds gecontroleerde voorbeelden.
3. Concretiseer de eerste slice uit [todo](todo.md), inclusief bestandsverwachting en testscenario.
4. Controleer eventuele nieuwe opties tegen de lokale 7.1-help en compiler.
5. Bouw en lever alleen die slice ter acceptatie op.

## Controlepunten

Na iedere slice: gerichte tests en build bij code, gebruikersreview en pas na akkoord commit/push. Na iedere milestone: gezamenlijke eindcriteria en merge na milestoneacceptatie.

Bij de eerste installer worden echte navigatie, tekstafwijkingen en zelfstandig compileren bewezen. Bij installertalen worden behoud van Nederlandse proefprojecten en onafhankelijke previewtaal bewezen. Bij bredere configuratie worden verwijzingen en voorwaardelijke regels bewezen. Bij productoplevering volgt de gezamenlijke regressie.

## Codestijl en teststrategie

C# gebruikt nullable reference types, PascalCase voor publieke leden en camelCase voor parameters. Houd domeinregels in Core, WPF-weergave in App en proces-/generatielogica buiten de views. Volg de bestaande stijl en verander niet tegelijk ongerelateerde code.

Een bestaande voorbeeldregel laat zien dat effectieve tekst via het model wordt opgehaald:

```csharp
public string NextCaption => Project.Caption(CurrentPage, ElementKind.Next);
```

De taalservice levert Studio-teksten; installerinhoud hoort bij het projectmodel. Nieuwe editorproperties moeten veranderingen melden zonder selectielussen of onbedoelde modelwijzigingen.

De bestaande testbasis is xUnit in tests/Studio.Core.Tests. Voeg gerichte voorbeelden toe voor modelregels, opslag/migraties, referenties, taalgedrag en scriptgeneratie. Generatorcontroles vergelijken betekenisvolle scriptinhoud en toetsen quoting/Unicode/voorwaarden. Integratiecontroles compileren eigen voorbeeldexports met de gekozen doelcompiler. Buildfouten en annulering krijgen gecontroleerde procesproeven.

Runtime- en visuele controles liggen bij de gebruiker. Hun resultaten worden met projectstand, compilerversie en testscenario vastgelegd; niet geteste platformen of styles krijgen geen fictief bewijs.

## Risico's en maatregelen

| Risico | Gevolg | Aanpak |
|---|---|---|
| Prototype biedt een niet-exporteerbare afwijking | Preview en installer verschillen | Bewijs in M01; scope registreren vóór verdere UI-uitbreiding |
| Online docs beschrijven latere opties | Export faalt op 7.1 | Lokale help/voorbeelden en doelcompiler als controle |
| Brede geavanceerde scope | Te grote slices of onvolledige dekking | Eerst catalogus; implementatie opsplitsen per instellingengroep |
| Taalwissel overschrijft eigen teksten | Gegevensverlies of gemengde talen | Taalgebonden waarden, migratie en expliciete herkomst |
| Stijl/DPI/RTL wijzigt weergave | Misleidende preview | Getrouwe delen onderscheiden van benadering; gebruiker vergelijkt echte installer |
| Eigen script botst met gegenereerde code | Buildfout of onjuist gedrag | Eigenaarschap en eventcompositie vastleggen vóór scripting |
| Oud projectformaat past niet bij nieuw model | Proefprojecten onbruikbaar | Migraties en fixturetests, geen automatische overschrijving |
| Oud bouwresultaat blijft zichtbaar | Verkeerde installer wordt getest | Resultaat aan buildstand koppelen; fouten en annuleren wissen successtatus |
| Lokale commits lopen vóór remote | Onbedoelde publicatie | Commitbereik expliciet beoordelen vóór de eerste push |

## Beslissingen die later concretisering vragen

De productspecificatie noemt vier reviewpunten. Daarnaast concretiseren we bij de betreffende slice: vertaalterugval voor eigen teksten, exact eventbeheer, opslag van eigen bronbestanden en het distributieformaat. Die onderwerpen blokkeren het documentatievoorstel niet; ze worden vóór hun implementatie beslist.

Er worden nu geen extra dependencies, CI-workflows, GitHub-issues of releaseprocessen ingericht.
