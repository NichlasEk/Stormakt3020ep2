# Handoff · Stormakt 3020: Atlands arv

Uppdaterad 2026-09-10. Läs denna först vid återupptagning. Äldre avsnitt i PROGRESS och README är historik och kan innehålla redan genomförda ”nästa steg”.

## Checkpoint och skyddsvärda filer

- Repo: `/home/nichlas/Stormakt3020ep2`.
- Origin: https://github.com/NichlasEk/Stormakt3020ep2, gren `main`.
- Senaste speländring: `0093eb5` — väntrummets målade port och gångyta. Pushad.
- Föregående milstolpar: `ad4907b` — spelbar Ebba och återförening; `1876ca3` — Saltkällan.
- Aktuell verifierad Linux-export: `dist/AtlandsArv-0.24.1-port/`; `play.sh` pekar hit.
- Vid denna handoff var endast användarens två PDF-filer ospårade: `Atlantica I - Olaus Rudbeck.pdf` och `gupea_2077_70416_1.pdf`. Lämna dem orörda och utanför commit.
- Originalspelet under `/home/nichlas/SystemRegisIII/` ska inte byggas om eller skrivas över.
- Assets använder Git LFS. Exporter i `dist/` följer inte med Git; en push är inte en backup av Linux-exporten eller användarens sparningar.

## Starta och fortsätt

```sh
cd /home/nichlas/Stormakt3020ep2
./play.sh                         # vanlig kampanj
./play.sh -- --rescue             # separat Ebba-prov, fortsätter dess sparning
./play.sh -- --rescue-new         # nystart av separat prov
./start.sh -- --rescue            # bygg/starta från källkod
```

Behåll samma startargument som användaren tidigare använde: provkapitlen har egna sparfiler. Normal kampanj använder `user://quay-save.json`; Ebba-provet använder `user://rescue-preview-save.json`. F5 sparar manuellt, F9 laddar den manuella sparningen. Exakta sökvägar och övriga provprofiler finns i `scripts/Main.cs`, `SavePath` och `ManualPath`. Radera inte sparningar för att lösa en passagebugg.

E/B interagerar, TAB byter vapen, I öppnar inventarium och C växlar stats. Dev-inställningen låter aktuell hjälte ta skada men överleva på 1 liv.

Håll **hela exportmappen tillsammans**, även `data_AtlandsArv_linuxbsd_x86_64/`. Utan DLL-mappen får användaren ”.NET assemblies not found”. Föregående export `dist/AtlandsArv-0.24-ebba/` finns som återgång.

SHA-256 för aktuell export, kontrollerade vid handoff:

```text
d776f83ae09d82cfa1295fb3d70ad7ab8a77fe66a4cc4f9d42244fae3665d8cc  AtlandsArv.x86_64
8740414824e1afb423e0e2ed4b611aa433afa785b610b0124007289db95852c7  AtlandsArv.pck
9d6e38f54a23399ffaec4eceb643068084b6525d904bf19c3cf8cbee4e7c64f4  data_AtlandsArv_linuxbsd_x86_64/AtlandsArv.dll
```

## Senaste användarläge och rättning

Användaren stod i **De överfördas väntrum**, vid vänstra kanten av den stora högra porten, och hade svårt att gå igenom. Målet visade ”Till Kungaminnet · följ Saltkällans högra port”. Porten i just väntrummet leder först till västra vägen, inte direkt till Saltkällan.

Länken `gamla-west-entry` går från `Gamla.Registry` till `West.Control`. Den målade öppningen var bredare än gångytan, och den smala anslutningen överlappade rummets golv dåligt. Nu är öppningen 144 i stället för 72 bildpunkter, anslutningen sträcks 100 enheter in mot golvet och övergångszonen tar emot från sidan. Dörrblad och kollision använder samma bredd. Befintliga sparningar fungerar; användaren behöver starta om spelet med samma kommando, inte starta om kapitlet.

