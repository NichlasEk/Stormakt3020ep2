# Tillgångarnas ursprung

Alla episodens figurer, scener, repliker och musik är nya produktioner. Originalets bilder används som identitets- och stilreferenser, inte som runtime-sprites. AI-tjänster behövs bara vid produktion.

## Aktuella bilder

- `art/likvarvet-scale-v5.png`: inbyggd bildgenerering, redigering av episodens matta v4-kaj. Mindre vardagsföremål och tre plana minneshällar; samma huvudsakliga gångyta. Visuellt granskat i Godot, inte en exakt måttsatt 3D-modell.
- `art/serious-cast-v6.png`: godkänd Karl/sabel och dansk vakt, tre stridsposer. Ursprunglig prompt i `source/rough-pass/serious-cast-prompt.md`.
- `art/serious-specialists-v1.png`: nya målade poser för Karl/hammare, pikenerare, skytt och indrivare. Samma stil som v6. Tre kolumner och fyra rader, faktiskt utfall 1086 × 1448. Bildstorleken läses vid körning.
- `art/radio-cast-v1.png`: nya porträtt av Ebba Grip, Hedvig Rålamb och indrivaren. Originalets `stormakt-radio-portraits-v1.png` ger Ebbas identitet; episodens specialistark ger indrivarens. Hedvig är en ny roll. Bilden är målad på mörk bakgrund och behöver ingen chroma-friläggning.

Ovanstående bilder använder det inbyggda bildverktyget. Produktionsbriefar, referenser och SHA-256 finns i `source/names-pass/`. `SpriteCutout.cs` bygger riktig alpha för magenta figurark, rensar kantfärg och skapar mipmaps. `PaintedCast.cs` normaliserar ståhöjd och placerar fötter; tre poser och horisontell spegling är inte fullständig gång-/riktningsanimation.

`art/ebba-radio-v3.png` är en separat revision av Ebba efter användarens önskemål om att återge originalets fylligare byst. Ansikte, hår och seriös målad stil utgår från det nya porträttet; fylligare silhuett och måttlig uniformsurringning inspirerad av originalets vuxna Ebba. Den används för Ebba i runtime; Hedvig och indrivaren läses fortsatt från atlasen.

## Röster, musik och effekter

- `audio/voice-*.ogg`: 16 svenska repliker, tre helt syntetiska rollreferenser (Ebba, Hedvig, indrivaren) via lokal VoxCPM2. Replikerna produceras med Dots MF via EutherLink. Manusbegäran, rå-WAV, jobb-ID och hash ligger i `source/voices/`. Inga verkliga personers röstprov används. Hedvigs första längre tagningar finns under `source/voices/rejected/`; runtime använder kortare repliker för tydligare leverans.
- `audio/score.ogg`, `boss-score.ogg`, `names-score.ogg`: lokala ACE-Step 1.5 Turbo-generationer. Råmastrar, begäranden, resultat och looprecept finns i `source/`. Den nya fyndmusiken har 48 sekunders råmaster och 40 sekunders runtime-loop med överlappad skarv. Slutets tystnad i råmastern används inte.
- `audio/scrape.ogg`, `inscription.ogg`, `paper.ogg`: egen deterministisk ljuddesign i `tools/build_discovery_sfx.py`, rå-WAV i `source/`.
- Övriga stridseffekter och hamnatmosfär: egen deterministisk syntes i `tools/build_sfx.py`.
- `fonts/NotoSerif-Regular.ttf`, `NotoSans-Regular.ttf`: installerade Noto-fonter. SIL Open Font License finns i `fonts/LICENSE`.

AI-källor bevarar begäran och utfall; identisk prompt garanterar inte identisk bild eller musik. Färdiga runtime-filer versioneras med Git LFS.

## Tidigare studier — inte aktuell runtime

`likvarvet.png`, `likvarvet-oil-v2.png` och lokala Krea/ComfyUI-försök har sparade grafer och prompter. `likvarvet-matte-v4.png` är den tidigare valda matta revisionen med inbyggd bildgenerering. Nuvarande v5 fortsätter från den.

`karl-saber.png`, `karl-hammer.png`, `guard.png`, `pikeman.png`, `gunner.png`, `collector.png` är äldre egen Blender-geometri, åtta riktningar och sju poser, via `render_actors.py` och `pack_actors.py`. De ersattes i runtime av den målade figurpresentationen efter användarens stilkorrigering.

`karl-combat-matte-v5.png`, `guard-combat-matte-v5.png` och `source/concepts/karl-oil-study-v3.png` är tidigare figurstudier. Den gamla shaderbaserade ljushetsmasken används inte längre. Äldre och avvisade bildpass har sina produktionsfiler under `source/rough-pass/`.

Godot, .NET, Blender, ComfyUI och modeller har egna licenser; träningsmodeller distribueras inte med spelet. Godots motorlicens: https://godotengine.org/license/

Den tidigare helt täckande `ebba-radio-v2.png` bevaras som föregående variant. V3 använder v2 för ansiktet och originalets radioporträtt för uniformsutformningen.
