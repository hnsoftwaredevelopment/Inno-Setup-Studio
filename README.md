# Inno Setup Studio

Een afzonderlijke visuele Windows-ontwerper als add-on voor **Inno Setup 7.1**. Het is geen add-on voor de bestaande InnoSetupStudio.

De bestaande variant in `C:\Devops\hnsoftwaredevelopment\InnoSetupStudio\InnoSetupStudio` blijft behouden. Dit nieuwe project krijgt directe elementselectie, een zichtbaar Basisontwerp, contextuele eigenschappen en afzonderlijke ontwerp- en uitprobeermodi.

Het doel is dat Studio `.iss`-scripts genereert die met de lokaal geïnstalleerde Inno Setup 7.1-compiler kunnen worden gebouwd, rechtstreeks of vanuit Studio. Inno Setup verzorgt de uiteindelijke installerinterface en installatiehandelingen.

## Status

De eerste WPF-werkplek werkt: Basisontwerp, Welkom en Installatiemap, direct selecteerbare elementen, gedeelde knopteksten met pagina-afwijkingen, een uitprobeermodus en opslaan/openen van `.issstudio`-projecten. Installatiemap, Bladeren-tekst en tooltip zijn instelbaar. Een mapkeuze in Uitproberen wijzigt het opgeslagen standaardpad niet.

De Studio-interface is beschikbaar in Nederlands, Engels en Duits. Rechtsboven wisselt **Studio-taal** direct van taal; de keuze wordt per Windows-gebruiker onthouden. Installerinhoud blijft onafhankelijk van deze keuze. Zie [meertaligheid en ResXManager](docs/Meertaligheid.md).

Het uitklapbare paneel **Applicatiegegevens** boven de ontwerpruimte bevat productnaam, AppId en versie. Die worden met het project opgeslagen. Oude projecten worden bij openen in het geheugen bijgewerkt en pas bij opslaan naar het nieuwe formaat geschreven. Zie [M01-S01 en teststappen](docs/slices/M01-S01-Applicatiegegevens.md).

Het paneel **Bestandsregels** opent via **Bestanden beheren** een overzicht van lokale bestandsregels. Iedere regel heeft een bronbestand of wildcard, een doel onder {app}, een keuze voor submappen en uitsluitingen. Een automatische voorbeeldlijst toont de geselecteerde regel. **Exporteer .iss** en **Compiler en bouwen** gebruiken alle regels, actieve projecttalen en ingestelde wizardpagina’s. Oude projecten migreren bij openen; opslaan gebruikt nu formaat 14 (inclusief mapregels, snelkoppelingen, installatietypen en componenten). [M04-S01 en teststappen](docs/slices/M04-S01-Bestandsverzamelingen.md).

Het beoogde eindproduct is beschreven in de geaccepteerde [productspecificatie](docs/Productspecificatie.md).

## Bouwen en starten

Vereist: Windows en de .NET 10 SDK voor bouwen; .NET 10 Desktop Runtime voor het starten van de gebouwde applicatie.

```powershell
dotnet build '.\Inno Setup Studio.slnx' -c Release
dotnet test '.\Inno Setup Studio.slnx' -c Release
dotnet run --project '.\src\Studio.App\Studio.App.csproj'
```

De Release-app staat in `Builds\Release\Inno Setup Studio.exe`. Distributie vereist de volledige uitvoermap, inclusief de `en`- en `de`-submappen. Inno Setup is voor de huidige ontwerpwerkplek nog niet nodig; bouwen vanuit Studio gebruikt de apart geïnstalleerde Inno Setup 7.1.

## Verificatie

Op 8 oktober 2026: 117 tests geslaagd, waaronder zes echte compilatieproeven met ISCC 7.1.0; Release-build zonder fouten of waarschuwingen. De gebruiker heeft M01-S02 volledig getest en geaccepteerd. Ook M01-S03 is door de gebruiker getest in Studio en in de gecompileerde, uitgevoerde installer en geaccepteerd. M01-S04 is door de gebruiker volledig getest en geaccepteerd. M01-S05 voegt live bouwmeldingen, annuleren en een opgeslagen uitvoernaam toe. Nieuwe projecten beginnen met mysetup; oude projecten behouden setup met een waarschuwing. Formaat 4 bewaart de naam en leest de eerdere formaten. M01-S05 is door de gebruiker handmatig getest en geaccepteerd. Zie [M01-S05 en teststappen](docs/slices/M01-S05-Bouwmeldingen-en-annuleren.md). M01-S01 en de eerdere Studio-taalbasis zijn eveneens geaccepteerd.

