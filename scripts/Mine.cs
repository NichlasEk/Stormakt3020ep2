using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;

public static class Mine
{
    public const string Mouth="mine-mouth",Bellows="mine-bellows",Coolway="mine-coolway";
    public static readonly string[] Ids={Mouth,Bellows,Coolway,Foundry.Room};
    public static bool Known(string id)=>Ids.Contains(id);
    public static string Name(string id)=>id switch{Foundry.Room=>"Den tomma kronans gjuteri",Mouth=>"Gruvmynningen",Bellows=>"Järnets lungor",_=>"Svalgången"};
    public static Vector2 Origin(string id)=>id switch{Foundry.Room=>new(36100,7180),Mouth=>new(30100,7000),Bellows=>new(32100,7060),_=>new(34100,7120)};
    public static readonly Vector2 Latch=new(1250,330),Ledger=new(770,610),Feed=new(480,640),Relief=new(1070,640),Imprint=new(960,665),Cache=new(520,690),Shortcut=new(720,900);
    public static readonly Vector2[] Vents={new(560,585),new(770,623),new(970,585)};
    public static Vector2[] Ground(string id)=>id==Foundry.Room?Foundry.Ground:new Vector2[]{new(90,400),new(650,280),new(1030,315),new(1450,450),new(1490,550),new(1100,775),new(850,940),new(700,940),new(285,700),new(90,510)};
    public static Vector2[][] Obstacles(string id)=>id switch
    {
        Foundry.Room=>Foundry.Obstacles,
        Mouth=>new[]{new Vector2[]{new(725,548),new(775,530),new(810,560),new(780,593),new(725,580)}},
        Bellows=>new[]{new Vector2[]{new(580,360),new(870,340),new(1080,470),new(990,550),new(740,580),new(575,470)}},
        _=>new[]{DoorTrialLayout.Bar(new(500,320),new(1377,486),18),DoorTrialLayout.Bar(new(1444,499),new(1540,560),18),new Vector2[]{new(870,520),new(965,480),new(1040,540),new(1000,625),new(915,620)}}
    };
}
public sealed class MineRun
{
    public bool EntranceOpen,LedgerRead,FeedClosed,PressureReleased,ShortcutOpen,ImprintTaken,CacheTaken;
    public float Pulse=2;
    public int Vent;
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InMine=>InConnectedWorld&&Mine.Known(Rooms!.Current);
    [JsonIgnore] public MineRun MineState=>Rooms!.Mine;
    [JsonIgnore] public string MineGoal=>Rooms!.Current switch
    {
        Mine.Mouth=>MineState.LedgerRead?"Följ ledningen till blåsbälgarna":"Läs bergmästarens driftbok",
        Mine.Bellows=>!MineState.FeedClosed?"Stäng matningen till vänster":!MineState.PressureReleased?"Öppna avlastningen till höger":"Fortsätt till svalgången",
        _=>MineState.ImprintTaken?(FoundryState.GateOpen?"Fortsätt in i gjuteriet":"Lossa porten till gjuteriet"):"Undersök gjutformen i svalgången"
    };
    [JsonIgnore] public Vector2 MineObjective=>Rooms!.Current switch
    {Mine.Mouth=>MineState.LedgerRead?new(1370,500):Mine.Ledger,Mine.Bellows=>!MineState.FeedClosed?Mine.Feed:!MineState.PressureReleased?Mine.Relief:new(1370,500),_=>MineState.ImprintTaken?Foundry.Gate:Mine.Imprint};
    public static Combat NewMinePreview(Order order)
    {
        var g=NewRegimentPreview(order);var r=g.Rooms!;var shift=g.WorldOrigin-ConnectedWorld.Origin(Regiment.Farled);
        foreach(var e in g.Enemies){e.Position+=shift;e.LockedAim+=shift;}
        foreach(var id in Regiment.Ids)r.Rooms[id].Visited=true;
        r.Current=Regiment.Farled;g.Player=Regiment.LandingBoat;
        r.Regiment.CaptainMet=r.Regiment.OrdersTaken=r.Regiment.ProofTaken=r.Regiment.MusterPassed=r.Regiment.MarshalDefeated=r.Regiment.Discharged=r.Regiment.FarledReached=true;
        g.Spawn(EnemyKind.RootMarshal,new Vector2(800,530)+Regiment.Origin(Regiment.Parade)-g.WorldOrigin);g.Enemies[^1].HomeRoom=Regiment.Parade;g.Enemies[^1].Health=0;
        g.Events.Clear();g.UpdateRoomSight(true);g.ValidateRooms();return g;
    }
    private void EnterMine(string room)
    {
        if(room==Foundry.Room)EnterFoundry();
        if(room==Mine.Mouth){Spawn(EnemyKind.Guard,new(1030,650));Spawn(EnemyKind.Gunner,new(1130,450));Emit("radio",Player,"mine-entry");}
        if(room==Mine.Bellows){Spawn(EnemyKind.Pikeman,new(850,720));Spawn(EnemyKind.Guard,new(1140,630));Emit("radio",Player,"mine-bellows");}
        if(room==Mine.Coolway)Emit("radio",Player,"mine-coolway");
    }
    private void StepMinePressure(float dt)
    {
        if(!InMine||Rooms!.Current!=Mine.Bellows||MineState.PressureReleased)return;
        var m=MineState;m.Pulse-=dt;
        if(m.Pulse>0)return;
        // Fixed outlets allow the player to learn the rhythm and lure guards.
        var at=Mine.Vents[m.Vent];m.Vent=(m.Vent+1)%Mine.Vents.Length;m.Pulse=m.FeedClosed?4.5f:2.4f;
        Hazards.Add(new(){Position=at,Timer=1.1f,Radius=68,Steam=true});Emit("room-sound",at,"mine-warning");
    }
    private bool StepMine(Func<Vector2,bool> near)
    {
        if(!InConnectedWorld)return false;var m=MineState;
        void Save()=>Emit("checkpoint",Player);
        if(Rooms!.Current==Regiment.Farled&&near(Mine.Latch))
        {
            if(!m.EntranceOpen){m.EntranceOpen=true;Emit("room-sound",Player,"stone-door");Emit("radio",Player,"mine-gate");Save();}
            else Emit("room-notice",Player,"Gruvportens spärr är lossad. Följ trappan in.");return true;
        }
        if(!InMine)return false;
        if(Rooms.Current==Mine.Mouth&&near(Mine.Ledger))
        {
            if(!m.LedgerRead){m.LedgerRead=true;Emit("radio",Player,"mine-ledger");Save();}
            Emit("campaign",Player,"DRIFTBOKEN: Stäng MATNINGEN på salens vänstra sida. Öppna sedan AVLASTNINGEN till höger. Golvutloppen blåser i turordning. Skrivaren har strukit ordet arbetare och ersatt det med bränsle.");return true;
        }
        if(Rooms.Current==Mine.Bellows)
        {
            if(near(Mine.Feed))
            {if(!m.FeedClosed){m.FeedClosed=true;Emit("room-sound",Player,"mine-valve");Emit("radio",Player,"mine-feed");Save();}else Emit("room-notice",Player,"Matningen är stängd. Öppna avlastningen till höger.");return true;}
            if(near(Mine.Relief))
            {
                if(!m.FeedClosed){Emit("room-notice",Player,"Matningen trycker tillbaka hjulet. Stäng ventilen till vänster först.");return true;}
                if(!m.PressureReleased){m.PressureReleased=true;Hazards.RemoveAll(h=>h.Steam);Emit("room-sound",Player,"pump-pressure");Emit("radio",Player,"mine-release");Save();}return true;
            }
        }
        if(Rooms.Current==Mine.Coolway)
        {
            if(near(Mine.Shortcut))
            {if(!m.ShortcutOpen){m.ShortcutOpen=true;Emit("room-sound",Player,"door-unlock");Emit("campaign",Player,"Underhållstrappan är öppen. Den går tillbaka till gruvmynningen utan att passera blåsbälgarna.");Save();}return true;}
            if(near(Mine.Cache)&&!m.CacheTaken){m.CacheTaken=true;DropItem("helmet",Mine.Cache);Save();return true;}
            if(near(Mine.Imprint))
            {
                if(!m.ImprintTaken){m.ImprintTaken=true;DropItem("memory",Mine.Imprint);Emit("radio",Player,"mine-imprint");Save();}
                Emit("campaign",Player,"GJUTFORMEN: Kronans insida saknar kunganamnet. På kanten finns samma stjärnfigur som i arkivets avtryck. Bakom den förseglade gjuteridörren fortsätter hammarslagen. Expeditionens nästa mål är Kronfogdens gjuteri.");return true;
            }
        }
        return true;
    }
    private void ValidateMine()
    {
        if(!InConnectedWorld)return;var m=MineState;
        if(m is null||!float.IsFinite(m.Pulse)||m.Pulse<0||m.Pulse>4.5f||m.Vent<0||m.Vent>=3
            ||(m.EntranceOpen&&!RegimentState.FarledReached)||(Mine.Ids.Any(id=>Rooms!.Rooms[id].Visited)&&!m.EntranceOpen)
            ||(m.LedgerRead&&!Rooms!.Rooms[Mine.Mouth].Visited)||(m.FeedClosed&&!Rooms!.Rooms[Mine.Bellows].Visited)
            ||(m.PressureReleased&&!m.FeedClosed)||(Rooms!.Rooms[Mine.Coolway].Visited&&!m.PressureReleased)
            ||((m.ImprintTaken||m.CacheTaken||m.ShortcutOpen)&&!Rooms.Rooms[Mine.Coolway].Visited))throw new System.IO.InvalidDataException("Ogiltigt gruvtillstånd");
    }
}
