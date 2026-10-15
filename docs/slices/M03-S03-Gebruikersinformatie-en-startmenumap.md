# M03-S03 — Gebruikersinformatie en startmenumap

Status: door de gebruiker getest en geaccepteerd op m03-wizardpaginas.

Resultaat: vanuit het paginaoverzicht Gebruikersinformatie en Startmenumap instellen. Gebruikersinformatie: pagina aan/uit, standaardnaam en organisatie en vorige gegevens gebruiken. Startmenumap: automatisch/tonen/verbergen, standaardnaam, keuze om geen map te maken en vorige keuze gebruiken. Alleen native richtlijnen; serienummer vereist CheckSerial en hoort bij latere scripting.

Criteria: instellingen worden opgeslagen en geëxporteerd met gecontroleerde migratie; preview-invoer in het instelvenster is tijdelijk; de paginastatus volgt de beschikbare inhoud. Zolang er geen Icons-regels zijn, verschijnt Startmenumap niet in de installer, ook niet bij Tonen. Snelkoppelingen volgen in M04; geen kunstmatige Icons-regel in het gebruikersproject. Voorwaarden en grenzen zijn zichtbaar in de UI.

Afbakening: nog geen volledige Uitproberen-route voor deze pagina’s. Het invoervoorbeeld toont de relevante velden en bewaart geen proefinvoer; native constanten blijven zichtbaar als expressies. Native gebruikersgegevens en updategedrag worden door Inno Setup bepaald. Algemene instellingen, geen eigen titelvertalingen in deze slice.

Bronnen: [UserInfoPage](https://jrsoftware.org/ishelp/topic_setup_userinfopage.htm), [DisableProgramGroupPage](https://jrsoftware.org/ishelp/topic_setup_disableprogramgrouppage.htm), [AllowNoIcons](https://jrsoftware.org/ishelp/topic_setup_allownoicons.htm).

Handmatig: stel naam/organisatie in, sluit/open en controleer opslag. Wijzig alleen proefinvoer en controleer dat de projectstatus niet verandert. Bouw en draai de installer: gebruikersinformatie verschijnt en de echte invoer is bewerkbaar. Schakel uit: de pagina verdwijnt en instellingen blijven bewaard. Stel startmenumapbeleid in, sla op/open en controleer export; de pagina blijft zonder snelkoppelingen afwezig. Controleer Nederlands/Engels/Duits en een kleiner venster.

Genoteerd voor latere wizardbediening: Licentie en andere configuratiepagina’s moeten ook duidelijk via het gewone hoofdscherm bereikbaar worden. Nu is toegang via het paginaoverzicht. Dit verandert de scope van S03 niet; meenemen bij de verdere navigatie-/wizardproef.

Verificatie: Release-build zonder fouten of waarschuwingen. De volledige regressierun slaagt met 202 tests. Daarna zijn opslag/migratie, ongeldige invoer, drie Studio-talen en drie native compilerproeven toegevoegd; de laatste gerichte run slaagt met 25 tests. ISCC 7.1.0 accepteert gebruikersinformatie en automatisch/tonen/verbergen voor de startmenumap. Een snelkoppeling wordt uitsluitend in de compilerproef toegevoegd om de native mapkeuze toepasbaar te maken; het gebruikersproject krijgt geen snelkoppelingen. Geen GUI of installer is automatisch gestart.

Formaat 9 bewaart PageSettings en een afzonderlijke ingeschakelde status per documentpagina. Formaten 1–8 krijgen gecontroleerde migratie; bestaande documenten, talen, teksten en identiteit blijven behouden. Gebruikersinformatie blijft standaard uit. Startmenubeleid begint Automatisch, zonder expliciete mapnaam; de native standaard blijft gelden. Beginvelden mogen native constanten bevatten. Studio-verwijzingen {AppName}/{AppVersion} worden voor export vervangen door letterlijke applicatiegegevens, zonder opnieuw als verwijzing te worden geïnterpreteerd.

Bediening: schakel Gebruikersinformatie in via de schakelaar in Paginaoverzicht; het instelvenster opent direct. Voor Startmenumap blijft Pagina instellen beschikbaar, omdat de zichtbaarheid nog afhankelijk is van toekomstige snelkoppelingen. Proefvelden onder Invoervoorbeeld zijn alleen tijdelijk. De proefvelden gebruiken de Studio-taal; zij zijn geen exacte afbeelding van de native wizard. De volledige wizardnavigatie volgt in de verdere M03-slices. Bij een kleinere venstergrootte kan het instelvenster scrollen.

Commit/push pas na expliciete gebruikersacceptatie. Documentatie is gespiegeld naar Obsidian en gecontroleerd.

Na gebruikersfeedback zijn de schakelaars bediening geworden voor Licentie, Informatie vóór/na installatie en Gebruikersinformatie. Inschakelen opent het bijbehorende instelvenster; uitschakelen bewaart alle gegevens. Een documentpagina zonder algemeen document blijft niet-actief wanneer het venster wordt gesloten. Dubbelklik op een ingeschakelde paginanaam of F2 opent bestaande instellingen zonder de pagina uit en aan te hoeven zetten. De extra instellingenknop vervalt voor deze vier pagina’s. Installeren blijft vast aan; overige nog niet instelbare statussen zijn alleen-lezen. De lijsthoogte en scrollpositie blijven behouden bij statusverversing.

Aanvullend testen: schakel de licentie aan en kies een bron; schakel uit, sla op en heropen; schakel opnieuw aan en controleer behoud van algemene bron en taalvarianten. Controleer hetzelfde voor gebruikersinformatie. Controleer dubbelklik en F2, en dat Installeren niet uitgeschakeld kan worden. Sluit een nieuwe documentinstelling zonder een bron te kiezen: de pagina blijft uit.

Het serienummerveld is op gebruikersverzoek geregistreerd voor de scriptingmilestone. Studio moet daarbij de benodigde CheckSerial-gebeurtenis kunnen genereren op basis van expliciet gekozen validatiegedrag; er wordt nu geen generieke controle toegevoegd die ieder serienummer accepteert. De gebruiker heeft het overige testresultaat en daarna de aangepaste schakelaars bevestigd en M03-S03 geaccepteerd.

Na de schakelaarwijzigingen: Release-build geslaagd zonder fouten of waarschuwingen; alle 211 tests geslaagd, inclusief behoud van documentbronnen bij uit/aan en migratie van formaat 8. De gebruiker heeft bevestigd dat de schakelaars en popupbediening werken zoals verwacht.

Gebruikersvraag: {sysuserinfoname} en {sysuserinfoorg} lezen de geregistreerde eigenaar en organisatie uit Windows op de doelcomputer, via HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion, RegisteredOwner en RegisteredOrganization. Dit zijn niet per definitie de ingelogde accountgegevens. In Studio kunnen deze native constanten al worden vervangen door vaste beginwaarden; de gebruiker van de installer kan ze vervolgens aanpassen. [Native constanten](https://jrsoftware.org/ishelp/topic_consts.htm).
