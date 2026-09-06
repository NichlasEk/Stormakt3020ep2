# Stormakt 3020: Atlands arv

Episod II. Ett nytt, fristående actionrollspel i Stormakts värld: nordisk barock, karoliner, mytologi och Rudbecks Atland.

## Spela första landstigningen

Från projektmappen:

```bash
./start.sh
```

`F11` växlar helskärm. Välj **Gå i land**, välj artilleri eller fältvård och börja landstigningen.

Spelprov 0.3, **Vägen under vattnet**: kajens sigill, indrivare, bronskarta och tre strukna namn följs nu av kronans magasin och en övervuxen strand. Hitta brynstålet, läs kollegiets mätorder, öppna grinden och pröva kartan mot tre riktningar i landskapet. Ebba och Hedvig följer upptäckten över radion. Slutet avslöjar en väg under vattnet — och att någon var här före expeditionen.

Välj **Öva sabelduell** i huvudmenyn för att prova sabel, parad och undanmanöver mot en vakt. Övningen skriver inte över kampanjens sparningar. Det är fortfarande ett kompakt spelprov; den fullständiga kampanjen återstår.

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

Kontrollpunkter sparas vid inskrifter, vägval, nya områden, fynd och mätningar. Delvis frilagda inskrifter behåller arbetet när du släpper knappen eller sparar manuellt. Befintliga avslutade 0.1-sparningar förblir avslutade; välj **Ny landstigning** för det nya uppdraget. Ett avslutat 0.2-läge med tre lästa namn kan fortsätta via **Fortsätt genom magasinet** i slutmenyn. F5 skriver ett separat manuellt läge, F9 laddar det. Dödsmenyn återgår till kontrollpunkten. Varje sparfil har checksumma, atomisk skrivning och en föregående säkerhetskopia. Sparfiler ligger i Godots användarkatalog för spelet, helt separat från originalet.

## Utveckling

Den godkända seriösa figurstilen används nu i spelet för samtliga stridsroller. `./start-art-study.sh --fullscreen` visar samma figurer och kaj; mellanslag växlar poser, G visar måttreferensen. Alla vuxna använder cirka 150 världspixlars ståhöjd. Kajföremålen har målats om i mindre skala. Karl med sabel och sabelvakten har nya gång- och anfallsnyckelbilder i fyra riktningar, samt försvars- och dödsposer. Gången har fortfarande ojämna mellanbilder; specialistrollerna har kvar tre poser och speglad sidriktning. Murar och props skymmer figurer som befinner sig bakom dem. Produktionsmått, korrigeringar och begränsningar finns i `docs/JOURNEY-PRODUCTION.md`.

Godot **4.7.2 .NET**, .NET 8 SDK eller senare, Linux x64. På Arch: `godot-mono`, `dotnet-sdk`. Versionerna är låsta i projektet och exportmallarna ska matcha.

```bash
dotnet run --project tests/Atland.Tests.csproj
./start.sh -- --smoke
./start.sh --fullscreen -- --integration
./start.sh --fullscreen -- --ui-check
./start.sh --fullscreen -- --scene-check
./start.sh -- --duel
```

`--smoke` sparar en bild efter två sekunders provkörning. `--integration` kör hela mötet med en testspelare genom vanliga stridsregler och fångar kaj, magasin, strand och slutbilder. Testkörningarna är tysta och skriver inga användarsparfiler. `--ui-check` provar volym, mute, sparad ljudnivå, klick utan hugg och fältdagboken samt fångar radioporträtten; dess inställningar ligger separat i `artifacts/`. `--scene-check` fångar överlappning bakom last, murar och sten samt avslöjandets kamerabild. Bilder/loggar hamnar i `artifacts/`.

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
