# Stormakt 3020: Atlands arv

Episod II. Ett nytt, fristående actionrollspel i Stormakts värld: nordisk barock, karoliner, mytologi och Rudbecks Atland.

## Spela första landstigningen

Från projektmappen:

```bash
./start.sh
```

`F11` växlar helskärm. Välj **Gå i land**, välj artilleri eller fältvård och börja landstigningen.

Det första spelprovet innehåller en kaj, två förstörbara sigill, fyra vakter med tre beteenden, varvets indrivare och ett avslutande kartfynd. Det är ett kort stridsprov som sätter grunden för den längre Blekinge-banan. Den fullständiga kampanjen, inventarie/loot-systemet, Varvsänkan och Atland byggs vidare senare.

| Handling | Tangentbord/mus | Handkontroll |
|---|---|---|
| Gå | WASD / pilar | Vänster spak |
| Sikta | Mus | Höger spak, annars gångriktning |
| Hugg | Vänsterklick / J | X |
| Tung attack | Högerklick / K | Y |
| Undanmanöver | Space | A |
| Gard / tajmad parad | Shift | LB |
| Byt sabel/hammare | Tab | RB |
| Tinktur | Q | Styrkors upp |
| Understöd | F | Styrkors ned |
| Undersök | E | B |
| Paus | Esc | Start |
| Manuell sparning/laddning | F5 / F9 | Sparning i pausmenyn |

En tajmad parad skickar tillbaka skyttens kula. Hammaren och tunga attacker bryter pikenerarens gard. Artilleriet träffar framför Karl i siktriktningen. Fältvård ger två extra tinkturer och återkommande läkning.

Kontrollpunkter sparas vid landstigning, före indrivaren och efter segern. F5 skriver ett separat manuellt läge, F9 laddar det. Dödsmenyn återgår till kontrollpunkten. Varje sparfil har checksumma, atomisk skrivning och en föregående säkerhetskopia. Sparfiler ligger i Godots användarkatalog för spelet, helt separat från originalet.

## Utveckling

Det senaste figur- och materialprovet öppnas med `./start-art-study.sh --fullscreen`. Mellanslag växlar mellan beredskap, upptakt och hugg; Esc avslutar. Det visar nya figurer med originalets identitet i spelets kameraskala. Detta är ett bildprov, inte färdiga riktningsanimationer. Den mattare kajen används även i den vanliga spelprototypen; dess tidigare figuranimationer är fortsatt provisoriska.

Godot **4.7.2 .NET**, .NET 8 SDK eller senare, Linux x64. På Arch: `godot-mono`, `dotnet-sdk`. Versionerna är låsta i projektet och exportmallarna ska matcha.

```bash
dotnet run --project tests/Atland.Tests.csproj
./start.sh -- --smoke
./start.sh --fullscreen -- --integration
```

`--smoke` sparar en bild efter två sekunders provkörning. `--integration` kör hela mötet med en testspelare genom vanliga stridsregler och fångar boss/slutbilder. Testlägen skriver inga användarsparfiler. Bilder/loggar hamnar i `artifacts/`.

- `scripts/Combat.cs`: stridsregler utan Godot-beroende.
- `scripts/Main.cs`: presentation, kontroller, menyer och radiokort.
- `scripts/SaveStore.cs`: sparformat och återhämtning.
- `scripts/Soundscape.cs`: musikövergångar, röster och effekter.
- `docs/VISION.md`: överenskommen kreativ riktning.
- `docs/PROGRESS.md`: aktuell checkpoint och nästa steg.
- `tools/`: lokal, reproducerbar assetproduktion.

Stora bilder, ljud och fontfiler använder Git LFS. Efter kloning: `git lfs pull`.

Originalet i `/home/nichlas/WaylandForge` är referensmaterial och ändras inte här. GitHub: https://github.com/NichlasEk/Stormakt3020ep2
