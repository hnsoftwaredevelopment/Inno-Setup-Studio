# ADR-002 — Projectmodel en gegenereerde scripts

Datum: 8 oktober 2026.
Status: geaccepteerd op 8 oktober 2026; technische concretisering in M01 en M08.

## Context

De huidige projecten bevatten ontwerpgegevens in .issstudio. Het eindproduct moet buiten Studio bruikbare .iss-scripts leveren en tegelijk eigen scripts kunnen behouden. Willekeurige scriptinvoer volledig visueel teruglezen zou een afzonderlijk parser-/importproject vereisen.

## Voorgesteld besluit

Het projectmodel is de bron voor visueel beheerde instellingen. Gegenereerde uitvoer is leesbaar en kan buiten Studio worden gebruikt, maar is geen tweede automatisch gesynchroniseerde projectbron.

Eigen Pascal-/preprocessorcode krijgt een afzonderlijk beheerde bron of gecontroleerd invoegpunt. De keuze tussen projectinterne opslag en losse bronbestanden wordt vóór M08-S01 vastgelegd; beide vereisen verplaatsbare verwijzingen en export zonder Studio-runtime.

Een instelling heeft één eigenaar. Eigen en gegenereerde eventimplementaties worden gecombineerd via een gecontroleerde aanpak op basis van de officiële mogelijkheden. Identifiers, volgorde en conflicten worden getest vóór aanbieding.

Bestanden die handmatig buiten Studio zijn aangepast worden bij hergenereren herkend; de gebruiker krijgt een bewuste keuze voordat uitvoer wordt vervangen.

## Alternatieven

Volledig vrije .iss-bewerking als enige bron is flexibel, maar maakt betrouwbaar visueel beheer en lokale overerving moeilijk. Twee bronnen automatisch heen en weer synchroniseren vergroot de scope naar algemene Inno Setup-import. Daarom kiezen we voor expliciet eigenaarschap.

## Gevolgen

Zelfstandig compileren blijft mogelijk met de export en genoemde resources. Eigen code behoudt haar betekenis bij opnieuw genereren. Import van een willekeurige .iss blijft een aparte mogelijke uitbreiding.

Formaatmigraties, scriptbronverwijzingen, foutlocaties en exportconflicten krijgen gerichte tests. De preview voert eigen scripts niet uit en claimt geen algemene scriptinterpretatie.
