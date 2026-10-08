# M01-S02 — Eerste bestandsregel en export

Datum: 8 oktober 2026. Branch: m01-eerste-installer.
Status: geïmplementeerd, gebouwd, automatisch gecontroleerd en op 8 oktober 2026 door de gebruiker volledig getest en geaccepteerd. Publicatie gebeurt op de milestonebranch.

## Resultaat

Het uitklapbare paneel **Bestandsregel** bevat een bronbestand, een knop om dat bestand te kiezen en een doelmap voor dit bestand. Eén lokaal bestand wordt opgenomen; wildcards, meerdere regels en overige bestandsopties volgen in M04. De doelmap is {app} of een submap, zoals {app}\Data. {app} betekent de installatiemap die de gebruiker van de installer kiest; het is geen voorbeeldpad dat Studio moet invullen.

De knop **Exporteer .iss** schrijft de actuele projectstand naar een gekozen scriptbestand. Export markeert het project niet als opgeslagen. Ongeldige gegevens of een ontbrekende bron stoppen de export vóór een bestand wordt vervangen. Bij een bestaande uitvoernaam vraagt het bestandsdialoogvenster om bevestiging. De export mag het bronbestand niet vervangen en moet de extensie .iss hebben. Schrijven gebeurt atomair, in UTF-8 met BOM.

De exporter staat in Studio.Core/InnoScript, buiten de WPF-views. Er zijn geen extra assemblies of dependencies toegevoegd. Interface en meldingen staan in de drie Studio-resx-bestanden. Tijdens Uitproberen zijn bestandsgegevens alleen-lezen en is export uitgeschakeld.

## Paden, opslag en migratie

Een relatief bronpad is gebaseerd op de map van het opgeslagen project, nooit op de werkmap van Studio of de exportmap. Voor export met een relatief pad moet het project eerst zijn opgeslagen. De bestandskiezer bewaart een bron binnen de projectmap relatief; bronnen daarbuiten blijven absoluut. Bij Opslaan als naar een andere map wordt een relatief bronpad opnieuw berekend zodat hetzelfde bestand bedoeld blijft. Na een mislukte opslag verandert het pad in het geheugen niet.

De export schrijft het volledig opgeloste bronpad. Daardoor kan hetzelfde .iss-bestand ook vanuit een andere map worden gecompileerd. Voor verplaatsen naar een andere computer moeten de bronverwijzingen opnieuw worden gecontroleerd; dit is nog geen zelfvoorzienend distributiepakket.

Projectformaat 3 voegt InstallFile met Source en Destination toe. Formaat 1 en 2 migreren in het geheugen naar een lege bron en {app} als bestandsdoel. Formaat-2-AppId, versie en ontwerpgegevens blijven behouden; de formaat-1-identiteit volgt dezelfde afleiding als in S01. Migratie schrijft niets automatisch terug. Onbekende versies, ontbrekende/null-bestandsregels in formaat 3 en een bestandsregel met een onjuist ouder formaatnummer worden afgewezen.

Een bron mag leeg of tijdelijk ontbrekend zijn bij het bewaren van een ontwerp. Export vereist wel een bestaand bestand. Opslaan en exporteren verliezen daarmee geen ontwerpwerk door een tijdelijk ontbrekende bron.

## Exportbereik en native koppeling

Deze eerste export bevat uitsluitend de onderstaande waarden en één bestandsregel. De huidige eigen wizardteksten, knopafwijkingen en Bladeren-instellingen worden nog niet vertaald; dat is M01-S03. De interface en het script benoemen deze beperking.

