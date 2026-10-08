# M01-S03 — Huidig wizardontwerp exporteren

Status: geïmplementeerd en door de gebruiker geaccepteerd op 8 oktober 2026. Branch: `m01-eerste-installer`. De gebruiker heeft Studio getest en het geëxporteerde script gecompileerd en uitgevoerd; alles werkt volgens de gebruikerscontrole.

## Resultaat

Exporteer .iss neemt het huidige ontwerp van Welkom en Installatiemap mee. De bestaande projectopslag (formaat 3) verandert niet. Studio blijft een add-on voor Inno Setup 7.1; navigatie, mapkeuze, installatie en deïnstallatie blijven native.

De installer is voorlopig Nederlands, onafhankelijk van Studio-taal. Installertalen volgen in M02. De overige wizardstappen worden door Inno Setup toegevoegd en zijn nog niet in Studio te ontwerpen.

## Vertaling en bereik

Alle onderstaande koppelingen zijn compiler-gecontroleerd met ISCC **7.1.0**, en door de gebruiker in de gecompileerde, uitgevoerde installer gecontroleerd. Dit bewijs geldt voor de huidige twee pagina's; overige functies blijven gepland. Betrokken eisen: huidige pagina's, gedeelde instellingen, pagina-afwijkingen en export; zie de [productspecificatie](../Productspecificatie.md).

| Studio-instelling | Native vertaling | Bereik en waarden |
|---|---|---|
| Welkom: titel | `WizardForm.WelcomeLabel1.Caption` | Letterlijke tekst, ook leeg; alleen Welkom |
| Welkom: tekst | `WizardForm.WelcomeLabel2.Caption` | Letterlijke tekst met regeleinden, ook leeg; alleen Welkom |
| Installatiemap: titel | `WizardForm.PageNameLabel.Caption` bij `wpSelectDir` | Letterlijke tekst, ook leeg; kop van de mappagina |
| Installatiemap: tekst | `WizardForm.SelectDirLabel.Caption` | Letterlijke tekst met regeleinden, ook leeg; boven de mapkeuze |
| Standaardpad | `[Setup] DefaultDirName` | Bestaande validatie; Inno-constanten worden native verwerkt |
| Bladeren: tekst | `WizardForm.DirBrowseButton.Caption` | Letterlijke knoptekst, ook leeg; alleen mappagina |
| Bladeren: tooltip | `Hint` en `ShowHint` | Letterlijke tekst; leeg schakelt de hint uit |
| Basisontwerp: Terug/Annuleren | `BackButton.Caption` / `CancelButton.Caption` in `CurPageChanged` | Gedeeld voor de wizard, tenzij huidige pagina een afwijking heeft |
| Basisontwerp: Verder | `NextButton.Caption` bij `wpWelcome` en `wpSelectDir` | Alleen de twee ontworpen pagina's; overige stappen houden native knoprollen |
| Lokale knoptekst | Dezelfde control bij de betreffende pagina-ID | Override wint van basis, ook wanneer leeg; bij paginawisseling opnieuw bepaald |
| Beschikbaarheid huidige pagina's | `DisableWelcomePage=no`, `DisableDirPage=no` | Beide ontworpen pagina's worden expliciet ingeschakeld |

