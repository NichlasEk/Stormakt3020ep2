using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;

public static class Regiment
{
    public const string Trail="regiment-trail",Barracks="regiment-barracks",Flags="regiment-flags",Parade="regiment-parade",Quay="regiment-quay",Farled="regiment-farled";
    public static readonly string[] Ids={Trail,Barracks,Flags,Parade,Quay,Farled};
    public static bool Known(string id)=>Ids.Contains(id);
    public static string Name(string id)=>id switch{Trail=>"Förläggningsstigen",Barracks=>"Sjukbaracken",Flags=>"Fanlunden",Parade=>"Mönstringsvallen",Quay=>"Den övergivna bryggan",_=>"Farleden vid berget"};
    public static readonly Vector2 Captain=new(610,600),Orders=new(760,615),Proof=new(380,425),Checkpoint=new(1130,650),Shortcut=new(800,930),Discharge=new(770,550),Boat=new(1170,610),LandingBoat=new(390,580);
    public static readonly Vector2[] Standards={new(485,410),new(1070,450),new(770,700)};
    public static Vector2 Origin(string id)=>id switch{Trail=>new(16650,4000),Barracks=>new(18750,4190),Flags=>new(20850,4300),Parade=>new(22950,4430),Quay=>new(25050,4600),_=>new(28000,7000)};
    public static Vector2[] Ground(string id)=>id switch
    {
        Trail=>new Vector2[]{new(0,355),new(620,280),new(1050,345),new(1536,490),new(1536,615),new(940,850),new(710,900),new(250,680),new(0,505)},
        Barracks=>new Vector2[]{new(95,420),new(520,280),new(960,320),new(1430,435),new(1490,495),new(1435,565),new(920,960),new(600,825),new(90,510)},
        Flags=>new Vector2[]{new(0,340),new(530,225),new(1030,255),new(1536,465),new(1536,600),new(900,810),new(860,1024),new(740,1024),new(675,765),new(0,470)},
        Parade=>new Vector2[]{new(90,405),new(650,260),new(1080,320),new(1440,440),new(1460,560),new(1230,790),new(820,895),new(310,725),new(95,480)},
        Quay=>new Vector2[]{new(115,415),new(465,330),new(790,440),new(1090,535),new(1270,590),new(1260,645),new(1140,675),new(970,605),new(550,540),new(285,670),new(100,550)},
        _=>new Vector2[]{new(145,545),new(620,455),new(830,400),new(1100,280),new(1280,225),new(1350,275),new(1250,370),new(1140,485),new(1090,535),new(1350,710),new(1270,810),new(810,740),new(270,600)}
    };
    public static Vector2[][] Obstacles(string id)=>id switch
    {
        Trail=>new[]{new Vector2[]{new(515,500),new(750,375),new(995,465),new(945,570),new(750,625)}},
        Barracks=>new[]{new Vector2[]{new(620,485),new(735,422),new(875,465),new(873,535),new(750,577),new(620,545)}},
        Flags=>new[]{new Vector2[]{new(400,440),new(580,320),new(880,340),new(1090,490),new(975,550),new(650,600)}},
        _=>Array.Empty<Vector2[]>()
    };
}
public sealed class RegimentRun
{
    public bool CaptainMet,OrdersTaken,ProofTaken,MusterPassed,ShortcutOpen,MarshalDefeated,Discharged,FarledReached,QuayCacheTaken;
    public float[] Standards={65,65,65};
    public float Exposed;
    public int Formation;
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InRegiment=>InConnectedWorld&&Regiment.Known(Rooms!.Current);
    [JsonIgnore] public RegimentRun RegimentState=>Rooms!.Regiment;
    [JsonIgnore] public float RootSnare;
    [JsonIgnore] public string RegimentGoal=>Rooms!.Current switch
    {
        Regiment.Trail=>"Följ trummorna till sjukbaracken",
        Regiment.Barracks=>!RegimentState.CaptainMet?"Tala med kapten Silfvergren":!RegimentState.OrdersTaken?"Läs avlösningsordern på bordet":"Fortsätt till fanlunden",
        Regiment.Flags=>!RegimentState.MusterPassed?"Visa din handling vid kontrollen":"Följ vägen till mönstringsvallen",
        Regiment.Parade=>!RegimentState.MarshalDefeated?(RegimentState.Exposed>0?"Eden vacklar · angrip Rotmarskalken":"Bryt fanornas rotförbindelser"):!RegimentState.OrdersTaken?"Hämta avlösningsordern i sjukbaracken":!RegimentState.Discharged?"Läs avlösningen vid mönstringsstenen":"Fortsätt till regementets brygga",
        Regiment.Quay=>"Båten väntar vid bryggan · E för överfart",
        _=>MineState.EntranceOpen?"Följ trappan in i berget":"Lossa gruvportens spärr"
    };
    [JsonIgnore] public Vector2 RegimentObjective=>Rooms!.Current switch
    {Regiment.Trail=>new(1440,550),Regiment.Barracks=>!RegimentState.CaptainMet?Regiment.Captain:!RegimentState.OrdersTaken?Regiment.Orders:new(1420,460),Regiment.Flags=>!RegimentState.MusterPassed?Regiment.Checkpoint:new(1450,520),Regiment.Parade=>RegimentState.MarshalDefeated?Regiment.Discharge:RegimentState.Exposed>0?new(800,550):Regiment.Standards[Array.FindIndex(RegimentState.Standards,h=>h>0) is var i&&i>=0?i:0],Regiment.Quay=>Regiment.Boat,_=>MineState.EntranceOpen?new(1260,290):Mine.Latch};
    public static Combat NewRegimentPreview(Order order)
    {
        var game=NewRooms(order);game.EnableConnectedWorld();game.Enemies.Clear();var r=game.Rooms!;
        foreach(var id in PortRooms.Ids)r.Rooms[id].Visited=true;
        r.KeyTaken=r.DoorOpen=r.CacheTaken=r.PressureReleased=r.WaterLowered=r.ShortcutOpen=r.RelicTaken=r.WitnessRead=r.OathDefeated=r.Completed=r.ArchiveRead=r.ArchiveSecured=r.RootGateOpen=r.GroveSecured=true;
        game.ArchiveChoice=1;r.GroveWave=1;r.Current=PortRooms.Grove;game.Player=new(800,845);
        void Fallen(string home,EnemyKind kind,Vector2 at){game.Spawn(kind,at+ConnectedWorld.Origin(home)-game.WorldOrigin);game.Enemies[^1].HomeRoom=home;game.Enemies[^1].Health=0;}
        Fallen(PortRooms.Chamber,EnemyKind.OathGuardian,new(800,550));
        for(int i=0;i<3;i++)Fallen(PortRooms.Archive,EnemyKind.Guard,new(1000+i*40,600));
        for(int i=0;i<2;i++)Fallen(PortRooms.Grove,EnemyKind.Guard,new(1000+i*40,600));
        foreach(var link in RoomLinks.All){var d=r.Doors[link.Id];d.Locked=!RoomLinks.Open(r,link);d.TargetOpen=!d.Locked;d.Openness=d.TargetOpen?1:0;}
        game.Events.Clear();game.UpdateRoomSight(true);game.ValidateRooms();return game;
    }
    private void EnterRegiment(string room)
    {
        if(room==Regiment.Trail){Spawn(EnemyKind.RootSoldier,new(470,560));Spawn(EnemyKind.RootSoldier,new(1100,500));Emit("radio",Player,"regiment-entry");}
        if(room==Regiment.Flags){Spawn(EnemyKind.RootSoldier,new(1110,590));Spawn(EnemyKind.RootSoldier,new(1150,650));}
        if(room==Regiment.Parade)
        {
            Spawn(EnemyKind.RootMarshal,new(800,530));Emit("radio",Player,"regiment-marshal");
            if(ArchiveChoice==2&&!RegimentState.ProofTaken)
            {
                Spawn(EnemyKind.RootSoldier,new(570,570));Spawn(EnemyKind.RootSoldier,new(1030,610));
                Emit("campaign",Player,"Passersedeln öppnade kontrollen, men fanvakterna står kvar under eden. Sjukrullans namn kan fortfarande befria dem.");
            }
        }
    }
    private bool StepRegiment(Func<Vector2,bool> near,bool peaceful)
    {
        if(!InRegiment)return false;var r=RegimentState;
        void Save()=>Emit("checkpoint",Player);
        if(Rooms!.Current==Regiment.Barracks)
        {
            if(near(Regiment.Captain))
            {if(!r.CaptainMet){r.CaptainMet=true;Emit("radio",Player,"regiment-captain");Save();}else Emit("room-notice",Player,"Kaptenen pekar mot ordern på bordet. Han har väntat färdigt.");return true;}
            if(near(Regiment.Orders))
            {if(!r.CaptainMet){Emit("room-notice",Player,"Tala med kaptenen innan du tar hans order.");return true;}
                if(!r.OrdersTaken){r.OrdersTaken=true;Emit("radio",Player,"regiment-orders");Emit("campaign",Player,"AVLÖSNINGSORDER: Kompaniet får lämna sin post när befälets ed är bruten och ordern läses vid mönstringsstenen. Sjukrullan vid bäddarna styrker de saknade namnen.");Save();}else Emit("room-notice",Player,"Avlösningsordern följer expeditionen.");return true;}
            if(near(Regiment.Proof))
            {if(!r.ProofTaken){r.ProofTaken=true;foreach(var soldier in ActorsInRoom(Regiment.Parade).Where(e=>e.Kind==EnemyKind.RootSoldier&&!e.Dead)){soldier.Health=0;soldier.Retired=true;}Emit("radio",Player,"regiment-proof");Emit("inscription",Player,"SJUKRULLAN ÄR SÄKRAD");Save();}return true;}
        }
        if(Rooms.Current==Regiment.Flags)
        {
            if(near(Regiment.Checkpoint))
            {
                if(!r.OrdersTaken){Emit("room-notice",Player,"Kontrollen begär en order från kaptenen i sjukbaracken.");return true;}
                if(!r.MusterPassed)
                {
                    r.MusterPassed=true;
                    foreach(var guard in ActorsInRoom(Regiment.Flags).Where(e=>!e.Dead)){guard.Health=0;guard.Retired=true;Emit("regiment-rest",guard.Position);}
                    Emit("radio",Player,ArchiveChoice==1||r.ProofTaken?"regiment-names":"regiment-pass");
                    Emit("campaign",Player,ArchiveChoice==1||r.ProofTaken?"Namnen räcker. Soldaterna sänker vapnen. En styrkt avlösning kan läsas vid vallen.":"Kontrollen godtar passersedeln. Vid mönstringsstenen behövs kaptenens order; sjukrullan kan fortfarande återfinnas i baracken.");Save();
                }
                return true;
            }
            if(near(Regiment.Shortcut))
            {if(!r.ShortcutOpen){r.ShortcutOpen=true;Emit("room-sound",Player,"door-unlock");Emit("campaign",Player,"Den gamla återtågsvägen är öppen. Den leder tillbaka till lunden under förläggningens murar.");Save();}return true;}
        }
        if(Rooms.Current==Regiment.Parade&&r.MarshalDefeated&&near(Regiment.Discharge))
        {
            if(!r.OrdersTaken){Emit("room-notice",Player,"Avlösningsordern ligger kvar hos kaptenen i sjukbaracken.");return true;}
            if(!r.Discharged){r.Discharged=true;Emit("radio",Player,"regiment-freed");DropItem("forge-hammer",Regiment.Discharge);Emit("inscription",Player,"REGEMENTET ÄR AVLÖST");Save();}return true;
        }
        if(Rooms.Current==Regiment.Quay&&near(new(640,465))&&!r.QuayCacheTaken)
        {r.QuayCacheTaken=true;DropItem("brigandine",new(640,465));Emit("campaign",Player,"Kaptenens kvarlåtenskap: en lagad brigantin och en anteckning om farleden mot berget.");Save();return true;}
        if((Rooms.Current==Regiment.Quay&&near(Regiment.Boat))||(Rooms.Current==Regiment.Farled&&near(Regiment.LandingBoat)))
        {
            if(!peaceful){Emit("room-notice",Player,"Säkra bryggan innan du stiger ombord.");return true;}
            Emit("boat-travel",Player,Rooms.Current==Regiment.Quay?Regiment.Farled:Regiment.Quay);return true;
        }
        return true;
    }
    // Called after the visible crossing, not from a map click. The same world
    // translation used for walking preserves every remote actor and dropped item.
    public bool FinishBoatCrossing(string destination)
    {
        if(!InRegiment||!RegimentState.Discharged||Dead||!((Rooms!.Current==Regiment.Quay&&destination==Regiment.Farled)||(Rooms!.Current==Regiment.Farled&&destination==Regiment.Quay)))return false;
        Events.Clear();bool first=!RegimentState.FarledReached;RegimentState.FarledReached=true;
        EnterConnectedRoom(destination);Player=destination==Regiment.Farled?Regiment.LandingBoat:Regiment.Boat;UpdateRoomSight(true);
        if(first)Emit("radio",Player,"regiment-boat");Emit("region",Player,RoomName);Emit("checkpoint",Player);return true;
    }
    private void HitRegimentStandards(float range,float damage,float arc)
    {
        if(!InConnectedWorld||RegimentState.MarshalDefeated||!Rooms!.Rooms[Regiment.Parade].Visited)return;
        var shift=Regiment.Origin(Regiment.Parade)-WorldOrigin;
        for(int i=0;i<3;i++)
        {
            var at=Regiment.Standards[i]+shift;var delta=at-Player;
            if(RegimentState.Standards[i]<=0||delta.Length()>range||Vector2.Dot(Normal(delta,Facing),Facing)<=arc||!ClearPath(Player,at))continue;
            RegimentState.Standards[i]=Math.Max(0,RegimentState.Standards[i]-damage);Emit("sealhit",at);
            if(RegimentState.Standards[i]==0)Emit("seal",at,"ROTFÖRBINDELSEN BRUTEN");
        }
    }
    private void StepRootMarshal(Fighter e,float dt)
    {
        var r=RegimentState;e.Hurt=Math.Max(0,e.Hurt-dt);e.Cooldown=Math.Max(0,e.Cooldown-dt);
        if(r.Standards.All(h=>h<=0)&&r.Exposed<=0){r.Exposed=8;e.State=3;e.Timer=8;Emit("inscription",e.Position,"FANORNA FALLER · ANGRIP");}
        if(r.Exposed>0)
        {
            r.Exposed=Math.Max(0,r.Exposed-dt);e.State=3;
            if(r.Exposed==0){r.Formation++;for(int i=0;i<3;i++)r.Standards[i]=e.Health<e.MaxHealth*.5f&&i==r.Formation%3?0:45;e.State=0;e.Cooldown=1.3f;}
            return;
        }
        var delta=Player-e.Position;e.Facing=Normal(delta,e.Facing);
        if(e.State==0)
        {
            var before=e.Position;if(delta.Length()>155)e.Position=MoveBody(e.Position,e.Position+Normal(NextWaypoint(e.Position,Player)-e.Position,e.Facing)*68*dt);
            e.Moving=Vector2.DistanceSquared(before,e.Position)>.001f;e.Walk+=Vector2.Distance(before,e.Position)*7/195;
            if(e.Cooldown<=0&&delta.Length()<420){e.State=1;e.Timer=1.2f;e.LockedAim=Player;Emit("warning",e.Position);}
        }
        else
        {
            e.Timer-=dt;if(e.Timer>0)return;
            if(e.State==1)
            {
                // Two small delayed roots leave ample escape lanes. Later formations
                // offset the second root, instead of merely speeding up every attack.
                Hazards.Add(new(){Position=Bound(e.LockedAim),Timer=.85f,Radius=65,Roots=true});
                if(e.Health<e.MaxHealth*.5f)Hazards.Add(new(){Position=Bound(e.LockedAim+new Vector2(r.Formation%2==0?120:-120,55)),Timer=1.4f,Radius=55,Roots=true});
                if(delta.Length()<175&&ClearPath(e.Position,Player)&&DodgeTime<=0)DamagePlayer(26,e.Position);
                e.State=2;e.Timer=1.6f;Emit("enemystrike",e.Position);
            }
            else{e.State=0;e.Cooldown=1.1f;}
        }
    }
    private void ValidateRegiment()
    {
        if(!InConnectedWorld)return;var r=Rooms!.Regiment;
        if(r is null||r.Standards is null||r.Standards.Length!=3||r.Standards.Any(h=>!float.IsFinite(h)||h<0||h>65)||!float.IsFinite(r.Exposed)||r.Exposed<0||r.Exposed>8||r.Formation<0||r.Formation>100000
            ||(Regiment.Ids.Any(id=>Rooms.Rooms[id].Visited)&&!Rooms.GroveSecured)||(r.CaptainMet&&!Rooms.Rooms[Regiment.Barracks].Visited)||(r.OrdersTaken&&!r.CaptainMet)||(r.ProofTaken&&!Rooms.Rooms[Regiment.Barracks].Visited)||(r.MusterPassed&&!r.OrdersTaken)||(r.ShortcutOpen&&!Rooms.Rooms[Regiment.Flags].Visited)||(r.Discharged&&(!r.MarshalDefeated||!r.OrdersTaken))||(r.FarledReached&&!r.Discharged)||(Rooms.Rooms[Regiment.Quay].Visited&&!r.Discharged)||(Rooms.Rooms[Regiment.Farled].Visited&&!r.FarledReached))throw new System.IO.InvalidDataException("Ogiltig regementesexpedition");
        var bosses=Enemies.Where(e=>e.Kind==EnemyKind.RootMarshal).ToArray();
        if(Rooms.Rooms[Regiment.Parade].Visited?(bosses.Length!=1||bosses[0].Dead!=r.MarshalDefeated):bosses.Length!=0)throw new System.IO.InvalidDataException("Ogiltig rotmarskalk");
    }
}
