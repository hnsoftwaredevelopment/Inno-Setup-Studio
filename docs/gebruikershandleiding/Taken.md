# Taken

Een taak is een extra keuze tijdens de installatie, bijvoorbeeld **Snelkoppeling op het bureaublad maken**. Voeg een taak toe via **Installatie → Taken**. Gebruik een herkenbare standaardomschrijving en een unieke interne naam, bijvoorbeeld `desktopicon`.

Kies of de taak aanvankelijk is aangevinkt, niet is aangevinkt of alleen bij een eerste installatie is aangevinkt. Bij die laatste optie laat Inno Setup het vakje leeg wanneer het een eerdere versie van dezelfde applicatie vindt. Inno Setup kan eerdere keuzes onthouden; **Uitproberen** toont het scenario zonder eerdere installatie.

Onder **Vertalingen** kies je iedere installertaal afzonderlijk. Een leeg veld gebruikt de standaardomschrijving. Met **Omhoog** en **Omlaag** bepaal je de volgorde op de takenpagina.

Zodra het project een taak bevat, verschijnt de takenpagina in **Uitproberen** en in de gebouwde installer. Je vinkjes in Uitproberen worden niet opgeslagen. De gekozen taken staan op de samenvatting voor installatie.

De optie om opnieuw op te starten wordt door de echte installer verwerkt als de taak is gekozen. Uitproberen voert geen taken of herstart uit. De koppeling tussen bijvoorbeeld `desktopicon` en de bureaubladsnelkoppeling volgt in S07. Tot die tijd bepaalt het vinkje nog niet of die snelkoppeling wordt gemaakt.
