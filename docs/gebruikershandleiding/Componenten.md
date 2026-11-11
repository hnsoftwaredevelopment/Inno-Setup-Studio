# Componenten

Een component is een onderdeel van je installatie, bijvoorbeeld het programma zelf of een handleiding. Met installatietypen zoals Volledig en Compact bepaal je welke onderdelen vooraf worden gekozen. Een aangepast type laat de gebruiker zelf onderdelen kiezen.

In de huidige versie kun je componenten en hun typekeuzes instellen en uitproberen. De componentenpagina verschijnt ook in de gebouwde installer. Bestanden, mapregels en snelkoppelingen worden in een latere stap aan componenten gekoppeld. Een onderdeel uitvinken slaat die bestanden nu dus nog niet over.

## Voorbeeld: programma en handleiding

1. Maak bij **Installatietypen** de typen `full` (Volledig), `compact` (Compact) en `custom` (Aangepast). Markeer alleen `custom` als aangepast type.
2. Open **Componenten → Componenten beheren** en klik op **Toevoegen**.
3. Geef de standaardomschrijving `Programma` en de interne naam `main` op.
4. Vink bij de typekoppelingen alle drie de typen aan. Open **Extra opties** en kies **Vast onderdeel**. Daarmee kan de gebruiker dit onderdeel niet zelf uitvinken.
5. Voeg een tweede component toe met de omschrijving `Handleiding` en de interne naam `help`. Koppel deze aan `full`. Laat Vast onderdeel uit.
6. Open **Uitproberen** en ga naar de componentenpagina. Bij Volledig is de handleiding geselecteerd, bij Compact niet. De programmacomponent is vast. Bij Aangepast kun je zelf selecteren.

Vast betekent niet automatisch dat een onderdeel altijd wordt geïnstalleerd: de typekoppelingen bepalen nog steeds of het geselecteerd is. Koppel een verplicht onderdeel daarom aan ieder gewenst type. Niet-vaste componentkoppelingen aan een aangepast type worden door Inno Setup genegeerd. Bij overschakelen naar Aangepast blijven de bestaande niet-vaste selecties behouden; de gebruiker kan ze daarna wijzigen.

Zonder eigen installatietypen gebruikt Inno Setup de standaardtypen full, compact en custom. Die zijn dan ook als koppeling beschikbaar. Voeg je later eigen typen toe, zorg dan dat alle bestaande verwijzingen weer bij die typen passen; Studio weigert ontbrekende verwijzingen bij opslaan of exporteren.

## Vertalingen

De vertalingen staan direct onder de standaardomschrijving. Selecteer iedere installertaal afzonderlijk en vul de omschrijving voor die taal in. Leeg betekent dat de standaardomschrijving wordt gebruikt; deze staat dan grijs in het invoerveld. Veranderen van de Studio-taal vertaalt deze eigen installerinhoud niet.

Uitschakelen van een installertaal bewaart haar vertalingen. Verwijderen verwijdert ook haar componentvertalingen via de bestaande bevestiging.

## Onderliggende onderdelen

Selecteer bijvoorbeeld `help` en klik op **Onderdeel toevoegen**. Studio maakt een onderliggend onderdeel, bijvoorbeeld `help\component1`. Geef dit de naam `help\dutch` en een herkenbare omschrijving. Het voorste deel verwijst naar de bovenliggende groep.

Een onderliggend onderdeel staat altijd binnen de groep van zijn bovenliggende onderdeel. Met **Omhoog** en **Omlaag** verplaats je een onderdeel tussen onderdelen op hetzelfde niveau; eventuele onderliggende onderdelen gaan mee. Een groep kan pas worden verwijderd of hernoemd als de onderliggende onderdelen zijn verwijderd.

Een groep met onderliggende onderdelen volgt hun selectie. Staat slechts een deel aan, dan toont de proef een gemengde selectiestatus. Met **Ook selecteerbaar zonder geselecteerde onderliggende onderdelen** kun je de groep ook apart selecteerbaar maken. Voor eenvoudige onafhankelijke onderdelen hoef je deze optie niet te gebruiken.

## Gebruikte typen aanpassen

Een type dat door componenten wordt gebruikt, kan niet direct worden hernoemd of verwijderd. Het typenvenster toont welke componenten het gebruiken. Pas eerst hun koppelingen aan. De omschrijvingen, vertalingen en volgorde van het type kun je wel veranderen.

Interne namen bestaan uit letters, cijfers en underscores, eventueel gescheiden door een backslash voor onderliggende onderdelen. Elk segment begint met een letter of underscore, is maximaal 128 tekens en mag niet and, or of not zijn. De volledige naam is maximaal 4096 tekens; het overzicht is begrensd op 1000 componenten. Omschrijvingen en vertalingen staan op één regel van maximaal 4096 tekens.

## Wat bewijst Uitproberen?

De proef laat de keuzes van een eerste installatie zien, met de gekozen installertaal en je eigen beschrijvingen. Teruggaan en opnieuw verdergaan behoudt de tijdelijke keuzes. Sluiten verandert het opgeslagen project niet. Eerdere installatiekeuzes, commandoregelopties, schijfruimte en het daadwerkelijk installeren van verschillende bestanden worden niet gesimuleerd. Zonder aangepast type toont de wizard alleen de typekeuze, geen vrij te wijzigen componentenlijst.
