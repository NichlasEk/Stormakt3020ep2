# Upptäcktsmörker och sikt · rumsprovet

2026-09-07. Nästa checkpoint efter de två beständiga rummen. Kör `./start.sh -- --rooms`; befintlig rumssparning fungerar.

## Spelbeteende

- Outforskad yta är mörk. Karl avslöjar närliggande mark med fri sikt. Lastön och rummets gränser stoppar sikten enligt samma geometri som passage och projektiler.
- Tidigare utforskad mark visas dämpad när Karl går därifrån. Fiender, lik, markfynd, föremålsetiketter och bossmätare visas bara i nuvarande sikt, inte enbart för att marken är känd.
- En liten karta visar utforskad mark, aktuell sikt, Karl och upptäckta dörrars låsstatus. Riktningspilen avslöjar inte koordinaten till ett okänt fynd.
- Vakterna börjar inte jaga genom lasten. De reagerar först när de har fri sikt inom samma räckvidd, eller blir skadade. Upptäckta vakter fortsätter jaga. Attackernas ursprungliga förvarningar används.
- Utforskning sparas per rum tillsammans med expeditionens kontrollpunkter/manuella sparning. Besök i logementet ändrar inte förgårdens karta. En öppnad dörr avslöjar inte automatiskt hela nästa rum.

## Teknik och bild

`RoomSight.cs` använder ett 48 × 32 fält med 32 världspixlar per cell och 330 pixlars siktradie. 192 byte per rum lagrar den beständiga bitmasken. Nuvarande sikt beräknas normalt var sjätte fysiksteg och direkt vid större förflyttning/rumsbyte/laddning. Exakt avstånd och fri väg avgör om en aktör eller interaktion får synas; grafikens interpolerade mask styr inte spelreglerna.

`RoomFog.cs` bygger en liten återanvänd textur för mörkret. Synlig mark förlängs uppåt i bild för målade figurer och arkitektur; hela synliga figurer bevaras vid cellkanter. Därför är den visuella masken något generösare än markens exakta sikt. Detta visar väggars höjd utan att visa fiender bakom dem. Målningarna och deras förgrundsskymning finns kvar. Ingen ny bildgeneration eller ändring av aktörsstilen.

Äldre rumssparningar utan `Explored` får en tom mask och avslöjar området runt nuvarande position. Vi kan inte återskapa exakt vilka korridorer som besöktes före funktionen fanns. Felaktig masklängd och utforskning i ett obesökt rum avvisas vid laddning. Vanliga kampanjsparningar påverkas inte.

## Omfattning

Funktionen gäller de två provrummen. Dörrarna binder fortfarande separata rum; ingen sömlös sikt in i grannrummet ännu. Lastön är det första provade sikthindret. Flera hinder, nya rumsmålningar, rörliga dörrblad och den nya bossen hör till fortsatt utbyggnad. Mörkret är ett utforskningssystem, inte fysisk ljussimulering från varje lykta.

## Verifiering

- 29 207 regelassertions passerar: fri/blockerad sikt, räckvidd, vaktupptäckt, bibehållen utforskning, rumsisolering, sparning/laddning, migrering av äldre rumssparningar och felaktiga masker. Även båda understöden genom rumsrutten och befintliga kampanjrutter passerar.
- `--fog-check`: skärmbilder före/efter upptäckt, bakom lastön, utanför nuvarande sikt och efter laddning. Renderade pixeldata jämförs med och utan dold boss/loot: hela bilden ska vara identisk, inklusive HUD och bossmätare.
- `--rooms-check`: fortsatt prov av nyckel, dörr, båda passagerna, kvarlämnade fynd, återbesök och inventarium.
- Export: `dist/AtlandsArv-0.6.1-fog/AtlandsArv.x86_64`. Native `--fog-check` och `--rooms-check` passerar med kod 0; bilder är granskade. Native bildfångster ligger i exportens egen `artifacts/`. Fogprovet rapporterar den återkommande avslutsvarningen om 16 ObjectDB-instanser / 8 resurser, tidigare noterad i inventarieprovet. Kontrollernas assertions passerar, men avslutet ska inte beskrivas som helt varningsfritt.
