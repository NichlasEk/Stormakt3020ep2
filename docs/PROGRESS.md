# Checkpoint · Vägen under vattnet · 2026-09-06

## Senast · målat inventarium 0.5.1 · 2026-09-07

Ny genererad trä-/läderbakgrund och 14 individuella målade föremålsbilder i utrustning, väska, stash och större detaljförhandsvisning. Fulla promptar och ursprung: `assets/source/inventory/art-v1.json`. C växlar stats på/av och återgår till paus om fliken öppnades därifrån.

Build utan varningar/fel; Godot- och native `--inventory-check` passerar inklusive laddning av hela bildkatalogen, C-växling från spel/paus och befintliga utrustnings-/stashflöden. Bilder granskade i 1280 × 720. Export: `dist/AtlandsArv-0.5.1/AtlandsArv.x86_64`. Native avslutar med kod 0 men rapporterar kvarhållna Ogg-musikresurser vid avslut; verbose-loggen identifierar fyra musikströmmar, inga inventariebilder. Detta är kvarstående ljudstädning, inte ett underkänt menyprov.

## Senast · inventarium, utrustning och stash 0.5 · 2026-09-07

I öppnar inventarium; C stats; pausmenyn ger handkontrollväg. Fem utrustningsplatser (två vapenslag, rustning, hjälm, sigill), väska 24 och stash 60. Bonusar påverkar verklig vapenskada, skadeskydd och uthållighetsåterhämtning. 14 föremål i katalogen, loot från fiender/bossar och portens gömma. Fulla behållare tappar inga föremål. Förrådet låses under aktiv fara. Sparade identiteter, utrustning och stash följer respektive kampanj, och äldre fältdagböcker migrerar till grundset.

29 165 regelassertions passerar inklusive de fyra fulla kampanjrutterna. Godot- och native-menykontroller för flikar, utrustning, stats, stash, sidor och lås passerar; bilder är granskade i 1280 × 720. Linux-export `dist/AtlandsArv-0.5/AtlandsArv.x86_64`. Se `INVENTORY.md` för detaljer. Inga nya 3D-figurer eller miljöbyten: målade 2D-aktörer och rika bakgrunder består. Utrustningen ändrar ännu inte figurens kläder visuellt.


## Senast · återställd målad port · 2026-09-07

Användaren förtydligade att oron för 3D gäller Karl och fienderna; de rika genererade miljöerna föredras framför modulprovet. Porten använder åter `world-atland-v1.png`, oförändrad på disk. Modulgolv, murar, cache-renderer och den avvisade skiljemurens kollisionsyta är borttagna. Den ursprungliga gångytan används igen. Gömman och dess engångsbelöning är kvar, och gamla sparpositioner anpassas till återställd gångyta.

`./start.sh -- --port`, samma separata sparfil. Native: `dist/AtlandsArv-0.4.2/AtlandsArv.x86_64`. Verifierat: 29 144 regelassertions inklusive båda arkivvalen och understöden genom alla åtta banor; native `--port-check` passerar och bilderna i `dist/AtlandsArv-0.4.2/artifacts/` är visuellt granskade. `VISION.md` förtydligar den fortsatta riktningen: rika målade bakgrunder och seriösa 2D-figurer. Tidigare modulbeskrivningar nedan är historik.


## Senaste checkpoint · modulär Atlands port 0.4.1

Portbanan använder nu direkt ritade 2D-moduler på ett fast 128:64-rutnät och en ny målad stenstruktur. Skiljemuren har kollision, stoppar kulor och hugg och skymmer figurer utifrån fotpunkter. Bakom den finns en sidoväg med en sparad engångsbelöning och ledtråd. Fyra småföremål återanvänds från det tidigare transparenta 2D-arket; inga av dess avvisade golv- eller murbilder används. Ingen Blender-produktion har gjorts.

Start: `./start.sh -- --port` eller **Spela Atlands port**, separat `port-save.json`. Ny native-export: `dist/AtlandsArv-0.4.1/AtlandsArv.x86_64`. Regeltester: 29 222 assertions. Full native-kampanj till avslut med 96 liv, 38 besegrade och 19 parader. Golv och sex murtyper cachas efter första 2D-ritningen; inget 3D-innehåll används. Visuellt granskad skymning, entré, gömma och portstatus. Se `PORT-MODULES.md` för exakta mått, verifiering och begränsningar. Konstnärlig rikedom, brutna kanter, omgivning, nya röster och gång återstår; detta är en spelbar grund, inte färdig slutgrafik.


## Gällande assetriktning · användarens senaste instruktion

Äkta 2D-isometriska spelassets med fast projektion, rena silhuetter, läsbara former, återhållen palett, diskret målad textur och återanvändbara moduler. Ingen fortsatt Blender-/3D-produktion. Det tidigare renderade gångprovet är arkiverat experiment och ska inte ersätta kampanjens figurer. Nedanstående Blender-anvisningar beskriver historiken, inte nästa produktionssteg.

Ett första direktgenererat 2D-kit med tolv delar finns i `assets/source/campaign/atland-modular-2d-candidate-v1.png`. Originalet har riktig alpha; golvprojektion och modulskarvar är ännu inte godkända. Tre korrigeringsförsök avvisades på grund av opaka bakgrunder respektive klippta golv. Kitet används inte i kampanjen. Promptar och kvalitetsstatus finns i intilliggande provenance-JSON.


## Senast · åtta banor genom fyra världar

