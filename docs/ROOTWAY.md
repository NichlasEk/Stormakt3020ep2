# Rotvägen · 0.9

2026-09-07. Två nya beständiga rum fortsätter direkt från Minnets arkiv. Rutten har nu nio rum, med sparad utforskning, dörrar, fiender/fynd och återväg. Gamla åttastegskampanjen är fortfarande separat.

## Prova

```bash
./start.sh -- --rooms
```

Fortsätt din rumssparning. Vid arkivets säkrade grind: ett nytt E/B-tryck går ut på Rotvägen. Har du inte avslutat kontrollstriden, gör det och säkra grinden först. Äldre sparningar får de nya rummen utan omstart.

| Rum | Innehåll |
|---|---|
| Rötternas trappa | Lugn upptäckt under stora askrötter. Lossa motviktens spärr med E/B och följ kedjan till östra valvet. |
| De namnlösas lund | Jämför arkivets handling med minnesstenen. E/B börjar avtrycket och kallar fram eftertruppen. Säkra området och ta vittnessigillet vid stenen. |

Arkivvalet styr mötet. Vittnesmålets namn passar stenens märken: en eftertrupp med pikenerare och skytt hinner fram. Med en falsk passersedel behöver Hedvig tyda stenen från början: efter första eftertruppen kommer två vakter från andra sidan. Skillnaden berättas vid ankomst och visas före E/B vid stenen. Andra vågen utlöses automatiskt när första är besegrad och farliga projektiler är borta.

Befintliga målade fiender används. Detta är ett möte med förstärkningar; ingen ny unik boss eller ny gånganimation ingår i 0.9. Minnesstenen blockerar rörelse och skott och ritas framför figurer bakom den. Avtrycket är berättelsens motiv till striden, inte ett separat objekt med egna livpoäng.

Efter striden ger E/B **Nornans vittnessigill** (+4 rustning, +6 återhämtning), med befintlig föremålsbild. Belöningen ligger kvar vid full väska. Återbesök ger inga nya vågor eller dubbla sigill. Samma Karl, hälsa, tinkturer, utrustning, stash och kvarlämnade fynd följer alla dörrar. R visar nio rum och aktuell ledtråd.

Slutpunkten är lundens återfunna namn och sigill. Avtrycket pekar mot berget; den fortsättningen är ännu inte byggd. Passagen till höger i lundmålningen är raserad.

## Bilder och röster

Två nya bilder är skapade med det inbyggda bildverktyget och kopierade oförändrade:

- `assets/art/room-rootway-v1.png`
- `assets/art/room-grove-v1.png`

Fullständiga promptar, källfiler och SHA-256 finns i `assets/source/rooms/rootway-art-v1.json`. Geometrin är mätt från bilderna. Skala och läsbarhet kontrollerades med Karl i spelkameran. Nyckelmekanismens öppet/låst-status och passage är implementerade; rörligt portblad och vevanimation hör till senare detaljpass.

Fem svenska repliker med Ebba och Hedvig producerades lokalt via EutherLink/dots.tts-mf med åtta steg och befintliga syntetiska röstidentiteter. Manus/undertexter: `assets/story/rootway-radio.json`. WAV-källor, förfrågningar och genereringsjobb ligger i `assets/source/voices/roots-*`; sammanställning med runtimehashar/längder i `rootway-radio-v1.json`. Befintliga porträtt, underjordsmiljö, portljud och musik återanvänds.

CPU Whisper kontrollerade alla fem klipp. Slutrepliken togs om med ”fyndet” efter osäkert uttal av ”sigillet”. Rapport: `assets/source/voices/rootway-radio-qa.json`. Kvarvarande små transkriptionsavvikelser, exempelvis ”märkerna” och ”företrädet”, behöver mänsklig provlyssning för att skilja uttalsfel från transkriptionsfel. Automatisk granskning är inte en garanti för naturligt tal eller rätt spelmix.

## Verifiering

- Bygge: 0 fel och 0 varningar.
- **29 558 regelassertions** passerar. Åtta kombinationer av vapen, understöd och arkivbeslut når slutet utan utvecklarskydd; 51–100 liv återstår i regelproven. De omfattar version 4-sparning, portkrav, båda vågförloppen, sparning under strid, full väska, återväg och unik belöning.
- Godot `--rootway-check` passerar via tidigare boss/arkiv och båda nya grenarna. Målningar, motvikt, strid, sparad belöning, fem röster/undertexter och nio rum i journalen kontrolleras. Bilderna från motvikten, lunden och journalen är visuellt granskade.
- Linux-export: `dist/AtlandsArv-0.9-rootway/AtlandsArv.x86_64`. Samma kontroll passerar på exporten, avslutningskod 0; slutlogg `artifacts/rootway-native-final.log`, bilder under exportens `artifacts/`.

Editorprovet avslutade med kod 0 men rapporterade de tidigare intermittenta ObjectDB-/resursvarningarna vid avstängning (20 instanser, 10 resurser). Slutkontrollen på den fristående Linux-exporten avslutade utan dessa resursvarningar. Spelkänsla och svårighetsgrad behöver mänsklig provspelning.

Layoutversion 5 migrerar 4→5 genom att lägga till `roots` och `grove`. Tidigare migrationskedja finns kvar. `ArchiveSecured` öppnar Rotvägen, `RootGateOpen` öppnar lunden, `GroveWave` sparar förstärkningarna och `GroveSecured` förhindrar omstart/belöningsduplicering. Rumssnapshots fortsätter bära fiender/fynd; övergången använder inte den gamla kampanjens återställande `EnterCampaign`.
