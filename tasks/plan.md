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
