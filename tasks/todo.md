# Slices — Inno Setup Studio

Status: planning geaccepteerd op 8 oktober 2026. Een vinkje betekent geaccepteerd, niet alleen geschreven of gebouwd. M00, M01-S01, M01-S02, M01-S03, M01-S04, M01-S05 en M01-S06 zijn geaccepteerd; M01 is als geheel geaccepteerd.

Milestoneresultaten en FR-koppelingen: [Milestones](../docs/Milestones.md). Werkwijze en controles: [Werkwijze](../docs/Werkwijze.md). Voortgang van de proefversie: [Historie](../docs/historie/Proefversie.md).

## Leeswijzer

B = Release-build; T = gerichte geautomatiseerde tests; C = compileren met ISCC 7.1.0; U = gebruiker controleert Studio; I = gebruiker controleert de echte installer in een geschikte testomgeving; D = document-/link-/spiegelcontrole.

Bij iedere codeslice zijn B en relevante T vereist; ze staan compact in de tabellen. C bewijst geen uitgevoerde installatie. I is altijd een gebruikersactie. De concrete teststappen worden bij oplevering geleverd.

De onderstaande slices zijn voorstellen. Bestandsgroepen noemen de te verwachten delen van de oplossing, geen opdracht om vooraf nieuwe modules te maken. Voor bouwen concretiseren we de eerste slice en splitsen we haar als ze meer dan één gerichte werksessie of veel onafhankelijke bestanden vraagt.

## M00 — m00-productspecificatie

Bestanden: docs/, tasks/, README.md en AGENTS.md. Dit is documentatie; geen applicatiebuild nodig.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [x] | M00-S01 Product en grenzen | Eindfuncties en huidige basis zijn onderscheiden; Inno Setup-grens is controleerbaar; vier reviewpunten zijn besproken. | D, gebruikersreview; huidige basis |
| [x] | M00-S02 Planning en werkwijze | Milestones/slices hebben resultaten en controles; branch-/akkoordproces staat vast; omgang met bestaande lokale commits is besloten vóór publicatie. | D, gebruikersreview; S01 |

Huidige stand: beide documentatieslices zijn door de gebruiker geaccepteerd, gepubliceerd op m00-productspecificatie en gemerged naar main (37ac314). Publicatie omvat ook de eerder beoordeelde Studio-taalbasis (d0bbc09 en 2ef4ccc).

## M01 — m01-eerste-installer

Bestandsgroepen: Core/project/opslag; App/projecteigenschappen; generator/compilerkoppeling; gerichte model-/generatietests; klein eigen voorbeeldproject.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [x] | M01-S01 Applicatiegegevens | Productnaam, AppId en versie zijn bewerkbaar en bewaard; proefprojecten openen zonder gegevensverlies; AppId blijft stabiel. | B/T/U; M00 |
| [x] | M01-S02 Eerste bestandsregel en export | Een lokale bron en doelmap zijn instelbaar; .iss bevat juiste metadata en bestandsregel; export compileert buiten Studio en meldt ontbrekende bron. | B/T/C/U; S01 |
| [x] | M01-S03 Huidig ontwerp exporteren | Twee huidige pagina's en standaardpad worden correct vertaald; gedeelde en lokale tekstafwijkingen werken of worden aantoonbaar begrensd; knoprollen blijven correct. | B/T/C/I; S02 |
| [x] | M01-S04 Compiler kiezen en bouwen | Ontdekken/handmatig kiezen en versiecontrole werken; Bouw compileert een herkenbare projectstand; de resulterende uitvoer is vindbaar. | B/T/C/U; S03 |
| [x] | M01-S05 Bouwmeldingen en annuleren | Fouten/waarschuwingen zijn zichtbaar; annuleren stopt gecontroleerd; een mislukte build presenteert geen oud bestand als nieuw resultaat. | B/T/C/U; S04 |
| [x] | M01-S06 Eenvoudige installer afronden | Uitvoer wordt niet ongemerkt overschreven bij handmatige wijzigingen; dezelfde export werkt buiten Studio; gebruiker controleert verse installatie en deïnstallatie. | B/T/C/I, milestonecheck; S05 |

