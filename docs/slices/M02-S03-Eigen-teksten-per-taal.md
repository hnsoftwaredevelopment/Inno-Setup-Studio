# M02-S03 — Eigen teksten per taal

Status: geaccepteerd door de gebruiker op m02-installertalen, inclusief terugval, compact tekstpaneel, appicoon en cursoruitlijning.

## Gebruikersresultaat en scope

Eén taal tegelijk bewerken in het bestaande eigenschappenpaneel. Voorbeeldtaal bepaalt welke installertaal wordt bewerkt en getoond; Studio-taal blijft onafhankelijk. Een teller boven de preview meldt ontbrekende eigen vertalingen in actieve projecttalen. De taallijst vermeldt aantallen per taal. Bij het geselecteerde veld staan de herkomst en de daadwerkelijk getoonde tekst. Zonder eigen tekst toont het lege invoerveld de effectieve tekst als grijze watermark. Overnemen kopieert die waarde naar eigen invoer en maakt plaats voor Herstellen. Herstellen wist alleen de eigen invoer, waarna watermark en Overnemen terugkeren. Dezelfde indeling geldt voor de tooltip; zonder effectieve tooltip is Overnemen uitgeschakeld. Aanvullende uitleg staat ingeklapt. Een brede tabel met alle talen volgt in S04.

De eerste ingeschakelde projecttaal is de standaard-/terugvaltaal. Dat wordt zichtbaar benoemd. Volgorde wijzigen kan dus de terugval veranderen; het verplaatst of kopieert geen eigen teksten. Uitgeschakelde talen behouden hun vertalingen en tellen niet mee in de waarschuwing.

## Vastgelegde terugvalregel

Een ontbrekende, lege of uitsluitend uit witruimte bestaande eigen vertaling levert geen lege vereiste wizardtekst op. Resolutie gebeurt in deze volgorde: eigen lokale tekst in de gekozen taal, eigen gedeelde knoptekst in die taal, standaardbericht uit de gekozen taalbron, toepasselijke eigen tekst uit de standaardtaal. Ontbreekt ook die eigen tekst, dan volgt het bronbericht uit de standaardtaal en als laatste het ingebouwde Engelse bericht. Terugval en ontbrekende vertaling blijven herkenbaar; Studio slaat de terugvaltekst niet automatisch als vertaling op.

Standaardberichten uit complete taalbestanden zijn geen ontbrekende eigen vertalingen. De teller meldt een ontbrekende vertaling wanneer er een toepasselijke eigen standaardtekst is zonder eigen vertaling in de gekozen taal, wanneer een eigen tekst leeg is, of wanneer een noodzakelijk bronbericht ontbreekt. Een geërfde knop telt niet opnieuw op iedere pagina; lokale afwijkingen tellen afzonderlijk. Een lege optionele tooltip zonder eigen standaardtooltip is toegestaan: dat veroorzaakt geen lege zichtbare plek.

Herstellen verwijdert uitsluitend de afwijking voor de gekozen taal en scope. Lokale knoppen vallen terug op gedeelde waarden; andere velden en gedeelde knoppen op de geldende terugval of het taalbestand. Tooltip heeft een eigen herstelactie.

Bestaande teksten uit formaten 1–5 blijven in hun oorspronkelijke taal bewaard, inclusief lege ruwe waarden. Lege vereiste waarden worden vanaf nu veilig weergegeven en geëxporteerd via de terugvalregel. Nieuwe projecten gebruiken standaardberichten totdat de gebruiker een eigen tekst instelt. Opslag krijgt formaat 6 met gecontroleerde migratie, zonder bestaande taalvolgorde, status, identiteit of bestands-/mapinstellingen te wijzigen.

Formaat 6 voegt TextOverrides en StandardTexts per projecttaal toe. De oorspronkelijke taal houdt haar bestaande tekstvelden; StandardTexts registreert welke daarvan zijn hersteld naar de bron. Andere talen bewaren afwijkingen onder vaste veldidentifiers. Bij migratie uit formaat 5 worden bestaande teksten niet op basis van inhoud als standaardtekst geraden: zij blijven eigen teksten. Daardoor kan een bestaand project meer ontbrekende eigen vertalingen melden dan een nieuw project. Herstellen kan zulke bestaande standaardachtige teksten bewust weer aan de bron koppelen.

Eigen titel-, body-, knop- en tooltipteksten ondersteunen {AppName} en {AppVersion}. Dit zijn Studio-verwijzingen die bij weergave en export eenmaal naar letterlijke projectwaarden worden vertaald. Onbekende verwijzingen blijven letterlijke tekst; controle daarvan volgt in S04. Het installatiepad gebruikt zijn bestaande Inno Setup-constanten en wordt hiermee niet gewijzigd.

