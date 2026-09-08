# Checkpoint · Vägen under vattnet · 2026-09-06

## 0.12.2 · Arkivets och Rotvägens målade portar

Nästa del av portpasset är byggd: Arkivet → Rotvägen → Lunden använder nu de befintliga målade öppningarna. Nya öppna bakgrunder, fysiska ekblad, förgrundsmasker, fiendeförföljelse åt båda håll och sparmigrering till anslutningsrevision 3. [Leverans och återstående arbete](ROOT-ARCHES.md).

## Nästa kapitel · plan efter provspelning

Användaren uppskattar den sammanhängande världen och berättelsen med radio-/porträttkort. Fortsättningen är nu konkretiserad i [NEXT-EXPEDITION.md](NEXT-EXPEDITION.md): återstående målade portanslutningar, fem föreslagna platser i regementets marker, Rotmarskalken, arkivvalets följder och båtfärden mot berget. Kapitelinnehållet är fortfarande planarbete; portpasset fortsätter i 0.12.2.

## 0.12.1 · gå genom den målade porten

Vittnesgalleriets högra valv leder nu fysiskt till Edskammarens vänstra port. Bron framför galleriväggen är borttagen. Utgångs- och riktningspilar är avstängda i den sammanhängande rutten. [Omfattning och provspelning](PAINTED-ARCHES.md).

## 2026-09-07 · 0.12 sammanhängande Atland

Alla nio huvudrum och cisternens genväg är nu fysiskt förbundna. Kameran och striden fortsätter över rumsgränser; nyckelport, reglad genväg, öppna gångvägar, sparning och minikarta ingår. [Aktuell omfattning och nästa detaljpass](CONNECTED-WORLD.md). Nästa geografiskt åtskilda område ska nås med båt eller Karl CCLV, med originalets skepp som referens. Transporterna är ännu inte byggda. Äldre checkpoints nedan är historik.

## Senast · dörrens visuella passning · 0.11.3

Gångjärn/stängningsände och höjd inmätta mot stenöppningens insida. Fast ekkarm, synliga järngångjärn och sammanhängande målade dörrytor. De tätade kollisionsskarvarna finns kvar vid den nya placeringen; 3 583 riktade kontroller samt native bild-/uppspelningskontroll används. Se `PHYSICAL-DOORS.md`. Sammanhängande karta är fortfarande nästa större steg.

## Senast · dörrskarvens kollision · 0.11.2

Danskens genomgång återskapad vid fria dörränden. Dörr/karm har nu överlappande rundade kollisionsändar, och dörrprovets figurförflyttning kontrollerar hela rörelsen inklusive undanträngning och rekyl. 3 583 riktade kontroller täcker Karl, vakter, båda sidor, båda karmar, trängsel samt öppen/förstörd dörr. Se `PHYSICAL-DOORS.md`. Inga nya miljöer eller passager i denna buggfix.

## Senast · ekportens tjocklek · 0.11.1

Målat dörrblad med fram-/baksida, överkant och ändträ. Renderingen följer gångjärnets isometriska projektion, inom befintlig kollisionsmarginal. Befintliga texturer och sparningar används. Bildkontroll i stängt, halvöppet, öppet och förstört läge. Nästa önskemål är hela kartan som sammanhängande områden, inklusive öppna passager; första föreslagna anslutningen är logementet till pumphuset. Se `ROADMAP.md` och `PHYSICAL-DOORS.md`.

## Senast · fysisk dörr och standardrutt · 0.11 · 2026-09-07

Niorumsrutten är huvudmenyns primära expedition och den vanliga strandens fortsättning. Hälsa, förbrukning och inventarium bevaras; senast spelad rumsexpedition hittas i befintliga sparplatser. Äldre påbörjade åttastegskampanjer lämnas intakta.

Separat spelbart två-rumsprov med ny gemensam målning, målad dörryta, exakt gångjärnsprojektion, aktiv vaktförföljelse, nyckel, öppna/stänga, kollision/sikt, upptäcktsmörker och förstöring. Egen sparning och fyra ljudskisser. 29 593 assertions. Se `PHYSICAL-DOORS.md`. Nästa steg är provspelning och därefter införande av de fysiska passagerna i huvudrutten; sprängning och fienders dörrbrytning återstår.

## Senast · Expeditionen till Atland · 0.10 · 2026-09-07

24,2 sekunders intro med tre genererade bilder, lokala LTX-klipp, tre nya Ebba-röster och tidsstyrd textning. Filmkatalogen innehåller introt och Arkivets port. Ny Atland-kampanj/rumsexpedition visar introt; återupptagen sparning gör det inte. Musik återanvänds. Kustklippet hittar på byggnader och mjukar upp penselstrukturen; nästa bildpass bör förbättra följsamheten mot målningen. Se `INTRO.md` för verifiering och produktionskällor.

Banornas nästa steg är fortfarande vägen mot berget. Detta pass tillför film, inte rum eller figuranimation.

## Senast · arkivfilm och videobibliotek · 0.9.1 · 2026-09-07

