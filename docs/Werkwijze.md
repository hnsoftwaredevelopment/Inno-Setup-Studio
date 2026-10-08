# Werkwijze — milestones, slices en acceptatie

Status: afgesproken proces, uitgewerkt op 8 oktober 2026. Geldt voor documentatie, code en vervolgsessies. De gebruiker bepaalt inhoudelijke acceptatie.

## 1. Milestone kiezen

Een milestone levert een herkenbaar resultaat. De [milestoneplanning](Milestones.md) beschrijft eindcriteria en afhankelijkheden. We pakken een milestone op na opdracht van de gebruiker en werken niet automatisch de hele roadmap af.

De technische naam van de milestone is ook de branchnaam, bijvoorbeeld m02-installertalen. De actuele documentatiemilestone heet m00-productspecificatie. Aan het begin controleren we werkmap, Git-status, doelbranch, remote en bestaande wijzigingen.

Een nieuwe milestone begint vanaf de gecontroleerde main-stand na de vorige merge. Bestaande gebruikerswijzigingen worden behouden. Geen automatische reset, stash, rebase of geschiedeniswijziging om een werkmap schoon te krijgen.

## 2. Slice concretiseren

Iedere slice krijgt vóór bouwen:

- Een ID, beoogd gebruikersresultaat en maximaal enkele toetsbare acceptatiecriteria.
- Afhankelijkheden, relevante FR's, waarschijnlijke bestandsgroepen en afbakening.
- Gerichte automatische controles en concrete handmatige testpunten.
- Een bekende beperking of onderzoeksvraag als iets nog niet bewezen is.

De verdere roadmap is een voorstel. Bij het oppakken wordt een slice zo nodig kleiner gemaakt; één slice mag geen verzameling onafhankelijke functies worden. Richting: één gerichte werksessie en ongeveer twee tot vijf bron-/testbestanden. Resource- en documentatiebestanden tellen mee voor reviewbaarheid, maar maken een kleine inhoudelijke wijziging niet automatisch te groot.

Een afgeronde slice laat de applicatie bruikbaar. Als een technische voorbereiding nodig is, moet het resultaat afzonderlijk controleerbaar zijn en wordt dat expliciet als voorbereiding beschreven.

## 3. Bouwen en controleren

De agent implementeert de afgesproken slice, werkt relevante documentatie bij en voert de build uit. Relevante automatische tests controleren de gewijzigde logica. Een documentatieslice krijgt documentcontrole; een applicatiebuild is daarvoor niet nodig.

Visueel doorklikken en echte installatieproeven doet de gebruiker. De agent voert geen automatische GUI-tests met muis/keyboard uit tenzij de gebruiker daar specifiek om vraagt. De WPF-uitprobeermodus voert geen installatieacties uit. Een compilatie mag waar nodig wel een installerbestand produceren; starten van die installer is een afzonderlijke gebruikersactie.

Standaardopdrachten vanuit de projectroot:

~~~powershell
dotnet build '.\Inno Setup Studio.slnx' -c Release
dotnet test '.\Inno Setup Studio.slnx' -c Release
~~~

Tests worden gericht gekozen en niet zonder reden op ongewijzigde code herhaald. Bij een fout wordt de oorzaak opgelost en de relevante controle opnieuw gedaan.

## 4. Opleverbericht

Bij iedere slice ontvangt de gebruiker:

| Onderdeel | Inhoud |
|---|---|
| Wijziging | Wat is anders en welk gebruikersresultaat levert dat op? |
| Controle | Build-/testresultaat of documentcontrole, met werkelijke uitvoerstatus |
| Testen door gebruiker | Korte stappen, beginwaarden en verwachte uitkomst |
| Beperkingen | Alleen relevante ontbrekende of niet bewezen onderdelen |
| Git-status | Branch en welke wijzigingen nog op akkoord wachten |

Een eerdere geslaagde test wordt niet als nieuwe controle gepresenteerd. Een previewcontrole wordt niet als bewijs van echte installatie gepresenteerd.

## 5. Akkoord, commit en push