| Projectgegeven | Type en bereik | Native vertaling in 7.1 | Controle |
|---|---|---|---|
| Name | Niet-lege producttekst, één regel | [AppName](https://jrsoftware.org/ishelp/topic_setup_appname.htm) | Compilatieproef met accenten, quotes, puntkomma en accolades |
| AppId | Stabiele tekst, maximaal 127 tekens | [AppId](https://jrsoftware.org/ishelp/topic_setup_appid.htm) | GUID-identiteit wordt correct als literal geëxporteerd |
| AppVersion | Niet-lege versietekst, één regel | [AppVersion](https://jrsoftware.org/ishelp/topic_setup_appversion.htm) | Versie met beta-suffix compileert |
| Destination.DefaultDirectory | Bestaande native padexpressie | [DefaultDirName](https://jrsoftware.org/ishelp/topic_setup_defaultdirname.htm) | Standaard {autopf}-pad compileert |
| InstallFile.Source | Eén bestaand bestand, absoluut of relatief aan project | [Files: Source](https://jrsoftware.org/ishelp/topic_filessection.htm) | Absoluut opgelost; compileert vanuit andere map |
| InstallFile.Destination | {app} of submap; geen traversal, wildcards of extra constanten | [Files: DestDir](https://jrsoftware.org/ishelp/topic_filessection.htm) | Submap met Unicode en puntkomma compileert |
| Vaste begininstallertaal | Nederlands, nog geen projectkeuze | [Languages](https://jrsoftware.org/ishelp/topic_languagessection.htm): compiler:Languages\Dutch.isl | Beide compilatieproeven gebruiken de geïnstalleerde taalbron |

Er worden geen Run-, Registry- of shortcutregels toegevoegd. Inno Setup behoudt zijn normale installatie- en deïnstallatiegedrag en huidige standaardbeleid, waaronder de standaard installatierechten. Beleidskeuzes worden later expliciet instelbaar.

Setup-waarden en Files-parameters krijgen hun eigen correcte quoteverwerking. Letterlijke accolades in applicatiegegevens worden ge-escaped; native padconstanten blijven expressies. Preprocessoruitdrukkingen ({#…}) in deze uitvoerwaarden worden afgewezen totdat expliciete scripting wordt ondersteund. Bronnen: [Parameters](https://jrsoftware.org/ishelp/topic_params.htm), [Setup](https://jrsoftware.org/ishelp/topic_setupsection.htm), [Constanten](https://jrsoftware.org/ishelp/topic_consts.htm) en de [officiële compilerbron](https://github.com/jrsoftware/issrc/blob/main/Projects/Src/Compiler.SetupCompiler.pas).

Uitvoer na compilatie staat standaard naast het .iss-bestand onder output/setup.exe. Studio start nog geen compiler of installer; compilerbediening volgt in S04.

## Verificatie

Release-build: geslaagd, nul waarschuwingen en fouten. Alle 72 tests slagen, zonder overgeslagen tests. De geïnstalleerde doelcompiler is ISCC 7.1.0.

Tests controleren behoud van metadata/bestandsregels, reproduceerbare export, migratie van formaat 1 en 2, padbasis en Opslaan als, behoud van bestanden bij fouten, lege/ontbrekende/ongeldige bronnen, doelmapgrenzen, script-/preprocessorinvoer en alleen-lezen gedrag tijdens Uitproberen. Twee integratietests roepen ISCC buiten Studio aan en controleren of een nieuwe setup.exe is gebouwd: één met bijzondere tekens en één met het meegeleverde voorbeeldproject.

Die twee tests worden met reden overgeslagen als Inno Setup 7 niet op zijn standaardlocatie aanwezig is. De overige tests hebben geen compiler nodig. Er is geen GUI-kliktest of echte installatie uitgevoerd.

De gebruiker heeft alle beschreven controles uitgevoerd en meldt dat alles werkt zoals verwacht. Tijdens compilatie is één waarschuwing gemeld over OutputBaseFilename=setup: volgens de compiler behandelt Windows setup.exe met compatibiliteitsaanpassingen die DLL's onveilig kunnen laden. De gebruiker accepteert deze slice met deze bekende waarschuwing en wil haar oppakken bij de toekomstige instelbare installernaam. Kies daar een andere standaardnaam en waarschuw bij setup.exe; behoud ook de oorspronkelijke compilerwaarschuwing in de bouwmeldingen. Dit is vastgelegd bij M01-S04/S05. De huidige generator wordt voor dit akkoord niet aangepast.

Het meegeleverde voorbeeldproject bevat de door de gebruiker geteste waarden: productnaam Test applicatie, versie 1.1 en gedeelde Verder-tekst Verder.... De door de gebruiker gegenereerde .iss-bestanden en output blijven lokaal aanwezig en worden als voorbeeld-uitvoer genegeerd door Git.

## Gebruikerscontrole

1. Open [Eerste installer.issstudio](<../../samples/Eerste installer/Eerste installer.issstudio>) in de Release-app. Klap Bestandsregel uit: bron is Lees mij.txt, doel is {app}\Documentatie. Controleer dat de gegevens na opslaan en heropenen gelijk blijven.
2. Kies een eigen klein testbestand. Verander de doelmap naar {app}\Data. Test Nederlands, Engels en Duits: labels en meldingen wisselen, bestandswaarden blijven gelijk. Controleer ook toetsenbordbediening, de minimale venstergrootte en de alleen-lezen velden tijdens Uitproberen.
3. Klik Exporteer .iss en kies een nieuwe scriptnaam, eventueel in een andere map. Open dat script in de Inno Setup-editor en compileer het. Metadata, het bronpad en DestDir moeten bij jouw project horen; output/setup.exe moet naast de export ontstaan. Eigen wizardteksten komen in deze slice nog niet overeen met de preview.
4. Vul een ontbrekend bronbestand in en probeer opnieuw te exporteren: je krijgt uitleg en bestaande uitvoer blijft intact. Test ook een wildcard of ongeldige doelmap, bijvoorbeeld {sys}. Controleer dat annuleren van het exportdialoogvenster geen bestand schrijft.
5. Sla een project met een relatieve bron via Opslaan als in een andere map op. Heropen de kopie en exporteer: hetzelfde bronbestand moet worden gebruikt en AppId moet gelijk blijven. Open ook je bestaande formaat-2-project en controleer dat applicatiegegevens en ontwerp behouden zijn. Bewaar de eerste migratie bij voorkeur als kopie; de oude app kan formaat 3 niet openen.

Voor deze slice is compileren voldoende. Echte installatie-/deïnstallatiecontrole hoort bij de latere milestoneproef. Het gebruikersakkoord is ontvangen; deze slice en haar documentatie worden samen gecommit en gepusht. De milestone blijft open.

## Wijziging vanaf M01-S06

De hierboven beschreven overschrijfbevestiging hoort bij de oorspronkelijke S02-stand. Vanaf [M01-S06](M01-S06-Eenvoudige-installer-afronden.md) blijft een bestaande afwijkende .iss behouden en wordt export naar een andere naam gevraagd; identieke inhoud blijft ongewijzigd.
