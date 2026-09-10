using System.Linq;
using System.Numerics;
namespace Atland;
public static class SaltPilot
{
    public static Controls Decide(Combat g)
    {
        var boss=g.Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.SaltWarden&&!e.Dead&&e.HomeRoom==g.Rooms!.Current);
        if(boss==null)return ObservatoryPilot.Decide(g);
        var aim=Combat.Normal(boss.Position-g.Player,Vector2.UnitX);
        if(boss.State==1&&Vector2.Distance(g.Player,boss.LockedAim)<135)
        {
            var away=Combat.Normal(g.Player-boss.LockedAim,g.Player.X<800?-Vector2.UnitX:Vector2.UnitX);
            if(!g.ClearPath(g.Player,g.Player+away*65))away=Vector2.UnitY;
            return new(away,aim,false,false,boss.Timer<.3f,false,false,g.Health<55,false,false);
        }
        if(g.SaltState.Tide<9&&Salt.Flooded(g.Player,g.SaltState.Chains))return new(Combat.Normal(new Vector2(790,g.Player.Y)-g.Player,Vector2.UnitX),aim,false,false,false,false,false,g.Health<55,false,false);
        return ObservatoryPilot.Decide(g);
    }
}
