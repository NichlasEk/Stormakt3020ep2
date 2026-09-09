# 0.18 · Ebba i egen hög person

Meridiansalens ”återvänd till Ebba” leder nu till ett faktiskt möte ombord på Karl CCLV. Kajutan är den 23:e platsen, med rik målad bakgrund, en fysisk 2D-Ebba efter det godkända porträttet, åtta nya direkta repliker och lågt motorbrum.

## Prova från din sparning

1. Stäng den äldre spelprocessen och starta `./play.sh` igen. Fortsätt samma sparning som tidigare.
2. Med ordern från Meridiansalen: gå tillbaka genom Klockgången till Instrumentgården.
3. Gå till skeppets landgång på vänstra sidan, tryck **E / B**: ”Gå ombord · möt Ebba i kajutan”. Om du redan flugit tillbaka nås samma möte från regementets brygga.
4. Gå fram till Ebba vid kartbordet. **E / B** visar ordern; tryck igen efter replikerna för föremålen och sedan nästa steg. Samtalsstegen sparas var för sig.
5. Vid kistan med förband på högra sidan kan Ebba återställa liv och uthållighet en gång efter genomgången.
6. Luckan leder ut till samma landgång. Instrumentet till vänster i kajutan startar den befintliga skeppsresan mellan Uppsala och bryggan.

Ingen tidigare sparning återställs. Layout 10 migreras till 11 med en obesökt kajuta; order, hälsa, utrustning och världens tillstånd bevaras. Äldre layouter går genom befintliga migreringar först. Den vanliga huvudrutten och separata Uppsala-/Meridianprov använder samma innehåll.

## Berättelsen

Ebba skiljer tydligt mellan Arvids döda regemente och Meridiansalens förflyttningsorder. Hon kvitterar deras vila, håller isär karta/form/plåt och förvarar ordern. Uppdraget avslutas när ni talat om nästa steg. Originalet i övre observatoriet är nästa spår; **observatoriet är fortfarande inte spelbart**.

[Storyboard, scen 16](STORYBOARD.md) innehåller placering och alla åtta exakta repliker. Karl kan röra sig och samtala; hugg och understöd är avstängda i den fredade kajutan. Ebba är en stillastående målad NPC, utan ny gång- eller läppanimation.

## Assets och ljud

- [Kajutans bakgrund och prompt](../assets/art/room-cabin-v1.prompt.md): `assets/art/room-cabin-v1.png`.
- [Ebbas kroppssprite och promptkedja](../assets/art/ebba-cabin-v1.prompt.md): `assets/art/ebba-cabin-v1.png`, med befintliga `ebba-radio-v3.png` som identitetsreferens.
- Båda genererades med inbyggda imagegen. Ebbas slutliga gröna bakgrund tas bort av befintliga `SpriteCutout`, utökat med grön nyckelfärg och kantavfärgning; källbilden bevaras. Ingen Blender-modell.
- `assets/story/cabin-radio.json` → `assets/audio/voice-cabin-*.ogg`. Lokal Dots-TTS med den befintliga syntetiska Ebba-referensen, mening för mening, 16 steg. Direkta röster utan radiolågpass, normaliserade till −18 LUFS.
- `tools/build_cabin_audio.py` bygger eget periodiskt motorbrum utan externa samplingar. Musik och utomhusljud tonas ned ombord, mastervolymen gäller fortsatt.
- Rösternas källor, förfrågningar och hashmanifest finns under `assets/source/voices/cabin-*`. CPU-Whisper användes som textkontroll; två formuleringar togs om. Detta ersätter inte mänsklig lyssning.

## Verifiering

- .NET-bygge: inga fel eller varningar.
- Full ren simuleringssvit: **78 191 kontroller**, inklusive äldre sparningar och hela Meridian-/Uppsalarutten.
- Kajutans riktade prov kontrollerar vanlig landgångsinteraktion, rörelse mellan alla interaktionspunkter, tre sparade samtalssteg, engångsläkning, återbesök, fredad strid, rätt kaj och fortsatt flygning.
- Native `--cabin-check`: scener, åtta laddade röstklipp, målad Ebba med transparent bakgrund, kartbord och lucka.
- Slutlig Linux-export: `dist/AtlandsArv-0.18-cabin/`, med executable, PCK och hela .NET-datakatalogen. Native `--cabin-check` passerade även i denna färdiga export; alla åtta röster och motorbrummet laddades. Bilderna inspekterades efter exporten.

För ett isolerat automatiskt bildprov, utan ändring av spelarens sparning: `./play.sh -- --cabin-check`. Bilder skrivs till exportmappens `artifacts/cabin-*.png`. Detta prov spelar igenom mötet automatiskt och avslutar spelet.
