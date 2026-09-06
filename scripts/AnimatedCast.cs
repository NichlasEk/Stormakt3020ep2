using Godot;
using System;
using System.Collections.Generic;

/// <summary>Four directional painted keyframe cycles, anchored to the actual feet.</summary>
public sealed class AnimatedCast : IDisposable
{
    private sealed record Sheet(ImageTexture Texture,Rect2[] Frames,Vector2[] Feet,float[] Scale);
    private readonly Dictionary<string,Sheet> _sheets=new();
    public AnimatedCast()
    {
        foreach(string role in new[]{"karl","guard","karl-hammer","collector","pikeman","gunner"})foreach(string action in (role is "karl" or "guard"?new[]{"walk","attack","react"}:new[]{"walk"}))
        {
            var texture=SpriteCutout.Load($"res://assets/art/{role}-{action}-v1.png",chromaKey:new Color(1,0,1));
            using var image=texture.GetImage();int width=image.GetWidth(),height=image.GetHeight();var data=image.GetData();
            var frames=new Rect2[16];var feet=new Vector2[16];var scales=new float[4];
            for(int row=0;row<4;row++)for(int col=0;col<4;col++)
            {
                var (left,up,right,down)=AnimationAtlasLayout.Cell(role,action,row,col,width,height);
                int top=down,bottom=up,minX=right,maxX=left;
                for(int y=up;y<down;y++)for(int x=left;x<right;x++)if(data[(y*width+x)*4+3]>180)
                {top=Math.Min(top,y);bottom=Math.Max(bottom,y);minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);}
                // An upright pike is taller than its owner. Scale/anchor from the
                // helmet-to-boot body, retaining the entire weapon in the UV region.
                if(role=="pikeman")
                {
                    for(int y=up;y<bottom;y++)
                    {
                        int run=0,best=0;
                        for(int x=left;x<right;x++){if(data[(y*width+x)*4+3]>180){run++;best=Math.Max(best,run);}else run=0;}
                        if(best>(right-left)*.065f){top=y;break;}
                    }
                }
                long sum=0,count=0;
                for(int y=Math.Max(top,bottom-12);y<=bottom;y++)for(int x=left;x<right;x++)if(data[(y*width+x)*4+3]>180){sum+=x;count++;}
                // Anchor a walk to the pelvis, not whichever boot happens to be lowest.
                // Otherwise every alternating step drags the torso sideways.
                if(action=="walk")
                {
                    sum=0;count=0;
                    for(int y=top+(bottom-top)*42/100;y<top+(bottom-top)*62/100;y++)
                    {
                        int run=0,best=0,end=0;
                        for(int x=left;x<right;x++)
                        {if(data[(y*width+x)*4+3]>180){run++;if(run>best){best=run;end=x;}}else run=0;}
                        if(best>12){sum+=end-best/2;count++;}
                    }
                }
                int index=row*4+col;frames[index]=new Rect2(left,up,right-left,down-up);
                feet[index]=action=="react"&&col==3?new Vector2((minX+maxX)/2f-left,(top+bottom)/2f-up):new Vector2(count>0?(float)sum/count-left:(right-left)/2f,bottom-up-2);
                if(col==0)scales[row]=PaintedCast.BodyHeight/Math.Max(1,bottom-top);
            }
            _sheets[role+"-"+action]=new(texture,frames,feet,scales);
        }
    }
    public void Draw(Node2D canvas,string role,Vector2 position,Vector2 facing,string action,int frame,float hurt,bool dead,Vector2 offset,float zoom)
    {
        int row=facing.Y>=0?(facing.X>=0?0:3):(facing.X>=0?1:2);bool flip=false;
        if(role is "karl-hammer" or "gunner" &&action=="walk"){if(row==1)row=2;else if(row==2)row=1;} // Generated back-view rows are reversed.
        // Karl's NW contact was painted facing forward; mirror the correct back view.
        if(role=="karl"&&action=="attack"&&row==2&&frame==2){row=1;flip=true;}
        var sheet=_sheets[role+"-"+action];int index=row*4+Math.Clamp(frame,0,3);float scale=sheet.Scale[row];
        canvas.DrawSetTransform(offset+position*zoom,0,new Vector2(flip?-scale:scale,scale)*zoom);
        canvas.DrawTextureRectRegion(sheet.Texture,new Rect2(-sheet.Feet[index],sheet.Frames[index].Size),sheet.Frames[index],dead?new Color(.65f,.61f,.57f,.85f):hurt>0?new Color(1.15f,1.05f,1):Colors.White);
        canvas.DrawSetTransform(offset,0,Vector2.One*zoom);
    }
    public void Dispose(){foreach(var sheet in _sheets.Values)sheet.Texture.Dispose();}
}
