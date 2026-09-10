using System.Linq;
using System.Numerics;
namespace Atland;
public static class WestPilot
{
    public static Controls Decide(Combat g)
    {
        var boss=g.Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.MusterOfficer&&!e.Dead&&e.HomeRoom==g.Rooms!.Current);
        if(boss==null)return ObservatoryPilot.Decide(g);
        if(boss.State==1&&Vector2.Distance(g.Player,boss.LockedAim)<110)
        {var away=Combat.Normal(g.Player-boss.LockedAim,new Vector2(0,1));return new(away,Combat.Normal(boss.Position-g.Player,Vector2.UnitX),false,false,boss.Timer<.4f,false,false,g.Health<55,false,false);}
        if(g.WestState.Exposed<=0)
        {
            var p=West.Brakes[(g.WestState.Brakes&1)==0?0:1];var delta=p-g.Player;
            return new(delta.Length()>35?Combat.Normal(g.NextWaypoint(g.Player,p)-g.Player,Vector2.UnitY):Vector2.Zero,Vector2.UnitY,false,false,false,false,false,g.Health<55,false,delta.Length()<55&&g.Tick%2==0);
        }
        return ObservatoryPilot.Decide(g);
    }
}