M01-S03 onderzoekt juist de bestaande previewbeloften. Als een property niet regulier werkt, wordt haar beperking vastgelegd vóór verdere uitbreiding.

M01-S01 is geaccepteerd en gepubliceerd als commit 5d7be9c op m01-eerste-installer. Bij oplevering slaagden 45 tests en de Release-build. [Testinstructies en formaatmigratie](../docs/slices/M01-S01-Applicatiegegevens.md).

M01-S02 is geïmplementeerd: 72 tests geslaagd, waaronder twee compilatieproeven met ISCC 7.1.0, en een Release-build zonder fouten of waarschuwingen. De gebruiker heeft alle controles uitgevoerd en op 8 oktober 2026 akkoord gegeven. [Resultaat en gebruikerscontrole](../docs/slices/M01-S02-Bestandsregel-en-export.md). De geaccepteerde stand is gepubliceerd als commit 71f21cf.

De bij S02 gemelde setup.exe-waarschuwing is verwerkt in M01-S05: instelbare uitvoernaam, standaard mysetup voor nieuwe projecten en zichtbare Studio- én native compilerwaarschuwing bij setup. De gebruiker heeft M01-S05 geaccepteerd.

## M02 — m02-installertalen

Bestandsgroepen: Core/taalbronnen/project/migratie; App/talen/preview/eigenschappen; generator; taal- en migratietests; resources.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [x] | M02-S01 Beschikbare taalbronnen | Default.isl en gevonden taalbestanden worden ingelezen zonder wijziging; extra isl kan worden gekozen; ontbrekende/ongeldige bron geeft uitleg. | B/T/U; M01 |
| [x] | M02-S02 Projecttalen en previewkeuze | Ingeschakelde talen en volgorde worden bewaard; nieuwe projecten beginnen Engels en oude Nederlandse waarden blijven Nederlands; Voorbeeldtaal staat los van Studio-taal. | B/T/U; S01 |
| [x] | M02-S03 Eigen teksten per taal | Standaard, gedeeld en lokaal hebben zichtbare herkomst; herstellen verwijdert de juiste afwijking; lege/ontbrekende eigen vertalingen volgen de vooraf vastgelegde terugvalregel. | B/T/U; S02 |
| [x] | M02-S04 Vertaaloverzicht | Meerdere projecttalen zijn naast elkaar te bewerken; placeholders en ontbrekende teksten worden gemeld; taal uitschakelen/verwijderen verliest geen tekst ongemerkt. | B/T/U; S03 |
| [x] | M02-S05 Installertaal bewijzen | Alleen-Nederlands en meertalig exporteren/compileren; echte wizard volgt taalkeuze; relevante RTL-/lettertypebeperkingen zijn getest of zichtbaar geregistreerd. | B/T/C/I, milestonecheck; S04 |

## M03 — m03-wizardpaginas

Bestandsgroepen: Core/paginamodel/status; App/navigatie/preview/eigenschappen; generator; pagina- en navigatietests; documentassets.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [x] | M03-S01 Standaardpaginaoverzicht | Alle reguliere fasen zijn herkenbaar; volgorde blijft native; ingeschakeld/uitgeschakeld/voorwaardelijk/runtime zijn onderscheiden. | B/T/U; M02 |
| [x] | M03-S02 Licentie en informatie | Documenten en taalvarianten zijn instelbaar; zichtbaarheidsvoorwaarden passen bij het project; gebruiker controleert licentieacceptatie en documenten. | B/T/C/I; S01 |
| [x] | M03-S03 Gebruikersinformatie en startmenumap | Ondersteunde velden en mapbeleid worden opgeslagen; preview toont toepasselijke invoer; export bevat de juiste reguliere instellingen. | B/T/C/U; S02 |
| [x] | M03-S04 Gereed, voortgang en voltooid | Samenvatting en afsluitrollen zijn herkenbaar; simulatie wijzigt geen model; installeren/voltooien zijn geen vaste Verder-tekst. | B/T/C/U; S03 |
| [ ] | M03-S05 Basisontwerp en pagina-afwijkingen | Overerving heeft juiste taal/rol/scope; lokale wijzigingen lekken niet bij terugnavigeren; onbewezen properties zijn niet actief bewerkbaar. | B/T/C/I; S04 |
| [ ] | M03-S06 Wizardproef | Een samengesteld voorbeeld toont de juiste pagina's en teksten; ontwerpselectie herstelt na Uitproberen; bestaande voorbeeldprojecten blijven bruikbaar. | B/T/C/I, milestonecheck; S05 |