De compilatietests gebruiken de standaardinstallatie van Inno Setup 7. Als die ontbreekt, worden de compilerafhankelijke tests met een expliciete reden overgeslagen. Ze bouwen een installer, maar starten die niet. De andere tests vereisen geen compiler.

M01-S06 introduceerde bescherming voor bestaande afwijkende .iss-bestanden. Op gebruikersverzoek kan Exporteer .iss deze nu na expliciete bevestiging vervangen; zonder akkoord blijven ze behouden. Een lokale SVG-eindproef staat in samples/SVGViewerDemo/SVG Viewer M01-proef.issstudio; de gebruiker heeft de eindproef uitgevoerd en M01 volledig geaccepteerd. Dit project installeert één bestand; volledige bestandsverzamelingen volgen in M04. Zie [M01-S06 en eindproef](docs/slices/M01-S06-Eenvoudige-installer-afronden.md). De aanvullende SVG-compilatietest vereist de lokaal aangeleverde executable; bij ontbreken wordt die test met uitleg overgeslagen.

M02-S01 biedt via Compiler en bouwen het venster Installertaalbronnen: alleen-lezen bronoverzicht van Default.isl en gevonden of zelf gekozen taalbestanden. [Scope en teststappen](docs/slices/M02-S01-Beschikbare-taalbronnen.md). M02-S01 is na gebruikerscontrole geaccepteerd, inclusief vertaalde/sorteerbare taalnamen en het samenvoegen van identieke bronkopieën.

M02-S02 voegt opgeslagen projecttalen, volgorde en inschakelen toe, met een aparte Voorbeeldtaal boven de wizard. Nieuwe projecten beginnen Engels; bestaande projecten houden hun Nederlandse teksten. Aanvullende talen tonen voorlopig standaardberichten; eigen vertalingen volgen in S03 en meertalige export in S05. M02-S02 is op 13 oktober 2026 door de gebruiker geaccepteerd. [Werking en teststappen](docs/slices/M02-S02-Projecttalen-en-previewkeuze.md).

## Documentatie en planning

M02-S03 voegt eigen teksten per installertaal toe in het bestaande eigenschappenpaneel. Ontbrekende of lege eigen vertalingen gebruiken eerst het taalbestand van de gekozen taal, daarna de standaardtaal; de teller blijft de ontbrekende eigen vertalingen melden. Eén invoerveld toont de effectieve tekst als grijze watermark zolang het leeg is. Daaronder staat óf Overnemen óf Herstellen. {AppName} en {AppVersion} verwijzen naar applicatiegegevens. Deze slice is door de gebruiker geaccepteerd; export voor alle actieve talen is uitgewerkt in S05. [Terugvalregels en teststappen](docs/slices/M02-S03-Eigen-teksten-per-taal.md).

**Inno Setup is the limit.** De productdekking volgt reguliere instellingen en gedocumenteerde native mogelijkheden van de ondersteunde Inno Setup-versie.

| Document | Doel |
|---|---|
| [Productspecificatie](docs/Productspecificatie.md) | Eindproduct, huidige basis, grenzen en reviewpunten |
| [Ondersteuningsmatrix](docs/InnoSetup-ondersteuning.md) | Koppeling naar Inno Setup en werkelijk verificatieniveau |
| [Milestones](docs/Milestones.md) | Resultaten en afhankelijkheden per milestone |
| [Slices](tasks/todo.md) | Testbare stappen en acceptatiestatus |
| [Ontwikkelplan](tasks/plan.md) | Technische richting en risico's |
| [Werkwijze](docs/Werkwijze.md) | Review, tests, commit/push en merge |
| [Meertaligheid](docs/Meertaligheid.md) | Huidige resx-/ResXManager-opzet |
| [Historie](docs/historie/Proefversie.md) | Oorspronkelijke proefplanning |
| [Werkinstructies](AGENTS.md) | Afspraken voor vervolgsessies |

