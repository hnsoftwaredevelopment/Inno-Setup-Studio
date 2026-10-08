# Productspecificatie — Inno Setup Studio

Status: geaccepteerd op 8 oktober 2026. De gebruiker heeft productomvang, milestones, slices en werkwijze goedgekeurd. Goedkeuring van dit document is nog geen opdracht om alle milestones achter elkaar uit te voeren.

## 1. Productdoel

Inno Setup Studio is een visuele Windows-werkplek waarmee een softwareontwikkelaar een installer ontwerpt, installatiegedrag instelt en een leesbaar Inno Setup-script genereert en compileert. De uiteindelijke installer en uninstaller worden door Inno Setup uitgevoerd.

De huidige werkplek met pagina's en elementen links, preview in het midden en eigenschappen rechts blijft de basis. Veelgebruikte instellingen zijn visueel te bewerken. Minder gebruikte, gedocumenteerde opties zijn bereikbaar in een geavanceerde instellingenweergave.

**Inno Setup is the limit.** Een instelling is pas productfunctionaliteit als er een gedocumenteerde vertaling naar de ondersteunde Inno Setup-versie is en die vertaling is gecontroleerd. De productscope beslaat de reguliere scriptsecties en officiële uitbreidingsmogelijkheden; niet iedere optie hoeft een eigen visueel scherm te krijgen. Zie de [ondersteuningsmatrix](InnoSetup-ondersteuning.md).

Productnaam en map: Inno Setup Studio. Het bestaande InnoSetupStudio is een afzonderlijk project en blijft behouden.

## 2. Versies en uitgangspunten

- Eerste compilerdoel: Inno Setup 7.1; lokale referentie op 8 oktober 2026: ISCC 7.1.0.
- Windows-app in C# / .NET 10 / WPF. De vereiste Windows-versie van Studio wordt vóór distributie gecontroleerd; dat is een andere eis dan de doelversie van de installer.
- Inno Setup wordt afzonderlijk door de Studio-gebruiker geïnstalleerd. Studio toont compilerlocatie en -versie en biedt een handmatige locatiekeuze.
- De gebruiker van de gemaakte installer heeft Studio of Inno Setup niet nodig.
- Studio heeft een eigen projectformaat en persoonlijke instellingenlocatie.
- Een toekomstige Inno Setup-versie wordt bewust toegevoegd met eigen controles; geen stilzwijgende garantie voor alle latere versies.
- De actuele online handleiding is een onderzoeksbron. De geïnstalleerde 7.1-help en compiler bepalen wat we daadwerkelijk ondersteunen.

Er komen geen extra installatie-engine, eigen installer-runtime, externe schermbibliotheek of verborgen launcher. Officieel Pascal Script mag reguliere Inno Setup-functionaliteit verbinden; het mag geen niet-ondersteunde vormgeving nabootsen. Deze grens is vastgelegd in [ADR-001](decisions/ADR-001-InnoSetup-als-productgrens.md).

## 3. Huidige basis en beoogd eindproduct

| Onderdeel | Nu aanwezig | Nog te realiseren |
|---|---|---|
| Werkplek | Basisontwerp, Welkom en Installatiemap; selectie en zoomen | Volledig paginabeheer en installatieonderdelen |
| Eigenschappen | Teksten, standaardpad, browse-tekst en tooltip | Ondersteunde eigenschappen per pagina en element |
| Overerving | Gedeelde knopteksten en lokale afwijkingen in het Studio-model | Export en werkelijk installerbewijs, inclusief talen en knoprollen |
| Projecten | Openen/opslaan van versie 1 van .issstudio | Productinstellingen, uitbreiding, migraties en undo/redo |
| Taal van Studio | Nederlands, Engels en Duits via resx | Alle nieuwe interfaceonderdelen in deze talen |
| Installertalen | Nederlandse voorbeeldteksten | Taalbestanden, taalkeuze, vertalingen en onafhankelijke previewtaal |
| Uitproberen | Navigatie en tijdelijke mapkeuze | Simulatie van ondersteunde pagina's en keuzes |
| Genereren en bouwen | Nog niet aanwezig | .iss-export, ISCC-koppeling en bouwmeldingen |
| Echte installer | Nog niet geleverd door Studio | Gecontroleerde installatie, update en deïnstallatie |

