# 0.22 · Musik för varje plats

0.24 lägger till Kungaminnets fyra rum och Sigillmästaren: totalt 57 spår, cirka 67,6 minuter. De nya spåren är loudness-kontrollerade kring −22 LUFS.

0.23 lägger till Saltkällans fyra rum och Saltväktarens strid: totalt 52 spår, cirka 62,6 minuter. Biblioteket är tyst tills ett spår väljs och spelar exklusivt utan bakgrundsmusik.

Musikpasset ersätter den tidigare återanvändningen av tre grundspår med 47 egna lokalt genererade instrumentala stycken, sammanlagt cirka 57 minuter musik:

- 33 rumsteman, ett för varje beständig spelplats.
- 3 teman för prologens kaj, magasin och strand.
- 8 bossstycken: indrivaren, Edsväktaren, Rotmarskalken, Kronfogden, Meridianväktaren, Zenitväktaren, Mönstringsförrättaren och den äldre kampanjens sista mönstring.
- 2 färdteman för båt respektive rymdfregatt.
- 1 stilla avlösningstema för regementets efterspel.

## Spela och lyssna

Starta med `./play.sh`. Musiken väljs automatiskt efter plats. När en boss engageras hålls dess stycke kvar även om bossen hamnar utanför synfältet; efter döden återkommer platsens tema. Oväckta bossar i observatoriet och Meridiansalen tar inte över rummets musik. Färderna har egna teman. Filmer behåller sina befintliga ljudspår och tystar spelmusiken.

I **Inställningar → Utvecklare · Musikbibliotek** kan du välja bland alla 47 spår utan att ändra sparningen. Byt sida med Föregående/Nästa. Esc/B återgår till inställningarna. Den vanliga volymkontrollen gäller även här. Rummets bakgrundsljud är tysta under förlyssningen.

Musiken tonas över mellan två kanaler under cirka 2,4 sekunder. Snabba rumsbyten får slutföra pågående övertoning innan en fortfarande hörbar kanal byts ut. Återbesök återupptar spårets tidigare uppspelningsposition under samma spelsession. Positionerna är inte del av kampanjsparningen.

## Klang och produktion

Allvarlig nordisk historisk fantasy: cello, viola da gamba, nyckelharpsliknande stråkar, träflöjt, luta, cembalo, orgel, bleckblås och återhållna militärtrummor. Varje prompt beskriver rummets funktion och känsla. Musik medvetet utan beställd sång, tal, modern pop eller komisk gestaltning. Musikens faktiska instrumentering kommer från den generativa modellen, inte från separata inspelade instrumentstems.

`assets/story/music.json` innehåller alla titlar, plats-ID, prompter, tempi, tonarter, frön och spelfiler. `assets/source/music/` innehåller förlustfria FLAC-master, genereringsbegäranden, jobbresultat, looprecept och mätningar. Spellooparna ligger i `assets/audio/music/`. Produktionen använder den lokala ACE-Step-modellen via EutherLinks GPU-kö.

Looparna överlappar en hel takt vid skarven. Integrerad ljudnivå korrigeras till cirka −22 LUFS, med marginal till digital klippning. Spelaren sänker musiken ytterligare under tal; huvudvolymen kan tysta allting. Detta är nya loopkompositioner, inte en nyinspelning av introfilmernas redan färdiga ljudmix.

## Återskapa och verifiera

```sh
python tools/generate_music_suite.py
python tools/check_music_suite.py
dotnet run --project tests -- --music-only
godot-mono --path . -- --music-check
```

Generatorn återanvänder färdiga master och sparade jobb-ID. Den initierar inte om en redan laddad modell. Ett misslyckat jobb lämnas för diagnos och skickas inte automatiskt om i en oändlig loop. Den första körningen behövde återställas efter modellens minnesfel; lyckade spår bevarades.

Testerna kontrollerar täckning av alla rum/bosstyper, aktivering och avslut av bossmusik samt färdprioritet. Native-provet öppnar samtliga musikströmmar, testar övertoning, huvudvolym noll, filmtystnad och bibliotekets tillbaka-knapp utan att kampanjtillstånd ändras. Mätningar av varje levererad fil finns i `assets/source/music/quality-report.json`. Automatisk nivåkontroll ersätter inte musikalisk bedömning vid lyssning.

## Spårlista

| Spår | Koppling |
| --- | --- |
| Den dränkta förgården | `court` |
| Väktarnas logement | `lodge` |
| Pumphusets hjärta | `pump` |
| Under vattenlinjen | `cistern` |
| Vittnenas namn | `gallery` |
| Den bundna eden | `chamber` |
| Minnets arkiv | `archive` |
| Rötternas trappa | `roots` |
| De namnlösas lund | `grove` |
| Förläggningsstigen | `regiment-trail` |
| De väntandes sjukbarack | `regiment-barracks` |
| Fanornas rötter | `regiment-flags` |
| Mönstringsvallen | `regiment-parade` |
| Den övergivna bryggan | `regiment-quay` |
| Farleden vid berget | `regiment-farled` |
| Gruvmynningen | `mine-mouth` |
| Blåsbälgarnas gång | `mine-bellows` |
| Svalgången | `mine-coolway` |
| Den tomma kronan | `mine-foundry` |
| Uppsalas felvända himmel | `uppsala-court` |
| Klockgångens morgon | `uppsala-clock` |
| Meridianernas sal | `uppsala-meridian` |
| Ebbas kajuta | `ship-cabin` |
| Astronomernas bostäder | `observatory-quarters` |
| Den sönderslagna verkstaden | `observatory-workshop` |
| Kupolens motvikter | `observatory-machine` |
| Övre observatoriet | `observatory-dome` |
| Kungshögarnas uppställning | `gamla-landing` |
| Gången under högen | `gamla-passage` |
| De överfördas väntrum | `gamla-registry` |
| Den västra kontrollgången | `gamla-west-control` |
| Västra vågen | `gamla-west-hall` |
| Rummet bakom vågen | `gamla-west-refuge` |
| Likvarvet | `prologue-quay` |
| Magasinets skuggor | `prologue-warehouse` |
| De tre vittnenas strand | `prologue-shore` |
| Över den mörka farleden | `travel-boat` |
| Karl CCLV lyfter | `travel-ship` |
| Avlösningen | `remembrance` |
| Varvets indrivare | `boss-collector` |
| Edsväktaren | `boss-oath` |
| Rotmarskalken | `boss-marshal` |
| Kronfogden | `boss-bailiff` |
| Meridianväktaren | `boss-meridian` |
| Zenitväktaren | `boss-zenith` |
| Mönstringsförrättaren | `boss-muster` |
| Kollegiets sista mönstring | `boss-tribunal` |

## Kontrollresultat

47 loopar, 57,38 minuter totalt, stereo 48 kHz. Alla uppmätta nivåer ligger inom 0,3 LUFS från −22 och topparna under −2 dBTP. Hela testsamlingen klarade 117 636 kontroller; det utökade separata musikprovet klarade 98.

Native-kontrollen är godkänd för samtliga 47 strömmar, musikval, övertoning, dialogdämpning, tyst huvudvolym, filmtystnad och bibliotekets återgång utan ändrat kampanjtillstånd. Biblioteksbild: `artifacts/music-library.png`.

Även Linux-exportens native-kontroll är godkänd. `play.sh` startar `dist/AtlandsArv-0.22-soundtrack`, med körfil, PCK och .NET-assembly verifierade tillsammans.
