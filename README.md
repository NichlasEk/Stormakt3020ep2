# Stormakt 3020: Atlands arv

Episod II. Ett nytt, fristående actionrollspel i Stormakts värld: nordisk barock, karoliner, mytologi och Rudbecks Atland.

## Spela första landstigningen

Från projektmappen:

```bash
./start.sh
```

`F11` växlar helskärm. Välj **Gå i land**, välj artilleri eller fältvård och börja landstigningen.

Spelprov 0.2, **De strukna namnen**: bryt kajens sigill, besegra indrivaren och undersök bronskartan. Frilägg därefter tre inskrifter medan nya patruller försöker stoppa dig. Jämför stenarna med kronans liggare, välj öppen sändning eller krypterad rapport och kämpa tillbaka till båten. Vägarna ger olika motstånd och fördelar under återtåget. Ebba Grip och antikvarien Hedvig Rålamb följer fynden över radion, med egna röster och målade porträtt. Detta är fortfarande ett kompakt spelprov som sätter grunden för den längre Blekinge-banan. Den fullständiga kampanjen, inventarie/loot-systemet, Varvsänkan och Atland byggs vidare senare.

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
| Undersök / frilägg inskrift | E / håll E stillastående | B / håll B |
| Läs fynd i fältdagboken | R | Back / Select |
| Ljudvolym | Synligt reglage eller +/−; M tyst/ljud | Inställningar i pausmenyn |
| Paus | Esc | Start |
| Manuell sparning/laddning | F5 / F9 | Sparning i pausmenyn |

En tajmad parad skickar tillbaka skyttens kula. Hammaren och tunga attacker bryter pikenerarens gard. Artilleriet träffar framför Karl i siktriktningen. Fältvård ger två extra tinkturer och återkommande läkning.

Startvolymen är 25 %. Reglaget går att dra direkt under platsnamnet; M återställer tidigare ljudnivå efter tyst läge. Volymen sparas mellan starter.

Kontrollpunkter sparas också vid inskrifter, vägval och ombordstigning. Delvis frilagda inskrifter behåller arbetet när du släpper knappen eller sparar manuellt. Befintliga avslutade 0.1-sparningar förblir avslutade; välj **Ny landstigning** för det nya uppdraget. F5 skriver ett separat manuellt läge, F9 laddar det. Dödsmenyn återgår till kontrollpunkten. Varje sparfil har checksumma, atomisk skrivning och en föregående säkerhetskopia. Sparfiler ligger i Godots användarkatalog för spelet, helt separat från originalet.

## Utveckling

Den godkända seriösa figurstilen används nu i spelet för samtliga stridsroller. `./start-art-study.sh --fullscreen` visar samma figurer och kaj; mellanslag växlar poser, G visar måttreferensen. Alla vuxna använder cirka 150 världspixlars ståhöjd. Kajföremålen har målats om i mindre skala. Figurerna har tre stridsposer och speglad sidriktning; fulla gångcykler och åtta riktningar är ännu inte klara.

Godot **4.7.2 .NET**, .NET 8 SDK eller senare, Linux x64. På Arch: `godot-mono`, `dotnet-sdk`. Versionerna är låsta i projektet och exportmallarna ska matcha.

```bash
dotnet run --project tests/Atland.Tests.csproj
./start.sh -- --smoke
./start.sh --fullscreen -- --integration
./start.sh -- --ui-check
```

`--smoke` sparar en bild efter två sekunders provkörning. `--integration` kör hela mötet med en testspelare genom vanliga stridsregler och fångar boss/slutbilder. Testkörningarna är tysta och skriver inga användarsparfiler. `--ui-check` provar volym, mute, sparad ljudnivå, klick utan hugg och fältdagboken samt fångar radioporträtten; dess inställningar ligger separat i `artifacts/`. Bilder/loggar hamnar i `artifacts/`.

- `scripts/Combat.cs`: stridsregler utan Godot-beroende.
- `scripts/Main.cs`: presentation, kontroller, menyer och radiokort.
- `scripts/PaintedCast.cs`: gemensam målad figurpresentation och kroppsskala.
- `scripts/FieldNotes.cs`: fyndens originalskrivna vittnesmål.
- `scripts/SaveStore.cs`: sparformat och återhämtning.
- `scripts/Soundscape.cs`: musikövergångar, röster och effekter.
- `docs/VISION.md`: överenskommen kreativ riktning.
- `docs/PROGRESS.md`: aktuell checkpoint och nästa steg.
- `docs/ATLANTICA-NOTES.md`: primärläsning med tryckta sidnummer och spelidéer.
- `tools/`: lokal, reproducerbar assetproduktion.

Stora bilder, ljud och fontfiler använder Git LFS. Efter kloning: `git lfs pull`.

Originalet i `/home/nichlas/WaylandForge` är referensmaterial och ändras inte här. GitHub: https://github.com/NichlasEk/Stormakt3020ep2
