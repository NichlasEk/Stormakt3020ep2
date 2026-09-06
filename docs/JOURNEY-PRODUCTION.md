# Vägen under vattnet · produktionsunderlag

## Bana och mått

`tools/build_level_blockouts.py` skapar två Blender-scener med samma ortografiska kamera, meter som enhet och en 1,78 m hög referensfigur som projiceras till 150 bildpixlar. Golvet är 8 × 8 meter. Blender är ett produktionsverktyg; spelet laddar färdiga målningar.

Bildverktyget flyttade en del props trots koordinaterna i briefen. Därför är `assets/source/journey/layout.json` den ursprungliga projektionen, medan `paint-calibration.json` innehåller granskade korrigeringar för den faktiska målningen: magasinets inre gångyta, lastens fotavtryck, dörren, förrådskistan och strandens tre interaktionsplattor. `python tools/export_journey_layout.py` kombinerar dem till `JourneyLayout.cs`. Den målade miljön är inte en exakt återgivning av varje ursprungligt metermått.

`Navigation.cs` hanterar golv, lastens utvidgade kollisionspolygon, fri sikt och vägval runt lasten. Kulor och närstrid stoppas av lasten. `SceneryDepth.cs` återanvänder bakgrundens UV-koordinater inom manuellt granskade konturer: last, kista, vinsch och lyktor sorteras med figurerna efter markdjup. Främre murar och strandens främsta bautasten ritas ovanpå figurer bakom dem. Konturerna måste granskas igen om målningen byts.

## Spelbar följd

1. Kajens befintliga strid, karta, tre namn och vägval.
2. Förföljare vid porten; E/B leder vidare till magasinet.
3. Tre vakter, brynstål + tinktur, kollegiets mätorder, vinsch och två nya vakter. Brynstålet ger nästa sabelkontakt +60 procent efter en perfekt parad, inom 2,6 sekunder.
4. Stranden: två vakter, tre riktningar som vardera kräver två sekunders ostörd mätning, en sista grupp om tre fiender.
5. E/B vid mittstenen riktar kartan. Kameran visar den tidigare dolda vägen och porten i gryningen. Nya mätband och mätordern visar kollegiets försprång. Därefter upphämtning och fältdagbok.

Nya uppdrag använder den långa rutten. Ett avslutat 0.2-läge med alla tre namn kan fortsätta från slutmenyn. Ursprungliga enumvärden är bevarade; nya tillstånd ligger sist. Gamla korta regeltester använder fortsatt `Combat.New(order)`; normalspelandet använder `Combat.New(order,true)`.

## Animation och kända gränser

Karl med sabel och sabelvakten har vardera tre nya atlaser: gång, anfall och reaktion. Fyra riktningar, fyra gångnyckelbilder och fyra attackfaser; gard, träffreaktion, undanmanöver och liggande dödsposer. Alla står cirka 150 världspixlar höga. Karl använder den korrekt bakåtvända, speglade NE-kontakten för den felmålade NW-kontakten.

Efter användarens återkoppling om ryckig gång är gångbilderna registrerade efter bäckenets mitt i stället för den nedersta stöveln. Stegrytmen följer verklig tillryggalagd sträcka, riktningen följer förflyttningen, och figurpositionerna interpoleras mellan fysikstegen. Sparfiler och stridsregler använder fortfarande de exakta fysikpositionerna.

Gångens målade nyckelbilder är fortfarande ojämna och vissa steg för lika. Det är en första riktad animationspassning, inte en slutlig mjuk åttariktad animationsuppsättning. Ett nytt åttabildsförsök avvisades eftersom riktning och alternerande ben fortfarande blev fel. Nästa pass bör använda en faktisk riggad rörelsereferens före ommålning. Specialistrollerna och hammaren har kvar tidigare tre poser och horisontell spegling.

Ingen full inventarieskärm, utrustningsloot, Uppsala-bana, Varvsänka eller filmproduktion ingår i 0.3. De fjorton nya replikerna använder de befintliga godkända porträtten och syntetiska röstidentiteterna. Tre befintliga musikspår återanvänds; denna milstolpe lägger till nytt fotstegsljud.
