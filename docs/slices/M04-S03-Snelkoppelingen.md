# M04-S03 — Snelkoppelingen

Beginnersuitleg: [Snelkoppelingen maken](../gebruikershandleiding/Snelkoppelingen.md). Deze legt het doel uit en geeft voorbeelden voor een programma en documentatie, zonder kennis van Inno Setup-paden of opstartargumenten te veronderstellen.

Status: door de gebruiker geaccepteerd op 17 oktober 2026, op m04-installatiekeuzes na geaccepteerde S02 (d03dfd2). De gebruiker heeft de werking en het vereenvoudigde venster bevestigd.

Omvang: meerdere snelkoppelingen met naam, geïnstalleerd doelbestand, parameters, optionele werkmap, Startmenu/Bureaublad en optionele submap. Doelen worden uit de bestandsregels verzameld; ontbrekende doelen worden zichtbaar gemeld en blokkeren export/bouwen. Naam ondersteunt de bestaande Studio-verwijzingen {AppName}/{AppVersion}. Geen URL’s, externe doelen, losse pictogrammen of aanvullende flags in deze slice; overige native opties volgen in de catalogus.

De export gebruikt native Icons-regels. Het paginaoverzicht en de wizardproef nemen de startmenupagina mee zodra native voorwaarden dit toestaan. Inno Setup gebruikt de aanwezigheid van enige Icons-regel voor HasIcons; ook een uitsluitend op het Bureaublad geplaatste snelkoppeling kan daarom de startmenupagina beschikbaar maken. Automatisch paginabeleid hangt af van een eerdere installatie en wordt als voorwaardelijk getoond; de proef toont het scenario van een eerste installatie. Bestaande startmenu-instellingen blijven leidend. Een lege standaardmap gebruikt de native naam (Default).

Projectformaat 12 bewaart snelkoppelingen; eerdere projecten krijgen een lege lijst. Het doel is een installerpad en blijft bij Opslaan als ongewijzigd. Bestandsbronnen worden wel volgens de bestaande regels herberekend. De proef en het beheervenster maken geen Windows-snelkoppelingen aan en starten geen programma’s.

