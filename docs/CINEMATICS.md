# Arkivets port · första filmprovet · 0.9.1

2026-09-07. En lokal LTX-2-film på cirka åtta sekunder är integrerad mellan Minnets arkiv och Rotvägen. Kedjan spänns, stenpanelen glider undan och dagsljus avslöjar stora rötter utanför. Filmen har genererat miljöljud, utan dialog. Detta dokument beskriver första checkpointen. Introt och den gemensamma filmkatalogen tillkom i [0.10](INTRO.md).

## Titta och spela

**Inställningar → Utvecklarverktyg · Videobibliotek → Arkivets port → Spela upp.** Biblioteket går att nå från huvudmenyn eller pausmenyn. Överlevnadsskyddet behöver inte vara påslaget. Uppspelning där ändrar inte sparning, progression eller utrustning. Naturligt slut, Esc, handkontrollens B/Start och knappen Hoppa över återvänder till biblioteket. Därifrån går Till inställningar tillbaka rätt, även när biblioteket öppnades från ett pausat spel.

Direkt filmprov från terminal:

```bash
./start.sh -- --gate-film
```

Detta återvänder till huvudmenyn. Normal rutt: `./start.sh -- --rooms`, sedan E/B vid arkivets säkrade grind. Filmen visas vid första passagen till Rotvägen. En sparning som redan har besökt Rotvägen spelar inte om scenen automatiskt; använd biblioteket.

Spelets volymreglage, M och +/− styr filmens ljud. Musik och miljöljud från spelet tystas under filmen; radiokön väntar tills spelet fortsätter. Simuleringen står still. Karl befinner sig redan vid Rotvägens ankomstpunkt och övergången har sparats innan uppspelningen är färdig. Ett avbrott kan därför inte duplicera fiender, fynd eller belöningar. Rummets befintliga Visited-flagga styr engångsvisningen; inget nytt sparformat behövs.

## Produktionsfiler

- Första bildruta, genererad med det inbyggda bildverktyget: `assets/source/cinematics/archive-gate-keyframe-v1.png`.
- Originalvideo från lokal ComfyUI/LTX-2: `assets/source/cinematics/archive-gate-ltx-v1.mp4`.
- Spelversion: `assets/video/archive-gate-v1.ogv`, 768×512, cirka 8,1 sekunder, Theora/Vorbis. Bildens proportioner bevaras i spelaren.
- Fullständig bildprompt, källor, hashvärden och videoproveniens: `assets/source/cinematics/archive-gate-manifest-v1.json`.
- Exakt LTX-prompt/nodgraf och jobbresultat: `archive-gate-ltx-v1.json`, `archive-gate-job-v1.json`, `archive-gate-history-v1.json` i samma mapp.
- Lokal reproduktion: `tools/render_archive_gate.py`, befintlig LTX-2-profil på port 8198 och EutherLinks GPU-kö. Scriptet förutsätter att keyframen finns i profilens input som `ep2-archive-gate-v1.png`. Följ CPU/GPU-minnet före körning. Denna körning använde en reserverad GPU-plats, avlastade den lediga VoxCPM-modellen och avslutade sin egen LTX-server efteråt.

Filmen använder en enda distilled-pass med åtta steg och 193 bildrutor vid 24 fps, utan uppskalningspass. Det är ett första rörelse-/stämningsprov i modest upplösning. Portens öppning är tydlig, men yttergårdens nytillkomna stenytor är slätare och mer fotografiska än spelmålningarna; det är ett konkret förbättringsmål inför ett större intro. Ingen ny 3D-figur eller spelanimation ingår.

Ljudet normaliseras försiktigt och tonas in/ut. Mätt i den slutliga OGV-filen: medelnivå −25,7 dB, max −13,3 dB. Inga repliker kräver undertexter. Mänsklig provlyssning av stämning och mix återstår.

## Verifiering

- Bygge utan fel eller varningar.
- **29 574 regelassertions**: tidigare rutten samt exakt en filmhändelse vid första passagen och ingen vid återbesök.
- Godot `--cinematic-check`: tidigare boss, arkiv och Rotvägen, sedan verklig videodekodning, bildtextur, fryst spelstatus, utvecklarbibliotek, återspelning, Esc/B, naturligt slut och återgång till spelet via berättelsehändelse.
- Native kontroll av mastervolym och mute utan att skriva användarens inställningar. Bilder från biblioteket och uppspelningen har granskats; inställningslayouten har också granskats i slutexporten.
- Filmen kräver riktig tid i testet: använd `--max-fps 60`, inte snabbspolad `--fixed-fps`. Video-/ljudklockan följer faktisk uppspelning, medan fast delta annars kan hinna löpa ifrån den.

Linux-export: `dist/AtlandsArv-0.9.1-cinematic/AtlandsArv.x86_64`. Samma kontroll passerar på Linux-exporten med avslutningskod 0. Slutkontrollens logg: `artifacts/cinematic-native-final.log`. Förhandsvisningsbilder hamnar under exportens `artifacts/`. Vid saknad film fortsätter rumsövergången, och en reservgräns på filmens kataloglängd plus tolv sekunder (sedan 0.10) förhindrar att ett uteblivet slutmeddelande låser spelaren.
