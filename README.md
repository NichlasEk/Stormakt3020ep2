# Stormakt 3020: Atlands arv

Episod II. Ett nytt, fristående actionrollspel i Stormakts värld: nordisk barock, karoliner, mytologi och Rudbecks Atland.

[Nästa arbetsmål och nulägesanalys](docs/ROADMAP.md): sammanhängande banor med flera målade rum, dörrar, upptäcktsmörker och egna bossmekaniker. Atlands port och Rotvägen har nu nio beständiga rum.

Starta senaste verifierade Linux-versionen med `./play.sh` (ingen ombyggnad). `./play.sh -- --gate-film` visar filmprovet direkt. Exportens programfil, `.pck` och `data_AtlandsArv_linuxbsd_x86_64` måste ligga tillsammans; `play.sh` kontrollerar detta innan start. `./start.sh` bygger och kör från källkoden.

## Fix · dörrens skarv · 0.11.2

Karl och danska vakter kunde ta sig genom en lucka vid den stängda dörrens karm. Skarvarna överlappar nu och figurernas hela förflyttning kontrolleras. Nytt prov: `./play.sh -- --doors-new`. [Detaljer och regressionstester](docs/PHYSICAL-DOORS.md).

## Nytt · tjock ekport · 0.11.1

Dörrprovet har nu målad fram- och baksida, överkant och ändträ som följer öppningsrörelsen. `./play.sh -- --doors` fortsätter provet; `--doors-new` återställer just dess sparning. [Detaljer](docs/PHYSICAL-DOORS.md). Sammanhängande karta med öppna passager är nästa steg.

## Nytt · fysiska dörrar och standardvägen · 0.11

**Spela expeditionen · nio rum** är nu huvudvalet. Landstigningens fortsättning går också till rumsvägen, med bevarad utrustning och resurser. Ett separat spelprov har två sammanhängande rum och en målad fysisk dörr: nyckel, öppna/stänga, verklig genomgång, förföljelse, kollision/sikt och förstöring med vapen. **Fysiska dörrar · spelprov** eller `./play.sh -- --doors`. `--doors-new` börjar om just provet. [Omfattning, produktion och verifiering](docs/PHYSICAL-DOORS.md).

Linux-export: `dist/AtlandsArv-0.11-doors/AtlandsArv.x86_64`. Övriga dörrar och sprängladdningar återstår.

## Nytt · Expeditionen till Atland · 0.10

Ett intro på 24 sekunder: skeppet, bronskartan och kusten, med tre nya Ebba-repliker, musik och svenska undertexter. Både introt och Arkivets port finns i **Inställningar → Utvecklarverktyg · Videobibliotek**. Direktprov: `./play.sh -- --intro-film`. Introt visas vid en ny Atland-expedition; befintliga sparningar fortsätter som vanligt. [Produktion och verifiering](docs/INTRO.md).

Linux-export: `dist/AtlandsArv-0.10-intro/AtlandsArv.x86_64`.

## Nytt · Arkivets port och videobibliotek · 0.9.1

Första mellansekvensen är ett lokalt genererat filmprov på åtta sekunder. Den visas en gång vid passagen från arkivet till Rotvägen. **Inställningar → Utvecklarverktyg · Videobibliotek** låter dig se den igen utan att ändra sparningen. Esc/B hoppar över; filmens ljud följer volymreglaget. Direktprov: `./start.sh -- --gate-film`. [Produktion och verifiering](docs/CINEMATICS.md).

Linux-export: `dist/AtlandsArv-0.9.1-cinematic/AtlandsArv.x86_64`. Detta var första filmprovet; introt tillkom i 0.10.

## Nytt · Rotvägen · 0.9

Två nya målade rum efter arkivets grind: Rötternas trappa med motviktsport och De namnlösas lund med avtryck, eftertrupper och vittnessigill. Arkivvalet avgör om en eller två eftertrupper hinner fram. Fem nya svenska Ebba-/Hedvig-repliker. Nio rum går att återbesöka med bevarad utrustning, stash, fiender och fynd. [Innehåll, bilder och verifiering](docs/ROOTWAY.md).

