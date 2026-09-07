using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public static class PortRooms
{
    public const string Court="court",Lodge="lodge",Pump="pump",Cistern="cistern";
    public static readonly Vector2 CourtDoor=new(490,250),LodgeDoor=new(975,435);
    public static readonly Vector2[] CourtGround={new(400,285),new(640,176),new(1280,370),new(1340,590),new(800,956),new(225,700),new(230,440)};
    public static readonly Vector2 Key=new(520,620),Cache=new(542,650);
    public static readonly Vector2 Pressure=new(1170,590),Wheel=new(405,570),Relic=new(365,665);
    public static readonly Vector2[] PumpGround={new(395,420),new(990,270),new(1410,485),new(1330,610),new(850,875),new(830,950),new(675,950),new(650,875),new(400,625)};
    public static readonly Vector2[] CisternGround={new(340,580),new(530,330),new(960,265),new(1310,375),new(1410,570),new(1330,790),new(1050,900),new(480,875),new(340,715)};
    public static readonly Vector2[] PumpBasin=Navigation.Expand(new Vector2[]{new(604,475),new(758,400),new(916,473),new(768,581)},20);
    public static readonly Vector2[] CisternBasin=Navigation.Expand(new Vector2[]{new(537,515),new(803,390),new(1014,523),new(756,683)},20);
    public static string Name(string id)=>id switch {Court=>"Den dränkta förgården",Lodge=>"Väktarnas logement",Pump=>"Pumphuset",_=>"Den sänkta cisternen"};
    public static bool Known(string id)=>id is Court or Lodge or Pump or Cistern;
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
    public int LayoutVersion=1;
    public string Current=PortRooms.Court;
    public bool KeyTaken,DoorOpen,CacheTaken,PressureReleased,WaterLowered,ShortcutOpen,RelicTaken;
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
    [JsonIgnore] public string RoomName=>PortRooms.Name(Rooms!.Current);
    [JsonIgnore] public string RoomGoal=>!Rooms!.KeyTaken?"Hitta väktarens nyckel":!Rooms.DoorOpen?"Lås upp logementet":!Rooms.CacheTaken?"Sök fyndet i logementet":!Rooms.WaterLowered?Rooms.Current==PortRooms.Pump?!Rooms.PressureReleased?"Släpp ut pumpens övertryck":"Vrid pumpens matarhjul":"Följ ritningen till pumphuset":Rooms.Current==PortRooms.Cistern?!Rooms.RelicTaken?"Undersök cisternens altare":!Rooms.ShortcutOpen?"Öppna genvägen till förgården":"Genvägen är öppen":"Cisternen är frilagd · utforska";
    [JsonIgnore] public Vector2 RoomObjective=>Rooms!.Current switch
    {
        PortRooms.Court=>!Rooms.KeyTaken?PortRooms.Key:Rooms.ShortcutOpen?RoomLinks.All[3].AtB:PortRooms.CourtDoor,
        PortRooms.Lodge=>!Rooms.CacheTaken?PortRooms.Cache:RoomLinks.All[1].AtA,
        PortRooms.Pump=>!Rooms.PressureReleased?PortRooms.Pressure:!Rooms.WaterLowered?PortRooms.Wheel:RoomLinks.All[2].AtA,
        _=>!Rooms.RelicTaken?PortRooms.Relic:RoomLinks.All[3].AtA
    };

    public static Combat NewRooms(Order order)
    {
        var game=NewAtland(order);game.Rooms=new(){LayoutVersion=2};game.Rooms.Rooms.Add(PortRooms.Pump,new());game.Rooms.Rooms.Add(PortRooms.Cistern,new());game.Enemies.Clear();game.Events.Clear();
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
        bool peaceful=Enemies.All(e=>e.Dead)&&Shots.All(s=>s.Reflected)&&Hazards.All(h=>h.Friendly);
        if(Rooms.Current==PortRooms.Pump&&(Near(PortRooms.Pressure)||Near(PortRooms.Wheel)))
        {
            if(!peaceful){Emit("room-notice",Player,"Säkra pumphuset först.");return;}
            if(Near(PortRooms.Pressure)&&!Rooms.PressureReleased)
            {Rooms.PressureReleased=true;Emit("campaign",Player,"Trycket faller. Den fastrostade matningen går nu att vrida. Vattenmärkena visar en trappa under cisternens yta.");}
            else if(Near(PortRooms.Wheel)&&!Rooms.WaterLowered)
            {
                if(!Rooms.PressureReleased){Emit("room-notice",Player,"Matarhjulet står under tryck. Öppna avlastningen först.");return;}
                Rooms.WaterLowered=true;Emit("campaign",Player,"Vattnet drar sig undan. En trappa löper ned längs cisternens vägg. Stegen är slitna av människor som gick här innan porten murades igen.");
                Emit("inscription",Player,"CISTERNEN ÄR FRILAGD");
            }
            Emit("checkpoint",Player);return;
        }
        if(Rooms.Current==PortRooms.Cistern&&Near(PortRooms.Relic)&&!Rooms.RelicTaken)
        {
            Rooms.RelicTaken=true;DropItem("atland-saber",PortRooms.Relic);
            Emit("campaign",Player,"En klinga vilar under saltkrusten. I bronsen intill är en väg ristad: från cisternen upp till förgårdens glömda lucka.");Emit("checkpoint",Player);return;
        }
        var link=RoomLinks.From(Rooms.Current).FirstOrDefault(l=>Near(l.At(Rooms.Current)));if(link is null)return;
        if(!peaceful){Emit("room-notice",Player,"Säkra rummet före passage.");return;}
        if(!RoomLinks.Open(Rooms,link))
        {
            if(link.Gate==PassageGate.Key&&Rooms.KeyTaken)
            {Rooms.DoorOpen=true;Emit("room-notice",Player,"Dörren är upplåst. Tryck E / B igen för passage.");}
            else if(link.Gate==PassageGate.Shortcut&&Rooms.Current==PortRooms.Cistern)
            {Rooms.ShortcutOpen=true;Emit("inscription",Player,"GENVÄGEN TILL FÖRGÅRDEN");}
            else{Emit("room-notice",Player,RoomLinks.LockedReason(link));return;}
            Emit("checkpoint",Player);return;
        }
        ChangeRoom(link.Other(Rooms.Current),link.Arrival(link.Other(Rooms.Current)));
    }

    private void ChangeRoom(string destination,Vector2 arrival)
    {
        Rooms!.Rooms[Rooms.Current].Enemies=Enemies;
        Rooms.Current=destination;var next=Rooms.Rooms[destination];
        Enemies=next.Enemies;next.Enemies=new();
        if(!next.Visited)
        {
            next.Visited=true;
            if(destination==PortRooms.Lodge){Spawn(EnemyKind.Guard,new(650,735));Spawn(EnemyKind.Gunner,new(530,585));}
            if(destination==PortRooms.Pump){Spawn(EnemyKind.Pikeman,new(610,700));Spawn(EnemyKind.Guard,new(1040,610));}
            // The optional cistern is deliberately quiet: discovery and reward between fights.
        }
        Player=arrival;Facing=MoveDirection=Vector2.UnitY;
        Shots.Clear();Hazards.Clear();AttackTime=AttackBuffer=DodgeTime=HitStop=Hurt=GuardTime=0;
        Guarding=Moving=false;
        UpdateRoomSight(true);
        Emit("region",Player,RoomName);Emit("checkpoint",Player);
    }

    public void ValidateRooms()
    {
        if(Rooms is null)return; // Original campaign saves have no room run.
        var r=Rooms;
        if(r.LayoutVersion==1)
        {
            if(r.Rooms is null||r.Rooms.Count!=2||!r.Rooms.ContainsKey(PortRooms.Court)||!r.Rooms.ContainsKey(PortRooms.Lodge))throw new System.IO.InvalidDataException("Ogiltig äldre rumskarta");
            r.Rooms.Add(PortRooms.Pump,new());r.Rooms.Add(PortRooms.Cistern,new());r.LayoutVersion=2;
        }
        if(!InCampaign||CampaignStage!=0||Region!=Region.Atland||CampaignFinished||!PortRooms.Known(r.Current)
            ||r.Rooms is null||r.LayoutVersion!=2||r.Rooms.Count!=4||new[]{PortRooms.Court,PortRooms.Lodge,PortRooms.Pump,PortRooms.Cistern}.Any(id=>!r.Rooms.ContainsKey(id))
            ||r.Rooms.Any(p=>p.Value is null||p.Value.Enemies is null||p.Value.Enemies.Count>100||p.Value.Explored is null||p.Value.Explored.Length!=RoomSight.Bytes||(!p.Value.Visited&&p.Value.Explored.Any(b=>b!=0)))
            ||!r.Rooms[PortRooms.Court].Visited||!r.Rooms[r.Current].Visited||r.Rooms[r.Current].Enemies.Count!=0
            ||(r.DoorOpen&&!r.KeyTaken)||(r.Rooms[PortRooms.Lodge].Visited&&!r.DoorOpen)||(r.CacheTaken&&!r.Rooms[PortRooms.Lodge].Visited)
            ||(r.Rooms[PortRooms.Pump].Visited&&!r.CacheTaken)||(r.PressureReleased&&!r.Rooms[PortRooms.Pump].Visited)
            ||(r.WaterLowered&&!r.PressureReleased)||(r.Rooms[PortRooms.Cistern].Visited&&!r.WaterLowered)
            ||((r.ShortcutOpen||r.RelicTaken)&&!r.Rooms[PortRooms.Cistern].Visited))
            throw new System.IO.InvalidDataException("Ogiltig rumsexpedition");
        var actors=Enemies.Concat(r.Rooms.Values.SelectMany(s=>s.Enemies)).ToArray();
        if(actors.Any(e=>e is null||e.Id<1||!Enum.IsDefined(e.Kind)||!float.IsFinite(e.Health)||!float.IsFinite(e.MaxHealth)||e.MaxHealth<=0||e.Health>e.MaxHealth
                ||!float.IsFinite(e.Position.X)||!float.IsFinite(e.Position.Y))||actors.Select(e=>e.Id).Distinct().Count()!=actors.Length
            ||actors.Any(e=>e.Id>=NextId)||r.Rooms.Values.Any(s=>!s.Visited&&s.Enemies.Count!=0))
            throw new System.IO.InvalidDataException("Ogiltiga rumsvakter");
        if(Inventory.Drops.Any(d=>d.Room!=""&&!PortRooms.Known(d.Room)))throw new System.IO.InvalidDataException("Ogiltigt fyndrum");
    }
}
