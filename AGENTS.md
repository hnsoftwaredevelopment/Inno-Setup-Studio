# Werkinstructies — Inno Setup Studio

## Project en bronnen

Projectroot: C:\Devops\hnsoftwaredevelopment\Inno Setup Studio.
Product: afzonderlijke add-on voor Inno Setup 7.1.
Het oude InnoSetupStudio is een ander project.

Lees bij werkzaamheden de relevante delen van [Productspecificatie](docs/Productspecificatie.md), [ondersteuningsmatrix](docs/InnoSetup-ondersteuning.md), [Milestones](docs/Milestones.md), [Werkwijze](docs/Werkwijze.md) en de actieve slice in [todo](tasks/todo.md).

Een conceptplanning is geen toestemming om alle functies te bouwen. De gebruiker kiest de volgende milestone/slice.

## Productgrens

Inno Setup is the limit. Gebruik reguliere instellingen en gedocumenteerde native API's voor de doelversie. Bied geen onbewezen previeweigenschappen of kunstgrepen aan om Inno Setup-beperkingen te omzeilen.

Behoud de huidige werkplek als basis. Studio-taal, installertalen, previewtaal en tijdelijke uitprobeerwaarden blijven afzonderlijk. Bestaande projectgegevens worden behouden met gecontroleerde migraties.

## Acceptatie en Git

- Gebruik een branch met exact de technische milestonenaam, bijvoorbeeld m02-installertalen.
- Werk één slice af en rapporteer wijzigingen, build-/testresultaat en concrete testpunten.
- Laat visuele kliktests en echte installatieproeven aan de gebruiker over, tenzij expliciet anders gevraagd.
- Commit en push een slice pas na expliciet gebruikersakkoord op de beoordeelde stand.
- Merge en push main pas als alle slices zijn geaccepteerd/gepusht en de gebruiker de milestone als afgerond accepteert.
- Neem geen andere wijzigingen of bestaande lokale commits ongemerkt mee.
- Geen force-push, automatische reset/rebase of verlies van gebruikerswerk.
- Bestaande gebruikersinstructies en later gegeven correcties hebben voorrang.

## Documentatie spiegelen

Bij elk nieuw of gewijzigd Markdown-bestand kopieer de bijgewerkte versie naar:

C:\Devops\Obsidian\markdown\Development\HNSoftwareDevelopment\Inno Setup Studio

Behoud de relatieve paden. Controleer de kopie; meld expliciet als de doelmap onbereikbaar is of kopiëren niet lukt.

Markdown in de repository is de bron. Applicatiecode en buildoutput hoeven niet naar Obsidian.

## Verificatie

Bij code: voer een Release-build en relevante automatische tests uit. Bij uitsluitend documentatie: controleer inhoud, lokale links, diff en spiegeling; een applicatiebuild is dan niet nodig.

Controleer vóór nieuwe branches, commits, pushes en merges de actuele Git-status en de daadwerkelijke scope van de handeling. De remote heet origin; het bekende adres is https://github.com/hnsoftwaredevelopment/Inno-Setup-Studio.git, maar verifieer dit vóór publiceren.
