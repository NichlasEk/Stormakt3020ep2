# 0.16.1 · Avlösningen och de tre fynden

Arvid säger nu uttryckligen att han och soldaterna är döda. Efter tre sekunders paus förklarar Hedvig att de ber om vila. Den befintliga orderkvittensen från Ebba följer när Karl tar ordern.

Vid mönstringsstenen ger Arvid sin sista order. Hans tack och överlämning av båten följs av **Avlösningen**, en ny 8,04 sekunder lång LTX-film av den tomma mönstringsplatsen. Efteråt kommer Ebba och Hedvig tillbaka på radion. Soldaterna tonar bort, kaptenens plats förblir tom och musiken tonar bort i regementets rum. Filmen visar platsens efterspel, inte en ny marschanimation.

Hedvig skiljer nu mellan:

- **Bronskartan:** expeditionens vägvisare.
- **Gjutformen:** stannar i svalgången; Karl gör en avritning.
- **Stjärnplåten:** tas vid pressen efter Kronfogden och bärs vidare till båten och Uppsala.

Nio nya svenska röstklipp använder de etablerade syntetiska rösterna och porträttkorten för Arvid, Ebba och Hedvig. Inga nya rollfigurer behövs för denna scen. `continuity-radio.json` är manusets källa. `foundry-return` behålls som äldre källasset men utlöses inte längre; den tydligare plåtförklaringen och Ebbas hemorder ersätter den.

## Prova

Starta `./play.sh`. Filmen finns under **Esc → Inställningar → Videobibliotek → Avlösningen**. Esc eller handkontrollens B hoppar över; biblioteksvisning ändrar inte sparningen.

Hela scenen finns i ordinarie rutt efter Rotmarskalken: läs ordern vid mönstringsstenen. Separat regementesprov: `./play.sh -- --regiment`. Kaptenen finns i sjukbaracken.

Äldre sparningar får förtydligandena när respektive plats används igen. Kaptenens nya samtal kräver att regementet ännu inte avlösts. Redan avlösta sparningar kan spela avskedet vid mönstringsstenen, utan ny belöning; en laddning på själva mönstringsvallen startar det automatiskt. Filmen markeras sedd när uppspelningen börjar, så även överhoppning eller avslut mitt i filmen räknas. Den finns alltid i biblioteket.

## Teknik och verifiering

Additiva booleska sparfält; ingen ändring av rummens layoutversion. Världssimuleringen står stilla under avskedssamtalet och filmen. Radioefterspelet fortsätter även efter överhoppning. Musikdämpningen lämnar miljöljud och röster kvar.

Källfilm, nyckelbild, ComfyUI-arbetsflöde och jobbproveniens ligger i `assets/source/cinematics/regiment-rest-*`. Verktyg: `tools/render_regiment_rest.py` och `tools/assemble_regiment_rest.py`. Lokal GPU-kö används för LTX; slutformatet är Theora/Vorbis och följer huvudvolymen.

Verifiering: 70 712 godkända simuleringskontroller inklusive 14 särskilda berättelsekontroller, samt godkänd native `--narrative-check` för röstresurser, radiokort, filmavkodning, naturligt slut, överhoppning, bibliotekets oförändrade speltillstånd och efterspel i både projektet och den färdiga Linux-exporten. Rösternas svenska innehåll har kontrollerats med lokal CPU-transkription; det ersätter inte användarens lyssningsbedömning av uttal och skådespel.

Export: `dist/AtlandsArv-0.16.1-story/`. Program, PCK och hela .NET-katalogen hålls tillsammans.

Nästa berättelsesteg är fortfarande Meridiansalen. Uppsalas upprepade morgon behöver gestaltas innan en ny stor förklaring, och Karl/personen kontra skeppets namn återstår att stämma av. Se [storyboarden](STORYBOARD.md).
