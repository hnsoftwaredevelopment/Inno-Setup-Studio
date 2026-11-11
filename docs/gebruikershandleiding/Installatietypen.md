# Installatietypen

Met installatietypen bied je de gebruiker een paar begrijpelijke keuzes aan. Denk aan **Volledig** voor je programma met alle extra onderdelen, **Compact** voor alleen de basis en **Aangepast** voor zelf onderdelen kiezen. Voor een klein programma zonder optionele onderdelen heb je dit niet nodig; je mag de lijst leeg laten.

Typen verschijnen op de componentenpagina zodra er ook [componenten](Componenten.md) zijn ingesteld. Alleen een type toevoegen verandert niet welke bestanden worden geïnstalleerd. Koppelingen tussen componenten en installatiebestanden volgen in een latere stap.

## Een type toevoegen

1. Open **Installatietypen → Installatietypen beheren** en klik op **Toevoegen**.
2. Vul bij **Standaardbeschrijving** bijvoorbeeld `Volledige installatie` in. Dit is de tekst die de gebruiker straks ziet.
3. Geef het type een unieke **Interne naam**, bijvoorbeeld `full`. Die naam gebruik je later om onderdelen aan dit type te koppelen. Hij hoeft niet te worden vertaald.
4. Voeg eventueel een tweede type toe, bijvoorbeeld `compact` met de beschrijving `Compacte installatie`.
5. Gebruik **Omhoog** en **Omlaag** om de volgorde te bepalen.

Een interne naam begint met een letter of underscore en bevat daarna alleen letters, cijfers of underscores. Gebruik geen spaties. `full` en `FULL` tellen als dezelfde naam. De woorden `and`, `or` en `not` zijn gereserveerd. Studio begrenst de naam op 128 tekens en het overzicht op 1000 typen.

De geselecteerde rij blijft bij opslaan en heropenen geselecteerd. Dat is je plek in het ontwerpoverzicht, geen instelling die de installer verplicht met dat type laat beginnen. Inno Setup bepaalt de selectie in de installer en kan daarbij een eerdere installatiekeuze gebruiken.

## Aangepast type

Vink **Aangepast type** aan voor bijvoorbeeld de beschrijving `Aangepaste installatie` met de interne naam `custom`. Daarmee geef je aan welk type hoort bij het zelf aan- en uitzetten van afzonderlijke componenten. Er mag maar één aangepast type zijn.

Zonder aangepast type kan de gebruiker straks wel een vooraf ingesteld type kiezen, maar niet zelf vrij onderdelen aan- en uitvinken. De onderdelen en hun typekoppelingen stel je in bij Componenten. De koppeling aan bestanden volgt later.

## Beschrijvingen vertalen

Direct onder de standaardbeschrijving staat **Vertalingen**, standaard opengeklapt. Kies een actieve installertaal en vul de beschrijving in die je voor die taal wilt gebruiken. Selecteer daarna de volgende taal en herhaal dit voor iedere taal waarvoor je een eigen beschrijving wilt. Bijvoorbeeld `Full installation` voor Engels en `Vollständige Installation` voor Duits.

Laat je een vertaling leeg, dan gebruikt de installer de standaardbeschrijving. De grijze tekst in het lege veld laat zien welke beschrijving wordt gebruikt. Wis een ingevulde vertaling om weer op die standaard terug te vallen. De standaardbeschrijving is verplicht; de installer krijgt zo geen lege typebeschrijving.

Deze vertalingen horen bij de installertalen van je project. Wisselen van de Studio-taal vertaalt je eigen beschrijvingen niet. Een installertaal uitschakelen bewaart haar vertalingen. Verwijder je een taal, dan worden ook haar typevertalingen verwijderd via de bestaande bevestiging.

Beschrijving en vertaling zijn gewone teksten op één regel, maximaal 4096 tekens. Een rode foutmelding geeft aan welke invoer je moet herstellen voordat je opslaat of exporteert.