Första mellansekvensen är byggd: 8,1 sekunder lokal LTX-2-film med eget miljöljud, ny genererad ingångsbild och portöppning mot rötterna. Visas vid första arkiv→Rotvägen-passagen. Inställningarnas utvecklarverktyg har ett videobibliotek för återspelning utan ändrad sparning; Esc/B och naturligt slut återvänder rätt. Spelet pausas och filmens ljud följer mastervolymen. Källvideo, exakt graf, promptar och jobb är sparade.

29 574 regelassertions. Godot/native videokontroller beskrivs i `CINEMATICS.md`; Linux-export `dist/AtlandsArv-0.9.1-cinematic/AtlandsArv.x86_64`. Direktprov `./start.sh -- --gate-film`. Nästa filmpass bör förbättra den målade stilen i yttergården och höja upplösningen efter användarens bedömning. Större intro och fler filmer är inte levererade. Banornas nästa checkpoint är fortfarande vägen mot berget.


## Senast · Rotvägen · 0.9 · 2026-09-07

Nio beständiga rum. Två nya målningar för Rötternas trappa och De namnlösas lund, motviktsport, avtrycksstrid med en/två eftertrupper beroende på arkivvalet och Nornans vittnessigill som beständig belöning. Fem nya lokala Ebba-/Hedvig-repliker. Layoutversion 5 migrerar äldre sparningar, utan hälsopåfyllning eller tappad stash. Full väska blockerar inte progression, återbesök dubblar inget.

29 558 regelassertions. Åtta vapen-/understöds-/beslutskombinationer klarar Rotvägen utan utvecklarskydd. Godot och Linuxkontroller samt bild-/röstgranskning dokumenteras i `ROOTWAY.md`. Export: `dist/AtlandsArv-0.9-rootway/AtlandsArv.x86_64`. Prova `./start.sh -- --rooms`, E/B vid arkivets säkrade grind.

Nästa byggsteg: den första vägen mot berget, med ett eget möte och bevarad återväg. Spelbalans och röster behöver användarens provspelning. Figurer, föremålsbild till sigillet och miljöljud återanvänds; ingen ny boss/gånganimation eller rörligt portblad i denna checkpoint. Historiken nedan beskriver äldre slutpunkter.


## Senast · Minnets arkiv · 0.8 · 2026-09-07

Sju beständiga rum. Arkivet har en egen rik målning, läsbord med kollision/förgrund och en målad dokumentvy. Två motstridiga handlingar ger ett permanent val: bevarat vittnesmål kallar tre vakter, falsk passersedel lämnar en kontrollant. Fyra nya svenska Ebba-/Hedvig-repliker. Layoutversion 4 migrerar äldre rumssparningar; beslut, fiender, utforskning, stash och återväg består. Ingen läkning eller duplicering vid dörren.

29 442 regelassertions passerar, inklusive åtta kombinationer av vapen, understöd och beslut utan utvecklarskydd. Godot `--archive-check` verifierar dokumentknappar, paus, Esc/återöppning, båda striderna, sparade resultat, röster och sjurumskarta. Linux-export och slutkontroll beskrivs i `ARCHIVE.md`.

Prova `./start.sh -- --rooms`. Fortsätt genom den öppnade bossporten med ett nytt E/B-tryck. Slutpunkten är arkivets säkrade grind, med återväg till alla tidigare rum. Rotvägen är ännu inte spelbar i denna rutt. Nästa steg: första rotmarksrummen och deras anslutning; gånganimation och mänsklig balans-/röstbedömning återstår.


## Senast · rumsradio och underjordiskt ljud · 2026-09-07

0.7.1 levererar åtta nya svenska Ebba-/Hedvig-repliker från lokal EutherLink/dots.tts-mf, sju egna syntetiserade effekt-/miljöljud och mjuk övergång från hamn till underjord. Porträtt och musik återanvänds. Vittnesboken kan fortsätta över en dörr; inaktuella råd/artilleri rensas efter bossen. Första rådet spelas en gång via sparad attackräknare. Den gamla kajbriefingen spelas inte längre i rumsrutten, och laddning visar aktuellt rumsmål. Manus, jobb, källor, hashvärden och automatisk uttalskontroll finns i `ROOM-AUDIO.md`.

29 290 assertions. Godot och Linux `--room-audio-check` passerar genom sex rum samt verifierar de åtta rösterna, undertexter, effekter, miljömix, köhantering och porträtt. Export: `dist/AtlandsArv-0.7.1-radio/AtlandsArv.x86_64`. Native slutkontroll avslutar med kod 0; editorprovet hade de tidigare intermittenta resursvarningarna. Uttal/mix behöver mänsklig provlyssning; Whisper-transkriptionen redovisar kvarvarande avvikelser.

Nästa byggsteg är Minnets arkiv som sjunde beständigt rum med dokumentval och återväg. Anslutningen är kodgranskad och beskriven i `ARCHIVE-CONNECTION.md`, men inte implementerad. Denna checkpoint utökar ljudet, inte antalet banor.


## Senast · Vittnesgalleriet och Edsväktaren · 2026-09-07

