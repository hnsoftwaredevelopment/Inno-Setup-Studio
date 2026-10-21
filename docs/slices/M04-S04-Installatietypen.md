# M04-S04 — Installatietypen

Status: geïmplementeerd op m04-installatiekeuzes, na geaccepteerde S03 (e1e8660). Door de gebruiker geaccepteerd op 21 oktober 2026, inclusief de verbeterde plaatsing en uitleg van vertalingen.

## Resultaat en afbakening

De gebruiker beheert installatietypen zoals Volledig, Compact en Aangepast, met unieke interne naam, verplichte standaardbeschrijving, optionele vertalingen per projecttaal en maximaal één type met de native vlag iscustom. Volgorde en de geselecteerde rij in het ontwerpoverzicht worden opgeslagen. Deze ontwerpselectie bepaalt niet de eerste keuze in een echte installer; Inno Setup regelt die zelf en kan een eerdere installatiekeuze gebruiken.

Ontbrekende vertalingen gebruiken de standaardbeschrijving; lege beschrijvingen worden niet geëxporteerd. Een leeg typenoverzicht is toegestaan. Bestaande projecten krijgen een lege lijst. Export gebruikt native Types en taalgebonden CustomMessages. Componenten en de bijbehorende wizardpagina volgen in S05, koppelingen met inhoud in S07. Er worden geen tijdelijke componenten of gesimuleerde installatiekeuzes toegevoegd om typen nu al zichtbaar te maken in de installer.

## Acceptatie en controles

- Toevoegen, verwijderen en ordenen werkt; openen alleen maakt een project niet gewijzigd. Ontwerpselectie en vertalingen blijven bij opslaan/heropenen en Opslaan als behouden.
- Namen zijn geldige native identifiers en uniek zonder onderscheid tussen hoofdletters en kleine letters. Maximaal één aangepast type; ongeldige of lege invoer wordt zichtbaar gemeld en blokkeert opslag/export.
- Taalbeschrijvingen vallen terug op de verplichte standaardbeschrijving. Uitschakelen van een taal behoudt teksten; verwijderen ruimt haar vertalingen op via de bestaande taalverwijdering.
- Automatische tests controleren migratie, validatie, selectie/volgorde, vertalingen, veilige export en compilatie met ISCC 7.1. Build is Release; zichtbare kliktests en installerproeven blijven bij de gebruiker.

Bronnen: meegeleverde Inno Setup 7.1-bron en help voor [Types](https://jrsoftware.org/ishelp/topic_typessection.htm) en [CustomMessages](https://jrsoftware.org/ishelp/topic_custommessagessection.htm).

## Implementatie en verificatie

Projectformaat 13 bewaart InstallTypes en SelectedInstallTypeId. Formaten 1–12 migreren met een lege lijst en zonder geselecteerd type. Opslaan als behoudt de typen en herberekent alleen de bestaande bronverwijzingen. Geen inhoudskoppelingen, fictieve componenten of wijzigingen aan de wizardroute in deze slice. Namen zijn maximaal 128 tekens, maximaal 1000 typen. Teksten worden als letterlijke CustomMessages geëxporteerd; procenttekens worden gecodeerd zodat %n en %1 geen onbedoelde opmaak worden. Preprocessorinvoer en regelovergangen worden geweigerd.

Release-build geslaagd met nul fouten en waarschuwingen. De volledige regressie slaagde met 274 tests; na twee aanvullende gevallen slagen alle 13 gerichte installatietypetests. Inclusief ISCC 7.1.0-compilatie met Engelse/Nederlandse beschrijvingen, terugval, aangepast type, procenttekens, accolades en aanhalingstekens, zonder kunstmatige Components-sectie. Niet-visuele WPF-proef in NL/EN/DE controleert openen zonder wijzigingsstatus, echte lijstselectie, ordenen, vertaalinvoer/terugval, bewaren/heropenen en verwijderen/opnieuw toevoegen. Er zijn geen zichtbare kliktests of installers gestart.

## Handmatige controles

1. Open je bestaande demoproject. Open Installatietypen beheren: aanvankelijk is het overzicht leeg. Voeg full (Volledige installatie), compact (Compacte installatie) en custom (Aangepaste installatie) toe. Markeer alleen custom als aangepast type.
2. Verplaats de typen omhoog/omlaag, selecteer een andere rij, sla op en heropen. Volgorde, waarden en geselecteerde rij moeten behouden zijn. Controleer ook Opslaan als.
3. Open Vertalingen, kies actieve installertalen en vul afwijkende beschrijvingen in. Leeg toont de standaardbeschrijving als grijze tekst; wis een vertaling om terug te vallen. Een andere Studio-taal verandert de inhoud niet. Schakel een installertaal uit en weer in: de vertaling blijft behouden.
4. Probeer een dubbele interne naam, een spatie in de naam, een lege standaardbeschrijving en twee aangepaste typen. Een rode foutmelding moet verschijnen en opslag/export blokkeren. Herstel de invoer; verwijder ook alle typen en voeg opnieuw toe.
5. Exporteer en compileer. Controleer Types en de taalgebonden StudioType-teksten in CustomMessages. De export compileert ook buiten Studio. De componentenpagina verschijnt nu nog niet: componenten volgen in S05. Een installatie uitvoeren bewijst daarom in deze slice nog geen verschillende inhoud per type.

Zie ook [beginnersuitleg](../gebruikershandleiding/Installatietypen.md). Na gebruikersakkoord wordt deze slice gecommit/gepusht; S05 wordt niet automatisch gestart.

## Gebruikerscontrole 21 oktober 2026

De gebruiker bevestigt dat de werking goed is en is akkoord met de overige slice. Eén bedieningspunt is aangepast: Vertalingen staat nu direct onder de standaardbeschrijving, standaard open. De uitleg vóór de taalselectie zegt expliciet iedere installertaal afzonderlijk te kiezen en beschrijft de terugval naar de standaardbeschrijving erboven. De gebruiker heeft de visuele correctie bevestigd en de slice geaccepteerd. De echte controle van de typekeuze blijft onderdeel van de componentenstap.
