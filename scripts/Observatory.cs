using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Observatory
{
    public const string Quarters="observatory-quarters",Workshop="observatory-workshop",Machine="observatory-machine",Dome="observatory-dome";
    public static readonly string[] Ids={Quarters,Workshop,Machine,Dome};
    public static bool Known(string id)=>Array.IndexOf(Ids,id)>=0;
    public static string Name(string id)=>id switch{Quarters=>"Astronomernas bostäder",Workshop=>"Den sönderslagna verkstaden",Machine=>"Kupolens motvikter",_=>"Övre observatoriet"};
    public static Vector2 Origin(string id)=>new(48300+Array.IndexOf(Ids,id)*2100,2200+Array.IndexOf(Ids,id)*250);
    public static readonly Vector2 Gate=new(1370,495),Marta=new(810,370),Talk=new(825,440),Diagram=new(710,345),Cabinet=new(905,320),Shortcut=new(760,445),Wake=new(770,735),Boss=new(770,535),Original=new(1235,430);
    public static readonly Vector2[] Locks={new(510,595),new(1020,595),new(765,375)},Brakes={new(485,595),new(770,715),new(1085,625)};
    public static readonly int[] Target={2,0,1};
    public static Vector2[] Ground(string id)=>id switch
    {
        Quarters=>new Vector2[]{new(65,350),new(230,355),new(530,330),new(855,330),new(1190,405),new(1470,390),new(1470,560),new(1290,720),new(1100,850),new(760,970),new(260,785),new(175,620),new(65,490)},
        Workshop=>new Vector2[]{new(45,430),new(190,345),new(610,320),new(970,310),new(1370,410),new(1490,450),new(1480,560),new(1240,720),new(1090,865),new(765,975),new(340,820),new(255,620),new(75,535)},
        Machine=>new Vector2[]{new(65,515),new(210,440),new(555,435),new(760,400),new(1020,440),new(1370,495),new(1470,545),new(1420,655),new(1160,820),new(770,960),new(370,835),new(145,670)},
        _=>new Vector2[]{new(65,445),new(250,380),new(660,355),new(1020,370),new(1300,430),new(1430,555),new(1300,750),new(1040,855),new(775,940),new(425,815),new(190,640),new(65,535)}
    };
    public static Vector2[][] Obstacles(string id)=>id switch
    {
        Quarters=>new[]{new Vector2[]{new(790,359),new(810,351),new(832,359),new(832,380),new(810,390),new(790,380)}},
        Workshop=>new[]{new Vector2[]{new(350,465),new(525,420),new(640,470),new(620,550),new(490,585),new(350,535)},new Vector2[]{new(900,465),new(1090,425),new(1250,505),new(1215,565),new(1060,595),new(890,540)}},
        Machine=>new[]{new Vector2[]{new(460,545),new(480,528),new(515,545),new(510,568),new(463,568)},new Vector2[]{new(742,650),new(773,630),new(800,650),new(790,676),new(745,676)},new Vector2[]{new(1055,575),new(1080,552),new(1110,567),new(1105,593),new(1055,593)}},
        _=>Array.Empty<Vector2[]>()
    };
}
public sealed class ObservatoryRun
{
    public bool EntryOpen,MartaMet,DiagramRead,KeyTaken,CabinetOpened,Aligned,ShortcutOpen,Awake,Defeated,OriginalTaken,Debriefed;
    public int[] Brakes={0,0,0};
    public float MechanismTime;
    public int WeightDrops;
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InObservatory=>InConnectedWorld&&Observatory.Known(Rooms!.Current);
    [JsonIgnore] public ObservatoryRun ObservatoryState=>Rooms!.Observatory;
    [JsonIgnore] public string ObservatoryGoal=>ObservatoryState.OriginalTaken?(ObservatoryState.Debriefed?"Gamla Uppsala är nästa mål":"Återvänd med originalet till Ebba"):Rooms!.Current switch
    {
        Observatory.Quarters=>!ObservatoryState.MartaMet?"Säkra rummet · tala med Märta":"Följ spåret till verkstaden",
        Observatory.Workshop=>!ObservatoryState.DiagramRead?"Läs stjärndiagrammet":!ObservatoryState.KeyTaken?"Säkra verkstaden · sök nyckeln":"Fortsätt till motvikterna",
        Observatory.Machine=>!ObservatoryState.Aligned?"Ställ bromsarna efter diagrammet":"Öppna vägen till kupolen",
        _=>!ObservatoryState.Awake?"Undersök kupolens drivverk":!ObservatoryState.Defeated?(Enemies.Any(e=>!e.Dead&&e.Kind==EnemyKind.ZenithLock)?"Hugg sönder låsningarna":"Verket är oskyddat · angrip mitten"):!ObservatoryState.OriginalTaken?"Läs originalet vid skrivbordet":ObservatoryState.Debriefed?"Gamla Uppsala är nästa mål":"Ta originalet till Ebba"
    };
    [JsonIgnore] public Vector2 ObservatoryObjective=>ObservatoryState.OriginalTaken?(Rooms!.Current==Observatory.Machine&&ObservatoryState.ShortcutOpen?Observatory.Shortcut:Rooms!.Current==Observatory.Quarters?new(95,360):Rooms.Current==Observatory.Workshop?new(90,450):Rooms.Current==Observatory.Machine?new(100,535):new(105,465)):Rooms!.Current switch{Observatory.Quarters=>ObservatoryState.MartaMet?new(1410,435):Observatory.Talk,Observatory.Workshop=>!ObservatoryState.DiagramRead?Observatory.Diagram:ObservatoryState.KeyTaken?new(1420,470):Observatory.Cabinet,Observatory.Machine=>ObservatoryState.Aligned?new(1400,550):Observatory.Brakes[Enumerable.Range(0,3).First(i=>ObservatoryState.Brakes[i]!=Observatory.Target[i])],_=>!ObservatoryState.Awake?Observatory.Wake:ObservatoryState.Defeated?Observatory.Original:Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.ZenithLock&&!e.Dead&&e.HomeRoom==Observatory.Dome)?.Position??Observatory.Boss};
    public static Combat NewObservatoryPreview(Order order)
    {
        var g=NewCabinPreview(order);g.CabinState.Conversation=3;g.CabinState.Rested=true;g.Rooms!.Rooms[Cabin.Room].Visited=true;g.Health=g.Stamina=100;
        g.EnterConnectedRoom(Meridian.Hall);g.Player=Observatory.Gate;g.Events.Clear();g.UpdateRoomSight(true);g.ValidateRooms();return g;
    }
    private void EnterObservatory(string room)
    {
        if(room==Observatory.Quarters){Spawn(EnemyKind.Guard,new(560,550));Spawn(EnemyKind.Gunner,new(1080,540));Emit("radio",Player,"observatory-quarters");}
        if(room==Observatory.Workshop)
        {Spawn(EnemyKind.Guard,new(760,560));Spawn(EnemyKind.Pikeman,new(1250,680));Spawn(EnemyKind.Gunner,new(1130,355));Spawn(EnemyKind.Guard,new(300,630));Rooms!.Doors["observatory-workshop"].TargetOpen=false;Emit("radio",Player,"observatory-workshop");}
        if(room==Observatory.Machine){Spawn(EnemyKind.Guard,new(1040,745));Spawn(EnemyKind.Pikeman,new(600,745));Emit("radio",Player,"observatory-weights");}
        if(room==Observatory.Dome)
        {Spawn(EnemyKind.ZenithGuardian,Observatory.Boss);for(int i=0;i<3;i++){Spawn(EnemyKind.ZenithLock,Observatory.Locks[i]);Enemies[^1].Pattern=i;}Emit("radio",Player,"observatory-zenith");}
    }
    private bool StepObservatory(Func<Vector2,bool> near)
    {
        if(!InConnectedWorld)return false;var o=ObservatoryState;
        if(Rooms!.Current==Meridian.Hall&&near(Observatory.Gate)&&MeridianState.OrderTaken&&CabinState.Briefed)
        {if(!o.EntryOpen){o.EntryOpen=true;Emit("room-sound",Player,"door-unlock");Emit("radio",Player,"observatory-entry");Emit("checkpoint",Player);}return true;}
        if(!InObservatory)return false;
        bool safe=EncounterEnemies.All(e=>e.Dead);
        if(Rooms.Current==Observatory.Quarters&&near(Observatory.Talk))
        {
            if(!safe){Emit("room-notice",Player,"Driv bort vakterna från bostäderna först.");return true;}
            if(!o.MartaMet){o.MartaMet=true;Emit("radio",Player,"observatory-marta");Emit("radio",Player,"observatory-list");Emit("checkpoint",Player);}else Emit("radio",Player,"observatory-marta-repeat");return true;
        }
        if(Rooms.Current==Observatory.Workshop)
        {
            if(near(Observatory.Diagram))
            {if(!o.DiagramRead){o.DiagramRead=true;Emit("radio",Player,"observatory-diagram");Emit("checkpoint",Player);}Emit("campaign",Player,"STJÄRNDIAGRAMMET: Ställ vänster broms på II, den främre på 0 och höger på I. Motvikterna följer markeringarna på golvet. Märta har ritat en säker väg genom verket.");return true;}
            if(near(Observatory.Cabinet))
            {
                if(o.CabinetOpened){Emit("room-notice",Player,"Gömman är tömd. Verkstadsnyckeln är tagen och passar underhållsdörren vid motvikterna.");return true;}
                if(!safe){Emit("room-notice",Player,"Vakterna har verkstadsnyckeln. Säkra rummet.");return true;}
                if(!o.KeyTaken){o.KeyTaken=true;Emit("inscription",Player,"VERKSTADSNYCKEL");Emit("radio",Player,"observatory-key");}
                if(!o.CabinetOpened){o.CabinetOpened=true;Emit("room-sound",Player,"door-creak");DropItem("forge-hammer",new(875,355));Emit("campaign",Player,"MÄRTAS GÖMMA: En förstärkt hammare och en anteckning: Elin, om de flyttar oss innan jag kommer, lämna lampan i fönstret.");Emit("checkpoint",Player);}return true;
            }
        }
        if(Rooms.Current==Observatory.Machine)
        {
            if(near(Observatory.Shortcut))
            {if(!o.KeyTaken){Emit("room-notice",Player,"Underhållsdörren kräver verkstadsnyckeln.");return true;}if(!o.ShortcutOpen){o.ShortcutOpen=true;Emit("radio",Player,"observatory-shortcut");Emit("room-sound",Player,"door-unlock");Emit("checkpoint",Player);}return true;}
            for(int i=0;i<3;i++)if(near(Observatory.Brakes[i]))
            {if(!o.DiagramRead){Emit("room-notice",Player,"Läs verkstadens stjärndiagram först.");return true;}if(o.Aligned){Emit("room-notice",Player,"Bromsen håller motvikten.");return true;}
                o.Brakes[i]=(o.Brakes[i]+1)%3;Emit("room-sound",Player,"meridian-turn");Emit("room-notice",Player,$"BROMS {i+1} · {o.Brakes[i]}");
                if(o.Brakes.SequenceEqual(Observatory.Target)){o.Aligned=true;Hazards.RemoveAll(h=>h.Zenith);Emit("radio",Player,"observatory-aligned");}Emit("checkpoint",Player);return true;}
        }
        if(Rooms.Current==Observatory.Dome)
        {
            if(near(Observatory.Wake)&&!o.Awake){o.Awake=true;Emit("radio",Player,"observatory-locks");Emit("checkpoint",Player);return true;}
            if(near(Observatory.Original))
            {if(!o.Defeated){Emit("room-notice",Player,"Zenitväktaren bevakar originalet.");return true;}
                if(!o.OriginalTaken){o.OriginalTaken=true;DropItem("memory",Observatory.Original);Emit("radio",Player,"observatory-original");Emit("radio",Player,"observatory-response");Emit("checkpoint",Player);}
                Emit("campaign",Player,"ORIGINALORDERN: Kvarterets invånare skall föras till mottagningsanläggningen under kungshögarna i Gamla Uppsala. Bland namnen står Märta Vinge och hennes syster Elin. Det framgår ännu inte om transporten genomfördes. Ta originalet till Ebba.");return true;}
        }
        return true;
    }
    private void StepObservatoryClock(float dt)
    {
        if(!InObservatory||Rooms!.Current!=Observatory.Machine||ObservatoryState.Aligned)return;
        var o=ObservatoryState;o.MechanismTime+=dt;if(o.MechanismTime<4)return;o.MechanismTime-=4;
        var p=Observatory.Brakes[o.WeightDrops++%3]+new Vector2(95,35);Hazards.Add(new(){Position=p,Timer=1.5f,Radius=64,Zenith=true});Emit("room-sound",p,"mine-warning");
    }
    private void StepZenith(Fighter e,float dt)
    {
        e.Hurt=Math.Max(0,e.Hurt-dt);e.Moving=false;if(e.Kind==EnemyKind.ZenithLock)return;
        if(!InObservatory||Rooms!.Current!=Observatory.Dome||!ObservatoryState.Awake)return;
        e.Facing=Normal(Player-e.Position,Vector2.UnitY);e.Cooldown=Math.Max(0,e.Cooldown-dt);
        if(e.State==0){if(e.Cooldown>0)return;e.State=1;e.Timer=e.Health<e.MaxHealth*.5f?1.05f:1.45f;e.LockedAim=Player;Emit("warning",e.Position);return;}
        e.Timer-=dt;if(e.Timer>0)return;
        if(e.State==1)
        {
            var axis=Normal(e.LockedAim-e.Position,Vector2.UnitX);var side=new Vector2(-axis.Y,axis.X);
            for(int k=-1;k<=1;k++){var p=e.LockedAim+side*k*90;if(OnWalkable(p))Hazards.Add(new(){Position=p,Timer=.75f,Radius=58,Zenith=true});}
            if(e.Health<e.MaxHealth*.5f)Hazards.Add(new(){Position=Player,Timer=1.25f,Radius=65,Zenith=true});
            e.State=2;e.Timer=2.3f;Emit("room-sound",e.Position,"meridian-turn");
        }
        else{e.State=0;e.Cooldown=.7f;}
    }
    private bool ZenithDeath(Fighter e)
    {
        if(!e.Dead||e.Kind is not (EnemyKind.ZenithGuardian or EnemyKind.ZenithLock))return false;
        if(e.Kind==EnemyKind.ZenithLock){foreach(var h in Hazards.Where(h=>h.Zenith))h.Timer=0;Emit("room-sound",e.Position,"door-break");Emit("inscription",e.Position,"LÅSNINGEN BRISTER");}
        else{ObservatoryState.Defeated=true;foreach(var h in Hazards.Where(h=>h.Zenith))h.Timer=0;Emit("radio",e.Position,"observatory-fallen");Emit("cinematic",Player,"observatory-dome");}
        Emit("checkpoint",Player);return true;
    }
    private void ValidateObservatory()
    {
        if(!InConnectedWorld)return;var o=ObservatoryState;
        if(o is null||o.Brakes is null||o.Brakes.Length!=3||o.Brakes.Any(v=>v<0||v>2)||o.MechanismTime<0||o.MechanismTime>4.1f||!float.IsFinite(o.MechanismTime)||o.WeightDrops<0||
          (o.EntryOpen&&!CabinState.Briefed)||(Rooms!.Rooms[Observatory.Quarters].Visited&&!o.EntryOpen)||(o.MartaMet&&!Rooms.Rooms[Observatory.Quarters].Visited)||(Rooms.Rooms[Observatory.Workshop].Visited&&!o.MartaMet)||(o.DiagramRead&&!Rooms.Rooms[Observatory.Workshop].Visited)||(o.KeyTaken&&!Rooms.Rooms[Observatory.Workshop].Visited)||(o.CabinetOpened&&!o.KeyTaken)||(o.Aligned&&(!o.DiagramRead||!o.Brakes.SequenceEqual(Observatory.Target)))||(o.ShortcutOpen&&!o.KeyTaken)||(Rooms.Rooms[Observatory.Dome].Visited&&!o.Aligned)||(o.Awake&&!Rooms.Rooms[Observatory.Dome].Visited)||(o.Defeated&&!o.Awake)||(o.OriginalTaken&&!o.Defeated)||(o.Debriefed&&!o.OriginalTaken))throw new System.IO.InvalidDataException("Ogiltigt observatorium");
        var bosses=ActorsInRoom(Observatory.Dome);if(Rooms!.Rooms[Observatory.Dome].Visited?(bosses.Count!=4||bosses.Count(e=>e.Kind==EnemyKind.ZenithGuardian)!=1||bosses.Single(e=>e.Kind==EnemyKind.ZenithGuardian).Dead!=o.Defeated||o.Defeated&&bosses.Any(e=>!e.Dead)):bosses.Count!=0)throw new System.IO.InvalidDataException("Ogiltigt zenitverk");
    }
}