Kod: `scripts/ConnectedWorld.cs` (`MouthWidth`, `PassageFor`), `scripts/PaintedPassages.cs`. Regression: `tests/WestTests.cs`, `scripts/WestChecks.cs`. Testerna går nu faktiskt från golvet i fyra olika sidolägen, kontrollerar stängd dörr och går därefter genom öppnad port. Tidigare teleportering till portmunnen missade felet.

Rutt: väntrummets högra port → Den västra kontrollgången → Västra vågen → Rummet bakom vågen → högra porten till Saltkällan. Efter Saltkällans avrapportering och ny briefing hos Ebba går fortsättningen genom Saltkällans högra port till Kungaminnet. Från Uppsala används fregattens kajuta och roder för resan till Gamla Uppsala.

## Vad som är byggt och berättelsens slutpunkt

41 beständiga platser, 57 musikspår, målade rum med dörrövergångar och djupskalning. Senaste kapitlet har fyra nya rum, 24 svenska röstklipp, fem musikstycken och återföreningsfilmen **Bara du och jag**.

Karl fångas av Kungaminnets kvarhållningsmekanism. Spelaren tar över Ebba ombord, hämtar hennes utrustning och går genom servicevägen till Sigillmästaren. Hennes pistol kan avbryta bossens sikte; pelare stoppar skott. Efter segern måste pulpeten användas för att frige Karl. Samtalen återför kontrollen till Karl, följs av romantisk återförening och avrapportering ombord. **Kungaminnets innersta valv är ännu inte byggt.**

Karl och Ebba har separata utrustnings- och hälsotillstånd. Ebba är spelbar i detta kapitel, inte en fritt valbar hjälte genom hela kampanjen. Ändra inte enumordning eller sparformat utan migrationskontroll. Bossens nödvändiga sigill garanteras av berättelsetillstånd, inte slumpmässigt byte.

Kanon: kartan visar vägen; gjutformen tillverkar stjärnplåten; stjärnplåten riktar in minnesmaskineriet. Kanslisigillets behörighet är en separat sak. Lägg inte till en bekräftad koppling till Rotmarskalkens ed utan att först utveckla den i storyboarden.

## Verifierat — skilj versionerna åt

På **0.24.1-port** passerade byggning utan varningar/fel, 116 västra-vägen-kontroller, 86 räddningskontroller och native passageprov både från källprojektet och från den exporterade Linux-versionen.

På **0.24-ebba**, före portpatchen, passerade hela regelsviten med 117923 kontroller, native räddningsprov och exportprov. Räddningsprovet omfattar hjältebyte, utrustning/sparning, boss, radiokö, filmuppspelning och överhoppning. Musikprovet omfattar alla 57 strömmar, tyst inträde i biblioteket, exklusiv provlyssning och återställning. Hela regelsviten har **inte** körts om efter portpatchen.

Röster har filkontrollerats och granskats genom ungefärlig CPU-Whisper-transkription; mänsklig provlyssning återstår. Automatiska tester är inte en full mänsklig genomspelning. Bildprov från portfixen: `artifacts/registry-door-approach.png` och `artifacts/registry-door-entering.png`. Temporära loggar i `/tmp/registry-*.log` kan försvinna.

## Rekommenderad nästa arbetsordning

1. Läs användarens återkoppling på portfixen och Ebba-kapitlet. Kontrollera aktuell Git-status och sparprofil före ändringar.
2. Putsa vägledning: Uppsala kan fortfarande visa det gamla målet ”Vittnena är trygga · ordern hos Ebba”. Mellanrummens mål borde säga nästa lokala dörr/resmål; långa mål kapas i HUD. Detta är ännu inte rättat.
3. Prova övriga målade portar från verkliga gångytor, särskilt sidointräde. Undvik tester som bara placerar figuren direkt på övergången.
4. Bedöm Ebbas gång, ansikte, pistol och röster i spel. Användarens provspelning är viktig för animationernas kvalitet.
5. Utveckla sedan innersta valvet i storyboarden och bygg nästa sammanhängande etapp. Bevara återföreningens efterspel.