Zolang een slice niet is geaccepteerd, blijven de wijzigingen ongecommit. Opmerkingen worden in dezelfde slice verwerkt; na aanpassing komen nieuwe testpunten als dat nodig is. Akkoord geldt voor de beoordeelde stand en niet automatisch voor later toegevoegde wijzigingen.

Na expliciet akkoord van de gebruiker:

1. Controleer de diff en neem alleen de bij deze slice horende bestanden op.
2. Controleer welke commits de push daadwerkelijk zal bevatten.
3. Commit de geaccepteerde slice met een beschrijvend bericht.
4. Push de milestonebranch naar de juiste remote; controleer of de push geslaagd is.
5. Leg de sliceacceptatie en voortgang vast in de bijbehorende geaccepteerde documentatie. Een nog onbekende commithash kan in de oplevermelding staan; maak geen extra commit uitsluitend om een hash in zijn eigen document te schrijven.

Een pushprobleem wordt gemeld. Er volgt geen force-push, automatische rebase of verwerping van remote werk. Een nieuwe noodzakelijke wijziging na akkoord wordt opnieuw ter review aangeboden.

Akkoord voor een slice is geen toestemming om andere lokale commits ongemerkt mee te publiceren.

## 6. Milestone afronden en mergen

Een milestone is klaar wanneer alle slices zijn geaccepteerd en gepusht en de gezamenlijke eindcriteria zijn gecontroleerd. Bij code horen een build van de volledige stand en de relevante tests. Eerder bewijs mag worden hergebruikt als de gecontroleerde stand en scope nog gelijk zijn.

Het opleverbericht noemt het resultaat en eventuele nog openstaande punten. Wanneer de gebruiker de milestone als afgerond accepteert, wordt de branch volgens de afgesproken werkwijze naar main gemerged en wordt main gepusht. Het akkoord op de laatste slice kan tegelijk milestoneakkoord zijn als dat duidelijk wordt aangegeven.

De merge wordt zichtbaar in de geschiedenis gehouden, bij voorkeur met een gewone mergecommit. Als GitHub branchregels een pull request vereisen, volgen we die regels; een PR-aanpak wordt bij de eerste merge vastgelegd. Branchverwijdering en tags gebeuren alleen als dat is afgesproken. Conflicten worden inhoudelijk opgelost en relevante controles worden herhaald; niet blind een kant kiezen.

## 7. Documentatie en Obsidian

Markdown in de repository is de bron. Elk nieuw of gewijzigd Markdown-bestand wordt gekopieerd naar:

~~~text
C:\Devops\Obsidian\markdown\Development\HNSoftwareDevelopment\Inno Setup Studio
~~~

Behoud relatieve paden: docs/ blijft docs/, tasks/ blijft tasks/. Na kopiëren worden bron en spiegel vergeleken. Ook conceptdocumentatie wordt gespiegeld; gespiegeld betekent niet geaccepteerd of gecommit.

Als kopiëren mislukt, meldt de agent het bestand en de oorzaak. Een onbereikbare spiegel wordt niet als geslaagde controle vermeld.

Productspecificatie beschrijft het gewenste product; ondersteuningsmatrix beschrijft bewijs en bereik; plan/taken beschrijven volgorde en voortgang; beslisdocumenten beschrijven waarom een grens of aanpak is gekozen.

## 8. Startpositie en bestaande commits

Op 8 oktober 2026 is de remote https://github.com/hnsoftwaredevelopment/Inno-Setup-Studio.git. De remote main staat bij de read-only controle op 4c55cee. Lokaal volgen daarop d0bbc09 en 2ef4ccc van de eerdere Studio-taaluitbreiding.

Deze commits zijn gemaakt vóór deze nieuwe akkoord-per-sliceafspraak. Ze worden niet teruggedraaid of ongemerkt gepusht. Bij het akkoord op de documentatie op 8 oktober 2026 wordt deze eerder beoordeelde taalbasis meegenomen in de eerste publicatie; dit is vooraf in de oplevercommunicatie benoemd. De huidige documentatiebranch vertrekt van de lokale basis met deze twee commits.

Voor het publiceren wordt de remote opnieuw gecontroleerd; deze momentopname is geen blijvende garantie over de GitHub-stand.
