using Atland;
using System.Linq;
using System.Numerics;

// Deterministic input driver for --oath-check and regression tests; never runs in normal play.
public static class OathCheckPilot
{
    public static Controls Decide(Combat g,bool optionalCistern=false)
    {
        var foe=g.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,g.Player)).FirstOrDefault();
        Vector2 target=g.RoomObjective;
        if(foe!=null)target=foe.Position;
        else if(optionalCistern&&g.Rooms!.Current==PortRooms.Pump&&!g.Rooms.RelicTaken&&g.Rooms.WaterLowered)target=RoomLinks.All[2].AtA;
        else if(g.Rooms!.Current==PortRooms.Court&&g.Rooms.ShortcutOpen)target=PortRooms.CourtDoor;
        bool guardian=foe?.Kind==EnemyKind.OathGuardian;
        if(guardian&&foe!.State!=3)
        {
            var pillar=PortRooms.Pillars.OrderBy(p=>Vector2.DistanceSquared(p,foe.Position)).First();
            target=g.Bound(pillar+Combat.Normal(pillar-foe.Position,Vector2.UnitY)*135);
        }
        float distance=Vector2.Distance(g.Player,target);
        var delta=(foe?.Position??target)-g.Player;
        bool guard=!guardian&&foe!=null&&foe.State==1&&foe.Timer<.16f&&delta.Length()<150;
        float stop=guardian&&foe!.State!=3?14:48;
        var move=distance>stop||!g.ClearPath(g.Player,target)?Combat.Normal(g.NextWaypoint(g.Player,target)-g.Player,Vector2.UnitX):Vector2.Zero;
        bool strike=foe!=null&&(!guardian||foe.State==3)&&delta.Length()<95&&g.ClearPath(g.Player,foe.Position)&&!guard;
        return new(move,Combat.Normal(delta,Vector2.UnitX),strike,false,false,guard,false,g.Health<48,foe!=null&&(!guardian||foe.State==3),g.Tick%30==0);
    }
}
