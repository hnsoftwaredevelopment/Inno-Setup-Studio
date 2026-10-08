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