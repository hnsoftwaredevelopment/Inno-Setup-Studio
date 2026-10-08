# M02-S01 — Beschikbare taalbronnen

Status: geïmplementeerd en door de gebruiker geaccepteerd op 8 oktober 2026. Branch `m02-installertalen`, gestart vanaf geaccepteerde en gemergede M01 (`e14824e`). Scope: FR-08 en bronbeheer uit FR-01/FR-10. Installertalen kiezen en previewtaal volgen in S02; eigen vertalingen in S03.

## Resultaat en acceptatiecriteria

Via **Compiler en bouwen → Installertaalbronnen** opent een bronoverzicht. Studio leest na 7.1-versiecontrole `Default.isl` en de `.isl`-bestanden uit de `Languages`-map naast de gekozen compiler. Links staan de taalnamen in de geselecteerde Studio-taal (NL/EN/DE), met de oorspronkelijke naam daaronder wanneer die afwijkt, rechts het volledige pad, LanguageID, rechts-naar-links-vlag en aantallen berichten. De tabel toont LangOptions, Messages en CustomMessages met sleutel en waarde, alleen-lezen.

1. De standaard Engelse bron en gevonden talen worden ingelezen zonder bronwijziging; de lijst is niet beperkt tot NL/EN/DE.
2. Een gebruiker kan aanvullende `.isl`-bestanden kiezen en opnieuw inlezen. Dubbelen van hetzelfde volledige pad verschijnen niet nogmaals.
3. Ontbrekende, onleesbare of ongeldige bronnen geven een melding met betrokken pad. Geldige bronnen blijven beschikbaar wanneer een andere bron faalt.
4. Het project, de huidige wizardteksten, installerexport en Studio-taal blijven ongewijzigd door broninspectie.

Extra bronpaden worden voor de huidige Studio-sessie onthouden, ook na sluiten/heropenen van het overzicht. Ze zijn nog geen opgeslagen of ingeschakelde projecttalen. Projectopslag blijft formaat 4. Bij een compilerwissel gebruikt een nieuw geopend overzicht de nieuw gekozen compilerlocatie.

## Parsing en grenzen

De lezer verwerkt drie reguliere secties, commentaarregels met `;`, sleutel/waarde-regels en lege waarden. Sleutels en sectienamen worden zonder hoofdlettergevoeligheid gelezen. Bij dezelfde sleutel binnen een sectie geldt de laatste waarde. De waarde wordt na het eerste `=` gelezen; extra `=`, semicolons en placeholders blijven tekst. `%n`, `%1`, `[name]` en andere placeholders worden hier niet uitgebreid of vertaald. De tabel is broninspectie, geen installerpreview.

UTF-8 met of zonder BOM wordt ondersteund. Voor oudere niet-UTF-8-bestanden gebruikt Studio een expliciet opgegeven ondersteunde `LanguageCodePage`; een impliciete systeemcodepage wordt niet geraden. Native bronbestanden worden nooit herschreven. Maximum: 1 MB per bestand.

Een zelfstandige catalogustaal vereist LanguageName, een numerieke LanguageID (hexadecimaal met `$` of decimaal, 0–65535) en ten minste één Messages-regel. RightToLeft is `yes` of `no`, met `no` als terugval. Gedeeltelijke overridebestanden zonder eigen taalidentiteit zijn nog geen zelfstandige catalogustalen. De parser certificeert niet iedere sleutel, ontbrekende vertaling of placeholdercombinatie als compilercompatibel; verdere projectvalidatie en compilatie volgen in latere slices.

Preprocessorregels en `{#...}` worden gemeld als nog niet ondersteund. Studio voert geen code of includes uit tijdens broninspectie. De genoemde grenzen zijn zichtbaar als bronmelding, niet als stil verdwenen taal.

