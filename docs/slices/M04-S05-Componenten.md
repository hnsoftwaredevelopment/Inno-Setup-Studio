# M04-S05 — Componenten

Status: geaccepteerd op 11 november 2026 op m04-installatiekeuzes na geaccepteerde S04 (e78e830).

## Resultaat en scope

Componenten hebben een unieke interne naam, verplichte standaardbeschrijving, optionele taalbeschrijvingen, typekoppelingen en de native keuzes fixed en checkablealone. Een hiërarchie wordt uit naamsegmenten afgeleid en moet in correcte volgorde staan. Voorbeelden zijn main, help en help\dutch. Ontbrekende typeverwijzingen en structurele fouten blokkeren bewaren/export. Verwijderen/hernoemen van een gebruikt type wordt begrensd met zichtbare verwijzingen.

Het beheer is apart toegankelijk. Het paginaoverzicht leidt de componentenpagina af van de componentenlijst. Uitproberen toont typen en componenten met tijdelijke keuzes, native teksten en taalbeschrijvingen. Het scenario is een eerste installatie zonder eerdere installatiekeuzes. Zonder expliciete typen gelden de drie native typen full, compact en custom. Inhoudskoppelingen naar Files/Dirs/Icons blijven S07; er wordt dus nog geen verschillende payload geïnstalleerd.

## Controles

Gerichte tests voor hiërarchie, typeverwijzingen, vertalingen, bewaren/migratie, native export/compilatie en tijdelijke previewkeuzes. Release-build en niet-visuele WPF-controle; visuele controle en echte installatie door de gebruiker. Deze slice wordt pas na gebruikersakkoord gecommit/gepusht.

## Verificatie en grenzen

Projectformaat 14 bewaart componenten en migreert formaten 1–13 met een lege lijst. Typekoppelingen worden hoofdletterongevoelig gecontroleerd; lege of onbekende beschrijvingsverwijzingen, dubbele namen, ongeldige hiërarchie en onbekende typen worden afgewezen. Opslaan als behoudt componenten en vertalingen. Gebruikte type-identifiers zijn beschermd tegen hernoemen/verwijderen; omschrijvingen en volgorde blijven bewerkbaar.

De proef volgt de eerste typekeuze van een verse installatie en bewaart tijdelijke keuzes tijdens navigatie en taalwisseling. Aangepast herberekent alleen vaste componenten; andere keuzes blijven behouden. Zonder aangepast type is er geen vrij bewerkbare lijst; bij uitsluitend één aangepast type wordt de typekeuzelijst verborgen. De normale lijst is zichtbaar met de native standaard AlwaysShowComponentsList=yes. Onderliggende selecties werken door naar hun groep; gemengde groepen worden herkenbaar getoond. De proef berekent geen bestandsgroottes en leest geen vorige installatie of commandoregelopties.

Native naslag: TypesComboChange, SelectComponentsFromType, initialisatie van de componentenpagina in Setup.WizardForm.pas; hiërarchische checkboxregels in NewCheckListBox.pas; Components en Types in ISHelp/isetup.xml. De export gebruikt Components, taalgebonden CustomMessages en native fixed/checkablealone, zonder gegenereerde installatiecode of inhoudskoppelingen.

Release-build geslaagd zonder fouten of waarschuwingen. Volledige regressie: 282 tests geslaagd. Daarna negen gerichte componententests geslaagd, inclusief drie aanvullende gevallen voor gemengde/vaste onderliggende onderdelen, taalbeheer en native standaardtypen. Twee nieuwe ISCC 7.1.0-proeven controleren expliciete meertalige typen en native standaardtypen. De niet-visuele WPF-proef controleert NL/EN/DE: openen zonder wijzigingsstatus, echte lijstselectie, verplaatsen van groepen, vertaalinvoer, typekeuze in de wizardproef, taalwisseling en terug/vooruit zonder projectwijzigingen. Geen zichtbare kliktests of installers gestart.

## Handmatige controle

1. Open je demoproject. Maak of behoud typen full, compact en custom; alleen custom is aangepast. Voeg via Componenten beheren main (Programma, vast, gekoppeld aan alle drie typen) en help (Handleiding, niet vast, alleen full) toe.
2. Vul bij Vertalingen voor meerdere actieve installertalen eigen omschrijvingen in. Controleer de grijze terugval bij een leeg veld. Voeg onder help een onderdeel toe, verplaats de groep en controleer dat de structuur behouden blijft. Verwijder het onderliggende onderdeel en voeg opnieuw toe.
3. Bewaar en heropen, ook via Opslaan als. Controleer namen, typekoppelingen, flags en vertalingen. Test een dubbele naam of onbekende typeverwijzing; herstellen moet de melding opheffen. Een gebruikt type kan niet direct hernoemd/verwijderd worden; de verwijzingen staan in het typenvenster.
4. Controleer Paginaoverzicht: Componenten staat aan bij een niet-lege componentenlijst. Open Uitproberen en navigeer naar Componenten. Wissel full/compact/custom, vink niet-vaste onderdelen aan/uit, ga terug/vooruit en wissel installertaal. Keuzes blijven tijdelijk, beschrijvingen volgen de taal. Test ook zonder aangepast type en met uitsluitend één aangepast type.
5. Exporteer en compileer het .iss-bestand; controleer Components en de taalgebonden StudioComponent-teksten. Voer de installer zelf uit en vergelijk de componentenpagina en typekeuzes. Verwijder alle componenten: de componentenpagina moet weer verdwijnen. De feitelijke bestandsinhoud verandert nog niet per component; dit volgt in S07.

[Beginnersuitleg](../gebruikershandleiding/Componenten.md). Na acceptatie wordt S05 gecommit/gepusht. Taken volgen pas na opdracht voor S06.
