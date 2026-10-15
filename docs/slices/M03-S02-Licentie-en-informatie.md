# M03-S02 — Licentie en informatie

Status: door de gebruiker geaccepteerd op m03-wizardpaginas.

Resultaat: via het paginaoverzicht Licentie, Informatie vóór en Informatie na installatie instellen. Eén algemeen TXT/RTF-document per pagina, met optionele variant per projecttaal. Een leeg algemeen pad schakelt de pagina uit en behoudt varianten; zonder taalvariant wordt het algemene document gebruikt. Het document is als bestand gekoppeld, niet als kopie in het project. Bronbestanden worden niet gewijzigd.

Criteria: bronkeuze en varianten blijven behouden bij opslaan/openen en Opslaan als; de geselecteerde previewtaal bepaalt het documentvoorbeeld; paginaoverzicht volgt ingestelde bronnen. Export gebruikt native LicenseFile, InfoBeforeFile en InfoAfterFile en taalparameters. Ontbrekende/ongeldige bronnen blokkeren export met uitleg. Licentieacceptatie is native; Studio voert geen installer uit.

Afbakening: een documentvoorbeeld in het instelvenster, nog geen uitbreiding van de volledige Uitproberen-route. Geen vrije paginaordening. TXT-preview ondersteunt UTF-8 en UTF-16LE, RTF via WPF; opmaak kan van native Inno Setup verschillen. Algemene bron verplicht voor een actieve documentpagina. Projectformaat wordt gecontroleerd gemigreerd.

Bronnen: [LicenseFile](https://jrsoftware.org/ishelp/topic_setup_licensefile.htm), [Languages](https://jrsoftware.org/ishelp/topic_languagessection.htm).

Handmatige controle: configureer drie documenten, voeg een Nederlandse variant toe, wissel previewtaal, open/sla opnieuw op en gebruik Opslaan als in een andere map. Bouw en draai de installer: weiger/accepteer licentie, controleer documenten vóór/na installatie en terugnavigatie. Wis algemeen pad en controleer verdwijnen van de pagina; herstel pad en controleer behoud van varianten. Controleer ontbrekend bestand en een klein RTF-bestand. Een installer wordt alleen door de gebruiker gestart.

Verificatie: Release-build zonder fouten of waarschuwingen. Volledige regressierun: 199 tests geslaagd. Daarna een aanvullende verwijder-/uitprobeertest en expliciete waarschuwing voor verwijderde documentvarianten; de gerichte run telt 24 geslaagde tests. Een echte ISCC 7.1.0-proef compileert de drie documenten met een Nederlandse RTF-licentievariant. Documentbytes blijven ongewijzigd. Na de laatste UI- en uitlegwijzigingen slaagt de Release-build zonder fouten of waarschuwingen; alle 24 gerichte tests slagen opnieuw.

Formaat 7 voegt Documents toe; formaten 1–6 krijgen lege documentinstellingen bij migratie, met behoud van bestaande identiteit, teksten en talen. Opslaan als herbaseert algemene documentpaden en taalvarianten vanuit de oorspronkelijke projectmap. Uitgeschakelde pagina’s en talen bewaren varianten; alleen actieve bronnen worden geëxporteerd. Bij verwijderen van een taal vraagt Studio bevestiging voor de documentvarianten naast de eigen teksten.

Het documentvoorbeeld gebruikt een eigen taalkeuze die bij openen de huidige wizardpreviewtaal overneemt. Het wijzigen daarvan wijzigt de algemene previewtaal niet. Klik Documentvoorbeeld tonen na bron- of taalwijzigingen. Bronnen van maximaal 4 MB worden als voorbeeld geladen; grotere documenten blijven exporteerbaar. RTF-opmaak en de uiteindelijke native wizard worden handmatig beoordeeld.

Gebruikersacceptatie: de gebruiker heeft de gecompileerde installer getest. De Engelse licentievariant wordt bij Engels getoond; de overige talen gebruiken het algemene document. De tijdelijk gemelde onvolledige taallijst is niet opnieuw reproduceerbaar: alle projecttalen verschijnen, ook in een nieuw project. Een controle van het geïnitialiseerde documentvenster zonder GUI te openen bevestigde drie taalopties bij een Duitse beginselectie. Hiervoor is geen codewijziging gedaan. De gebruiker heeft vervolgwerk en afronding van deze slice geaccordeerd.
