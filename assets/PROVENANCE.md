# Tillgångarnas ursprung

Alla speltillgångar i denna episod är nya. Inga originalsprites eller originalrepliker har kopierats från episod I.

- `art/likvarvet-oil-v2.png`: lokalt omarbetad med Krea-2-Turbo genom ComfyUI efter önskemålet om mörk, sliten oljemålning. Bildlayoutens första version `likvarvet.png` bevaras som referens. Exakta API-grafer, prompter, seed och SHA-256 i `source/likvarvet*`. Skript: `tools/generate_art.py` och `tools/repaint_dock.py`.
- `art/karl-saber.png`, `karl-hammer.png`, `guard.png`, `pikeman.png`, `gunner.png`, `collector.png`: egen geometridesign, renderad med Blender Cycles på CPU, sliten materialtextur och varm/kall sidobelysning. Åtta riktningar, sju poser, 256 × 320 pixlar per cell. `tools/render_actors.py` och `tools/pack_actors.py` är fullständig reproduktionskälla. Andra passet har mindre huvuden/händer, smalare silhuetter och dämpade material. Mer uttrycksfulla övergångar och målad ytbehandling återstår.
- `audio/voice-*.ogg`: sex nya svenska repliker. Två helt syntetiska, nya rollreferenser med VoxCPM2; repliker med Dots MF. Begäranden, råfiler och jobbmanifest i `source/voices/`. Inga verkliga personers röstprov används.
- `audio/score.ogg`, `boss-score.ogg`: två lokala ACE-Step 1.5 Turbo-generationer. Råmaster, begäran, resultat och looprecept i `source/`. Hela takter överlappar loopskarven; originalmastrarna bevaras.
- Övrigt ljud: egen deterministisk ljudsyntes och lagerdesign i `tools/build_sfx.py`, råmaster i `source/`.
- `fonts/NotoSerif-Regular.ttf`, `NotoSans-Regular.ttf`: installerade Noto-fonter. SIL Open Font License finns i `fonts/LICENSE`.

Godot, .NET, Blender, ComfyUI och respektive modell är externa verktyg med egna licenser; träningsmodeller distribueras inte med spelet. Godots motorlicens: https://godotengine.org/license/

Bildproduktionen använde lokala ComfyUI enligt projektets val, inte OpenAI:s inbyggda bildverktyg. AI-tjänster behövs enbart vid produktion, aldrig vid spelstart.
# Fristående figurstudie

`assets/source/concepts/karl-oil-study-v3.png` genererades lokalt via ComfyUI/Krea med `tools/paint_karl_study.py`; exakt graf och manifest finns bredvid bilden. Detta är en fristående oljemålningsstudie, inte en runtime-sprite. Efter granskning är den inte vald som identitetsreferens: nästa figurpass ska utgå från originalspelets dungeon-Karl och användarens Diablo 2-riktning.
