using Godot;
using System;
using System.Collections.Generic;

/// <summary>Approved painted combat poses. Three poses with horizontal facing;
/// full walk cycles and eight-way animation remain a separate production pass.</summary>
public sealed class PaintedCast : IDisposable
{
    public const float BodyHeight=150;
    private readonly ImageTexture _cast,_specialists;
    private readonly Dictionary<string,(Texture2D Texture,Rect2[] Frames,Vector2[] Feet,float Scale)> _roles=new();
    public PaintedCast()
    {
        _cast=SpriteCutout.Load("res://assets/art/serious-cast-v6.png",chromaKey:new Color(1,0,1));
        _specialists=SpriteCutout.Load("res://assets/art/serious-specialists-v1.png",chromaKey:new Color(1,0,1));
        Add("karl-saber",_cast,new[]{new Rect2(0,0,512,500),new Rect2(512,0,512,500),new Rect2(1024,0,512,500)},
            new[]{new Vector2(260,447),new Vector2(170,444),new Vector2(114,444)},425);
        Add("guard",_cast,new[]{new Rect2(0,505,490,510),new Rect2(490,505,445,510),new Rect2(950,505,586,510)},
            new[]{new Vector2(270,445),new Vector2(210,430),new Vector2(143,405)},440);
        using var image=_specialists.GetImage();
        int w=image.GetWidth()/3,h=image.GetHeight()/4;
        var data=image.GetData();int stride=image.GetWidth();
        string[] names={"karl-hammer","pikeman","gunner","collector"};
        for(int row=0;row<4;row++)
        {
            var frames=new Rect2[3];var feet=new Vector2[3];int body=1;
            for(int col=0;col<3;col++)
            {
                frames[col]=new Rect2(col*w,row*h,w,h);
                int top=h,bottom=0;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                    if(data[((row*h+y)*stride+col*w+x)*4+3]>180){top=Math.Min(top,y);bottom=Math.Max(bottom,y);}
                long sum=0,count=0;
                for(int y=Math.Max(top,bottom-18);y<=bottom;y++)for(int x=0;x<w;x++)
                    if(data[((row*h+y)*stride+col*w+x)*4+3]>180){sum+=x;count++;}
                feet[col]=new Vector2(count>0?(float)sum/count:w/2f,bottom-3);
                if(col==0)body=bottom-top;
            }
            Add(names[row],_specialists,frames,feet,body);
        }
    }
    private void Add(string name,Texture2D texture,Rect2[] frames,Vector2[] feet,float body)
        =>_roles[name]=(texture,frames,feet,BodyHeight/body);

    public void Draw(Node2D canvas,string kind,Vector2 position,Vector2 facing,int pose,float hurt,bool dead,Vector2 offset,float zoom)
    {
        var role=_roles[kind];int frame=pose==3?1:pose==4?2:0;
        bool flip=facing.X<-.1f;
        float scale=role.Scale;
        var tint=dead?new Color(.48f,.45f,.41f,.72f):hurt>0?new Color(1.5f,1.3f,1.1f):Colors.White;
        canvas.DrawSetTransform(offset+position*zoom,dead?Mathf.Pi*.48f:0,
            new Vector2(flip?-scale:scale,dead?scale*.43f:scale)*zoom);
        canvas.DrawTextureRectRegion(role.Texture,new Rect2(-role.Feet[frame],role.Frames[frame].Size),role.Frames[frame],tint);
        canvas.DrawSetTransform(offset,0,Vector2.One*zoom);
    }
    public void Dispose(){_cast.Dispose();_specialists.Dispose();}
}
