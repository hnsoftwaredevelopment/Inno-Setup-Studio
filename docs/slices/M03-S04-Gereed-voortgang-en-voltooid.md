# M03-S04 — Gereed, voortgang en voltooid

Status: door de gebruiker getest en geaccepteerd op m03-wizardpaginas.

Resultaat: via het paginaoverzicht de vier afsluitfasen bekijken in een korte, afzonderlijke simulatie. Gereed toont een samenvatting en de native Installeren-rol. Voorbereiden is voorwaardelijk; een simulatieprobleem kan worden getoond en opgelost. Installeren toont handmatig instelbare voortgang zonder installatieacties. Voltooid gebruikt de native Voltooien-rol.

Criteria: native taalteksten en juiste knoprollen; gedeelde Verder-tekst lekt niet naar Installeren/Voltooien. Simuleren, lokale taalkeuze en sluiten veranderen geen project, algemene previewtaal, ontwerpselectie of wijzigingsstatus. Native bronnen blijven ongewijzigd. Langere teksten kunnen scrollen. Alleen ondersteunde preview; geen echte installatie, programma-acties of herstart.

Afbakening: zichtbaarheid volgt de huidige native standaardinstellingen. Geen nieuwe tekstafwijkingen of projectinstellingen; de afsluitsimulatie wijzigt het projectformaat en de exportinhoud niet. De complete route inclusief documenten/gebruikersinformatie volgt in S06. De korte simulatie bevat alleen de vier afsluitfasen. Constanten blijven expressies; de echte installer bepaalt hun waarden. Ontbrekende taalbronnen gebruiken een duidelijk aangegeven Engels voorbeeld.

Handmatig: selecteer iedere afsluitfase in Paginaoverzicht en open Fase bekijken. Bekijk Gereed en de samenvatting. Stel eventueel een voorbereidingsprobleem in en klik Installeren; los het simulatieprobleem op om verder te gaan. Wijzig de voortgang; tijdens Installeren is geen Verder-/Installeren-knop aanwezig. Rond de simulatie bij 100% af en klik Voltooien. Herhaal in verschillende installertalen, met een afwijkende gedeelde Verder-tekst en een kleinere venstergrootte. Na sluiten blijven projectgegevens, taalkeuze, selectie en wijzigingsstatus gelijk. De gebruiker kan de bestaande export bouwen om de native rollen te vergelijken.

Bron: [Wizardpagina’s](https://jrsoftware.org/ishelp/topic_wizardpages.htm). Native knoprollen zijn daarnaast gecontroleerd in de lokale read-only Inno Setup-bron, Setup.WizardForm.pas.

Implementatie: Fase bekijken is beschikbaar voor Gereed, Voorbereiden, Installeren en Voltooid. De lokale taalkeuze begint met de huidige previewtaal maar wijzigt die niet. De native taalbron wordt alleen-lezen ingeladen; ontbrekende bronnen/teksten gebruiken een zichtbaar gemeld Engels voorbeeld. Bestaande projectteksten voor Terug en Annuleren blijven geldig; Installeren en Voltooien gebruiken hun native roltekst. De bestaande RTL-previewbeperking blijft gemeld.

Voorbereiden wordt normaal overgeslagen bij klikken op Installeren; met Een voorbereidingsprobleem simuleren blijft die fase staan tot het probleem expliciet wordt opgelost. De voortgang is handmatig instelbaar. Bij 100% wordt Simulatie bij 100% afronden beschikbaar; de native Verder-/Installeren-knop is tijdens Installeren verborgen. Voltooid heeft alleen de Voltooien-rol; de korte simulatie kan altijd via Simulatie sluiten worden beëindigd.

De samenvatting toont het standaard installatiepad en, indien ingeschakeld, de beginwaarden van gebruikersinformatie. Er zijn nog geen componenten, taken of snelkoppelingen in het projectmodel; die worden niet verzonnen. De echte samenvatting kan afwijken door native constanten, eerder opgeslagen gebruikersgegevens en latere keuzegegevens. Echte installatie, documentpagina’s na installatie, herstart en Run-acties horen niet bij deze korte simulatie.

Verificatie: Release-build geslaagd met 0 fouten en 0 waarschuwingen. Vijf gerichte tests controleren knoprollen, voortgang, een voorbereidingsprobleem, Duitse native teksten, eigen onvolledige bronnen, Engelse terugval en behoud van project/export/bronbytes. De volledige regressierun wordt hieronder geregistreerd. Geen installer of GUI is automatisch gestart. Het gebruikersakkoord voor commit/push is ontvangen.

Volledige regressiecontrole: alle 216 tests geslaagd, inclusief bestaande ISCC-compilatieproeven. De build heeft 0 fouten en 0 waarschuwingen. De gebruiker heeft deze slice geaccepteerd na controle van de native Voltooid/Finish-tekst en exportvervanging.

Aanvullende gebruikerscorrectie — bestaand ISS vervangen: de gebruiker bedoelt Exporteer .iss. Studio vraagt bij een bestaand doelbestand expliciet of de huidige export de oude inhoud mag vervangen, inclusief handmatige wijzigingen. Nee laat het bestand intact. De bevestiging geldt voor de inhoud die vóór de vraag aanwezig was; een tussentijdse wijziging of verwijdering geeft een melding en vereist opnieuw exporteren. De bron van de bestandsregel blijft beschermd. Export naar een nieuwe naam blijft zonder vervangingsvraag werken. Project-Opslaan als heeft de reguliere bevestiging voor het vervangen van een .issstudio-bestand; deze route is eveneens getest.

Handmatig aanvullend: exporteer naar een bestaand afwijkend .iss-bestand. Kies eerst Nee en controleer behoud; herhaal met Ja en controleer de nieuwe projectstand in het bestand. Bouw de export met Inno Setup. Gewone projectopslag en Opslaan als blijven .issstudio-opslag; .iss-scripts worden via Exporteer .iss geschreven. De normale Uitproberen-route bevat momenteel alleen Welkom en Installatiemap; de volledige route blijft S06.

Gebruikersbewijs: de installer is gebouwd en uitgevoerd met beschikbare pagina’s ingeschakeld en met een aantal uitgeschakeld; werking en projectopslag zijn bevestigd. De gebruiker heeft de Voltooid/Finish-knop gecontroleerd bij een afwijkende gedeelde Volgende-tekst en bevestigd dat Ja bestaande exports vervangt en Nee ze behoudt. M03-S04 is geaccepteerd.

Na de exportcorrectie: Release-build zonder fouten/waarschuwingen; alle 221 tests geslaagd. Controles omvatten bevestigde vervanging, behoud zonder toestemming, wijziging/verwijdering na bevestiging, bronbescherming en vervangen van een bestaand alternatief .issstudio-project.
