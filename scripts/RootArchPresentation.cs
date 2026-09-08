using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main
{
    private void AddRootArchLayers(List<(float Depth,Action Draw)> layers)
    {
        // Original painting UVs keep every root and stone registered with the
        // background. Apertures remain holes; only the surrounding wall sorts
        // over a character walking behind it, in narrow strips along its base.
        var art=RoomBackground;
        void Wall(Vector2[] polygon,float baseX,float baseY,float slope)
        {
            for(float x=polygon.Min(p=>p.X);x<polygon.Max(p=>p.X);x+=10)
            {
                var strip=ClipDoorStrip(ClipDoorStrip(polygon,x,true),x+10,false);
                if(strip.Length<3)continue;
                layers.Add((baseY+(x+5-baseX)*slope,()=>PaintForeground(art,strip)));
            }
        }
        if(PaintRoom==PortRooms.Archive)
            Wall(new Vector2[]{new(1250,0),new(1536,0),new(1536,535),new(1474,505),new(1474,373),new(1460,348),new(1438,333),new(1418,349),new(1405,378),new(1405,467),new(1250,428)},1440,480,.38f);
        if(PaintRoom==PortRooms.Roots)
        {
            Wall(new Vector2[]{new(1200,0),new(1536,0),new(1536,586),new(1468,535),new(1468,474),new(1455,440),new(1435,418),new(1414,427),new(1393,450),new(1393,499),new(1200,473)},1430,510,.5f);
            Wall(new Vector2[]{new(0,0),new(320,0),new(320,360),new(169,462),new(165,370),new(99,395),new(97,474),new(0,530)},135,465,-.48f);
        }
        if(PaintRoom==PortRooms.Grove)
            Wall(new Vector2[]{new(0,0),new(360,0),new(360,390),new(230,451),new(224,369),new(213,343),new(192,331),new(174,336),new(157,355),new(149,382),new(150,457),new(0,540)},190,453,-.46f);
    }
}
