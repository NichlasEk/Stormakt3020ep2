using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public static class Rootway
{
    public static readonly Vector2 Door=new(165,480),Arrival=new(270,500),Winch=new(745,420),Exit=new(1400,550);
    public static readonly Vector2 GroveDoor=new(205,470),GroveArrival=new(305,510),Memorial=new(790,710);
    public static readonly Vector2[] Ground={new(120,480),new(460,355),new(645,385),new(810,385),new(1240,510),new(1440,530),new(1420,590),new(1270,735),new(870,925),new(540,810),new(335,650),new(150,560)};
    public static readonly Vector2[] GroveGround={new(130,465),new(390,390),new(660,360),new(1020,370),new(1280,470),new(1410,555),new(1260,720),new(935,860),new(630,840),new(285,675),new(125,545)};
    public static readonly Vector2[] Slab=Navigation.Expand(new Vector2[]{new(646,590),new(798,517),new(925,580),new(778,650)},12);
}

public sealed partial class Combat
{
    [JsonIgnore] public int GroveWaves=>ArchiveChoice==1?1:2;
    [JsonIgnore] public string RootGoal=>Rooms!.Current==PortRooms.Roots
        ?Rooms.RootGateOpen?"Följ kedjan till lunden":"Lossa motviktsportens spärr"
        :Rooms.GroveSecured?"Lundens namn är återfunna · återväg öppen":Rooms.GroveWave>0&&Enemies.Any(e=>!e.Dead)?$"Skydda avtrycket · eftertrupp {Rooms.GroveWave}/{GroveWaves}":Rooms.GroveWave>0?"Ta sigillet vid minnesstenen":"Lägg arkivets handling mot stenen";
    [JsonIgnore] public string GroveClue=>ArchiveChoice==1
        ?"Vittnesmålets namn passar stenens märken. Ett snabbt avtryck räcker; en eftertrupp hinner fram."
        :"Passersedeln saknar namnen. Hedvig måste tyda stenen på nytt; två eftertrupper hinner fram.";
    private void AdvanceGrove()
    {
        if(Rooms!.Current!=PortRooms.Grove||Rooms.GroveSecured||Rooms.GroveWave==0||Rooms.GroveWave>=GroveWaves||Dead)return;
        if(Enemies.All(e=>e.Dead)&&Shots.All(s=>s.Reflected)&&Hazards.All(h=>h.Friendly))SpawnGroveWave();
    }
    private void SpawnGroveWave()
    {
        Rooms!.GroveWave++;
        if(Rooms.GroveWave==1){Spawn(EnemyKind.Pikeman,Bound(new(1160,610)));Spawn(EnemyKind.Gunner,Bound(new(1190,520)));}
        else{Spawn(EnemyKind.Guard,Bound(new(380,530)));Spawn(EnemyKind.Guard,Bound(new(435,445)));}
        foreach(var foe in Enemies.Where(e=>!e.Dead)){foe.State=2;foe.Timer=2.5f;foe.Cooldown=1.5f;}
        Emit("inscription",Player,$"KOLLEGIETS EFTERTRUPP · {Rooms.GroveWave}/{GroveWaves}");Emit("checkpoint",Player);
    }
    private bool StepRootway(Func<Vector2,bool> near,bool peaceful)
    {
        var r=Rooms!;
        if(r.Current==PortRooms.Roots&&near(Rootway.Winch))
        {
            if(!r.RootGateOpen)
            {r.RootGateOpen=true;Emit("room-sound",Player,"stone-door");Emit("radio",Player,"roots-winch");Emit("campaign",Player,"Spärren lossnar och motvikten lyfter portens lås. Kedjan löper mot valvet i öster. Återvägen till arkivet är fri.");Emit("checkpoint",Player);}
            else Emit("room-notice",Player,"Motvikten håller. Följ kedjan till östra valvet.");
            return true;
        }
        if(r.Current!=PortRooms.Grove||!near(Rootway.Memorial))return false;
        if(!peaceful){Emit("room-notice",Player,"Skydda avtrycket innan du tar sigillet.");return true;}
        if(r.GroveWave==0)
        {
            Emit("campaign",Player,GroveClue);Emit("radio",Player,ArchiveChoice==1?"roots-names":"roots-forged");SpawnGroveWave();return true;
        }
        if(!r.GroveSecured)
        {
            r.GroveSecured=true;DropItem("norn",Rootway.Memorial);Emit("radio",Player,"roots-secured");Emit("inscription",Player,"DE NAMNLÖSAS LUND");
            Emit("campaign",Player,"Under de borthuggna namnen ligger ett nytt avtryck: en väg mot berget och en sol som går upp i norr. Nornans vittnessigill lossnar ur stenen. Rotvägen är säkrad; återvänd för att hämta kvarlämnade fynd.");Emit("checkpoint",Player);
        }
        else Emit("room-notice",Player,"Namnen är återfunna. Vägen mot berget är nästa expedition.");
        return true;
    }
    private void ValidateRootway()
    {
        var r=Rooms!;var actors=r.Current==PortRooms.Grove?Enemies:r.Rooms[PortRooms.Grove].Enemies;
        if((r.Rooms[PortRooms.Roots].Visited&&!r.ArchiveSecured)||(r.RootGateOpen&&!r.Rooms[PortRooms.Roots].Visited)
            ||(r.Rooms[PortRooms.Grove].Visited&&!r.RootGateOpen)||r.GroveWave<0||r.GroveWave>GroveWaves
            ||(r.GroveWave>0&&!r.Rooms[PortRooms.Grove].Visited)||actors.Count!=r.GroveWave*2
            ||(r.GroveSecured&&(r.GroveWave!=GroveWaves||actors.Any(e=>!e.Dead))))
            throw new System.IO.InvalidDataException("Ogiltig rotväg");
    }
}
