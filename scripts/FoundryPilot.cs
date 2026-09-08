using System.Linq;
using System.Numerics;
namespace Atland;
// Input-only driver used by native and regression checks, never during normal play.
public static class FoundryPilot
{
    public static Controls Decide(Combat g,bool cooling=true)
    {
        var boss=g.Enemies.First(e=>e.Kind==EnemyKind.CrownBailiff);
        bool water=cooling&&g.FoundryState.CoolingCooldown<=0;
        var target=water?Foundry.Valve:boss.Position;var delta=boss.Position-g.Player;
        var move=Vector2.Distance(g.Player,target)>55?Combat.Normal(g.NextWaypoint(g.Player,target)-g.Player,Vector2.UnitX):Vector2.Zero;
        bool guard=boss.State==1&&boss.Timer<.17f&&delta.Length()<165;
        var hazard=g.Hazards.FirstOrDefault(h=>!h.Friendly&&h.Timer<.5f&&Vector2.Distance(h.Position,g.Player)<h.Radius+22);
        if(hazard!=null)move=Combat.Normal(g.Player-hazard.Position,Vector2.UnitY);
        bool attack=!water&&!guard&&hazard==null&&delta.Length()<85&&g.ClearPath(g.Player,boss.Position);
        return new(move,Combat.Normal(delta,Vector2.UnitX),attack,false,hazard!=null&&hazard.Timer<.3f,guard,false,g.Health<45,boss.State is 2 or 3,water&&Vector2.Distance(g.Player,Foundry.Valve)<=55&&g.Tick%3==0);
    }
}
