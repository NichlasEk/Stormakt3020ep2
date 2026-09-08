using System;
using System.Collections.Generic;
using System.Numerics;

namespace Atland;

public enum PassageGate { Key,Plans,Water,Shortcut,Witness,InnerPort,Archive,RootGate,Free,RegimentAccess,RegimentExit,RegimentShortcut }
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
        new("retreat",Regiment.Flags,new(800,930),new(800,830),PortRooms.Grove,new(800,920),new(800,830),PassageGate.RegimentShortcut)
    };
    public static IEnumerable<RoomLink> From(string room)=>Array.FindAll(All,l=>l.A==room||l.B==room);
    public static bool Open(RoomRun run,RoomLink link)=>link.Gate switch
    {PassageGate.Free=>true,PassageGate.RegimentAccess=>run.GroveSecured,PassageGate.RegimentExit=>run.Regiment.Discharged,PassageGate.RegimentShortcut=>run.Regiment.ShortcutOpen,PassageGate.Archive=>run.ArchiveSecured,PassageGate.RootGate=>run.RootGateOpen,PassageGate.InnerPort=>run.Completed,PassageGate.Key=>run.DoorOpen,PassageGate.Plans=>run.CacheTaken,PassageGate.Water=>run.WaterLowered,PassageGate.Witness=>run.WitnessRead,_=>run.ShortcutOpen};
    public static string LockedReason(RoomLink link)=>link.Gate switch
    {PassageGate.RegimentAccess=>"Återfinn lundens namn innan du följer stigen.",PassageGate.RegimentExit=>"Läs avlösningen vid mönstringsstenen.",PassageGate.RegimentShortcut=>"Återtågsvägen är reglad från fanlunden.",PassageGate.Free=>"Öppen gångväg",PassageGate.Archive=>"Säkra arkivets grind efter kontrollen.",PassageGate.RootGate=>"Lossa spärren vid motvikten först.",PassageGate.InnerPort=>"Besegra Edsväktaren och öppna den inre porten.",PassageGate.Key=>"Låst · väktarens nyckel saknas.",PassageGate.Plans=>"Undersök logementets ritning först.",PassageGate.Witness=>"Läs vittnesboken innan du bryter kammarens försegling.",PassageGate.Water=>"Trappan ligger under vatten. Frilägg den med pumpen.",_=>"Reglad från cisternens sida."};
}
