using Atland;
using Godot;
using System;
using System.Collections.Generic;

public partial class Main
{
    private OathCast? _oathCast;
    private void DrawOathActor(Fighter e)
    {
        var p=RenderPosition(e);
        var shadow=new Vector2[24];
        for(int i=0;i<shadow.Length;i++)shadow[i]=p+new Vector2(Mathf.Cos(i*Mathf.Tau/shadow.Length)*29,Mathf.Sin(i*Mathf.Tau/shadow.Length)*11);
        DrawColoredPolygon(shadow,new Color(.01f,.02f,.025f,e.Dead?.2f:.42f));
        if(!e.Dead)DrawArc(p,33,0,Mathf.Tau,36,new Color(e.State==3?Teal:Gold,.65f),e.State==3?3:1,true);
        _oathCast!.Draw(this,p,G(e.Facing),e.Dead?3:e.State==3?2:e.State is 1 or 4?1:0,e.Hurt,Offset,Zoom);
    }
    private void DrawOathTelegraph(Fighter e)
    {
        var p=G(e.Position);var f=G(e.Facing);var side=new Vector2(-f.Y,f.X)*35;
        var end=p;
        for(int i=0;i<100;i++){var next=end+f*5;if(!_game.OnWalkable(N(next)))break;end=next;}
        if(p.DistanceTo(end)>8)DrawColoredPolygon(new[]{p-side,p+side,end+side,end-side},new Color(Red,.20f));
        DrawLine(p-side,end-side,Gold,2,true);DrawLine(p+side,end+side,Gold,2,true);
        DrawArc(end,19,0,Mathf.Tau,24,Gold,2,true);
    }
    private void AddOathLayers(List<(float Depth,Action Draw)> layers)
    {
        if(_game.Rooms!.Current==PortRooms.Gallery)
            layers.Add((704,()=>PaintForeground(_galleryArt!,new[]{new Vector2(727,649),new(738,603),new(776,550),new(856,575),new(850,636),new(886,666),new(819,711)})));
        if(_game.Rooms.Current!=PortRooms.Chamber)return;
        layers.Add((638,()=>PaintForeground(RoomBackground,new[]{new Vector2(522,612),new(531,539),new(528,506),new(567,486),new(610,506),new(608,537),new(611,617),new(567,642)})));
        layers.Add((658,()=>PaintForeground(RoomBackground,new[]{new Vector2(954,632),new(963,553),new(959,521),new(1000,501),new(1042,520),new(1040,550),new(1044,635),new(1001,661)})));
    }
}
