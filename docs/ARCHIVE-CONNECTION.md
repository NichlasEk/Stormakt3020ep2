# Nästa rum: Minnets arkiv

2026-09-07. Teknisk anslutning granskad mot `Rooms.cs`, `RoomLinks.cs`, `Campaign.cs`, `Inventory.cs` och `SaveStore.cs`. Följande är planerat, ännu inte implementerat.

Arkivet blir ett sjunde beständigt rum bakom Edskammarens inre port. Samma Karl, hälsa, vapen, utrustning, stash, markfynd och sex tidigare rum följer med. Spelaren ska kunna gå tillbaka och hämta cisternens klinga även efter arkivbesöket.

## Spelidé

Ett lugnt arkiv med två motstridiga dokument: den officiella liggaren och ett återfunnet vittnesmål. Spelaren jämför dem vid ett läsbord och väljer att bevara handlingarna eller använda Kollegiets blankett för en falsk passersedel. Ingen ny obligatorisk runda med tre identiska insamlingar. Hedvig tolkar dokumenten; Ebba tar ställning till den praktiska följden. Nästa bevakning ska reagera på valet.

## Byggordning

1. Måla en egen arkivinteriör och mät golv, läsbord, hyllor och dörrpunkter. In-/utgången ska ha fri ankomst och tydlig koppling tillbaka till Edskammaren.
2. Lägg till stabilt `archive`-ID och en `RoomLink` med krav `Completed`. Första E på bossporten öppnar den; nästa separata tryck passerar. Gamla avslutade sexrumssparningar får därmed fortsättningen direkt.
3. Layoutversion 4 migrerar version 3 genom att lägga till ett obesökt arkiv. Den befintliga kedjan 1→2→3 får fortsätta till 4. Spara dokumentundersökning, valt beslut, fiender/fynd och utforskning.
4. Låt `Combat.ArchiveChoice` vara det enda beslut som senare berättelse/grupper läser. Rumsprovet använder fortfarande sin egen scenlogik medan arkivrummet spelas. Ge beslutet en konkret och testad effekt på nästa möte.
5. Prova hela vägen från ny start och äldre version 3-sparning, bägge beslut, full väska, återbesök efter valet och sparning före/efter passage. Ingen fri läkning eller duplicerad belöning vid dörren.

## När nästa värld ansluts

`EnterCampaign(index)` tömmer aktiva fiender, använder de gamla gemensamma arenakoordinaterna och ger hälsa/tinktur. `ValidateRooms()` kräver i dag kampanjsteg 0. `LocalDrops` kopplar fynd till region, steg och rum-ID. Övergången vidare mot rotmarkerna måste därför uttryckligen bevara rumsruttens vilande tillstånd och definiera återvägen, fyndens ägarskap och resursregler. Att koppla sjunde rummet är en egen checkpoint före detta större världssteg.

Nya bilder, arkivdialog och läsbordsdetaljer produceras tillsammans med arkivrummet. Rumsradions första ljudpass är levererat i `ROOM-AUDIO.md`.
