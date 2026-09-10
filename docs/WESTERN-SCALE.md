# 0.21 · Västra vågen

Fortsättningen efter Nils vittnesmål är spelbar. Lämna först kvittensen till Ebba, återvänd till de överfördas väntrum och gå genom porten till höger. Den leder genom tre nya målade rum: kontrollgången, Västra vågen och rummet bakom vågen. Kartan har nu 33 beständiga platser.

## Spelväg

1. Säkra kontrollgången och läs vågens instruktion vid bakre skrivbordet.
2. Öppna porten till vågsalen. Mönstringsförrättarens skydd hålls uppe av belastningen. Lossa båda bromsarna med E/B. Då får du nio sekunder att angripa innan bromsarna griper igen.
3. Lämna den markerade nedslagsplatsen innan varningen löper ut. Förrättaren står vid vågen och använder sin sigillstav; han har fyra egna målade poser. Vid halva livet kallar han på två vakter.
4. Efter segern öppnas vägen bakom vågen. Läs Elins handling på bordet och tala med henne två gånger.
5. Gå samma väg tillbaka till fregatten. Tala med Ebba för avrapportering; Elin finns också ombord och kan tilltalas separat med E/B.

Elin lever och har vägrat skriva under ett annat namn. Hon blev därför kvarhållen medan de andra fördes mot Saltkällan. Hon vet inte vad som hände dem där. Vi har inte avslöjat beställaren bakom transporterna eller knutit detta till regementets ed. Återföreningen med Märta är utlovad i berättelsen, men ännu ingen spelad scen. Saltkällan är nästa byggmål.

## Prova

Fortsätt befintlig huvudkampanj eller Gamla Uppsala-sparning. Ett separat prov börjar i väntrummet med Nils kvittens avrapporterad:

```sh
./play.sh -- --west
```

`--west-new` börjar om endast denna provsparning. Inget krav på hammare eller slumpmässigt nyckeldropp. Räddningen, öppnade portar, instruktioner, stridsläge och avrapportering sparas. Äldre 0.20-sparningar migreras från layout 13 till 14 utan att tidigare rum återställs.

## Presentation och assets

Tre nya bakgrunder: `assets/art/room-gamla-west-{control,hall,refuge}-v1.png`. Väntrummets högra öppning har en separat öppen målning i `room-gamla-registry-open-v1.png`. Samma målade djupövergångar som i 0.20 används. Portarnas lås och dörrblad är fortsatt fysiska.

`muster-officer-v1.png` innehåller fyra poser för förrättaren. `elin-vinge-v1.png` används för både kropp och porträtt. Bilderna skapades med det inbyggda bildverktyget; slutliga promptar och separata friläggningspromptar ligger intill assets som `west-*.prompt.md`. Inga Blender-figurer används.

17 nya lokalt genererade svenska repliker finns i `assets/story/west-radio.json`, med Ebba och två nya syntetiska roller: Elin och Mönstringsförrättaren. Begäranden, referenser och tagningar ligger i `assets/source/voices`. `tools/generate_voices.py --west-only` återskapar saknade klipp. Automatisk transkription har kontrollerats; detta ersätter inte mänsklig lyssning. Den här etappen har ingen ytterligare film; Gamla Uppsalas ankomstfilm finns kvar.

Elins återresa hanteras som del av expeditionens beständiga tillstånd. Det finns ännu ingen separat animerad följeslagare som går bakom Karl genom varje passage. Hon syns i räddningsrummet före avrapporteringen och ombord när Karl återvänder. Efter avrapportering står hon kvar ombord.

## Verifiering

- Hela testsamlingen: 117 537 kontroller godkända före den sista tilläggsrepliken på Elins egen interaktionspunkt.
- Separata West-tester kör hela vägen med både ordervalen och både värja/hammare. De kontrollerar migration, laddat skydd, bromsar, förstärkningar, räddning, retur genom fem portar, sparning under utgångsanimationen och att belöningen inte kan dupliceras.
- Native-kontroll: `godot-mono --path . -- --west-check` kontrollerar bilder, bossposer, portalgång, Elin i båda miljöerna och att alla 17 röster laddas. Bildfångster skrivs till `artifacts/west-*.png`.

Linux-export: `dist/AtlandsArv-0.21-western-scale`. Behåll PCK, körfil och katalogen med .NET-assembly tillsammans.

Slutkontroll: 116 separata West-kontroller godkända efter kajuttillägget. Native-provet är även godkänt mot den färdiga Linux-exporten. Storyboardens 149 repliker är avstämda mot samtliga 15 manusfiler. `play.sh` startar 0.21.
