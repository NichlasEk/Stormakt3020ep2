using Godot;
using System;
using System.Text.Json;

/// Offline-rendered RGBA frames. No Blender, mesh skinning or AI runs in Godot.
public sealed class BakedKarl : IDisposable
{
    public const int FrameCount=30,Cell=384,Columns=6;
    public const float Scale=.6919973f,CycleDistance=84.83417f;
    private readonly Texture2D[] _views=new Texture2D[4];
    private readonly ImageTexture _heads;
    private readonly Vector2[][] _headCenters=new Vector2[4][];
    private static readonly Rect2[] HeadRegions={new(127,32,53,62),new(134,337,51,61),new(98,643,58,65),new(98,945,57,66)};
    private static readonly Vector2 Anchor=new(192,303.0257f);
    public BakedKarl()
    {
        var names=new[]{"se","ne","nw","sw"};
        for(int i=0;i<4;i++)_views[i]=GD.Load<Texture2D>($"res://assets/art/karl-baked-walk-v1-{names[i]}.png");
        _heads=SpriteCutout.Load("res://assets/art/karl-attack-v1.png",chromaKey:new Color(1,0,1));
        using var metadata=JsonDocument.Parse(FileAccess.GetFileAsString("res://assets/story/karl-baked-walk-v1.json"));
        int direction=0;
        foreach(var view in metadata.RootElement.GetProperty("views").EnumerateArray())
        {
            var centers=view.GetProperty("head_centers_px");
            if(centers.GetArrayLength()!=FrameCount)throw new InvalidOperationException("Head anchors must cover the complete gait");
            _headCenters[direction]=new Vector2[FrameCount];int index=0;
            foreach(var center in centers.EnumerateArray())_headCenters[direction][index++]=new(center[0].GetSingle(),center[1].GetSingle());
            direction++;
        }
    }
    public void Draw(Node2D canvas,Vector2 feet,int direction,float distance,Vector2 offset,float zoom)
    {
        float phase=distance/CycleDistance-Mathf.Floor(distance/CycleDistance);
        int frame=Math.Clamp((int)(phase*FrameCount),0,FrameCount-1);
        var region=new Rect2(frame%Columns*Cell,frame/Columns*Cell,Cell,Cell);
        canvas.DrawSetTransform(offset+feet*zoom,0,Vector2.One*Scale*zoom);
        canvas.DrawTextureRectRegion(_views[Math.Clamp(direction,0,3)],new Rect2(-Anchor,region.Size),region);
        var head=HeadRegions[direction];var size=head.Size*.52f;
        canvas.DrawTextureRectRegion(_heads,new Rect2(_headCenters[direction][frame]-Anchor-size*.5f,size),head);
        canvas.DrawSetTransform(offset,0,Vector2.One*zoom);
    }
    public void Dispose(){foreach(var texture in _views)texture?.Dispose();_heads.Dispose();}
}
