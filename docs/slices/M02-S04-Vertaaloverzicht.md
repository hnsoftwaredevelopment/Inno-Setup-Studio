# M02-S04 — Vertaaloverzicht en controles

Status: geaccepteerd door de gebruiker op m02-installertalen. Het overzicht is ook met veel talen als overzichtelijk beoordeeld.

## Resultaat en scope

De knop naast Voorbeeldtaal opent een afzonderlijk Vertaaloverzicht. Dit toont de huidige wizardteksten als rijen en alle projecttalen als kolommen, in projectvolgorde. Basisontwerp, Welkom en Installatiemap blijven herkenbaar. De vaste eerste kolom beschrijft het tekstveld. Horizontaal en verticaal scrollen houden meerdere talen bereikbaar zonder het ontwerpscherm te verbreden.

Elke taalcel bewerkt uitsluitend de eigen tekst van die taal en scope. Leeg betekent watermark met de effectieve tekst; Overnemen en Herstellen worden afwisselend getoond. De bestaande S03-resolver blijft de bron voor herkomst, preview en terugval. Wijzigingen gelden direct voor het project; Sluiten annuleert ze niet. Bewaren blijft de bestaande projectactie. Studio-taal en previewselectie wijzigen niet door het overzicht.

Uitgeschakelde talen blijven zichtbaar en bewerkbaar, duidelijk gemarkeerd, maar tellen niet mee in de actieve waarschuwingen. Uitschakelen bewaart teksten. Verwijderen krijgt een expliciete bevestiging met het aantal eigen tekstvelden dat verloren gaat. De laatste actieve taal kan niet worden verwijderd. Bij verwijderen van de oorspronkelijke opslagtaal worden de teksten van de resterende talen behouden; opslagformaat 6 blijft geldig.

Controles melden ontbrekende eigen vertalingen en bronberichten volgens S03. Eigen teksten worden gecontroleerd op onbekende of ongesloten Studio-verwijzingen en ontbrekende {AppName}/{AppVersion} uit de toepasselijke eigen tekst in de standaardtaal. Meldingen zijn waarschuwingen: ze herschrijven of blokkeren tekst niet. Deze verwijzingsmeldingen verschijnen ook in het gewone eigenschappenpaneel, bij het betreffende tekst- of tooltipveld. Letterlijke projectwaarden worden niet als invoersyntax onderzocht. Standaardberichten uit taalbestanden gebruiken hun bestaande native opmaakregels.

## Afbakening en acceptatie

Afhankelijk van geaccepteerde M02-S03. Koppeling: FR-08, FR-09 en FR-10. Bestandsgroepen: overzicht/model, tekstcontrole, EditorSession, nieuw WPF-venster, taalbeheer, resx, tests en documentatie. Meertalige export blijft S05. Licentie-/informatiepagina's en hun bestanden volgen M03. Toegangstoetsen en native berichtsyntax worden in deze slice niet opnieuw geïnterpreteerd.

Automatisch controleren we taal-/scopegebonden bewerking, terugval en overerving, waarschuwingen, bewaren/heropenen, behoud bij uitschakelen en verwijderen, wisselen van opslagtaal en onafhankelijkheid van Studio-/previewselectie. Release-build en volledige regressiesuite sluiten de slice af. GUI- en installatietests blijven bij de gebruiker.

## Handmatige controle

1. Open een project met minstens drie talen en het Vertaaloverzicht. Controleer taalvolgorde, standaardtaal, rijbenamingen en horizontaal/verticaal scrollen.
2. Bewerk dezelfde titel in twee taalkolommen. Sluit het overzicht, wissel Voorbeeldtaal en controleer beide teksten. Bewaar/heropen: teksten blijven behouden.
3. Neem een watermark over en herstel de eigen tekst. Controleer de wisselende knoppen en gedeelde versus lokale knopteksten.
4. Zet in de standaardtaal een titel met {AppName} en {AppVersion}. Laat een verwijzing weg in een eigen vertaling; probeer ook {AppNmae} en een ontbrekende sluitaccolade. Controleer de waarschuwingen en herstel de invoer.
5. Schakel een taal met eigen teksten uit: de kolom blijft zichtbaar, gemarkeerd en bewerkbaar; actieve aantallen dalen. Schakel weer in en controleer behoud.
6. Verwijder een taal: annuleer eerst de bevestiging en controleer behoud. Bevestig daarna en controleer dat andere talen, ook bij verwijderen van de oorspronkelijke taal, exact hun eigen teksten behouden. Bewaar/heropen.

## Verificatie

Release-build geslaagd zonder fouten of waarschuwingen. De volledige suite slaagt met 175 tests, zonder overgeslagen tests, inclusief de bestaande ISCC-compilatieproeven. De twintig nieuwe overzichtstests bewijzen bewerking zonder selectiewissel, gedeeld/lokaal en overnemen/herstellen, bewerken van uitgeschakelde talen, actieve tellers, vertaalde meldingen, bekende/onbekende/onvolledige verwijzingen, ontbrekende verwijzingen uit de toepasselijke standaardscope, behoud van regelovergangen en behouden van andere taalteksten bij uitschakelen/verwijderen/bewaren/heropenen. Zowel oude als nieuwe oorspronkelijke opslagtaal en een uitgeschakelde derde taal zijn gecontroleerd.

Na de laatste resource-/vensterwijziging slagen de Release-build en 31 gerichte overzichts-/lokalisatietests. Alle resourceverwijzingen van de gewijzigde XAML zijn gecontroleerd. Het overzicht houdt rijen/cellen stabiel bij tekstbewerking en gebruikt virtualisatie voor de scrollbare matrix. Er is geen GUI of installer automatisch gestart; de visuele toetsing en bediening blijven bij de gebruiker.

Markdown is gespiegeld naar Obsidian en inhoud en lokale links zijn gecontroleerd. Gebruikersakkoord ontvangen; de slice wordt gecommit en gepusht op de milestonebranch. Samenvoegen volgt na acceptatie van de volledige milestone.
