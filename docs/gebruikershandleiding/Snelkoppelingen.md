# Snelkoppelingen maken

Een snelkoppeling geeft de gebruiker een handige plek om je programma te starten, bijvoorbeeld in het Startmenu of op het Bureaublad. Je kunt ook een snelkoppeling naar een handleiding maken. Snelkoppelingen zijn niet verplicht: voeg alleen toe wat voor jouw programma nuttig is.

Een snelkoppeling installeert het doelbestand niet zelf. Stel daarom eerst bij **Bestandsregels** in welke bestanden je installer moet meenemen. Daarna kun je die bestanden als doel kiezen. Er hoeft op je ontwikkelcomputer nog niets geïnstalleerd te zijn.

## Voorbeeld: je programma in het Startmenu

Stel dat je programma `SVGViewer.exe` heet.

1. Zorg dat je bestandsregels `SVGViewer.exe` meenemen.
2. Open **Snelkoppelingen beheren** en klik op **Toevoegen**.
3. Geef de snelkoppeling een herkenbare naam, bijvoorbeeld `SVG Viewer`.
4. Kies **Startmenu** als plaatsing.
5. Kies het doelbestand uit de lijst, bijvoorbeeld `{app}\SVGViewer.exe`.
6. Open **Startmenu instellen** om de naam van de startmenumap in te stellen, bijvoorbeeld `SVG Viewer`.
7. Bouw je installer en controleer na installatie of de snelkoppeling je programma opent.

`{app}` betekent hier de map waarin de gebruiker je programma laat installeren. Het is geen map die je nu op je computer moet opzoeken. Je hoeft deze verwijzing niet zelf te typen als je een bestand uit de lijst kiest. `{AppName}` is een verwijzing naar de applicatienaam en is geen vervanging voor het doelbestand.

Is de lijst leeg of ontbreekt je bestand? Controleer de bron en eventuele uitsluitingen bij **Bestandsregels**. Heb je bestanden buiten Studio toegevoegd, klik dan op **Vernieuwen** om de lijst opnieuw in te lezen.

## Voorbeeld: een handleiding openen

Neem bijvoorbeeld `Handleiding.pdf` op in je bestandsregels en voeg een snelkoppeling met de naam `Handleiding` naar dat bestand toe. Windows opent de PDF met de toepassing die op de computer van de gebruiker aan PDF-bestanden is gekoppeld. Voor andere documenttypen, zoals RTF, geldt hetzelfde. De installer levert daarmee niet automatisch een programma om het document te lezen.

## Wanneer heb je Extra opties nodig?

Voor een gewone snelkoppeling kun je **Extra opties** ingeklapt laten. De velden zijn optioneel.

- **Submap**: plaats de snelkoppeling in een extra map binnen de gekozen locatie.
- **Parameters**: geef opstartargumenten mee, alleen als je programma die ondersteunt. Laat dit leeg als je programma gewoon moet starten.
- **Werkmap**: stel de map in van waaruit het programma wordt gestart. Meestal kun je dit leeg laten; normaal gebruikt Inno Setup dan de map van het doelbestand.
- **Doelpad handmatig aanpassen**: gebruik dit alleen als je bewust het toekomstige installatiepad wilt invoeren. Het moet blijven verwijzen naar een bestand uit de bestandsregels. De keuzelijst is voor normaal gebruik voldoende.

## Startmenu en Bureaublad zijn afzonderlijke keuzes

**Startmenu instellen** regelt de startmenumap. De instelling waarmee de gebruiker die map mag overslaan geldt niet voor bureaubladsnelkoppelingen.

In de huidige versie wordt een ingestelde bureaubladsnelkoppeling altijd aangemaakt. Een keuze waarmee de gebruiker deze tijdens installatie kan aan- of uitzetten volgt later via de takenpagina. Hiervoor hoef je nu geen startmenu-instelling te zoeken.
