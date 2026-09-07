# Förseglade rum · första checkpointen

2026-09-07. Första tekniska delen av mål 1 i `ROADMAP.md`. Två sammankopplade provrum: Den dränkta förgården och Väktarnas logement. Detta är inte den färdiga sexrumsbanan. **Uppdatering:** sparat upptäcktsmörker, sikt och utforskningskarta finns nu i rumsprovet; se `EXPLORATION.md` för senaste funktioner och export. Verifieringen nedan beskriver första rumscheckpointen.

## Prova

`./start.sh -- --rooms` eller **Atlands förseglade rum · prov** i huvudmenyn. Separat `rooms-save.json` och `rooms-manual-save.json`. F5 sparar manuellt, F9 laddar. I och C fungerar som i kampanjen. Befintliga kampanj- och portfiler behåller sina tidigare rutter.

1. Besegra förgårdens väktare och sök packningen vid markeringen med E/B. Nyckeln är en beständig uppdragsflagga; den kräver ingen väskplats.
2. Följ riktningen till den stora bronsporten. E/B låser upp; släpp och tryck igen för passage.
3. Säkra logementet, undersök kistan och plocka upp sigillet. Full väska lämnar fyndet på marken.
4. Återvänd genom öppningen. Dörren förblir öppen, vakterna döda och fynden stannar i rätt rum. R visar rumssambandet och fyndets ledtråd.

Den här versionen kräver att rummet är säkrat före passage, inklusive inkommande projektiler och fientliga områdesfaror. Ingen fiendejakt mellan rum ännu. Att hålla E/B nedtryckt ger inte flera dörrhandlingar. Rumsbyte ger inga liv, tinkturer eller nya föremål.

## Implementation

- `Rooms.cs`: rumstillstånd, nyckel, dörr, besök, belöning och övergång. Aktiva fiender ligger i `Combat.Enemies`; inaktiva rums fiender ligger i sina snapshots. Listan flyttas vid övergång, inte kopieras eller återskapas.
- `Inventory.cs`: markfynd har även `Room`, tom sträng för äldre kampanjfynd. Föremål behåller samma unika identitet när de lämnas kvar, hämtas, utrustas eller lagras i stash.
- `SaveStore.cs`: rum sparas med den befintliga fältdagboken och kontrolleras för kända rum, besök, låsberoenden och unika fiende-ID:n. Gamla sparningar saknar `Rooms` och fortsätter utan rumsläget.
- `RoomPresentation.cs`: separat start, markeringar, låsstatus, journal och GUI-prov. `SceneryDepth.cs` använder magasinets befintliga förgrundsskymning för logementets last och murar.
- Förgårdens gångpolygon når den målade bronsporten; logementet använder magasinets befintliga gångyta/hinder. Kameraoffset begränsas till målningen i rumsläget.

## Grafik och kvarstående arbete

Provet återanvänder `world-atland-v1.png` och `warehouse-v1.png` utan ändringar på disk. Bronsportens lås har en liten statusikon; dess rörliga dörrblad är ännu inte animerade. Logementets öppning finns i målningen. Alla aktörer är samma målade 2D-sprites. Ingen ny bild-/röstgeneration i denna tekniska checkpoint.

Upptäcktsmask och siktregler finns nu. Nästa del är särskilt målade rum/dörrlägen, fler passager och genvägar. Nyckel/fyndmarkeringar och rummens återbrukade innehåll är fortfarande ett prov. Sex rum, nya bossen, förladdning/övergångseffekt och integration i den långa kampanjen återstår. Det här provet ändrar inte gamla sparningar till den framtida rutten.

## Verifiering

- `dotnet run --project tests/Atland.Tests.csproj`: 29 192 assertions. Befintliga fyra kampanjrutter passerar. Nya fall täcker lås, full väska, nedhållen interaktion, spara/ladda vid flera steg, kvarlämnade fynd, återbesök, skadad fiende, skadade rumssparningar och gamla kampanjfiler.
- Två genomspelningar via vanliga `Combat.Step`-regler utan utvecklarskydd: artilleri och fältvård. Tre fiender besegras, kistan hittas och Karl återvänder. Detta är botkörningar, inte bedömd mänsklig speltid eller färdig balansering.
- `--rooms-check` tar bilder av låst/upplåst port, logement, kista och stats; provar båda passagerna och återbesök i Godot. Testläget skriver inte till användarens sparningar.
- Linux-export: `dist/AtlandsArv-0.6-rooms/AtlandsArv.x86_64`. Native `--rooms-check` passerar med avslutskod 0 och utan resursläckagefel; bildproven är granskade. Bilder från paketet ligger under dess egen `artifacts/`, inte rotens.
