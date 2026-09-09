# Övre observatoriet — Den strukna destinationen

0.19 fortsätter huvudrutten efter Ebbas genomgång i rymdfregattens kajuta. Fyra nya målade rum gör att expeditionen nu har 27 beständiga platser. Originalspelet och de två Rudbeck-PDF:erna är orörda.

## Spela

Starta `./play.sh` och fortsätt din sparning. Tala färdigt med Ebba om ordern, kartan, gjutformen och stjärnplåten. Gå sedan tillbaka till Meridiansalen och använd **E/B vid högra porten**. Äldre sparningar får de nya rummen utan att inventarium, avlösning, resor eller kajutsamtal nollställs.

För att prova enbart fortsättningen:

```sh
./play.sh -- --observatory
```

Det använder `observatory-preview-save.json` och en separat manuell sparning. `--observatory-new` börjar om just detta prov. Vanlig Fortsätt och äldre kapitelprov behåller sina egna sparfiler.

## Spelvägen

1. **Astronomernas bostäder:** slå tillbaka vakterna och tala med Märta Vinge vid skrivbordet. Hon är levande och hennes syster Elin saknas.
2. **Sönderslagna verkstaden:** fienderna reglar återvägen. Öppna dörren igen eller slå sönder den. Läs diagrammet vid det bakre skrivbordet. Säkra rummet och undersök gömman för en hammare och verkstadsnyckeln.
3. **Kupolens motvikter:** undvik vikternas varnade nedslag. Ställ vänster broms på **II**, den främre på **0**, höger på **I**. Nyckeln öppnar den bakre underhållstrappan tillbaka till bostäderna; den vanliga vägen fungerar också.
4. **Övre observatoriet:** undersök drivverket. Slå sönder de tre låsningarna innan Zenitväktaren kan skadas. Undvik svepens markerade nedslag; under halva livet kommer ett extra efterslag. Angrip mitten när den är oskyddad.
5. Efter kupolfilmen: läs originalet vid högra skrivbordet och gå tillbaka till Ebba ombord. Samtalet avslutar kapitlet. Gamla Uppsala är nästa mål, **ännu ingen spelbar destination**.

Utforskning, dörrars tillstånd, brutna låsningar, bromsar och samtal sparas. Belöningar och filmutlösning upprepas inte av att man går tillbaka.

## Grafik, röster och film

Bilderna är genererade 2D-rasterassets med fast kamera och mörkt målad yta. Ingen ny Blender-figur eller 3D-scen används. Runtime ritar Zenitverkets separata delar och armar; dörrarnas tjocka blad använder den befintliga fysiska dörrlösningen.

| Filer i assets | Användning och läge |
| --- | --- |
| `art/room-observatory-{quarters,workshop,machine,dome}-v1.png` | Fyra nya bakgrunder, generate; bostäder och motvikter har dessutom edit-pass för synliga trappor. |
| `art/room-meridian-observatory-open-v1.png` | Edit av tidigare Meridiansal, högra porten öppnad. |
| `art/marta-vinge-v1.png` | Generate, allvarlig vuxen instrumentassistent; samma bild ger kropp och radioporträtt. Grön bakgrund tas bort vid laddning. |
| `art/zenith-guardian-v1.png` | Generate, transparent 2×2-atlas: kärna, sveparm, låsning och vrak. |
| `audio/voice-observatory-*.ogg` | 18 svenska klipp, lokal EutherLink/Dots med syntetiska Ebba- och Märta-referenser. |
| `video/observatory-dome-v1.ogv` | Drygt åtta sekunders lokal LTX-2-film, Theora/Vorbis med dämpat mekanikljud. Även i Inställningar → videobibliotek. |

Exakta bildprompter ligger i motsvarande `.prompt.md`. Lokala röstförfrågningar, syntetiska referenser, filmworkflow och råfilm finns under `assets/source/`. `tools/render_observatory.py` och `tools/assemble_observatory.py` återskapar filmproduktionen. Musiken och grundstridsljuden återanvänder spelets befintliga bibliotek.

## Verifiering

- Hela simulationssviten passerade 117 198 kontroller efter kapitelintegrationen.
- Det avslutande utökade kapitelprovet passerade **39 004 kontroller** och går med riktiga rörelse- och stridsinmatningar genom varje ny passage, båda riktningar på genvägen, bossen med båda vapnen och båda ordnarna, sparning/laddning och återkomsten till Ebba. Där ingår även att slå sönder verkstadsdörren från andra sidan.
- Native `--observatory-check` laddar de fyra målningarna, karaktärerna och alla 18 klipp, sparar skärmbilder och kontrollerar att nya filmen avkodas utan att ändra spelstatus.
- Komplett Linux-export i `dist/AtlandsArv-0.19-observatory/` har också passerat nativekontrollen med exporterad grafik, alla röster och filmavkodning. `play.sh` pekar på denna version; behåll exe, PCK och hela data-katalogen tillsammans.
- CPU-Whisper används som uttalskontroll. Fyra repliker kortades och spelades om efter första transkriptionen. Maskinell transkription ersätter inte mänsklig lyssning.

Automatiserade stridsprov visar att kapitlet går att klara; mänsklig provspelning återstår för tempo, svårighet och hur naturliga passagerna känns. Märtas kropp är ännu en stillastående målad pose. Mellangångarna använder samma återanvända murmaterial som tidigare kapitel.

Berättelsens kanon, öppna frågor och alla repliker finns i [STORYBOARD.md](STORYBOARD.md).
