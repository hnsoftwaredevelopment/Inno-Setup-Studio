# M02-S02 — Projecttalen en previewkeuze

Status: door de gebruiker geaccepteerd op 13 oktober 2026 op m02-installertalen.

## Scope en afspraken

Deze slice bewaart projecttalen, ingeschakelde status en volgorde. De eerste ingeschakelde taal is de terugvaltaal. Minstens één taal blijft actief. Uitschakelen behoudt de bron en alle bestaande teksten; verwijderen en eigen vertalingen bewerken volgen in S03/S04.

Nieuwe Studio-projecten beginnen met Engels, onafhankelijk van de Studio-taal. Projecten in formaat 1–4 worden naar formaat 5 gemigreerd met Nederlands als taal van hun bestaande teksten. Titels, bodyteksten, knoppen, lokale afwijkingen, mapinstellingen, bestandsregel, identiteit en bestaande uitvoernaam blijven behouden. Alleen formaten vóór 4 krijgen de historische uitvoernaam setup. Migratie gebeurt in het geheugen; het bronbestand verandert pas bij opslaan.

Voorbeeldtaal bevat uitsluitend ingeschakelde projecttalen. Deze tijdelijke keuze staat buiten de projectopslag en verandert de wijzigingsstatus niet. Studio-taal, ontwerpselectie en tijdelijke invoer in Uitproberen blijven onafhankelijk. Na opnieuw openen begint de preview bij de eerste ingeschakelde taal.

De bestaande eigen teksten blijven gekoppeld aan hun oorspronkelijke taal. Bij die taal blijven de huidige tekstvelden bewerkbaar. Andere talen tonen voorlopig standaardberichten uit de gekozen taalbron; hun tekstvelden zijn alleen-lezen totdat S03 eigen vertalingen toevoegt. Het installatiepad is taalneutraal en blijft bewerkbaar. Studio kopieert geen Nederlandse eigen tekst naar een andere taal.

## Opslag en preview

Een projecttaal bevat een eigen identifier, oorspronkelijke taalnaam, numerieke LanguageID, bronverwijzing, ingeschakelde status, RTL-indicatie en een momentopname van de negen berichten voor de huidige twee pagina's. LanguageID is geen unieke bronidentifier: verschillende eigen bronnen met dezelfde ID zijn toegestaan. Dezelfde bronverwijzing wordt niet dubbel toegevoegd; de broncatalogus herkent daarnaast identieke kopieën.

Bronnen binnen de compilerinstallatie gebruiken compiler-relatieve verwijzingen. Eigen bronnen gebruiken een absoluut pad. De berichtmomentopname houdt de preview beschikbaar zonder compiler of bronbestand; ze is geen volledige kopie voor distributie. Opnieuw laden in de broncatalogus wijzigt een al toegevoegde projecttaal niet. Bronhercontrole bij export en volledige taalexport volgen in S05.

Voor standaardberichten worden de native verwijzingen [name], [name/ver], %1 en %n voor weergave verwerkt; toegangstoetsmarkeringen worden niet als ampersand weergegeven. Dit voegt nog geen verwijzingen toe aan de bestaande eigen tekstvelden; dat hoort bij S03. Ontbrekende bronberichten blijven herkenbaar als [berichtnaam]. De preview meldt RTL, maar spiegelt de wizard nog niet en bewijst geen native lettertypegedrag.

## Export tijdens deze tussenstap

De bestaande uitsluitend Nederlandse export blijft beschikbaar. Nieuwe uitsluitend Engelse projecten exporteren met compiler:Default.isl. Export/bouwen van andere of meerdere ingeschakelde talen wordt met uitleg geblokkeerd tot S05; Studio genereert dus geen Nederlandstalige installer bij een meertalige projectkeuze. Een nieuw project kan weer worden gebouwd zodra uitsluitend de taal van de eigen projectteksten actief is.

## Gebruikerscontrole

1. Maak een nieuw project: Engels staat bij Voorbeeldtaal en de wizardteksten zijn Engels. Wissel Studio naar Nederlands/Duits; de wizard blijft Engels.
2. Open Projecttalen onder de compilerinstellingen. Kies Taal toevoegen, selecteer bijvoorbeeld Nederlands of Duits en klik Aan project toevoegen. Sluit de bronlijst en het projecttalenvenster. De taal staat nu bij Voorbeeldtaal.
3. Wissel Voorbeeldtaal. De titel, body, knoppen en mapkeuzeknop volgen de bron. De herkomst vermeldt dat eigen vertalingen in S03 volgen; de tekstvelden zijn bij aanvullende talen alleen-lezen.
4. Zet talen omhoog/omlaag en schakel een taal uit. Bewaar en heropen: volgorde en status blijven behouden. Uitschakelen verwijdert geen taal. De laatste actieve taal kan niet worden uitgeschakeld.
5. Open een bestaand Nederlands project: eigen teksten, mapinstellingen, bestandsregel, identiteit en uitvoernaam blijven gelijk. Bewaar onder een nieuwe naam en heropen. Wissel Studio-taal: Nederlandse eigen teksten blijven Nederlands.
6. Ga naar Uitproberen, voer op de installatiemap een tijdelijk pad in en wissel Voorbeeldtaal. Het pad blijft staan en de taalwissel maakt het project niet gewijzigd. Terug naar Ontwerpen herstelt de eerdere ontwerpselectie en het opgeslagen standaardpad.
7. Bouw een uitsluitend Engels project met een geldige bestandsbron en controleer de export op Default.isl. Met meerdere talen geeft bouwen/exporteren de aangekondigde melding voor S05. Controleer ook dat een oud uitsluitend Nederlands project nog kan bouwen.

## Verificatie

Release-build geslaagd zonder fouten of waarschuwingen. De volledige suite telt 138 geslaagde tests, zonder overgeslagen tests. Dit omvat formaatmigratie, ontbrekende/ongeldige taalgegevens, behoud van uitvoernaam en identiteit, taalvolgorde en inschakelen, stabiele previewbindings, onafhankelijke Studio-/previewtaal, tijdelijke mapinvoer, alleen-lezen aanvullende teksten en behoud van bronberichten na opnieuw openen zonder bronbestand. Productnamen worden bij de native berichtweergave als letterlijke waarden behandeld, ook als zij ampersands of berichtmarkeringen bevatten.

Echte compilatie met ISCC 7.1.0 slaagde voor de nieuwe Engelse export en de bestaande Nederlandse proeven, inclusief het SVG-voorbeeld. Er is geen gegenereerde installer gestart. Kliktests en echte installatie blijven gebruikerscontroles. Markdown is naar Obsidian gespiegeld en met hashes gecontroleerd. De gebruiker heeft selectie en volgorde, behoud na herstart/heropenen, previewweergave en de gegenereerde installer gecontroleerd en akkoord gegeven. Publicatie gebeurt op de milestonebranch; M02 wordt pas na de resterende slices afgerond.