## M04 — m04-installatiekeuzes

Bestandsgroepen: Core/installatieregels/referenties; App/regeloverzichten/keuzepreview; generator; conditie- en generatietests.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [ ] | M04-S01 Bestandsverzamelingen | Wildcards, submappen en uitsluitingen zijn bewerkbaar; voorbeeldlijst past bij de bron; export behoudt de regels bij opslaan/openen. | B/T/C/U; M03 |
| [ ] | M04-S02 Mapregels | Losse mapregels zijn te beheren; relevante opties worden gevalideerd; regels volgen projectreferenties en opslag. | B/T/C/U; S01 |
| [ ] | M04-S03 Snelkoppelingen | Naam, doel, parameters en map zijn instelbaar; ontbrekende verwijzingen worden gemeld; gebruiker controleert een startmenusnelkoppeling. | B/T/C/I; S02 |
| [ ] | M04-S04 Installatietypen | Typen hebben unieke identifiers en vertaalbare beschrijvingen; selectie wordt bewaard in ontwerpgegevens; export compileert. | B/T/C/U; S03 |
| [ ] | M04-S05 Componenten | Componenten hebben gecontroleerde structuur en typekoppeling; componentpagina werkt in de preview; ongeldige referenties worden gemeld. | B/T/C/U; S04 |
| [ ] | M04-S06 Taken | Taken hebben beginselectie en relevante opties; takenpagina past bij projectinhoud; vertalingen en opslag werken. | B/T/C/U; S05 |
| [ ] | M04-S07 Regels koppelen en keuzes bewijzen | Bestanden/mappen/icons volgen typen, componenten en taken; verwijderen toont afhankelijke regels; gebruiker vergelijkt twee verschillende echte installatiekeuzes. | B/T/C/I, milestonecheck; S06 |

## M05 — m05-systeemintegratie

Bestandsgroepen: Core/installatiebeleid/configuratieregels; App/beleid/regelbewerking; generator; validatie- en exporttests.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [ ] | M05-S01 Architecturen | Doelarchitectuur, 64-bit installatiemodus en setup-architectuur zijn afzonderlijk instelbaar; ongeldige combinaties worden gemeld; geschikte voorbeelden compileren. | B/T/C/U; M04 |
| [ ] | M05-S02 Rechten en doelpaden | Huidige-gebruiker/alle-gebruikersbeleid werkt; defaultpaden passen bij beleid; constanten blijven expressies en zijn geen lokale machinepaden. | B/T/C/I; S01 |
| [ ] | M05-S03 Registerregels | Sleutel, type, waarde en bitness worden bewaard; voorwaarden werken; gebruiker controleert de bedoelde testwaarde en opruiming. | B/T/C/I; S02 |
| [ ] | M05-S04 INI-regels | Bestand/sectie/sleutel/waarde zijn bewerkbaar; quoting en voorwaarden zijn correct; gebruiker controleert testconfiguratie. | B/T/C/I; S03 |
| [ ] | M05-S05 Programma-acties | Run/UninstallRun hebben heldere uitvoermomenten en parameters; export behoudt volgorde/opties; preview voert geen acties uit. | B/T/C/U; S04 |
| [ ] | M05-S06 Opruimregels | InstallDelete/UninstallDelete zijn apart zichtbaar; doel en moment zijn duidelijk; gebruiker controleert beperkte eigen testbestanden. | B/T/C/I; S05 |
| [ ] | M05-S07 Update en deïnstallatie | Stabiele applicatie-identiteit blijft behouden; verse installatie/update/deïnstallatie zijn gecontroleerd; niet geteste architecturen zijn niet als bewezen gemarkeerd. | B/T/C/I, milestonecheck; S06 |

