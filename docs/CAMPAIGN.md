# Expeditionen genom Atland · 0.4

## Gällande assetriktning · användarens senaste instruktion

Äkta 2D-isometriska spelassets med fast projektion, rena silhuetter, läsbara former, återhållen palett, diskret målad textur och återanvändbara moduler. Ingen fortsatt Blender-/3D-produktion. Det tidigare renderade gångprovet är arkiverat experiment och ska inte ersätta kampanjens figurer. Nedanstående Blender-anvisningar beskriver historiken, inte nästa produktionssteg.

Ett första direktgenererat 2D-kit med tolv delar finns i `assets/source/campaign/atland-modular-2d-candidate-v1.png`. Originalet har riktig alpha; golvprojektion och modulskarvar är ännu inte godkända. Tre korrigeringsförsök avvisades på grund av opaka bakgrunder respektive klippta golv. Kitet används inte i kampanjen. Promptar och kvalitetsstatus finns i intilliggande provenance-JSON.


Åtta sammanhängande nya banor efter kajen, magasinet och stranden. Fyra målade miljögrunder används i två banor vardera. Detta är ett spelbart breddpass, inte slutlig miljödetaljering.

| Värld | Bana | Spelidé |
| --- | --- | --- |
| Atland | Atlands port | Läs ledtråden och aktivera Land, Vatten, Minne i rätt ordning. |
| Atland | Minnets arkiv | Samla tre fynd; bevara arkivet eller förfalska passet. |
| Rotmarkerna | Rotvägen | Tänd tre väglyktor under anfall. |
| Rotmarkerna | Regementet som stannar | Bryt tre mönstringsvågor och ring avlösningen. |
| Bergslagens underjord | Järnets lungor | Stäng ventiler i ordning, undvik varnade ångutbrott. |
| Bergslagens underjord | Den tomma kronan | Besegra Kronfogden och ta kronans avtryck. |
| Uppsala | Uppsalas felvända himmel | Tolka stjärnmekanismen: Nordstjärnan, Månen, Solen. |
| Uppsala | Nornornas protokoll | Bryt kollegiets sista mönstring och lämna vittnesmålet. |

Arkivvalet består genom sparning och områdesbyten. Bevarat arkiv ger förråd men fler förföljare; förfalskat pass minskar senare motstånd. Sluttexten speglar valet. Båda understöden fungerar, och alla banor går att klara utan utvecklarskydd. E/B undersöker; R öppnar ledtråden i fältdagboken. Checkpoints sparas vid fynd, vågor, val och områdesbyten.

## Start och sparning

`./start.sh` → **Spela nästa del · åtta banor**. Direktstart: `./start.sh -- --atland`.

Kapitelstarten använder `atland-save.json`, skild från landstigningens `quay-save.json`. Befintlig kapitelprogress återupptas. Ny landstigning fortsätter hela vägen efter strandens avslöjande. En avslutad 0.3-fältdagbok kan fortsätta med knappen **Fortsätt in i Atland**. Äldre enumvärden har bevarats.

## Verifiering

- `dotnet run --project tests/Atland.Tests.csproj`: 29 139 assertions, inklusive åtta banor × båda arkivvalen × båda understöden, sparning/laddning vid varje bana, sekvenslås och 0.3-fortsättning.
- Full Godot-körning `--campaign-check`: åtta banor, 38 besegrade, 19 parader, 96 liv vid avslut. Vanliga stridsregler, utvecklarskydd av, inga användarsparningar.
- Bildfångster: `artifacts/campaign-01.png` till `campaign-08.png` och `ending.png`.

Automatiserad körning utan Wayland-fönstrets bakgrundsstrypning:

```sh
xvfb-run -a -s '-screen 0 1280x720x24' godot-mono --display-driver x11 --path "$PWD" --resolution 1280x720 --disable-vsync --fixed-fps 60 -- --campaign-check
```

## Eget detaljpass senare

Bana 1 har nu modulärt golv, en kolliderande skiljemur och en sidoväg med sparat fynd; se `PORT-MODULES.md`. Banorna 2–8 har ännu gemensam öppen spelgolvsgeometri. Nästa pass bör ge varje bana egna landmärken, avgränsningar och interaktionsföremål: arkivsigill, rotlyktor, mönstringsklocka, ventiler, gjutform och stjärninstrument. Kontrollera alltid vuxenskala, passage och skymning framför/bakom figurerna.

Kronfogden och kollegiets väktare återanvänder indrivarens grafik; nya vakter använder befintliga stridsroller. Nya berättelser visas som anteckningar. Egna porträtt, röster, musikvariationer och slutlig riggad gång hör till det senare assetpasset. Det målade gångprovet körs separat med `./start-walk-study.sh`.
