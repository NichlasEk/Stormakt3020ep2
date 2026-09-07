# Rumsradio och underjordiskt ljud · 0.7.1

2026-09-07. Ljudpass för den befintliga sexrumsrutten, med åtta nya inspelade repliker och sju egna effekt-/miljöljud. Ingen ny bana läggs till i denna checkpoint.

## Hörbart i spelet

| Händelse | Röst / ljud |
| --- | --- |
| Ny rumsrutt | Ebba söker planritningen; Kollegiet frågar vilket århundrade ärendet gäller |
| Första besöket i pumphuset | Ebba förklarar tryckavlastningen |
| Tryckavlastning / tömning | Två olika mekaniska ljud; Hedvig beskriver de två vägarna |
| Cisternens fynd | Hedvig läser vägen tillbaka |
| Vittnesboken | Hedvig förklarar pelaren och den lösta eden |
| Första sköldruset | Ebba förklarar låst riktning och skadeöppning |
| Varje rus / pelarträff | Sköldlås, rörelse och resonant brons mot sten |
| Besegrad boss | Ebba kommenterar hjälmen och Kollegiets påstående att platsen var obemannad |
| Öppnad inre port | Stenportens mekanik; Hedvig pekar vidare mot arkivet |

Berättelsehändelser utlöses en gång med befintliga sparade framsteg. Bossens första råd använder hans sparade attackräknare. Redan passerade händelser spelas inte upp igen vid laddning. Befintliga porträtt och tydliga undertexter används. Vittnesboken fortsätter över ett dörrbyte, medan gamla artilleriorder och passerade råd rensas. Rumsrutten spelar inte längre den gamla kajintroduktionen om två sigill; vid laddning visas rummets aktuella mål.

Havsmiljön tonas över till ett 23 sekunder långt loopat ljud av luft och låga rörresonanser i inomhusrummen. Galleriet/cisternen använder befintlig upptäcktsmusik; Edsväktaren använder befintlig bossmusik. Ingen ny musikkomposition i detta pass. Befintligt volymreglage och sparad ljudnivå gäller allt.

Sköldens ljud kan höras inom 500 världspixlar även bakom en pelare, men dess partiklar/text ritas bara vid synlig träffpunkt. Det gör träffen hörbar utan att rita dolda fiender genom dimman.

## Produktion och kvalitet

- Röster: lokal EutherLink och `dots.tts-mf`, med de befintliga syntetiska Ebba-/Hedvig-referenserna. Fyra Ebba-tagningar med 4 steg; fyra omtagna och kortade Hedvig-repliker med 8 steg.
- Manus: `assets/story/rooms-radio.json`. Rendera saknade filer med `python3 tools/generate_voices.py --rooms-only`.
- Käll-WAV, prompt och jobbmanifest: `assets/source/voices/rooms-*`.
- [Samlat röstmanifest](../assets/source/room-audio/voice-manifest.json): text, roll, referens, jobb, runtime-hash, speltid, toppnivå och Whisper-transkription.
- Originaleffekter: deterministisk PCM-syntes med `python3 tools/build_room_audio.py`; inga externa samplingar. Källor och [manifest](../assets/source/room-audio/manifest.json) under `assets/source/room-audio/`.
- Alla 15 spelassets ligger under `assets/audio/`. Rösterna filtreras varsamt och normaliseras mot −18 LUFS, −2 dBTP före Vorbis-kodning. Avkodade filer har kontrollerats mot klippning.

Whisper small på lokal CPU har transkriberat alla åtta repliker. Första Hedvig-tagningarna kortades efter uteblivna/otydliga ord. Slutkontrollen har fortfarande avvikelser, bland annat Karl/kal, Eden/är den och sköld/sköl. Detta är automatisk screening, inte bevis på perfekt uttal; mänsklig provlyssning av uttal och mix återstår. Korrekt manus visas som undertext.

## Verifiering och start

`dotnet run --project tests/Atland.Tests.csproj`: **29 290 assertions**. Hela rutten verifierar att alla åtta berättelsehändelser utlöses exakt en gång, pumpreglage låter en gång, bossljud är särskilda och äldre kajbriefing inte återkommer. De tidigare kampanj-, inventarie-, rums-, migrations- och bossfallen passerar också.

Godot och Linux-exportens `--room-audio-check` passerar: sex rum utan utvecklarskydd, samtliga röster laddas och startar i den riktiga ljudspelaren, undertexter matchar manus, sju effekt-/miljöklipp laddas, miljöljudet tonas åt båda håll och radiokön hanterar dörrar/bossdöd. Porträtt/undertext har bildgranskats. Kontrollen spelar tyst via testläget.

Export: `dist/AtlandsArv-0.7.1-radio/AtlandsArv.x86_64`. Vanlig start: `./start.sh -- --rooms`. Körningen fortsätter från din befintliga rumssparning.

Loggar: `artifacts/rooms-audio-tests.log`, `rooms-audio-native-editor.log`, `rooms-audio-native-export.log`, `rooms-voice-qa.log`. Native-bild: `dist/AtlandsArv-0.7.1-radio/artifacts/rooms-radio-hedvig.png`. Bygget har 0 varningar/fel. Editorprovet rapporterade tidigare kända avslutsvarningar (24 ObjectDB/12 resurser); sista fristående körningen avslutar med kod 0 och endast Xvfb-drivrutinens V-Sync-varning. Resursvarningarnas grundorsak är fortfarande inte utredd.

Nästa byggsteg finns i `ARCHIVE-CONNECTION.md`.
