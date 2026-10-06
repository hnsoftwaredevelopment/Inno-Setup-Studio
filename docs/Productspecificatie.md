# Productspecificatie — Inno Setup Studio

Status: gecorrigeerd naar de gebruikersverduidelijking van 6 oktober 2026. Productrichting vastgesteld; technische uitwerking volgt.

## Doel en positie

Een afzonderlijke Windows-app als add-on voor Inno Setup 7.1. Het is geen add-on voor de bestaande InnoSetupStudio. Dat bestaande project blijft behouden en ongewijzigd; de nieuwe variant krijgt de voorgestelde verbeterde visuele editor.

De Studio-gebruiker installeert Inno Setup 7.1 zelf. Studio genereert een leesbaar `.iss`-script dat rechtstreeks met de Inno Setup-compiler of vanuit Studio kan worden gebouwd. De gemaakte installer kan vanuit Studio worden gestart. De ontvanger van die installer hoeft Inno Setup of Studio niet te installeren.

## Uitgangspunten

- Productnaam en projectmap: `Inno Setup Studio`.
- Projectroot: `C:\Devops\hnsoftwaredevelopment\Inno Setup Studio`.
- Voorgestelde basis: C# / .NET 10 / WPF, aansluitend bij het bestaande project.
- Eigen applicatie-identiteit en instellingenlocatie, zodat beide varianten naast elkaar kunnen draaien.
- Bestaande opslag-, vertaal- en generatielogica kan na beoordeling worden hergebruikt.
- Hergebruik van het bestaande projectformaat wordt onderzocht; een nieuw formaat is geen voorwaarde.

## Ontwerper

Links staan pagina’s en hun elementen, met een afzonderlijk Basisontwerp. Hier kunnen pagina’s ook worden ingeschakeld en uitgeschakeld. Midden staat een schaalbaar ontwerpvlak; rechts staan eigenschappen van het geselecteerde element. Een klik in ontwerpmodus selecteert een element en toont een selectierand. In de afzonderlijke uitprobeermodus voert dezelfde knop zijn ingestelde actie uit.

Het Basisontwerp krijgt een zichtbare preview van de gedeelde elementen, zoals logo en navigatie. Pagina’s erven ondersteunde instellingen en kunnen daarvan per eigenschap afwijken. De editor toont de herkomst en biedt herstellen naar het Basisontwerp aan. Een expliciet lege tekst moet waar relevant te onderscheiden zijn van een overgeërfde tekst.

Veelgebruikte eigenschappen worden in het vaste paneel bewerkt, zonder steeds een dialoog te openen. De previewtaal is selecteerbaar. Installatiegedrag krijgt eigen onderdelen, zoals bestanden, snelkoppelingen, register, architectuur en talen, los van eigenschappen van een geselecteerd schermelement.

## Koppeling met Inno Setup 7.1

De editor genereert standaard Inno Setup-richtlijnen en waar nodig Pascal Script in een `.iss`-bestand. De uiteindelijke installerinterface en installatiehandelingen worden door Inno Setup verzorgd. Een eigen extern interfaceprogramma, WPF-installerruntime of verborgen installer achter een eigen launcher hoort niet bij deze variant.

De WPF-preview is een ontwerpweergave van de Inno Setup-schermen. Ondersteunde instellingen moeten correct naar het script worden vertaald en in een echte installer worden gecontroleerd. De preview mag geen vormgevingsmogelijkheden beloven die de gegenereerde installer niet waarmaakt. De eerder vastgestelde beperking rond tekst- en achtergrondkleur van standaardknoppen blijft daarom een grens; deze variant belooft daarvoor geen oplossing.

Het gegenereerde `.iss`-script moet met de benodigde reguliere projectbestanden buiten Studio met Inno Setup 7.1 compileerbaar zijn. Studio gebruikt diezelfde lokaal geïnstalleerde compiler en toont bouwmeldingen en fouten.

## Eerste succescriteria

