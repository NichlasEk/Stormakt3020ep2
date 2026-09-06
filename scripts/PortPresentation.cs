using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main
{
    private Texture2D _portStone=null!,_portProps=null!,_portFloorCache=null!,_portWallCache=null!;
    private Node2D? _portDrawingTarget;
    private Node2D PortInk=>_portDrawingTarget??this;
    private readonly Dictionary<(float,float,float),Rect2> _portWallRects=new();
    private static Vector2 Iso(float u,float v)=>G(PortLayout.At(u,v));
    private void LoadPort()
    {
        _portStone=GD.Load<Texture2D>("res://assets/art/port-stone-v1.png");
        _portProps=GD.Load<Texture2D>("res://assets/art/port-props-v1.png");
        Texture2D Cache(Vector2I size,Action<Node2D> paint)
        {
            var viewport=new SubViewport{Size=size,TransparentBg=true,Disable3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Once};
            AddChild(viewport);var canvas=new PortCanvas{Paint=paint};viewport.AddChild(canvas);return viewport.GetTexture();
        }
        _portFloorCache=Cache(new Vector2I(1536,1100),canvas=>
        { _portDrawingTarget=canvas;DrawPortFloor();_portDrawingTarget=null; });
        _portWallCache=Cache(new Vector2I(1024,256),canvas=>
        {
            _portDrawingTarget=canvas;int x=0,seed=0;
            foreach(var shape in new[]{(1f,.28f,96f),(.28f,1f,96f),(1f,.28f,42f),(.28f,1f,42f),(.4f,.6f,74f),(.38f,.4f,162f)})
            {
                var (du,dv,height)=shape;var size=new Vector2(Mathf.Ceil((du+dv)*64)+4,Mathf.Ceil((du+dv)*32+height)+4);
                _portWallRects[shape]=new Rect2(new Vector2(x,0),size);
                canvas.DrawSetTransform(new Vector2(x+dv*64+2,height+2)-Iso(0,0));
                PortBlock(0,0,du,dv,height,seed++);x+=(int)size.X+2;
            }
            canvas.DrawSetTransform(Vector2.Zero);_portDrawingTarget=null;
        });
    }
    // All outlines and UVs are authored as 2D polygons. No model, light or camera in 3D.
    private void StoneFace(Vector2[] points,int seed,Color tint,bool outline=true)
    {
        float u=(seed*37%71)/100f,v=(seed*17%71)/100f;
        var uv=new[]{new Vector2(u,v),new Vector2(u+.24f,v),new Vector2(u+.24f,v+.24f),new Vector2(u,v+.24f)};
        PortInk.DrawPolygon(points,new[]{tint},uv,_portStone);
        if(outline)PortInk.DrawPolyline(points.Append(points[0]).ToArray(),new Color("302e29"),1,true);
    }
    private void DrawPortFloor()
    {
        PortInk.DrawRect(new Rect2(-1500,-1500,4500,4000),new Color("111b1c"));
        // Repeated two-dimensional water strokes outside the masonry platform.
        for(int i=0;i<70;i++)
        {
            float x=80+(i*197%1380),y=300+i*11;
            PortInk.DrawLine(new Vector2(x,y),new Vector2(x+25+i%34,y),new Color(.18f,.23f,.22f,.25f),1);
        }
        PortInk.DrawColoredPolygon(PortLayout.Ground.Select(G).ToArray(),new Color("302f27"));
        // Staggered half-metre flags share exact outside boundaries; inset chips break the grid inside.
        for(int row=0;row<30;row++)for(int column=-1;column<20;column++)
        {
            float a=Math.Max(0,column*.5f+(row%2)*.25f),right=Math.Min(10,(column+1)*.5f+(row%2)*.25f);
            if(right<=a)continue;
            float b=row/3f,bottom=(row+1)/3f;
            int seed=(row*47+(column+1)*19)%997;
            float gap=.010f+(seed%3)*.003f;
            bool lane=Math.Abs(a-b)<.9f;
            float value=(lane?1.04f:.87f)+(seed%7)*.014f;
            StoneFace(new[]{Iso(a+gap,b+gap),Iso(right-gap,b+gap),Iso(right-gap,bottom-gap),Iso(a+gap,bottom-gap)},seed,new Color(value,value*.98f,value*.89f),false);
            if(seed%17==0)
            {
                var p=Iso(a+.12f,b+.07f);
                PortInk.DrawPolyline(new[]{p,p+new Vector2(6,4),p+new Vector2(1,8),p+new Vector2(8,12)},new Color(.14f,.15f,.13f,.6f),1,true);
            }
        }
        // Entry stair nosings follow the same diagonals as the tiles.
        for(int step=0;step<5;step++)
        {
            float v=8.8f+step*.19f;
            PortInk.DrawLine(Iso(8.1f,v),Iso(9.5f,v),new Color("777363"),1.4f,true);
        }
        // The main route leads to a bronze threshold rather than a floating target.
        PortInk.DrawLine(Iso(.55f,1.3f),Iso(1.9f,1.3f),Gold,3,true);
        for(int i=0;i<3;i++)
        {
            var p=G(Expedition.Nodes[i]);
            PortInk.DrawArc(p,33,0,Mathf.Tau,40,new Color(Gold,.35f),1.3f,true);
        }
    }
    private void PortBlock(float u,float v,float du,float dv,float height,int seed)
    {
        var a=Iso(u,v);var b=Iso(u+du,v);var c=Iso(u+du,v+dv);var d=Iso(u,v+dv);var h=new Vector2(0,height);
        void Courses(Vector2 left,Vector2 right,float shade,int salt)
        {
            PortInk.DrawColoredPolygon(new[]{left-h,right-h,right,left},new Color("262b27"));
            int rows=Math.Max(2,(int)(height/19));
            int count=Math.Max(1,(int)(left.DistanceTo(right)/28));
            for(int row=0;row<rows;row++)for(int brick=-1;brick<count;brick++)
            {
                float a=Math.Max(0,(brick+(row%2)*.5f)/count),b=Math.Min(1,(brick+1+(row%2)*.5f)/count);
                if(b<=a)continue;
                var lo=left.Lerp(right,a)+new Vector2(.45f,-height*row/rows-1);
                var ro=left.Lerp(right,b)+new Vector2(-.45f,-height*row/rows-1);
                var lift=new Vector2(0,height/rows-2);
                float value=shade+(seed*3+row*7+brick+3)%5*.027f;
                StoneFace(new[]{lo-lift,ro-lift,ro,lo},seed+row*11+brick+salt,new Color(value,value*1.01f,value*.93f),false);
                PortInk.DrawLine(lo-lift,ro-lift,new Color(.47f,.47f,.38f,.32f),.8f,true);
            }
        }
        Courses(d,c,.79f,3);Courses(c,b,.68f,7);
        StoneFace(new[]{a-h,b-h,c-h,d-h},seed+13,new Color(1.07f,1.04f,.93f));
    }
    private void DrawCachedPortFloor()
    {
        DrawRect(new Rect2(-1500,-1500,4500,4000),new Color("111b1c"));
        DrawTexture(_portFloorCache,Vector2.Zero);
    }
    private void PortProp(int kind,Vector2 at,float height,Color? tint=null)
    {
        // Reviewed alpha bounds from the original direct-2D atlas, with two pixels of padding.
        var sources=new[]{new Rect2(14,875,277,254),new Rect2(410,839,118,255),new Rect2(671,895,225,212),new Rect2(994,788,237,368)};
        var source=sources[kind];var size=source.Size*(height/source.Size.Y);
        DrawTextureRectRegion(_portProps,new Rect2(at-new Vector2(size.X/2,size.Y-3),size),source,tint??Colors.White);
    }
    private void AddPortLayers(List<(float Depth,Action Draw)> layers)
    {
        // Segment baselines interleave with feet; a long diagonal wall is never one giant overlay.
        void Block(float u,float v,float du,float dv,float height,int seed)
        {
            float depth=Iso(u+du/2,v+dv/2).Y;
            layers.Add((depth,()=>
            {
                if(!_portWallRects.TryGetValue((du,dv,height),out var source))return;
                var position=Iso(u,v)+new Vector2(-dv*64-2,-height-2);
                DrawTextureRectRegion(_portWallCache,new Rect2(position,source.Size),source);
            }));
        }
        for(int i=0;i<10;i++)
        {
            Block(i,-.28f,1,.28f,96,i);
            Block(-.28f,i,.28f,1,96,20+i);
            Block(i,10,1,.28f,42,40+i);
            Block(10,i,.28f,1,42,60+i);
        }
        for(int i=0;i<6;i++)Block(2,5+i*.6f,.4f,.6f,PortLayout.WallHeight,80+i);
        // Two masonry gateposts at the threshold; the opening itself remains traversable.
        Block(.25f,1.03f,.38f,.4f,162,92);Block(1.95f,1.03f,.38f,.4f,162,93);
        var start=Iso(.63f,1.23f);var end=Iso(1.95f,1.23f);
        layers.Add(((start.Y+end.Y)/2,()=>
        {
            if(!_game.CampaignReady)
                for(int i=0;i<9;i++){var p=start.Lerp(end,i/8f);DrawLine(p-new Vector2(0,119),p-new Vector2(0,7),new Color("514d3c"),4,true);}
            DrawLine(start-new Vector2(0,125),end-new Vector2(0,125),new Color("555448"),12,true);
        }));
        for(int i=0;i<3;i++)
        {
            int index=i;var at=G(Expedition.Nodes[i]);
            layers.Add((at.Y,()=>PortProp(index==1?3:0,at,index==1?96:44,(_game.CampaignMask&(1<<index))!=0?new Color(1.05f,1,.82f):Colors.White)));
        }
        var cache=G(PortLayout.Cache);
        layers.Add((cache.Y,()=>PortProp(2,cache,55,_game.PortCacheTaken?new Color(.5f,.5f,.46f):Colors.White)));
        foreach(var at in new[]{Iso(3.1f,8.7f),Iso(7.8f,3),Iso(1.05f,6.2f)})
        {
            var p=at;layers.Add((p.Y,()=>PortProp(1,p,30)));
        }
    }
}
