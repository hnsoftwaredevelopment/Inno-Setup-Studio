# Inno Setup Studio

Een afzonderlijke visuele Windows-ontwerper als add-on voor **Inno Setup 7.1**. Het is geen add-on voor de bestaande InnoSetupStudio.

De bestaande variant in `C:\Devops\hnsoftwaredevelopment\InnoSetupStudio\InnoSetupStudio` blijft behouden. Dit nieuwe project krijgt directe elementselectie, een zichtbaar Basisontwerp, contextuele eigenschappen en afzonderlijke ontwerp- en uitprobeermodi.

Het doel is dat Studio `.iss`-scripts genereert die met de lokaal geïnstalleerde Inno Setup 7.1-compiler kunnen worden gebouwd, rechtstreeks of vanuit Studio. Inno Setup verzorgt de uiteindelijke installerinterface en installatiehandelingen.

## Status

De eerste WPF-werkplek werkt: Basisontwerp, Welkom en Installatiemap, direct selecteerbare elementen, gedeelde knopteksten met pagina-afwijkingen, een uitprobeermodus en opslaan/openen van `.issstudio`-projecten. Installatiemap, Bladeren-tekst en tooltip zijn instelbaar. Een mapkeuze in Uitproberen wijzigt het opgeslagen standaardpad niet.

De Studio-interface is beschikbaar in Nederlands, Engels en Duits. Rechtsboven wisselt **Studio-taal** direct van taal; de keuze wordt per Windows-gebruiker onthouden. Installerinhoud blijft onafhankelijk van deze keuze. Zie [meertaligheid en ResXManager](docs/Meertaligheid.md).

Het uitklapbare paneel **Applicatiegegevens** boven de ontwerpruimte bevat productnaam, AppId en versie. Die worden met het project opgeslagen. Oude projecten worden bij openen in het geheugen bijgewerkt en pas bij opslaan naar het nieuwe formaat geschreven. Zie [M01-S01 en teststappen](docs/slices/M01-S01-Applicatiegegevens.md).

Het paneel **Bestandsregel** bevat één lokaal bronbestand en een doelmap binnen {app}. **Exporteer .iss** schrijft een zelfstandig compileerbaar script met applicatiegegevens, het standaard installatiepad, een vaste Nederlandse installer en deze bestandsregel. De teksten van Welkom en Installatiemap, gedeelde knoppen, pagina-afwijkingen en Bladeren-tooltip worden eveneens geëxporteerd. Het paneel Compiler en bouwen kan nu ISCC kiezen, versie 7.1 controleren en het opgeslagen project bouwen. Meertalige installers volgen later. Zie [M01-S04 en teststappen](docs/slices/M01-S04-Compiler-kiezen-en-bouwen.md). Zie [M01-S03 en teststappen](docs/slices/M01-S03-Wizardontwerp-export.md). Zie [M01-S02 en teststappen](docs/slices/M01-S02-Bestandsregel-en-export.md).

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

Op 8 oktober 2026: 108 tests geslaagd, waaronder zes echte compilatieproeven met ISCC 7.1.0; Release-build zonder fouten of waarschuwingen. De gebruiker heeft M01-S02 volledig getest en geaccepteerd. Ook M01-S03 is door de gebruiker getest in Studio en in de gecompileerde, uitgevoerde installer en geaccepteerd. M01-S04 is door de gebruiker volledig getest en geaccepteerd. M01-S05 voegt live bouwmeldingen, annuleren en een opgeslagen uitvoernaam toe. Nieuwe projecten beginnen met mysetup; oude projecten behouden setup met een waarschuwing. Formaat 4 bewaart de naam en leest de eerdere formaten. M01-S05 is door de gebruiker handmatig getest en geaccepteerd. Zie [M01-S05 en teststappen](docs/slices/M01-S05-Bouwmeldingen-en-annuleren.md). M01-S01 en de eerdere Studio-taalbasis zijn eveneens geaccepteerd.

De compilatietests gebruiken de standaardinstallatie van Inno Setup 7. Als die ontbreekt, worden de compilerafhankelijke tests met een expliciete reden overgeslagen. Ze bouwen een installer, maar starten die niet. De andere tests vereisen geen compiler.

M01-S06 beschermt bestaande afwijkende .iss-bestanden tegen vervangen. Een lokale SVG-eindproef staat in samples/SVGViewerDemo/SVG Viewer M01-proef.issstudio; de gebruiker heeft de eindproef uitgevoerd en M01 volledig geaccepteerd. Dit project installeert één bestand; volledige bestandsverzamelingen volgen in M04. Zie [M01-S06 en eindproef](docs/slices/M01-S06-Eenvoudige-installer-afronden.md). De aanvullende SVG-compilatietest vereist de lokaal aangeleverde executable; bij ontbreken wordt die test met uitleg overgeslagen.

## Documentatie en planning

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

De roadmap is geaccepteerd en M00 is gepubliceerd en gemerged. De voortgang en acceptatiestatus staan in de slicelijst.

Markdown wordt gespiegeld naar `C:\Devops\Obsidian\markdown\Development\HNSoftwareDevelopment\Inno Setup Studio`, met behoud van relatieve paden.
