# Animation · rörelsen före målningen

## Gällande assetriktning · användarens senaste instruktion

Äkta 2D-isometriska spelassets med fast projektion, rena silhuetter, läsbara former, återhållen palett, diskret målad textur och återanvändbara moduler. Ingen fortsatt Blender-/3D-produktion. Det tidigare renderade gångprovet är arkiverat experiment och ska inte ersätta kampanjens figurer. Nedanstående Blender-anvisningar beskriver historiken, inte nästa produktionssteg.

Ett första direktgenererat 2D-kit med tolv delar finns i `assets/source/campaign/atland-modular-2d-candidate-v1.png`. Originalet har riktig alpha; golvprojektion och modulskarvar är ännu inte godkända. Tre korrigeringsförsök avvisades på grund av opaka bakgrunder respektive klippta golv. Kitet används inte i kampanjen. Promptar och kvalitetsstatus finns i intilliggande provenance-JSON.


## Beslut från speltestet

Användaren avvisade först gummiben i en deformerad 2D-målning och sedan brutna leder i samma bild uppdelad i stela delar. Båda försöken är borttagna från spelkoden. De lokala försöken ligger endast under ignorerade `artifacts/`. Den tidigare spelbara atlasgången gäller fortfarande.

Fortsatt produktion ska börja med en riktig tredimensionell rörelse och sedan få Karls godkända målade utseende. En sammanhängande modell ger dolda ledytor, gemensam anatomi och konsekventa vyer. Vi ska inte försöka lösa anatomin genom att tänja färdiga atlasrutor eller generera varje mellanbild oberoende.

## Första rörelseunderlaget

`assets/source/animation/karl-walk-reference.blend` innehåller en animerad armatur, en enkel provfigur och ett rutnät med 25 cm mellan linjerna. Provfiguren är uttryckligen ett rörelseunderlag, inte en godkänd ny Karl-modell. Ansikte, produktionskläder, texturer och rockanimation återstår.

En cykel har 30 bilder, en meters förflyttning och 62 procent stödfas per fot. Benen löses i 3D med fasta lår- och underbenslängder. Höften förskjuts, kroppen rör sig återhållsamt och armarna pendlar mot steget. Vapnet sitter på handens ben. Renderingen följer kroppen medan markrutnätet rör sig förbi, så glidning kan granskas.

Generatorn verifierar både beräknade segment och Blenders faktiskt poserade leder. Senaste kontroll: största segmentlängdsfel under 0,000001 m, största ledpositionsfel under 0,00001 m och numeriskt försumbar glidning under den beräknade stödfasen. Detta verifierar geometrin; det ersätter inte granskning av rörelsekänslan. Fotrullning från häl till tå, viktöverföring vid start/stopp och svängar behöver vidare bearbetning.

```sh
blender -b -t 4 --python tools/build_walk_reference.py -- --render
```

Fram- och bakvy hamnar i `artifacts/walk-reference/se/` och `ne/`. Ett sex sekunders bildprov finns lokalt som `artifacts/karl-walk-reference.mp4`. Källkod och `.blend` versionshanteras; filmen kan återskapas från bildsekvenserna.

## Nästa produktionssteg

### Första målade 2D-provet

Användaren godkände provfigurens gång och gav klartecken att gå vidare med 2D-produktionen. `karl-costume-v1.blend` bygger rock, stövlar, handskar, pannlampa och värja på samma rörelse. Riggens vilorotation har korrigerats till lösarens rotationskonvention; kontroll av enbart ledändpunkter missade tidigare att rockens fram- och baksida kunde vändas runt ryggraden.

Bildverktyget har målat ett gemensamt projektionsunderlag från modellens fyra registrerade vyer och den godkända `serious-cast-v6.png`. Full prompt och ursprung finns i `assets/source/animation/karl-paint-v1.provenance.json`. Den målningen läggs på 3D-ytan en gång. Alla gångbilder renderas därefter från samma modell. Det är inte 120 oberoende bildgenereringar. Grå bakgrund från projektionsmarginalerna maskas bort till respektive materials grundfärg.

`BakedKarl.cs` visar vanliga RGBA-atlaser: fyra riktningar, 30 bilder per riktning, 384 × 384 pixlar per ruta, sex kolumner. Ingen 3D-rendering eller AI används i spelet. Markankaret kommer från kamerans projektion av modellens rot, inte lägsta stövelpixel i varje bild. Skalan följer 150 världspixlars ståhöjd. Ett varv motsvarar cirka 84,834 världspixlar i den här projektionen.

Kör `./start-walk-study.sh` för 2D-provet på den godkända stranden. Vänster/höger byter riktning, mellanslag pausar, F11 växlar helskärm och Esc avslutar. Provet är separat från kampanjen. Modellens ansiktslikhet, axel-/kragform och projektionsskarvar behöver fortfarande konstnärlig granskning; detta är inte slutligt godkänd Karl-grafik och har därför inte ersatt kampanjens figurer.

Reproduktion efter ändringar i modellen:

```sh
blender -b -t 4 --python tools/build_walk_reference.py
blender -b -t 4 --python tools/build_karl_sprite_model.py
blender -b -t 4 --python tools/prepare_karl_paint.py
# En ändrad silhuett kräver ett nytt registrerat målningsunderlag via bildverktyget.
blender -b -t 4 --python tools/project_karl_paint.py -- --sprites
python tools/pack_karl_walk.py
./start-walk-study.sh -- --capture-walk
```

Paketeringen verifierar alla 120 rutors alpha, dimensioner och fria marginaler, inklusive värjspetsen. `karl-baked-v1.json` innehåller riktningarnas projicerade rörelse, ankare, skala och atlasernas SHA-256. Godot-provet fångar två gångcykler per riktning till `artifacts/shore-walk/` för granskning i kameraskala.

Granska och förfina gångens tyngd i provfiguren. Bygg därefter en sammanhängande Karl-modell med viktade leder och den godkända vuxna anatomin, kläderna och materialbehandlingen. Måla modellens sammanhängande texturer; rendera sedan alla riktningar från samma animerade modell. Kontrollera silhuett, fotkontakt, vapen och djup vid faktisk kameraskala innan nya atlaser ersätter spelets gång. Återanvänd den kontrollerade rörelsen med anpassad hållning för hammare och fiender.

### Ansiktskorrigering och separat detaljpass

Efter användarens bildgranskning renderas kroppen utan det projicerade modellhuvudet. Det tidigare godkända huvudet från `karl-attack-v1.png` följer i stället armaturens projicerade huvudcentrum, med fasta proportioner. Alla 120 bildrutor har packats om; Godot-provet fångar 240 gångbilder. Kostymens övergångar och slutlig integration i kampanjen återstår till det separata detaljpasset. Användaren prioriterar nu banor och världar.
