# M03-S05 — Basisontwerp en pagina-afwijkingen

Status: door de gebruiker geaccepteerd op m03-wizardpaginas.

Resultaat: het eigenschappenpaneel vermeldt het werkelijke bereik van de geselecteerde instelling. Gedeelde Verder-tekst werkt op gewone native invoerpagina’s; Installeren, Voltooien en runtimevoorbereiding behouden native rollen. Lokale knopteksten van Welkom en Installatiemap blijven alleen op hun pagina en in hun taal geldig. Bij paginawisseling worden gedeelde waarden eerst opnieuw toegepast en daarna de lokale waarden.

Criteria: taal/rol/pagina zijn gescheiden; herstellen verwijdert alleen de gekozen afwijking; terug/vooruit lekt geen lokale tekst. Een expliciete whitelist bepaalt welke bestaande eigenschappen bewerkbaar zijn. Geen nieuwe stijl-, kleur-, positie- of onbewezen properties. Projectformaat en terugvalregels blijven gelijk.

Bereik: Terug en Annuleren gelden waar hun knop zichtbaar is. Verder geldt voor Welkom, Licentie, Wachtwoord, Informatie vóór, Gebruikersinformatie, Installatiemap, Componenten, Startmenumap, Taken en Informatie na. Voorbereiden, Installeren, Gereed en Voltooid vallen buiten deze Verder-afwijking. Lokale afwijkingen zijn in deze slice alleen beschikbaar voor de twee bestaande ontwerppagina’s.

Bron: [CurPageChanged en native pagina-ID’s](https://jrsoftware.org/ishelp/topic_scriptevents.htm). Native controles blijven intact; er worden alleen captions gezet.

Handmatig: stel per installertaal gedeelde Terug/Verder/Annuleren in. Geef Welkom en Installatiemap verschillende lokale teksten, navigeer terug/vooruit en herstel één afwijking. Controleer dat de gedeelde tekst terugkomt, andere talen/velden gelijk blijven en de bereik-uitleg klopt. Bouw een project met Licentie, Informatie en Gebruikersinformatie: die gewone pagina’s gebruiken de gedeelde Verder-tekst; Gereed blijft Installeren en Voltooid blijft Voltooien. Controleer lokale afwijkingen in de echte installer bij terugnavigeren. Onbewezen kleur-/positie-eigenschappen blijven afwezig.

Afbakening: geen nieuwe vrije ontwerppagina’s of volledige uitprobeerroute. Die volgt in S06. Vormgeving blijft voor M06 en eigen scripting voor M08.

Verificatie: Release-build geslaagd met 0 fouten en 0 waarschuwingen; alle 230 automatische tests geslaagd, zonder overgeslagen tests. De nieuwe ISCC-proef compileert gedeelde en lokale knopteksten in twee talen met alle drie documentsoorten en Gebruikersinformatie. De tests controleren de expliciete native pagina-ID’s, lokale voorrang en herstel per taal/pagina. De installer is niet gestart; visuele en runtimecontrole worden door de gebruiker uitgevoerd.

Gebruikersacceptatie: de teksten verschijnen op de verwachte pagina’s. De gebouwde installer is getest in diverse talen, met en zonder aangepaste knoppen; deze slice is akkoord.