Native naslag: [Languages](https://jrsoftware.org/ishelp/topic_languagessection.htm), [LangOptions](https://jrsoftware.org/ishelp/topic_langoptionssection.htm). Werkelijk bronbewijs: de taalbestanden bij de lokale Inno Setup **7.1.0**-installatie. De online handleiding kan later zijn; projecttaalexport wordt pas in latere slices gebouwd.

## Verificatie

De tests controleren letterlijke placeholders, lege teksten, semicolons en extra gelijktekens, UTF-8/BOM en expliciete oudere codepage, ongeldige/codehoudende bestanden, groottebegrenzing en afzonderlijke meldingen bij gedeeltelijk mislukte ontdekking. Een extra test leest alle taalbestanden van de lokale compiler en vergelijkt SHA-256 vóór/na om te bewijzen dat bronnen ongewijzigd blijven. De volledige Release-build slaagt zonder fouten of waarschuwingen. Alle 117 regressietests slagen, zonder overgeslagen tests; de geïnstalleerde-bronnenproef is geslaagd. De GUI-controle blijft aan de gebruiker.

Review: broninhoud wordt als data verwerkt, buffers zijn per bestand begrensd, taalbronnen zijn alleen-lezen en buiten het projectmodel gehouden. Een fout in één bron blokkeert niet de overige talen. Geen installer of taalbroncode wordt uitgevoerd door het catalogusvenster; alleen de bestaande compiler-versiecontrole wordt gebruikt.

## Handmatig testen

1. Start de Release-app. Open **Compiler en bouwen → Installertaalbronnen**. Controleer onder andere Engels, Nederlands en Duits (in de Nederlandse Studio-interface), inclusief verwijzing naar Default.isl respectievelijk Dutch.isl/German.isl.
2. Selecteer een taal. Bekijk LanguageID, de RTL-vlag en bijvoorbeeld WelcomeLabel1, WelcomeLabel2 en ButtonNext. Placeholders horen letterlijk zichtbaar te zijn. Het bestand en de teksten zijn niet bewerkbaar in dit venster.
3. Kopieer voor een veilige bronproef een eigen `.isl` naar een testmap en voeg die toe. Dezelfde bron opnieuw toevoegen mag geen dubbele rij geven. Sluit/heropen het overzicht: de bron blijft in deze sessie bekend.
4. Voeg een ongeldig testbestand met `.isl`-extensie toe, bijvoorbeeld met alleen `geen taalbestand`. Verwacht een pad en uitleg; de oorspronkelijke talen blijven zichtbaar. Verwijder de gekozen kopie buiten Studio en klik Opnieuw inlezen: ontbrekende bron hoort als melding te verschijnen. Pas de geïnstalleerde bronbestanden niet aan.
5. Controleer dat openen, selecteren en opnieuw inlezen geen projectwijzigingsmarkering veroorzaken en huidige wizardteksten niet veranderen. Wissel Studio-taal na sluiten van het venster en heropen: de nieuwe labels volgen NL/EN/DE, de bronwaarden behouden hun oorspronkelijke taal.

M02-S01 is na gebruikerscontrole geaccepteerd, inclusief de vertaalde taalnamen, sortering en inhoudelijke herkenning van bronkopieën. Broninspectie verandert nog geen installer- of previewtaal.

## Aanpassing na gebruikersreview

De lijst gebruikt nu via LanguageID gekoppelde taalnamen uit de Studio-resx. Alle 33 taalbronnen van de lokale Inno Setup-installatie hebben namen in NL/EN/DE; de oorspronkelijke LanguageName blijft zichtbaar en ongewijzigd. Onbekende IDs, waaronder 0, vallen terug op de oorspronkelijke naam. Chinese en Portugese varianten hebben onderscheidende namen. Extra vertaalde namen kunnen via ResXManager worden toegevoegd met sleutel InstallerLanguageName_ gevolgd door de viercijferige hexadecimale taal-ID. Dit wijzigt geen installerinhoud. Controleer bijvoorbeeld Koreaans/Korean/Koreanisch door Studio-taal te wisselen en het venster opnieuw te openen.

De reguliere Release-map was tijdens deze aanpassing bezet door de draaiende Studio-app. De nieuwe testbuild staat daarom in Builds/LanguageNames. De gerichte taalbron-, naam- en resourcecontroles zijn bij oplevering uitgevoerd; de eerdere volledige regressie van 117 tests blijft historisch bewijs voor de vorige stand.

Na sluiten van Studio door de gebruiker is de reguliere Release-build opnieuw uitgevoerd en geslaagd zonder fouten of waarschuwingen. De aangepaste taalnamen zijn nu ook beschikbaar in Builds/Release; de gerichte 25 tests waren al geslaagd voor dezelfde code.

## Sortering en identieke kopieën

Na verdere gebruikersreview wordt de lijst alfabetisch gesorteerd op de vertaalde taalnaam, volgens de gekozen Studio-taal. Inhoudelijk identieke bronnen worden één keer getoond, ook wanneer ze op verschillende paden staan. De eerste bron blijft zichtbaar: de compilerbron heeft daardoor voorrang op een toegevoegde identieke kopie. De kopie wordt niet verwijderd of gewijzigd. De vergelijking omvat LangOptions, Messages en CustomMessages; gewijzigde teksten, taalnaam of ID blijven een afzonderlijke bron. Paden van extra bronnen blijven bekend en worden bij Opnieuw inlezen opnieuw gelezen. Alle 27 gerichte taalbron-, naam- en lokalisatietests slagen. De actuele versie is gebouwd in Builds/LanguageNames zonder fouten of waarschuwingen.

## Acceptatie

De gebruiker heeft de aangepaste lijst getest en de slice geaccepteerd. Verificatie bij de definitieve stand: 27 gerichte taalbron-/naam-/lokalisatietests geslaagd en een Release-build zonder fouten of waarschuwingen. De eerdere volledige regressie van 117 tests is niet als een nieuwe volledige controle van de aangepaste stand gepresenteerd. Uitleg over eigen taalbestanden is op verzoek genoteerd voor de latere gebruikersdocumentatie.
