# Checkpoint · 2026-09-06

## Byggt

Godot 4.7.2 .NET-projektet är separat från originalet. Första spelprovet i Blekinges likvarv har titelmeny, krigsrådsval, en sammanhängande kajstrid, två förtöjningssigill, fyra vakter, varvets indrivare, bronskartan och avslutningskort. Sabel/hammare, tunga attacker, undanmanöver, gard, projektilparad, tinkturer och artilleri/fältvård fungerar. Tangentbord/mus och första handkontrollens knappar/axlar är kopplade. Paus, ljudvolym, kameraskakning och helskärm finns.

Manuell F5/F9 och automatiska kontrollpunkter är separata; filer är checksummade och skrivs atomiskt med föregående backup. Det äldre spelets sparfiler berörs inte.

Lokalt genererade nya tillgångar: hamnmålning, två syntetiska röstreferenser, sex svenska repliker, två orkesterspår. Egen geometri för sex figuruppsättningar, egna effektljud och hamnatmosfär. Källor och produktionsverktyg finns i repot. Ljuden för understödets radioreplik och själva kanonen har uttryckligen olika filnamn.

## Senaste styrning från användaren

### Genomfört matt bildpass

Användaren bad uttryckligen att också göra miljön ruffare och mindre glansig. `likvarvet-matte-v4.png` är nu spelprototypens bakgrund; gångpolygon, sigill och kartfynd behölls. Dekorativa vattenglimtar är borttagna, och den redan mörkt målade miljön ritas utan ytterligare färgdämpning. Den nya bilden har grov sten, sotigt trä, matt metall och små återhållna lyktsken.

`./start-art-study.sh --fullscreen` visar nytillverkade referensbaserade Karl- och soldatbilder på kajen i spelets kameraskala. Mellanslag växlar tre poser. Detta är den utlovade avgränsade bildgranskningen: den fulla spelprototypen använder ännu föregående animerade figurer. Nästa steg är att bedöma bilden tillsammans med användaren och därefter bygga en sammanhängande riktnings- och animationsuppsättning. Inget motorbyte genomfört.

Lokala Krea-pass kasserades som bildriktning eftersom de gav mer glans och ändrade detaljer. Valda bilder är skapade med inbyggd bildgenerering; prompter, referenser, avvisade studier och manifest finns i `assets/source/rough-pass/`. Bildprovet använder en tillfällig ljushetsmask för generatorns ljusrutiga RGB-bakgrund; riktiga alphakanter krävs vid slutproduktion. Vissa höga metallglanspunkter påverkas av denna mask.

Validering av bildpasset: C# bygger utan varningar/fel. Verklig Godot/OpenGL-körning med den matta kajen sparade gameplay-bild och avslutade utan motorfel. Figurprovet sparar tre poser med `./start-art-study.sh --fullscreen -- --capture-study` till `artifacts/rough-art-study*.png`. Stridsreglerna är oförändrade i detta pass.

Även andra figurpasset upplevdes för gulligt. Användarens senaste, överordnade referens är **Diablo 2 och originalspelets gamla assets**. Granskade originalbilder: `dungeon-karl-combat-v1.png`, `dungeon-danish-enemies-v1.png`, `dungeon-gruva1-environment-v1.png` under `/home/nichlas/WaylandForge/assets/stormakt3020/`. De visar målade material, vuxen anatomi, sammanhängande dräkter och läsbara stridsposer. Originalets Karl har ungt ansikte, pannlampa, blå karolinerrock, bröstharnesk och gula handskar: bevara identiteten när uttrycket görs mörkare och mer slitet.

Nuvarande Blender-figurer är tekniska provisorier, inte godkänd slutstil. Studien `assets/source/concepts/karl-oil-study-v3.png` visar oljebehandling men avviker i ålder, identitet och kameravinkel; den är inte en runtime-sprite och ska inte bli figurförlaga. Nästa avgränsade bildprov ska visa Karl och en fiende i spelets faktiska perspektiv och storlek, med originalets dungeon-bilder som referenser. Bedöm silhuett, tyg/metall, ansikte och attackläsbarhet i miljön innan fler figurer produceras. Motorfrågan är diskuterad, inget byte till WaylandForge beslutat.

Första bildpasset var fint men för gulligt och runt. Ny riktning: mörk sliten oljemålning från stormaktstiden. Miljön har målats om med bibehållen hamnidé, gångpolygon/sigill/kartfynd anpassats, figurer fått vuxnare proportioner och smutsigare material. Den här styrningen är viktigare än den första ljusa konceptbilden.

## Verifiering

- Slutlig .NET-build utan varningar/fel. Godot/OpenGL-smoke efter bildrevision och resursstädning sparade `artifacts/gameplay.png` och avslutade utan läckagevarningar eller motorfel.
- 16 012 assertions i tester: en träff per hugg, tung attack, projektilparad, undanmanöver, understöd/cooldown, 8 000 rörelsesteg inom kajen och med begränsad stamina, deterministisk fortsättning från en sparad aktiv strid, korrupt sparfil med backup.
- Testspelaren klarar hela mötet via vanliga kontroller, även efter ändrad gångpolygon: 5 besegrade fiender, 5 parader, 53 liv kvar, ungefär 53 logiska sekunder. Detta mäter körbarhet, inte mänsklig speltid eller upplevd stridskvalitet.
- Verklig Godot/OpenGL-körning gav gameplay-, boss- och slutbilder och avslutade integrerat möte utan motorfel före sista bildpasset. Slutliga bilder tas efter revisionen.
- Musik: 48 kHz stereo, 70 respektive 54 sekunders runtime-loopar, tysta slutpartier från råmastrarna utanför looparna. Bossloopens kontrollerade peak -4,4 dB efter kodning; ingen längre tystnad detekterad.
- Lokal Whisper-kontroll återgav replikernas huvudsakliga innehåll. Egennamnet Rudbeck och ordet sigill fick osäkra transkriberingar; mänsklig lyssning behövs innan rösterna betraktas som slutcasting.
- Handkontroll är kodad men fysisk handkontroll har inte provats under detta pass.

## Viktiga nästa steg

1. Användaren provspelar. Först bedöms rörelsekänsla, attacktiming, kamera, målens tydlighet och den nya mörka stilen.
2. Förbättra figurernas målade detalj och animation; nu finns sju tydliga poser per riktning men inga fullständiga mellanbilder. Spelets strid är oberoende av antalet renderade poser.
3. Bygg den längre 20–30-minuters Blekinge-missionen: fler rum, utforskning, loot och ett riktigt första möte med Varvsänkan. Det nuvarande korta stridsprovet ska inte beskrivas som den färdiga missionen.
4. Fördjupa strategins bestående konsekvenser och föremål som ändrar spelstil. Fullständigt inventory och ediktsystem är ännu inte byggda.
5. Filmsekvenser med LTX-2 först när scenkomposition och bildidentitet håller. Ingen film har genererats eller kopplats in ännu.

## Körning

`./start.sh` importerar resurser, bygger C# och startar spelet. `dotnet run --project tests/Atland.Tests.csproj` kör reglernas tester. `./start.sh --fullscreen -- --integration` kör Godot-testspelaren. `--smoke` och `--capture-title` ger korta bildprov. Alla testbilder och loggar ligger i `artifacts/` och skrivs inte till användarsparfiler.

Origin: https://github.com/NichlasEk/Stormakt3020ep2 . Commit/push efter fungerande milstolpar är uttryckligen önskat. Stor grafik, råljud och runtime-ljud går via Git LFS.
