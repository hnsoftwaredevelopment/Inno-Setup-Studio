# M01-S05 — Bouwmeldingen en annuleren

Status: geïmplementeerd, handmatig getest en door de gebruiker geaccepteerd. Branch `m01-eerste-installer`, na geaccepteerde M01-S04 (`33d8c33`). Eisen FR-01, FR-10 en FR-13 uit de [productspecificatie](../Productspecificatie.md).

## Resultaat en criteria

Een afzonderlijk venster **Bouwmeldingen** toont tijdens het bouwen de native compileruitvoer. Waarschuwingen en fouten krijgen een herkenbaar label; waar ISCC het levert worden scriptbestand en regelnummer getoond. Het venster blijft bedienbaar terwijl het project geblokkeerd is. **Bouw annuleren** stopt het door Studio gestarte compilerproces en eventuele subprocessen; na beëindiging kan het project weer worden bewerkt en gebouwd. Sluiten van het meldingenvenster tijdens het bouwen vraagt eveneens annulering aan en houdt het venster open totdat de compiler is gestopt.

De uitvoernaam is instelbaar in **Compiler en bouwen**, zonder `.exe`. Nieuwe projecten beginnen met `mysetup`. De keuze `setup` blijft toegestaan, maar toont direct een Studio-waarschuwing. Na een geslaagde build met compilerwaarschuwingen staat expliciet dat de installer met waarschuwingen is gebouwd. Studio start de installer niet.

Acceptatiecriteria:

1. Live uitvoer, native waarschuwingen en fouten zijn leesbaar; bouwmeldingen blijven na afloop beschikbaar zolang het project in deze Studio-sessie open is.
2. Annuleren stopt de actieve compiler; een afgebroken of mislukte build toont geen oudere executable als nieuw resultaat.
3. De bestandsnaam blijft na opslaan/openen behouden en geldt zowel voor zelfstandig geëxporteerde scripts als bouwen vanuit Studio.
4. Oude projecten behouden applicatie-identiteit, bronregel, pagina-inhoud en de eerder gebruikte uitvoernaam; de bron wordt niet automatisch overschreven bij openen.

## Native koppeling en bereik

ISCC 7.1.0 ondersteunt `/MJ` (`--messages-jsonl`). Studio leest stdout en stderr tegelijk en verwerkt de JSONL-velden `severity`, `message`, `filename` en `line`. Gewone regels en niet herkende regels blijven als tekst behouden. Dit is relevant omdat 7.1.0 sommige fouten nog als gewone tekst meldt. De oorspronkelijke uitvoer wordt niet vertaald; alleen Studio-labels volgen Nederlands, Engels of Duits.

