# M04-S06 — Taken

Status: op 11 november 2026 volledig geaccepteerd op m04-installatiekeuzes, inclusief het compacte hoofdmenu. S05 is geaccepteerd en gepusht als 471e026.

Met **Installatie → Taken** voeg je zelfstandige selectievakjes toe, verplaats je ze en stel je standaardomschrijvingen en vertalingen in. Namen zijn uniek zonder onderscheid tussen hoofdletters en kleine letters. Beginselectie wordt vertaald naar de native standaard, unchecked of checkedonce. Herstartverzoek exporteert de native restart-vlag. De export gebruikt [Tasks en taalgebonden CustomMessages](https://jrsoftware.org/ishelp/topic_taskssection.htm). Projectformaat 15 bewaart taken; oudere formaten krijgen een lege takenlijst.

Paginaoverzicht en Uitproberen tonen Taken alleen bij een niet-lege takenlijst. Uitproberen toont een eerste installatie, behoudt tijdelijke keuzes bij navigatie en taalwisseling en toont gekozen taken in de samenvatting. Het voert geen installatie of herstart uit. Voorwaardelijke koppelingen met installatieregels volgen in S07. Subtaken, exclusieve groepen en aanvullende native opties vallen buiten deze slice.

## Handmatig controleren

1. Voeg een taak toe die standaard uit staat, en een die alleen bij eerste installatie aan staat. Geef beide een unieke naam en voeg vertalingen toe. Test volgorde, bewaren en heropenen.
2. Controleer de takenpagina in Uitproberen. Wijzig vinkjes, wissel van taal en navigeer terug en vooruit. Controleer de samenvatting; heropen de proef en kijk of de beginselectie terug is.
3. Exporteer en compileer. Controleer de takenpagina in de gebouwde installer. De keuze bepaalt nog geen bestanden of snelkoppelingen. Test het herstartverzoek alleen als je dat expliciet wilt.
4. Verwijder alle taken. De takenpagina verdwijnt uit de route en het script heeft geen [Tasks]-sectie meer.

Zie ook [de handleiding](../gebruikershandleiding/Taken.md).

Release-build: 0 fouten en 0 waarschuwingen. Alle 290 tests geslaagd, inclusief vijf nieuwe taaktests en compilatie van de meertalige export met ISCC. Geen installer gestart.

## Compact hoofdvenster

Na gebruikersfeedback zijn de gestapelde panelen vervangen door Project, Installatie en Bouwen. Open Taken via Installatie. Controleer op een klein venster ook opslaan, applicatiegegevens, uitvoernaam en bouwen. De testbuild slaagde zonder fouten of waarschuwingen. Bij een beschikbare inhoudsmaat van 934 × 480 DIP meet de kop 74 DIP en de editor 370 DIP in NL/EN/DE; de Opslaan-opdracht blijft bereikbaar. Er zijn geen visuele kliktests uitgevoerd.
