# Expeditionen till Atland · 0.10

2026-09-07. Ett 24,2 sekunders intro med tre åttasekundersklipp: örlogsskeppet, bronskartan och kusten. Tre nya svenska Ebba-repliker, textning och befintlig musik blandas med klippens genererade miljöljud.

## Prova

```bash
./play.sh -- --intro-film
```

Eller **Inställningar → Utvecklarverktyg · Videobibliotek → Expeditionen till Atland**. Där finns också Arkivets port. Esc/B/Start eller Hoppa över avslutar. Filmljudet följer volym och mute. Biblioteket ändrar inte sparningen.

Introt ligger efter upptäckten av kartan: det visas vid en ny Atland-kampanj eller ny rumsexpedition, samt första kampanjövergången från stranden. En befintlig sparning återupptas utan intro. Den ursprungliga kajlandstigningen får inte denna film före kartfyndet.

**De nio nya rummen är fortfarande en separat rutt.** Välj huvudmenyns **Atlands förseglade rum · prov** eller `./play.sh -- --rooms`. Där finns dörrar, beständig utforskning, upptäcktsmörker, Edsväktaren, arkivet och Rotvägen. Vanlig kampanjfortsättning använder ännu den äldre åttastegsrutten. Sammanfogningen är nästa integrationsarbete, inte levererad i filmpasset.

## Produktionskällor

- Tre ingångsbilder från det inbyggda bildverktyget: `assets/source/cinematics/intro-{ship,chart,coast}-keyframe-v1.png`.
- Lokala ComfyUI LTX-2-klipp: motsvarande `intro-*-ltx-v1.mp4`. Isolerad LTX-profil, fp8 distilled, åtta steg, 193 bildrutor vid 24 fps och 768×512. GPU-broker reserverades per klipp; renderprocessens egen server avlastades och stoppades efteråt.
- Exakta bildpromptar, hashvärden och jobb: `assets/source/cinematics/intro-manifest-v1.json`. Varje klipp har dessutom nodgraf, jobb och historik i samma katalog.
- Röster: lokal EutherLink/dots.tts-mf, befintlig Ebba-referens. Manus `assets/story/intro-radio.json`, käll-WAV, requests och manifest under `assets/source/voices/intro-*`.
- Reproduktion: `tools/render_intro.py`, `tools/generate_voices.py --intro-only`, därefter `tools/assemble_intro.py`. Renderaren kräver LTX-servern på port 8198 och lokalt GPU-broker-API. Den kopierar ingångsbilder till profilens input och hoppar över färdiga klipp.
- Spelfilm: `assets/video/atland-intro-v1.ogv`, Theora/Vorbis 48 kHz. Mixkommandot finns i `intro-assembly-v1.json`. Befintlig `names-score.ogg` återanvänds; röster spelas med 0,9× hastighet. Mätt slutnivå: medel −22,7 dB, max −3,0 dB.

Filmkatalogen `assets/story/films.json` styr titel, fil, längd, beskrivning och undertexter. Spelaren använder videons faktiska tidsposition för textningen. Biblioteket paginerar tre filmer per sida. Reservgränsen är respektive filmlängd plus tolv sekunder, så det längre introt avbryts inte av den gamla 20-sekundersgränsen.

## Verifiering och begränsningar

- .NET-bygge: noll fel och varningar. Regelkontroller: 29 574 assertions.
- Godot `--cinematic-check` provar katalogknappar, tre texter mot manus, tidsluckor, verklig Theora-dekodning, båda filmerna, upprepning, Esc/B, volym/mute, oförändrat spelstatus och naturligt slut efter hela introt. Tidigare boss-, arkiv- och Rotvägskontroller ingår.
- Samma kontroll passerar med avslutningskod 0 i Linux-exporten `dist/AtlandsArv-0.10-intro`. Loggar: `artifacts/intro-native.log` och `artifacts/intro-export-native.log`. Testet måste använda `--max-fps 60`, inte snabbspolad `--fixed-fps`.
- Bibliotek och bildrutor vid 2, 10 och 18 sekunder har granskats visuellt, liksom sena källklippsrutor. Automatisk CPU-Whisper-kontroll finns i `assets/source/voices/intro-radio-qa.json`. Den skrev ”Carl”, ”uttiken” och ”Ta det i land”; mänsklig provlyssning återstår.
- LTX mjukar upp målningens penseldrag och hittar på kustbyggnader senare i tredje klippet. Filmens slut följer därför inte ingångsbildens geometri exakt. Förbättrad följsamhet och högre upplösning är möjliga senare filmpass. Detta är inte nya 3D-spelfigurer eller gånganimationer.
