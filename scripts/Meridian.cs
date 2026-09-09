using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Meridian
{
    public const string Clock="uppsala-clock",Hall="uppsala-meridian";
    public static readonly Vector2 CourtGate=new(1040,435),Ledger=new(540,465),Bell=new(870,595),ClockExit=new(1370,465),Plate=new(960,425),Warden=new(850,420),Order=new(1340,485);
    public static readonly Vector2[] Centers={new(558,484),new(866,552),new(1240,610)},Controls=Centers.Select(p=>p+new Vector2(0,65)).ToArray();
    public static readonly int[] Target={1,3,2};
    public static readonly Vector2[] ClockGround={new(100,515),new(300,450),new(660,415),new(900,360),new(1180,430),new(1430,465),new(1450,655),new(1140,795),new(770,935),new(330,800),new(105,645)};
    public static readonly Vector2[] HallGround={new(110,490),new(345,415),new(650,325),new(970,375),new(1440,475),new(1480,650),new(1170,760),new(900,855),new(680,900),new(280,690),new(110,575)};
    public static readonly Vector2[][] HallObstacles=Centers.Select(p=>new[]{p+new Vector2(-34,-12),p+new Vector2(0,-29),p+new Vector2(34,-12),p+new Vector2(34,18),p+new Vector2(0,30),p+new Vector2(-34,18)}).ToArray();
    public static string Name(string id)=>id==Clock?"Klockgångens återkomst":id==Hall?"Meridiansalen":"Uppsalas felvända himmel";
}
public sealed class MeridianRun
{
    public bool CourtOpen,ClockStarted,LedgerRead,ClockAnchored,PlateSet,WardenDefeated,OrderTaken,SecondPhase;
    public int Cycles,Breaks;
    public float ClockTime,Exposed,Departure,Dawn;
    public Vector2 ReleaseFrom;
    public int[] Angles={2,0,3};
}
public sealed partial class Combat
{
    [JsonIgnore] public MeridianRun MeridianState=>Rooms!.Meridian;
    [JsonIgnore] public bool InMeridian=>InUppsala&&!InCabin&&Rooms!.Current!=Uppsala.Court;
    [JsonIgnore] public string MeridianGoal=>Rooms!.Current==Meridian.Clock?
        !MeridianState.LedgerRead?"Läs klockans liggare":!MeridianState.ClockStarted?"Starta slagverket vid spaken":MeridianState.Cycles<2?"Följ vakten · lyssna efter nästa klockslag":!MeridianState.ClockAnchored?"Håll stjärnplåten mot slagverkets spärr":"Fortsätt genom valvet till Meridiansalen":
        !MeridianState.PlateSet?"Jämför stjärnplåten vid astronomens bord":!MeridianState.WardenDefeated?MeridianState.Exposed>0?"Verket tappar takten · angrip väktaren":"Rikta det rörliga instrumentet · undvik mätlinjen":!MeridianState.OrderTaken?"Undersök förflyttningsordern vid porten":CabinState.Briefed?"Ordern är hos Ebba":"Ordern är säkrad · återvänd till Ebba";
    [JsonIgnore] public Vector2 ClockEchoPosition=>MeridianState.ClockAnchored?Vector2.Lerp(MeridianState.ReleaseFrom,new(1400,425),Math.Clamp(MeridianState.Departure/8,0,1)):Vector2.Lerp(new(470,555),new(1000,550),MeridianState.ClockTime<=6?MeridianState.ClockTime/6:(12-MeridianState.ClockTime)/6);
    public static Combat NewMeridianPreview(Order order)
    {
        var g=NewUppsalaPreview(order);g.FinishShipTravel(Uppsala.Court);var u=g.UppsalaState;
        u.ClueRead=u.Aligned=u.Secured=u.KeyTaken=true;u.Rings=(int[])Uppsala.Target.Clone();
        foreach(var kind in new[]{EnemyKind.Guard,EnemyKind.Pikeman,EnemyKind.Gunner}){g.Spawn(kind,new(1200,700));g.Enemies[^1].Health=0;}
        g.Player=Meridian.CourtGate;g.Events.Clear();g.UpdateRoomSight(true);g.ValidateRooms();return g;
    }
    private void EnterMeridian(string room)
    {
        if(room==Meridian.Clock)Emit("radio",Player,"meridian-entry");
        if(room==Meridian.Hall){Spawn(EnemyKind.MeridianWarden,Meridian.Warden);Emit("radio",Player,"meridian-warden");}
    }
    private void StepMeridianClock(float dt)
    {
        if(!InUppsala)return;var m=MeridianState;
        m.Exposed=Math.Max(0,m.Exposed-dt);if(m.WardenDefeated)m.Dawn=Math.Min(12,m.Dawn+dt);
        if(Rooms!.Current!=Meridian.Clock)return;
        if(m.ClockAnchored){m.Departure=Math.Min(8,m.Departure+dt);return;}
        if(!m.ClockStarted)return;m.ClockTime+=dt;
        if(m.ClockTime<12)return;m.ClockTime-=12;m.Cycles=Math.Min(1000,m.Cycles+1);
        Emit("room-sound",Player,"meridian-bell");Emit("inscription",Player,"SJÄTTE TIMMEN · SAMMA SLAG");
        if(m.Cycles==1)Emit("radio",Player,"meridian-repeat");
        if(m.Cycles==2){Emit("radio",Player,"meridian-noticed");Emit("radio",Player,"meridian-anchor");}
        Emit("checkpoint",Player);
    }
    private bool StepMeridian(Func<Vector2,bool> near)
    {
        if(!InUppsala||InCabin)return false;var m=MeridianState;
        if(Rooms!.Current==Uppsala.Court)
        {
            if(!UppsalaState.KeyTaken||!near(Meridian.CourtGate))return false;
            if(!m.CourtOpen){m.CourtOpen=true;Emit("radio",Player,"meridian-door");Emit("room-sound",Player,"stone-door");Emit("checkpoint",Player);}return true;
        }
        if(Rooms.Current==Meridian.Clock)
        {
            if(near(Meridian.Ledger))
            {if(!m.LedgerRead){m.LedgerRead=true;Emit("radio",Player,"meridian-ledger");Emit("checkpoint",Player);}Emit("campaign",Player,"SLAGVERKETS LIGGARE: Sjätte timmen, sjuttonde minuten. Samma vakt, samma vändpunkt. Slagverkets spärr har ett märke som också finns på stjärnplåten.");return true;}
            if(near(Meridian.Bell))
            {
                if(!m.ClockStarted){m.ClockStarted=true;m.ClockTime=0;Emit("room-sound",Player,"meridian-bell");Emit("radio",Player,"meridian-repeat");Emit("checkpoint",Player);return true;}
                if(m.Cycles<2){Emit("room-notice",Player,"Följ vaktens vändpunkt genom två klockslag.");return true;}
                if(!m.LedgerRead){Emit("room-notice",Player,"Läs liggaren under klockan först.");return true;}
                if(!m.ClockAnchored){m.ReleaseFrom=ClockEchoPosition;m.ClockAnchored=true;m.Departure=0;Emit("radio",Player,"meridian-open");Emit("room-sound",Player,"door-unlock");Emit("checkpoint",Player);}return true;
            }
            return true;
        }
        if(near(Meridian.Plate))
        {
            if(!m.PlateSet){m.PlateSet=true;Emit("radio",Player,"meridian-plate");Emit("radio",Player,"meridian-fight");Emit("radio",Player,"meridian-tactic");Emit("checkpoint",Player);}
            Emit("campaign",Player,"STJÄRNPLÅTENS MÄRKEN: Sol mot öster. Äpple mot väster. Nyckel mot söder. Rätta det instrument som väktaren vrider. Då bryts hans skydd en kort stund. Plåten tillhör fortfarande expeditionen.");return true;
        }
        for(int i=0;i<3;i++)if(near(Meridian.Controls[i]))
        {
            if(!m.PlateSet){Emit("room-notice",Player,"Jämför först plåten vid bordet.");return true;}
            if(m.WardenDefeated){Emit("room-notice",Player,"Instrumentet mäter nästa minut.");return true;}
            if(m.Exposed>0){Emit("room-notice",Player,"Verket tappar takten · väktaren är oskyddad.");return true;}
            if(i!=m.Breaks%3){Emit("room-notice",Player,"Detta instrument står stilla. Rätta det som väktaren vrider.");return true;}
            m.Angles[i]=(m.Angles[i]+1)%4;Emit("room-sound",Player,"meridian-turn");
            Emit("room-notice",Player,$"{Uppsala.Names[i]} · {Uppsala.Directions[m.Angles[i]]}");
            if(m.Angles[i]==Meridian.Target[i])
            {m.Exposed=8;m.Breaks++;int next=m.Breaks%3;m.Angles[next]=(Meridian.Target[next]+1)%4;Hazards.RemoveAll(h=>h.Meridian);Emit("inscription",Player,"MÄTLINJEN BRYTS");}
            Emit("checkpoint",Player);return true;
        }
        if(near(Meridian.Order))
        {
            if(!m.WardenDefeated){Emit("room-notice",Player,"Väktaren håller kvar dagens order.");return true;}
            if(!m.OrderTaken){m.OrderTaken=true;DropItem("memory",Meridian.Order);Emit("radio",Player,"meridian-order");Emit("radio",Player,"meridian-stop");Emit("checkpoint",Player);}
            Emit("campaign",Player,"FÖRFLYTTNINGSORDER: Ett helt kvarter skall flyttas vid gryningen. Målet är överstruket; originalet förvaras i det övre observatoriet. Väktaren lever men har lagt ned mätstaven. Nästa port är förseglad inifrån. Ta handlingen till expeditionen innan du går vidare.");return true;
        }
        return true;
    }
    public System.Collections.Generic.IEnumerable<Vector2> MeridianStrikePoints(Fighter e)
    {
        var axis=Normal(e.LockedAim-e.Position,Vector2.UnitY);var side=new Vector2(-axis.Y,axis.X);
        for(int i=-1;i<=1;i++){var p=e.LockedAim+side*i*95;if(OnWalkable(p)&&ClearPath(e.Position,p))yield return p;}
    }
    private void StepMeridianWarden(Fighter e,float dt)
    {
        e.Hurt=Math.Max(0,e.Hurt-dt);e.Moving=false;
        if(!InMeridian||Rooms!.Current!=Meridian.Hall)return;var m=MeridianState;
        if(!m.PlateSet)return;e.Cooldown=Math.Max(0,e.Cooldown-dt);e.Facing=Normal(Player-e.Position,e.Facing);
        if(!m.SecondPhase&&e.Health<e.MaxHealth*.5f){m.SecondPhase=true;Emit("radio",Player,"meridian-rage");}
        if(m.Exposed>0){e.State=3;e.Timer=m.Exposed;return;}
        if(e.State==3){e.State=0;e.Cooldown=1.5f;}
        if(e.State==0)
        {if(e.Cooldown<=0&&ClearPath(e.Position,Player)){e.State=1;e.Timer=m.SecondPhase?1:1.35f;e.LockedAim=Player;Emit("warning",e.Position);}return;}
        e.Timer-=dt;if(e.Timer>0)return;
        if(e.State==1)
        {
            foreach(var p in MeridianStrikePoints(e))Hazards.Add(new(){Position=p,Timer=.7f,Radius=52,Meridian=true});
            e.State=2;e.Timer=m.SecondPhase?1.8f:2.4f;Emit("room-sound",e.Position,"meridian-turn");
        }
        else{e.State=0;e.Cooldown=.8f;}
    }
    private void ValidateMeridian()
    {
        if(!InConnectedWorld)return;var m=MeridianState;
        if(m is null||!float.IsFinite(m.Dawn)||m.Dawn<0||m.Dawn>12||!float.IsFinite(m.ReleaseFrom.X)||!float.IsFinite(m.ReleaseFrom.Y)||m.Angles is null||m.Angles.Length!=3||m.Angles.Any(a=>a<0||a>3)||m.Cycles<0||m.Cycles>1000||m.Breaks<0||m.Breaks>100000||!float.IsFinite(m.ClockTime)||m.ClockTime<0||m.ClockTime>12||!float.IsFinite(m.Departure)||m.Departure<0||m.Departure>8||!float.IsFinite(m.Exposed)||m.Exposed<0||m.Exposed>8||
          (m.CourtOpen&&!UppsalaState.KeyTaken)||(Rooms!.Rooms[Meridian.Clock].Visited&&!m.CourtOpen)||(m.ClockStarted&&!Rooms.Rooms[Meridian.Clock].Visited)||(m.ClockAnchored&&(!m.LedgerRead||m.Cycles<2))||(Rooms.Rooms[Meridian.Hall].Visited&&!m.ClockAnchored)||(m.PlateSet&&!Rooms.Rooms[Meridian.Hall].Visited)||(m.WardenDefeated&&!m.PlateSet)||(m.OrderTaken&&!m.WardenDefeated))throw new System.IO.InvalidDataException("Ogiltigt meridiantillstånd");
        var boss=Enemies.Where(e=>e.Kind==EnemyKind.MeridianWarden).ToArray();if(Rooms!.Rooms[Meridian.Hall].Visited?(boss.Length!=1||boss[0].Dead!=m.WardenDefeated):boss.Length!=0)throw new System.IO.InvalidDataException("Ogiltig meridianväktare");
    }
}