Iedere milestone gebruikt een eigen branch met dezelfde technische naam. Slices worden pas na gebruikersakkoord gecommit en gepusht. Een milestone wordt gemerged na acceptatie van haar volledige resultaat. De gebruiker doet de visuele en echte installatieproeven.

De roadmap is geaccepteerd; M00 en M01 zijn gepubliceerd en gemerged. M01-S06 is gepubliceerd als 6c84951 en de M01-merge als e14824e. De voortgang en acceptatiestatus staan in de slicelijst.

Markdown wordt gespiegeld naar `C:\Devops\Obsidian\markdown\Development\HNSoftwareDevelopment\Inno Setup Studio`, met behoud van relatieve paden.

M02-S04 voegt een afzonderlijk Vertaaloverzicht toe, bereikbaar naast Voorbeeldtaal: tekstvelden als rijen en alle projecttalen als kolommen. Eigen teksten, overnemen/herstellen en terugval blijven aangesloten op het ontwerpscherm. Verwijzingswaarschuwingen verschijnen per veld; verwijderen van een taal vraagt expliciet bevestiging. Deze slice is door de gebruiker geaccepteerd. [Scope en teststappen](docs/slices/M02-S04-Vertaaloverzicht.md).

M02-S05 exporteert alle actieve projecttalen in projectvolgorde. De native taalkeuze selecteert dezelfde wizardteksten en terugval als de preview. Uitsluitend Nederlands werkt ook met een uitgeschakelde oorspronkelijke Engelse opslagtaal. Eigen .isl-bronnen blijven als bestanden nodig; fouten noemen taal en bronpad. Release-build zonder fouten/waarschuwingen en 187 tests geslaagd, inclusief meertalige, RTL-, Nederlandse en zelfstandige externe-broncompilatie. De gebruiker heeft het .iss-bestand gecontroleerd en de installer met verschillende talen gebouwd en uitgevoerd; M02-S05 is geaccepteerd. [Teststappen en milestonecheck](docs/slices/M02-S05-Installertaal-export-en-proef.md).

M02 is afgerond en naar main gemerged als dc4ba08. M03-S01 voegt links **Paginaoverzicht** toe: alle veertien native wizardfasen met hun huidige status en uitleg. Welkom en Installatiemap openen hun bestaande ontwerppagina. De overige bediening volgt in de geplande slices. [Scope en teststappen](docs/slices/M03-S01-Standaardpaginaoverzicht.md).

M03-S02: selecteer Licentie of Informatie vóór/na installatie in **Paginaoverzicht** en kies **Document instellen**. Een algemeen TXT/RTF-document activeert de pagina. Een optionele variant per projecttaal vervangt het algemene document voor die taal. **Documentvoorbeeld tonen** laat de gekozen bron zien. Documenten blijven als bestanden nodig en worden met native richtlijnen geëxporteerd. [Handmatige controles en grenzen](docs/slices/M03-S02-Licentie-en-informatie.md).

M03-S03: schakel **Gebruikersinformatie** of een documentpagina in via de schakelaar in Paginaoverzicht; de instellingen openen direct. Uitschakelen bewaart de gegevens. Dubbelklik op een actieve paginanaam of F2 opent bestaande instellingen. Voor **Startmenumap** kies je **Pagina instellen**. Stel beginwaarden en updategedrag in; Invoervoorbeeld bewaart geen proefgegevens. Startmenumapbeleid is al instelbaar, maar de native mapkeuzepagina verschijnt pas als het project snelkoppelingen bevat. [Teststappen en grenzen](docs/slices/M03-S03-Gebruikersinformatie-en-startmenumap.md).

M03-S04: selecteer Gereed, Voorbereiden, Installeren of Voltooid in **Paginaoverzicht** en kies **Fase bekijken**. De korte simulatie toont samenvatting, native Installeren/Voltooien-knoprollen en handmatige voortgang. Er worden geen installatieacties uitgevoerd en de lokale taalkeuze verandert de algemene previewtaal niet. [Teststappen en grenzen](docs/slices/M03-S04-Gereed-voortgang-en-voltooid.md).

