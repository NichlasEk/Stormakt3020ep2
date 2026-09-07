using System;
using System.Linq;
using System.Numerics;

namespace Atland;

public sealed partial class Combat
{
    // 0 approach, 1 locked rush warning, 4 rush, 2 recovery, 3 exposed by a pillar.
    // Pillars never break: every failed bait can be retried, including after loading.
    private void StepOathGuardian(Fighter e,float dt)
    {
        e.Hurt=Math.Max(0,e.Hurt-dt);e.Cooldown=Math.Max(0,e.Cooldown-dt);
        bool furious=e.Health<e.MaxHealth*.5f;
        if(e.State==0)
        {
            var delta=Player-e.Position;e.Facing=Normal(delta,e.Facing);
            if(delta.Length()>350)
            {
                var before=e.Position;
                e.Position=Bound(e.Position+Normal(NextWaypoint(e.Position,Player)-e.Position,e.Facing)*75*dt);
                var move=e.Position-before;e.Moving=move.LengthSquared()>.001f;
                if(e.Moving){e.MoveDirection=Vector2.Normalize(move);e.Walk+=move.Length()*7/195;}
            }
            if(e.Cooldown<=0&&delta.Length()<410)
            {
                e.State=1;e.LockedAim=Player;e.Facing=Normal(Player-e.Position,e.Facing);
                e.Timer=furious?.95f:1.25f;e.ChargeHit=false;Emit("warning",e.Position);
            }
            return;
        }
        if(e.State==4)
        {
            // Small swept steps prevent tunnelling through stone or Karl at low frame rates.
            float travel=(furious?510:420)*Math.Min(dt,e.Timer);
            int steps=Math.Max(1,(int)MathF.Ceiling(travel/5));
            for(int i=0;i<steps;i++)
            {
                var next=e.Position+e.Facing*(travel/steps);
                bool pillar=Rooms?.Current==PortRooms.Chamber&&PortRooms.OathObstacles.Any(o=>Navigation.Contains(o,next));
                if(pillar||!OnWalkable(next))
                {
                    e.State=pillar?3:2;e.Timer=pillar?2.8f:1.15f;e.Cooldown=1.1f;
                    Emit(pillar?"parry":"slam",e.Position,pillar?"EDEN VACKLAR · ANGRIP":"",pillar?0:45);
                    if(pillar)HitStop=.08f;
                    return;
                }
                e.Position=next;
                if(!e.ChargeHit&&Vector2.Distance(Player,next)<43&&ClearPath(next,Player))
                {DamagePlayer(furious?30:24,next);e.ChargeHit=true;}
            }
            e.Timer-=dt;
            if(e.Timer<=0){e.State=2;e.Timer=furious?.85f:1.2f;e.Cooldown=1.15f;}
            return;
        }
        e.Timer-=dt;
        if(e.Timer>0)return;
        if(e.State==1){e.State=4;e.Timer=1.15f;Emit("enemystrike",e.Position,"",50);}
        else {e.State=0;e.Cooldown=.6f;}
    }
}
