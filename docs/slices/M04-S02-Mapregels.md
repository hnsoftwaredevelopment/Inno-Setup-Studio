# M04-S02 — Mapregels

Status: door de gebruiker geaccepteerd op m04-installatiekeuzes. M04-S01 is geaccepteerd en gepubliceerd als bec03f0. De gebruiker heeft ook het resultaat van de uitgevoerde installer gecontroleerd.

Doel: losse native Dirs-regels beheren, vooral voor lege mappen. Bestandsregels maken hun eigen doelmappen al aan. Mapregels blijven daarom een afzonderlijk onderdeel; het bestandsvoorbeeld doet geen uitspraak over expliciete lege doelmappen.

Omvang: doelpad onder {app}; mapkenmerken alleen-lezen, verborgen, systeem en niet-inhoudsgeïndexeerd; NTFS-compressie ongewijzigd/aan/uit; native standaarddeïnstallatie, behouden of ook een vooraf bestaande lege map proberen te verwijderen; optioneel na installatie verwijderen indien leeg. De laatste optie verwijdert geen vooraf bestaande map. Deïnstallatie verwijdert via deze flags geen niet-lege mappen.

Bron: [native Dirs-parameters en flags](https://jrsoftware.org/ishelp/topic_dirssection.htm). Permissions volgen met rechtenbeleid in M05; component-/taak-/typekoppelingen volgen in M04-S07. Geen nieuwe runtimehandelingen of simulatie van daadwerkelijke Windows-mapstatus. Andere doelconstanten volgen met het doelpadbeleid.

Projectformaat 11 bewaart de mapregels en opties. Oudere projecten krijgen een lege mapregellijst en behouden bestanden, documenten, talen en identiteit. Doelpaden zijn installerexpressies en worden bij Opslaan als niet herberekend. De toekomstige algemene instellingenpagina wordt niet in deze slice gebouwd.

## Bediening en grenzen

Open **Mapregels → Mapregels beheren**. Links staat de lijst met Toevoegen en Verwijderen. Rechts staan doelmap, kenmerken, compressie en opruimgedrag. Een nieuw project heeft geen mapregels; Toevoegen begint met {app}\Data. Alle regels kunnen weer worden verwijderd. Wijzigingen zijn direct projectwijzigingen; alleen openen of wisselen van regel maakt het project niet gewijzigd.

Ongeldige paden en opties krijgen direct een rode melding onder het pad. Opslaan en export weigeren ongeldige regels; een bestaand projectbestand blijft bij een validatiefout intact. Het venster controleert alleen installerexpressies, niet het bestaan of de kenmerken van mappen op deze computer. Er wordt lokaal niets aangemaakt of verwijderd.

Het doelpad blijft binnen {app}; puntsegmenten, extra constanten en scriptinvoer worden geweigerd. Native standaard-/behoud-/ook-bestaande-lege-mapbeleid en compressie zijn elk één keuze, zodat tegenstrijdige flags niet gecombineerd worden. Alleen-lezen is een Windows-mapkenmerk en verandert geen ACL-schrijfrechten. Bestaande mapkenmerken worden aangevuld; compressie wijzigt niet de compressiestatus van bestanden die al in de map staan.

De mapregellijst wordt met het project opgeslagen. Er zijn nog geen componenten, taken of typen om naar te verwijzen; koppelingen volgen volgens planning in S07. Het bestandsvoorbeeld blijft uitsluitend bestandsregels tonen. De korte wizardproef voert mapregels niet uit. Lege bronmappen worden niet automatisch omgezet: maak daarvoor expliciete mapregels.

## Verificatie

Release-build: 0 fouten en 0 waarschuwingen. Alle 253 automatische tests geslaagd, zonder overgeslagen tests. Negen nieuwe testgevallen controleren native export, alle attributen en beide compressieflags, exclusief deïnstallatiebeleid, ongeldige paden/opties, veilige opslag, bewaren/openen/Opslaan als, migratie van formaat 10 en regelbeheer tot een lege lijst. De echte Inno Setup 7.1-compiler heeft een project met alle beleidskeuzes en kenmerken gecompileerd; de installer is niet gestart.

Een WPF-proef heeft de dialoog zonder tonen gecontroleerd in NL/EN/DE: openen zonder wijzigingsstatus, toevoegen, wisselen, verwijderen tot nul, opnieuw toevoegen, beleidsbindingen en directe foutmelding/herstel. Dit is geen visuele kliktest. De bestaande migratieproeven zijn aangepast om het nieuwe veld uit historische JSON-fixtures weg te laten.

## Handmatige controles

1. Open een bestaand project uit S01. Controleer dat bestandsregels, uitsluitingen en talen behouden zijn en dat de lijst Mapregels leeg begint. Voeg {app}\Empty en {app}\Logs toe, wissel regels, wijzig opties en verwijder desgewenst beide. Daarna moet opnieuw toevoegen mogelijk blijven.
2. Probeer `{app}\..\Outside` of een leeg pad. De fout staat meteen rood onder het pad; herstellen wist de fout. Geldige regels kunnen worden opgeslagen. Bewaar/open en gebruik Opslaan als in een andere map: doelpaden blijven hetzelfde en bestandsbronnen blijven correct verwijzen.
3. Exporteer en controleer de Dirs-sectie, Name, Attribs en Flags. Zonder mapregels hoort geen Dirs-sectie te verschijnen. Bouw de installer en controleer dat een expliciete lege map wordt aangemaakt.
4. Test in een eigen proefmap het standaardbeleid met een nieuwe lege map en met een al bestaande lege map. Test vervolgens Behouden en Ook vooraf bestaande lege map verwijderen. Niet-lege mappen mogen via deze flags niet worden verwijderd. Gebruik hiervoor een schoon testproject en geen waardevolle gegevens.
5. Test Na installatie verwijderen als leeg met een tijdelijke nieuw aangemaakte map. Een al bestaande map hoort behouden te blijven. Controleer verborgen/alleen-lezen of compressie alleen als je dat wilt testen; Windows-/bestandssysteemgedrag wordt door Inno Setup uitgevoerd.

Na gebruikersakkoord wordt S02 gecommit/gepusht. S03 en de latere instellingenpagina worden niet automatisch gestart.
