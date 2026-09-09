# 0.17 · Morgonen som inte tar slut

Uppsalas instrumentgård fortsätter nu till **Klockgångens återkomst** och **Meridiansalen**. Två nya beständiga rum, sammanlagt 22 platser, är inkopplade i den ordinarie expeditionen. Gårdens daterade sigill öppnar den målade porten; därefter går Karl genom sammanhängande passager till fots.

## Prova

Starta `./play.sh` och fortsätt din expedition. Återvänd till Instrumentgården om du redan tagit datumavtrycket. Använd E vid gårdens stora port.

Direktprov: `./play.sh -- --meridian`, eller **Esc → Inställningar → Prova Meridiansalen**. Det har egen sparning, `meridian-preview-save.json`, skild från både huvudexpeditionen och det tidigare Uppsalaprovet. `--meridian-new` börjar om just detta prov. Provet börjar vid Instrumentgårdens port med de tidigare fynden säkrade.

## Klockgången

Läs liggaren under urverket. Starta slagverket vid golvspaken och följ vakten genom två slag. Han går till samma märke, vänder och upprepar sträckan. Ett och samma inspelade Ebba-meddelande upprepas innan hon själv reagerar. Karl, hälsa, inventarium, föremål och tidigare rum återställs inte: det är ett lokalt förlopp som sitter fast, inte en omstart av sparningen.

Efter två klockslag kan plåtens märke hållas mot spärren. Vakten går då vidare genom valvet och försvinner ur gången. Dörren till salen lossnar. Vakten är en synlig återklang med det befintliga målade gångkortet, inte en ny stridsfiende eller källa till upprepat byte.

## Meridiansalen

Väktaren har egen målad 2D-grafik och eget radioporträtt. Han står vid sitt verk och angriper med mätlinjer; ingen glidande förflyttning eller ny 3D-modell används. Första mötet leder till undersökningen av plåten vid astronomens bord.

Plåten visar tre lägen: **sol mot öster, äpple mot väster, nyckel mot söder**. Detta är salens mönster, skilt från de tidigare gårdsinstrumentens anvisning. Det aktiva instrumentet markeras diskret. Vrid det rätt för att bryta skyddet och angripa under öppningen. Varje tredjedel av väktarens hälsa kräver nästa instrument; alla tre får en roll. Mätlinjerna varnas innan de slår och låser sitt mål före nedslaget. Under andra halvan blir takten snabbare.

Väktaren överlever och sjunker ned på knä. Ljuset förskjuts över golvet. En förflyttningsorder vid den förseglade porten blir kapitlets beständiga fynd: ett helt kvarter skall flyttas, men målet är överstruket. Väktaren kunde inte återkalla ordern och försökte hindra dagen då den skulle gälla.

Detta förklarar hans lokala handling, inte ursprunget till Atland eller hela tidssystemet. Det övre observatoriet är ännu inte spelbart. Detta är avsiktligt provspelningsstopp: ta handlingen tillbaka till expeditionen, diskutera vad fyndet betyder och bestäm sedan fortsättningen. Återväg och skeppsresa förblir möjliga.

## Bild och ljud

Fem nya bilder skapade med **inbyggda imagegen**, sparade och inkopplade i projektet:

- [Gårdens öppna port](../assets/art/room-uppsala-open-v1.png) · [prompt](../assets/art/room-uppsala-open-v1.prompt.md)
- [Klockgången](../assets/art/room-clockwalk-v1.png) · [prompt](../assets/art/room-clockwalk-v1.prompt.md)
- [Meridiansalen](../assets/art/room-meridian-hall-v1.png) · [prompt](../assets/art/room-meridian-hall-v1.prompt.md)
- [Väktarens fyra poser](../assets/art/meridian-warden-v1.png) · [prompt](../assets/art/meridian-warden-v1.prompt.md)
- [Väktarens radioporträtt](../assets/art/meridian-radio-v1.png) · [prompt](../assets/art/meridian-radio-v1.prompt.md)

Sexton nya svenska repliker i [meridian-radio.json](../assets/story/meridian-radio.json). Ebba och Hedvig behåller sina syntetiska röster; väktaren har en ny syntetisk VoxCPM2-referens och lokalt Dots-TTS-tal, mening för mening. Källjud, förfrågningar och manifest sparas i `assets/source/voices/meridian-*`. Lokal CPU-Whisper används för innehållskontroll och omtag, inte som ersättning för provlyssning. Klockslag och kuggrörelse är original PCM från `tools/build_meridian_audio.py`; befintlig musik används och allt följer huvudvolymen. Ingen ny videofil ingår i detta kapitelpass.

## Sparning och kontroll

Rumslayout **10** lägger till de två nya rummen och deras passager i äldre sparningar. Tidigare rum, fynd, föremål och hälsa bevaras. Instrumentlägen, upprepningsförlopp, öppnad gång, väktarens tillstånd och orderfyndet sparas. Belöningen delas inte ut igen vid återbesök. Provets sparning hålls separat.

`--meridian-check` kör kapitlet i Godot och fångar portar, gång, återklang, radioporträtt, mätlinjer, instrument, nederlag och karta. Simuleringstester går båda passagerna i båda riktningarna, migrerar 0.16-sparningar, kontrollerar upprepning utan återställning eller byte, kör båda vapnen och båda understöden utan utvecklarosårbarhet och återvänder med skeppet. Kontrollerna kompletteras med bildgranskning av förgrund, instrument och dörrblad.

Verifierat 2026-09-09: **78 166 simuleringskontroller passerar**. Bygget har noll varningar/fel. Både `--meridian-check` och den tidigare Uppsalafärdens `--uppsala-check` passerar i den kompletta Linux-exporten. Berättelsebilagan stämmer med samtliga 93 JSON-repliker.

Export: `dist/AtlandsArv-0.17-meridian/`. Behåll programmet, PCK och hela .NET-katalogen tillsammans.
