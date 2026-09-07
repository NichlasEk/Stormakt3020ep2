# Minnets arkiv: anslutningen är byggd

2026-09-07 · 0.8. Den tidigare planen är genomförd: sjunde beständiga rummet, egen målning och dokumentvy, layoutmigration 3→4, två beslut med olika kontrollpatruller och återväg med sparad utrustning/fynd. Se [levererad funktion och verifiering](ARCHIVE.md).

## När nästa värld ansluts

`EnterCampaign(index)` tömmer aktiva fiender, använder de gamla gemensamma arenakoordinaterna och ger hälsa/tinktur. `ValidateRooms()` kräver fortfarande kampanjsteg 0. `LocalDrops` kopplar fynd till region, steg och rum-ID. Övergången vidare mot rotmarkerna måste därför uttryckligen bevara rumsruttens vilande tillstånd och definiera återvägen, fyndens ägarskap och resursregler. Den gamla åttastegskampanjen är fortfarande separat.

`Combat.ArchiveChoice` är beslutets enda källa (1 bevara, 2 förfalska). `RoomRun.ArchiveSecured` anger att kontrollen besegrats och grinden undersökts. Nästa värld bör läsa dessa flaggor och inte fråga efter valet på nytt. Arkivets patrull får inte återställas eller belönas igen vid återbesök.
