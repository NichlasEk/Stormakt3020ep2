# Regementet som stannar · 0.13

2026-09-08. Spelbar fortsättning på den sammanhängande expeditionen.

## Prova

Starta `./play.sh` och fortsätt din rumsexpedition. Efter avtrycket och sigillet i De namnlösas lund leder trappan i målningens nederkant vidare till regementet.

För att komma direkt till det nya innehållet: `./play.sh -- --regiment`, eller Inställningar → **Prova regementet · separat sparning**. Provet börjar i den säkrade lunden, med bevarat vittnesmål som förvalt arkivbeslut. Det använder `regiment-preview-save.json` och egen manuell sparplats. `--regiment-new` börjar om endast detta prov. Ordinarie expedition och utrustning skrivs inte över. Devöverlevnad följer inställningen du redan valt.

## Spelbar väg

Förläggningsstigen → Sjukbaracken → Fanlunden → Mönstringsvallen → Den övergivna bryggan. Gå till fots mellan platserna och tillbaka. Fanlundens reglade återtågsväg går till lunden. Portar, utforskning, fiender, fynd och genvägar sparas.

Kapten Arvid Silfvergren vill ha avlösning för sitt döda kompani. Tala med honom och ta ordern från bordet. Sjukrullan vid bäddarna är frivillig. Visa handlingen vid kontrollen. Bevarade namn eller sjukrullan befriar fanvakterna; en förfalskad passersedel släpper igenom Karl men lämnar två extra vakter vid Rotmarskalken. Sjukrullan kan hämtas senare.

Rotmarskalkens tre fanor kan huggas sönder. När alla rotförbindelser bryts får Karl åtta sekunder att angripa det blottade befälet. Därefter bildas en ny formation. Rotgrepp förvarnas och saktar tillfälligt ned Karl; under halva bossens liv ändras mönstret. Avlösningsordern läses vid mönstringsstenen efter striden. Hammarbelöning och bryggans brigantin kan hämtas utan att låsa återvägen.

E vid båten visar en sex sekunder lång illustrerad överfart till Farleden vid berget. Återresa finns. Detta är en panorerad målning med radio, inte en ny videofilm. Landstigningsplatsen går att utforska; gruvans inre och Järnets lungor är fortfarande nästa byggsteg.

## Grafik och röster

Alla nya målningar producerades med det inbyggda **imagegen**-verktyget. Exakta promptar ligger bredvid respektive PNG som `.prompt.md` i `assets/art/`.

| Assets | Innehåll |
| --- | --- |
| `room-regiment-{trail,barracks,flags,parade,quay,farled}-v1.png` | Sex nya mörka, målade 2D-miljöer |
| `root-marshal-v1.png`, `root-soldier-v1.png` | Sex målade poser per figur, inklusive två gångposer och nederlag |
| `captain-arvid-v1.png` | Världsfigur och porträttutsnitt |
| `regiment-standard-v1.png` | Hel och bruten fana med alpha |
| `world-atland-open-v1.png`, `warehouse-passage-v1.png`, `room-cistern-open-v1.png`, `room-grove-passage-v1.png` | Nya öppna varianter; original bevarade |

Nio nya radioinslag finns i `assets/story/regiment-radio.json` och `assets/audio/voice-regiment-*.ogg`. Ebba och Hedvig återanvänder sina syntetiska röstidentiteter. Arvid har egen syntetisk VoxCPM2-referens; replikerna produceras lokalt via EutherLink/Dots. Kaptenens presentation, avlösningen och bossinstruktionen fick meningsvisa omtag efter transkriptionskontroll. Källjud, request och manifest finns i `assets/source/voices/`; `*-complete.manifest.json` beskriver sammanfogade slutversioner. Slutljud normaliseras till -18 LUFS med -2 dBTP-mål.

## Kontroller och gränser

Slutkontroll: **64 195 assertions** i hela motortestsviten. Bygge utan varningar/fel. Både `--regiment-check` och `--ports-check` passerade med den exporterade Linux-binären; endast Xvfb-drivrutinens väntade VSync-varning förekom.

- Motortester går igenom båda arkivbesluten, nya gångvägar, återtåg, båt fram/åter, sparning mellan rum, belöningsskydd och migrering av publicerad niorumssparning till femton platser.
- Bossboten klarar Rotmarskalken med sabel/hammare och båda understöden utan devöverlevnad. Den testar bossen isolerat; ruttestet kontrollerar vakternas tillkomst och berättelseutfall, inte hela kapitlets mänskliga svårighetskurva.
- Native `--ports-check` provgår alla nio äldre förbindelser. `--regiment-check` provgår fem nya huvudanslutningar, visar kapten/boss/båt och kontrollerar nio ljudklipp mot textningen. Bilder sparas lokalt i `artifacts/`.
- Lokal Whisper-transkription används för att hitta avklippta fraser. Uttal av namn och vissa ord behöver fortfarande bedömas med öronen vid provspelning.
- Mellangångarna har återanvänt murmaterial och är enklare än målningarna. Fullständiga gångkort i alla riktningar, nya kapitelmusikstycken och miljöljud samt gruvans inre ingår inte i denna checkpoint.

Export: `dist/AtlandsArv-0.13-regiment/`, med programfil, PCK och hela .NET-datakatalogen tillsammans.
