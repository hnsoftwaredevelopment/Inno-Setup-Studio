# Milestones — Inno Setup Studio

Status: geaccepteerd op 8 oktober 2026. Geen tijdschema of automatische bouwopdracht. De gebruiker kiest telkens welke milestone we oppakken.

De technische milestonenaam is exact de branchnaam. De titel achter die naam beschrijft het resultaat. Slice-ID's staan in [tasks/todo.md](../tasks/todo.md). Elke milestone wordt afgerond volgens [Werkwijze](Werkwijze.md).

## Overzicht

| Naam / branch | Resultaat | Afhankelijk van | Functie-eisen |
|---|---|---|---|
| m00-productspecificatie | Productgrens, werkwijze en planning geaccepteerd | Huidige proefversie | Alle |
| m01-eerste-installer | Een klein project levert een echte, zelfstandig compileerbare installer op | M00 | FR-01, 02, 03, 07, 09, 10 |
| m02-installertalen | Installertalen en previewtaal zijn onafhankelijk van Studio-taal | M01 | FR-01, 08, 10 |
| m03-wizardpaginas | Standaardpagina's en hun gedrag zijn passend te configureren | M02 | FR-06, 07, 09 |
| m04-installatiekeuzes | Bestanden, mappen, snelkoppelingen, typen, componenten en taken vormen een geheel | M03 | FR-03, 04, 06 |
| m05-systeemintegratie | Architectuur, rechten, configuratie en installatieacties zijn instelbaar | M04 | FR-02, 05, 10 |
| m06-vormgeving | Gecontroleerde native vormgeving en gedeelde instellingen werken in echte installers | M03, na M05 in voorgestelde volgorde | FR-07, 09, 13 |
| m07-geavanceerde-opties | Overige reguliere 7.1-opties krijgen gecontroleerde bediening en export | M05, M06 | FR-11, 13 |
| m08-scripting | Eigen code en standaard eigen invoerpagina's passen veilig in de generatie | M02, M07 | FR-12, 10 |
| m09-productoplevering | Samenhang, compatibiliteit, gebruikerskwaliteit en distributie zijn gecontroleerd | M01–M08 | FR-01, 13 en eindcriteria |

De proefversie is bestaande basis en krijgt geen fictieve afgeronde productmilestone. De twee bestaande taalcommits worden vóór publicatie bewust behandeld.

## M00 — m00-productspecificatie

Resultaat: een beoordeelbare productspecificatie, matrix van Inno Setup-koppelingen en een vaste ontwikkelwerkwijze.

Slices: S01 specificatie en grenzen; S02 milestones, slices en acceptatieproces.

Eindcriteria: gebruiker heeft de productomvang en voorstellen geaccepteerd; documentatie is gespiegeld; de omgang met de twee bestaande lokale commits is afgesproken vóór push. Deze milestone wijzigt geen applicatiecode.

## M01 — m01-eerste-installer

Resultaat: vanuit Studio een eenvoudige installer met applicatiegegevens, een lokale bestandsregel, de huidige twee ontwerppagina's en gecontroleerde tekstafwijkingen genereren en bouwen.

Slices: S01 projectgegevens; S02 eenvoudige bestandsexport; S03 vertaling van huidig ontwerp; S04 compilerkeuze en bouwen; S05 bouwresultaat en foutafhandeling; S06 eindproef en zelfstandige export.

Eindcriteria: export compileert met 7.1.0 binnen én buiten Studio; ontbrekende bronnen en compilerfouten zijn begrijpelijk; een build toont de gebruikte projectstand; de gebruiker heeft een verse installatie en deïnstallatie van het testproject gecontroleerd.

Dit is het vroege controlepunt voor de technische haalbaarheid van de huidige preview. De eerste export gebruikt een expliciete taal die bij de huidige projectinhoud past; nieuwe Engelse beginwaarden en een volledige taalkeuze volgen in M02.

M01 is als geheel door de gebruiker geaccepteerd na de eindproef van S06. De gebruiker heeft opdracht gegeven voor merge naar main en start van M02.

## M02 — m02-installertalen

Resultaat: talen uit Inno Setup kiezen, eigen vertalingen bewaren, de previewtaal apart wisselen en dezelfde talen in de echte installer krijgen.

Slices: S01 taalbronnen; S02 projecttalen en migratie; S03 taalgebonden teksten; S04 vertaaloverzicht en controles; S05 taalexport en echte taalproef.

Eindcriteria: alleen Nederlands werkt; een meertalig project werkt; nieuwe projecten beginnen Engels; Nederlandse proefprojecten behouden hun inhoud; Studio-taal wijzigen maakt het project niet vuil; herstellen en ontbrekende vertalingen hebben voorspelbaar gedrag.

De talenlijst is geen vaste Nederlandse/Engelse/Duitse selectie. Bij een rechts-naar-links-taal wordt de kwaliteit van de preview expliciet getest of als beperking aangegeven.

## M03 — m03-wizardpaginas

Resultaat: het standaardpaginaoverzicht is gebaseerd op de werkelijke wizard. De gebruiker bewerkt alleen relevante, ondersteunde eigenschappen.

Slices: S01 paginaoverzicht en voorwaarden; S02 licentie en informatie; S03 gebruikersinformatie en startmenumap; S04 gereed/voortgang/voltooid en knoprollen; S05 gedeelde/lokale afwijkingen; S06 navigatieproef.

Eindcriteria: standaardvolgorde blijft geldig; documenten en vertalingen werken; voorwaardelijke pagina's zijn herkenbaar; terugnavigeren herstelt juiste teksten; Installeren en Voltooien behouden hun rol.

