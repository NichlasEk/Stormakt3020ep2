using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;

// Paintings remain in their own 1536x1024 coordinate system. Simulation uses the
// current room's frame; a frame change translates every live object, never time.
public static class ConnectedWorld
{
    public static readonly string[] RoomIds=PortRooms.Ids.Concat(Regiment.Ids).Concat(Mine.Ids).ToArray();
    public const int Revision=4;
    public static bool Painted(RoomLink link)=>true;
    public static float MouthWidth(RoomLink link)=>link.Id switch{"lodge"=>.8f,"pump" or "cistern"=>.75f,"archive"=>1.15f,"gallery"=>.42f,"shortcut"=>.38f,"chamber"=>.42f,_=>.36f};
    public static Vector2 Origin(string id)=>Mine.Known(id)?Mine.Origin(id):Regiment.Known(id)?Regiment.Origin(id):id switch
    {
        PortRooms.Court=>new(0,0),PortRooms.Lodge=>new(2100,482.5f),PortRooms.Pump=>new(4200,1275),
        PortRooms.Cistern=>new(2100,1950),PortRooms.Gallery=>new(6300,1515),PortRooms.Chamber=>new(8400,2005),
        PortRooms.Archive=>new(10500,2365),PortRooms.Roots=>new(12600,2795),_=>new(14700,3172.5f)
    };
    public static Vector2[] Ground(string id)=>Mine.Known(id)?Mine.Ground(id):Regiment.Known(id)?Regiment.Ground(id):id switch
    {PortRooms.Court=>PortRooms.CourtGround,PortRooms.Lodge=>JourneyLayout.WarehouseGround,PortRooms.Pump=>PortRooms.PumpGround,PortRooms.Cistern=>PortRooms.CisternGround,PortRooms.Gallery=>PortRooms.GalleryGround,PortRooms.Chamber=>PortRooms.OathGround,PortRooms.Archive=>ArchiveRoom.Ground,PortRooms.Roots=>Rootway.Ground,_=>Rootway.GroveGround};
    public static Vector2[][] Obstacles(string id)=>Mine.Known(id)?Mine.Obstacles(id):Regiment.Known(id)?Regiment.Obstacles(id):id switch
    {PortRooms.Lodge=>new[]{Navigation.Expand(JourneyLayout.WarehouseObstacle,22)},PortRooms.Pump=>new[]{PortRooms.PumpBasin},PortRooms.Cistern=>new[]{PortRooms.CisternBasin},PortRooms.Gallery=>new[]{PortRooms.Lectern},PortRooms.Chamber=>PortRooms.OathObstacles,PortRooms.Archive=>new[]{ArchiveRoom.Table},PortRooms.Grove=>new[]{Rootway.Slab},_=>Array.Empty<Vector2[]>()};
    public static Vector2[] Route(RoomLink l)
    {
        if(l.Id=="mine-entry")return new[]{Origin(l.A)+new Vector2(1260,300),Origin(l.A)+new Vector2(1260,170),Origin(l.A)+new Vector2(1260,-80),Origin(l.B)+new Vector2(-60,350),Origin(l.B)+new Vector2(125,420),Origin(l.B)+new Vector2(230,480)};
        if(l.Id is "mine-bellows" or "mine-coolway")return new[]{Origin(l.A)+new Vector2(1360,510),Origin(l.A)+new Vector2(1450,410),Origin(l.A)+new Vector2(1600,350),Origin(l.B)+new Vector2(-60,350),Origin(l.B)+new Vector2(130,420),Origin(l.B)+new Vector2(240,480)};
        if(l.Id=="mine-return")return new[]{Origin(l.A)+new Vector2(720,900),Origin(l.A)+new Vector2(720,1060),new Vector2(34820,8500),new Vector2(30880,8500),Origin(l.B)+new Vector2(780,1060),Origin(l.B)+new Vector2(780,900)};
        if(l.Id=="regiment")return new[]{Origin(l.A)+new Vector2(800,830),Origin(l.A)+new Vector2(800,1030),new Vector2(16400,4450),Origin(l.B)+new Vector2(0,450),Origin(l.B)+new Vector2(180,465)};
        if(l.Id=="barracks")return new[]{Origin(l.A)+new Vector2(1400,550),Origin(l.A)+new Vector2(1590,570),Origin(l.B)+new Vector2(-60,360),Origin(l.B)+new Vector2(120,400),Origin(l.B)+new Vector2(230,470)};
        if(l.Id=="flags")return new[]{Origin(l.A)+new Vector2(1400,490),Origin(l.A)+new Vector2(1510,405),Origin(l.A)+new Vector2(1640,365),Origin(l.B)+new Vector2(-60,390),Origin(l.B)+new Vector2(130,435)};
        if(l.Id=="parade")return new[]{Origin(l.A)+new Vector2(1400,535),Origin(l.A)+new Vector2(1590,530),Origin(l.B)+new Vector2(-60,320),Origin(l.B)+new Vector2(100,375),Origin(l.B)+new Vector2(230,450)};
        if(l.Id=="regiment-quay")return new[]{Origin(l.A)+new Vector2(1380,495),Origin(l.A)+new Vector2(1470,395),Origin(l.A)+new Vector2(1630,330),Origin(l.B)+new Vector2(-60,320),Origin(l.B)+new Vector2(135,375),Origin(l.B)+new Vector2(250,465)};
        if(l.Id=="retreat")return new[]{Origin(l.A)+new Vector2(800,945),Origin(l.A)+new Vector2(800,1060),new Vector2(21650,6000),new Vector2(15500,6000),Origin(l.B)+new Vector2(800,1030),Origin(l.B)+new Vector2(800,830)};
        if(l.Id=="roots")return new[]{Origin(l.A)+new Vector2(1390,545),Origin(l.A)+new Vector2(1490,415),Origin(l.A)+new Vector2(1610,355),Origin(l.B)+new Vector2(-60,335),Origin(l.B)+new Vector2(105,415),Origin(l.B)+new Vector2(180,495)};
        if(l.Id=="grove")return new[]{Origin(l.A)+new Vector2(1390,565),Origin(l.A)+new Vector2(1470,455),Origin(l.A)+new Vector2(1610,385),Origin(l.B)+new Vector2(-60,315),Origin(l.B)+new Vector2(185,425),Origin(l.B)+new Vector2(280,505)};
        if(l.Id=="chamber")return new[]{Origin(l.A)+new Vector2(1220,460),Origin(l.A)+new Vector2(1280,380),Origin(l.A)+new Vector2(1580,260),Origin(l.B)+new Vector2(-60,350),Origin(l.B)+new Vector2(245,390),Origin(l.B)+new Vector2(335,470)};
        if(l.Id=="lodge")return new[]{Origin(l.A)+new Vector2(500,290),Origin(l.A)+new Vector2(500,150),Origin(l.A)+new Vector2(500,-110),Origin(l.B)+new Vector2(1080,-100),Origin(l.B)+new Vector2(1030,260),Origin(l.B)+new Vector2(990,350),Origin(l.B)+new Vector2(925,470)};
        if(l.Id=="pump")return new[]{Origin(l.A)+new Vector2(760,870),Origin(l.A)+new Vector2(760,1010),new Vector2(4000,1690),Origin(l.B)+new Vector2(1080,50),Origin(l.B)+new Vector2(1020,220),Origin(l.B)+new Vector2(960,340)};
        if(l.Id=="cistern")return new[]{Origin(l.A)+new Vector2(760,880),Origin(l.A)+new Vector2(760,1010),new Vector2(4700,2480),Origin(l.B)+new Vector2(1060,30),Origin(l.B)+new Vector2(995,180),Origin(l.B)+new Vector2(975,315)};
        if(l.Id=="shortcut")return new[]{Origin(l.A)+new Vector2(1360,520),Origin(l.A)+new Vector2(1480,390),Origin(l.A)+new Vector2(1590,310),new Vector2(3900,2750),new Vector2(3900,3300),new Vector2(700,3400),new Vector2(-200,1500),Origin(l.B)+new Vector2(170,710),Origin(l.B)+new Vector2(330,660)};
        if(l.Id=="gallery")return new[]{Origin(l.A)+new Vector2(1350,490),Origin(l.A)+new Vector2(1430,370),Origin(l.A)+new Vector2(1590,310),Origin(l.B)+new Vector2(-60,330),Origin(l.B)+new Vector2(245,385),Origin(l.B)+new Vector2(340,470)};
        if(l.Id=="archive")return new[]{Origin(l.A)+new Vector2(810,360),Origin(l.A)+new Vector2(810,230),Origin(l.A)+new Vector2(810,-80),Origin(l.B)+new Vector2(-80,320),Origin(l.B)+new Vector2(100,435),Origin(l.B)+new Vector2(220,520)};
        var a=Ground(l.A).MaxBy(p=>p.X);var b=Ground(l.B).MinBy(p=>p.X);
        return new[]{Origin(l.A)+Vector2.Lerp(a,new(780,620),.13f),Origin(l.A)+a+new Vector2(130,65),Origin(l.B)+b-new Vector2(130,65),Origin(l.B)+Vector2.Lerp(b,new(780,620),.13f)};
    }
    public static Vector2 Center(RoomLink l){var p=Route(l);return Painted(l)?(p[0]+p[1])/2:(p[1]+p[2])/2;}
    public static Vector2 Side(Vector2 a,Vector2 b){var axis=Vector2.Normalize(new Vector2(b.X-a.X,(b.Y-a.Y)*2));return new Vector2(-axis.Y*100,axis.X*50);}
    public static Vector2[] Passage(Vector2 a,Vector2 b){var side=Side(a,b);var along=Vector2.Normalize(b-a)*3;a-=along;b+=along;return new[]{a-side,b-side,b+side,a+side};}
    public static Vector2 Hinge(RoomLink l){var p=Route(l);return Center(l)+(Painted(l)?Side(p[0],p[1])*MouthWidth(l):Side(p[1],p[2]));}
    public static Vector2 Tip(RoomLink l,float open)
    {var h=Hinge(l);var d=(Center(l)-h)*2;float a=open*MathF.PI/2;return h+new Vector2(d.X*MathF.Cos(a)-d.Y*2*MathF.Sin(a),d.X*.5f*MathF.Sin(a)+d.Y*MathF.Cos(a));}
    public static readonly Dictionary<string,Vector2[][]> Floors=RoomIds.ToDictionary(id=>id,id=>new[]{Ground(id).Select(p=>p+Origin(id)).ToArray()});
    public static Vector2[] PassageFor(RoomLink l,int segment)
    {var route=Route(l);var a=route[segment-1];var b=route[segment];var side=Side(a,b)*(Painted(l)&&(segment==1||segment==route.Length-1)?(segment==1?MouthWidth(l):.4f):1);var along=Vector2.Normalize(b-a)*3;return new[]{a-along-side,b+along-side,b+along+side,a-along+side};}
    public static readonly Dictionary<string,Vector2[][]> Corridors=RoomLinks.All.ToDictionary(l=>l.Id,l=>Enumerable.Range(1,Route(l).Length-1).Select(i=>PassageFor(l,i)).ToArray());
    public sealed class Shape
    {
        public readonly Vector2[] Polygon;public readonly Vector2 Min,Max;
        public Shape(Vector2[] p){Polygon=p;Min=new(p.Min(v=>v.X),p.Min(v=>v.Y));Max=new(p.Max(v=>v.X),p.Max(v=>v.Y));}
        public bool Contains(Vector2 p)=>p.X>=Min.X&&p.Y>=Min.Y&&p.X<=Max.X&&p.Y<=Max.Y&&Navigation.Contains(Polygon,p);
    }
    public static readonly Shape[] FloorShapes=Floors.Values.SelectMany(v=>v).Concat(Corridors.Values.SelectMany(v=>v)).Select(p=>new Shape(p)).ToArray();
    public static readonly Vector2[][] ArchJambs={
        DoorTrialLayout.Bar(Origin(PortRooms.Gallery)+new Vector2(1085,328),Origin(PortRooms.Gallery)+new Vector2(1202,398),21),
        DoorTrialLayout.Bar(Origin(PortRooms.Gallery)+new Vector2(1290,433),Origin(PortRooms.Gallery)+new Vector2(1536,585),21),
        DoorTrialLayout.Bar(Origin(PortRooms.Chamber)+new Vector2(0,560),Origin(PortRooms.Chamber)+new Vector2(200,440),18),
        DoorTrialLayout.Bar(Origin(PortRooms.Chamber)+new Vector2(305,398),Origin(PortRooms.Chamber)+new Vector2(460,315),18)
    };
    public static readonly Vector2[][] Solids=RoomIds.SelectMany(id=>Obstacles(id).Select(poly=>poly.Select(p=>p+Origin(id)).ToArray())).Concat(ArchJambs).ToArray();
}