`./start.sh -- --rooms` fortsätter din rumssparning. E/B vid den säkrade arkivgrinden går vidare. Äldre sparningar migrerar utan omstart. Linux-export: `dist/AtlandsArv-0.9-rootway/AtlandsArv.x86_64 -- --rooms`. Slutpunkten är lundens sigill; nästa expedition mot berget återstår.

## Nytt · Minnets arkiv · 0.8

Sjunde rummet ligger bakom Edsväktarens öppnade port. E/B vid läsbordet jämför två motstridiga handlingar på en egen målad dokumentvy. Bevara vittnesmålet och möt tre vakter, eller förfalska en passersedel och möt en kontrollant. Valet och stridens resultat sparas. Fyra nya svenska radiorepliker från Ebba och Hedvig följer besöket. [Omfattning, assets och verifiering](docs/ARCHIVE.md).

Starta med `./start.sh -- --rooms` och fortsätt din rumssparning. Vid den öppnade bossporten: tryck E/B igen för att gå ned. Äldre rumssparningar uppgraderas utan omstart; utrustning, stash och kvarlämnade fynd följer med. Efter arkivstriden säkrar du grinden med E/B och kan gå tillbaka. Rotvägen bortom grinden är nästa byggsteg.

Linux-export: `dist/AtlandsArv-0.8-archive/AtlandsArv.x86_64 -- --rooms`.

## Nytt · Ebba, Hedvig och rumsljud · 0.7.1

Åtta nya svenska radiorepliker från de lokala AI-rösterna och sju egna ljud för pumpar, portar, sköldrus och underjord. Ledtrådar följer med genom dörrar; stridsråd rensas när de blivit inaktuella. [Ljudpass och verifiering](docs/ROOM-AUDIO.md). Linux-export: `dist/AtlandsArv-0.7.1-radio/AtlandsArv.x86_64`.

## Nytt · sex rum och Edsväktaren · 0.7

Vittnesgalleriet och Edskammaren har egna rika målningar. Läs vittnesboken och locka Edsväktarens sköldrus mot en edspelare för att bryta hans gard. Bossen har egna målade 2D-poser, en utrustningsbelöning och en port som öppnas efter striden. Cisternens fynd och genväg är valfria. [Rutt, boss och verifiering](docs/OATH-ROOMS.md).

`./start.sh -- --rooms` eller **Atlands förseglade rum · prov**. Sex rum med beständiga fiender/fynd, dörrkrav, upptäcktsmörker och utforskningskarta. Äldre rumssparningar får de nya rummen utan omstart; separat sparfil från kampanjen. Linux-export: `dist/AtlandsArv-0.7-oath/AtlandsArv.x86_64`.

0.7 slutade vid den öppnade inre porten; 0.8 fortsätter till arkivet. Full bossgång återstår; rumsradion och första ljudpasset finns i 0.7.1. [Vattenmekanism](docs/WATER-ROOMS.md) · [Sikt](docs/EXPLORATION.md) · [Grundsystem](docs/ROOMS.md).

## Nytt · inventarium och utrustning 0.5.1

**I** öppnar/stänger inventariet, **C** öppnar/stänger stats. Målad inventariebakgrund och 14 egna föremålsbilder med större förhandsvisning. Utrusta vapen, rustning, hjälm och sigill, jämför bonusar och flytta föremål mellan 24 väskplatser och 60 stashplatser. Spelet pausas. Stashöverföringar kräver ett säkrat område; **E/B** plockar upp fynd från fiender. Äldre sparningar fungerar och får ett grundset automatiskt. [System och kontroller](docs/INVENTORY.md).

## Nytt · den målade Atlands port är tillbaka · 0.4.2

