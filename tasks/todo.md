# Taken — eerste stap

- [ ] Model voor gedeelde knopteksten en pagina-afwijkingen. Acceptatie: wijzigen, overerven, lege tekst en herstellen zijn getest. Controle: Core-tests. Bestanden: model en modeltests.
- [ ] Editorstatus. Acceptatie: ontwerpen selecteert, uitproberen navigeert, afsluiten herstelt de ontwerpselectie. Controle: status-tests. Afhankelijkheid: model. Bestanden: editorstatus, notificatiebasis en tests.
- [ ] Projectopslag. Acceptatie: roundtrip behoudt afwijkingen; ongeldige of vreemde bestanden worden afgewezen. Controle: opslagtests. Afhankelijkheid: model. Bestanden: opslagservice en tests.
- [ ] WPF-preview. Acceptatie: twee pagina’s en Basisontwerp zichtbaar; knopselectie en live tekst werken. Controle: build en weergavecontrole. Afhankelijkheid: editorstatus. Bestanden: preview en stijlen.
- [ ] WPF-werkplek. Acceptatie: pagina’s links, ontwerp midden, eigenschappen rechts; openen, opslaan en waarschuwing voor niet-opgeslagen wijzigingen. Controle: build en integratiecontrole. Afhankelijkheid: preview en opslag. Bestanden: App en MainWindow.
- [ ] Oplevering. Acceptatie: tests en build slagen; handleiding en status bijgewerkt en gespiegeld. Controle: hashes en Git-status.
