# Historie — visuele proefversie

Status: historische planning van de eerste werkplek en Studio-meertaligheid, gearchiveerd op 8 oktober 2026. De onderstaande teksten beschrijven hun oorspronkelijke fase; onder meer de uitspraak over het ontbreken van een remote is historisch. De actuele planning staat in [tasks/plan.md](../../tasks/plan.md), de actuele afspraken in [Werkwijze](../Werkwijze.md).

Deze oorspronkelijke afvinklijst is behouden als bouwgeschiedenis. Zij is geen bewijs van volledige productoplevering of geaccepteerde installerexport.

---

# Eerste stap — werkende visuele editor

De gebruiker heeft de productspecificatie goedgekeurd en opdracht gegeven de eerste stap te bouwen.

## Afbakening

Een afzonderlijke WPF-app met een werkend voorbeeldproject: Basisontwerp, Welkom en Installatiemap. Selectie in de preview en elementenlijst toont een contextueel eigenschappenpaneel. Knopteksten erven van het Basisontwerp, met lokale afwijkingen en herstellen. Ontwerpen en uitproberen zijn gescheiden. Projecten kunnen worden opgeslagen en geopend.

Geen scriptgeneratie of compilerkoppeling in deze stap. De interface benoemt dat dit een ontwerpvoorbeeld is. Geen willekeurige positionering of kleurinstellingen die de standaardknoppen van Inno Setup niet ondersteunen.

## Volgorde en controlepunten

1. Solution, WPF-onafhankelijk model en tests voor overerving.
2. Editorstatus voor selectie, uitproberen en wijzigingen; gecontroleerde JSON-opslag.
3. WPF-werkplek met live eigenschappen en toetsenbordbediening.
4. Build, tests, weergavecontrole, documentatie en Obsidian-spiegeling.

## Ontwerpkeuzes

- WPF en .NET 10; geen nieuwe runtimebibliotheken nodig voor deze eerste stap.
- Projectopslag gebruikt voorlopig `.issstudio` met een expliciete formaatidentiteit en versie. Oude `.issproj`-bestanden worden niet overschreven of automatisch geconverteerd. Een importbesluit volgt later.
- De gedeelde knopteksten hebben eigen waarden; ontbrekende pagina-afwijkingen vallen daarop terug. Een lege lokale tekst blijft een expliciete afwijking.
- Klein beginnen met Core en App; aparte Preview- en InnoSetup-projecten pas als hun omvang dat rechtvaardigt.
- Git-repository uitsluitend in de nieuwe projectmap; geen remote aangemaakt.

## Risico’s

De preview is een benadering van Inno Setup, geen pixelgarantie. Daarom blijft die beperking zichtbaar. Opslag moet eerst het volledige document valideren en via een tijdelijk bestand vervangen. Annuleren bij openen of sluiten mag wijzigingen niet verliezen.

## Uitbreiding — Studio-meertaligheid

De gebruiker heeft opdracht gegeven de basis voor Nederlands, Engels en Duits nu te bouwen, los van toekomstige installerlocalisatie.

1. Neutrale Nederlandse en Engelse/Duitse resources, taalservice en persoonlijke instelling.
2. Live bindings voor de werkplek, editorstatus en meldingen; projectinhoud en selectie behouden.
3. Controles op volledige vertalingen, terugval, opslag, uitprobeergedrag en herstart.
4. Handleiding voor ResXManager en synchronisatie naar Obsidian.

Gerealiseerd op 8 oktober 2026. De resources staan in Core zodat zowel editorlogica als WPF dezelfde teksten gebruiken zonder WPF-afhankelijkheid in Core. De WPF-markup-extensie maakt hiervan live bindings. De eerste stap en de taaluitbreiding zijn lokaal gecontroleerd; scriptgeneratie blijft de volgende productfase.

---

# Taken — eerste stap

- [x] Model voor gedeelde knopteksten en pagina-afwijkingen. Acceptatie: wijzigen, overerven, lege tekst en herstellen zijn getest. Controle: Core-tests. Bestanden: model en modeltests.
- [x] Editorstatus. Acceptatie: ontwerpen selecteert, uitproberen navigeert, afsluiten herstelt de ontwerpselectie. Controle: status-tests. Afhankelijkheid: model. Bestanden: editorstatus, notificatiebasis en tests.
- [x] Projectopslag. Acceptatie: roundtrip behoudt afwijkingen; ongeldige of vreemde bestanden worden afgewezen. Controle: opslagtests. Afhankelijkheid: model. Bestanden: opslagservice en tests.
- [x] WPF-preview. Acceptatie: twee pagina’s en Basisontwerp zichtbaar; knopselectie en live tekst werken. Controle: build en weergavecontrole. Afhankelijkheid: editorstatus. Bestanden: preview en stijlen.
- [x] WPF-werkplek. Acceptatie: pagina’s links, ontwerp midden, eigenschappen rechts; openen, opslaan en waarschuwing voor niet-opgeslagen wijzigingen. Controle: build en integratiecontrole. Afhankelijkheid: preview en opslag. Bestanden: App en MainWindow.
- [x] Oplevering. Acceptatie: tests en build slagen; handleiding en status bijgewerkt en gespiegeld. Controle: hashes en Git-status.

## Studio-meertaligheid — 8 oktober 2026

- [x] Nederlandse neutrale resources plus Engels en Duits voor ResXManager.
- [x] Directe taalwissel en aparte persoonlijke taalinstelling.
- [x] Installerinhoud, ontwerpselectie en uitprobeerwaarden blijven behouden.
- [x] Vertaalde eigenschappen, navigatie, statusmeldingen en dialoogtitels.
- [x] Selectielus opgelost met stabiele lijsten en regressietest.
- [x] 31 tests, Release-build en visuele taalcontrole met herstart.
- [x] Documentatie bijgewerkt en gespiegeld naar Obsidian.
