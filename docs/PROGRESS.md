# Checkpoint · De strukna namnen · 2026-09-06

## Spelbart nu

Godot 4.7.2 .NET, separat från originalet. Spelprov 0.2 utökar Blekinges likvarv: krigsråd med artilleri/fältvård, två kajsigill, fyra vakter, indrivaren, bronskartan, tre inskrifter med två vakter per avläsning, ett konkret vägval och återtåg till båten.

Håll E/B stillastående nära en inskrift. Patrullen kommer endast första gången stenen störs. Fiender inom 160 världspixlar hindrar arbetet; påbörjat arbete bevaras när spelaren avbryter. Tre avlästa stenar och besegrade patruller öppnar vägvalet. R/Back öppnar fynden med stenens vittnesmål jämfört med Ebbas liggare. Olästa fynd avslöjas inte där.

- **Öppen sändning:** tre förföljare, +30 liv, perfekta parader återger 4 liv under återtåget.
- **Krypterad rapport:** två förföljare, +15 liv, extra tinktur, understöd redo direkt.
- Alla förföljare måste besegras före ombordstigning. Namn, framsteg och vägval sparas.

Sabel/hammare, tunga attacker, undanmanöver, gard, projektilparad, understöd och handkontroll finns sedan tidigare. F5/F9 och automatiska kontrollpunkter är separata; checksummade atomiska sparfiler har föregående backup. Enumvärdena för 0.1 är bevarade, och äldre avslutade sparningar förblir avslutade. Starta en ny landstigning för att spela hela 0.2.

## Aktuell bildriktning

Användaren godkände den seriösa figurstilen och därefter den nya spelbilden: ”ser rätt bra ut”, ”såg ju riktigt coolt ut! jag älskart!”. Humorn hör till berättelsen och radion, aldrig till töntig anatomi eller karikatyrer.

Runtime använder nu `serious-cast-v6.png` för Karl/sabel och vakter samt `serious-specialists-v1.png` för Karl/hammare, pikenerare, skytt och indrivare. De tidigare Blender-figurerna används inte längre i spelet. `PaintedCast.cs` används även av bildstudien och normaliserar ståhöjden till cirka 150 världspixlar. `SpriteCutout.cs` frilägger magenta och rensar kantfärg innan mipmaps/filtrering.

**Begränsning:** tre stridsposer och speglad sidriktning. Fullständiga gångcykler, åtta riktningar, riktiga dödsanimationer och mellanbilder är ännu inte byggda. Presentera inte detta som färdig animation.

`likvarvet-scale-v5.png` ersätter v4: mindre lyktor, tunnare förtöjningar, finare stenläggning och tre plana minneshällar. Spelpunkterna har justerats till de målade hällarna. Kroppsskalan är nu gemensam, i stället för att förstora figurer mot överstora props. Bildgeneratorn följde inte alla exakta pixelmått; detta är en visuellt granskad förbättring, inte en fullständig metrisk 3D-miljö. G i bildstudien visar ståhöjdsreferensen 1,78 m / 150 px.

## Radio och ljud

`radio-cast-v1.png` innehåller tre nya målade porträtt: Ebba Grip, Hedvig Rålamb och indrivaren. Ebbas blonda flätade hår, blå ögon och blå officersidentitet utgår från originalets porträtt; ytan är ommålad i episodens allvarliga stil. Indrivaren matchar sin stridsfigur. Ebbas separata `ebba-radio-v3.png` återger därefter originalets fylligare byst och måttliga urringning på uttrycklig begäran, med samma vuxna ansikte och seriösa stil. Hedvig är en ny, cirka 55-årig antikvarie med egen identitet, äldre låg röst och eftertänksam replikföring.

16 svenska repliker totalt, varav 10 nya i detta steg. Tre egna syntetiska rollreferenser via VoxCPM2, repliker via Dots MF på lokal EutherLink. Referenser, manusbegäranden, råljud och jobbmanifest ligger i `assets/source/voices/`. Inga verkliga personers röstprov används. Porträtt och undertext följer talaren. Föråldrade stridsrepliker rensas vid fyndövergången.

