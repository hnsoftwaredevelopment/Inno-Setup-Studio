# Inno Setup Studio

Een afzonderlijke visuele Windows-ontwerper als add-on voor **Inno Setup 7.1**. Het is geen add-on voor de bestaande InnoSetupStudio.

De bestaande variant in `C:\Devops\hnsoftwaredevelopment\InnoSetupStudio\InnoSetupStudio` blijft behouden. Dit nieuwe project krijgt directe elementselectie, een zichtbaar Basisontwerp, contextuele eigenschappen en afzonderlijke ontwerp- en uitprobeermodi.

Het doel is dat Studio `.iss`-scripts genereert die met de lokaal geïnstalleerde Inno Setup 7.1-compiler kunnen worden gebouwd, rechtstreeks of vanuit Studio. Inno Setup verzorgt de uiteindelijke installerinterface en installatiehandelingen.

## Status

De eerste WPF-werkplek werkt: Basisontwerp, Welkom en Installatiemap, direct selecteerbare elementen, gedeelde knopteksten met pagina-afwijkingen, een uitprobeermodus en opslaan/openen van `.issstudio`-projecten. Installatiemap, Bladeren-tekst en tooltip zijn instelbaar. Een mapkeuze in Uitproberen wijzigt het opgeslagen standaardpad niet.

De Studio-interface is beschikbaar in Nederlands, Engels en Duits. Rechtsboven wisselt **Studio-taal** direct van taal; de keuze wordt per Windows-gebruiker onthouden. Installerinhoud blijft onafhankelijk van deze keuze. Zie [meertaligheid en ResXManager](docs/Meertaligheid.md).

Scriptgeneratie, compilerkoppeling en meertalige installers volgen later. Zie [de specificatie](docs/Productspecificatie.md).

## Bouwen en starten

Vereist: Windows en de .NET 10 SDK voor bouwen; .NET 10 Desktop Runtime voor het starten van de gebouwde applicatie.

```powershell
dotnet build '.\Inno Setup Studio.slnx' -c Release
dotnet test '.\Inno Setup Studio.slnx' -c Release
dotnet run --project '.\src\Studio.App\Studio.App.csproj'
```

De Release-app staat in `Builds\Release\Inno Setup Studio.exe`. Distributie vereist de volledige uitvoermap, inclusief de `en`- en `de`-submappen. Inno Setup is voor de huidige ontwerpwerkplek nog niet nodig; de toekomstige compilerkoppeling gebruikt de apart geïnstalleerde Inno Setup 7.1.

## Verificatie

Op 8 oktober 2026: 31 Core-tests geslaagd; Release-build zonder fouten of waarschuwingen. Handmatig gecontroleerd: live taalwissel, behoud van installerinhoud en selectie, Duitse en Engelse weergave en behoud van de taal na herstart.

## Documentatie

Markdown wordt gespiegeld naar `C:\Devops\Obsidian\markdown\Development\HNSoftwareDevelopment\Inno Setup Studio`, met behoud van relatieve paden.
