# M03-S06 — Wizardproef

Status: wizardproef door de gebruiker geaccepteerd op m03-wizardpaginas. M03 is nog niet gemerged.

## Werking en grenzen

**Uitproberen** opent één afzonderlijk wizardvenster vanaf Welkom. De route volgt de actieve pagina’s uit Paginaoverzicht: Welkom, eventueel Licentie/Informatie vóór/Gebruikersinformatie, Installatiemap, Gereed, Installeren, eventueel Informatie na en Voltooid. Wachtwoord, Componenten, Startmenumap en Taken worden overgeslagen zolang de huidige export daarvoor geen actieve gegevens bevat. De voorwaardelijke voorbereidingsfase verschijnt alleen bij het gekozen proefprobleem.

Het afzonderlijke venster geeft de samengestelde wizard ruimte en scheidt tijdelijke invoer van ontwerpeigenschappen. Het hergebruikt de bestaande afsluitsimulatie en tekstresolutie. Paginaoverzicht blijft de bron voor volgorde en beschikbaarheid; opgeslagen ontwerppagina’s en projectformaat 9 blijven gelijk.

De lokale taalkeuze verandert de algemene voorbeeldtaal niet. Welkom en Installatiemap tonen hun pagina-afwijkingen. Gewone overige pagina’s gebruiken gedeelde knopteksten; Installeren en Voltooien behouden native rolteksten. Native paginaberichten worden alleen-lezen uit de taalbron geladen; ontbrekende berichten krijgen zichtbare Engelse terugval. RTL/vormgeving blijven benaderingen.

Documenten worden automatisch gelezen met dezelfde taalvariant/algemene terugval als de export. TXT en RTF worden ondersteund tot 4 MiB. Een ontbrekend, onleesbaar of te groot document krijgt een melding en blokkeert Verder op die documentpagina. Documentlinks zijn niet actief. Licentieacceptatie is vereist en wordt bij een taalwissel gewist; terugnavigeren binnen dezelfde taal behoudt de keuze.

Naam, organisatie en installatiemap zijn tijdelijke proefwaarden. De samenvatting toont die waarden. Studio-verwijzingen zoals {AppName} worden ingevuld; Windows- en Inno Setup-constanten blijven als expressies zichtbaar. Windows-gegevens, eerdere installatiegegevens en native pad-/schijfruimtecontroles worden niet uitgevoerd.

Installeren heeft handmatig ingestelde voortgang. Bij 100% wordt **Installatie afronden** beschikbaar; daarna volgt Informatie na, indien actief, en Voltooid. Het voorbereidingsprobleem moet expliciet worden opgelost of via Terug worden verlaten. Er worden geen bestanden geïnstalleerd, scripts uitgevoerd of herstartacties gestart. Na installatie kan niet naar de invoerpagina’s worden teruggegaan.

Sluiten, Annuleren en Voltooien herstellen de eerdere ontwerpselectie. Tijdelijke waarden verdwijnen; eerdere onopgeslagen wijzigingen blijven bestaan. Projectinhoud, taalbronnen en export worden niet veranderd.

## Verificatie

Release-build: 0 fouten, 0 waarschuwingen. Alle 234 automatische tests geslaagd, zonder overgeslagen tests, inclusief bestaande echte ISCC-compilatieproeven. Vier nieuwe routeproeven controleren actieve/uitgeschakelde pagina’s, acceptatie, taalgebonden bronnen en captions, voorbereiding, voortgang, samenvatting, herstel en behoud van project/export/taalbronbytes. Het WPF-venster is zonder tonen aangemaakt en gesloten; selectieherstel is gecontroleerd. Dit is geen visuele kliktest.

Native volgorde en knopzichtbaarheid zijn vergeleken met de aangeleverde Inno Setup-bron, Setup.WizardForm.pas (RegisterExistingPage, GetPreviousPageID, UpdateCurPageButtonState). De installer is in deze slice niet gestart. Gebruikerscontrole levert bewijs voor uiterlijk en werkelijk installatiegedrag.

## Handmatige controles

1. Open een bestaand project en selecteer een ontwerppagina en element. Kies Uitproberen: de proef begint bij Welkom. Doorloop een minimale route tot Voltooid; de ontwerpselectie komt na sluiten terug.
2. Activeer Licentie, beide informatiepagina’s en Gebruikersinformatie. Controleer volgorde, teksten en documenten. Verder is op Licentie pas mogelijk na acceptatie. Schakel een documentpagina uit en controleer dat die wordt overgeslagen.
3. Geef documenten taalvarianten en knoppen gedeelde/lokale teksten. Wissel in de proef van taal. Controleer bronnen en knopbereik; licentieacceptatie moet opnieuw worden gegeven. De algemene voorbeeldtaal blijft na sluiten gelijk.
4. Wijzig proefnaam, organisatie en map; navigeer terug/vooruit en controleer behoud binnen de proef. Gereed toont de proefwaarden. Sluiten bewaart deze waarden niet en wist geen eerdere onopgeslagen ontwerpwijzigingen.
5. Kies op Gereed het voorbereidingsprobleem. Controleer blokkering, Terug en Oplossen. Tijdens Installeren verschijnt geen Verder-knop. Zet voortgang op 100% en kies Installatie afronden: Informatie na komt vóór Voltooid. De laatste knop gebruikt de native Finish-tekst.
6. Probeer een ontbrekend document; herstel het bronpad in het ontwerp en start opnieuw. De proef toont een melding in plaats van stil door te gaan. Controleer ook een RTF-document.
7. Bouw en draai het project met meerdere talen en pagina’s aan/uit. Vergelijk de native pagina’s, documenten en knopteksten met de proef. Constante-expansie en echte voortgang worden uitsluitend door de installer uitgevoerd.

Na acceptatie wordt S06 gecommit/gepusht. M03 wordt pas na expliciet akkoord op de gehele milestone gemerged; M04 wordt niet automatisch gestart.

Gebruikersacceptatie: vooruit/achteruit en verschillende talen werken. Native constanten blijven terecht zichtbaar in de simulatie. Aanvullend gevraagd: documentfouten automatisch en rood boven het documentvoorbeeld melden. Die aanvulling krijgt een afzonderlijke controle.

Milestoneafronding: M03 is na expliciet gebruikersakkoord op 15 oktober 2026 naar main gemerged en gepusht als 84c8a06.
