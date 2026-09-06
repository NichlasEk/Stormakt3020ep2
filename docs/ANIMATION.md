# Animation · rörelsen före målningen

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

Granska och förfina gångens tyngd i provfiguren. Bygg därefter en sammanhängande Karl-modell med viktade leder och den godkända vuxna anatomin, kläderna och materialbehandlingen. Måla modellens sammanhängande texturer; rendera sedan alla riktningar från samma animerade modell. Kontrollera silhuett, fotkontakt, vapen och djup vid faktisk kameraskala innan nya atlaser ersätter spelets gång. Återanvänd den kontrollerade rörelsen med anpassad hållning för hammare och fiender.
