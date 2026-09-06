using System.Numerics;
namespace Atland;

// A shared 2D projection for art, navigation and measurements: no 3D scene.
public static class PortLayout
{
    public const float TileWidth=128,TileHeight=64,WallHeight=74;
    public static Vector2 At(float u,float v)=>new(768+64*(u-v),300+32*(u+v));
    public static readonly Vector2[] Ground={At(0,0),At(10,0),At(10,10),At(0,10)};
    public static readonly Vector2[] Wall={At(2,5),At(2.4f,5),At(2.4f,8.6f),At(2,8.6f)};
    public static readonly Vector2[] Collision=Navigation.Expand(Wall,22);
    public static readonly Vector2 Cache=At(1.05f,7.4f);
    public static readonly Vector2[] SideRoute={At(3.3f,9.1f),At(1,9.1f),Cache};
}