`./start.sh -- --port` eller **Spela Atlands port** i menyn. Den ursprungliga rika bakgrundsmålningen är återställd, och det modulära provets skiljemur och kollision är borttagna. Karl och fienderna är fortfarande målade 2D-sprites. Gömman och separata sparningar finns kvar. E/B undersöker, R visar ledtråden.

## Spela 0.4

Välj **Spela nästa del · åtta banor** för att hoppa direkt till den nya expeditionen genom Atland, rotmarkerna, Bergslagens underjord och det felvända Uppsala. Den har en egen sparfil och fortsätter där du slutade. En ny landstigning leder också vidare genom alla åtta banor. Äldre avslutade 0.3-sparningar kan fortsätta in i Atland.

Fyra nya målade miljöer, två banor i varje, med mekanismer, försvarsvågor, bossar och ett arkivval som påverkar senare motstånd och slutet. Detta är ett breddpass: detaljföremål, nya fiendeutseenden och nya röster kommer i ett separat assetpass. Se [banöversikten](docs/CAMPAIGN.md).

## Spela första landstigningen

Från projektmappen:

```bash
./start.sh
```

`F11` växlar helskärm. Välj **Gå i land**, välj artilleri eller fältvård och börja landstigningen.

Spelprov 0.3, **Vägen under vattnet**: kajens sigill, indrivare, bronskarta och tre strukna namn följs nu av kronans magasin och en övervuxen strand. Hitta brynstålet, läs kollegiets mätorder, öppna grinden och pröva kartan mot tre riktningar i landskapet. Ebba och Hedvig följer upptäckten över radion. Slutet avslöjar en väg under vattnet — och att någon var här före expeditionen.

Välj **Öva sabelduell** i huvudmenyn för att prova sabel, parad och undanmanöver mot en vakt. Övningen skriver inte över kampanjens sparningar. Det är fortfarande ett spelprov under utveckling.

| Handling | Tangentbord/mus | Handkontroll |
|---|---|---|
| Gå | WASD / pilar | Vänster spak |
| Sikta | Mus | Höger spak, annars gångriktning |
| Hugg | Vänsterklick / J | X |
| Tung attack | Högerklick / K | Y |
| Undanmanöver | Space | A |
| Gard / tajmad parad | Shift | LB |
| Byt sabel/hammare | Tab | RB |
| Tinktur | Q | Styrkors upp |
| Understöd | F | Styrkors ned |
| Undersök / frilägg inskrift | E / håll E stillastående | B / håll B |
| Läs fynd i fältdagboken | R | Back / Select |
| Ljudvolym | Synligt reglage eller +/−; M tyst/ljud | Inställningar i pausmenyn |
| Paus | Esc | Start |
| Manuell sparning/laddning | F5 / F9 | Sparning i pausmenyn |

En tajmad parad skickar tillbaka skyttens kula. Hammaren och tunga attacker bryter pikenerarens gard. Artilleriet träffar framför Karl i siktriktningen. Fältvård ger två extra tinkturer och återkommande läkning.

**Utvecklarläge:** ESC → Inställningar → **Utvecklarläge: överlevnad PÅ**. Karl tappar liv och reagerar på träffar, men stannar på minst 1 liv. Läget är av från början, sparas i inställningarna och gäller även efter laddning. Stäng av det för vanlig dödlig skada. En DEV-markering syns under volymreglaget när skyddet är aktivt.

Startvolymen är 25 %. Reglaget går att dra direkt under platsnamnet; M återställer tidigare ljudnivå efter tyst läge. Volymen sparas mellan starter.

Kontrollpunkter sparas vid inskrifter, vägval, nya områden, fynd och mätningar. Delvis frilagda inskrifter behåller arbetet när du släpper knappen eller sparar manuellt. Befintliga avslutade 0.1-sparningar förblir avslutade; välj **Ny landstigning** för det nya uppdraget. Ett avslutat 0.2-läge med tre lästa namn kan fortsätta via **Fortsätt genom magasinet** i slutmenyn. F5 skriver ett separat manuellt läge, F9 laddar det. Dödsmenyn återgår till kontrollpunkten. Varje sparfil har checksumma, atomisk skrivning och en föregående säkerhetskopia. Sparfiler ligger i Godots användarkatalog för spelet, helt separat från originalet.

