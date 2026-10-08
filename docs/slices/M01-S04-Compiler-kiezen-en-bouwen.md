# M01-S04 — Compiler kiezen en bouwen

Status: geïmplementeerd en door de gebruiker volledig getest en geaccepteerd. Branch: `m01-eerste-installer`. Afhankelijk van geaccepteerde M01-S03. Eisen: FR-01, FR-09 en FR-10 uit de [productspecificatie](../Productspecificatie.md).

## Resultaat en acceptatiecriteria

Het nieuwe paneel **Compiler en bouwen** toont de gekozen ISCC-locatie en gecontroleerde versie, biedt handmatig kiezen en opnieuw controleren, en bouwt het opgeslagen project. Na succes zijn projectpad en installerpad zichtbaar en opent **Open uitvoermap** de bijbehorende map. Studio start de installer niet.

Acceptatiecriteria:

1. Automatische ontdekking op gebruikelijke locaties of handmatige keuze werkt; de keuze blijft na herstart bewaard buiten het project.
2. Alleen compilerfamilie 7.1 wordt toegelaten; ontbreken, onbereikbaarheid of een andere versie wordt zichtbaar gemeld.
3. Bouw slaat een nieuw of gewijzigd project eerst op; annuleren van opslaan start geen compiler. Projectbewerking en sluiten zijn tijdens de bouw geblokkeerd.
4. Iedere bouwpoging krijgt een eigen script en uitvoermap. Een fout geeft geen oudere installer als nieuw resultaat weer.

## Technische koppeling en scope

De gebruikelijke locaties zijn Program Files, Program Files (x86) en LocalAppData/Programs, steeds onder Inno Setup 7. Een bestaande persoonlijke keuze heeft voorrang; een verwijderde gekozen compiler wordt gemeld in plaats van ongemerkt vervangen. Er wordt niet recursief gezocht. Afwijkende installaties zijn via de handmatige keuze bereikbaar.

De compilerkeuze staat in `compiler.json` naast de bestaande persoonlijke `settings.json`; Studio-taal blijft behouden. Het projectformaat blijft 3. Projectgegevens bevatten geen machineafhankelijke compilerlocatie.

Versiecontrole voert `ISCC.exe --version` uit, met een grens van 10 seconden. Toegestaan: 7.1.x; concreet gecontroleerd: **7.1.0**. De build gebruikt afzonderlijke procesargumenten, zonder shell, en een native `/O<map>`-override. Zie [Compiler Command-Line Parameters](https://jrsoftware.org/ishelp/topic_compilercmdline.htm). De online handleiding is naslag; de lokale doelcompiler is daadwerkelijk gebruikt.

Uitvoer staat onder `<projectmap>/.studio-builds/<UTC-tijd>-<unieke-id>/`. Daar staan `installer.iss` en bij succes `setup.exe`. Deze map is genegeerd door Git. Eerdere builds en handmatig geëxporteerde scripts worden niet overschreven. Het script is een momentopname van de opgeslagen projectstand; bronbestanden worden tijdens de compilatie door ISCC gelezen, niet vooraf gekopieerd. Wijzig externe bronbestanden daarom niet tijdens de build.

Compileruitvoer wordt op beide proceskanalen gelezen. Een compilatiefout krijgt nu een eenvoudige melding met native uitvoer. Live log, afzonderlijke waarschuwingen/fouten, gebruikersannulering en instelbare uitvoernaam volgen in M01-S05. De bekende compilerwaarschuwing over `setup.exe` blijft bewust tot die uitwerking staan. Er is wel een technische bovengrens van 30 minuten; bij een timeout wordt het gestarte compilerproces beëindigd. Er wordt geen installer uitgevoerd.

## Verificatie en review

Release-build zonder fouten of waarschuwingen. **83 tests geslaagd**, inclusief drie echte compilatietests met ISCC 7.1.0. De nieuwe compileerproef controleert twee opeenvolgende builds met aparte resultaten, behoud van bronbestand/scriptinhoud en een derde mislukte compilatie zonder verwijzing naar de eerdere installer. Aanvullende tests controleren versiebereik, ontbrekende compiler en behoud van compilerkeuze/Studio-taal.

Review: directe argumentoverdracht, versiegate vóór uitvoermap, asynchrone procesafhandeling, blokkeren van projectmutaties, apart opgeslagen persoonlijke instellingen en uitsluitend nieuw resultaat per build gecontroleerd. De gebruiker heeft alle teststappen en aanvullende klik- en wijzigingscontroles doorlopen; de werking is geaccepteerd. Automatische detectie van versie 6 is bewust niet nodig; de bestaande 7.1-versiecontrole volstaat.

## Testen door de gebruiker

1. Start de Release-app en open **Compiler en bouwen**. De standaardinstallatie hoort als Inno Setup 7.1.0 te verschijnen. Kies dezelfde ISCC.exe handmatig, herstart Studio en controleer locatie en Studio-taal.
2. Open een testproject met een geldig klein bronbestand. Pas bijvoorbeeld de titel aan en klik **Bouw installer**. De opgeslagen projectstand wordt gebouwd; het resultaat toont projectpad en het pad naar setup.exe.
3. Open de uitvoermap. Open eventueel installer.iss met de Inno Setup-IDE en vergelijk de gewijzigde titel. Je kunt de gebouwde installer zelf uitvoeren om die titel te controleren.
4. Bouw opnieuw. De nieuwe uitvoermap verschilt; de eerdere map en installer blijven behouden.
5. Test een ontbrekend bronbestand: er mag geen oude installer als nieuw resultaat verschijnen. Herstel de bron en bouw opnieuw.
6. Probeer een nieuw project te bouwen en annuleer het opslaan: er mag geen build starten. Als een andere Inno-versie beschikbaar is, kies die en controleer dat bouwen wordt geweigerd. Controleer ook de paneelteksten in Nederlands, Engels en Duits.

Na gebruikersakkoord wordt alleen deze slice gecommit/gepusht; het aangepaste gebruikersvoorbeeld en de gedownloade Inno-broncode worden niet ongemerkt gepubliceerd.