De eerder gerapporteerde 31 Core-tests en geslaagde Release-build horen bij de proefversie. Ze bewijzen nog geen correcte .iss-export of installer.

## 4. Projecten en bestanden — FR-01

Een project bevat applicatiegegevens, installatieonderdelen, wizardconfiguratie, ingeschakelde talen, vertaalafwijkingen, ondersteunde geavanceerde opties en eventuele eigen scripts. Assets en bronbestanden worden met verwijzingen opgenomen.

De gebruiker kan een nieuw project maken, openen, bewaren en onder een andere naam opslaan. Persoonlijke Studio-instellingen, compilerlocaties en tijdelijke uitprobeerwaarden staan buiten het project. Relatieve paden hebben als basis de projectmap; absolute paden blijven herkenbaar. Verplaatsen van een project maakt ontbrekende bestanden zichtbaar.

Projecten hebben een expliciete formaatversie. Oudere projecten worden gecontroleerd gemigreerd met behoud van gebruikerswaarden; migratie schrijft niet automatisch het oorspronkelijke bestand over. Een project uit een onbekende nieuwere versie wordt afgewezen met uitleg. De bestaande proefprojecten, inclusief hun Nederlandse teksten, moeten blijven werken.

Een ontbrekende resource of ongeldig pad mag bestaande wijzigingen niet vernietigen. Opslaan is atomair. Bij sluiten, vervangen of openen blijft de keuze om wijzigingen te bewaren, te verwerpen of te annuleren beschikbaar. Undo/redo hoort bij modelwijzigingen; een taalwissel of uitprobeeractie maakt geen projectwijziging.

## 5. Applicatie en installatiebeleid — FR-02

De gebruiker stelt productnaam, stabiele applicatie-identiteit, versie, uitgever en relevante metadata in. Een nieuw project bevat bruikbare beginwaarden, maar maakt nooit stilzwijgend een nieuwe applicatie-identiteit bij iedere build.

Het beleid omvat installatiepad, installatie voor de huidige gebruiker of alle gebruikers, doelarchitecturen en ondersteunde Windows-versies. Applicatiearchitectuur, installatiemodus en architectuur van het setup-programma zijn afzonderlijke begrippen.

