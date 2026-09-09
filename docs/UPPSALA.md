# 0.16 · Karl CCLV till Uppsala

**Fortsättningen är nu byggd i [0.17 · Meridiansalen](MERIDIAN.md).** Nedan bevaras leveransbeskrivningen för första Uppsalagården.

2026-09-08. Resan till Uppsala och den första spelbara gården är byggda. Huvudrutten har tjugo beständiga platser. Meridiansalen bakom gårdens port är nästa byggmål, inte en färdig bana.

## Spela

Fortsätt med `./play.sh`. Efter Kronfogden: ta stjärnplattan, gå tillbaka genom gruvan, ta båten från farleden till regementets brygga. Nära bryggans landfäste, före båtpunkten, finns **E / B · Ombord på Karl CCLV · Uppsala**. Skeppet väntar ute över vattnet; den lilla båten tar expeditionen ut till det.

Direktprov med separat sparning: `./play.sh -- --uppsala`. Börja om provet med `--uppsala-new`. Inställningarnas utvecklarval har också knappen **Prova Uppsalafärden**. Det påverkar inte huvudexpeditionens sparplats.

Avfärden har en målad resescen med lugn kamerarörelse, motorljud och radio. Ebba stänger rampen och Hedvig tolkar ån nedanför. Resans längd följer replikerna. Den är en scen i spelmotorn, inte en ny färdigrenderad videofil i videobiblioteket. Paus stoppar resan; lämnar man till huvudmenyn före ankomst fortsätter sparningen från avreseplatsen.

## Uppsalagården

Gå från skeppets ramp till astronomens bord och läs anvisningen. Tre kompassringar ska riktas: solen mot söder, äpplet mot norr, nyckeln mot väster. E vrider en ring ett kvarts varv. Namn, aktuellt väderstreck och riktning visas vid instrumentet. Varje vridning sparas.

När instrumenten svarar angriper tre vakter. Värja, pik och skjutvapen ger olika avstånd att hantera. Befintliga målade fiendeassets används. Efter striden kan portens daterade sigill undersökas. Datumet blir ett beständigt berättelsefynd och ett vittnessigill släpps som utrustningsfynd. Återvänd till Ebba med avtrycket; Meridiansalen förblir förseglad tills fortsättningen byggts.

Karl CCLV kan flyga tillbaka till bryggan och senare återvända till gården. Hälsa, inventarium, utforskning, ringar, besegrade vakter och fynd bevaras. Ingen gratis läkning, återuppstånden vakt eller dubblerad belöning tillkommer genom resan. Ombordstigning blockeras under strid.

## Bild och ljud

Originalets fregatt finns numera i `/home/nichlas/WaylandForge/assets/stormakt3020/karl-cclv-dark-frigate-v1.png`. Dess mörkblå pansar, guldkronor, spetsiga profil och dubbla motorer användes som referens. Originalprojektet har inte ändrats.

Nya målade bilder skapade med Codex inbyggda imagegen:

- [Fregatten vid bryggan](../assets/art/room-quay-frigate-v1.png) · [prompter](../assets/art/room-quay-frigate-v1.prompt.md)
- [Flygningen över Uppland](../assets/art/flight-uppsala-v1.png) · [prompt](../assets/art/flight-uppsala-v1.prompt.md)
- [Uppsalas instrumentgård](../assets/art/room-uppsala-court-v1.png) · [prompt](../assets/art/room-uppsala-court-v1.prompt.md)

Nio nya Ebba-/Hedvig-repliker i [uppsala-radio.json](../assets/story/uppsala-radio.json), skapade lokalt med befintliga syntetiska röstreferenser och Dots-TTS, mening för mening. Källjud och manifest i `assets/source/voices/uppsala-*`. Automatisk Whisper-avstämning finns lokalt i `artifacts/voice-transcripts.json`; detta ersätter inte mänsklig lyssning. Motorstarten är original PCM från [build_ship_audio.py](../tools/build_ship_audio.py) med måttlig nivå genom spelets volymkontroll. Befintlig musik återanvänds.

## Sparning och kontroller

Rumslayout 9 migrerar 19-rumssparningar till tjugo platser. Äldre migreringskedjor testas också. Inga äldre rum eller gånganslutningar tas bort; Uppsala ligger geografiskt separat och nås med skeppet.

`--uppsala-check` provar den verkliga tidsstyrda avfärden och ankomsten i Godot, instrumenten, striden, radio-/ljudresurserna och återresan, och sparar bilder i `artifacts/uppsala-*.png`. Simuleringstester kör båda vapnen och båda understöden utan utvecklarosårbarhet, vandring mellan interaktionspunkter, blockering av tidig avfärd, sparning efter ringvridningar och återbesök utan återställning.

Verifiering: hela testsviten passerar **70 698 kontroller**. Bygget har noll varningar/fel. Linux-exportens Uppsalaprov och gjuteriprov passerar, inklusive den riktiga tidsstyrda flygningen. Ankomst, flygbild och instrument går att inspektera i fångsterna.

Export: `dist/AtlandsArv-0.16-uppsala/`. Behåll programfil, PCK och hela .NET-katalogen tillsammans.

## Nästa arbete

Öppna Meridiansalen som en fysisk fortsättning genom den målade porten, bygg fler rum kring den instängda morgonen och låt datumet påverka ett berättelseval. Resans kamera använder en målad bild; ett senare filmpass kan ge själva skeppet fler rörelser. Instrumentgårdens bakgrund och kollisionsytor är avpassade för denna första del, men ytterkanter och förgrundsmurar kan behöva fortsatt visuellt puts.
