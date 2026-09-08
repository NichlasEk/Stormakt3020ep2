# Arkivets och Rotvägens portar · 0.12.2

2026-09-08. Nästa avgränsade leverans i det godkända portpasset. Arkivets högra dörr leder till Rotvägens vänstra öppning. Rotvägens östra motviktsport leder till lundens befintliga vänstra valv. Karl går hela sträckan; återbesök använder samma geometri och sparade dörrar.

## Provspelning

Starta `./play.sh` och fortsätt den sammanhängande expeditionen. I arkivet: välj handling och besegra kontrollen som tidigare, säkra sigillet vid högra dörren och gå genom porten. På Rotvägen: lossa spärren vid vinschen och gå genom östra valvet. E öppnar/stänger de frigjorda dörrbladen. Dessa berättelseportar är inte sönderslagbara; nyckelportens befintliga förstörbarhet finns kvar.

De nya öppna bildvarianterna låter spelet rita ett enda fysiskt dörrblad. Karmar och rötter ritas i förgrunden med originalbildens koordinater, så Karl kan skymmas under valven. Anslutningsgolven ligger bakom rumsmålningarna. Inga utgångspilar har återinförts.

Sigillet har företräde framför dörrhandtaget tills arkivet är säkrat. Siktkontrollen för en stängd dörr tittar nu på golvet framför och bakom bladet; tidigare kunde dörren själv blockera siktprovet och därmed försvinna ur bilden.

## Sparning och kollision

Anslutningsrevision 3 migrerar sparningar från både 0.12 och 0.12.1. Karl, fiender och fynd på borttagen gångyta återförs till golvet i sitt rum. Rummens världspositioner är oförändrade. Hälsa, utrustning och berättelsebeslut bevaras. Utforskning rensas bara för de flyttade passagerna; galleriets utforskning behålls vid uppgradering från revision 2.

Stängda portar blockerar närmande från båda håll, även förskjutet mot kanterna. Fiender kan följa genom öppna portar. Ett upptaget dörrsvep stoppas fortfarande av en kropp: spelaren kan behöva backa eller få undan en fiende innan bladet kan röra sig.

## Assets och kontroller

Öppna bakgrundsvarianter producerade med det inbyggda image_gen-verktyget:

- `assets/art/room-archive-open-v1.png`
- `assets/art/room-rootway-open-v1.png`

Fullständiga redigeringsprompter finns bredvid bilderna i motsvarande `.prompt.md`. Originalbilderna är bevarade. Tabellens förgrund använder samma bildvariant som arkivets bakgrund.

Full testsvit: **41 486 kontroller**. Riktad världssvit: **8 329**. Täcker gång åt båda håll, blockerade och öppna dörrar, fiender från båda håll, prioritering av sigill, gammal sparposition, fynd och bevarad utforskning. Native `--root-arch-check` tar åtta bildprov: stängt, öppet, under valvet och ankomst för båda paren. `--arch-check` bevakar även det tidigare godkända galleriparet.

Linux-exporten `dist/AtlandsArv-0.12.2-root-arches` är byggd och verifierad med båda native-kontrollerna. `play.sh` pekar på den. Hela exportmappen, inklusive `data_AtlandsArv_linuxbsd_x86_64`, måste behållas tillsammans. .NET-build: inga varningar eller fel.

## Fortsättning

Tre av nio förbindelser är nu förankrade i målade öppningar. Förgårdens och logementets anslutningar, vattenslingan, pumphus–galleri och kammare–arkiv återstår. Mellangångarnas återanvända stenmaterial är fortfarande enklare än rumsmålningarna och behöver ett eget miljöpass; denna leverans är inte färdig målning av hela sammanhängande kartan.

Huvudrutten slutar fortfarande i lunden. Regementets fem platser, Rotmarskalken, nya röster/porträtt och båtfärden byggs enligt [NEXT-EXPEDITION.md](NEXT-EXPEDITION.md).
