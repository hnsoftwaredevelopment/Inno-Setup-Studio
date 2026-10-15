# M03 — Documentfouten direct tonen

Status: aanvulling op M03-S06, door de gebruiker geaccepteerd op 15 oktober 2026. De geaccepteerde wizardproef is gepubliceerd als bb919b7; M03 is niet gemerged.

## Aanleiding en werking

Het documentvenster zette foutmeldingen als gewone tekst in het voorbeeldveld en controleerde pas na klikken op Documentvoorbeeld tonen. Daardoor leek een foutmelding op documentinhoud. Licentie, Informatie vóór en Informatie na controleren nu automatisch bij openen, na een padwijziging en na taalkeuze. Bij typen wordt na 250 ms rust gelezen; oude leesresultaten worden geannuleerd en kunnen de huidige weergave niet overschrijven.

De algemene bron en alle ingevulde taalvarianten worden gecontroleerd, ook als de betreffende variant niet geselecteerd is. Fouten verschijnen met bron/taal en pad in rood boven het voorbeeldveld. Een fout in de geselecteerde bron laat het voorbeeld leeg; een geldige geselecteerde bron blijft zichtbaar wanneer een andere variant fout is. Foutmeldingen worden nooit als documentinhoud weergegeven. Een ontbrekende algemene bron blijft herkenbaar als inactieve pagina.

Een gedeelde, alleen-lezen documentlezer gebruikt dezelfde padcontrole en limiet van 4 MiB in instellingen en simulatie. TXT ondersteunt UTF-8 en herkenbare Unicode-BOM’s; ongeldige byte-inhoud en ongebruikelijke besturingstekens worden geweigerd. RTF vereist een RTF-header en wordt daarnaast door WPF geladen. Dit is een controle voor het voorbeeld, geen volledige vervanging van de native compiler. Documentlinks zijn niet actief.

**Voorbeeld vernieuwen** blijft beschikbaar wanneer een bestand buiten Studio is gewijzigd. De automatische controle wijzigt geen projectgegevens. Het kiezen of typen van een ander pad blijft een normale ontwerpwijziging. In de wizardproef staan documentfouten eveneens rood boven het documentveld en wordt Verder geblokkeerd.

## Verificatie en handmatige controle

Release-build: 0 fouten en 0 waarschuwingen. Alle 236 automatische tests geslaagd, inclusief bestaande ISCC-compilatieproeven. Nieuwe documentleestests controleren Unicode, binaire/te grote invoer, ontbrekende/ongeldige bronnen en RTF-bytes/header. Een niet-visuele WPF-proef controleert voor alle drie documentvensters automatisch laden, afzonderlijke rode fouten, herstel, snelle padwijzigingen, ongeldige RTF en een ongeldige niet-geselecteerde taalvariant. Er zijn geen vensters getoond of visuele kliktests uitgevoerd.

1. Open elk van de drie documentvensters met een ontbrekend of ongeldig bestand. Zonder op een voorbeeldknop te drukken moet een rode fout boven het lege veld verschijnen.
2. Kies een geldig TXT- of RTF-bestand. Het voorbeeld verschijnt automatisch en de betreffende fout verdwijnt.
3. Test een verkeerde algemene bron met een geldige geselecteerde taalvariant, en omgekeerd. De melding benoemt de foutieve bron; geldige geselecteerde inhoud blijft zichtbaar.
4. Stel een foutieve variant in een andere taal in. Ook die wordt gemeld zonder eerst die taal te selecteren. Herstel alle fouten en controleer dat de melding verdwijnt.
5. Wijzig snel van pad of taal en open een opgeslagen project opnieuw. Oude fouten mogen niet terugkomen bij de nieuwe bron. Controleer dezelfde documentfout ook in Uitproberen.

De gebruiker heeft de duidelijke meldingen gecontroleerd en akkoord gegeven. Deze aanvulling wordt afzonderlijk gecommit/gepusht. M03 wordt pas na expliciete acceptatie van de hele milestone gemerged.

Milestoneafronding: M03 is na expliciet gebruikersakkoord op 15 oktober 2026 naar main gemerged en gepusht als 84c8a06.
