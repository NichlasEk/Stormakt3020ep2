# Fysiska dörrar och standardvägen · 0.11

2026-09-07. Ett spelbart prov med förgård och logement i samma målade miljö. Karl och vakten delar koordinater, navigation och simulering. Dörröppningen byter inte scen och kräver inget extra knapptryck för genomgång.

## Tjock ekport · 0.11.1

Dörrbladet har nu fram- och baksida, en synlig överkant och ändträ på båda kortsidorna. Alla ytor följer samma isometriska gångjärnsprojektion. Den målade tjockleken ryms i den befintliga kollisionsmarginalen; navigation och sparformat är oförändrade. Befintlig ektextur återanvänds med olika utsnitt och matt skuggning. Native bildkontroll omfattar även halvöppen dörr. Export: `dist/AtlandsArv-0.11.1-oak`. Bygget och native dörrkontrollen passerar med avslutningskod 0; bilderna från slutexporten är granskade. Logg: `artifacts/door-thickness-export-native.log`.

## Prova

Huvudmenyn → **Fysiska dörrar · spelprov**, eller:

```bash
./play.sh -- --doors
```

Nyckeln ligger på gården till vänster om Karl. E/B hämtar den och låser upp/öppnar dörren. Gå genom öppningen med WASD eller styrspaken. E/B stänger igen från dörröppningen eller dörrbladet. Vakten aktiveras av sikt eller dörrhugg och kan följa efter genom öppningen. Stäng dörren för att stoppa honom. Han bryter ännu inte upp den själv.

Vänster mus/J hugger, höger mus/K ger tungt slag; Tab/RB byter till hammare. Slag ger ljud och skada. Hammaren är effektivare mot träet. En förstörd dörr lämnar målade brädor och kan inte stängas igen. Dörrbladet stannar om en levande figur står i svepytan. Gå undan för att låta rörelsen slutföras.

Provet har egna `door-trial-save.json` och `door-trial-manual-save.json` (F5/F9). Nyckel, dörrens skador, öppningsläge, fiende och utforskning består. För att börja om just provet, inklusive återställd dörr/vakt:

```bash
./play.sh -- --doors-new
```

Det ersätter provets sparning, med vanlig `.bak`-backup. Kampanj- och rumssparningar påverkas inte. Dev-inställningen som låter Karl överleva på ett liv fungerar också här. R visar provets förklaring.

## Standardvägen

Huvudmenyns primära knapp är nu **Spela expeditionen · nio rum**. Den öppnar den senast spelade rumsexpeditionen: den separata rumssparningen eller en nyare landstigningssparning som redan nått rumsvägen. Det ursprungliga landstigningsavsnittet finns kvar som ett eget tydligt menyval.

Vanlig landstigning fortsätter från stranden till de nio rummen. Samma Combat-objekt behåller utrustning, stash, fyndhistorik, hälsa och förbrukningsvaror; övergången skapar ingen ny karaktär eller gratis läkning. Rumsbilderna laddas vid övergång och återupptagning. Landstigningen behåller sin sparplats, så en separat rumsexpedition inte skrivs över. Redan påbörjade äldre åttastegskampanjer migreras inte tyst; deras menyval finns kvar märkt Äldre kampanj.

**De fysiska dörrarna är än så länge ett separat två-rumsprov.** Niorumsruttens befintliga passager är ännu inte ombyggda. Sprängladdningar, fler material, fiender som bryter dörrar och fortsättningen till pumphuset ingår inte i detta prov. Nästa steg är användarens bedömning av skala, passning, rörelse och stridskänsla innan samma teknik flyttas in i huvudrutten.

## Bild och teknik

`assets/art/door-courtyard-v1.png` är en ny gemensam målad miljö med tom öppning. `assets/art/door-oak-face-v1.png` är dörrens plana målade yta. Båda genererades med det inbyggda bildverktyget. Exakta promptar finns i `assets/source/door-trial-manifest-v1.json`.

Ingen Blender behövdes för detta prov. En deterministisk isometrisk gångjärnsprojektion driver både den målade dörrytan och kollisionslinjen. Ytan ritas som 16 texturerade remsor med djupsortering; de befintliga målade väggarna ritas om i smala remsor framför/bakom figurer. Dörren har ingen fri fysiksimulering. Blender kan senare användas för att baka mer komplicerade portmekanismer till 2D-bildrutor.

Samma dynamiska hinder används av gång, undanmanöver, navigation, närstrid, skott och markens sikt. Aktiva fiender finns samtidigt i båda rummen. Mörkret använder samma sparade utforskningsmask som huvudrutten. Föremålets geometriska linje och renderade gångjärn har en gemensam definition i `PhysicalDoors.cs`.

Fyra nya korta syntetiserade ljudskisser: lås, gnissel, träträff och brott. Reproduktion via `tools/generate_door_sounds.py`; WAV-källor under `assets/source/`, Ogg under `assets/audio/`. De följer spelets ljudreglage. Inspelad Foley kan ersätta skisserna senare.

## Verifiering

- .NET-bygge utan fel eller varningar.
- 29 593 regelassertions, inklusive kontinuerlig genomgång, aktiv förföljelse, nyckel, lås, öppna/stänga, blockerad svepyta, förstöring utan nyckel, sparning, projektilkollision, stoppad undanmanöver samt bevarade resurser vid strandens nya fortsättning.
- `--door-check`: native Godot-bilder av stängd/öppen/förstörd dörr och interiör, animation, huvudmenyns standardknapp och den verkliga fortsättningshanteraren. Bilder och loggar under `artifacts/door-*`.
- Linux-export `dist/AtlandsArv-0.11-doors/AtlandsArv.x86_64` passerar dörrkontrollen med avslutningskod 0; boss-, arkiv-, Rotvägs- och filmkontrollerna passerar också. `play.sh` pekar på denna verifierade export. Exportkontroller loggas som `artifacts/door-export-native.log` och `artifacts/door-export-cinematics.log`.
- Mänsklig provspelning av gångjärn, passning, ljud och stridskänsla återstår. Perspektivet är fast och provets golv är nyritat; ingen sömlös sammanfogning av alla nio ursprungliga målningar har gjorts.
