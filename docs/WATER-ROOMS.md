# Pumphuset och cisternen · fyra rum

2026-09-07. Rumsprovet omfattar nu Förgården → Logementet → Pumphuset → Cisternen, med en genväg från cisternen tillbaka till förgården. Detta är nästa checkpoint mot sexrumsbanan, inte hela banan eller en ny bossleverans.

## Prova

`./start.sh -- --rooms`, samma menyknapp och sparfil som tidigare. Äldre två-rumssparningar får de två nya rummen tillagda som obesökta; fiender, utrustning, fynd och utforskning i de gamla rummen behålls.

Efter logementets kista följer Karl ritningen ut genom södra passagen. I pumphuset väntar två vakter. Säkra rummet, använd östra tryckavlastningen och därefter västra matarhjulet med E/B. Den centrala bassängen töms synligt och passagen till cisternen blir tillgänglig. Att försöka vrida matarhjulet först ger en begriplig låsorsak.

Cisternen är avsiktligt fri från obligatorisk strid. Altaret ger en Atlands edsklinga, som kan lämnas kvar om väskan är full. Fyndet är valfritt: det behövs inte för att lyfta genvägens regel vid östra väggen. Släpp och tryck E/B igen efter upplåsningen för passage. Genvägen fungerar sedan åt båda hållen och förblir öppen efter laddning. Karl kan också gå tillbaka samma väg genom pumphuset.

R visar besökta rum, vatten-/genvägsstatus och ritningens instruktioner. Utforskningsmörker, karta och rumsbundna fynd fungerar i de nya rummen.

## Grafik och geometri

Tre nya originalbilder, genererade med det inbyggda OpenAI-bildverktyget:

- `assets/art/room-pump-v1.png`: vattenfyllt pumphus.
- `assets/art/room-pump-low-v1.png`: samma målning med tömd bassäng.
- `assets/art/room-cistern-v1.png`: den sänkta cisternen.

Fulla promptar och källvägar finns i `assets/source/rooms/water-art-v1.json`. Bilderna är kopierade utan efterbearbetning. Förgårdens och logementets tidigare målningar är oförändrade. Samma målade 2D-figurer används; inga Blender-/3D-aktörer.

Gångpolygoner, bassänghinder, ingångar, reglage och altare är kalibrerade efter de faktiska bilderna i `PortRooms`. Bassängerna blockerar rörelse och direkt sikt/skottväg. Bassängernas målade kanter deltar i djupsorteringen. Reglagens/altarets målade kroppar står utanför gångytan; mörkermasken visar dem från respektive åtkomlig interaktionspunkt. Det är grafisk visning, inte en genväg runt fiendernas siktregler.

## Beständighet

`RoomLinks` innehåller fyra förbindelser med punkt på båda sidor, ankomstpunkt och låsvillkor. Rumsövergången flyttar fiendelistan mellan aktivt tillstånd och rummets snapshot. Nya rum skapar sina möten endast vid första besöket. Inga resurser fylls på vid rumsbyte.

`LayoutVersion` är nu 2. En äldre karta med exakt de två ursprungliga rummen uppgraderas vid laddning. Felaktiga äldre kartor avvisas. Tryckavlastning, sänkt vatten, genväg och relik har egna flaggor. Valideringen kräver förenliga besök/lås: en besökt cistern kräver exempelvis sänkt vatten. Markloot behåller sitt rum och sin unika föremålsidentitet.

## Verifiering och kvarstående arbete

29 228 regelassertions passerar. De provar hela fyrarumsloopen med båda understöden utan utvecklarskydd, migration från äldre sparning, trycklås, spärrad trappa, bassängkollision, delvis löst mekanism, valfri relik, full väska, återbesök och genväg åt båda hållen efter laddning. Befintliga kampanj-, inventarie-, rums- och sikttester passerar också. Botkörningarna når tillbaka med 70 respektive 66 liv; detta är inte mänsklig provspelning eller en bedömning av slutlig svårighet.

`--water-check` tar spelbilder vid ankomst, före/efter tömning, vid altaret, genvägen och journalen. Godot- och native-proven passerar. Native `--fog-check` behåller exakt bildlikhet med/utan dold boss och loot. Bilder har granskats. Export: `dist/AtlandsArv-0.6.2-water/AtlandsArv.x86_64`; paketets bilder ligger i dess egen `artifacts/`. Första native-vattenprovet rapporterade kvarhållna resurser vid avslut, som tidigare prov; verbose-omprovet avslutades utan varningen. Båda gav kod 0 och godkända assertions.

Vittnesgalleriet och Edskammaren med Edsväktaren återstår. Nya röster, rumsambienser, dedikerade mekanismljud och rörliga dörrblad är inte producerade här. Vattenbytet växlar mellan två målningar; det är ännu ingen kontinuerlig tömningsanimation. Genvägen på förgårdssidan använder tills vidare en passagemarkering på befintlig målning. Rumsprovet har fortsatt egen sparfil och är ännu inte infogat i den långa kampanjrutten.
