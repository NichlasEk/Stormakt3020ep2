using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public static class PortRooms
{
    public const string Court="court",Lodge="lodge";
    public static readonly Vector2 CourtDoor=new(490,250),LodgeDoor=new(975,435);
    public static readonly Vector2[] CourtGround={new(400,285),new(640,176),new(1280,370),new(1340,590),new(800,956),new(225,700),new(230,440)};
    public static readonly Vector2 Key=new(520,620),Cache=new(542,650);
    public static bool Known(string id)=>id is Court or Lodge;
    public static Vector2 Door(string id)=>id==Court?CourtDoor:LodgeDoor;
    public static Vector2 Arrival(string id)=>id==Court?new(530,330):new(930,510);
}

public sealed class RoomSnapshot
{
    public bool Visited;
    public byte[] Explored=new byte[RoomSight.Bytes];
    // Only inactive rooms own actors here. The current room uses Combat.Enemies.
    public List<Fighter> Enemies=new();
}

public sealed class RoomRun
{
    public string Current=PortRooms.Court;
    public bool KeyTaken,DoorOpen,CacheTaken;
    public Dictionary<string,RoomSnapshot> Rooms=new()
    {
        [PortRooms.Court]=new(){Visited=true},[PortRooms.Lodge]=new()
    };
}

public sealed partial class Combat
{
    public RoomRun? Rooms;
    [JsonIgnore] private bool _roomInteractHeld=true;
    [JsonIgnore] public bool InRooms=>Rooms!=null;
    [JsonIgnore] public string RoomName=>Rooms?.Current==PortRooms.Lodge?"Väktarnas logement":"Den dränkta förgården";
    [JsonIgnore] public string RoomGoal=>!Rooms!.KeyTaken?"Hitta väktarens nyckel":!Rooms.DoorOpen?"Lås upp logementet":!Rooms.CacheTaken?"Sök fyndet i logementet":"Fyndet säkrat · återvänd fritt";
    [JsonIgnore] public Vector2 RoomObjective=>!Rooms!.KeyTaken?PortRooms.Key:Rooms.Current==PortRooms.Lodge&&!Rooms.CacheTaken?PortRooms.Cache:PortRooms.Door(Rooms.Current);

    public static Combat NewRooms(Order order)
    {
        var game=NewAtland(order);game.Rooms=new();game.Enemies.Clear();game.Events.Clear();
        game.Spawn(EnemyKind.Guard,new(650,625));game.Player=new(768,805);
        game.UpdateRoomSight(true);
        game.Emit("region",game.Player,game.RoomName);game.Emit("campaign",game.Player,"Väktaren bar nyckeln till logementet. Hans packning ligger kvar i förgården.");
        return game;
    }

    private void StepRooms(Controls input)
    {
        UpdateRoomSight();
        bool pressed=input.Interact&&!_roomInteractHeld;_roomInteractHeld=input.Interact;
        if(!pressed||Dead||Moving||AttackTime>0||DodgeTime>0||Guarding||Hurt>0)return;
        bool Near(Vector2 at)=>Vector2.Distance(Player,at)<72&&ClearPath(Player,at);
        if(Rooms!.Current==PortRooms.Court&&!Rooms.KeyTaken&&Near(PortRooms.Key))
        {
            if(Enemies.Any(e=>!e.Dead)){Emit("room-notice",Player,"Slå tillbaka väktaren innan du söker hans packning.");return;}
            Rooms.KeyTaken=true;Emit("inscription",Player,"VÄKTARENS NYCKEL");Emit("checkpoint",Player);return;
        }
        if(Rooms.Current==PortRooms.Lodge&&!Rooms.CacheTaken&&Near(PortRooms.Cache))
        {
            if(Enemies.Any(e=>!e.Dead)){Emit("room-notice",Player,"Säkra logementet innan du undersöker kistan.");return;}
            Rooms.CacheTaken=true;DropItem("memory",PortRooms.Cache);
            Emit("campaign",Player,"Under filtarna ligger ett vittnessigill och en fuktskadad ritning. En cistern förbinder logementet med portens äldre grund.");
            Emit("checkpoint",Player);return;
        }
        if(!Near(PortRooms.Door(Rooms.Current)))return;
        if(!Rooms.KeyTaken){Emit("room-notice",Player,"Låst · väktarens nyckel saknas.");return;}
        if(Enemies.Any(e=>!e.Dead)||Shots.Any(s=>!s.Reflected)||Hazards.Any(h=>!h.Friendly))
        {Emit("room-notice",Player,"Säkra rummet före passage.");return;}
        if(!Rooms.DoorOpen)
        {
            Rooms.DoorOpen=true;Emit("room-notice",Player,"Dörren är upplåst. Tryck E / B igen för att gå in.");
            Emit("checkpoint",Player);return;
        }
        ChangeRoom(Rooms.Current==PortRooms.Court?PortRooms.Lodge:PortRooms.Court);
    }

