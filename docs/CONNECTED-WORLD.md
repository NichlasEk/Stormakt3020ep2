# Sammanhängande Atland · 0.12

Alla nio rum i huvudrutten ligger nu på samma gångbara karta. Förgården → logementet → pumphuset → vittnesgalleriet → edskammaren → arkivet → rötternas trappa → lunden. Pumphuset → cisternen → förgården är en fysisk sidoväg som sluter en krets. Gå genom passagerna med vanliga rörelsekontroller. E / B används för föremål, reglar och dörrblad.

## Spela

`./play.sh` → **Spela · sammanhängande Atland**. Befintliga rumssparningar migreras när expeditionen öppnas. `./play.sh -- --rooms` går direkt till rumsvägen. Den gamla separata dörrstudien finns kvar med egen sparning; `--doors-new` återställer endast studien.

Första ekporten låses upp med väktarens nyckel eller slås sönder med vapen. Nyckeln uppstår inte magiskt vid förstöring. Cisternens genväg reglas upp från cisternsidan. Sten-/motviktsportar följer rummens berättelsevillkor. Vattenvägar och ritnings-/vittnespassager blir fria gångvägar efter respektive uppgift. Vanliga öppnade dörrblad kan stängas igen; bladets svep stannar om Karl eller en vakt står i vägen.

Kartan i journalen visar förbindelserna. Minikartan följer Karl även mellan rum. Befintliga fynd, utrustning, stash, sår och utforskning behålls. Passagernas utforskning sparas också.

## Teknik

`ConnectedWorld.cs` placerar de ursprungliga målningarna och deras uppmätta golv i en gemensam, fast isometrisk projektion. Navigeringen testar unionen av golv och korridorer, med gemensamma hinder för gång, anfall, projektiler och sikt. Huvudstråket följer målningarnas diagonala markaxel. Cisternens nedre slinga korsar inte andra passager.

Simuleringen behåller rummets lokala koordinatsystem för befintliga interaktioner. Vid en gräns flyttas koordinatsystemets origo; spelare, samtliga aktörer, låsta anfallsriktningar, projektiler, farozoner och fynd får exakt samma translation. Världspositionen ändras inte. Kamera, interpolering och synliga effekter får samma förflyttning. Ingen scenladdning, förflyttning till en ankomstpunkt, läkning eller rensning av striden sker.

Alla besökta rums aktörer ligger kvar i simuleringen med permanent `HomeRoom`. Deras ägarrum styr engångshändelser och sparvalidering; närliggande fiender kan följa Karl över gränser. Avlägsna fiender sover. Nya rums möten skapas vid första inträdet. Besökta rum skapar aldrig nya kopior av vakter eller fynd.

Det gamla lokala rumsläget behålls för äldre regressionstester. Huvudmenyn, `--rooms`, återupptagna rumssparningar och landstigningens fortsättning aktiverar den sammanhängande världen. Den separata dörrstudien ändras inte.

## Bilder och avgränsning

De nio rika genererade bakgrunderna är bevarade. Förbindelserna byggs av 2D-polygoner med det nya, direktgenererade `passage-materials-v1.png`, målad ekport, murade karmar och låga murar med djupsortering. Ingen Blender-/3D-scen används. Målade föremål i rummen behåller sina förgrundslager.

Detta är första integrerade passagepasset. Korridorerna har återanvändbara material; deras anslutningar till varje målning och individuell miljörekvisita behöver ett separat konstnärligt detaljpass. Det finns ännu inga spelbara båt-/rymdskeppsresor mellan nya regioner i denna checkpoint.

## Färder och originalets skepp

Designregeln är nu: fysisk passage mellan sammanhängande rum; båt mellan skilda kuster; Karls rymdskepp när avståndet eller berättelsen kräver det. Ingen snabbresa genom klick på rumskartan.

Originalets aktuella projekt finns i `/home/nichlas/WaylandForge`, inte längre i den gamla SystemRegisIII-sökvägen. `assets/stormakt3020/README.md` beskriver spelarskeppet **Karl CCLV**, blå/gult och mässing, med tre kronor, ångventiler och modulära vapen. Samma dokument beskriver uppskjutningen efter Louhis tempel. Läs även `docs/stormakt3020-level-07-kopenhamns-ring-codex-argentum.md` inför kommande reseproduktion. Originalet är endast läst. Skeppet ska behandlas lika sakligt som en örlogsbåt; absurditeten ligger i situationen och radiodialogen.

## Verifiering

- `dotnet run --project tests/Atland.Tests.csproj`: äldre strids-, rums-, inventarie- och dörrtester samt den nya sammanhängande rutten.
- `--world-only`: fysisk förflyttning genom alla nio rum och genvägen i båda riktningar, ingen positionsdiskontinuitet, projektiler kvar vid gräns, sparning i korridor, migration, vaktförföljelse över rumsgräns och upptaget dörrsvep.
- Native `--world-check`: bildkontroll i spelets renderer, öppen port och faktisk gång in i logementet. Bilder i `artifacts/connected-door.png` och `artifacts/connected-lodge.png`.

Verifierat 2026-09-07: 38 631 kontroller i hela sviten; 5 474 kontroller i den avgränsade världssviten. .NET-build utan varningar. Komplett Linux-export `dist/AtlandsArv-0.12-connected`, inklusive .NET-data, klarar både `--world-check` och det befintliga `--door-check`. Kamerans världsposition kontrolleras vid koordinatbytet. `play.sh` pekar på denna verifierade export.
