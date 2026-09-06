# Checkpoint · Vägen under vattnet · 2026-09-06

## Senaste justering · överlevnad och provisorisk specialistgång

Användaren tycker indrivaren är svår och vill kunna fortsätta spela trots skador. Inställningar har nu **Utvecklarläge: överlevnad**. Standard AV. PÅ behåller skada, skadeblink, ljud, gard och ordinarie träffimmunitet, men livet stannar vid 1. Synlig DEV-markering. Preferensen sparas i settings.cfg och tillämpas på aktuell kampanj, ny kampanj, duell och laddning. Den lagras inte i kampanjens JSON; nuvarande inställning styr. Automatiska stridstester kör utan skydd.

Gruvhammar-Karl och indrivaren har två nya provisoriska fyrariktningsark för gång. Hammar-Karls bakåtvända rader kom i omvänd ordning och mappas rätt i `AnimatedCast.cs`. Övriga stridsposer är kvar. Användaren har uttryckligen pekat ut att **hela gången behöver ett nytag**. Nästa animationsarbete ska därför utgå från en sammanhängande riggad gångcykel med fotkontakt, gemensamt rotläge och kroppsbalans före ommålning, inte fler fristående poser. Det omtaget är ännu inte genomfört.

28 857 regelassertions passerar, inklusive skydd mot dödlig kula/områdesskada, bibehållen skadefeedback och återställd dödlighet vid avstängning. UI-kontroll provar omkopplare, settings-reload samt ny kampanj/duell. Specialistark och inställningslayout har fångats i bildprov. Native-versionen `dist/AtlandsArv-0.3.1/AtlandsArv.x86_64` är exporterad separat så den äldre spelprocessen kan fortsätta vara öppen. Samma UI-kontroller passerar även i den exporterade versionen.

## Spelbart nu

Spelprov 0.3 i Godot 4.7.2 .NET. Separat från originalet. Hela kajuppdraget fortsätter nu genom kronans magasin till De tre vittnenas strand och det första spelbara Atland-avslöjandet.

- Kaj: två sigill, fyra vakter, indrivaren, karta, tre inskrifter med patruller, öppen sändning eller krypterad rapport och förföljare.
- Magasin: tre vakter, förrådskista med brynstål/tinktur, mätorder, vinsch och två nya vakter. Lasten blockerar närstrid och kulor; fiender går runt den.
- Strand: två vakter, tre ostörda mätningar, tre förföljare och kartinpassning vid mittstenen. Ny målad bakgrund och en kort kamerapanorering visar vägen och porten i gryningen. Kollegiets färska mätband och order ger berättelsens vändning.
- Sabelduell från huvudmenyn: en vakt, fristående från kampanjens sparfiler.
- Brynstålet gör nästa sabelhugg 60 % starkare inom 2,6 sekunder efter en perfekt parad. Öppen sändning behåller sin paradläkning; krypterad rapport ger färre förföljare och förråd.
- Fältdagboken har en fjärde sida för mätorder, uppgradering och expeditionens upptäckt.

Kontrollpunkter vid områdesbyten, fynd och mätningar. Delvis avläst inskrift och delvis mätt riktning överlever manuell sparning/laddning. Checksummade atomiska sparfiler med föregående backup. Äldre enumvärden är bevarade. Ett avslutat 0.2-läge med tre lästa namn kan fortsätta genom magasinet från slutmenyn; 0.1-lägen förblir avslutade. Normal ny kampanj använder den långa rutten.

## Grafik, rörelse och användarens senaste återkoppling

Allvarliga, vuxna figurer i mörk sliten oljemålningsstil. Absurd humor ligger i berättelsen och radion. Ebbas godkända `ebba-radio-v3.png` är kvar.

Karl/sabel och sabelvakten använder sex nya atlaser med fyra riktningar, gång- och attacknyckelbilder samt gard, träffreaktion, undanmanöver och liggande dödspose. Specialistroller och hammare använder fortsatt tidigare tre poser. `AnimatedCast.cs` kompletterar `PaintedCast.cs`; båda utgår från 150 världspixlars ståhöjd. Magenta friläggs och kantfärg rensas före mipmaps.

Användaren såg ryckig gång. Gångförankringen följer nu bäckenets mitt, rytmen beror på faktiskt tillryggalagd sträcka, rörelsebilderna följer gångriktningen och renderpositioner interpoleras mellan fysiksteg. De målade stegnycklarna är fortfarande ojämna och vissa för lika. Ett åttabildsförsök avvisades efter granskning. Presentera inte detta som färdig mjuk åttariktad animation. Nästa förbättring bör utgå från en riggad rörelsereferens.

