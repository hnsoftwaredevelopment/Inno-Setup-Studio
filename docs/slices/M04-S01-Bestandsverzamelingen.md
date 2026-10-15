# M04-S01 — Bestandsverzamelingen

Status: geïmplementeerd op m04-installatiekeuzes, na merge van M03 naar main (84c8a06). Door de gebruiker geaccepteerd op 15 oktober 2026.

Resultaat: meerdere lokale bestandsregels met een bronbestand of wildcard in de bestandsnaam, een doel onder {app}, submappen en native Excludes. Een apart beheervenster biedt regelkeuze en een alleen-lezen voorbeeld van bron- en doelbestanden. De export houdt wildcardregels intact; het voorbeeld wordt niet als vaste bestandslijst opgeslagen.

De bestaande eerste bestandsregel blijft behouden; aanvullende regels krijgen dezelfde eigenschappen. Projectformaat 10 bewaart de aanvullende regels en opties. Versies 1–9 migreren zonder bronnen te verliezen; Opslaan als verplaatst relatieve verwijzingen consequent. Er blijft minstens één regel; een nieuw project mag een nog lege bron hebben, maar bouwen vereist geldige bronnen.

Bron: [native Files-parameters en flags](https://jrsoftware.org/ishelp/topic_filessection.htm). Alleen lokale bronnen; wildcards in mapnamen, downloads, externe bestanden en overige flags zijn niet onderdeel van S01. Lege mappen volgen in S02. Native Source-wildcards en Unix-achtige Excludes hebben verschillende punt-/vraagtekenregels. Het voorbeeld controleert ook uitgesloten submappen. Onleesbare bronnen, gekoppelde submappen of te grote verzamelingen geven uitleg in plaats van een onvolledige lijst als volledig te presenteren.

## Bediening en afbakening

Open **Bestandsregels → Bestanden beheren**. Links staat de regelkeuze met Toevoegen/Verwijderen; rechts staan bron, doel onder {app}, Submappen meenemen en uitsluitingen (één patroon per regel). Bronmap kiezen vult een wildcard in; submappen blijven een afzonderlijke keuze. Wijzigingen werken direct door in het project. Sluiten is geen annulering van regelwijzigingen.

Het voorbeeld verschijnt automatisch na een korte typepauze en toont alleen de geselecteerde regel. Vernieuwen leest externe bronwijzigingen opnieuw. Een achtergrondtaak leest een momentopname van de regel; oudere resultaten worden bij wijzigen/sluiten geannuleerd. Het voorbeeld verandert de projectstatus niet. Fouten staan afzonderlijk rood boven de lijst. Tot 10.000 passende bestanden en 50.000 submappen per regel worden gecontroleerd; bij overschrijding ontstaat een fout, geen schijnbaar volledige deellijst. Dezelfde broncontrole wordt bij export gebruikt.

De native compiler verzamelt de bronnen opnieuw tijdens bouwen. Het voorbeeld is daarom een momentopname en geen garantie tegen latere bronwijzigingen. Bestanden met het kenmerk Verborgen worden bij wildcards overgeslagen; verborgen submappen worden niet doorlopen, overeenkomstig de native compiler. Gekoppelde submappen geven een expliciete fout. Overlappende regels blijven zelfstandige native regels; deze slice voegt geen deduplicatie, component-/taakkoppelingen of lege mapregels toe.

Exportbescherming geldt voor bronbestanden van alle regels, inclusief wildcardmatches: ook met expliciete vervangingstoestemming kan een payloadbestand niet door de export worden vervangen. Doelpaden kunnen niet via puntsegmenten buiten {app} terechtkomen. Opslag blijft atomair; relatieve bronnen van zowel eerste als aanvullende regels worden bij Opslaan als herberekend.

## Verificatie

Release-build zonder fouten of waarschuwingen. Alle 244 tests geslaagd, inclusief native compilatie van meerdere regels en controle van het compilerlog: een bestand uit een submap wordt gecomprimeerd, een uitgesloten bestand niet. Nieuwe tests controleren verschillende wildcardsemantiek, recursie, uitsluitingen, opslag/Opslaan als, migratie van formaat 9, regelbeheer, meerdere uitsluitingsregels, doelpadgrenzen en bescherming van aanvullende payloadbronnen.

Een WPF-proef heeft het nieuwe venster zonder tonen aangemaakt en automatisch laden, regelkeuze/toevoegen/verwijderen, meerdere uitsluitingsregels, foutmelding en herstel gecontroleerd. Dit vervangt geen visuele gebruikerstest. Installers zijn alleen gecompileerd, niet gestart.

## Handmatige controles

1. Open een bestaand project. De oude regel staat in Bestanden beheren. Controleer bron/doel en dat alleen openen of vernieuwen geen ontwerpwijziging maakt. Opslaan schrijft formaat 10; eerdere Studio-versies kunnen dat nieuwe formaat niet lezen.
2. Maak een kleine eigen bronmap met enkele bestanden, een submap, een bestand zonder extensie en een map Temp. Kies de bronmap en vergelijk de lijst met Submappen meenemen aan/uit. Controleer dat *.* ook een bestand zonder extensie selecteert.
3. Voeg uitsluitingen *.pdb en \\Temp toe op afzonderlijke regels. De lijst moet direct aansluiten; een uitsluiting *.* laat juist bestanden zonder extensie staan. Beginnen met \\ verankert de uitsluiting aan de bronmap.
4. Voeg een tweede regel met een ander doel, bijvoorbeeld {app}\\Data, toe. Wissel regels en wijzig ze. Verwijder een regel en controleer dat de andere behouden blijft. De laatste regel kan niet worden verwijderd.
5. Probeer een ontbrekende bron en een ongeldig doel; controleer de rode melding en herstel met een geldige waarde. Wijzig snel tussen regels om oude resultaten uit te sluiten.
6. Bewaar/open opnieuw en gebruik Opslaan als in een andere projectmap. Bronbestanden moeten naar dezelfde echte bestanden blijven verwijzen. Controleer opties en uitsluitingen na heropenen.
7. Exporteer, bouw en voer een kleine installer uit. Vergelijk de geïnstalleerde bestanden en submappen met de gekozen regels. S02 en latere slices volgen pas na afzonderlijke startopdracht.

Gebruikerscontrole: toevoegen/beheren en de echte installatie zijn getest; een bewust toegevoegd PDB-bestand wordt door *.pdb uitgesloten. Opmerking over de benaming verwerkt: het huidige onderdeel heet Bestandsregels. De latere verplaatsing van projectinstellingen wordt apart gepland; de slice is na deze aanpassing door de gebruiker geaccepteerd.