Sex sammankopplade rum med valfri cistern. Två nya rumsmålningar, pumpens sidopassage, öppen bossport och egen fyrposers 2D-bossatlas. Galleriets vittnesbok öppnar Edskammaren; Edsväktarens riktade sköldrus mot återanvändbara pelare ger en skadeöppning. Solid pulpet och två pelare styr navigation, sikt och förgrundsordning. Layoutversion 3 migrerar äldre rumsrutter utan reset. Hjälmbelöning, boss/slutflagga och återbesök sparas. Se `OATH-ROOMS.md` för exakt omfattning och bildproveniens.

29 274 assertions, inklusive åtta nya kombinationer av vapen/understöd/valfri rutt utan utvecklarskydd. Godot och fristående Linux `--oath-check` passerar genom sex rum, boss, port, sparning och journal; bilder granskade. Bygget har 0 varningar/fel. Exporten finns i `dist/AtlandsArv-0.7-oath/AtlandsArv.x86_64`. Rutten slutar vid den öppnade inre porten; arkivanslutning och nya radio-/ljudassets är nästa steg. Bossens gång består ännu av fasta poser; mänsklig svårighets- och tempobedömning återstår.


## Senast · pumphus, cistern och genväg · 2026-09-07

Rumsprovet är nu fyra rum. Tre nya genererade bilder: pumphus högt/lågt vatten och cistern. De två första målningarna består. Logementets ritning öppnar vägen till pumphuset; avlasta östra trycket och vrid västra matarhjulet. Cisternens fynd är valfritt, och dess regel öppnar en återvändbar genväg till förgården. Rumslayout version 2 migrerar gamla två-rumssparningar utan omstart. Geometri och sikt följer granskade gångytor/bassänger. Reglage/altare visas från sina åtkomliga interaktionspunkter. Detaljer och promptmanifest i `WATER-ROOMS.md`.

29 228 regelassertions passerar inklusive fyra kampanjrutter och fyrarumsloopen med båda understöden utan utvecklarskydd. Export: `dist/AtlandsArv-0.6.2-water/AtlandsArv.x86_64`. Nästa arbete: Vittnesgalleriet och Edskammaren med Edsväktaren; nya röster/ljud och rörliga dörrblad återstår.

## Senast · upptäcktsmörker och sikt · 2026-09-07

Rumsprovet har nu sparad utforskning, dämpad mark vid återbesök, aktuell sikt, hinderstyrd fiendeupptäckt och en liten utforskningskarta. Dolda fiender, lik, loot, etiketter, riktningspilar och bossmätare avslöjas inte av UI. Målningar och 2D-figurer består. Sikt använder samma gång-/hindergeometri som rörelse och projektiler; mörkermasken förlängs uppåt över synliga figurer så huvuden/ben inte kapas vid cellkanter. Se `EXPLORATION.md` för omfattning och nästa steg.

Regeltester: 29 207 assertions, inklusive äldre rumssparning, maskvalidering, hinder, vaktupptäckt, sparning och båda understöden genom rumsprovet. Bildtestet jämför hela renderade bildens pixlar med/utan en dold boss och loot; de är identiska. Fler rum, flera hinder och nya bossen återstår. Ny Linux-export: `dist/AtlandsArv-0.6.1-fog/AtlandsArv.x86_64`.

## Senast · två beständiga rum · 2026-09-07

Första kodcheckpointen mot `ROADMAP.md` mål 1: `./start.sh -- --rooms`, egen menyknapp och `rooms-save.json`. Förgård och logement binds av en nyckellåst passage. Fiender och markfynd bevaras per rum, nedhållen E/B ger inte återstuds och dörrbyten ger inga resurser. R visar besöks-/fyndstatus. Nuvarande målningar återanvänds och deras faktiska öppningar används; ingen ny 3D eller ny assetgeneration. Sexrumsbanan, upptäcktsmasken och nya bossen återstår. Exakt omfattning, begränsningar och testrecept i `ROOMS.md`.

29 192 regelassertions passerar inklusive båda understöden genom hela rumsprovet utan utvecklarskydd och de fyra befintliga kampanjrutterna. Godot-menykontrollen för rum passerar, och bilder har granskats. Native-export under `dist/AtlandsArv-0.6-rooms/`; egna bilder under dess `artifacts/`.

## Senast · nulägesgranskning och nästa arbetsmål · 2026-09-07

Användaren upplever repetition och önskar fler rum per bana, dörrsystem, mörker på outforskade ytor och olika bossar. `ROADMAP.md` är nästa arbetsplan med konkret sexrumsrutt i Atland och kriterier för rum, dörrar, sparning, upptäckt och egna bossmekaniker. Granskningen bekräftar att de åtta senare stegen delar koordinater och fyra miljömålningar; senare bossar återanvänder Collector. Nya rumsfunktioner är ännu inte implementerade. Nästa kodsteg är sparbar rums-/dörrmodell och två sammankopplade provrum, sedan resten av första banan. Regeltesterna är omkörda: 29 165 assertions passerar. Denna checkpoint ändrar dokumentation, inte spelet.

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