Component- en takenpagina's krijgen hun gegevens in M04. Wachtwoordbeleid volgt in M07. Runtimefasen krijgen een herkenbare simulatie in plaats van werkelijke handelingen.

## M04 — m04-installatiekeuzes

Resultaat: een installer kan verschillende onderdelen en optionele acties aanbieden, inclusief passende bestanden en snelkoppelingen.

Slices: S01 bestandsverzamelingen; S02 mapregels; S03 snelkoppelingen; S04 installatietypen; S05 componenten; S06 taken; S07 koppelingen en keuzetest.

Eindcriteria: kiezen verandert de juiste inhoud van de installer; een verwijderde identifier laat geen kapotte verwijzingen achter; startmenumap, component- en takenpagina komen overeen met het project; alle keuzes worden bewaard.

## M05 — m05-systeemintegratie

Resultaat: reguliere systeemintegratie en installatie-/deïnstallatieacties zijn configureerbaar en correct gegenereerd.

Slices: S01 doelarchitecturen; S02 rechten en doelpaden; S03 register; S04 INI; S05 programma-acties; S06 opruimregels; S07 update- en deïnstallatieproef.

Eindcriteria: geschikte architectuurcombinaties compileren; rechten en paden passen bij elkaar; gekozen componenten/talen beïnvloeden regels; de gebruiker controleert nieuwe installatie, update en deïnstallatie in een geschikte testomgeving.

Beschikbare testmachines bepalen welke runtimevarianten daadwerkelijk bewezen zijn. Een geslaagde x64-proef telt niet automatisch als een geslaagde Arm64-proef.

## M06 — m06-vormgeving

Resultaat: ondersteunde stijl- en afbeeldingskeuzes zijn visueel te kiezen en komen aantoonbaar overeen met Inno Setup.

Slices: S01 bewezen eigenschappencatalogus; S02 afbeeldingen; S03 native stijlen; S04 afmetingen en lokale scope; S05 vergelijking en toetsenbordcontrole.

Eindcriteria: de gebruiker kan bruikbare branding instellen; uitsluitend gecontroleerde properties zijn bewerkbaar; globale instellingen worden niet als vrije pagina-afwijking aangeboden; beperkingen van preview, Windows/DPI en stijlen zijn zichtbaar.

De milestone mag instellingen schrappen uit het voorstel als controle uitwijst dat ze niet regulier werken; dat gebeurt met uitleg en bijgewerkte matrix.

## M07 — m07-geavanceerde-opties

Resultaat: de overige reguliere niet-verouderde 7.1-instellingen zijn bereikbaar zonder voor iedere optie een speciaal scherm te maken.

Slices: S01 volledige 7.1-catalogus; S02 geavanceerde projectrichtlijnen; S03 geavanceerde regelparameters; S04 build-/runtimebeleid; S05 native downloads en archieven; S06 signing en verificatie; S07 controle van dekking.

Eindcriteria: iedere in-scope richtlijn/parameter heeft een geregistreerde eigenaar, versie, type, export en verificatie; conflicten met visuele velden zijn onmogelijk of expliciet; ontbrekende dekking staat niet ongemerkt als ondersteund gemarkeerd.

Deze milestone is het breedste onderdeel. De catalogus bepaalt bij oppakken de definitieve kleine vervolgslices. Zonder volledige catalogus is dit geen belofte dat één generiek invoerveld alles correct kan verwerken.

## M08 — m08-scripting

Resultaat: de gebruiker kan officiële scripts uitbreiden en standaard eigen invoerpagina's visueel toevoegen.

Slices: S01 eigen codebron; S02 event-/generatieconflicten; S03 eigen preprocessorinvoer; S04 tekst- en informatiepagina's; S05 keuze-/map-/bestandspagina's; S06 integratieproef.

Eindcriteria: genereren behoudt eigen code; eigen en gegenereerde events worden correct gecombineerd; fouten verwijzen naar een herkenbare bron; eigen pagina's gebruiken native API's en hun ondersteunde invoegpunten; de preview voert scripts niet uit.

Een brede parser, volledige debugger en onbeperkte vrije-controlontwerper horen niet bij deze milestone.

## M09 — m09-productoplevering

Resultaat: de losse mogelijkheden vormen een bruikbaar, gedocumenteerd product.

Slices: S01 undo/redo; S02 compatibiliteit en herstelpaden; S03 toegankelijkheid/schaal/talen; S04 volledige regressie en gebruikershandleiding; S05 distributie en releasecontrole.

Eindcriteria: alle productcriteria en resterende matrixpunten zijn beoordeeld; bestaande projecten blijven bruikbaar; representatieve installers zijn getest; uitgiftepakket en runtimevereisten zijn duidelijk; er zijn geen onverklaarde blokkerende fouten.

Een GitHub-release publiceren, een tag plaatsen of de branch verwijderen is een afzonderlijke afspraak bij oplevering; deze planning voert dat niet alvast uit.

## Optioneel na de hoofdplanning

Inno Setup-import en een Visual Studio-extensie worden alleen als afzonderlijk vervolgproject/milestone uitgewerkt na een expliciete keuze. Zij horen niet bij de huidige eindcriteria.

## Herplannen

Een milestone mag worden gesplitst als een slice te breed blijkt. ID's en oude besluiten blijven traceerbaar; scope, afhankelijkheden en eindcriteria worden eerst bijgewerkt. Bij een technisch onhaalbare property geldt de Inno Setup-grens, niet de wens om koste wat kost de preview te behouden.
