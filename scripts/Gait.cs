using System;
using System.Numerics;
namespace Atland;

/// Visual gait only; preserve saved distance counters and combat movement.
public static class Gait
{
    public static Vector2 Facing(Vector2 wanted,Vector2 previous)
    {
        if(wanted.LengthSquared()<.0001f)return previous;
        wanted=Vector2.Normalize(wanted);
        float Axis(float next,float last)=>MathF.Abs(next)<.18f?(last<0?-.18f:.18f):next;
        return new(Axis(wanted.X,previous.X),Axis(wanted.Y,previous.Y));
    }
    // Walk is historically distance * 7 / 195; sample in world units so
    // slower actors do not hold a lifted boot for half a second.
    public static int Footfall(float walk)=>(int)(Math.Max(0,walk)*195/7/44);
    public static int Frame(float walk,float cycleDistance=88)=>
        (int)(Math.Max(0,walk)*195/7/cycleDistance*4)%4;
}
