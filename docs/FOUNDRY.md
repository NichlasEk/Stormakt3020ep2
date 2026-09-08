# 0.15 · Den tomma kronans gjuteri

2026-09-08. Huvudexpeditionens nittonde beständiga plats fortsätter genom svalgångens målade port. Läs kronans gjutform, lossa porten med E och gå in. Samma gång fungerar tillbaka; gamla gruvsparningar får det nya rummet utan att börja om.

## Prova

`./play.sh` fortsätter den vanliga expeditionen. `./play.sh -- --foundry` öppnar ett separat gjuteriprov vid svalgångens port; `--foundry-new` börjar om provet. Provet finns även i inställningarnas utvecklarval. Det har en egen sparplats och påverkar inte huvudexpeditionen.

Kronfogden har 880 liv och låser hammarslagen längs en synlig linje. Kliv åt sidan; vid halva livet kommer fyra nedslag i stället för tre. Slå under återhämtningen, parera när hammaren når fram eller använd kylvattenventilen: fyra sekunders avbrott med tolv sekunders återladdning. Nedslagen upphör när Kronfogden faller. Bossen återuppstår inte vid återbesök.

Undersök därefter stjärnplattan. Karl får kronhjälmen och Hedvig tolkar den felvända himlen över Uppsala. Återvägen till båten finns; själva Uppsalaresan och området är nästa arbetsmål och är ännu inte spelbara.

## Bild och ljud

Tre nya 2D-bilder framställdes med Codex inbyggda imagegen-verktyg. Slutbilder och exakta produktionsprompter:

- [Gjuteriet](../assets/art/room-mine-foundry-v1.png) · [prompt](../assets/art/room-mine-foundry-v1.prompt.md)
- [Öppnad svalgångsport](../assets/art/room-coolway-open-v1.png) · [prompt](../assets/art/room-coolway-open-v1.prompt.md)
- [Kronfogdens sex poser och porträttkälla](../assets/art/crown-bailiff-v1.png) · [prompt och korrigering](../assets/art/crown-bailiff-v1.prompt.md)

Åtta nya svenska repliker med Ebba, Hedvig och Kronfogden finns i [foundry-radio.json](../assets/story/foundry-radio.json). Kronfogden har en egen syntetisk röstreferens; replikerna skapades lokalt med Dots-TTS och VoxCPM2. Källjud, parametrar och manifest finns i `assets/source/voices/foundry-*` samt `bailiff-reference.*`. Textavstämning med Whisper täcker alla repliker. Befintlig bossmusik, ånga, portljud och tungt slagljud återanvänds.

## Sparning och verifiering

Rumslayout 8 migrerar publicerade 18-rumssparningar till 19 rum. Anslutningsrevision 5 återför äldre positioner från den nytätade målade väggen till svalgångens golv. Port, bossliv, fas, kylvatten, belöning och återbesök sparas. Portens karmskarvar testas över hela öppningen.

Tester kör bossen med båda vapnen, båda understöden och med respektive utan kylvatten, utan utvecklarosårbarhet. Rumsprov täcker vandring åt båda håll, sparning mitt i strid, kylventilens återladdning och skydd mot dubbla belöningar. Native-provet spelar striden genom samma inmatning och kontrollerar bilder, porträtt, ljudresurser och avslut.

Verifierat: hela sviten passerar **70 479 kontroller**, bygget har noll varningar/fel och den färdiga Linux-exportens `--foundry-check` och `--mine-check` avslutas med kod 0. Exportens bilder av hammarns varningslinje och slutrepliken är visuellt granskade.

Linux-exporten är `dist/AtlandsArv-0.15-foundry/`. Hela mappen behövs, inklusive `.pck` och .NET-katalogen.

## Kvar att förbättra

Kronfogden har sex målade poser och en enkel tvåbildsgång; fullständiga gång- och riktningskort återstår. Mellangångens material behöver fortfarande ett detaljpass. Automatisk stridsverifiering ersätter inte mänsklig balansbedömning: perfekt parering kan göra kampen betydligt lättare. Inga nya mellanfilmer ingår i denna checkpoint.

Nästa sammanhängande kapitel: Uppsala, med ankomst från båten, flera rum kring den felvända himlens instrument och nya berättelseval. Karl CCLV sparas till en senare resa där skeppet behövs.
