using System;
using System.Collections.Generic;
using System.Numerics;

namespace Atland;

public enum PassageGate { Key,Plans,Water,Shortcut,Witness,InnerPort,Archive,RootGate,Free,RegimentAccess,RegimentExit,RegimentShortcut,MineEntrance,MinePressure,MineShortcut,Foundry,Clock,Meridian,ObservatoryEntry,ObservatoryMarta,ObservatoryBrake,ObservatoryShortcut,GamlaLedger, WestAccess, WestRegister, WestRelease, SaltEntry, SaltManifest, SaltDrain, SaltLedger, SaltShortcut, RescueEntry, RescueWrit, RescueService, RescueLever, RescueRelease }
public sealed record RoomLink(string Id,string A,Vector2 AtA,Vector2 ArrivalA,string B,Vector2 AtB,Vector2 ArrivalB,PassageGate Gate)
{
    public Vector2 At(string room)=>room==A?AtA:AtB;
    public Vector2 Arrival(string room)=>room==A?ArrivalA:ArrivalB;
    public string Other(string room)=>room==A?B:A;
}

public static class RoomLinks
{
    public static readonly RoomLink[] All={
        new("lodge",PortRooms.Court,PortRooms.CourtDoor,PortRooms.Arrival(PortRooms.Court),PortRooms.Lodge,PortRooms.LodgeDoor,PortRooms.Arrival(PortRooms.Lodge),PassageGate.Key),
        new("pump",PortRooms.Lodge,new(768,860),new(768,770),PortRooms.Pump,new(1000,310),new(965,385),PassageGate.Plans),
        new("cistern",PortRooms.Pump,new(768,850),new(768,760),PortRooms.Cistern,new(975,310),new(970,370),PassageGate.Water),
        new("shortcut",PortRooms.Cistern,new(1315,655),new(1200,685),PortRooms.Court,new(290,655),new(375,660),PassageGate.Shortcut),
        new("gallery",PortRooms.Pump,new(1310,535),new(1200,555),PortRooms.Gallery,new(290,435),new(375,490),PassageGate.Water),
        new("chamber",PortRooms.Gallery,new(1250,445),new(1160,520),PortRooms.Chamber,new(275,435),new(370,500),PassageGate.Witness),
        new("archive",PortRooms.Chamber,PortRooms.OathExit,new(810,395),PortRooms.Archive,ArchiveRoom.Door,ArchiveRoom.Arrival,PassageGate.InnerPort),
        new("roots",PortRooms.Archive,ArchiveRoom.Seal,new(1300,565),PortRooms.Roots,Rootway.Door,Rootway.Arrival,PassageGate.Archive),
        new("grove",PortRooms.Roots,Rootway.Exit,new(1300,580),PortRooms.Grove,Rootway.GroveDoor,Rootway.GroveArrival,PassageGate.RootGate),
        new("regiment",PortRooms.Grove,new(800,920),new(800,830),Regiment.Trail,new(90,460),new(200,460),PassageGate.RegimentAccess),
        new("barracks",Regiment.Trail,new(1430,550),new(1300,550),Regiment.Barracks,new(120,420),new(240,475),PassageGate.Free),
        new("flags",Regiment.Barracks,new(1430,450),new(1310,480),Regiment.Flags,new(100,420),new(230,430),PassageGate.Free),
        new("parade",Regiment.Flags,new(1430,530),new(1290,530),Regiment.Parade,new(125,420),new(240,480),PassageGate.Free),
        new("regiment-quay",Regiment.Parade,new(1425,445),new(1320,490),Regiment.Quay,new(150,410),new(260,475),PassageGate.RegimentExit),
        new("retreat",Regiment.Flags,new(800,930),new(800,830),PortRooms.Grove,new(800,920),new(800,830),PassageGate.RegimentShortcut),
        new("mine-entry",Regiment.Farled,new(1260,300),new(1260,340),Mine.Mouth,new(125,420),new(240,480),PassageGate.MineEntrance),
        new("mine-bellows",Mine.Mouth,new(1360,510),new(1300,550),Mine.Bellows,new(130,420),new(240,480),PassageGate.Free),
        new("mine-coolway",Mine.Bellows,new(1360,510),new(1300,550),Mine.Coolway,new(130,420),new(240,480),PassageGate.MinePressure),
        new("mine-return",Mine.Coolway,Mine.Shortcut,new(780,820),Mine.Mouth,Mine.Shortcut,new(780,820),PassageGate.MineShortcut),
        new("foundry",Mine.Coolway,Foundry.Gate,new(1250,600),Foundry.Room,new(130,420),new(240,480),PassageGate.Foundry),
        new("uppsala-clock",Uppsala.Court,Meridian.CourtGate,new(1040,475),Meridian.Clock,new(100,525),new(240,555),PassageGate.Clock),
        new("meridian-hall",Meridian.Clock,Meridian.ClockExit,new(1300,550),Meridian.Hall,new(110,490),new(240,545),PassageGate.Meridian),
        new("observatory-entry",Meridian.Hall,Observatory.Gate,new(1300,560),Observatory.Quarters,new(95,360),new(240,450),PassageGate.ObservatoryEntry),
        new("observatory-workshop",Observatory.Quarters,new(1410,435),new(1300,495),Observatory.Workshop,new(90,450),new(235,510),PassageGate.ObservatoryMarta),
        new("observatory-machine",Observatory.Workshop,new(1420,470),new(1300,540),Observatory.Machine,new(100,535),new(250,590),PassageGate.Free),
        new("observatory-dome",Observatory.Machine,new(1400,550),new(1270,625),Observatory.Dome,new(105,465),new(250,525),PassageGate.ObservatoryBrake),
        new("observatory-shortcut",Observatory.Machine,Observatory.Shortcut,new(760,520),Observatory.Quarters,new(760,930),new(760,840),PassageGate.ObservatoryShortcut),
        new("gamla-mound",Gamla.Landing,new(1175,495),new(1175,570),Gamla.Passage,new(100,490),new(250,555),PassageGate.Free),
        new("gamla-registry",Gamla.Passage,new(1300,465),new(1250,535),Gamla.Registry,new(100,490),new(250,555),PassageGate.GamlaLedger),
        new("gamla-west-entry",Gamla.Registry,new(1300,460),new(1210,530),West.Control,new(140,490),new(270,555),PassageGate.WestAccess),
        new("gamla-west-scale",West.Control,new(1400,490),new(1290,555),West.Hall,new(140,490),new(270,555),PassageGate.WestRegister),
        new("gamla-west-exit",West.Hall,new(1400,510),new(1290,580),West.Refuge,new(140,490),new(270,555),PassageGate.WestRelease),
        new("salt-entry",West.Refuge,new(1400,490),new(1280,570),Salt.Loading,new(140,490),new(270,555),PassageGate.SaltEntry),
        new("salt-stairs",Salt.Loading,new(1400,490),new(1280,570),Salt.Stairs,new(140,490),new(270,555),PassageGate.SaltManifest),
        new("salt-register",Salt.Stairs,new(1400,490),new(1280,570),Salt.Archive,new(140,420),new(270,500),PassageGate.SaltDrain),
        new("salt-spring",Salt.Archive,new(1400,420),new(1280,500),Salt.Spring,new(140,490),new(270,555),PassageGate.SaltLedger),
        new("rescue-entry",Salt.Spring,new(1400,490),new(1280,570),Rescue.Hall,new(140,490),new(270,555),PassageGate.RescueEntry),
        new("rescue-prison",Rescue.Hall,new(1400,490),new(1280,570),Rescue.Prison,new(140,490),new(270,555),PassageGate.RescueWrit),
        new("rescue-service",Rescue.Hall,new(1000,400),new(1000,505),Rescue.Service,new(140,490),new(270,555),PassageGate.RescueService),
        new("rescue-machine",Rescue.Service,new(1400,490),new(1280,570),Rescue.Machine,new(140,490),new(270,555),PassageGate.RescueLever),
        new("rescue-release",Rescue.Machine,new(1400,490),new(1280,570),Rescue.Prison,new(1400,490),new(1280,570),PassageGate.RescueRelease),
        new("salt-return",Salt.Spring,new(905,440),new(940,510),Salt.Loading,new(790,390),new(790,520),PassageGate.SaltShortcut)
    };
    public static IEnumerable<RoomLink> From(string room)=>Array.FindAll(All,l=>l.A==room||l.B==room);
    public static bool Open(RoomRun run,RoomLink link)=>link.Gate switch
    {PassageGate.RescueEntry=>run.Rescue.Briefed,PassageGate.RescueWrit=>(run.Rescue.WritRead&&!run.Rescue.Trapped)||run.Rescue.Released,PassageGate.RescueService=>run.Rescue.Ready,PassageGate.RescueLever=>run.Rescue.ServiceOpen,PassageGate.RescueRelease=>run.Rescue.Released,PassageGate.SaltEntry=>run.Salt.Reunited,PassageGate.SaltManifest=>run.Salt.ManifestRead,PassageGate.SaltDrain=>run.Salt.Drained,PassageGate.SaltLedger=>run.Salt.LedgerRead,PassageGate.SaltShortcut=>run.Salt.ShortcutOpen,PassageGate.WestAccess=>run.Gamla.Debriefed,PassageGate.WestRegister=>run.West.RegisterRead,PassageGate.WestRelease=>run.West.Defeated,PassageGate.GamlaLedger=>run.Gamla.LedgerRead,PassageGate.ObservatoryEntry=>run.Observatory.EntryOpen,PassageGate.ObservatoryMarta=>run.Observatory.MartaMet,PassageGate.ObservatoryBrake=>run.Observatory.Aligned,PassageGate.ObservatoryShortcut=>run.Observatory.ShortcutOpen,PassageGate.Clock=>run.Meridian.CourtOpen,PassageGate.Meridian=>run.Meridian.ClockAnchored,PassageGate.Foundry=>run.Foundry.GateOpen,PassageGate.MineEntrance=>run.Mine.EntranceOpen,PassageGate.MinePressure=>run.Mine.PressureReleased,PassageGate.MineShortcut=>run.Mine.ShortcutOpen,PassageGate.Free=>true,PassageGate.RegimentAccess=>run.GroveSecured,PassageGate.RegimentExit=>run.Regiment.Discharged,PassageGate.RegimentShortcut=>run.Regiment.ShortcutOpen,PassageGate.Archive=>run.ArchiveSecured,PassageGate.RootGate=>run.RootGateOpen,PassageGate.InnerPort=>run.Completed,PassageGate.Key=>run.DoorOpen,PassageGate.Plans=>run.CacheTaken,PassageGate.Water=>run.WaterLowered,PassageGate.Witness=>run.WitnessRead,_=>run.ShortcutOpen};
    public static string LockedReason(RoomLink link)=>link.Gate switch
    {PassageGate.RescueEntry=>"Tala med Ebba om Kungaminnet ombord.",PassageGate.RescueWrit=>"Kvarhållning · endast återkallelse öppnar låset efter kontrollen.",PassageGate.RescueService=>"Underhållsvägen · Ebbas fältutrustning behövs.",PassageGate.RescueLever=>"Lossa den mekaniska spärren.",PassageGate.RescueRelease=>"Återkalla kvarhållningen vid sigillpulpeten.",PassageGate.SaltEntry=>"Återvänd till Ebba för systrarnas återförening.",PassageGate.SaltManifest=>"Läs lastmanifestet.",PassageGate.SaltDrain=>"Stäng tillförseln vid trappans hjul.",PassageGate.SaltLedger=>"Läs minnesverkets instruktion.",PassageGate.SaltShortcut=>"Underhållsgången öppnas vid källans pulpet.",PassageGate.WestAccess=>"Lämna Nils vittnesmål till Ebba först.",PassageGate.WestRegister=>"Läs vågens instruktion i kontrollgången.",PassageGate.WestRelease=>"Mönstringsförrättaren håller porten stängd.",PassageGate.GamlaLedger=>"Läs transportliggaren och använd orderns sigill.",PassageGate.ObservatoryEntry=>"Visa Ebba ordern. Återvänd sedan till den inre porten.",PassageGate.ObservatoryMarta=>"Tala med assistenten i bostäderna.",PassageGate.ObservatoryBrake=>"Ställ maskinrummets bromsar efter diagrammet.",PassageGate.ObservatoryShortcut=>"Underhållsvägen är reglad från maskinrummet.",PassageGate.Clock=>"Använd datumavtrycket i portens lås.",PassageGate.Meridian=>"Låt slagverket avsluta sin upprepning.",PassageGate.Foundry=>"Undersök gjutformen och lossa gjuteriets spärr.",PassageGate.MineEntrance=>"Lossa gruvportens spärr vid trappan.",PassageGate.MinePressure=>"Stäng matningen och öppna avlastningen först.",PassageGate.MineShortcut=>"Reglad från svalgångens sida.",PassageGate.RegimentAccess=>"Återfinn lundens namn innan du följer stigen.",PassageGate.RegimentExit=>"Läs avlösningen vid mönstringsstenen.",PassageGate.RegimentShortcut=>"Återtågsvägen är reglad från fanlunden.",PassageGate.Free=>"Öppen gångväg",PassageGate.Archive=>"Säkra arkivets grind efter kontrollen.",PassageGate.RootGate=>"Lossa spärren vid motvikten först.",PassageGate.InnerPort=>"Besegra Edsväktaren och öppna den inre porten.",PassageGate.Key=>"Låst · väktarens nyckel saknas.",PassageGate.Plans=>"Undersök logementets ritning först.",PassageGate.Witness=>"Läs vittnesboken innan du bryter kammarens försegling.",PassageGate.Water=>"Trappan ligger under vatten. Frilägg den med pumpen.",_=>"Reglad från cisternens sida."};
}
