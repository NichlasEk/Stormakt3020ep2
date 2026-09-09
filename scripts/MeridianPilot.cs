using System.Linq;
using System.Numerics;
namespace Atland;
// Input-only regression driver; never enabled during ordinary play.
public static class MeridianPilot
{
    public static Controls Decide(Combat g)
    {
        var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.MeridianWarden);var m=g.MeridianState;
        var target=m.Exposed>0?boss.Position:Meridian.Controls[m.Breaks%3];var delta=boss.Position-g.Player;
        var move=Vector2.Distance(g.Player,target)>48?Combat.Normal(g.NextWaypoint(g.Player,target)-g.Player,Vector2.UnitX):Vector2.Zero;
        var danger=g.Hazards.FirstOrDefault(h=>h.Meridian&&h.Timer<.5f&&Vector2.Distance(g.Player,h.Position)<h.Radius+20);
        if(danger!=null)move=Combat.Normal(g.Player-danger.Position,Vector2.UnitY);
        bool attack=m.Exposed>0&&delta.Length()<90&&g.ClearPath(g.Player,boss.Position)&&danger==null;
        return new(move,Combat.Normal(delta,Vector2.UnitX),attack,false,danger!=null&&danger.Timer<.25f,false,false,g.Health<50,m.Exposed>0,m.Exposed<=0&&Vector2.Distance(g.Player,target)<=48&&g.Tick%3==0);
    }
}
