# Minnets arkiv · 0.8

2026-09-07. Sjunde beständiga rummet i Atlands port, efter Edsväktaren. Den separata åttastegskampanjen är oförändrad; den nya rumsrutten slutar vid arkivets säkrade grind.

## Prova i kväll

```bash
./start.sh -- --rooms
```

Fortsätt din rumssparning. Besegra Edsväktaren om han lever, öppna bossporten med E/B och tryck sedan E/B igen vid porten för att gå ned. Gamla avslutade sexrumssparningar får det nya arkivet utan omstart.

E/B vid läsbordet öppnar de två handlingarna. Striden pausas medan du läser; Esc stänger utan beslut, och bordet kan öppnas igen. Klicka ett beslut eller välj med menykontrollerna. Följden står vid respektive knapp:

| Beslut | Faktiskt möte |
|---|---|
| Bevara vittnesmålet | Vakt, pikenerare och skytt |
| Förfalska en passersedel | En ensam vakt som kontrollant |

Beslutet sparas direkt och kan inte väljas om. Besegra kontrollen och använd E/B vid inre grinden. Därefter kan du återvända genom de tidigare rummen och hämta kvarlämnade fynd. Ingen läkning, nya tinkturer eller återställda fiender vid dörrarna. Full väska hindrar inte dokumentvalet. R visar kartan över sju rum och ditt beslut. I öppnar inventariet, C stats, volymreglaget finns kvar.

Färdig Linux-export: `dist/AtlandsArv-0.8-archive/AtlandsArv.x86_64 -- --rooms`.

## Bild och ljud

Två nya bitmapbilder är genererade med det inbyggda bildverktyget, kopierade oförändrade: `assets/art/room-archive-v1.png` och `assets/art/archive-documents-v1.png`. Fullständiga instruktioner, källsökvägar och SHA-256 finns i `assets/source/rooms/archive-art-v1.json`. Golvpolygon, bordskollision, förgrund och dörrpunkter är mätta från resultatet. Texten läggs på i spelet och förblir läsbar. Karl och fienderna behåller sina målade 2D-figurer.

Fyra nya svenska radiorepliker använder befintliga syntetiska Ebba-/Hedvig-referenser, lokalt EutherLink/dots.tts-mf med åtta steg. Manus och undertexter: `assets/story/archive-radio.json`. WAV-källor, genereringsjobb, förfrågningar, runtimehashar och längder: `assets/source/voices/archive-radio-v1.json` och motsvarande filer bredvid. Tidigare lågmälda underjordsljud och upptäcktsmusik återanvänds.

CPU Whisper användes för automatisk uttalsgranskning, inte som ersättning för mänsklig provlyssning. Två repliker togs om: ett osäkert uttal av passersedel och trettiosju ersattes med enklare formuleringar. Dokumentet anger fortfarande exakt trettiosju. Slutkontrollen återger hela läsrepliken korrekt; övriga kvarvarande avvikelser är bland annat ”padrullen”, ”kontroll anstannar” och ett utelämnat ”att”. Det kan vara uttal eller transkription. Rapporten finns i `assets/source/voices/archive-radio-qa.json`. Mix och röster behöver fortfarande provlyssnas i spel.

## Beständighet och verifiering

Layoutversion 4 lägger till ett obesökt arkiv i version 3-sparningar; kedjan från version 1 och 2 är kvar. `Combat.ArchiveChoice` är beslutets enda källa. `ArchiveRead` och `ArchiveSecured` samt rummets fiender/fynd/utforskning sparas. Besök kräver öppnad bossport, beslut kräver läsning, och säkrad grind kräver besegrad kontroll. Befintlig hälsa, utrustning och stash bevaras.

- `dotnet build AtlandsArv.csproj --nologo --verbosity quiet`: 0 fel, 0 varningar.
- `dotnet run --project tests/Atland.Tests.csproj`: **29 442 assertions**. Arkivrutten provas med båda vapnen, båda understöden och båda besluten utan utvecklarskydd. Hälsa efter rutterna: 61–100. Äldre sparning, full väska, återbesök före/efter beslut, fiendeidentiteter och kvarlämnat cisternfynd ingår.
- Godot `--archive-check`: verklig E-interaktion, dokumentknappar, paus, Esc/återöppning, båda kontrollstriderna, sparning/laddning, fyra spelbara röstklipp och journal med sju rum. Dokumentvyn, läsbordet och journalen har bildgranskats.
- Fristående Linux-export kör samma kontroll, inklusive föregående sex rum och Edsväktaren. Slutlogg: `artifacts/archive-native-final.log`; skärmbilder/sparprov under exportens `artifacts/`.

Regelproven är automatiserade pilotkörningar; de bevisar inte att svårighetsgrad, gånganimation eller tempo känns bra för en människa. Rotvägen bortom arkivet är nästa byggsteg. Anslutningsgränserna finns i `ARCHIVE-CONNECTION.md`.
