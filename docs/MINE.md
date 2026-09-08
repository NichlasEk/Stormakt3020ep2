# Järnets lungor · första gruvdelen · 0.14

2026-09-08. Huvudexpeditionen fortsätter från regementets båtfärd genom tre nya målade platser. Den här leveransen slutar vid gjutformen i svalgången; Kronfogdens gjuteri och bossmöte är nästa byggsteg.

## Prova

`./play.sh` fortsätter den vanliga expeditionen. Vid Farleden vid berget: gå uppför trappan, lossa spärren med E och gå genom gruvporten. Båten finns kvar för återresa till regementet.

Direktprov: `./play.sh -- --mine`. Inställningar har även **Prova gruvan · separat sparning**. Provet börjar vid bergets kaj efter ett avlöst regemente och använder `mine-preview-save.json` samt egen manuell sparplats. `--mine-new` börjar om endast gruvprovet. Gamla regementes- och huvudsparningar migrerar från 15 till 18 platser; hälsa, utrustning, fynd och tidigare berättelseval behålls.

## Innehåll

- **Gruvmynningen:** vakter, driftbok och förbindelse tillbaka till kajen. Boken förklarar trycksystemet och antyder vad kronans verk förbrukar.
- **Järnets lungor:** stora blåsbälgar, vakter och tre fasta ångutlopp. Utbrotten vandrar mellan utloppen med 1,1 sekunders varning. Ångan skadar Karl och vakterna. Stäng matningen till vänster för längre pauser; öppna avlastningen till höger för att stoppa ångan och frigöra vägen. Omvänd ordning ger en förklaring utan att nollställa något.
- **Svalgången:** kall vattenarkitektur, en frivillig förrådskista, kronans gjutform och ett stjärnspår som knyter an till arkivet. Underhållstrappan öppnar en beständig genväg till gruvmynningen.

Alla fyra nya gångförbindelser går att färdas genom åt båda håll, inklusive genvägen. Ingen kartteleport används. Den större världskartan skiljer sidovägar från huvudlinjen och visar båtresan separat.

## Assets

Tre nya rumsbakgrunder och två kompletterande assets producerades med det inbyggda **imagegen**-verktyget. Exakta promptar ligger bredvid bilderna som `.prompt.md`:

| Sökväg under `assets/art/` | Användning |
| --- | --- |
| `room-mine-mouth-v1.png` | Gruvmynningen |
| `room-mine-bellows-v1.png` | Blåsbälgarnas sal med inmätta golvutlopp |
| `room-mine-coolway-v1.png` | Svalgången och gjutformen |
| `room-farled-open-v1.png` | Öppen variant av befintlig gruvport; originalet bevarat |
| `mine-valve-v1.png` | Återanvändbar ventil med riktig alpha |

Åtta nya Ebba-/Hedvig-repliker: `assets/story/mine-radio.json` och `assets/audio/voice-mine-*.ogg`. Lokalt Dots-TTS, befintliga syntetiska röstidentiteter, meningsvis produktion för att undvika avklippta slut. Slutversionernas sammansättning och hash finns i `assets/source/voices/mine-*-complete.manifest.json`. Lokal Whisper har transkriberat hela serien; namn/uttal behöver fortsatt mänsklig lyssning.

Tre nya originaleffekter – ventil, tryckvarning, ångutbrott – skapas deterministiskt av `tools/build_mine_audio.py`. Källor och manifest i `assets/source/mine-audio/`. Ångan har en egen ljus markvarning och en kort synlig plym. Befintlig musik, underjordiskt bakgrundsljud och vakternas 2D-figurer används fortsatt.

## Verifiering och kvarstående arbete

Hela motortestsviten: **69 516 assertions**. Bygge utan varningar/fel. Exporterad Linux-binär har passerat gruvprovet och regementets regressionsprov.

`MineTests` provar sparmigrering, stängd ingång, ventilordning, skada på både Karl/vakter, beständig avlastning, båda riktningar, genväg, ledtråd/belöningsskydd och båtfärd efter gruvbesöket. Separata stridsprov täcker båda rummen med båda vapnen och understöden, utan devöverlevnad.

Native `--mine-check` granskar gruvporten stängd/öppen, trösklar, rumsankomst, driftbok, ångvarning, ventilinteraktion, gjutform, karta och samtliga åtta ljud-/textpar. Bildprov finns lokalt i `artifacts/mine-*.png`.

Gruvmynningen och maskinhallen delar byggnadsspråk; svalgången byter till kall sten och vatten. Mellangångarnas enkla murmaterial, helt egna gruvfiender, musik och gånganimationernas detaljpass återstår. Kronfogden är ännu inte inkopplad i den sammanhängande huvudrutten.

Export: `dist/AtlandsArv-0.14-mine/`. Behåll programfil, PCK och .NET-datakatalog tillsammans.