Native functies: [Support Classes](https://jrsoftware.org/ishelp/topic_scriptclasses.htm), [Event Functions](https://jrsoftware.org/ishelp/topic_scriptevents.htm), [DisableWelcomePage](https://jrsoftware.org/ishelp/topic_setup_disablewelcomepage.htm) en [DisableDirPage](https://jrsoftware.org/ishelp/topic_setup_disabledirpage.htm).

Tekst wordt rechtstreeks aan controls toegewezen: apostrofs, Unicode, procenttekens, accolades en `[name]` blijven tekst. Studio breidt deze teksten niet uit als Inno-messages of constanten. Regeleinden worden CRLF. Het teken `#` wordt als karaktercode geschreven zodat letterlijke `{#...}` geen preprocessorcode wordt. Native `&`-mnemonics in knopteksten blijven beschikbaar.

## Begrenzing van de preview

De preview is een benadering, geen exacte kopie van de native indeling. Op de mappagina blijven de native beschrijving in de kop, aanvullende Bladeren-instructie, icoon en vrije-schijfruimtetekst aanwezig. De bewerkbare body wordt boven de mapkeuze geplaatst. Het logo-vak in Studio is nog een placeholder; afbeeldingen volgen later.

De generator gebruikt `AdjustLabelHeight` en `IncTopDecHeight` voor tekstindeling en schuift de mapcontrols omlaag wanneer meer ruimte nodig is. De native pagina en knoppen hebben eindige afmetingen: zeer lange teksten kunnen worden afgekapt of buiten de beschikbare ruimte vallen. Er is geen automatische lettertypeverkleining, onbeperkte knopverbreding of vervangende control. Controleer passende tekstlengtes in de echte wizard, ook bij afwijkende Windows-schaling.

Terug is op de echte welkompagina verborgen; Studio toont daar momenteel een niet-actieve knop in Uitproberen. Op Gereed blijft de native knop **Installeren**, op Voltooid **Voltooien**. Een eigen Verder-tekst geldt daar niet. Deze stappen komen in M03 beschikbaar in de preview.

Het standaardpad wordt nooit opnieuw naar `DirEdit.Text` geschreven tijdens navigatie. Een gekozen map blijft daardoor intact bij Terug/Verder. Bij een eerder geïnstalleerde AppId kan Inno Setup het vorige installatiepad hergebruiken; dit is native gedrag.

## Verificatie

Release-build zonder fouten of waarschuwingen; **75 tests geslaagd**, waaronder twee echte compilatieproeven met ISCC 7.1.0. De proeven omvatten het meegeleverde voorbeeld, lokale knopafwijkingen, meerregelige Unicode-teksten, apostrofs en letterlijke preprocessorachtige tekst. De installer is door de agent niet gestart.

Review: tekstencoding, eventbereik, native knoprollen, bronbestandbehoud en projectimmutabiliteit gecontroleerd. De bekende compilerwaarschuwing over `setup.exe` blijft bewust staan tot M01-S04/S05. Dit is afzonderlijk van de waarschuwingvrije .NET-build.

## Testen door de gebruiker

Gebruik een kopie van het voorbeeld of een eigen testproject met een klein lokaal bronbestand. Kies voor een verse installatie desgewenst een nieuwe AppId in die kopie. Open de nieuwe Release-build en exporteer naar een eigen `.iss`-bestand; compileer dit met Inno Setup 7.1.

1. Geef beide pagina's herkenbaar verschillende titels en teksten. Gebruik ook een apostrof en enkele regeleinden. Controleer deze op Welkom en Installatiemap in de echte installer; vergelijk de tekstinhoud, niet pixelposities.
2. Stel gedeelde knopteksten in. Geef Welkom een eigen Verder-tekst en Installatiemap een eigen Terug- of Annuleren-tekst. Navigeer heen en terug: afwijkingen horen bij hun pagina. Terug is op Welkom verborgen.
3. Wijzig Bladeren-tekst en tooltip. Controleer de hint met de muis en kies een andere map. Navigeer terug en weer verder: de gekozen map moet behouden blijven. Controleer eventueel een lege tooltip via een tweede export.
4. Ga naar Gereed: de volgende knop heet Installeren, geen eigen Verder-tekst. De lokale Annuleren-tekst van Installatiemap mag hier niet blijven staan. Na een testinstallatie moet de afsluitknop Voltooien heten.
5. Wissel Studio-taal: het ontwerp en de installerteksten blijven ongewijzigd. Opslaan/heropenen behoudt alle waarden; een mapkeuze in Uitproberen mag het standaardpad niet wijzigen.

Acceptatiecriterium: de twee pagina's tonen de ingestelde inhoud binnen hun native grenzen, afwijkingen lekken niet tussen pagina's en de gewone navigatie/mapkeuze/knoprollen blijven werken. Installatie/deïnstallatie als volledig milestonebewijs volgt in M01-S06.
