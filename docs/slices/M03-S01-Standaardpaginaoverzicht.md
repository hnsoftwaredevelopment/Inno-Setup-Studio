# M03-S01 — Standaardpaginaoverzicht

Status: door de gebruiker geaccepteerd op m03-wizardpaginas.

Resultaat: vanuit de paginanavigatie een overzicht openen van alle veertien reguliere wizardpagina’s/fasen, in native volgorde. Per fase staan status, reden en beschikbaarheid van de ontwerper vermeld. Dit overzicht is alleen-lezen; de huidige export bepaalt de status. Welkom en Installatiemap verwijzen naar hun bestaande ontwerppagina. Alle andere instellingen volgen in de reeds geplande slices.

Acceptatiecriteria:

- Volgorde en voorwaarden volgen de [officiële wizardpagina’s](https://jrsoftware.org/ishelp/topic_wizardpages.htm); Basisontwerp en taalkeuzedialoog zijn geen reguliere pagina.
- Ingeschakeld, uitgeschakeld, voorwaardelijk en runtime zijn herkenbaar met tekst; toekomstige configuratie wordt onderscheiden van huidige zichtbaarheid.
- Openen, selecteren en sluiten veranderen projectgegevens, previewtaal en wijzigingsstatus niet. Alleen de expliciete ontwerpactie wijzigt de ontwerpselectie.
- Alle teksten zijn beschikbaar in de Studio-talen Nederlands, Engels en Duits. Bestaande projecten en export blijven bruikbaar.

Afbakening: geen nieuwe projectvelden, migratie, zichtbaarheidsschakelaars, vrij herschikken of onbewezen preview. Gereed en Voltooid worden wel al door de huidige native installer getoond, hoewel hun ontwerper later volgt. Voorbereiden kan alleen bij runtimevoorwaarden pauzeren; Installeren is de daadwerkelijke runtimefase.

Bestandsgroepen: Core/paginaoverzicht, App/overzichtvenster en toegang, resources en gerichte tests. FR-06/07/09; afhankelijk van M02.

Handmatige controle:

1. Open het paginaoverzicht vanuit de linker paginanavigatie. Controleer volgorde en leesbare status/redenen, ook bij een kleiner venster.
2. Selecteer Licentie, Componenten, Voorbereiden en Installeren: de uitleg onderscheidt ontbrekende instellingen, runtimevoorwaarden en uitvoering. Er is geen ontwerpactie voor nog niet ondersteunde pagina’s.
3. Kies Welkom of Installatiemap en open de ontwerper. De juiste bestaande pagina verschijnt. Sluit het overzicht zonder ontwerpactie: de oorspronkelijke selectie blijft behouden.
4. Herhaal in Nederlands, Engels en Duits. De installertaal en wizardteksten blijven onveranderd. Controleer dat een schoon project schoon blijft.
5. Open een bestaand project en controleer dat huidige tekstbewerking, Uitproberen en export nog werken.

Release-build geslaagd met 0 fouten en 0 waarschuwingen. De volledige regressierun telt 192 geslaagde tests. Daarna is een aanvullende koppelingstest met de huidige export toegevoegd; de laatste gerichte pagina- en lokalisatierun telt 17 geslaagde tests. De gebruiker heeft het volledige overzicht, de smallere vormgeving en de stabiele lijsthoogte getest en geaccepteerd. De laatste Release-build slaagt zonder fouten of waarschuwingen. Commit/push is daarmee akkoord.

Na gebruikersfeedback: het venster opent smaller (540 breed) en gebruikt de bestaande Studio-lijststijl. Paginanummers en tabelkolommen vervallen. Groene/rode schakelaars tonen alleen-lezen aan/uit; Installeren is vast aan. Voorbereiden heeft een amber vraagteken en expliciete voorwaardelijke uitleg. Status is ook als tekst en voor toegankelijkheid beschikbaar. Native pagina’s zijn niet vrij herschikbaar; het aangeleverde changeorder.svg is daarom niet nodig. Controleer scrollen, rijselectie, de vaste schakelaars en uitleg ook bij een kleiner venster.

Het uitlegvlak en de knoppenrij hebben vaste hoogtes, zodat selectiewisselingen de paginalijst niet laten verspringen. Lange uitleg kan binnen het eigen uitlegvlak scrollen. Controleer vooral wisselen tussen Welkom, Voorbereiden en Installeren, ook in Duits en met een smal venster.
