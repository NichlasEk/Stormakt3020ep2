using Godot;
using System;

/// <summary>Four painted poses; fixed body scale, explicit anchors, no deformed limbs.</summary>
public sealed class OathCast : IDisposable
{
    private readonly ImageTexture _texture=SpriteCutout.Load("res://assets/art/oath-guardian-v1.png",chromaKey:new Color(1,0,1));
    private static readonly Vector2[] Feet={new(397,463),new(246,461),new(384,409),new(309,380)};
    public void Draw(Node2D canvas,Vector2 at,Vector2 facing,int pose,float hurt,Vector2 offset,float zoom)
    {
        float scale=160f/451;var cell=new Vector2(_texture.GetWidth()/2f,_texture.GetHeight()/2f);
        var source=new Rect2(new Vector2(pose%2,pose/2)*cell,cell);
        canvas.DrawSetTransform(offset+at*zoom,0,new Vector2(facing.X<0?-scale:scale,scale)*zoom);
        canvas.DrawTextureRectRegion(_texture,new Rect2(-Feet[pose],cell),source,pose==3?new Color(.65f,.62f,.57f):hurt>0?new Color(1.35f,1.2f,1.1f):Colors.White);
        canvas.DrawSetTransform(offset,0,Vector2.One*zoom);
    }
    public void Dispose()=>_texture.Dispose();
}