Användaren påpekade också att murar måste ritas framför figurer bakom dem. `SceneryDepth.cs` innehåller nu granskade förgrundskonturer för kajens och magasinets främre murar samt strandens främsta sten. Last, förrådskista, vinsch och lyktor sorteras med figurerna efter markdjup. Bildprov finns i `artifacts/depth-*.png`.

Nya miljöer utgår från två måttsatta Blender-blockouter. Bildverktyget flyttade flera props; gångyta och spelpunkter har därefter kalibrerats mot de faktiska målningarna. Projektion, korrigeringar, fulla promptar och ursprung ligger i `assets/source/journey/`. Se `docs/JOURNEY-PRODUCTION.md` för detaljer och reproduktion.

## Radio och ljud

30 svenska repliker totalt, varav 14 nya i 0.3. Ebba och Hedvig använder samma egna syntetiska röstidentiteter och godkända porträtt. `assets/story/journey-radio.json` är gemensam textkälla för spelet och produktionen. Lokal VoxCPM2 för rollreferenser, Dots MF för repliker; inga verkliga personers röstprov.

Lokal CPU-Whisper granskade nya repliker. Den första mätordern tappade slutet och ersattes med kortare text/tagning; även brynstålsrepliken kortades. Egennamn och enskilda uttal förblir osäkra i automatisk transkribering. Råljud, begäranden och jobbmanifest sparas; avvisade tagningar ligger under `assets/source/voices/rejected/`.

Tre befintliga ACE-Step-spår används med övergångar och dämpning under tal. Nytt egenproducerat fotstegsljud. Ingen AI-tjänst behövs vid spel. Standardvolym 25 %, dragreglage, M tyst/återställ, +/− femprocentsteg och sparad ljudnivå. Automatiska tester är tysta och skriver inga användarsparningar.

## Verifiering

- 28 852 regelassertions: fyra kompletta långa genomspelningar, båda understöden × båda berättelsevalen. 23–24 besegrade, 70–100 liv kvar, cirka 124–152 logiska sekunder. Detta är botkörningar, inte mänsklig speltid.
- Kort originalrutt, gränser, ammunition/parad, sparåterhämtning, fortsatt 0.2-läge, delvis sparad mätning, sikthinder, projektilstopp och brynstålets engångsripost provas.
- Godot/OpenGL-integration når slutet via vanliga kontroller och fångar kaj, boss, namn, val, magasin, strand, avslöjande och slut.
- UI-testet klarar mute/återställning, tangent, musdrag/clamp, ingen attack vid reglageklick, sparad ljudnivå, dagbok och tre porträtt/röstpar.
- `--scene-check` visar överlappning bakom last, vid vinsch/mur, bakom sten och vid kajens kant. Bilderna är visuellt granskade.
- Linux-exporten i `dist/AtlandsArv/` är ombyggd; JSON-manuset ingår uttryckligen i paketet. Native bildprov laddar nya assets och avslutar med kod 0. Ett första exportprov gav en resursvarning vid avslut; omprovet med verbose-logg avslutades utan varningen.
- Fysisk handkontroll har inte provats. Full inventariehantering, större kampanj, Varvsänkan, Uppsala och filmsekvenser återstår.

## Körning och nästa steg

`./start.sh` startar spelet; **Öva sabelduell** går direkt till stridsträningen. Färdig 0.2-sparning kan fortsätta från slutmenyn. `dotnet run --project tests/Atland.Tests.csproj` kör regeltest. `./start.sh --fullscreen -- --integration`, `--ui-check` eller `--scene-check` ger tysta prov. `--duel` startar övningen direkt. Bildstudien visar nu samma nya atlaser: mellanslag växlar rörelse/pose, vänster/höger riktning, G måttreferens.

Låt användaren bedöma gång, murarnas överlappning och den längre banan. Förfina stridskänslan och animationerna från verkliga spelintryck före nästa stora kampanjutbyggnad. Bevara godkänd seriös anatomi och Ebba.

Rudbeck: `docs/ATLANTICA-NOTES.md` innehåller primärläsning med tryckta sidnummer. Mätordern, mätbanden och denna expedition är originalskriven fiktion. Användarens två PDF-filer i roten är lokalt researchmaterial och ligger utanför Git.

Origin: https://github.com/NichlasEk/Stormakt3020ep2 . Commit/push efter fungerande milstolpar är önskat. Assets använder Git LFS. Originalet i /home/nichlas/WaylandForge berörs inte.