Bronnen: [Icons](https://jrsoftware.org/ishelp/topic_iconssection.htm), [DisableProgramGroupPage](https://jrsoftware.org/ishelp/topic_setup_disableprogramgrouppage.htm), [DefaultGroupName](https://jrsoftware.org/ishelp/topic_setup_defaultgroupname.htm) en [AllowNoIcons](https://jrsoftware.org/ishelp/topic_setup_allownoicons.htm). Rechten/doelarchitectuur en component-/taakkoppelingen volgen in hun geplande slices. De algemene instellingenpagina blijft uitgesteld.

## Bediening en validatie

Open **Snelkoppelingen → Snelkoppelingen beheren**. Voeg regels toe en kies een doel uit de toekomstige installatiepaden van de bronbestanden. Vooraf installeren is niet nodig. Alle opgenomen bestandstypen zijn beschikbaar, inclusief PDF en RTF. Alleen naam, plaatsing en doelbestand staan direct in beeld; submap, parameters, werkmap en handmatige doelpadinvoer staan onder **Extra opties**. Doelen moeten bestanden onder {app} zijn die door de huidige bestandsregels worden meegenomen. De doelindex wordt bij openen automatisch opgebouwd; Vernieuwen leest wijzigingen buiten Studio opnieuw. Er worden maximaal 100.000 verschillende doelen geïndexeerd, naast de bestaande begrenzing per bestandsregel. Bij een fout wordt geen onvolledige index als volledig gepresenteerd.

Naam en plaatsing/submap bepalen samen de unieke snelkoppelingslocatie. Dubbele locaties, ongeldige namen/paden en scriptinvoer worden geweigerd. Ook de uitgewerkte naam is begrensd. Letterlijke accolades in product-/bestandsnamen worden bij export correct gecodeerd. Parameters ondersteunen de bestaande Studio-productverwijzingen en native constanten; aanhalingstekens worden veilig als parameterdata geschreven. Geen parameters worden door Studio uitgevoerd.

Het venster vermeldt hoeveel doelverwijzingen ontbreken; de geselecteerde regel krijgt een rode melding. Structureel ongeldige regels kunnen niet worden opgeslagen. Een syntactisch geldig maar ontbrekend doel mag bewaard blijven om later te herstellen, maar export/bouwen wordt geblokkeerd. De fout vermeldt snelkoppelingsnaam en doel. Wijzigingen in bestandsregels moeten dus ook worden gecontroleerd tegen de snelkoppelingen. Werkmap wordt als doelpad gevalideerd; er is geen lokale mapkeuze. Een lege werkmap gebruikt het native gedrag, normaal de map van het doelbestand.

**Startmenu instellen** opent dezelfde instellingen als Paginaoverzicht. De standaardmap wordt expliciet getoond; leeg betekent native (Default), niet automatisch de productnaam. Stel bijvoorbeeld {AppName} in als standaardmap als dat gewenst is. Paginaoverzicht volgt het beleid Tonen/Verbergen/Automatisch. In Uitproberen komt de startmenupagina na Installatiemap en vóór Gereed. Mapnaam en de eventuele optie Geen startmenumap blijven tijdelijk; de laatste keuze geldt native voor de huidige startmenu-snelkoppelingen, niet voor bureaubladsnelkoppelingen. De simulatie leest geen eerdere installatiegegevens.

## Verificatie

Release-build zonder fouten of waarschuwingen. Alle 263 tests geslaagd, zonder overgeslagen tests. Tien nieuwe testgevallen controleren export/parametercodering, ontbrekende verwijzingen, ongeldige/te lange namen, opslag/Opslaan als, migratie van formaat 11, regelbeheer en tijdelijke startmenu-invoer. De native compilerproef gebruikt beide locaties, gequote parameters en letterlijke accolades in product- en doelbestandsnamen. Installers zijn niet uitgevoerd.

Een niet-visuele WPF-proef controleert NL/EN/DE: openen zonder wijzigingsstatus, behoud van een bestaand doel bij automatisch laden/vernieuwen, toevoegen/verwijderen, doelkeuze, ontbrekende verwijzingen en het aanmaken/sluiten van de wizardproef. Native startmenuvoorwaarden zijn vergeleken met HasIcons en ShouldSkipPage in de aangeleverde Inno Setup-bron. Visuele en echte installatieproeven worden door de gebruiker uitgevoerd.

## Handmatige controles

1. Open een bestaand project. Voeg een Startmenu-snelkoppeling naar een executable uit de bestandsregels toe. Stel naam, parameters en eventueel werkmap/submap in. Voeg ook een Bureaublad-snelkoppeling toe; bewaren/openen en Opslaan als moeten alle waarden behouden.
2. Kies of typ een ontbrekend doel. Controleer de rode melding en de verwijzingenteller. Export/bouwen moet weigeren en de betreffende snelkoppeling en het doel benoemen. Herstel het doel; wijzig ook eens een bestandsregel of uitsluiting waardoor een doel niet meer wordt meegenomen.
3. Probeer twee namen op dezelfde locatie en een ongeldige naam. Herstel en verwijder alle regels; opnieuw toevoegen moet werken. Sluiten bewaart regelwijzigingen in het project; alleen openen/vernieuwen maakt het project niet gewijzigd.
4. Stel via Startmenu instellen de standaardmap op {AppName}. Controleer Paginaoverzicht en Uitproberen bij Tonen, Verbergen en Automatisch. Automatisch is voorwaardelijk; de proef toont een eerste installatie. Test tijdelijk een andere mapnaam en Geen startmenumap als AllowNoIcons aanstaat. De ontwerpwaarden moeten na sluiten gelijk blijven.
5. Bouw en voer de installer uit. Controleer de Startmenu- en Bureaublad-snelkoppelingen en hun eigenschappen, waaronder doel, parameters en werkmap. Start de applicatie zelf om de ingestelde parameters te beoordelen. Test bij voorkeur ook een tweede installatie, omdat native Automatisch dan de startmenupagina kan overslaan.

S03 wordt na dit akkoord gecommit/gepusht. S04 en de algemene instellingenpagina worden niet automatisch gestart.

## Correctie na gebruikerscontrole

De doelkeuze gebruikt nu een expliciete selectie in plaats van een bewerkbare combobox. Een lege selectie tijdens vernieuwen wist het opgeslagen doel niet meer. Dit verlies is eerst gereproduceerd met een niet-visuele WPF-proef die daadwerkelijk een lijstitem selecteert. Een afzonderlijke regressietest controleert behoud bij vernieuwen, documenten en weigeren van doelen buiten de index. Een leeg doel krijgt een gerichte keuzemelding. Controleer opnieuw: doel kiezen zonder vooraf te installeren, vernieuwen, wisselen tussen regels, opslaan/heropenen en export/bouwen. Beoordeel ook het vereenvoudigde venster met ingeklapte extra opties.

### Bureaublad en documentatie

Snelkoppelingen mogen naar alle opgenomen bestanden verwijzen, ook PDF/RTF. Windows gebruikt de gekoppelde toepassing om documenten te openen. Bij Bureaublad worden de startmenu-instellingen verborgen; de UI meldt dat de snelkoppeling voorlopig altijd wordt aangemaakt. Optioneel aanmaken gebruikt native Tasks, gepland bij M04-S06 (taken) en M04-S07 (regelkoppelingen); dit wordt niet vervroegd ingebouwd. De niet-visuele WPF-proef controleert het wisselen van de contextuele instellingen bij Startmenu/Bureaublad in alle drie interfacetalen.