    private void ChangeRoom(string destination)
    {
        Rooms!.Rooms[Rooms.Current].Enemies=Enemies;
        Rooms.Current=destination;var next=Rooms.Rooms[destination];
        Enemies=next.Enemies;next.Enemies=new();
        if(!next.Visited)
        {
            next.Visited=true;
            Spawn(EnemyKind.Guard,new(650,735));Spawn(EnemyKind.Gunner,new(530,585));
        }
        Player=PortRooms.Arrival(destination);Facing=MoveDirection=Vector2.UnitY;
        Shots.Clear();Hazards.Clear();AttackTime=AttackBuffer=DodgeTime=HitStop=Hurt=GuardTime=0;
        Guarding=Moving=false;
        // No healing, potion award, loot respawn or stamina refill on a doorway.
        UpdateRoomSight(true);
        Emit("region",Player,RoomName);Emit("checkpoint",Player);
    }

    public void ValidateRooms()
    {
        if(Rooms is null)return; // Original campaign saves have no room run.
        var r=Rooms;
        if(!InCampaign||CampaignStage!=0||Region!=Region.Atland||CampaignFinished||!PortRooms.Known(r.Current)
            ||r.Rooms is null||r.Rooms.Count!=2||!r.Rooms.ContainsKey(PortRooms.Court)||!r.Rooms.ContainsKey(PortRooms.Lodge)
            ||r.Rooms.Any(p=>p.Value is null||p.Value.Enemies is null||p.Value.Enemies.Count>100||p.Value.Explored is null||p.Value.Explored.Length!=RoomSight.Bytes||(!p.Value.Visited&&p.Value.Explored.Any(b=>b!=0)))
            ||!r.Rooms[PortRooms.Court].Visited||!r.Rooms[r.Current].Visited||r.Rooms[r.Current].Enemies.Count!=0
            ||(r.DoorOpen&&!r.KeyTaken)||(r.Rooms[PortRooms.Lodge].Visited&&!r.DoorOpen)||(r.CacheTaken&&!r.Rooms[PortRooms.Lodge].Visited))
            throw new System.IO.InvalidDataException("Ogiltig rumsexpedition");
        var actors=Enemies.Concat(r.Rooms.Values.SelectMany(s=>s.Enemies)).ToArray();
        if(actors.Any(e=>e is null||e.Id<1||!Enum.IsDefined(e.Kind)||!float.IsFinite(e.Health)||!float.IsFinite(e.MaxHealth)||e.MaxHealth<=0||e.Health>e.MaxHealth
                ||!float.IsFinite(e.Position.X)||!float.IsFinite(e.Position.Y))||actors.Select(e=>e.Id).Distinct().Count()!=actors.Length
            ||actors.Any(e=>e.Id>=NextId)||r.Rooms.Values.Any(s=>!s.Visited&&s.Enemies.Count!=0))
            throw new System.IO.InvalidDataException("Ogiltiga rumsvakter");
        if(Inventory.Drops.Any(d=>d.Room!=""&&!PortRooms.Known(d.Room)))throw new System.IO.InvalidDataException("Ogiltigt fyndrum");
    }
}