## M06 — m06-vormgeving

Bestandsgroepen: eigenschappencatalogus; Core/vormgeving; App/preview/eigenschappen; generator; assets en generatietests.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [ ] | M06-S01 Eigenschappen bewijzen | De huidige en voorgestelde eigenschappen hebben scope en native koppeling; risico-eigenschappen worden eerst in kleine exports onderzocht; matrix registreert uitkomsten. | B/T/C/I; M03, na M05 in volgorde |
| [ ] | M06-S02 Afbeeldingen | Wizard-/headerafbeeldingen zijn te kiezen; formaat en ontbrekende bron worden gemeld; preview en export gebruiken dezelfde assets. | B/T/C/I; S01 |
| [ ] | M06-S03 Ingebouwde stijlen | Alleen voor 7.1 gecontroleerde stijlen zijn kiesbaar; relevante licht/donker-keuzes worden vertaald; previewbeperkingen zijn herkenbaar. | B/T/C/I; S02 |
| [ ] | M06-S04 Afmetingen en scopes | Ondersteunde schaal-/afmetingsinstellingen worden bewaard; alleen bewezen properties krijgen lokale afwijkingen; herstellen werkt. | B/T/C/I; S03 |
| [ ] | M06-S05 Vormgevingsproef | Representatieve stijl/assetcombinaties zijn vergeleken; langere teksten en keyboardbediening blijven bruikbaar; unsupported kleuren/controltrucs ontbreken. | B/T/C/I/U, milestonecheck; S04 |

## M07 — m07-geavanceerde-opties

Bestandsgroepen: versie-/optiecatalogus; Core/validatie; App/geavanceerde editor; generator/compilerprofielen; optie- en integratietests.

Vooraf is de omvang nog niet per richtlijn bekend. S02–S06 worden na S01 opgesplitst in kleine sub-slices als de catalogus dat vereist. M07 is niet klaar zolang in-scope dekking zonder besluit ontbreekt.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [ ] | M07-S01 Complete 7.1-catalogus | Iedere niet-verouderde richtlijn, sectieparameter en flag is ingedeeld; scope/type/bron zijn vastgelegd; ontbrekende dekking wordt zichtbaar in vervolgslices. | D, broncontrole; M05/M06 |
| [ ] | M07-S02 Projectrichtlijnen | Geavanceerde projectopties krijgen passende invoer; ze delen eigenaarschap met visuele velden; versie/waardefouten worden gemeld. | B/T/C/U; S01 |
| [ ] | M07-S03 Regelparameters | Geavanceerde parameters/flags op ondersteunde regeltypen worden bewaard; combinaties worden gecontroleerd; generatie verliest geen waarden. | B/T/C/U; S02 |
| [ ] | M07-S04 Build- en runtimebeleid | Uitvoer/compressie/schijfverdeling en reguliere herstart-/bestandsgebruik-/wachtwoordopties hebben geregistreerde bediening; export volgt keuzes; silent gedrag krijgt een gebruikersscenario. | B/T/C/I; S03 |
| [ ] | M07-S05 Native downloads en archieven | Ondersteunde native bronopties en vereiste parameters zijn instelbaar; regels worden gevalideerd; gebruiker test een klein gecontroleerd voorbeeld. | B/T/C/I; S04 |
| [ ] | M07-S06 Ondertekening en verificatie | Door gebruiker ingerichte signing en publieke verificatiesleutels passen in export/build; ontbrekend hulpmiddel geeft uitleg; gevoelige lokale gegevens worden niet ongemerkt projectinhoud. | B/T/C, gebruikerstest waar middelen beschikbaar; S05 |
| [ ] | M07-S07 Dekkingscontrole | Iedere in-scope catalogusregel is beschikbaar en gecontroleerd of expliciet uit scope besloten; voorbeeldexports compileren; matrix weerspiegelt werkelijk bewijs. | D/B/T/C/I, milestonecheck; S06 |

## M08 — m08-scripting

