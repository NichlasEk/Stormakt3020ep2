# Atlands port · modulärt 2D-prov · 0.4.1

## Prova

`./start.sh -- --port` eller **Spela Atlands port** i huvudmenyn.

Detta använder `port-save.json` och `port-manual-save.json`, skilda från huvudexpeditionens och landstigningens sparfiler. Kapitelprovet kan fortsätta genom resterande sju banor. Den ordinarie expeditionen använder också den nya portbanan när den når bana 1.

Följ den ljusare stenläggningen till de tre minnena. Den låga muren på vänster sida kan rundas; bakom den ligger murarens gömma. Håll E/B vid de tre minnena, E/B hämtar gömman, R öppnar ledtråden. Porten svarar på Land → Vatten → Minne. Gömman ger två tinkturer, 20 liv (max 100) och ett avtryck av samma ledtråd. Fyndet kan bara tas en gång och sparas direkt.

## Produktionsgrund

- `scripts/PortLayout.cs`: gemensam 2D-projektion, 128 × 64 världspixlar per golvmodul. Grund, skiljemur, kollisionsmarginal och sidofynd definieras i samma koordinater.
- `scripts/PortPresentation.cs`: återanvändbara golvflaggor i förband, stenmurssegment och hörn, portstolpar/galler samt trappnosar. Alla ytor är direkt ritade 2D-polygoner med en målad textur. Inga 3D-modeller, ljus eller Blender-renderingar.
- `assets/art/port-stone-v1.png`: ny lågmäld målad stenstruktur från inbyggd bildgenerering. Full prompt i `assets/source/campaign/port-modules-v1.provenance.json`.
- `assets/art/port-props-v1.png`: exakt kopia av det tidigare direkta 2D-arkets original med riktig alpha. Bara fyra småföremål från sista raden används: plint, lykta, kista och runsten. De avvisade golv- och murdelarna används inte. Källrektanglar och storlek finns i `PortProp`.

Karl är cirka 150 världspixlar hög. Skiljemuren är 74, lyktan 30, kistan 55 och runstenen 96. Figurerna är befintliga 2D-sprites; detta pass ändrar inte gången.

Golvet och sex typer av murdelar rasteriseras en gång vid start till två cachade 2D-texturer i SubViewport med Disable3D. Därefter ritas de som återanvändbara sprites. Detta undviker tusentals små ritkommandon per bildruta.

Muren delas upp i sex segment som sorteras tillsammans med figurernas fotpunkter. Golvet ritas först. Samma murfotavtryck stoppar rörelse, kulor, närstrid och interaktion genom sten. Navigeringen hittar runt ändarna. Äldre sparlägen i den nytillkomna muren flyttas till närmaste fria sida vid laddning.

## Verifierat

29 222 regelassertions, inklusive samtliga åtta banor med båda understöden och arkivvalen. Nya kontroller täcker sidovägens räckvidd via vanlig förflyttning, väggkollision, kulor, sabel, engångsbelöning, sparning/laddning och återhämtning av gamla koordinater.

`--port-check` fångar entré, Karl framför/bakom muren, gömman och stängd/öppen port. Bilderna i `artifacts/port-*.png` är granskade vid 1280 × 720. Testet är tyst och skriver inga användarsparningar.

Full native-kampanj har också gått till slutet: 96 liv, 38 besegrade och 19 parader.

## Kvar efter detta pass

Detta är en spelbar modulgrund, inte den färdiga konstnärliga nivån. Stenläggning och murar är fortfarande regelbundna; fler kantavslut, skadade segment, vegetation och omgivande arkitektur behövs för att nå den älskade strandens rikedom. Trappnosarna är visuell stenläggning i samma gångplan, inte ett system för höjdskillnader. Portens galler visar gåtans låsstatus; övergången sker med E vid tröskeln. Nya röster och Karls nya gång återstår enligt den överenskomna ordningen.
