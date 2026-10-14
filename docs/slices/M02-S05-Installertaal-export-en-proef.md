# M02-S05 — Installertaalexport en echte taalproef

Status: door de gebruiker getest en geaccepteerd op m02-installertalen.

## Gebruikersresultaat en afbakening

Exporteren en Bouwen gebruiken alle ingeschakelde projecttalen, in projectvolgorde. Een uitsluitend Nederlandstalig project is geldig, ook wanneer de oorspronkelijke opslagtaal Engels is. Uitgeschakelde talen en hun eigen teksten blijven in het project maar worden niet geëxporteerd.

Elke taal verwijst naar haar gekozen .isl-bron via de reguliere [Languages]-sectie. Externe relatieve bronpaden worden bij export vanuit de projectmap absoluut gemaakt; compiler:-bronnen blijven compilergebonden. Ontbrekende of ongeschikte externe bronpaden krijgen een begrijpelijke melding met taal en opgeslagen bronpad. Eén .isl-bron per taal: komma, aanhalingstekens, accolades en jokertekens in bronpaden worden geweigerd; compiler:-paden mogen de compilermap niet verlaten. Bronnen worden niet gewijzigd. Een los geëxporteerd .iss-bestand blijft afhankelijk van de vermelde bronbestanden en Inno Setup 7.1.

De native taalkeuze gebruikt de reguliere Inno Setup-detectie en taalkeuzedialoog. In Pascal Script selecteert ActiveLanguage de bijbehorende wizardteksten. Titels, bodyteksten, knopafwijkingen en tooltip gebruiken de bestaande S03-resolver, inclusief gekozen-taalbron vóór projectstandaardtaal en ingebouwd Engels. De Studio-/previewtaal is geen exportinstelling. Native Installeren/Voltooien-rollen en andere wizardstappen blijven native. Ampersands in Studio-knoptekst worden voor de native caption ge-escaped, zodat de zichtbare tekst gelijk blijft aan de preview; tooltiptekst blijft letterlijk. Controleer bijvoorbeeld R&D op de Bladeren-knop en in de tooltip.

Inno Setup levert RTL-indeling en taalgebonden lettertypen uit de taalbestanden. De Studio-preview spiegelt nog niet; deze beperking blijft zichtbaar. Compilatie bewijst geen correcte runtime-indeling op elk systeem. Handmatige beoordeling van RTL, langere vertalingen, navigatie en lettertypen blijft noodzakelijk.

Afhankelijk van geaccepteerde S04; koppeling FR-08 en FR-10. Bestandsgroepen: taalexport, wizardgeneratie, exportmeldingen/resources, tests en documentatie. Geen nieuwe wizardpagina's, taalkeuzebeleid-instellingen of opslagmigratie.

Officiële koppelingen: [Languages](https://jrsoftware.org/ishelp/topic_languagessection.htm), [ActiveLanguage](https://jrsoftware.org/ishelp/topic_isxfunc_activelanguage.htm), [ShowLanguageDialog](https://jrsoftware.org/ishelp/topic_setup_showlanguagedialog.htm) en [LangOptions](https://jrsoftware.org/ishelp/topic_langoptionssection.htm).

## Controles en acceptatie

Automatisch: uitsluitend Nederlands, ingeschakelde taalvolgorde, veilige taalidentifiers, scope/terugval/refverwerking, externe bronnen en zelfstandige export, geen modelwijziging door generatie, volledige regressie, Release-build en compilatieproeven met ISCC 7.1. De agent start geen installer of GUI.

1. Maak Engels/Nederlands/Duits actief. Geef elke taal een herkenbare welkomsttitel en body, met {AppName}/{AppVersion}. Bouw; start de installer zelf en controleer iedere taalkeuze, beide huidige pagina's en terug-/verdernavigatie.
2. Laat een eigen vertaling ontbreken: controleer dat de echte wizard het standaardbericht in de gekozen taal gebruikt, gelijk aan de preview. Controleer ook gedeelde en lokale knopteksten en de Bladeren-tooltip.
3. Schakel alle talen behalve Nederlands uit, zonder de oorspronkelijke Engelse opslagtaal te verwijderen. Exporteer en compileer ook buiten Studio; de installer is uitsluitend Nederlands.
4. Wijzig Studio-/previewtaal zonder projectteksten te wijzigen. Exportinhoud blijft gelijk. Verander taalvolgorde of schakel een taal uit: de [Languages]-sectie volgt uitsluitend de actieve projectkeuze.
5. Test een eigen .isl-bron; controleer de bronverwijzing en vergelijk eigen titel/terugval. Een verplaatste of verdwenen externe bron geeft een duidelijke fout. Bronnen blijven ongewijzigd.
6. Neem een beschikbare RTL-taal op en controleer de echte wizard op leesrichting, lettertypen, tekstlengte en knopbediening. De waarschuwing voor de niet-gespiegelde Studio-preview blijft staan. Dit runtimebewijs is een handmatige controle, geen gevolg van een geslaagde compilatie.

## Milestonecheck

M02-S01–S05 zijn geaccepteerd. S05 is gecommit en gepusht als f526bc7. De complete milestone is daarna geaccepteerd en naar main gemerged als dc4ba08. Bekende previewbeperkingen moeten daarbij zichtbaar geregistreerd blijven.

## Verificatie

Release-build geslaagd met nul fouten en nul waarschuwingen. Alle 187 tests slagen, zonder overgeslagen tests. Twaalf nieuwe exporttests omvatten actieve talen, volgorde, model-/selectieonafhankelijkheid, eigen/gedeeld/lokaal en terugval, veilige identifiers, ongeschikte bronpaden, relatieve/missende bronbestanden, context bij bronfouten en letterlijke ampersands op knoppen versus tooltip. De eerdere tijdelijke exportblokkeringsproef is vervangen door het uiteindelijke taalgedrag.

Drie nieuwe compilerproeven gebruiken ISCC 7.1.0: één proef bouwt achtereenvolgens Engels/Nederlands/Duits/Hebreeuws en uitsluitend Nederlands; één exporteert een eigen Nederlandse bron met relatieve naam en puntkomma, en compileert het losse script vanuit een andere map zonder Studio; één bouwt een onvolledige eigen taal met LanguageID=0, verwachte native waarschuwingen en de eigen projectterugval. Bronbytes van de compilerbestanden en externe kopie blijven gelijk. Ook de bestaande compiler-/SVG-proeven slagen.

Geen installer of GUI is automatisch gestart. De gebruiker heeft alle handmatige tests afgerond, de juiste teksten in het .iss-bestand bevestigd en de installer met verschillende talen gebouwd en uitgevoerd. Alles werkt zoals verwacht; M02-S05 is geaccepteerd. De specifieke geteste talen zijn niet afzonderlijk geregistreerd. De previewwaarschuwing voor RTL blijft zichtbaar. De gebruiker heeft M02 op 14 oktober 2026 als geheel geaccepteerd. M02 is naar main gemerged en gepusht als dc4ba08.

Documentatie is bijgewerkt en gespiegeld naar Obsidian; lokale links en kopie-inhoud zijn gecontroleerd. Slice en milestone zijn geaccepteerd en gepubliceerd.
