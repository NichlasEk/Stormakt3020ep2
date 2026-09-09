# 0.20 · Under kungshögarna och målade passager

Första spelbara etappen i Gamla Uppsala omfattar uppställningsplatsen, stenpassagen och de överfördas väntrum. Säkra rummen, läs liggaren, tala två gånger med Nils Berg och återvänd ombord till Ebba. Västra vågen och Mönstringsförrättaren är nästa byggmål, inte en färdig boss bakom den stängda västra porten.

## Fortsätta och provspela

Fortsätt din sparning: lämna observatoriets originalorder till Ebba och använd därefter rodret i kajutan. Fregatten Karl CCLV tar dig till Gamla Uppsala. På land går du fram till högens öppning. E/B används för föremål, samtal och dörrar; själva passagerna går du genom. Återvänd samma väg till landgången och Ebba när Nils har vittnat. Rodret kan sedan flyga tillbaka till Instrumentgården.

Separat prov från kajutan med observatoriet avklarat:

```sh
./play.sh -- --gamla
```

`--gamla-new` börjar om just provsparningen. Huvudkampanjens sparning påverkas inte. Filmen Under kungshögarna finns även i utvecklarinställningarnas videobibliotek.

## Målade dörrövergångar

Den vanliga spelvägen använder nu samma scenknep även i äldre rum: Karl går in i en öppen passage, skalas ned runt fötterna, försvinner bakom portalen och kommer fram i nästa målning. En kort nedtoning täcker kamerans byte; han växer tillbaka medan han går ut. Hela förflyttningen tar cirka 1,8 sekunder. De byggda brogolven, broväggarna och deras kartstreck visas inte längre.

Fysiska dörrblad, lås, förstörda portar och rummens fynd består. Övergången börjar först när dörren faktiskt är tillräckligt öppen. Under den korta förflyttningen pausas stridssimuleringen. Fiender stannar i sina respektive rum; följande fiender genom portalernas animation är inte infört. Det är målade scenövergångar, inte en enda sömlös kamerapanorering över hela världen.

Bakom presentationen finns tidigare korridorgeometri kvar för kompatibilitet och geometritester. Äldre sparningar migreras till 30 rum, layout 13. En sparad position ute på en gammal bro återställs till närmaste anslutnings ankomstpunkt när den nya presentationen slås på. Pågående övergång har eget sparat tillstånd.

## Bild, ljud och berättelse

Tre nya målade bakgrunder och Nils kropp/porträtt kommer från bildverktyget; promptfiler ligger intill bilderna. Riktmärket är matta, slitna historiska målningar och mänskliga proportioner. Den stora stolen i astronomernas bostäder har minskats separat, utan att hela rummet skalats om.

13 nya repliker använder Ebba samt en syntetisk röst för Nils. Källor, begäranden och kompletta tagningar finns i `assets/source/voices`; radio- och direktdialog använder `assets/story/gamla-radio.json`. Automatisk taltranskription användes för kontroll och två repliker togs om; detta ersätter inte lyssning vid provspelning. LTX-källfilm och arbetsflöde finns under `assets/source/cinematics`, med verktygen `render_gamla.py` och `assemble_gamla.py`. Spelfilmen är Theora och cirka åtta sekunder lång.

Nils såg Elin levande när hon fördes vidare. Spelet påstår inte att han vet hur hon mår nu. Den gränsen står även i [storyboardens scen 17](STORYBOARD.md).

## Kontroller

`dotnet run --project tests` täcker kampanj, migration, strid, dörrar, sparningar och nya etappen. `--gamla-only` går hela nya rutten fram och tillbaka med båda ordervalen. `--painted-only` kontrollerar även 48 riktningar genom äldre anslutningar. Native-provet `godot-mono --path . -- --gamla-check` tar bilder av portalanimationerna, kontrollerar Nils/röster och att ankomstfilmen avkodas utan att ändra kampanjens tillstånd. Samma kontroll kan köras på exporten.

Exporten ligger i `dist/AtlandsArv-0.20-painted-passages`. Native-provet är godkänt även mot exportens PCK och .NET-assembly. `play.sh` pekar på denna version. Bildfångsterna finns i exportens `artifacts/` och källträdets `artifacts/`.

Slutkontroll: hela testsamlingen godkänd med 117 422 assertions, inklusive de 48 äldre portalriktningarna. Storyboardens 132 repliker har jämförts ordagrant mot alla 14 JSON-manus.
