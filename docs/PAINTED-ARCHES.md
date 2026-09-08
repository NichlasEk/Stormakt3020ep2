# Målade portar · 0.12.1

Historik för det första portparet. **Aktuell fortsättning: [0.12.2, Arkivet och Rotvägen](ROOT-ARCHES.md).**

Första ombyggda förbindelsen är **Vittnesgalleriet → Edskammaren**, porten i användarens bild. Utgången vid galleriets högra golvkant har ersatts av en gångväg genom den befintliga målade dörröppningen. Återvägen går genom Edskammarens vänstra valv. Ingen scenövergång eller teleport används.

- Läs vittnesboken som tidigare. Gå sedan uppåt/höger in genom galleriets högra valv. E används inte för själva förflyttningen.
- Karmarnas uppmätta kollision och målade förgrund följer öppningen. Karl skymts av stenen under passagen.
- Förbindelsens golv och sidomurar målas bakom rumsmålningarna. De läggs inte över porten eller dess utsmyckning.
- Alla utgångspilar och den flytande riktningspilen är borttagna i den sammanhängande expeditionen. Journalen och minikartan finns kvar.
- Sparningar från 0.12 migreras till anslutningsrevision 2. Spelare, fiender eller fynd på borttagen golvyta återförs till lagligt golv i respektive rum. Hälsa, föremål och progression behålls; gammal utforskning av den flyttade förbindelsen rensas.

Detta gäller det första portparet. De andra rummen använder ännu 0.12:s anslutningar och behöver motsvarande individuell inpassning. Mellanrummet bakom valven har också tills vidare återanvändbara stenmaterial; den här ändringen är ingen ny färdig målning av hela korridoren.

Kontroller: hela den sammanhängande rutten, galleriets passage åt båda hållen, sparmigrering och fynd; native `--arch-check` tar bilder före valvet, under valvet och vid ankomst till Edskammaren. Bilder sparas under `artifacts/painted-arch-*.png`.

Verifierat: 39 791 kontroller i hela testsviten, .NET-build utan varningar och native `--arch-check` i den kompletta Linux-exporten `dist/AtlandsArv-0.12.1-painted-arch`. `play.sh` pekar på den exporten. Kontrollprogrammets ljudloopar stoppas före avslut så att ljudtråden hinner släppa sina resurser.
