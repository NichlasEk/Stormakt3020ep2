# Arkivets fortsättning

2026-09-07 · 0.9. Arkivet är kopplat till Rotvägen: två nya beständiga rum, motviktsport och ett stridsmöte vars förstärkningar följer arkivvalet. Se [Rotvägen](ROOTWAY.md). Nio rum delar samma rumssparning, hälsa, utrustning och stash. Layout 4 migrerar till 5.

## Fortsatt expansion

Rumsrutten behåller `CampaignStage == 0` och använder stabila rum-ID:n. `EnterCampaign(index)` i gamla åttastegskampanjen tömmer fiender och ger hälsa/tinktur; den används inte vid passage från arkivet till Rotvägen. Framtida rum mot berget ska följa samma snapshot-/dörrmodell eller uttryckligen migrera den. De två kampanjstarterna är fortfarande separata.

`Combat.ArchiveChoice` är enda källan till dokumentbeslutet. `ArchiveSecured` öppnar Rotvägen. `RootGateOpen` öppnar lunden, `GroveWave` minns utlösta förstärkningar och `GroveSecured` skyddar belöningen från upprepning. Nästa värld ska bevara dessa flaggor, utrustning, utforskning och fyndens rumsägarskap. Återvägen är en del av systemet.