Tre lokala ACE-Step-musikstycken. Nya `names-score.ogg` är en 40-sekunders kammarmusikloop för inskrifterna, med bevarad 48-sekunders råmaster och dokumenterad överlappning. Musik tonas mellan kaj, fynd och strid och dämpas under tal. Nya författade effekter: borste mot sten, inskriftsfynd och pappersvändning. Ingen AI-tjänst behövs när spelet körs.

Användaren tyckte ljudet var för högt. Standardvolymen är sänkt från 75 till 25 procent. Synligt dragreglage i spelbilden och menyerna; M växlar tyst/ljud, +/− ändrar i femprocentsteg. Regleringen sparas, och klick på reglaget utlöser inte attacker. Automatiska testkörningar är tysta. UI-testet använder separat settingsfil i artifacts.

## Rudbeck och berättelse

Läs `docs/ATLANTICA-NOTES.md` för primärhänvisningar. Minne, tal och skrift (tryckt s. 543–544) bär det första fyndet. Ingrid Jonsdotter, Mats Eriksson och Siri Nilsdotter och deras inskrifter är originalskriven fiktion, inte citat ur Atlantica. Hedvig kopplar kartans linjer till landskapet. Underjordiskt Atland är vår fiktion; Rudbeck läser vissa underjordsresor geografiskt.

Användarens två PDF-filer i roten är lokalt researchmaterial. De har inte lagts till i Git. De större framtidsspåren — Ymers geologi, Gläsisvall, landskapskalendern och Uppsala — är konceptunderlag, inte färdiga banor.

## Verifiering och fortsatt arbete

- Regeltest: 16 031 assertions. Hela artilleri/öppen-rutten: 14 besegrade, 12 parader, 99 liv kvar, cirka 91 logiska sekunder. Hela fältvård/krypterade rutten: 13 besegrade, 100 liv kvar, cirka 96 sekunder. Detta är botkörning, inte uppskattad mänsklig speltid.
- Sparprov omfattar delvis läst inskrift, utebliven dubbel patrull/belöning, väntan på sista fienden, sparat vägval, återtåg och en äldre sparfil utan de nya fälten.
- Godot/OpenGL-integration når slutet via samma regler. Bilder: gameplay, boss, names, testimony, ending i artifacts.
- UI-kontroll provar mute/återställning, tangent, musdrag/clamp, ingen attack vid reglageklick, settings-reload och fältdagbok. Separata radiobilder för de tre talarna.
- Lokal Whisper-kontroll finns i artifacts. Egennamn och vissa ord får osäkra transkriberingar; det är en uttalskontroll, inte ett påstående om slutlig mänsklig castinggranskning.
- Fysisk handkontroll har inte provats. Den längre Blekinge-missionen, loot/inventory, Varvsänkan, vidare kampanj och filmsekvenser återstår.

Nästa produktionssteg: rörelser och riktningsanimationer från den godkända målade identiteten, därefter nästa sammanhängande miljö med måttsatt grundlayout. Bevara den allvarliga figurstilen, läsbara attacker och den nya radiobesättningen. Utöka inte hela kampanjen på en gång.

## Körning

`./start.sh` startar spelet. `./start-art-study.sh --fullscreen` visar samma bildsystem. Tester: `dotnet run --project tests/Atland.Tests.csproj`, `./start.sh -- --integration`, `./start.sh -- --ui-check`. `--smoke` ger ett kort bildprov. Loggar och fångster ligger i artifacts.

Origin: https://github.com/NichlasEk/Stormakt3020ep2 . Commit/push efter fungerande milstolpar är uttryckligen önskat. Grafik, råljud och runtime-ljud använder Git LFS. Originalet i /home/nichlas/WaylandForge berörs inte.