Architectuurkeuzes worden vertaald naar de daarvoor bestemde richtlijnen; de gebruiker ziet bij ongeldige combinaties welke instelling moet worden aangepast. [ArchitecturesAllowed](https://jrsoftware.org/ishelp/topic_setup_architecturesallowed.htm), [ArchitecturesInstallIn64BitMode](https://jrsoftware.org/ishelp/topic_setup_architecturesinstallin64bitmode.htm) en [SetupArchitecture](https://jrsoftware.org/ishelp/topic_setup_setuparchitecture.htm).

Een nieuwe installatie, een herinstallatie en een update krijgen controlepunten. Studio introduceert geen eigen updateprotocol of automatische updater.

## 6. Bestanden, mappen en snelkoppelingen — FR-03

De gebruiker kan bestanden en bestandsregels toevoegen, bewerken, ordenen en verwijderen. Bron, doel, eventuele nieuwe naam, submappen, uitsluitingen en ondersteunde bestandsopties zijn instelbaar. Een overzicht maakt zichtbaar welke bestanden een wildcard naar verwachting omvat, zonder te doen alsof runtime-bronnen al bekend zijn. [Files](https://jrsoftware.org/ishelp/topic_filessection.htm).

Losse mapregels en snelkoppelingen krijgen eigen bewerking, inclusief relevante metadata en voorwaarden. Gewone doelmappen die uit bestandsregels volgen hoeven niet dubbel te worden ingevoerd. [Dirs](https://jrsoftware.org/ishelp/topic_dirssection.htm) en [Icons](https://jrsoftware.org/ishelp/topic_iconssection.htm).

Runtime-constanten, bijvoorbeeld {app}, blijven als expressie bewaard. Een voorbeeldwaarde in Studio is herkenbaar als voorbeeld en wordt nooit als werkelijke doelcomputerwaarde opgeslagen. [Constanten](https://jrsoftware.org/ishelp/topic_consts.htm).

## 7. Installatietypen, componenten en taken — FR-04

De gebruiker definieert installatietypen, componenten en optionele taken en koppelt regels eraan. Namen die als identifier dienen zijn uniek; beschrijvingen kunnen per installertaal verschillen. [Types](https://jrsoftware.org/ishelp/topic_typessection.htm), [Components](https://jrsoftware.org/ishelp/topic_componentssection.htm) en [Tasks](https://jrsoftware.org/ishelp/topic_taskssection.htm).

Verwijdering van een gebruikte keuze toont de betrokken verwijzingen. Studio mag geen ongeldige verwijzingen genereren. Uitproberen simuleert de gekozen typen, componenten en taken voor zover ze uit het project zijn af te leiden. Het voert de bijbehorende installatiehandelingen niet uit.

## 8. Register, configuratie en acties — FR-05

Registerregels ondersteunen sleutel, waardetype, waarde, relevante opties en architectuur. INI-regels ondersteunen bestand, sectie, sleutel en waarde. De gebruiker kan voorwaarden verbinden aan deze regels. [Registry](https://jrsoftware.org/ishelp/topic_registrysection.htm) en [INI](https://jrsoftware.org/ishelp/topic_inisection.htm).

Uit te voeren programma's krijgen bestandsnaam, parameters, werkmap, beschrijving en ondersteunde uitvoeropties. Installatie- en deïnstallatieacties worden duidelijk onderscheiden. Studio voert deze acties uitsluitend uit via een echt door de gebruiker gestart installerproces. [Run en UninstallRun](https://jrsoftware.org/ishelp/topic_runsection.htm).

Expliciete opruimregels zijn onderdeel van het project. Doelpad en moment worden zichtbaar weergegeven. Deïnstallatiegedrag blijft het gedrag van Inno Setup; Studio belooft geen algemene transacties of herstelmogelijkheid voor willekeurige eigen scripts. [InstallDelete](https://jrsoftware.org/ishelp/topic_installdeletesection.htm) en [UninstallDelete](https://jrsoftware.org/ishelp/topic_uninstalldeletesection.htm).

## 9. Wizardpagina's — FR-06

Het uiteindelijke paginaoverzicht omvat:

| Pagina of fase | Bediening in Studio |
|---|---|
| Welkom | Inhoud en ondersteunde zichtbaarheid |
| Licentie | Document, taalvariant en weergave |
| Wachtwoord | Reguliere Inno Setup-instelling en beschrijving van gedrag |
| Informatie vóór installatie | Document en taalvariant |
| Gebruikersinformatie | Ondersteunde velden en beginwaarden |
| Installatiemap | Standaardpad en ondersteund mapkeuzegedrag |
| Componenten | Afgeleid van typen en componenten |
| Startmenumap | Mapbeleid en relevante snelkoppelingen |
| Taken | Afgeleid van projecttaken |
| Gereed voor installatie | Samenvatting en ondersteunde zichtbaarheid |
| Voorbereiden en installeren | Voorbeeld van status en voortgang |
| Informatie na installatie | Document en taalvariant |
| Voltooid | Teksten en reguliere afsluitacties |

De standaardvolgorde blijft de Inno Setup-volgorde. Niet iedere pagina kan vrij worden toegevoegd, verborgen of verplaatst. Studio toont of een pagina ingeschakeld, uitgeschakeld, voorwaardelijk of onderdeel van de vaste runtime is. Zichtbaarheid wordt afgeleid van instellingen en projectinhoud; een uitprobeerscenario kan een keuze simuleren. Het daadwerkelijke gedrag wordt door Inno Setup bepaald. [Wizardpagina's](https://jrsoftware.org/ishelp/topic_wizardpages.htm).

De taalkeuzedialoog en eventuele Windows-verhogingsdialoog zijn geen vrij ontwerpbare wizardpagina's.

## 10. Basisontwerp en vormgeving — FR-07

Het Basisontwerp verzamelt gedeelde instellingen met hun werkelijke bereik. Afbeeldingen gebruiken de daarvoor bedoelde Inno Setup-plaatsen; een logo kan niet automatisch op iedere pagina op iedere gewenste positie worden gezet.

Studio biedt alleen gecontroleerde stijlen, afbeeldingsinstellingen, afmetingen en elementeigenschappen. De ingebouwde stijlkeuzes komen uit de ondersteunde compilerversie. Een externe stijldefinitie kan later als bestaand bestand worden gekozen als 7.1 dit regulier ondersteunt; het maken van een eigen stijleditor hoort niet bij het product. [WizardStyle](https://jrsoftware.org/ishelp/topic_setup_wizardstyle.htm).

Per eigenschap toont Studio de herkomst: Inno Setup-standaard, Basisontwerp of pagina-afwijking. Een lokale afwijking bestaat alleen waar er een gecontroleerde vertaling is. Een expliciet lege waarde en een ontbrekende afwijking blijven onderscheiden waar de Inno Setup-instelling dat toestaat.

Gedeelde navigatie behandelt Terug, Verder, Installeren, Voltooien en Annuleren als verschillende rollen. Een globale tekst voor Verder mag de installatiestap of afsluitknop niet onbedoeld veranderen. Bij vooruit- en terugnavigeren worden pagina-afwijkingen correct toegepast en hersteld. Gedocumenteerde eigenschappen van WizardForm en gebeurtenissen zijn hiervoor mogelijke reguliere middelen; uitvoering wordt vóór aanbieding gecontroleerd. [Support classes](https://jrsoftware.org/ishelp/topic_scriptclasses.htm) en [events](https://jrsoftware.org/ishelp/topic_scriptevents.htm).

Vrije buttontekstkleur, vervangen van standaardcontrols, Windows-API-trucs en onbeperkte positionering worden niet aangeboden om beperkingen te omzeilen. De huidige prototype-eigenschappen zijn nog geen bewijs dat iedere lokale afwijking exporteerbaar is.

## 11. Studio-taal en installertalen — FR-08

De Studio-interface gebruikt Nederlands, Engels en Duits uit resx-bestanden. Nederlands is de terugvaltaal. Zie [Meertaligheid](Meertaligheid.md).

Installertalen zijn projectgegevens. De gebruiker kiest uit de beschikbare taalbestanden van de gekoppelde Inno Setup-installatie; Studio heeft hiervoor geen vaste lijst van drie talen. Aanvullende compatibele isl-bestanden kunnen als projectresource worden opgenomen. Een ontbrekend of incompatibel taalbestand wordt gemeld.

Voor nieuwe projecten stellen we Engels als begininstelling voor. Een uitsluitend Nederlandstalige installer is geldig. De eerste projecttaal is de terugvaltaal; taaldetectie, een eventuele taalkeuzedialoog en het hergebruiken van een eerdere taal volgen de reguliere Inno Setup-instellingen. [Languages](https://jrsoftware.org/ishelp/topic_languagessection.htm).

Boven de preview komt **Voorbeeldtaal**, met alleen de ingeschakelde installertalen. Deze keuze staat los van **Studio-taal**. Zo kan een Nederlandse Studio een Engelse installer tonen en andersom.

Standaardteksten komen uit de gekozen isl-bron. Gedeelde en lokale afwijkingen zijn per taal opgeslagen. De herkomst is zichtbaar; herstellen verwijdert de afwijking. Taal verwijderen vereist een bewuste keuze over het bewaren of verwijderen van eigen vertalingen.

Tekstbewerking past in het eigenschappenpaneel; een vertaaloverzicht ondersteunt meerdere talen naast elkaar. Placeholders, toegangstoetsen, regelovergangen en vertaalde licentie-/informatiebestanden worden gecontroleerd. Een ontbrekende eigen vertaling wordt zichtbaar, zonder ongemerkt een Nederlandse tekst in iedere andere taal te gebruiken. De gekozen terugvalregel wordt vóór M02-S03 vastgelegd en getest. [Messages](https://jrsoftware.org/ishelp/topic_messagessection.htm) en [CustomMessages](https://jrsoftware.org/ishelp/topic_custommessagessection.htm).

Rechts-naar-links en lettertypeverschillen zijn onderdeel van de controle wanneer zo'n taal wordt ingeschakeld. Een niet-getrouwe preview krijgt een duidelijke beperking; de echte installer blijft het controlemiddel.

## 12. Ontwerpen, uitproberen en de echte installer — FR-09

Ontwerpen selecteert elementen en wijzigt projectwaarden. Uitproberen simuleert navigatie en tijdelijke invoer, met behoud van het ontwerp. Het kopieert geen bestanden, wijzigt geen register en start geen gebruikersscripts, download of installatieacties.

De preview toont wat voorspelbaar is uit het project, met herkenbare voorbeeldwaarden voor runtimegegevens. De gebruiker kan eigen scripts niet volledig testen in de WPF-preview. Bestandskeuzes en invoer in Uitproberen verdwijnen bij verlaten van de sessie.

Een geslaagde build levert een afzonderlijk te starten installer op. Starten gebeurt op expliciete gebruikersactie; installatieproeven en deïnstallatie doet de gebruiker in een passende testomgeving. Overeenkomst van teksten, zichtbaarheid, stijl en navigatie wordt per relevante slice vergeleken.

## 13. Scriptgeneratie en bouwen — FR-10

Studio exporteert een leesbare .iss en benodigde projectresources. Dezelfde projectwaarden leveren dezelfde scriptinhoud, afgezien van bewust ingestelde variabele buildgegevens. Quoting, Unicode, regelovergangen, constanten en sectievoorwaarden worden correct verwerkt.

Het projectmodel is de bron voor visueel beheerde onderdelen. De gegenereerde .iss is uitleesbaar; wijzigingen in die uitvoer worden niet automatisch teruggeschreven naar het visuele project. Een opnieuw genereren overschrijft nooit ongemerkt een handmatig aangepaste uitvoer. De gebruiker krijgt een concrete conflictkeuze.

Studio gebruikt ISCC met afzonderlijke procesargumenten en toont versie, uitvoer, waarschuwingen, fouten en resultaat. Een fout is waar mogelijk te herleiden tot een projectinstelling, regel of scriptlocatie. De log blijft in de compilergeleverde taal; Studio-meldingen volgen de Studio-taal. [Compileropdrachten](https://jrsoftware.org/ishelp/topic_compilercmdline.htm).

Een build is gebaseerd op een herkenbare projectstand. Niet-opgeslagen wijzigingen en latere wijzigingen mogen geen verwarring geven over wat is gebouwd. Een mislukte of afgebroken build mag nooit een oudere executable als nieuw resultaat presenteren. Annuleren stopt de compiler gecontroleerd.

De export kan buiten Studio met Inno Setup 7.1 worden gecompileerd. Absolute of externe afhankelijkheden worden vooraf zichtbaar gemaakt; zelfstandigheid vereist de genoemde bronbestanden, scripts en assets, niet alleen de .iss.

## 14. Geavanceerde reguliere mogelijkheden — FR-11

Geavanceerde instellingen bieden de overige niet-verouderde 7.1-richtlijnen en sectieparameters, met hun type, bereik, onderlinge beperkingen en hulpverwijzing. Een register van gecontroleerde instellingen bepaalt de dekking. Een optie heeft één eigenaar; een geavanceerde invoer mag een visueel beheerde waarde niet stil overschrijven.

Dit bereik omvat ook reguliere uitvoer- en compressieopties, schijfverdeling, herstart- en bestandsgebruikbeleid, versiemetadata, encryptie, bestandsverificatie en native download-/archiefopties. Een optie wordt pas actief na versiecontrole en verificatie; de matrix maakt zichtbaar wat nog gepland is.

Ondertekening koppelt aan het normale compilerproces en door de gebruiker ingerichte hulpmiddelen. Studio biedt geen eigen certificaatbeheer of ondertekeningsdienst. Certificaten, wachtwoorden en sleutels mogen niet onbedoeld in projectbestanden of Git terechtkomen. [SignTool](https://jrsoftware.org/ishelp/topic_setup_signtool.htm). Publieke verificatiesleutels volgen de daarvoor bestemde sectie. [ISSigKeys](https://jrsoftware.org/ishelp/topic_issigkeyssection.htm).

## 15. Officiële scripting en eigen pagina's — FR-12

Het voorstel omvat een editor voor eigen Pascal Script en reguliere preprocessorinvoer, met gecontroleerde invoegpunten, foutlocaties en behoud bij genereren. Pascal Script is bedoeld voor runtimegedrag; de preprocessor voor compileertijd. Dit levert geen belofte op dat Studio elke scriptconstructie visueel kan begrijpen of simuleren. [Pascal Script](https://jrsoftware.org/ishelp/topic_scriptintro.htm) en [preprocessor](https://jrsoftware.org/ishelp/topic_isppoverview.htm).

Als eerste visuele uitbreiding gebruiken eigen pagina's de standaard Inno Setup-functies voor invoer van tekst, opties, mappen, bestanden en informatie. Zij krijgen een ondersteund invoegpunt in de bestaande wizard. De benodigde kleine scripts mogen worden gegenereerd met gedocumenteerde API's. [Eigen wizardpagina's](https://jrsoftware.org/ishelp/topic_scriptpages.htm).

Het visuele beheer van volledig lege custompagina's met willekeurige controls is geen onderdeel van de eerste productversie. Eigen code blijft mogelijk binnen de afgesproken Inno Setup-grens, maar krijgt geen automatische previewgarantie. Studio levert geen DLL-uitbreidingen, dependency-downloader-framework of eigen debugger. De reguliere Inno Setup-IDE kan voor debugging worden gebruikt.

## 16. Gebruikskwaliteit en validatie — FR-13

De werkplek blijft herkenbaar. Acties hebben duidelijke namen, toetsenbordbediening, zichtbare selectie en voldoende ruimte voor Nederlandse, Engelse en Duitse teksten. Schalen en kleinere vensters mogen geen noodzakelijke bediening onbereikbaar maken.

Validatie onderscheidt blokkerende fouten en waarschuwingen. Meldingen noemen de betrokken instelling en herstelactie. Studio controleert projectstructuur, referenties, bestanden, versiecompatibiliteit, vertaalplaceholders en evidente ongeldige combinaties. ISCC blijft het laatste controlepunt voor de geldigheid van scripts.

Geen doel voor een willekeurig percentage testdekking. Gerichte tests bewijzen modelregels, migraties en generatie. De gebruiker controleert de zichtbare ervaring en de echte installer.

## 17. Buiten scope — FR-14

- Een eigen installatie-engine, eigen installer-runtime of oplossingen buiten de gedocumenteerde Inno Setup-grens.
- Een clone van alle functies van Advanced Installer of een oplossing voor ieder Windows-installatieprobleem.
- Automatisch teruglezen van willekeurige .iss-bestanden naar het visuele model.
- Import uit het oude InnoSetupStudio zonder afzonderlijke afspraak.
- Een Visual Studio-extensie, cloudbuilddienst, samenwerking in één project of pluginplatform.
- Automatische dependency-installatie, updatecontrole en distributiedienst als apart Studio-framework.
- Een eigen editor voor externe stijlen of een volledige Pascal-debugger.

Inno Setup-import en een Visual Studio-extensie blijven mogelijke uitbreidingen na de hoofdplanning. Ze zijn geen vereiste voor productoplevering.

## 18. Oplevercriteria

Het beoogde product is klaar wanneer een gebruiker een project kan bewaren, een representatieve installer visueel configureren, talen en ondersteunde afwijkingen instellen, een zelfstandige export bouwen en het resultaat buiten Studio gebruiken.

De definitieve controle omvat: een nieuw project; het openen van een proefproject; een Nederlandstalige en een meertalige installer; componenten/taken; register/configuratie; ten minste een eigen standaardinvoerpagina; een passende architectuurkeuze; een update; een deïnstallatie; compilerfouten en ontbrekende resources; herstart van Studio met behoud van persoonlijke instellingen.

De [milestones](Milestones.md) organiseren dit einddoel. De [werkwijze](Werkwijze.md) bepaalt hoe een slice wordt gebouwd, gecontroleerd en geaccepteerd. De uiteindelijke dekking wordt aantoonbaar gemaakt in de ondersteuningsmatrix.

## 19. Geaccepteerde uitgangspunten

De gebruiker heeft deze uitgangspunten bij M00-S01 geaccepteerd:

1. Veelgebruikte mogelijkheden visueel; alle overige reguliere, niet-verouderde 7.1-opties via een gecontroleerde geavanceerde weergave.
2. Officiële Pascal Script-/preprocessorinvoer en standaard eigen invoerpagina's in de hoofdplanning; een volledig vrije custom-controlontwerper buiten de eerste versie.
3. Engels als begininstallertaal voor nieuwe projecten; Nederlandse proefprojecten behouden hun inhoud.
4. Volgorde: eerst een echte eenvoudige installer, vervolgens installertalen, standaardpagina's en verdere installatieonderdelen.