Användaren prioriterade autonom bredd och ett separat detaljassetpass. 0.4 fortsätter nu från stranden genom åtta banor i fyra nya målade miljöer: Atland, rotmarkerna, Bergslagens underjord och Uppsala. Sekvensmekanismer, arkivval med följder, försvarsvågor, ångfaror, två bossmöten och ett förgrenat avslut är spelbara. Se `CAMPAIGN.md` för exakt omfattning, start, sparning och nästa assetpass.

Direktstart från huvudmenyn: **Spela nästa del · åtta banor**, med separat kapitelspartillstånd. Landstigning och äldre 0.3-avslut kan också fortsätta in i Atland. Nya röster och detaljföremål är inte producerade i breddpasset. Varje miljömålning delas av två banor; figurgrafik och musik återanvänds.

Verifierat: 29 139 regelassertions; full Godot-kampanj utan utvecklarskydd till slutet med 96 liv, 38 besegrade och 19 parader. Fyra rutter i regeltesterna täcker båda understöden och arkivvalen. Kampanjbilderna har granskats; överlappande kontrollhjälp och långa måltexter har justerats. Automatiserade GUI-prov körs med Xvfb för att undvika Waylands bakgrundsstrypning.

Karls skrämmande projicerade ansikte är ersatt i det separata gångprovet med det tidigare godkända målade huvudet, följande riggens huvudcentrum. Kampanjens gång är fortfarande den gamla. Detaljgranskning och integration återstår enligt användarens nya prioritering.

## Föregående · animationens omtag

Användaren godkände 3D-provets rörelse och gav klartecken att fortsätta till målade 2D-sprites. Första kostymmodellen och ett gemensamt målat projektionsunderlag är framtagna. Fyra riktningar med 30 transparenta bilder vardera visas i ett separat Godot-prov på stranden: `./start-walk-study.sh`. Vänster/höger riktning, mellanslag paus. Ingen 3D eller AI körs i Godot. Kampanjens gång är fortfarande oförändrad; ansiktslikhet, axel-/kragform och texturövergångar i nya modellen behöver fortsatt visuell bearbetning. Se `ANIMATION.md` och `assets/source/animation/` för exakt produktionskedja och ursprung.

Användaren avvisade två 2D-försök: deformerade målningar gav ”gummiben”, stela bilddelar såg ut som brutna ben. Båda är borttagna från spelkoden. Ny riktning: rörelsen byggs i 3D före målning och atlasrendering. Första riktiga armaturen och en animerad provfigur finns i `assets/source/animation/karl-walk-reference.blend`, genererad och geometriskt verifierad av `tools/build_walk_reference.py`. Fram-/bakvy är renderad; lokalt videoprov `artifacts/karl-walk-reference.mp4`. Detta är rörelseunderlag, inte färdig Karl-grafik eller en ny spelanimation. Se `ANIMATION.md` för reproduktion, begränsningar och fortsatt produktion. Spelet behåller gången från föregående checkpoint.

## Föregående · danska gångark och värjspets

Pikeneraren och skytten har nu egna provisoriska gångark i fyra riktningar. Sabelvakten hade redan ett. Alla fiender markerar nu verklig förflyttning, även skyttens reträtt; stegrytm beror på gångsträcka och gångbilden följer förflyttningsriktningen. Gång stoppar vid siktning/anfall. Pikenerarens kroppsskala mäts från hjälmen, inte från pikspetsen. Skyttens felordnade bakåtrader mappas om. Det samlade riggade omtaget av gång är fortfarande nästa större animationsarbete.

Karls värjspets korsade rutgränsen i atlasen: den klipptes i kontakten och syntes som en lös bit i angränsande poser. `AnimationAtlasLayout.cs` ger nu granskade individuella källrektanglar för Karl och sabelvakten. Tunga sabelhugg visar kontaktbilden relativt verklig kontakttid, i stället för att hoppa över den vid 0,22 sekunder. Fyrariktningsbildprov: `./start-art-study.sh --fullscreen -- --capture-attacks`, resultat `artifacts/sword-frames.png`. `--scene-check` fångar även tre danska roller i samtliga riktningar/steg.

Användaren godkände strandens visuella riktning uttryckligen: ”jag älskar denna miljö den är så cool”. `shore-v1.png` är riktmärke för fortsatt miljöarbete: mossig sten, tungt vatten, blek gryning och allvarlig nordisk storslagenhet. Bilden har inte ändrats i denna fix.

Nästa avsnitt får gärna blanda in känslan av LucasArts Indiana Jones och Fate of Atlantis som en lätt hyllning. Konkret riktning finns nu i `VISION.md`: arkeologiska mysterier, användbara fynd, mekanismer, hemliga passager och alternativa lösningar, med fortsatt närstrid och seriösa figurer. Detta är nästa avsnitts kreativa riktning, ännu inte implementerat innehåll.

Verifierat efter gång- och värjfixarna: 28 865 regelassertions passerar, inklusive skyttens framåt-/bakåtrörelse och Karls atlasgränser. Scenkontrollen passerar; danska roller och värjans fyra riktningar är visuellt granskade. Separat Linux-export `dist/AtlandsArv-0.3.2/AtlandsArv.x86_64` laddar och fångar spelbild med avslutskod 0. Native-bildprovets avslut rapporterar fortfarande 24 ObjectDB-instanser och 12 resurser kvar; ingen laddnings- eller spelfunktion fallerade i provet. Befintlig spelprocess har inte startats om.

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

Linux-export: `dist/AtlandsArv-0.4/AtlandsArv.x86_64`. Native `--smoke --atland` passerar med bildfångst, avslutskod 0 och utan resursläckagevarningar efter att bildfångstens Image-disposal rättats. Exporten innehåller de åtta banorna, inte det senare 2D-kitförsöket.