## Utveckling

Den godkända seriösa figurstilen används nu i spelet för samtliga stridsroller. `./start-art-study.sh --fullscreen` visar samma figurer och kaj; mellanslag växlar poser, G visar måttreferensen. Alla vuxna använder cirka 150 världspixlars ståhöjd. Kajföremålen har målats om i mindre skala. Karl med sabel och sabelvakten har nya gång- och anfallsnyckelbilder i fyra riktningar, samt försvars- och dödsposer. Gången har fortfarande ojämna mellanbilder; gruvhammar-Karl, indrivaren, pikeneraren och skytten har nu provisoriska gångark, men deras stridsposer och övriga specialistroller använder fortfarande den enklare presentationen. Alla gångcykler behöver ett samlat omtag med riggad rörelsereferens. Murar och props skymmer figurer som befinner sig bakom dem. Produktionsmått, korrigeringar och begränsningar finns i `docs/JOURNEY-PRODUCTION.md`.

Godot **4.7.2 .NET**, .NET 8 SDK eller senare, Linux x64. På Arch: `godot-mono`, `dotnet-sdk`. Versionerna är låsta i projektet och exportmallarna ska matcha.

```bash
dotnet run --project tests/Atland.Tests.csproj
./start.sh -- --smoke
./start.sh --fullscreen -- --integration
./start.sh --fullscreen -- --ui-check
./start.sh --fullscreen -- --scene-check
./start.sh -- --duel
```

`--smoke` sparar en bild efter två sekunders provkörning. `--integration` kör hela mötet med en testspelare genom vanliga stridsregler och fångar kaj, magasin, strand och slutbilder. Testkörningarna är tysta och skriver inga användarsparfiler. `--ui-check` provar volym, mute, sparad ljudnivå, klick utan hugg och fältdagboken samt fångar radioporträtten; dess inställningar ligger separat i `artifacts/`. `--scene-check` fångar överlappning bakom last, murar och sten samt avslöjandets kamerabild. Bilder/loggar hamnar i `artifacts/`.

- `scripts/Combat.cs`: stridsregler utan Godot-beroende.
- `scripts/Main.cs`: presentation, kontroller, menyer och radiokort.
- `scripts/PaintedCast.cs`: gemensam målad figurpresentation och kroppsskala.
- `scripts/FieldNotes.cs`: fyndens originalskrivna vittnesmål.
- `scripts/SaveStore.cs`: sparformat och återhämtning.
- `scripts/Soundscape.cs`: musikövergångar, röster och effekter.
- `docs/VISION.md`: överenskommen kreativ riktning.
- `docs/PROGRESS.md`: aktuell checkpoint och nästa steg.
- `docs/ATLANTICA-NOTES.md`: primärläsning med tryckta sidnummer och spelidéer.
- `tools/`: lokal, reproducerbar assetproduktion.

Stora bilder, ljud och fontfiler använder Git LFS. Efter kloning: `git lfs pull`.

Originalet i `/home/nichlas/WaylandForge` är referensmaterial och ändras inte här. GitHub: https://github.com/NichlasEk/Stormakt3020ep2
# Målat gångprov

`./start-walk-study.sh` visar Karls nya riggade gång som vanliga 2D-bilder på stranden. Vänster/höger byter riktning, mellanslag pausar, F11 helskärm, Esc avslutar. Separat grafikprov under arbete; kampanjens figurer är ännu inte ersatta. Produktionskedjan finns i `docs/ANIMATION.md`.

Senaste assetriktningen är rika genererade bakgrundsmålningar med seriösa 2D-sprites för Karl och fiender. Blender-gångprovet ovan är ett arkiverat experiment, inte fortsatt produktion.
