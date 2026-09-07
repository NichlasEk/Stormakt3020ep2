# Inventarium, utrustning och stash · 0.5

## Användning

- **I** öppnar/stänger inventariet. **C** öppnar statsfliken. Spelet pausas medan flikarna är öppna.
- Inventarium finns också i pausmenyn, så det går att nå med handkontroll. Styrkors väljer knappar, A aktiverar, B går tillbaka.
- Välj ett föremål för namn, beskrivning, bonusar och jämförelse med utrustat föremål. **Utrusta**, **Ta av**, **Flytta till stash** och **Flytta till väskan** gör verkliga, omedelbart sparade byten.
- **E/B** plockar upp närliggande fynd när Karl står stilla. En full väska lämnar fyndet på marken.
- Stash kan flyttas till/från först när området är säkrat: inga levande fiender, inkommande kulor eller fientliga områdesfaror. Den kan läsas även i strid.

## Innehåll

24 väskplatser, 60 stashplatser med sidindelning. Fem utrustningsplatser: sabel, hammare, rustning, hjälm och sigill. Båda vapenslagen finns alltid tillgängliga; TAB/RB väljer aktivt vapen. Ett vapen ersätts med ett annat i samma vapenslag. Rustning, hjälm och sigill kan tas av om väskan har plats.

14 originalskrivna föremål finns i katalogen. Karl börjar med sin vanliga sabel, hammare och slitna rock utrustade, samt hjälm, vadderad rock och ett sigill i väskan för att kunna prova systemet direkt. Grundutrustningen har inga bonusar, så äldre stridsbalans bevaras tills spelaren utrustar något bättre.

Var tredje besegrad vanlig fiende lämnar ett föremål. Indrivare/Kronfogde/slutväktare har egna belöningar. Murarens gömma ger dessutom Minnets sigill. Fynd visas som små tydliga markörer med namn när Karl närmar sig. Oupphämtade fynd lagras med område och bana; de flyttas inte automatiskt till stash vid avfärd. Duellövningen ger inget loot.

Bonusarna påverkar faktisk strid:

- Vapnets och sigillets skadebonus läggs till grundskadan före tungt hugg, kombinationer, ripost och fiendegard. Det inaktiva vapnet ger ingen skadebonus.
- Rustning, hjälm och vissa sigill ger procentuellt skydd mot närstrid, kulor och områdesskada. Gard räknas först, sedan utrustningsskydd. Skyddet är begränsat till 60 procent.
- Sigill kan öka återhämtning av uthållighet. Stats visar återhämtning i vila; gard/anfall har sina vanliga lägre grundvärden.

Statsfliken visar aktuellt liv och uthållighet, aktivt vapen, grundskada/tung skada, skadeskydd, återhämtning samt besegrade/parader. Utrustningen förändrar för närvarande stridsvärden och vapennamn, inte Karls målade figurutseende. Miljöerna är fortsatt de rika bakgrundsmålningarna och alla aktörer är 2D-sprites.

## Sparning och integritet

Inventarium, utrustning, stash och markfynd sparas i samma fältdagbok som expeditionen. Stash följer banbyten inom den sparningen. Landstigning, direktstart av Atland och separat portprov behåller sina skilda sparfiler; stash är inte global mellan dessa kampanjer.

Äldre sparningar utan inventariefält får grundutrustningen automatiskt. Varje föremål har en unik instansidentitet, så flera exemplar av samma sort kan finnas utan sammanblandning. Byte mot befintlig utrustning fungerar även när väskan är full; tidigare utrustning tar den nya sakens plats. Full väska/stash och låsta överföringar lämnar föremålet hos sin ursprungliga ägare. Vid laddning valideras identiteter, katalogreferenser, kapacitet, utrustningsplatser och markpositioner.

## Kod och verifiering

`Inventory.cs` innehåller katalog, ägande, byten, stats och lootregler utan Godot-beroende. `InventoryPresentation.cs` innehåller pausade flikar, egna enkla graverade UI-symboler och markfynd. Inga nya bild- eller röstgenerationer krävs för detta system.

- `dotnet run --project tests/Atland.Tests.csproj`: 29 165 assertions. Alla åtta banor med båda understöden och arkivvalen passerar. Nya tester täcker faktisk kul-/vapenskada, sigillåterhämtning, fulla behållare, dubbla upphämtningar, stashlås, sparning och äldre sparformat.
- `--inventory-check`: öppna flikar via tangentbord, utrusta, stats, stashsidor, deposit/withdraw, stridslås och återgång till spel. Bildfångster finns i `artifacts/inventory-*.png`; i native-exporten under dess egen `artifacts/`.
- Linux-version: `dist/AtlandsArv-0.5/AtlandsArv.x86_64`. Native-menykontrollen passerar.

Första systemversionen använder fasta föremålsdefinitioner, en ruta per föremål och knappstyrda flyttar. Handel, slumpade affix, drag-and-drop och synliga klädbyten är inte implementerade.