Bildriktning: rika genererade, slitna målade bakgrunder; seriösa vuxna 2D-figurer och läsbar skala. Ingen gullig/plastig stil. Humorn ligger i absurd handling och radio, inte töntig anatomi. Användaren avvisade fattigare modulmiljöer och synliga broar mellan scener. Resor mellan skilda platser sker med båt eller Karls seriöst gestaltade rymdfregatt. Ebba möter Karl ombord på rymdskeppet.

## Dokument och kod att börja med

- [EBBA-RESCUE.md](EBBA-RESCUE.md): hela spelvägen, repliker, hjältebyte, assets och portpatch.
- [STORYBOARD.md](STORYBOARD.md): gemensam handling och vad som är genomfört/föreslaget.
- [SALTSPRING.md](SALTSPRING.md), [WESTERN-SCALE.md](WESTERN-SCALE.md): föregående etapper.
- [SOUNDTRACK.md](SOUNDTRACK.md), [ROADMAP.md](ROADMAP.md), [PROGRESS.md](PROGRESS.md): musik och historiska milstolpar.
- `scripts/Rescue.cs`, `scripts/RescuePresentation.cs`: kapitelregler, hjältetillstånd och presentation.
- `scripts/NarrativePresentation.cs`: radiokö, berättelsefilm och efterspel.
- `assets/story/rescue-radio.json`, `music.json`, `films.json`: innehållskällor.
- `assets/source/`: produktionsmanifest, råljud, musikmaster och filmarbetsflöden; inte med i spelexporten.

## Bygga och kontrollera

```sh
dotnet build --nologo -v:q
dotnet run --project tests -- --west-only
dotnet run --project tests -- --rescue-only
# Hela regelsviten vid bredare speländringar:
dotnet run --project tests
# Import respektive native prov:
godot-mono --headless --editor --path "$PWD" --import
godot-mono --path "$PWD" -- --west-check
godot-mono --path "$PWD" -- --rescue-check
godot-mono --path "$PWD" -- --music-check
```

Godot Mono 4.7.2, .NET/net8.0. Exportera till en ny versionsmapp, provkör den exporterade binären och uppdatera därefter `play.sh`. Skriv inte över en körande export. Exempel, byt versionsnamnet:

```sh
mkdir -p dist/AtlandsArv-NEXT
godot-mono --headless --path "$PWD" --export-release Linux dist/AtlandsArv-NEXT/AtlandsArv.x86_64
```

Commit/push efter verifierade milstolpar är redan önskat. Stagea uttryckliga filer; håll PDF:er och användarsparningar utanför. Kontrollera att push inklusive LFS verkligen lyckas.

## Lokal medieproduktion vid behov

Inga pågående genereringsjobb behöver återupptas för denna checkpoint. Befintliga assets är inkopplade.

- Röster: `tools/generate_rescue_voices.py`, `tools/retake_rescue_voices.py`, `tools/check_voices.py`; VoxCPM2 via lokal broker på 8765.
- Musik: `tools/generate_music_suite.py` och `tools/check_music_suite.py`; ACE-Step på 8001. Kapitelnycklar: `rescue-hall`, `rescue-prison`, `rescue-service`, `rescue-machine`, `boss-censor`.
- Film: `tools/render_rescue.py`, `tools/assemble_rescue.py`; LTX/Comfy på 8198, profil `/home/nichlas/ai/comfy-profiles/ltx-2`.
- Läs respektive skript och kontrollera aktuell tjänstestatus före körning. Kör en tung GPU-produktion i taget och avbryt inte användarens andra jobb. Bevara etablerade syntetiska rollröster och produktionsmanifest.
