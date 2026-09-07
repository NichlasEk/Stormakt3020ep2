# Från arenor till expeditioner

2026-09-07. Nulägesgranskning mot `VISION.md`, `CAMPAIGN.md`, `Campaign.cs`, `Combat.cs`, `Navigation.cs`, `SaveStore.cs` och inventariekoden. Användaren vill ha mindre repetition, fler rum per bana, dörrar, mörker över outforskade områden och olika bossar. Detta är arbetsplanen; nulägeslistan nedan beskriver utgångsläget före rumsarbetet.

**Första checkpointen finns nu:** två sammankopplade provrum med nyckel/lås, separata beständiga fiender och fynd, återbesök och sparning. Se `ROOMS.md`. Mål 1 som helhet är fortfarande pågående; sex nya målade rum och bossen är inte klara.

**Andra checkpointen:** upptäcktsmask, sparad utforskning, sikt bakom last, dold fiende-/fyndvisning och utforskningskarta finns i de två provrummen. Se `EXPLORATION.md`. Mål 2 är provat i denna begränsade geometri, inte i en hel sexrumsbana.

**Tredje checkpointen:** fyra rum, egna målningar för pumphus/cistern, tryckavlastning och synlig tömning, ett valfritt fynd och en beständig genväg till förgården. Äldre två-rumssparningar migrerar. Se `WATER-ROOMS.md`. Vittnesgalleriet och Edskammaren med bossen återstår.

**Fjärde checkpointen:** sex rum, Vittnesgalleriets ledtråd, egen målad Edsväktare med pelarrus, unik hjälmbelöning och öppnad inre port. Flershindergeometri och migration av två-/fyrarumssparningar finns. Se `OATH-ROOMS.md`. Mål 1 återstår att koppla vidare till arkivet; mänsklig balansbedömning och radio-/ljudpass väntar. Ovanstående checkpoints är historik.

**Femte checkpointen:** åtta nya radiorepliker, pump-/port-/bossljud och underjordiskt miljöljud finns. Se `ROOM-AUDIO.md`. Arkivanslutningen är granskad och planerad i `ARCHIVE-CONNECTION.md`.

**Sjätte checkpointen:** Minnets arkiv är sjunde beständiga rummet. Egen målning och dokumentvy, två beslut med olika patruller, fyra nya radiorepliker och sparad återväg finns. Äldre layoutversion 3 får arkivet utan omstart. Se `ARCHIVE.md`. Nästa steg är en första sammanhängande rotmarksrutt med bevarad återväg; den gamla åttastegskampanjen är ännu separat. Animation, mänsklig balansbedömning och senare detaljpass återstår.

**Sjunde checkpointen:** Rotvägen fortsätter arkivet med två egna målningar, motviktsport, arkivberoende förstärkningar, sigillbelöning och fem röster. Nio rum, migration och återväg är provade. Se `ROOTWAY.md`. Nästa steg är den första vägen mot berget; balans, gånganimation och detaljpass återstår.

## Nuläge före rumsarbetet

- Spelbar inledning med kaj, magasin och strand, därefter åtta kampanjsteg i fyra målade miljöer. Understöd och arkivval har faktiska följder.
- Sabel, hammare, pistol, gard/parad, undanmanöver, fiender, loot, sparning, volym, utvecklarskydd, inventarium/stash och stats finns. 14 föremål har egna bilder.
- Repetitionens orsak syns i koden: alla åtta senare steg delar `Expedition.Entry`, `Exit`, `Nodes` och `Spawns`. Uppdragen varierar, men rörelsemönstret återkommer.
- Tre vanliga fienderoller och en Collector-roll. Kronfogden och slutväktaren använder Collector med ändrade livvärden och viss möteslogik; de är inte fullt egna bossar.
- Ingen generell rumskarta, dörrmodell eller upptäcktsmask. Navigationen hanterar en gångpolygon och ett hinder. Nuvarande områdesbyte tömmer stridstillståndet; det räcker inte för återbesök.
- Animationerna är fortfarande en svag punkt. Senare kampanjens unika röster, porträtt, fiendebilder och detaljföremål är ofullständiga. De tidiga radioinslagen och miljöernas riktning ger en bra grund.