Bij **Exporteer .iss** naar een bestaand bestand vraagt Studio of de huidige export het bestand mag vervangen, inclusief handmatige wijzigingen. **Nee** behoudt de oude inhoud; **Ja** schrijft de nieuwe projectstand. Projectbestanden blijven via Opslaan/Opslaan als bewaard als .issstudio.

M03-S05 verduidelijkt het bereik van elke bestaande ontwerpeigenschap in het eigenschappenpaneel. Gedeelde Verder-tekst geldt ook op de gewone native invoer- en documentpagina’s. Installeren en Voltooien behouden hun eigen knoprollen; lokale knopteksten gelden alleen voor hun pagina en installertaal. Het projectformaat blijft gelijk. [Scope en teststappen](docs/slices/M03-S05-Basisontwerp-en-pagina-afwijkingen.md).

M03-S06: **Uitproberen** opent één wizard met de actieve pagina’s in native volgorde. Documenten, licentieacceptatie, gebruikersinformatie, tijdelijke installatiemap en afsluitsimulatie vormen één route. Informatie na verschijnt tussen Installeren en Voltooid. De proef installeert niets; sluiten herstelt de ontwerpselectie. [Werking en teststappen](docs/slices/M03-S06-Wizardproef.md).

De documentvensters controleren nu automatisch de algemene bron en ingevulde taalvarianten. Fouten staan rood boven het voorbeeldveld; geldig documentmateriaal wordt automatisch geladen. Voorbeeld vernieuwen leest wijzigingen buiten Studio opnieuw. [Controle en teststappen](docs/slices/M03-Documentfouten-direct-tonen.md). Deze aanvulling is door de gebruiker geaccepteerd.

M03 is na gebruikersacceptatie naar main gemerged als 84c8a06. M04-S01 is geïmplementeerd op m04-installatiekeuzes en door de gebruiker geaccepteerd: Release-build zonder fouten/waarschuwingen, alle 244 tests geslaagd. De gebruiker heeft regelbeheer, uitsluitingen en de echte installatie gecontroleerd.

M04-S02 voegt **Mapregels → Mapregels beheren** toe voor expliciete, onder andere lege doelmappen. Mapkenmerken, NTFS-compressie en native opruimgedrag zijn instelbaar. Projectformaat 11 bewaart deze regels en migreert bestaande projecten. Release-build zonder fouten/waarschuwingen; 253 tests geslaagd. [Scope en handmatige controles](docs/slices/M04-S02-Mapregels.md). Deze slice is door de gebruiker geaccepteerd na controle in Studio en de uitgevoerde installer.

M04-S03 voegt **Snelkoppelingen → Snelkoppelingen beheren** toe: Startmenu/Bureaublad, naam, doel, parameters en werkmap. Ontbrekende geïnstalleerde doelen worden gemeld en blokkeren bouwen. De startmenupagina volgt native beleid en is opgenomen in de wizardproef. [Scope en teststappen](docs/slices/M04-S03-Snelkoppelingen.md). Deze slice wacht op gebruikersacceptatie.

**Installatietypen** beheert geordende typen met een interne naam, standaardbeschrijving, vertalingen en maximaal één aangepast type. De geselecteerde ontwerpregel wordt bewaard; export gebruikt native Types en CustomMessages. De componentenpagina volgt de gedefinieerde componenten; inhoudskoppelingen volgen in S07. [Beginnersuitleg](docs/gebruikershandleiding/Installatietypen.md) · [M04-S04 en teststappen](docs/slices/M04-S04-Installatietypen.md).

**Componenten** beheert onderdelen, onderliggende groepen, typekoppelingen, vertalingen en vaste/afzonderlijk selecteerbare onderdelen. Paginaoverzicht en Uitproberen tonen de native componentenstap; de proef bewaart tijdelijke type- en componentkeuzes. Bestanden en snelkoppelingen worden pas in S07 gekoppeld. [Beginnersuitleg](docs/gebruikershandleiding/Componenten.md) · [M04-S05 en teststappen](docs/slices/M04-S05-Componenten.md).