Bestandsgroepen: Core/bronbeheer/eigen-pagina's; App/scripteditor/preview; generator/eventbeheer; bron- en compilatietests.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [ ] | M08-S01 Eigen Pascal-bron | Opslagstrategie is vastgelegd; code blijft bewaard bij genereren/openen; compilerfouten verwijzen naar een herkenbare bron. | B/T/C/U; M07 |
| [ ] | M08-S02 Eventcompositie | Eigen en gegenereerde events hebben gecontroleerde volgorde; conflicten worden gemeld; navigatie-afwijkingen blijven correct werken. | B/T/C/I; S01 |
| [ ] | M08-S03 Preprocessorinvoer | Eigen definities/includes worden bewaard en geëxporteerd; ontbrekende bron geeft uitleg; gegenereerde secties worden niet stil overschreven. | B/T/C/U; S02 |
| [ ] | M08-S04 Tekst- en informatiepagina's | Native tekstinvoer-/informatiepagina's hebben een geldig invoegpunt; titels en labels zijn vertaalbaar; waarden in preview blijven tijdelijk. | B/T/C/I; S03, M02 |
| [ ] | M08-S05 Keuze-, map- en bestandspagina's | Native opties en invoer zijn instelbaar; binding van ingevoerde waarden is gedocumenteerd; echte wizard volgt het gecontroleerde voorbeeld. | B/T/C/I; S04 |
| [ ] | M08-S06 Scriptingproef | Herhaald genereren behoudt eigen bron; export werkt buiten Studio; preview voert scripts niet uit en belooft geen volledige simulatie. | B/T/C/I, milestonecheck; S05 |

## M09 — m09-productoplevering

Bestandsgroepen: Core/geschiedenis/migraties; App/toegankelijkheid/status; tests/fixtures; gebruikersdocumentatie; distributieconfiguratie.

| Acceptatie | Slice / resultaat | Toetsbare criteria | Controle / afhankelijk |
|---|---|---|---|
| [ ] | M09-S01 Undo/redo | Modelwijzigingen zijn terug te nemen en opnieuw toe te passen; wijzigingsstatus blijft correct; taalwissel en uitprobeeracties komen niet in undo. | B/T/U; M08 |
| [ ] | M09-S02 Compatibiliteit en herstel | Proef- en nieuwe projecten openen betrouwbaar; ongeldige/nieuwere documenten vernietigen geen huidig werk; paden, ontbrekende assets en exportconflicten hebben herstelbare melding. | B/T/U; S01 |
| [ ] | M09-S03 Gebruikskwaliteit | Keyboard, schermschaal en kleinere vensters zijn bruikbaar; NL/EN/DE zijn volledig voor Studio; relevante installertaalbeperkingen zijn duidelijk. | B/T/U; S02 |
| [ ] | M09-S04 Gezamenlijke regressie | Eindscenario's uit de productspecificatie zijn uitgevoerd; handleiding beschrijft actuele werking en grenzen; geen blokkerende fouten staan open. | B/T/C/I/D, gebruikerreview; S03 |
| [ ] | M09-S05 Distributie | Volledige uitgifte bevat juiste bestanden en talen; runtime-/compilervereisten zijn gecontroleerd; versie en oplevernotities zijn correct. | B/T/D, gebruiker start pakket; S04 |

## Actuele volgende handeling

M01-S03 is geïmplementeerd op m01-eerste-installer: Release-build geslaagd en 75 tests geslaagd, inclusief twee ISCC 7.1.0-compilatieproeven. [Native vertaling, beperkingen en gebruikerscontrole](../docs/slices/M01-S03-Wizardontwerp-export.md). De gebruiker heeft Studio getest, de export gecompileerd en uitgevoerd en op 8 oktober 2026 akkoord gegeven. Deze geaccepteerde slice is gepubliceerd als commit 95c94d3. M01 wordt pas na acceptatie van alle slices gemerged.

M01-S04 is op verzoek geïmplementeerd: compiler ontdekken/kiezen, 7.1-versiecontrole, opgeslagen project bouwen en afzonderlijke uitvoermappen. Release-build geslaagd; 83 tests geslaagd, inclusief drie ISCC-compilatieproeven. [Scope en testinstructies](../docs/slices/M01-S04-Compiler-kiezen-en-bouwen.md). De gebruiker heeft alle teststappen en aanvullende controles doorlopen en de slice geaccepteerd. De geaccepteerde stand is gepubliceerd als commit 33d8c33.

