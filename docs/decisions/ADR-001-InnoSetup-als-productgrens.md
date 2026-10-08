# ADR-001 — Inno Setup als productgrens

Datum: 8 oktober 2026.
Status: geaccepteerd op 8 oktober 2026, inclusief concrete productdekking.

## Context

De huidige visuele werkplek is bruikbaar bevonden. Het product wordt een add-on voor Inno Setup 7.1. De gebruiker wil reguliere Inno Setup-mogelijkheden ondersteunen en geen oplossingen die beperkingen met kunstgrepen omzeilen.

## Besluit

Iedere productinstelling heeft een gedocumenteerde vertaling naar de ondersteunde Inno Setup-versie. Het gewone script en de officiële native API's vormen de grens. Een proof in de doelcompiler en waar relevant de echte installer gaat vooraf aan de claim dat iets werkt.

Officieel Pascal Script is een regulier uitbreidingsmiddel, maar geen reden om vervangende controls, DLL-schermframeworks of externe runtimes te introduceren. De hoofdplanning stelt beperkte standaard eigen invoerpagina's voor; de volledige vrije-controlontwerper valt daarbuiten.

De preview biedt uitsluitend gecontroleerde mogelijkheden of herkenbare benaderingen. Globale instellingen mogen niet als vrije lokale afwijkingen worden gepresenteerd.

## Overwogen alternatieven

Een eigen installatie-engine of installer-runtime biedt meer vrijheid, maar vergroot het product en verlaat de door de gebruiker gekozen richting. Een WPF-preview met onbewezen properties is snel uit te breiden, maar kan een onjuiste verwachting scheppen. Beide worden verworpen als productaanpak.

## Gevolgen

Een mooie editor betekent niet onbeperkte vormgeving. Onhaalbare instellingen verdwijnen uit het aanbod met uitleg. Veelgebruikte opties krijgen visuele bediening; geavanceerde reguliere opties kunnen een meer technische weergave krijgen.

De [ondersteuningsmatrix](../InnoSetup-ondersteuning.md) bewaart bereik en verificatie. Een nieuwe Inno Setup-versie krijgt een bewuste compatibiliteitscontrole.
