# M01-S06 — Eenvoudige installer afronden

Status: geïmplementeerd; de gebruiker heeft de tests afgerond en S06 én milestone M01 als geheel geaccepteerd. Branch `m01-eerste-installer`. Afhankelijk van geaccepteerde M01-S01 t/m S05. Scope: FR-01, FR-02, FR-03, FR-07, FR-09 en FR-10; zie [milestonecriteria](../Milestones.md).

## Resultaat en grenzen

Een export naar een bestaande `.iss` met andere inhoud wordt nu geweigerd met de uitleg dat het bestand behouden blijft en een nieuwe naam gekozen moet worden. Dit geldt zowel voor handmatige aanpassingen als een oudere projectstand. Een inhoudelijk identieke export laat het bestaande bestand ongewijzigd. Er is geen stille vervanging of automatische samenvoeging. Verschijnt tijdens export alsnog een bestand op de doelnaam, dan mislukt het verplaatsen van het tijdelijke bestand zonder dat doel te vervangen. Nieuwe Studio-builds behouden de eerdere afzonderlijke uitvoermappen.

SVGViewerDemo dient als lokale eindproef. De nieuwe variant `samples/SVGViewerDemo/SVG Viewer M01-proef.issstudio` gebruikt `Source/SVGViewer.exe`, doel `{app}`, een eigen AppId en uitvoernaam, en standaardmap `{autopf}/SVG Viewer M01-proef`. Het oorspronkelijke `samples/SVG Viewer.issstudio` blijft intact, inclusief de `{appie}`-waarde van de eerdere foutproef. Het gewijzigde oorspronkelijke eerste voorbeeld blijft eveneens gebruikerswerk.

De M01-proef installeert uitsluitend SVGViewer.exe. Het demo-pakket bevat ook losse DLL's, helpbestanden en assets: de complete applicatie installeren en starten is pas onderdeel van de bestandsverzamelingen in M04. De huidige proef bewijst bestandsplaatsing, wizardinhoud, zelfstandige compilatie en deïnstallatie van de ene bestandsregel. Er worden geen snelkoppelingen of automatische startacties toegevoegd. De lokale executable blijft een door de gebruiker aangeleverde bron en wordt niet automatisch met deze slice in Git opgenomen.

## Automatische controle

De exportbescherming is getest met een echte handmatige toevoeging: aangepaste bytes blijven intact en er blijven geen tijdelijke bestanden achter. Een identieke herexport behoudt eveneens de bytes.

Een afzonderlijke lokale SVG-proef laadt het project, exporteert buiten de projectmap en start rechtstreeks ISCC 7.1.0, zonder Studio of de bouwservice. Ze controleert het compilerresultaat, de gekozen installernaam en afzonderlijke AppId. Deze test wordt met expliciete reden overgeslagen wanneer de lokale SVGViewer-bron of compiler ontbreekt; de andere export- en compilerproeven blijven beschikbaar. Geen test start de installer of de demo-app.

De volledige Release-build slaagt zonder fouten of waarschuwingen. Alle 108 tests slagen, inclusief zes tests met daadwerkelijke compilatie; de lokale SVG-compilatie is geslaagd en niet overgeslagen. Eerdere compilerproeven voor fouten, waarschuwingen, annuleren, projectmigratie en wizardteksten blijven van toepassing. Nieuwe milestoneacceptatie volgt pas nadat de gebruiker de onderstaande installatie-/deïnstallatieproef heeft uitgevoerd.

## Handmatige eindproef

1. Open `samples/SVGViewerDemo/SVG Viewer M01-proef.issstudio` in Studio. Controleer bronbestand, `{app}` als doel, eigen productidentiteit, titel-/bodyteksten en uitvoernaam. Bouw en bewaar de gevonden installerlocatie.
2. Exporteer naar een nieuwe `.iss`-naam in een eigen map. Open die met Inno Setup 7.1 en compileer buiten Studio. Beide routes horen een installer voor hetzelfde project te leveren. De bronlocatie moet op deze machine beschikbaar blijven: de gegenereerde bestandsregel bevat het volledige bronpad.
3. Voeg met een editor een commentaarregel toe aan de geëxporteerde `.iss` en sla op. Exporteer vanuit Studio opnieuw naar dezelfde naam. Verwacht een beschermingsmelding en behoud van de commentaarregel. Exporteer vervolgens naar een andere naam; dat moet werken. De app blijft bruikbaar na de melding.
4. Voer één van de installers uit in je testomgeving, met een nog niet gebruikte testmap. Controleer Welkom, Installatiemap, Bladeren en knopteksten. Kies een eigen map, navigeer terug/vooruit en voltooi de installatie. Gereed en Voltooid houden hun native knoprol.
5. Controleer dat SVGViewer.exe in de gekozen map staat en dezelfde bestandsgrootte en bij voorkeur SHA-256 heeft als `samples/SVGViewerDemo/Source/SVGViewer.exe`. Controleer ook de vermelding **SVG Viewer M01-proef** bij geïnstalleerde Windows-apps. Starten van de demo-app is geen M01-criterium omdat de losse afhankelijkheden nog niet worden geïnstalleerd.
6. Deïnstalleer via Windows of de gegenereerde native uninstaller. Controleer dat de geïnstalleerde executable en appvermelding verwijderd zijn. Bronbestanden, Studio-project en eerdere bouwmappen blijven behouden. Bij een lege testmap hoort Inno deze waar mogelijk op te ruimen; eigen extra bestanden mogen behouden blijven.
7. Heropen het project, wissel Studio-taal en bouw nogmaals. Controleer behoud van AppId en projectwaarden. Bevestig vervolgens afzonderlijk de slice en het volledige resultaat van M01 als de eindproef slaagt.

## Oplevering

De gebruiker heeft alle tests afgerond en meldt dat alles werkt zoals afgesproken. S06 en M01 zijn geaccepteerd; commit/push en merge naar main zijn expliciet geautoriseerd. Er wordt geen volledigheid van de SVG-appinstallatie of runtimeproef door de agent geclaimd.