Bedömning: fungerande spelbar prototyp med tydlig visuell identitet och kampanjens stomme. Innehållsdjup och variation behöver nu komma ikapp systembredden. Ett procenttal för hela spelet skulle ge falsk precision.

## Mål 1 · En sammanhängande Atland-bana

Nästa byggmål: **Atlands port och de förseglade kamrarna**. Sex handbyggda, målade spelrum med olika form och uppgift. Sikta på 15–25 minuter första besöket; tiden måste senare mätas med en mänsklig spelare.

| Rum | Funktion | Förbindelse / belöning |
| --- | --- | --- |
| Den dränkta förgården | Bekant portmålning, ankomst och kort strid | Två tydliga vägar in |
| Väktarnas logement | Trängre närstrid med skydd mot skyttar | Öppnar vägen till pumphuset |
| Pumphuset | Avläs vattenmärken och sänk vattnet; strid i avbrotten | Öppnar cistern och en återväg |
| Den sänkta cisternen | Valfri upptäckt, dold passage och utrustningsfynd | Genväg tillbaka till förgården |
| Vittnesgalleriet | Stilla upptäckt, användbar ledtråd och radio | Förberedelse inför portväktaren |
| Edskammaren | Eget bossmöte | Öppnar Minnets arkiv |

Vägen ska förgrena sig och sluta i en genväg. Minst ett rum utan obligatorisk strid. Ingen rutin att samla tre saker i varje rum. Varje rum får en egen målning eller en tydligt särskiljbar komposition; sex namn på samma arena räknas inte som sex färdiga rum.

### Teknisk ordning

1. Separera stabila rum-/dörrdefinitioner från sparat tillstånd. Behåll nuvarande kapitel-ID:n. Ett rum äger sin målning, gångytor, hinder, ingångar, möten och fynd.
2. Gör återbesök beständiga: besegrade fiender, bossliv, fynd, mekanismer och dörrar sparas per rum. Markloot får rum-ID utöver nuvarande region/steg. Övergång ger inte automatiskt läkning eller nytt loot. Projektiler avslutas vid säker rumsövergång.
3. Dörrar har öppet/stängt/låst tillstånd, krav och tydlig orsak när de inte går att öppna. Vanlig dörr, reglad genväg, mekaniskt lås och bossport är första uppsättningen. E/B interagerar. Samma dörrstatus styr passage, sikt och projektiler där rum delar spelutrymme.
4. Börja med separata sammanhängande rumsscener och kort övergång vid tröskeln. Det passar de rika målningarna och begränsar laddning/minne. Närliggande rum förladdas; ankomstpunkten ska vara fri och ge kontroll direkt. Inga automatiska fram-och-tillbakaövergångar medan E hålls nere.
5. För synliga stängda dörrar och rumsinteriörer behövs flera hinder och gemensamma siktregler. Utöka navigationen innan fiender förväntas gå runt dem. En målad dörr utan motsvarande geometri är inte färdig.
6. Ge den nya rutten en separat utvecklingsstart och sparfil tills migrationen är provad. Gamla kampanjsparningar ska behålla utrustning, stash och berättelseval; inga tysta omstarter.

### Klart när

- Alla sex rum går att nå, återbesöka och lämna; den valfria vägen behövs inte för att avsluta.
- Dörrar kan inte passeras låsta, ingen kan slå/skjuta genom ett stängt hinder, och fiender fastnar inte i trösklar.
- Spara/ladda före och efter lås, loot och boss fungerar utan duplicering eller återuppståndna fiender.
- En hel körning når arkivet utan utvecklarskydd. Bildprov visar alla rum, skalreferens, skymning och dörrlägen.

## Mål 2 · Utforskning och mörker

Bygg på rumsgrunden under mål 1. Outforskat är mörkt; redan besökta ytor kan visas dämpat, medan aktuellt synfält är fullt läsbart. Väggar och stängda dörrar stoppar avslöjandet. Masken sparas per rum. En liten utforskningskarta visar besökta rum och kända dörrar.

Fiender, lootnamn, interaktionsetiketter och bossmätare får inte avslöja oupptäckta rum. Mörkret är en upptäcktsregel, inte bara en svart cirkel runt Karl. Varningsytor för en pågående synlig strid måste förbli läsbara. Skymningsordning framför/bakom målade murar fortsätter att gälla.

