# 0.23 · Saltkällan

## Spelväg och prov

1. Efter Elins avrapportering: tala med Ebba igen för återföreningen. Märta finns nu ombord.
2. Återvänd till rummet bakom Västra vågen och gå genom dess högra port.
3. Säkra lastplatsen och läs manifestet vid skrivbordet. Följ högra porten till Salttrappan.
4. Säkra trappan och stäng tillförseln vid hjulet. Läs instruktionen i nästa rums register. Kistan vid vänstra sidan är valfri.
5. Besegra Saltväktaren. Kedjespelen avlastar vattnet; mittgången är torr. Nedslagen markeras innan kedjan träffar. Han blir rörlig vid halva livet.
6. Använd pulpeten efter striden för att frigöra vittnena. Underhållsgången bakom pulpeten leder tillbaka till lastplatsen. Gå tillbaka till fregatten och avrapportera till Ebba.

`./play.sh -- --salt` startar ett separat prov ombord före återföreningen. `--salt-new` återställer bara detta prov; automatisk och manuell provsparning är separata. Huvudkampanjens gamla 0.22-sparningar migreras från layout 14 till 15 med orörda äldre rum och framsteg.

## Presentation och begränsningar

Fyra målningar, en öppnad version av räddningsrummet och Saltväktarens strids-/gångkort är genererade 2D-assets. Karaktärerna använder ingen Blender-modell. Markens vattenrisk har mjuka ritade kanter som följer samma polygoner som skadekontrollen. Bossen har fyra gångbilder i en vy; han har ännu inte ett fullständigt gångkort för alla riktningar.

21 nya svenska repliker använder Ebbas, Märtas och Elins tidigare syntetiska röstreferenser och en ny Saltväktare. Rösterna producerades lokalt med Dots/VoxCPM2, och originalbegäranden samt tagningar bevaras. Automatisk Whisper-transkription är granskad; mänsklig lyssning under spel återstår. Kritiska repliker fick alternativa VoxCPM2-tagningar i `tools/retake_salt_voices.py`.

Fem egna lokala ACE-Step-spår är nivåkontrollerade till cirka −22 LUFS. Filmen är drygt åtta sekunder lokal LTX-2-video med låg vatten-/kopparklang och finns även i utvecklarnas videobibliotek. Den visas bara första gången källsalen nås, kan hoppas över och ändrar inte sparningen vid förhandsvisning.

Vittnenas evakuering berättas via ledningen och avrapporteringen; det finns ännu ingen animerad grupp civila som går genom varje rum. Systrarna är synliga ombord. Nästa etapp med spelbar Ebba beskrivs i storyboarden och är inte spelbar i denna version.

## Verifiering

168 riktade kontroller täcker båda ordervalen, båda vapnen, migration från publicerad sparning, vatten/skadefri mittgång, alla nya portaler, sparning mitt i övergång, bossens faser, beständig återväg och belöningar som inte kan dupliceras. Hela testsamlingen godkändes med 117822 kontroller. Native-provet kontrollerar rumsbilder, systrarna ombord, bossens förvarning, 21 röstkort, musik samt filmens avkodning och tillbaka-funktion. Bildfångster finns under `artifacts/salt-*.png`.

## Scenmanus och avsedd bakgrund

Efter Elins avrapportering har Ebbas skyttel hämtat Märta från observatoriet. Systrarna möts i kajutan. Återföreningen ger ingen ny sakkunskap till Elin; hon vet fortfarande bara att de andra fördes nedåt. Ebba pekar ut porten bakom vågen på anläggningens plan.

Fyra rum följer: Saltkällans lastplats, Salttrappan, Det dränkta registret och Saltkällans minnesverk. Sidodörrarna leder genom målade övergångar. En underhållsgång tillbaka till lastplatsen öppnas från källsidan. En valfri gömma innehåller förnödenheter.

Lastmanifestet säger att levande vittnen skall intyga äldre identiteter. Arkivets tekniska instruktion visar varför: minnesverket kontrollerar namn som tillträdesrätt till gamla kungliga befogenheter. Namnbytet är försök till bedrägeri mot denna kontroll. Detta är ett nytt fiktivt avslöjande i spelets värld, inte ett historiskt påstående om Rudbecks Atlantica. Ingen bekräftad koppling till Rotmarskalkens ed ännu.