Naslag: [Compiler Command-Line Parameters](https://jrsoftware.org/ishelp/topic_compilercmdline.htm). De gedownloade `samples/issrc-main/Projects/ISCC.dpr` is read-only gebruikt voor de berichtstructuur; de lokale **7.1.0**-compiler bevestigt het gedrag. De gedownloade hoofdbranch is geen bewijs van doelversieondersteuning op zichzelf.

Per voltooide compilatiepoging, ook bij native fout of annulering tijdens de compilatie, staat `compiler.log` naast `installer.iss` in de unieke `.studio-builds`-map. Dit log bevat de onbewerkte regels van de twee proceskanalen; hun onderlinge volgorde is afhankelijk van de procesuitvoer. Het live venster heeft een begrensde tekstbuffer; de normale opgeslagen compilerlog bevat de volledige opgevangen uitvoer. Bij fouten vóór het compilerproces start of een technische timeout kan een uitvoermap/log ontbreken; de fout blijft zichtbaar in Bouwmeldingen.

Annuleren beëindigt de door Studio gestarte procesboom, wacht op procesafsluiting en leest de resterende uitvoer. Het resultaat is geannuleerd, ook wanneer het verzoek tegelijk met compilerafronding binnenkomt. Eventuele gedeeltelijke bestanden in de eigen bouwmap blijven voor onderzoek behouden en gelden niet als een bruikbare installer. Eerdere bouwmappen worden niet gewijzigd. De bestaande timeout blijft een technische bovengrens van 30 minuten.

`OutputBaseFileName` wordt regulier als `[Setup] OutputBaseFilename` geëxporteerd; de bouwactie gebruikt daarnaast de overeenkomstige `/F`-naam en `/O`-map. Alleen een gewone Windows-basisnaam van maximaal 120 tekens is toegestaan, zonder pad, `.exe`, constanten, controlekarakters, ongeldige tekens, eindpunt/-spatie of gereserveerde apparaatnaam. Unicode en spaties binnen de naam zijn toegestaan. De waarschuwing voor `setup` geldt ongeacht hoofdletters. Er worden geen compilerwaarschuwingen onderdrukt.

## Projectformaat 4

Nieuwe en opgeslagen projecten gebruiken formaat **4**, met verplichte `OutputBaseFileName`. Formaten 1, 2 en 3 blijven leesbaar. Hun eerdere vaste naam `setup` wordt expliciet overgenomen; de gebruiker kan die aanpassen. Bij formaat 3 blijft de bestaande bestandsregel behouden; alleen de oudere formaten 1 en 2 krijgen de al eerder gebruikte lege regel. Een migratie schrijft niets totdat de gebruiker opslaat. Bestanden met een oudere versiemarkering én een nieuwe uitvoernaam worden geweigerd zodat die waarde niet ongemerkt wordt weggegooid.

## Verificatie

Release-build zonder fouten of waarschuwingen; **106 tests geslaagd**. Vijf automatische tests gebruiken daadwerkelijke compilatie met ISCC 7.1.0, inclusief succes, native waarschuwing/fout en annulering. Daarnaast zijn migratie/roundtrip, ontbrekende formaatvelden, uitvoernaamvalidatie, JSONL- en gewone tekstmeldingen en annuleren vóór processtart gecontroleerd. De agent heeft geen GUI-kliktest of installer uitgevoerd.

Review: tekst wordt als data verwerkt, losse procesargumenten voorkomen shellinterpretatie, annuleren richt zich op het gestarte proces, recente resultaten worden vóór een nieuwe poging gewist en migratie bewaart bestaande gegevens. Voor de live meldingen gebruikt Studio een afzonderlijk venster zodat de annuleeractie buiten de geblokkeerde ontwerpruimte blijft.

## Testen door de gebruiker

1. Maak een nieuw testproject met een klein bronbestand. De naam begint met `mysetup`; wijzig naar bijvoorbeeld `Mijn installer-1.0`, sla op en heropen. Exporteer/compileer én bouw vanuit Studio: beide gebruiken de gekozen naam.
2. Open het bestaande voorbeeldproject. De oude naam `setup` is behouden en toont een waarschuwing. Bouw: de oorspronkelijke compilerwaarschuwing moet in Bouwmeldingen staan en het resultaat meldt succes met waarschuwingen. Wijzig daarna naar `mysetup` en bouw opnieuw: die specifieke waarschuwing hoort weg te zijn.
3. Geef een ongeldige naam zoals `../setup`, `CON` of `setup.exe`: opslaan/export/bouwen wordt met uitleg geweigerd. Herstel daarna een geldige naam.
4. Controleer een echte compilerfout door tijdelijk een ongeldige Inno-constante in het standaardpad te zetten, bijvoorbeeld `{bestaatniet}`. De native fout moet zichtbaar zijn, waar beschikbaar met scriptregel. Er mag geen oudere installer als nieuw resultaat verschijnen. Herstel het standaardpad en bouw opnieuw.
5. Kies eventueel een groter lokaal testbestand zodat bouwen lang genoeg duurt om op **Bouw annuleren** te klikken. De status wordt Geannuleerd; na procesafsluiting is Studio weer bedienbaar en werkt een volgende bouwpoging. Sluiten van het meldingenvenster tijdens bouwen hoort eveneens te annuleren.
6. Open na afloop de uitvoermap en controleer `installer.iss` en `compiler.log`. Sluit het meldingenvenster en open het opnieuw via **Bouwmeldingen**. Controleer ook NL/EN/DE voor de nieuwe labels; de compilertekst blijft native.

De gebruiker heeft de handmatige tests uitgevoerd en de slice geaccepteerd. De live compileruitvoer is expliciet als goed werkend beoordeeld. Het wijzigen van het bestandsdoel van {app} naar {appie} is vóór compilatie geweigerd: daarom is terecht geen bouwmap of compiler.log aangemaakt. Dit is Studio-exportvalidatie; het automatische bewijs voor een native compilerfout met scriptregel staat afzonderlijk hierboven. Gebruikerswijzigingen in voorbeelden en de gedownloade Inno-broncode blijven behouden buiten de slice.
