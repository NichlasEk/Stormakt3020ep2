using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
public partial class Main
{
    private void AddOlderArchLayers(List<(float Depth,Action Draw)> layers)
    {
        var art=RoomBackground;
        void Wall(Vector2[] polygon,float x0,float y0,float slope)
        {
            for(float x=polygon.Min(p=>p.X);x<polygon.Max(p=>p.X);x+=10)
            {var strip=ClipDoorStrip(ClipDoorStrip(polygon,x,true),x+10,false);if(strip.Length>=3)layers.Add((y0+(x+5-x0)*slope,()=>PaintForeground(art,strip)));}
        }
        switch(PaintRoom)
        {
            case PortRooms.Court:
                Wall(new Vector2[]{new(0,0),new(395,0),new(395,140),new(405,190),new(345,290),new(0,410)},500,220,-.45f);
                Wall(new Vector2[]{new(540,0),new(710,0),new(710,176),new(548,245),new(540,180)},500,220,-.45f);break;
            case PortRooms.Lodge:
                Wall(new Vector2[]{new(0,0),new(970,0),new(970,305),new(985,355),new(925,403),new(0,690)},990,375,.5f);
                Wall(new Vector2[]{new(1035,0),new(1536,0),new(1536,655),new(1035,395)},1035,395,.5f);
                Wall(new Vector2[]{new(970,0),new(1035,0),new(1035,140),new(1000,116),new(970,133)},990,375,.5f);break;
            case PortRooms.Pump:
                Wall(new Vector2[]{new(700,0),new(1150,0),new(1150,335),new(1042,300),new(1042,160),new(1016,110),new(986,109),new(954,143),new(944,252),new(700,310)},1000,280,.48f);
                Wall(new Vector2[]{new(1200,0),new(1536,0),new(1536,550),new(1440,430),new(1440,320),new(1410,285),new(1385,310),new(1370,410),new(1200,360)},1390,430,.5f);break;
            case PortRooms.Cistern:
                Wall(new Vector2[]{new(800,0),new(1150,0),new(1150,300),new(1008,270),new(1008,130),new(985,102),new(960,110),new(940,160),new(932,250),new(800,295)},980,255,.3f);
                Wall(new Vector2[]{new(1250,0),new(1536,0),new(1536,720),new(1480,515),new(1470,260),new(1398,240),new(1398,450),new(1250,430)},1420,455,.48f);break;
            case PortRooms.Gallery:
                Wall(new Vector2[]{new(0,0),new(215,0),new(215,440),new(0,575)},260,420,-.5f);
                Wall(new Vector2[]{new(315,0),new(455,0),new(455,350),new(330,425),new(315,360)},260,420,-.5f);
                Wall(new Vector2[]{new(215,0),new(315,0),new(315,300),new(291,280),new(267,286),new(240,315),new(215,360)},260,420,-.5f);break;
            case PortRooms.Chamber:
                Wall(new Vector2[]{new(500,0),new(1100,0),new(1100,320),new(930,320),new(930,100),new(880,50),new(740,50),new(680,90),new(680,320),new(500,320)},810,320,0);break;
            case PortRooms.Archive:
                Wall(new Vector2[]{new(0,0),new(85,0),new(85,500),new(0,570)},135,470,-.5f);
                Wall(new Vector2[]{new(175,0),new(370,0),new(370,420),new(190,490),new(190,310),new(170,255),new(145,245),new(115,270),new(85,360),new(85,0)},135,470,-.5f);break;
        }
    }
}