public sealed partial class Combat
{
    [JsonIgnore] public bool InConnectedWorld=>InRooms&&!InDoorTrial&&Rooms!.Connected;
    [JsonIgnore] public Vector2 WorldOrigin=>InConnectedWorld?ConnectedWorld.Origin(Rooms!.Current):Vector2.Zero;
    [JsonIgnore] public Vector2 ConnectedObjective
    {
        get
        {
            if(InMine)return MineObjective;
            if(InRegiment)return RegimentObjective;
            var r=Rooms!;
            Vector2 Gate(string id)=>ConnectedWorld.Center(RoomLinks.All.First(l=>l.Id==id))-WorldOrigin;
            return r.Current switch
            {
                PortRooms.Court=>!r.KeyTaken&&!r.DoorOpen?PortRooms.Key:Gate("lodge"),
                PortRooms.Lodge=>!r.CacheTaken?PortRooms.Cache:Gate("pump"),
                PortRooms.Pump=>!r.PressureReleased?PortRooms.Pressure:!r.WaterLowered?PortRooms.Wheel:Gate("gallery"),
                PortRooms.Cistern=>!r.RelicTaken?PortRooms.Relic:Gate("shortcut"),
                PortRooms.Gallery=>!r.WitnessRead?PortRooms.Witness:Gate("chamber"),
                PortRooms.Chamber=>!r.OathDefeated?new(800,550):!r.Completed?PortRooms.OathExit:Gate("archive"),
                PortRooms.Archive=>!r.ArchiveRead||ArchiveChoice==0?ArchiveRoom.Desk:!r.ArchiveSecured?ArchiveRoom.Seal:Gate("roots"),
                PortRooms.Roots=>!r.RootGateOpen?Rootway.Winch:Gate("grove"),_=>r.GroveSecured?Gate("regiment"):Rootway.Memorial
            };
        }
    }
    [JsonIgnore] public IEnumerable<Fighter> EncounterEnemies=>InConnectedWorld?Enemies.Where(e=>e.HomeRoom==Rooms!.Current||(!e.Dead&&Vector2.Distance(e.Position,Player)<500)):Enemies;
    public List<Fighter> ActorsInRoom(string id)=>InConnectedWorld?Enemies.Where(e=>e.HomeRoom==id).ToList():Rooms!.Current==id?Enemies:Rooms!.Rooms[id].Enemies;
    public void EnableConnectedWorld()
    {
        if(!InRooms||InDoorTrial||InConnectedWorld)return;
        var r=Rooms!;foreach(var id in Regiment.Ids.Concat(Mine.Ids))r.Rooms.TryAdd(id,new());r.LayoutVersion=7;r.Connected=true;r.ConnectionRevision=ConnectedWorld.Revision;
        foreach(var e in Enemies)e.HomeRoom=r.Current;
        foreach(var pair in r.Rooms)
        {
            var delta=ConnectedWorld.Origin(pair.Key)-WorldOrigin;
            foreach(var e in pair.Value.Enemies){e.HomeRoom=pair.Key;e.Position+=delta;e.LockedAim+=delta;Enemies.Add(e);}
            pair.Value.Enemies.Clear();
        }
        foreach(var d in Inventory.Drops.Where(d=>d.Room!=""))d.Position+=ConnectedWorld.Origin(d.Room)-WorldOrigin;
        foreach(var l in RoomLinks.All)r.Doors[l.Id]=new(){Locked=!RoomLinks.Open(r,l),TargetOpen=RoomLinks.Open(r,l),Openness=RoomLinks.Open(r,l)?1:0};
        UpdateRoomSight(true);
    }
    public static bool HasWorldDoor(RoomLink l)=>l.Gate is PassageGate.Key or PassageGate.Shortcut or PassageGate.Archive or PassageGate.RootGate or PassageGate.RegimentExit or PassageGate.RegimentShortcut or PassageGate.MineShortcut;
    private bool WorldGround(Vector2 world){foreach(var p in ConnectedWorld.FloorShapes)if(p.Contains(world))return true;return false;}
    [JsonIgnore] private int _solidStamp=int.MinValue;
    [JsonIgnore] private ConnectedWorld.Shape[] _worldSolids=Array.Empty<ConnectedWorld.Shape>();
    private ConnectedWorld.Shape[] CachedWorldSolids()
    {
        int stamp=17;foreach(var l in RoomLinks.All){var d=Rooms!.Doors[l.Id];stamp=unchecked(stamp*31+BitConverter.SingleToInt32Bits(d.Openness)+(RoomLinks.Open(Rooms,l)?1:0)+(d.Broken?7:0));}
        if(stamp!=_solidStamp){_solidStamp=stamp;_worldSolids=WorldSolids().Select(p=>new ConnectedWorld.Shape(p)).ToArray();}return _worldSolids;
    }
    private static bool Blocked(ConnectedWorld.Shape[] solids,Vector2 p){foreach(var o in solids)if(o.Contains(p))return true;return false;}
    public IEnumerable<Vector2[]> WorldSolids()
    {
        foreach(var p in ConnectedWorld.Solids)yield return p;
        foreach(var link in RoomLinks.All)
        {
            var d=Rooms!.Doors[link.Id];
            if(!RoomLinks.Open(Rooms,link))yield return DoorTrialLayout.Bar(ConnectedWorld.Hinge(link),ConnectedWorld.Tip(link,0),18);
            else if(HasWorldDoor(link)&&!d.Broken)yield return DoorTrialLayout.Bar(ConnectedWorld.Hinge(link),ConnectedWorld.Tip(link,d.Openness),15);
        }
    }
    private bool WorldWalkable(Vector2 p)=>WorldGround(p)&&!Blocked(CachedWorldSolids(),p);
    private bool WorldClear(Vector2 a,Vector2 b)
    {
        a+=WorldOrigin;b+=WorldOrigin;
        var obstacles=CachedWorldSolids();int n=Math.Max(1,(int)MathF.Ceiling(Vector2.Distance(a,b)/7));
        for(int i=0;i<=n;i++){var p=Vector2.Lerp(a,b,(float)i/n);if(!WorldGround(p)||Blocked(obstacles,p))return false;}
        return true;
    }
    private Vector2 WorldBound(Vector2 p)
    {
        var world=p+WorldOrigin;if(WorldWalkable(world))return p;
        // Nearest legal point, used for old-save migration and small edge sliding.
        var obstacles=WorldSolids().ToArray();var best=world;float distance=float.MaxValue;
        foreach(var poly in ConnectedWorld.Floors.Values.SelectMany(v=>v).Concat(ConnectedWorld.Corridors.Values.SelectMany(v=>v)))
        {
            var q=Navigation.Clamp(poly,obstacles,world);
            if(WorldWalkable(q)&&Vector2.DistanceSquared(q,world)<distance){distance=Vector2.DistanceSquared(q,world);best=q;}
        }
        return distance<float.MaxValue?best-WorldOrigin:p;
    }
    private Vector2 WorldNext(Vector2 from,Vector2 target)
    {
        if(WorldClear(from,target))return target;
        // Local visibility graph includes bends, floor corners and solid islands.
        // Only nearby nodes are needed; far-away guards sleep until approached.
        var nodes=new List<Vector2>{from,target};var world=from+WorldOrigin;
        foreach(var p in WorldSolids().Where(p=>p.Any(v=>Vector2.DistanceSquared(v,world)<850*850)))
            nodes.AddRange(Navigation.Expand(p,22).Select(v=>v-WorldOrigin).Where(OnWalkable));
        foreach(var l in RoomLinks.All)nodes.AddRange(ConnectedWorld.Route(l).Where(p=>Vector2.DistanceSquared(p,world)<850*850).Select(p=>p-WorldOrigin));
        var dist=Enumerable.Repeat(float.MaxValue,nodes.Count).ToArray();var prev=Enumerable.Repeat(-1,nodes.Count).ToArray();var used=new bool[nodes.Count];dist[0]=0;
        for(int pass=0;pass<nodes.Count;pass++)
        {int k=-1;for(int i=0;i<nodes.Count;i++)if(!used[i]&&(k<0||dist[i]<dist[k]))k=i;
            if(k<0||dist[k]==float.MaxValue||k==1)break;used[k]=true;
            for(int i=0;i<nodes.Count;i++)if(!used[i]){float d=dist[k]+Vector2.Distance(nodes[k],nodes[i]);if(d<dist[i]&&WorldClear(nodes[k],nodes[i])){dist[i]=d;prev[i]=k;}}
        }
        int next=1;if(prev[next]<0)return from;while(prev[next]>0)next=prev[next];return nodes[next];
    }
    private void StepConnectedWorld(Controls input,float dt)
    {
        var r=Rooms!;var world=Player+WorldOrigin;
        foreach(var l in RoomLinks.All)
        {
            var d=r.Doors[l.Id];if(!RoomLinks.Open(r,l)||d.Broken)continue;
            if(d.Locked){d.Locked=false;d.TargetOpen=true;}
            float next=Math.Clamp(d.Openness+(d.TargetOpen?1:-1)*dt*1.6f,0,1);if(next==d.Openness)continue;
            bool blocked=false;
            for(int i=1;i<=4;i++)if(Enemies.Where(e=>!e.Dead).Select(e=>e.Position+WorldOrigin).Append(world).Any(p=>DoorTrialLayout.Distance(p,ConnectedWorld.Hinge(l),ConnectedWorld.Tip(l,d.Openness+(next-d.Openness)*i/4))<31)){blocked=true;break;}
            if(!blocked)d.Openness=next;
        }
        foreach(var l in RoomLinks.From(r.Current))
        {
            if(!RoomLinks.Open(r,l))continue;string other=l.Other(r.Current);
            if(Navigation.Contains(ConnectedWorld.Ground(other),world-ConnectedWorld.Origin(other)))
            {EnterConnectedRoom(other);break;}
        }
        if(!input.Interact||_roomInteractHeld||Dead||AttackTime>0||DodgeTime>0)return;
        if(r.Current==Regiment.Farled&&Vector2.Distance(Player,Mine.Latch)<72)return;
        if(r.Current==Mine.Coolway&&!r.Mine.ShortcutOpen&&Vector2.Distance(Player,Mine.Shortcut)<72)return;
        // Release the adjacent archive seal before operating its unlocked leaf.
        if(r.Current==PortRooms.Chamber&&!r.Completed&&Vector2.Distance(Player,PortRooms.OathExit)<72)return;
        if(r.Current==Regiment.Flags&&!r.Regiment.ShortcutOpen&&Vector2.Distance(Player,Regiment.Shortcut)<72)return;
        if(r.Current==PortRooms.Archive&&!r.ArchiveSecured&&Vector2.Distance(Player,ArchiveRoom.Seal)<72)return;
        foreach(var l in RoomLinks.All)
        {
            if(Vector2.Distance(world,ConnectedWorld.Center(l))>120)continue;
            var d=r.Doors[l.Id];if(!HasWorldDoor(l)&&RoomLinks.Open(r,l))continue;
            if(!RoomLinks.Open(r,l))
            {
                if(l.Gate==PassageGate.RegimentShortcut&&r.Current==Regiment.Flags)r.Regiment.ShortcutOpen=true;
                else if(l.Gate==PassageGate.Key&&r.KeyTaken)r.DoorOpen=true;
                else if(l.Gate==PassageGate.Shortcut&&r.Current==PortRooms.Cistern)r.ShortcutOpen=true;
                else{Emit("room-notice",Player,RoomLinks.LockedReason(l));_roomInteractHeld=true;return;}
            }
            if(d.Broken)Emit("room-notice",Player,"Porten är sönderslagen.");
            else{d.Locked=false;d.TargetOpen=!d.TargetOpen;Emit("room-sound",Player,"door-creak");}
            _roomInteractHeld=true;Emit("checkpoint",Player);return;
        }
    }
    private void EnterConnectedRoom(string destination)
    {
        var r=Rooms!;var delta=WorldOrigin-ConnectedWorld.Origin(destination);Player+=delta;
        foreach(var e in Enemies){e.Position+=delta;e.LockedAim+=delta;}
        foreach(var s in Shots)s.Position+=delta;foreach(var h in Hazards)h.Position+=delta;
        foreach(var d in Inventory.Drops.Where(d=>d.Room!=""))d.Position+=delta;
        // Cues already emitted by this tick share the same simulation frame.
        for(int i=0;i<Events.Count;i++)Events[i]=Events[i] with {Position=Events[i].Position+delta};
        r.Current=destination;bool first=!r.Rooms[destination].Visited;r.Rooms[destination].Visited=true;
        if(first)
        {
            if(Mine.Known(destination))EnterMine(destination);
            if(Regiment.Known(destination))EnterRegiment(destination);
            if(destination==PortRooms.Lodge){Spawn(EnemyKind.Guard,new(650,735));Spawn(EnemyKind.Gunner,new(530,585));}
            if(destination==PortRooms.Pump){Spawn(EnemyKind.Pikeman,new(610,700));Spawn(EnemyKind.Guard,new(1040,610));Emit("radio",Player,"rooms-pump");}
            if(destination==PortRooms.Chamber)Spawn(EnemyKind.OathGuardian,new(800,515));
            if(destination==PortRooms.Roots){Emit("radio",Player,"roots-entry");Emit("cinematic",Player,"archive-gate");}
            if(destination==PortRooms.Grove)Emit("campaign",Player,GroveClue);
        }
        Events.Insert(0,new("world-frame",delta,RoomName));UpdateRoomSight(true);Emit("checkpoint",Player);
    }
    private void HitConnectedDoor(float range,float damage,float arc)
    {
        if(!InConnectedWorld)return;
        foreach(var l in RoomLinks.All)
        {
            if(l.Gate!=PassageGate.Key)continue;
            var d=Rooms!.Doors[l.Id];var at=(ConnectedWorld.Hinge(l)+ConnectedWorld.Tip(l,d.Openness))/2-WorldOrigin;
            if(d.Broken||Vector2.Distance(Player,at)>range+45||Vector2.Dot(Normal(at-Player,Facing),Facing)<arc)continue;
            d.Health=Math.Max(0,d.Health-damage);Emit("room-sound",at,d.Broken?"door-break":"door-hit");
            if(d.Broken){Rooms.DoorOpen=true;Emit("checkpoint",Player);}
        }
    }
    public void UpgradeConnectedPassages()
    {
        if(!InConnectedWorld||Rooms!.ConnectionRevision>=ConnectedWorld.Revision)return;
        // Recover actors and loot from removed edge bridges. Room origins are
        // unchanged, so positions on retained floor keep their exact frame.
        Vector2 Restore(string room,Vector2 at)
        {var offset=ConnectedWorld.Origin(room)-WorldOrigin;return Navigation.Clamp(ConnectedWorld.Ground(room),ConnectedWorld.Obstacles(room),at-offset)+offset;}
        if(!OnWalkable(Player))Player=Restore(Rooms.Current,Player);
        foreach(var foe in Enemies)if(!OnWalkable(foe.Position))foe.Position=Restore(foe.HomeRoom,foe.Position);
        foreach(var drop in Inventory.Drops.Where(d=>d.Room!=""))if(!OnWalkable(drop.Position))drop.Position=Restore(drop.Room,drop.Position);
        int previous=Rooms.ConnectionRevision;
        Rooms.CorridorSeen.RemoveWhere(key=>RoomLinks.All.Take(9).Any(l=>key.StartsWith(l.Id+":",StringComparison.Ordinal)&&(l.Id=="chamber"?previous<2:l.Id is "roots" or "grove"?previous<3:previous<4)));
        Rooms.ConnectionRevision=ConnectedWorld.Revision;
    }
    private void ValidateConnectedWorld()
    {
        if(!InConnectedWorld)return;
        if(Rooms!.ConnectionRevision<1||Rooms.ConnectionRevision>ConnectedWorld.Revision||Rooms.CorridorSeen is null||Rooms.CorridorSeen.Count>10000||Rooms.Doors is null||Rooms.Doors.Count!=RoomLinks.All.Length||RoomLinks.All.Any(l=>!Rooms.Doors.TryGetValue(l.Id,out var d)||d is null||!float.IsFinite(d.Openness)||d.Openness<0||d.Openness>1||!float.IsFinite(d.Health)||d.Health<0||d.Health>180)||Enemies.Any(e=>!PortRooms.Known(e.HomeRoom)||!Rooms.Rooms[e.HomeRoom].Visited))throw new System.IO.InvalidDataException("Ogiltig sammanhängande expedition");
    }
}