M01-S05 is op verzoek geïmplementeerd: live bouwmeldingen, annuleren, native waarschuwingen/fouten en opgeslagen uitvoernaam (formaat 4). Release-build geslaagd; 106 tests geslaagd, inclusief vijf echte compilatietests met ISCC 7.1.0. [Scope en testinstructies](../docs/slices/M01-S05-Bouwmeldingen-en-annuleren.md). De gebruiker heeft de handmatige tests uitgevoerd en deze slice geaccepteerd. De geaccepteerde stand is gepubliceerd als commit ee2eb8f.

M01-S06 is op verzoek geïmplementeerd: expliciete bescherming van afwijkende exports en een lokale SVG-eindproef met afzonderlijke identiteit. Release-build zonder fouten/waarschuwingen; 108 tests geslaagd, inclusief zelfstandige SVG-compilatie. [Teststappen en afbakening](../docs/slices/M01-S06-Eenvoudige-installer-afronden.md). De gebruiker heeft alle tests afgerond en S06 en de gehele M01-milestone geaccepteerd. S06 is gepubliceerd als 6c84951; M01 is gemerged en main gepusht als e14824e.

M02-S01 is op verzoek geïmplementeerd op m02-installertalen: beschikbare taalbronnen lezen, inspecteren en aanvullende .isl kiezen. Release-build zonder fouten/waarschuwingen; 117 tests geslaagd. Alle geïnstalleerde taalbronnen zijn gelezen en hun hashes blijven gelijk. [Scope en testinstructies](../docs/slices/M02-S01-Beschikbare-taalbronnen.md). De gebruiker heeft de slice geaccepteerd, inclusief vertaalde namen, alfabetische sortering en herkenning van identieke kopieën. Na die aanpassingen slaagden 27 gerichte tests en de Release-build. S02 volgt na opdracht.

M02-S02 is op verzoek geïmplementeerd op m02-installertalen: projecttalen met ingeschakelde status en volgorde, formaat 5 met behoud van oude Nederlandse teksten, Engelse nieuwe projecten en onafhankelijke Voorbeeldtaal. Release-build zonder fouten/waarschuwingen; alle 138 tests geslaagd, inclusief Engelse en bestaande Nederlandse compilatieproeven. Aanvullende talen tonen voorlopig native standaardteksten; eigen vertalingen volgen in S03 en meertalige export in S05. [Scope, tussenstapgrenzen en gebruikerscontrole](../docs/slices/M02-S02-Projecttalen-en-previewkeuze.md). De gebruiker heeft selectie, volgorde, behoud na herstart/heropenen, preview en de gegenereerde installer getest en op 13 oktober 2026 geaccepteerd. De volgende slice is M02-S03, na opdracht.

M02-S03 is op verzoek geïmplementeerd op m02-installertalen: eigen teksten per taal in één eigenschappenpaneel, teller voor ontbrekende actieve vertalingen, herkomst/effectieve tekst en terugval eerst op het gekozen taalbestand, daarna op de eerste actieve taal zonder lege vereiste wizardteksten. Het compacte invoerveld toont een watermark en afwisselend Overnemen/Herstellen. Het hoofdvenster gebruikt het appicoon als vectorresource. Herstellen is taal-/scopegebonden; {AppName}/{AppVersion} volgen applicatiegegevens. Formaat 6 behoudt oudere teksten, talen, volgorde en instellingen. Release-build zonder fouten/waarschuwingen; alle 155 tests geslaagd, inclusief een nieuwe ISCC 7.1.0-proef voor verwijzingen en lege tekst met terugval. [Regels en handmatige teststappen](../docs/slices/M02-S03-Eigen-teksten-per-taal.md). De gebruiker heeft alle situaties, het appicoon en de bovenaan uitgelijnde cursor getest en deze slice geaccepteerd. Het brede vertaaloverzicht volgt in M02-S04 na opdracht; meertalige export blijft voor S05.

