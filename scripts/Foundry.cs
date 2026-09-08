using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Foundry
{
    public const string Room="mine-foundry";
    public static readonly Vector2 Gate=new(1300,560),Valve=new(450,580),Plate=new(1000,570),Spawn=new(860,600);
    public static readonly Vector2[] Ground={new(95,400),new(620,290),new(1050,320),new(1450,470),new(1460,600),new(1050,840),new(800,940),new(330,740),new(95,510)};
    public static readonly Vector2[][] Obstacles={new Vector2[]{new(920,405),new(1000,380),new(1080,430),new(1060,505),new(980,525),new(920,490)}};
}
public sealed class FoundryRun
{
    public bool GateOpen,BailiffDefeated,PlateTaken,SecondPhase;
    public float Cooling,CoolingCooldown;
    public int Quenches;
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InFoundry=>InConnectedWorld&&Rooms!.Current==Foundry.Room;
    [JsonIgnore] public FoundryRun FoundryState=>Rooms!.Foundry;
    [JsonIgnore] public string FoundryGoal=>!FoundryState.BailiffDefeated?(FoundryState.Cooling>0?"Järnet svalnar · angrip Kronfogden":"Undvik hammaren · använd kylvattnet"):FoundryState.PlateTaken?"Uppsalas stjärnspår är säkrat":"Ta kronans avtryck vid pressen";
    [JsonIgnore] public Vector2 FoundryObjective=>FoundryState.BailiffDefeated?Foundry.Plate:Foundry.Valve;
    public static Combat NewFoundryPreview(Order order)
    {
        var g=NewMinePreview(order);var shift=g.WorldOrigin-Mine.Origin(Mine.Coolway);
        foreach(var e in g.Enemies){e.Position+=shift;e.LockedAim+=shift;}
        foreach(var id in Mine.Ids.Take(3))g.Rooms!.Rooms[id].Visited=true;
        g.Rooms!.Current=Mine.Coolway;g.Player=new(1170,650);
        var m=g.MineState;m.EntranceOpen=m.LedgerRead=m.FeedClosed=m.PressureReleased=m.ImprintTaken=true;
        g.Events.Clear();g.UpdateRoomSight(true);g.ValidateRooms();return g;
    }
    private void EnterFoundry()
    {Spawn(EnemyKind.CrownBailiff,Foundry.Spawn);Emit("radio",Player,"foundry-bailiff");Emit("radio",Player,"foundry-tactic");}
    private void StepFoundryClock(float dt)
    {
        if(!InConnectedWorld)return;var f=FoundryState;
        f.Cooling=Math.Max(0,f.Cooling-dt);f.CoolingCooldown=Math.Max(0,f.CoolingCooldown-dt);
    }
    private bool StepFoundry(Func<Vector2,bool> near)
    {
        if(!InConnectedWorld)return false;var f=FoundryState;
        if(Rooms!.Current==Mine.Coolway&&near(Foundry.Gate))
        {
            if(!MineState.ImprintTaken){Emit("room-notice",Player,"Undersök kronans gjutform innan du lossar porten.");return true;}
            if(!f.GateOpen){f.GateOpen=true;Emit("room-sound",Player,"stone-door");Emit("radio",Player,"foundry-gate");Emit("checkpoint",Player);}return true;
        }
        if(!InFoundry)return false;
        if(near(Foundry.Valve)&&!f.BailiffDefeated)
        {
            if(f.CoolingCooldown>0){Emit("room-notice",Player,$"Kylledningen fylls · {MathF.Ceiling(f.CoolingCooldown)} s");return true;}
            f.Cooling=4;f.CoolingCooldown=12;f.Quenches++;Hazards.RemoveAll(h=>h.Forge);
            var boss=Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.CrownBailiff&&!e.Dead);if(boss!=null){boss.State=3;boss.Timer=4;}
            Emit("room-sound",Player,"mine-steam");Emit("inscription",Player,"JÄRNET SVALNAR");if(f.Quenches==1)Emit("radio",Player,"foundry-cooling");Emit("checkpoint",Player);return true;
        }
        if(near(Foundry.Plate))
        {
            if(!f.BailiffDefeated){Emit("room-notice",Player,"Kronfogden håller pressen. Bryt hans grepp först.");return true;}
            if(!f.PlateTaken){f.PlateTaken=true;DropItem("crown-helm",Foundry.Plate);Emit("radio",Player,"foundry-plate");Emit("radio",Player,"foundry-return");Emit("checkpoint",Player);}
            Emit("campaign",Player,"KRONANS AVTRYCK: Ingen kung anges. Stjärncirklarna pekar mot Uppsala, men himlen i avtrycket är vänd åt fel håll. Hedvig begär hela plåten ombord. Båten ligger kvar vid bergets kaj; vägen tillbaka genom gruvan är öppen.");return true;
        }
        return true;
    }
    public System.Collections.Generic.IEnumerable<Vector2> CrownStampPoints(Fighter e)
    {
        var axis=Normal(e.LockedAim-e.Position,e.Facing);
        for(int i=0;i<(FoundryState.SecondPhase?4:3);i++)
        {var p=e.Position+axis*(90+i*100);if(OnWalkable(p)&&ClearPath(e.Position,p))yield return p;}
    }
    private void StepCrownBailiff(Fighter e,float dt)
    {
        e.Hurt=Math.Max(0,e.Hurt-dt);e.Moving=false;if(!InFoundry)return;
        var f=FoundryState;e.Cooldown=Math.Max(0,e.Cooldown-dt);
        if(!f.SecondPhase&&e.Health<e.MaxHealth*.5f){f.SecondPhase=true;Emit("radio",Player,"foundry-rage");}
        if(f.Cooling>0){e.State=3;e.Timer=f.Cooling;return;}
        var delta=Player-e.Position;
        if(e.State==0)
        {
            e.Facing=Normal(delta,e.Facing);var before=e.Position;
            if(delta.Length()>145)e.Position=MoveBody(e.Position,e.Position+Normal(NextWaypoint(e.Position,Player)-e.Position,e.Facing)*90*dt);
            var distance=Vector2.Distance(before,e.Position);e.Moving=distance>.001f;if(e.Moving)e.MoveDirection=(e.Position-before)/distance;e.Walk+=distance*7/195;
            if(delta.Length()<330&&e.Cooldown<=0&&ClearPath(e.Position,Player)){e.State=1;e.Timer=1.1f;e.LockedAim=Player;Emit("warning",e.Position);}
            return;
        }
        e.Timer-=dt;if(e.Timer>0)return;
        if(e.State==1)
        {
            // Committed stamps leave their sides open and never track Karl.
            int stamp=0;foreach(var p in CrownStampPoints(e))Hazards.Add(new(){Position=p,Timer=.35f+stamp++*.23f,Radius=57,Forge=true});
            if(delta.Length()<165&&ClearPath(e.Position,Player)&&DodgeTime<=0)
            {
                if(Guarding&&GuardTime<.18f&&Vector2.Dot(Facing,Normal(e.Position-Player,Facing))>.15f)
                {Parries++;RememberedParry();e.State=3;e.Timer=2;Emit("parry",Player,"HAMMAREN UR BALANS");return;}
                DamagePlayer(24,e.Position);
            }
            e.State=2;e.Timer=2.2f;Emit("enemystrike",e.Position);
        }
        else{e.State=0;e.Cooldown=.8f;}
    }
    private void ValidateFoundry()
    {
        if(!InConnectedWorld)return;var f=FoundryState;var bosses=Enemies.Where(e=>e.Kind==EnemyKind.CrownBailiff).ToArray();
        if(f is null||!float.IsFinite(f.Cooling)||f.Cooling<0||f.Cooling>4||!float.IsFinite(f.CoolingCooldown)||f.CoolingCooldown<0||f.CoolingCooldown>12||f.Quenches<0
            ||(f.GateOpen&&!MineState.ImprintTaken)||(Rooms!.Rooms[Foundry.Room].Visited&&!f.GateOpen)||(f.PlateTaken&&!f.BailiffDefeated)
            ||(Rooms.Rooms[Foundry.Room].Visited?(bosses.Length!=1||bosses[0].Dead!=f.BailiffDefeated):bosses.Length!=0)
            ||bosses.Any(e=>e.State<0||e.State>3||!float.IsFinite(e.Timer)||!float.IsFinite(e.Cooldown)||!float.IsFinite(e.LockedAim.X)||!float.IsFinite(e.LockedAim.Y)))throw new System.IO.InvalidDataException("Ogiltigt gjuteritillstånd");
    }
}
