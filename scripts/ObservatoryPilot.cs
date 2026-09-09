using System.Linq;
using System.Numerics;
namespace Atland;
public static class ObservatoryPilot
{
    public static Controls Decide(Combat g)
    {
        var target=g.Enemies.Where(e=>!e.Dead&&e.HomeRoom==g.Rooms!.Current).OrderBy(e=>e.Kind==EnemyKind.ZenithGuardian?1:0).ThenBy(e=>Vector2.DistanceSquared(e.Position,g.Player)).FirstOrDefault();
        if(target==null)return default;
        var delta=target.Position-g.Player;var move=delta.Length()>57?Combat.Normal(g.NextWaypoint(g.Player,target.Position)-g.Player,Vector2.UnitX):Vector2.Zero;
        var danger=g.Hazards.FirstOrDefault(h=>!h.Friendly&&Vector2.Distance(g.Player,h.Position)<h.Radius+30);
        if(danger!=null)move=Combat.Normal(g.Player-danger.Position,Vector2.UnitY);
        bool attack=danger==null&&delta.Length()<83&&g.ClearPath(g.Player,target.Position);
        return new(move,Combat.Normal(delta,Vector2.UnitX),attack,false,danger!=null&&danger.Timer<.35f,false,false,g.Health<55,attack,false);
    }
}