M02-S04 is geïmplementeerd op m02-installertalen: een afzonderlijk overzicht met alle projecttalen naast elkaar, live eigen tekstbewerking en Studio-verwijzingswaarschuwingen. Uitgeschakelde talen blijven bewerkbaar; verwijderen vraagt bevestiging en behoudt teksten van resterende talen, ook bij wisselen van oorspronkelijke opslagtaal. Release-build zonder fouten/waarschuwingen; alle 175 tests geslaagd. Na de laatste resource-/vensterwijziging slagen ook de 31 gerichte overzichts- en lokalisatietests. [Scope en handmatige controles](../docs/slices/M02-S04-Vertaaloverzicht.md). De gebruiker heeft het overzicht, ook met veel talen, beoordeeld en M02-S04 geaccepteerd. De volgende slice is M02-S05: installertaalexport en echte taalproef, na opdracht.

M02-S05 is geïmplementeerd op m02-installertalen: alle actieve taalbronnen in projectvolgorde en taalgebonden wizardcode via ActiveLanguage. De S03-terugvalregels blijven gelijk; uitsluitend Nederlands werkt ook met uitgeschakelde oorspronkelijke Engelse opslagtaal. Externe bronpaden worden vanuit het project opgelost; fouten noemen taal en bron. Release-build zonder fouten/waarschuwingen; 187 tests geslaagd, met nieuwe meertalige/RTL-, alleen-Nederlandse, zelfstandige externe-bron- en onvolledige-broncompilatieproeven. [Teststappen en milestonecheck](../docs/slices/M02-S05-Installertaal-export-en-proef.md). De gebruiker heeft alle handmatige tests afgerond, de juiste teksten in het .iss-bestand bevestigd en de installer met verschillende talen gebouwd en uitgevoerd. M02-S05 is geaccepteerd. Alle M02-slices zijn geaccepteerd en gepusht. De gebruiker heeft op 14 oktober 2026 de milestone geaccepteerd; M02 is naar main gemerged en gepusht als dc4ba08.

## Genoteerd voor latere uitwerking

De eerdere gebruikerssuggestie voor verwijzingen naar algemene applicatiegegevens is in M02-S03 verwerkt: {AppName} en {AppVersion} werken in eigen titel-, body-, knop- en tooltipteksten, met dezelfde eenmaal ingevulde letterlijke waarden in preview en bestaande export. Het installatiepad gebruikt zijn bestaande Inno Setup-constanten. Controle van onbekende verwijzingen en volledigheid volgt in S04.

Voor de latere gebruikersdocumentatie (M09-S04): beschrijf eigen .isl-bronnen, LanguageName, de officiële LanguageID of 0 wanneer geen ID bestaat, UTF-8 en LanguageCodePage=0, en het onderscheid tussen een bron toevoegen en een installertaal inschakelen. Vermeld dat een numeriek toegestane ID niet automatisch een geldige Windows-taal-ID is en dat gedeeltelijke overridebestanden nu geen zelfstandige catalogustaal zijn. De gebruiker heeft verzocht deze uitleg later uit te werken.

## Appicoon — afzonderlijke gebruikersopdracht

Het aangeleverde AppIcon.ico is toegevoegd aan Studio.App/Resources, gekoppeld aan het uitvoerbare bestand en de vensters Hoofdscherm, Bouwmeldingen en Installertaalbronnen. AppIcon.svg is ongewijzigd meegenomen als bronbestand voor later documentatiegebruik. De oorspronkelijke bestanden in het oude project zijn behouden. Release-build geslaagd zonder fouten of waarschuwingen; Het icoon is meegenomen in de beoordeelde applicatiestand en afzonderlijk gecommit als 3d44380 bij publicatie van de geaccepteerde M02-S02. Dit verandert de volgorde van de M02-slices niet.

M03-S01 is geïmplementeerd op m03-wizardpaginas: een afzonderlijk alleen-lezen overzicht van alle veertien native wizardfasen met status, reden en directe toegang tot de twee bestaande ontwerppagina’s. Geen projectformaat- of exportwijziging. [Afbakening en handmatige controles](../docs/slices/M03-S01-Standaardpaginaoverzicht.md). De gebruiker heeft het overzicht, de aangepaste vormgeving en de stabiele lijsthoogte getest en geaccepteerd. M03-S01 is gecommit en gepusht als 72e37eb; de volgende invulling is M03-S02, Licentie en informatie.

