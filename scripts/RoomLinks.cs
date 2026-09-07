using System;
using System.Collections.Generic;
using System.Numerics;

namespace Atland;

public enum PassageGate { Key,Plans,Water,Shortcut }
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
        new("shortcut",PortRooms.Cistern,new(1315,655),new(1200,685),PortRooms.Court,new(290,655),new(375,660),PassageGate.Shortcut)
    };
    public static IEnumerable<RoomLink> From(string room)=>Array.FindAll(All,l=>l.A==room||l.B==room);
    public static bool Open(RoomRun run,RoomLink link)=>link.Gate switch
    {PassageGate.Key=>run.DoorOpen,PassageGate.Plans=>run.CacheTaken,PassageGate.Water=>run.WaterLowered,_=>run.ShortcutOpen};
    public static string LockedReason(RoomLink link)=>link.Gate switch
    {PassageGate.Key=>"Låst · väktarens nyckel saknas.",PassageGate.Plans=>"Undersök logementets ritning först.",PassageGate.Water=>"Trappan ligger under vatten. Frilägg den med pumpen.",_=>"Reglad från cisternens sida."};
}