1. Een werkplek met pagina’s en elementen links, een schaalbaar ontwerpvlak midden en contextuele eigenschappen rechts.
2. Een klik in ontwerpmodus selecteert een element en toont de eigenschappen direct in het vaste paneel.
3. Het Basisontwerp heeft een zichtbare preview.
4. Een tweede pagina erft ondersteunde basisinstellingen en wijkt op één eigenschap af; de herkomst is zichtbaar en herstellen werkt.
5. Ontwerpen en uitproberen hebben verschillende kliksemantiek.
6. Een minimaal voorbeeldproject levert een `.iss`-script op dat met Inno Setup 7.1 buiten Studio compileert.
7. Compileren vanuit Studio gebruikt de geïnstalleerde compiler en maakt uitvoer en fouten zichtbaar.
8. De echte installer wordt vergeleken met de preview op vormgeving, paginagedrag en taal.
9. Beide Studio-varianten kunnen naast elkaar bestaan zonder elkaars instellingen of projectbestanden te overschrijven.

Een eigen installatie-engine of installerinterface, Visual Studio-extensie en Inno Setup-import vallen buiten de eerste versie.

## Voorgestelde projectstructuur

- `src/Studio.App/`: ontwerpomgeving.
- `src/Studio.Core/`: projectmodel, overerving en validatie.
- `src/Studio.Preview/`: ontwerpweergave van ondersteunde Inno Setup-schermen.
- `src/Studio.InnoSetup/`: scriptgeneratie en compilerkoppeling.
- `tests/`: gerichte geautomatiseerde tests.
- `samples/`: kleine voorbeeldprojecten.
- `docs/`: specificaties en bevindingen.

Deze structuur is een voorstel; er zijn nog geen broncodeprojecten aangemaakt.

## Bouw- en testcommando’s

Er is nu nog niets te bouwen of te starten. Beoogde opdrachten vanuit de projectroot, pas uitvoerbaar na het aanmaken van de solution:

```powershell
dotnet build '.\Inno Setup Studio.slnx'
dotnet test '.\Inno Setup Studio.slnx'
dotnet run --project '.\src\Studio.App\Studio.App.csproj'
```

De ISCC.exe-locatie wordt gedetecteerd of door de gebruiker ingesteld; geen vast installatiepad aannemen. Argumenten worden afzonderlijk aan het proces doorgegeven. De gebruikte compilerversie wordt gecontroleerd.

## Codestijl

C# met nullable reference types, PascalCase voor publieke leden, camelCase voor parameters en MVVM voor de editor. Projectmodel en overerving hebben geen WPF-afhankelijkheid. Voorbeeld van expliciete overerving:

```csharp
public sealed record PropertyOverride<T>(bool IsOverridden, T Value)
{
    public T Resolve(T inheritedValue) => IsOverridden ? Value : inheritedValue;
}
```

## Verificatie

Gerichte unit tests voor overerving, reset, serialisatie en scriptgeneratie. Integratietests met de geïnstalleerde Inno Setup 7.1-compiler. Installatieproeven in een aparte testomgeving. Visuele controles op selectie, taal, schaalfactor, toetsenbordbediening en overeenkomst tussen preview en echte installer. Een compilatie alleen bewijst geen werkende installer.

## Grenzen

- Altijd: oude project behouden, geïnstalleerde Inno Setup 7.1-compiler gebruiken, beperkingen zichtbaar maken en gewijzigde Markdown spiegelen naar Obsidian.
- Vooraf afstemmen: overstap naar een eigen installatie-engine of installerinterface, nieuwe distributievereisten of verlies van zelfstandig compileren buiten Studio.
- Nooit: bestaande gebruikersinstellingen of projectbestanden van de oude variant overschrijven; niet-ondersteunde vormgeving presenteren als werkend.

## Open ontwerpbeslissingen

- Welke onderdelen van opslag, vertalingen en scriptgeneratie worden uit de bestaande code hergebruikt?
- Welke Inno Setup 7.1-elementen en eigenschappen ondersteunt de eerste visuele editor?
- Hoe maken we het bestaande projectformaat bruikbaar zonder de oude variant te beïnvloeden?

De productrichting is vastgesteld: een afzonderlijke add-on voor Inno Setup 7.1 met verbeterde visuele bediening. Deze vragen betreffen de implementatie daarvan.