Klart när en stängd dörr döljer innehållet bakom den, öppning visar rätt passage, återbesök minns upptäckten och sparning återställer masken. Testa också att UI inte läcker information.

## Mål 3 · Bossar med egen spelidé

Första nya bossen ingår i mål 1. Därefter en i taget med egen målad 2D-identitet, tydliga förvarningar, återhämtning, ljudeffekter och belöning. Varje talande roll behöver porträtt och röst. Nya namn eller fler livpoäng räcker inte.

| Bossförslag | Vad spelaren behöver göra |
| --- | --- |
| Edsväktaren, Atland | Locka ett riktat sköldrus mot en edspelare; angrip i återhämtningen. Skyddade lägen och rus ska vara läsbara utan textinstruktion. |
| Rotmarskalken, rotmarkerna | Välj mellan att bryta fanor som håller formationen samman och pressa befälhavaren. Rotgrepp ändrar säkra vägar. |
| Kronfogden, Bergslagen | Använd tryckventiler för att kyla rustningen; undvik långsamma hammarslag och tydligt varnade glödstråk. |
| Kollegiets siste fullmäktige, Uppsala | Bryt hans anspråk vid vittnesborden medan arenans sektorer förändras. Nornorna förblir självständiga makter i berättelsen. |

Klart per boss när spelaren kan läsa och undvika varje attack, en egen mekanik påverkar striden, mötet kan klaras med båda vapnen och ingen obligatorisk handling kräver tur eller ett förbrukat engångsföremål. Senare fas ska utveckla mekaniken utan att göra telegraph-tiden orimlig.

## Mål 4 · Ge varje värld egen rytm

Efter första kompletta banan: bygg ut en värld åt gången. Preliminär riktning är 4–6 rum per större bana, med längd anpassad efter innehållet. Ingen massproduktion av likadana rum för att nå en siffra.

- Atland: portar, vattennivåer, arkiv och arkeologiska genvägar.
- Rotmarkerna: öppna skogsgläntor, slingrande stigar, läger och ett regemente som vill få vila.
- Bergslagen: vertikalt antydda gruvgångar, tryck, smedjor och kontrollerade industriella faror.
- Uppsala: innergårdar, observatorium, gravrum och himmelsmekanismer med konkreta ledtrådar.

Varva tät strid, kort upptäckt, belöning och återkoppling från radion. Strategiska beslut ska kunna ändra väg, resurser eller fiendekombinationer. Bevara den älskade strandens lugn och nordiska storhet mellan striderna.

## Mål 5 · Rörelse, detaljpass och leverans

Rörelseförbättring är fortsatt ett separat viktigt arbete: sammanhängande målade 2D-steg, tydliga attackposer, hela vapen och trovärdiga ansikten. Det ska granskas i riktig kameraskala, inte bara i ett fristående animationsprov.

Gör varje ny banas detaljpass när geometri och mekanik fungerar: dörrarnas öppna/stängda bilder, framkantsmasker, kistor, mekanismer, bossbilder, radio med porträtt, rumsambienser och musikövergångar. Basala funktionella dörr- och bossbilder måste finnas redan i första spelbara versionen. Fulla promptar och ljudjobb sparas som tidigare.

Leverera ett granskbart delmål per checkpoint: aktuell status, testresultat, skärmbilder, startkommando, vad som återstår, commit och push. Tekniska tester kan köras medan användaren är på jobbet; upplevd svårighet, gång och tempo markeras som väntande mänsklig bedömning. Ingen tid låtsas vara provspelad.

## Nästa konkreta arbetssteg

Sexrumsrutten och Edsväktarens pelarmekanik är byggda och automatiskt provade med båda vapnen/understöden. Ljud-/radiopasset för vittnesboken och bossmötet är levererat. Bygg nu Minnets arkiv som sjunde beständigt rum enligt `ARCHIVE-CONNECTION.md`, med bevarad utrustning, fynd, återväg och berättelseval. Mänsklig balansbedömning väntar; därefter rotmarkernas egen rumsrutt och Rotmarskalken. Den långa kampanjen använder fortfarande sin befintliga rutt; rumsprovet startas separat.
