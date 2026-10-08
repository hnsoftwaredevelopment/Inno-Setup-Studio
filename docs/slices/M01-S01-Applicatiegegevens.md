# M01-S01 — Applicatiegegevens

Datum: 8 oktober 2026. Branch: m01-eerste-installer.
Status: geïmplementeerd, automatisch gecontroleerd en op 8 oktober 2026 door de gebruiker volledig getest en geaccepteerd. Publicatie gebeurt op de milestonebranch volgens de afgesproken werkwijze.

## Resultaat

Het uitklapbare paneel **Applicatiegegevens** boven de ontwerpruimte bevat productnaam, applicatie-ID (AppId) en versie. Deze instellingen gelden voor het hele project. Het paneel is een afzonderlijke WPF-view met bindings naar EditorSession; de wizardpagina's blijven afzonderlijk.

Nieuwe projecten krijgen een eigen GUID als AppId en versie 1.0. AppId is bewerkbaar en hoeft geen GUID te zijn. Productnaam wijzigen, versie wijzigen, Studio-taal wisselen en Opslaan als veranderen de identiteit niet. Alleen een bewuste wijziging van AppId doet dat. Bewerkingen markeren het project als gewijzigd; tijdens Uitproberen zijn de gegevens alleen-lezen.

Eigen pagina-, knopteksten en installatiepad worden niet automatisch herschreven bij een nieuwe productnaam. De paneeltoelichting benoemt dat. Alle nieuwe Studio-teksten staan in de Nederlandse, Engelse en Duitse resx-bestanden.

Opslaan vereist drie niet-lege waarden op één regel, zonder controletekens. AppId mag maximaal 127 tekens bevatten. Een ongeldige waarde voorkomt overschrijven van een bestaand bestand. Voor de voorgestelde bestandsnaam worden ongeldige bestandsnaamtekens vervangen; de productnaam zelf blijft behouden.

De instellingen sluiten aan op [AppId](https://jrsoftware.org/ishelp/topic_setup_appid.htm) en [AppVersion](https://jrsoftware.org/ishelp/topic_setup_appversion.htm). Dit bewijst nog geen werkende export of installer; scriptgeneratie begint in M01-S02.

## Projectformaat en bestaande bestanden

Bij oplevering van deze slice werd de projectformaatversie 2. Het bestaande veld Name blijft de productnaam; AppId en AppVersion zijn toegevoegd. Version is de projectformaatversie en staat los van AppVersion. [M01-S02](M01-S02-Bestandsregel-en-export.md) breidt het formaat uit naar versie 3 met behoud van deze gegevens.

Formaat 1 wordt bij openen alleen in het geheugen gemigreerd. Pagina's, gedeelde knoppen, afwijkingen, installatiepad en Bladeren-instellingen blijven behouden. De nieuwe AppId wordt deterministisch afgeleid van de oorspronkelijke JSON-inhoud (SHA-256, eerste 16 bytes als GUID); de beginversie is 1.0. Zo krijgt hetzelfde oude bestand bij opnieuw openen vóór opslaan dezelfde identiteit, ook als het is gekopieerd naar een ander pad. Externe inhoudswijzigingen vóór de eerste opslag kunnen deze afgeleide identiteit veranderen.

Het project wordt als gewijzigd gemarkeerd en een statusmelding vraagt om opslaan. Pas de gebruiker schrijft via Opslaan of Opslaan als het nieuwe formaat. Daarna staat AppId expliciet in het bestand en blijft deze onafhankelijk van inhoudswijzigingen. Een nieuw formaat-2-bestand zonder expliciete AppId of AppVersion wordt afgewezen; onbekende formaatversies worden niet stilzwijgend aangenomen.

De oude Studio-versie kan formaat 2 niet openen. Gebruik bij de eerste controle **Opslaan als** om je oorspronkelijke proefproject te behouden. Het meegeleverde samples/Mijn applicatie.issstudio is niet gewijzigd.

## Automatische controle

Release-build van de oplossing: geslaagd, nul waarschuwingen en fouten. Alle 45 Core-tests slagen, zonder overgeslagen tests.

Gerichte tests bewijzen nieuwe projectidentiteiten, behoud bij hernoemen/versie/taal/Opslaan als, bewerkbare tekst-ID's en de lengtegrens, blokkeren van bewerkingen tijdens Uitproberen, behoud van een bestaand bestand bij ongeldige invoer, vereiste metadata en gecontroleerde migratie van de bestaande proefinhoud. Een vaste formaat-1-fixture bewaart die proefinhoud voor regressietests. De bestaande taaltest controleert ook volledigheid van de nieuwe vertalingen.

De app is niet automatisch bediend en er is geen echte installer uitgevoerd.

## Gebruikerscontrole

1. Start Builds/Release/Inno Setup Studio.exe. Klap Applicatiegegevens uit. Wijzig productnaam en versie, noteer AppId en sla een testproject op. Heropen het: alle drie waarden moeten gelijk blijven. Opslaan als moet dezelfde AppId bewaren; Nieuw moet een andere AppId geven.
2. Wijzig AppId bewust in bijvoorbeeld MijnTestApplicatie. Sla op en heropen: de ingevoerde waarde blijft behouden. Maak een veld leeg en probeer op te slaan: je krijgt uitleg en het eerder opgeslagen bestand blijft intact.
3. Open je bestaande proefproject. Controleer Welkom, Installatiemap, gedeelde knoppen, pagina-afwijkingen en Bladeren-instellingen. De status vermeldt de migratie. Gebruik Opslaan als naar een nieuwe naam en heropen die kopie: dezelfde AppId en instellingen blijven behouden, zonder nieuwe migratiemelding.
4. Wissel Studio-taal tussen Nederlands, Engels en Duits. Labels en toelichting veranderen, projectwaarden en wizardteksten blijven gelijk. Test het paneel ook bij de minimale venstergrootte en met het toetsenbord.
5. Schakel Uitproberen in: de applicatiegegevens zijn alleen-lezen. Terug in Ontwerp zijn ze bewerkbaar. Controleer dat selectie en uitprobeerpad nog werken zoals vóór deze slice.

Het gebruikersakkoord is ontvangen. Deze slice en haar documentatie worden samen gecommit en gepusht. De volledige milestone blijft open voor de volgende slices.