M03-S02 is geïmplementeerd op m03-wizardpaginas: algemene TXT/RTF-documenten en taalvarianten voor Licentie en Informatie vóór/na installatie, documentvoorbeeld, afgeleide paginastatus en native export. Formaat 7 migreert formaten 1–6; Opslaan als behoudt documentreferenties. [Scope en teststappen](../docs/slices/M03-S02-Licentie-en-informatie.md). De gebruiker heeft de installer en de Engelse variant met algemene terugval getest en bevestigd dat alle talen verschijnen, ook in nieuwe projecten. M03-S02 is geaccepteerd. De volgende slice is M03-S03: Gebruikersinformatie en startmenumap.

M03-S03 is geïmplementeerd op m03-wizardpaginas: gebruikersinformatie met beginwaarden en vorige gegevens, startmenumapbeleid, tijdelijke proefinvoer en native export. Formaat 9 behoudt oudere projecten en afzonderlijke documentstatus. Schakelaars openen de instellingen bij inschakelen en bewaren gegevens bij uitschakelen; dubbelklik/F2 opent bestaande instellingen. Release-build zonder fouten/waarschuwingen; 202 regressietests en daarna 25 gerichte tests geslaagd, inclusief drie ISCC-proeven. [Scope en handmatige controle](../docs/slices/M03-S03-Gebruikersinformatie-en-startmenumap.md). De gebruiker heeft de tests en de aangepaste schakelaarbediening bevestigd; M03-S03 is geaccepteerd.

Genoteerd op gebruikersverzoek voor verdere wizardbediening: toegang tot Licentie en andere configuratiepagina’s vanuit het gewone hoofdscherm, naast het paginaoverzicht. Meenemen bij de navigatie-/wizardproef; geen ad-hoc uitbreiding van M03-S03.

Op gebruikersverzoek voor M08: serienummerveld bij native Gebruikersinformatie met door Studio gegenereerde CheckSerial-validatie. Eerst het gewenste validatiegedrag expliciet configureren; de native pagina toont het veld pas met CheckSerial. Niet opgenomen in M03-S03.

Na de schakelaarwijzigingen: Release-build geslaagd zonder fouten of waarschuwingen; alle 211 tests geslaagd, inclusief behoud van documentbronnen bij uit/aan en migratie van formaat 8. De gebruiker heeft de schakelaars en popupbediening getest en geaccepteerd.

M03-S04 is geïmplementeerd op m03-wizardpaginas: een korte simulatie van de vier afsluitfasen via Fase bekijken, native rolteksten en een read-only samenvatting. Voorbereidingsprobleem en voortgang zijn tijdelijke simulatiewaarden; project, export en native bronbytes blijven gelijk. [Scope en handmatige teststappen](../docs/slices/M03-S04-Gereed-voortgang-en-voltooid.md). De gebruiker heeft deze slice geaccepteerd, inclusief de juiste Voltooid/Finish-tekst en Ja/Nee bij het vervangen van exports. De complete wizardroute volgt in S06.

Volledige regressiecontrole: alle 216 tests geslaagd, inclusief bestaande ISCC-compilatieproeven. De build heeft 0 fouten en 0 waarschuwingen. M03-S04 is door de gebruiker geaccepteerd.

Gebruikerscorrectie bij S04: Exporteer .iss moet een bestaande alternatieve export bewust kunnen vervangen. Expliciete vervangingsvraag en controle van de bevestigde inhoud zijn geïmplementeerd; project-Opslaan als is ook gecontroleerd. De gebruiker heeft de echte installer en projectopslag getest. De gebruiker heeft de Voltooid/Finish-rol en exportcorrectie getest en geaccepteerd.

Na de exportcorrectie: Release-build zonder fouten/waarschuwingen; alle 221 tests geslaagd. Controles omvatten bevestigde vervanging, behoud zonder toestemming, wijziging/verwijdering na bevestiging, bronbescherming en vervangen van een bestaand alternatief .issstudio-project.