Kartan anger platser och väg; gjutformen tillverkar metallplåten; stjärnplåten riktar maskinen. Vittnenas intyg är en separat behörighetskontroll. Föremålen är alltså inte tre namn på samma sak.

Saltväktaren använder kedjeslag och reglerar översvämningar. Karl kan avlasta två luckspel; vid halva livet lossnar väktaren från sitt fäste och blir rörligare. Han är sårbar även före avlastning. Efter striden måste Karl använda pulpeten för att häva kvarhållningen. Röster över ledningen bekräftar överlevande, och expeditionen för dem via lastplatsen till skytteln. Ingen osedd räddning räknas som utförd innan pulpeten används.

Ordern namnger Riksantikvariens kansli som beställare och Kungaminnet som målet. Vem på kansliet som står bakom, vad som finns i innersta valvet och vilka följder ett öppnande får återstår. Karl återvänder till Ebba för avrapportering och systrarnas efterspel. Ingen ny färd öppnas förrän den byggts.

## Repliker

**`salt-reunion-arrival` · ebba**

> Skytteln har hämtat Märta från observatoriet. Elin, hon väntar här hos oss.

**`salt-reunion-marta` · marta**

> Elin. Jag har läst ditt namn varje kväll. Jag vågade inte sluta.

**`salt-reunion-elin` · elin**

> Jag är här. Och jag heter fortfarande Elin Vinge.

**`salt-reunion-answer` · marta**

> Det vet jag. Kom hit. Du behöver inte bevisa det för mig.

**`salt-reunion-route` · ebba**

> Ni stannar ombord. Karl, vi går ned genom porten bakom vågen. Den leder till Saltkällans lastplats.

**`salt-loading` · ebba**

> Tomma bårar. Inga gravar. Följ spåren, Karl. Vi vet ännu inte om de andra lever.

**`salt-manifest` · ebba**

> Här står att levande vittnen skall bekräfta äldre namn. Leveransen går till källans minnesverk. Det här är mottagarens egen instruktion.

**`salt-stairs` · marta**

> Rören matar vatten till namnplåtarna. Stäng tillförseln vid hjulet, annars når du inte arkivet.

**`salt-drained` · ebba**

> Vattnet sjunker. Den inre porten är fri.

**`salt-archive` · marta**

> De vill få minnesverket att godta nya människor som de gamla kungarnas vittnen. Namnen är nycklar till deras befogenheter.

**`salt-proof` · ebba**

> Kartan visar vägen. Gjutformen tillverkar plåten. Stjärnplåten ställer in verket. Men det är en levande människas intyg som skall öppna det. Därför behövde de Elin.

**`salt-watcher` · saltwarden**

> Vittnena står under mitt beskydd. Ingen återkallar en bekräftad överföring.

**`salt-chains` · ebba**

> Kedjorna styr luckorna. Dra i spelen för att sänka vattnet. Håll dig undan där golvet mörknar.

**`salt-rising` · saltwarden**

> Tillförseln öppnas. Stå kvar vid ert tilldelade namn.

**`salt-second` · ebba**

> Han lämnar sitt fäste. Se upp för kedjan, Karl.

**`salt-fallen` · ebba**

> Väktaren är nere. Stäng verket vid pulpeten. Vi måste få upp dem innan vattnet stiger igen.

**`salt-release` · marta**

> Kvarhållningen är bruten. Jag har röster i ledningen. De lever, Karl. Skytteln tar emot dem vid lastplatsen.

**`salt-order` · ebba**

> Beställningen är signerad av Riksantikvariens kansli. De begär tillträde till Kungaminnets innersta valv. Jag tar originalet. Ingen skall kunna skriva om det här också.

**`salt-return` · elin**

> De fick tillbaka sina egna namn. Tack. Jag vill träffa dem när de orkar.

**`salt-debrief` · ebba**

> Alla från liggaren är räknade ombord. Nästa spår är kansliets beställning. I kväll avslutar vi räkningen här.

**`salt-cache` · marta**

> En underhållskista. De som arbetade här lämnade en väg ut åt sig själva.


Linux-export: `dist/AtlandsArv-0.23-saltkallan`. Hela mappen med körfil, PCK och .NET-katalog skall behållas tillsammans. Exporten utesluter äldre `dist`-innehåll.