Een eigen taalbestand kan standaardberichten missen. Inno Setup behandelt dit zelf ook: de compiler waarschuwt bij een ontbrekend bericht en gebruikt Engels uit Default.isl. Studio houdt het gevraagde projectvangnet aan; de aansluiting van meertalige export op deze regel hoort bij S05. Lokale bronreferentie: samples/issrc-main/Projects/Src/Compiler.SetupCompiler.pas, ReadMessages, en Compiler.Messages.pas, SCompilerMessagesMissingMessageWarning.

Het logo linksboven gebruikt een WPF DrawingImage-resource, rechtstreeks afgeleid van de paden en gebruikte gradient uit AppIcon.svg. Het oorspronkelijke SVG en ICO blijven behouden; er is geen extra bibliotheek nodig.

## Afbakening en controles

Afhankelijk: geaccepteerde S02. Koppeling: FR-08 en FR-09. Bestandsgroepen: tekstresolutie/model/opslag, EditorSession, bestaande WPF-eigenschappen/projecttalen, resources, gerichte tests en documentatie. Meertalige export blijft voor S05; bestaande uitsluitend Engelse/Nederlandse export gebruikt nu dezelfde resolutieregel als de preview. Er worden geen nieuwe wizardcontrols of stylingtrucs toegevoegd.

Automatisch: terugval bij ontbrekend/leeg veld, gedeeld/lokaal en herstellen, veranderende standaardtaal, uitschakelen zonder tekstverlies, opslag en migratie uit formaat 5, letterlijke projectverwijzingen, onafhankelijke Studio-/previewtaal, stabiele bindings, Release-build en relevante compilatieproeven. Visueel doorklikken en installers starten doet de gebruiker.

## Handmatige acceptatie

1. Nieuw project met Engels en Nederlands: standaardteksten volgen de gekozen taal zonder waarschuwing voor complete bronberichten. Bewerk een Engelse titel; Nederlands toont het Nederlandse standaardbericht en blijft de ontbrekende eigen vertaling melden.
2. Vul een Nederlandse vertaling in: de teller vermindert en de herkomst wordt lokaal/eigen. Maak de vertaling leeg: de standaardtekst verschijnt in de wizard, met waarschuwing, terwijl het lege invoerveld de grijze watermark toont. Controleer dat Overnemen en Herstellen nooit tegelijk zichtbaar zijn. Overnemen vult het veld; Herstellen wist alleen de Nederlandse afwijking.
3. Wijzig een gedeelde knop in Basisontwerp en daarna lokaal op één pagina. Controleer overerving, eigen vertaling, terugval en herstellen op beide pagina's.
4. Gebruik {AppName} en {AppVersion}, wijzig applicatiegegevens en controleer preview. Bouw met uitsluitend de oorspronkelijke Engelse/Nederlandse taal actief en controleer de echte wizardtekst.
5. Zet een andere actieve taal bovenaan en controleer de benoemde standaardtaal/terugval. Schakel een taal uit en weer in, bewaar/heropen en controleer dat de eigen vertalingen behouden blijven.
6. Open een bestaand formaat-5-project met meerdere talen: geen tekst, taalkeuze of volgorde verdwijnt. Wissel Studio-taal en Voorbeeldtaal in Ontwerpen en Uitproberen; projectteksten en tijdelijke mapinvoer veranderen niet door de wissel.

## Verificatie bij oplevering

Release-build geslaagd zonder fouten of waarschuwingen. Alle 155 tests slagen, zonder overgeslagen tests. Zeventien gerichte teksttests controleren onder meer ontbrekend/leeg versus bronbericht, standaardtaal wisselen, gedeeld/lokaal/resetten, tooltipherstel, expliciet overnemen, geen modelwijziging door Studio-/previewtaal, behoud van uitgeschakelde vertalingen, formaatmigratie 5 → 6 en verplichte tekstgegevens. Ook bronberichten die na opmaak alleen witruimte bevatten krijgen zichtbare terugval.

De nieuwe ISCC 7.1.0-proef compileert eigen tekst met {AppName}/{AppVersion}, een productnaam met apostrof/ampersand en een lege body met terugval. De bestaande compilerproeven, inclusief SVG, slagen eveneens. Geen installer of GUI is automatisch gestart; runtime/klikcontrole blijft voor de gebruiker. De bestaande test die een lege lokale Verder-knop verwachtte is aangepast aan de nieuwe gebruikersregel: de ruwe lege waarde blijft opgeslagen, maar de export gebruikt de gedeelde tekst.

Markdown is naar Obsidian gespiegeld en gecontroleerd. Gebruikersakkoord ontvangen; de slice wordt gecommit en gepusht op de milestonebranch. Samenvoegen volgt pas bij afronding van de milestone.

Controle door gebruiker: terugval, tekstbewerking en appicoon werken correct. De eigen tekst- en tooltipvelden zijn expliciet bovenaan uitgelijnd, zodat ook de cursor bij een leeg veld bovenaan begint. De algemene stijl voor korte invoervelden blijft behouden.
