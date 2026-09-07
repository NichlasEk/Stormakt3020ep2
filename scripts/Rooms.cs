using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public static class PortRooms
{
    public const string Court="court",Lodge="lodge",Pump="pump",Cistern="cistern",Gallery="gallery",Chamber="chamber";
    public static readonly Vector2 CourtDoor=new(490,250),LodgeDoor=new(975,435);
    public static readonly Vector2[] CourtGround={new(400,285),new(640,176),new(1280,370),new(1340,590),new(800,956),new(225,700),new(230,440)};
    public static readonly Vector2 Key=new(520,620),Cache=new(542,650);
    public static readonly Vector2 Pressure=new(1170,590),Wheel=new(405,570),Relic=new(365,665);
    public static readonly Vector2[] PumpGround={new(395,420),new(990,270),new(1410,485),new(1330,610),new(850,875),new(830,950),new(675,950),new(650,875),new(400,625)};
    public static readonly Vector2[] CisternGround={new(340,580),new(530,330),new(960,265),new(1310,375),new(1410,570),new(1330,790),new(1050,900),new(480,875),new(340,715)};
    public static readonly Vector2[] PumpBasin=Navigation.Expand(new Vector2[]{new(604,475),new(758,400),new(916,473),new(768,581)},20);
    public static readonly Vector2[] CisternBasin=Navigation.Expand(new Vector2[]{new(537,515),new(803,390),new(1014,523),new(756,683)},20);
    public static readonly string[] Ids={Court,Lodge,Pump,Cistern,Gallery,Chamber};
    public static readonly Vector2 Witness=new(785,735),OathExit=new(810,330);
    public static readonly Vector2[] GalleryGround={new(245,400),new(340,380),new(550,300),new(800,250),new(1130,380),new(1270,410),new(1390,560),new(1430,640),new(970,930),new(600,950),new(110,645),new(170,535)};
    public static readonly Vector2[] OathGround={new(240,410),new(460,315),new(690,300),new(970,300),new(1240,440),new(1430,525),new(1425,650),new(1040,890),new(980,932),new(570,932),new(70,610),new(70,520)};
    public static readonly Vector2[] Lectern=Navigation.Expand(new Vector2[]{new(730,655),new(789,620),new(880,665),new(818,706)},10);
    public static readonly Vector2[] Pillars={new(570,620),new(1000,640)};
    public static readonly Vector2[][] OathObstacles=Pillars.Select(p=>new[]{p+new Vector2(-52,-8),p+new Vector2(0,-38),p+new Vector2(52,-8),p+new Vector2(0,30)}).ToArray();
    public static string Name(string id)=>id switch {Court=>"Den dränkta förgården",Lodge=>"Väktarnas logement",Pump=>"Pumphuset",Cistern=>"Den sänkta cisternen",Gallery=>"Vittnesgalleriet",_=>"Edskammaren"};
    public static bool Known(string id)=>Array.IndexOf(Ids,id)>=0;
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
    public bool KeyTaken,DoorOpen,CacheTaken,PressureReleased,WaterLowered,ShortcutOpen,RelicTaken,WitnessRead,OathDefeated,Completed;
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
    [JsonIgnore] public string RoomGoal
    {
        get
        {
            var r=Rooms!;
            if(r.Completed)return "Atlands inre port är öppen";
            return r.Current switch
            {
                PortRooms.Court=>!r.KeyTaken?"Hitta väktarens nyckel":!r.DoorOpen?"Lås upp logementet":"Fortsätt genom logementet",
                PortRooms.Lodge=>!r.CacheTaken?"Sök fyndet i logementet":"Följ ritningen till pumphuset",
                PortRooms.Pump=>!r.PressureReleased?"Släpp ut pumpens övertryck":!r.WaterLowered?"Vrid pumpens matarhjul":"Fortsätt till Vittnesgalleriet",
                PortRooms.Cistern=>!r.RelicTaken?"Undersök cisternens altare":!r.ShortcutOpen?"Öppna genvägen":"Återvänd via genvägen",
                PortRooms.Gallery=>!r.WitnessRead?"Läs galleriets vittnesbok":"Fortsätt till Edskammaren",
                _=>r.OathDefeated?"Öppna den inre porten":Enemies.Any(e=>e.Kind==EnemyKind.OathGuardian&&e.State==3)?"Skölden är nere · angrip":"Locka ruset mot en edspelare"
            };
        }
    }
    [JsonIgnore] public Vector2 RoomObjective=>Rooms!.Current switch
    {
        PortRooms.Court=>!Rooms.KeyTaken?PortRooms.Key:PortRooms.CourtDoor,
        PortRooms.Lodge=>!Rooms.CacheTaken?PortRooms.Cache:RoomLinks.All[1].AtA,
        PortRooms.Pump=>!Rooms.PressureReleased?PortRooms.Pressure:!Rooms.WaterLowered?PortRooms.Wheel:RoomLinks.All[4].AtA,
        PortRooms.Gallery=>!Rooms.WitnessRead?PortRooms.Witness:RoomLinks.All[5].AtA,
        PortRooms.Chamber=>Rooms.OathDefeated?PortRooms.OathExit:new Vector2(800,550),
        _=>!Rooms.RelicTaken?PortRooms.Relic:RoomLinks.All[3].AtA
    };

    public static Combat NewRooms(Order order)
    {
        var game=NewAtland(order);game.IntroPlayed=true;game.Rooms=new(){LayoutVersion=3};foreach(var id in PortRooms.Ids.Skip(2))game.Rooms.Rooms.Add(id,new());game.Enemies.Clear();game.Events.Clear();
        game.Spawn(EnemyKind.Guard,new(650,625));game.Player=new(768,805);
        game.UpdateRoomSight(true);
        game.Emit("region",game.Player,game.RoomName);game.Emit("radio",game.Player,"rooms-entry");game.Emit("campaign",game.Player,"Väktaren bar nyckeln till logementet. Hans packning ligger kvar i förgården.");
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
            {Rooms.PressureReleased=true;Emit("room-sound",Player,"pump-pressure");Emit("campaign",Player,"Trycket faller. Den fastrostade matningen går nu att vrida. Vattenmärkena visar en trappa under cisternens yta.");}
            else if(Near(PortRooms.Wheel)&&!Rooms.WaterLowered)
            {
                if(!Rooms.PressureReleased){Emit("room-notice",Player,"Matarhjulet står under tryck. Öppna avlastningen först.");return;}
                Rooms.WaterLowered=true;Emit("room-sound",Player,"pump-drain");Emit("radio",Player,"rooms-drained");Emit("campaign",Player,"Vattnet drar sig undan. En trappa löper ned längs cisternens vägg. Stegen är slitna av människor som gick här innan porten murades igen.");
                Emit("inscription",Player,"CISTERNEN ÄR FRILAGD");
            }
            Emit("checkpoint",Player);return;
        }
        if(Rooms.Current==PortRooms.Cistern&&Near(PortRooms.Relic)&&!Rooms.RelicTaken)
        {
            Rooms.RelicTaken=true;Emit("radio",Player,"rooms-cistern");DropItem("atland-saber",PortRooms.Relic);
            Emit("campaign",Player,"En klinga vilar under saltkrusten. I bronsen intill är en väg ristad: från cisternen upp till förgårdens glömda lucka.");Emit("checkpoint",Player);return;
        }
        if(Rooms.Current==PortRooms.Gallery&&Near(PortRooms.Witness)&&!Rooms.WitnessRead)
        {
            Rooms.WitnessRead=true;Emit("radio",Player,"rooms-witness");
            Emit("campaign",Player,"Vittnesboken: Ingen ed får brytas av järn. Endast stenen som bevittnade eden kan lösa den. Reliefen visar en väktare som rusar med skölden mot en ringmärkt pelare.");
            Emit("checkpoint",Player);return;
        }
        if(Rooms.Current==PortRooms.Chamber&&Near(PortRooms.OathExit)&&Rooms.OathDefeated&&!Rooms.Completed)
        {
            Rooms.Completed=true;Emit("room-sound",Player,"stone-door");Emit("radio",Player,"rooms-port");Emit("inscription",Player,"ATLANDS INRE PORT ÄR ÖPPEN");
            Emit("campaign",Player,"Bakom porten ligger Minnets arkivs nedre trappa. Rötter grövre än skeppsmaster har sprängt muren. På kartans baksida finns samma kust igen — men havet ligger åt fel håll. Första rutten är säkrad. Du kan återvända och hämta kvarlämnade fynd.");
            Emit("checkpoint",Player);return;
        }
        var link=RoomLinks.From(Rooms.Current).FirstOrDefault(l=>Near(l.At(Rooms.Current)));if(link is null)return;
        if(!peaceful){Emit("room-notice",Player,"Säkra rummet före passage.");return;}
        if(!RoomLinks.Open(Rooms,link))
        {
            if(link.Gate==PassageGate.Key&&Rooms.KeyTaken)
            {Rooms.DoorOpen=true;Emit("room-sound",Player,"stone-door");Emit("room-notice",Player,"Dörren är upplåst. Tryck E / B igen för passage.");}
            else if(link.Gate==PassageGate.Shortcut&&Rooms.Current==PortRooms.Cistern)
            {Rooms.ShortcutOpen=true;Emit("room-sound",Player,"stone-door");Emit("inscription",Player,"GENVÄGEN TILL FÖRGÅRDEN");}
            else{Emit("room-notice",Player,RoomLinks.LockedReason(link));return;}
            Emit("checkpoint",Player);return;
        }
        ChangeRoom(link.Other(Rooms.Current),link.Arrival(link.Other(Rooms.Current)));
    }

    private void ChangeRoom(string destination,Vector2 arrival)
    {
        Rooms!.Rooms[Rooms.Current].Enemies=Enemies;
        Rooms.Current=destination;var next=Rooms.Rooms[destination];
        Enemies=next.Enemies;next.Enemies=new();bool firstVisit=!next.Visited;
        if(!next.Visited)
        {
            next.Visited=true;
            if(destination==PortRooms.Lodge){Spawn(EnemyKind.Guard,new(650,735));Spawn(EnemyKind.Gunner,new(530,585));}
            if(destination==PortRooms.Pump){Spawn(EnemyKind.Pikeman,new(610,700));Spawn(EnemyKind.Guard,new(1040,610));}
            if(destination==PortRooms.Chamber)Spawn(EnemyKind.OathGuardian,new(800,515));
            // The optional cistern is deliberately quiet: discovery and reward between fights.
        }
        Player=arrival;Facing=MoveDirection=Vector2.UnitY;
        Shots.Clear();Hazards.Clear();AttackTime=AttackBuffer=DodgeTime=HitStop=Hurt=GuardTime=0;
        Guarding=Moving=false;
        UpdateRoomSight(true);
        Emit("region",Player,RoomName);if(firstVisit&&destination==PortRooms.Pump)Emit("radio",Player,"rooms-pump");Emit("checkpoint",Player);
    }

    public bool RoomRadioRelevant(string id)=>id switch
    {
        "rooms-entry"=>InRooms&&!Rooms!.DoorOpen,
        "rooms-pump"=>Rooms?.Current==PortRooms.Pump&&!Rooms.WaterLowered,
        "rooms-drained"=>InRooms&&!Rooms!.WitnessRead,
        "rooms-rush"=>Rooms?.Current==PortRooms.Chamber&&!Rooms.OathDefeated,
        _=>id.StartsWith("rooms-",StringComparison.Ordinal)&&InRooms
    };

    public void ValidateRooms()
    {
        if(Rooms is null)return; // Original campaign saves have no room run.
        IntroPlayed=true; // Never replay the old quay/sigil briefing when resuming a room run.
        var r=Rooms;
        if(r.LayoutVersion==1)
        {
            if(r.Rooms is null||r.Rooms.Count!=2||!r.Rooms.ContainsKey(PortRooms.Court)||!r.Rooms.ContainsKey(PortRooms.Lodge))throw new System.IO.InvalidDataException("Ogiltig äldre rumskarta");
            r.Rooms.Add(PortRooms.Pump,new());r.Rooms.Add(PortRooms.Cistern,new());r.LayoutVersion=2;
        }
        if(r.LayoutVersion==2)
        {
            if(r.Rooms is null||r.Rooms.Count!=4||PortRooms.Ids.Take(4).Any(id=>!r.Rooms.ContainsKey(id)))throw new System.IO.InvalidDataException("Ogiltig äldre vattenkarta");
            r.Rooms.Add(PortRooms.Gallery,new());r.Rooms.Add(PortRooms.Chamber,new());r.LayoutVersion=3;
        }
        if(!InCampaign||CampaignStage!=0||Region!=Region.Atland||CampaignFinished||!PortRooms.Known(r.Current)
            ||r.Rooms is null||r.LayoutVersion!=3||r.Rooms.Count!=6||PortRooms.Ids.Any(id=>!r.Rooms.ContainsKey(id))
            ||r.Rooms.Any(p=>p.Value is null||p.Value.Enemies is null||p.Value.Enemies.Count>100||p.Value.Explored is null||p.Value.Explored.Length!=RoomSight.Bytes||(!p.Value.Visited&&p.Value.Explored.Any(b=>b!=0)))
            ||!r.Rooms[PortRooms.Court].Visited||!r.Rooms[r.Current].Visited||r.Rooms[r.Current].Enemies.Count!=0
            ||(r.DoorOpen&&!r.KeyTaken)||(r.Rooms[PortRooms.Lodge].Visited&&!r.DoorOpen)||(r.CacheTaken&&!r.Rooms[PortRooms.Lodge].Visited)
            ||(r.Rooms[PortRooms.Pump].Visited&&!r.CacheTaken)||(r.PressureReleased&&!r.Rooms[PortRooms.Pump].Visited)
            ||(r.WaterLowered&&!r.PressureReleased)||(r.Rooms[PortRooms.Cistern].Visited&&!r.WaterLowered)
            ||(r.Rooms[PortRooms.Gallery].Visited&&!r.WaterLowered)||(r.WitnessRead&&!r.Rooms[PortRooms.Gallery].Visited)
            ||(r.Rooms[PortRooms.Chamber].Visited&&!r.WitnessRead)||(r.OathDefeated&&!r.Rooms[PortRooms.Chamber].Visited)||(r.Completed&&!r.OathDefeated)
            ||((r.ShortcutOpen||r.RelicTaken)&&!r.Rooms[PortRooms.Cistern].Visited))
            throw new System.IO.InvalidDataException("Ogiltig rumsexpedition");
        var actors=Enemies.Concat(r.Rooms.Values.SelectMany(s=>s.Enemies)).ToArray();
        if(actors.Any(e=>e is null||e.Id<1||!Enum.IsDefined(e.Kind)||!float.IsFinite(e.Health)||!float.IsFinite(e.MaxHealth)||e.MaxHealth<=0||e.Health>e.MaxHealth
                ||!float.IsFinite(e.Position.X)||!float.IsFinite(e.Position.Y))||actors.Select(e=>e.Id).Distinct().Count()!=actors.Length
            ||actors.Any(e=>e.Id>=NextId)||r.Rooms.Values.Any(s=>!s.Visited&&s.Enemies.Count!=0))
            throw new System.IO.InvalidDataException("Ogiltiga rumsvakter");
        var guardians=actors.Where(e=>e.Kind==EnemyKind.OathGuardian).ToArray();
        if(guardians.Any(e=>e.State<0||e.State>4||!float.IsFinite(e.Timer)||!float.IsFinite(e.Cooldown)
            ||!float.IsFinite(e.Facing.X)||!float.IsFinite(e.Facing.Y)||!float.IsFinite(e.LockedAim.X)||!float.IsFinite(e.LockedAim.Y))
            ||(r.Rooms[PortRooms.Chamber].Visited&&(guardians.Length!=1||guardians[0].Dead!=r.OathDefeated))
            ||(!r.Rooms[PortRooms.Chamber].Visited&&guardians.Length!=0))throw new System.IO.InvalidDataException("Ogiltig edsväktare");
        if(Inventory.Drops.Any(d=>d.Room!=""&&!PortRooms.Known(d.Room)))throw new System.IO.InvalidDataException("Ogiltigt fyndrum");
    }
}
