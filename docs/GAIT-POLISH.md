# 0.15.1 · Gångputs

Karl och de vanliga soldaternas fyra målade gångbilder löper nu över 88 världsenheter i stället för cirka 111. Bildhållningen blir kortare, särskilt för långsamma fiender. Karls stegljud följer den nya takten. Gången fortsätter att följa verkligt avstånd och stannar när kroppen stannar.

Riktningsvalet behåller föregående sida inom ett smalt område runt lodrät/vågrät gång. Små rörelseavvikelser byter därför inte fram-/bakbild eller spegelvändning varje bildruta. Tydliga riktningsbyten slår igenom direkt. Detta gäller även regementets figurer och Kronfogden. Bossarnas gångriktning använder förflyttningen längs gångvägen.

Kronfogden har fått ett separat [målat 2D-gångkort](../assets/art/crown-bailiff-walk-v1.png), fyra faser i fram- och bakvy, speglat åt väster. Bältets position förankras per bild för att minska sidoryck. [Prompter och korrigeringar](../assets/art/crown-bailiff-walk-v1.prompt.md), framställda med Codex inbyggda imagegen. Ingen 3D eller benförvrängning används. Hans stridsposer är kvar.

Detta är ett putsningspass, inte färdiga högupplösta animationscykler. De målade gångkorten har fortfarande få bilder; Kronfogdens passerande ben och övergång till stridsställning behöver fortsatt bedömning i spel. Karl och de äldre soldaterna har befintliga teckningar med ny uppspelning.

## Prova och verifiera

`./play.sh -- --foundry-new` börjar ett nytt separat gjuteriprov. Lossa porten, gå in och flytta dig åt olika håll så att Kronfogden följer efter. Vanliga sparningar fungerar också.

`--gait-check` fångar 96 bilder av Karl och Kronfogden i fyra gångriktningar i den riktiga spelrenderingen. Lokal jämförelsefilm: `artifacts/gait-review.mp4`. Bilder ur fram- och bakvy granskas i spelets skala. Automatiska kontroller täcker riktningsstabilitet vid små avvikelser, avsiktligt riktningsbyte och cykelavstånd. Verifierat: hela sviten passerar 70 582 kontroller; bygget har noll varningar/fel. Native-gångprovet ger 96 bilder och Linux-exportens bossprov avslutas med kod 0.

Linux-checkpoint: `dist/AtlandsArv-0.15.1-gait/`, med full .NET-katalog.

## Uppsala

Transporten är ännu inte byggd. Efter användarens fråga om rymdskeppet föreslås båten tillbaka från berget och därefter Karl CCLV till Uppsala. Avfärden spelas högtidligt; Ebba sköter radiotrafiken som ett vanligt expeditionsärende. Se [nästa expedition](NEXT-EXPEDITION.md).
