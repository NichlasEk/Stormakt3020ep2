# Vittnesgalleriet och Edskammaren · 0.7

2026-09-07. Rumsprovet har sex sammanhängande rum och sin första egna boss. Den långa åttastegskampanjen är fortfarande separat.

## Spela

```bash
./start.sh -- --rooms
```

Eller kör `dist/AtlandsArv-0.7-oath/AtlandsArv.x86_64 -- --rooms`. Meny: **Atlands förseglade rum · prov**. Äldre två-/fyrarumssparningar fortsätter i den utökade rutten. E/B undersöker och passerar; R visar ledtrådar, I inventarium, C stats.

Förgård → logement → pumphus → Vittnesgalleriet → Edskammaren. Pumphus → cistern → förgård är den valfria fynd-/genvägsloopen. Pumpens tryck måste avlastas innan matarhjulet vrids. Galleriets vittnesbok förklarar hur eden kan lösas och öppnar kammarens försegling.

Efter bossen: plocka upp **Den tomma kronans hjälm**, gå till den inre porten och tryck E. Portmålningen öppnas och rutten markeras säkrad. Alla rum kan återbesökas. Minnets arkiv bakom porten är nästa anslutning, ännu inte spelbart i denna rutt.

## Edsväktaren

Egen `OathGuardian`-roll, fyra egna målade 2D-poser och separat tillståndsmaskin. 720 liv. Bossen låser en riktning, visar en bred rusmarkering och rusar med skölden. Locka linjen mot en ringmärkt stenpelare och gå undan. Pelarträff ger 2,8 sekunders sänkt sköld och 1,65 × vapenskada; annars dämpar eden skadan till 18 %. Vanlig skada är möjlig, men långsam.

Under halvt liv blir förvarningen 0,95 i stället för 1,25 sekunder och ruset snabbare. En rusning ger högst en kroppsträff. Pelarna förbrukas aldrig, så mekaniken kan upprepas. Hammarslag kan inte permanent låsa bossen i generisk vackling. Båda vapnen och understöden klarar mötet i automatiska inputtester utan utvecklarskydd.

Pelarna och galleriets läspulpet är solida. Samma flershindergeometri styr gång, vägval, sikt och vanliga direkta attacker/projektiler. Bossens rus använder små svepta delsteg för att inte hoppa genom sten eller Karl. Målade förgrundsmasker ger rätt överlappning framför/bakom pulpet och pelare.

## Sparning och bilder

Layoutversion 3 migrerar version 1 via version 2. Inaktiva rum behåller sina fiender, fynd och utforskningsmasker. Vittnesbok, bossliv, låst rusriktning, exponering, unik belöning och slutflagga sparas. Rumsbyte återställer inte resurser. Gamla kampanjsparningar får ingen ny rutt automatiskt.

Nya assets från det inbyggda `image_gen`-verktyget, kopierade oförändrade till projektet:

- `assets/art/room-gallery-v1.png` — galleriet.
- `assets/art/room-chamber-v1.png` — edskammare med stängd inre port.
- `assets/art/room-chamber-open-v1.png` — samma rum, öppnad port.
- `assets/art/room-pump-low-v2.png` — torrlagt pumphus med synlig sidopassage.
- `assets/art/oath-guardian-v1.png` — gard, rus, exponering, fallen kropp.

[Fulla promptar, referenser och ursprung](../assets/source/rooms/oath-art-v1.json). Bossatlasen förankras i fotpunkter och friläggs med spelets befintliga magentanyckling. Inga Blender-/3D-assets används i tillägget.

## Verifiering

`dotnet run --project tests/Atland.Tests.csproj`: **29 274 assertions**. Inkluderar äldre kampanj-/inventarie-/rums-/siktfall, migration 1→3 och 2→3, sparning mitt i rus, verklig skydds-/exponeringsskada, blockerad flykt, belöning utan duplicering och beständigt återbesök. Åtta nya inputrutter kombinerar sabel/hammare, artilleri/fältvård samt med/utan cistern. De krävde 2–3 pelarträffar. Detta mäter regler och framkomlighet, inte mänsklig speltid eller upplevd svårighet.

Build: 0 varningar/fel. Både Godot och den fristående Linux-exporten passerar `--oath-check`: 3 811 ticks, 70 liv, utan utvecklarskydd. Senaste båda körningarna avslutar med kod 0 och endast Xvfb-drivrutinens V-Sync-varning; tidigare intermittenta avslutsvarningar är inte generellt utredda.

Exportens `--fog-check` passerar också, inklusive exakt bildlikhet med/utan dold boss och loot. Den körningen rapporterar de sedan tidigare observerade avslutsvarningarna (16 ObjectDB-instanser/8 resurser) och avslutar med kod 0. Logg: `artifacts/oath-fog-export.log`.

`--oath-check` kör hela sexrumsrutten med verkliga kontroller i Godot och fångar galleriet, rusförvarning, exponering, öppen port och journal. För förvarningsbilden flyttar bildprovet tillfälligt Karl till en fri observationspunkt och återställer honom innan simuleringen fortsätter; testspelaren lockar normalt bossen från bakom en pelare. Bildprovet använder ingen vanlig sparfil.

Loggar: `artifacts/oath-tests.log`, `oath-build.log`, `oath-native-editor.log`, `oath-export.log`, `oath-native-export.log`. Bilder finns under `artifacts/oath-*.png`; exportens egna bilder ligger i dess `artifacts/`.

## Återstår

- Mänsklig bedömning av svårighet, tempo, skala och stridsläsbarhet.
- Bossens sammanhängande gång-/riktningsanimationer; nu fyra fasta målade poser med spegling.
- Första rumsambienserna, bossljuden och inspelad radiodialog är levererade i 0.7.1; se `ROOM-AUDIO.md`. Mänsklig provlyssning återstår.
- Rörliga dörrblad/övergångar; öppet/stängt visas nu med bildtillstånd.
- Anslutning till Minnets arkiv och kampanjens fortsättning, därefter rotmarkernas rumsrutt och Rotmarskalken enligt `ROADMAP.md`.
