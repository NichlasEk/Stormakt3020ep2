using Godot;
using Atland;
using System;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private Texture2D? _cabinArt,_ebbaCabin;
    private void LoadCabinArt()
    {
        _cabinArt??=GD.Load<Texture2D>("res://assets/art/room-cabin-space-v2.png");
        _ebbaCabin??=SpriteCutout.Load("res://assets/art/ebba-cabin-v1.png",chromaKey:new Color(0,1,0));
    }
    private void AddCabinLayers(List<(float Depth,Action Draw)> layers)
    {
        if(PaintRoom!=Cabin.Room)return;
        layers.Add((Cabin.Ebba.Y,()=>
        {
            var at=G(Cabin.Ebba);
            DrawColoredPolygon(new[]{at+new Vector2(-24,-3),at+new Vector2(0,-10),at+new Vector2(27,0),at+new Vector2(0,10)},new Color(0,0,0,.30f));
            // Full image has 20px padding below the lowest sole; fixed feet never bob.
            float scale=172f/1500;var size=_ebbaCabin!.GetSize()*scale;
            DrawTextureRect(_ebbaCabin,new Rect2(at-new Vector2(520,1515)*scale,size),false,new Color(.83f,.80f,.75f));
        }));
        layers.Add((Cabin.FromPainting(new(0,455)).Y,()=>
        {
            var delta=G(ConnectedWorld.Origin(Cabin.Room)-_game.WorldOrigin);
            DrawSetTransform(Offset+(delta+G(Cabin.FromPainting(NVec.Zero)))*Zoom,0,Vector2.One*Zoom*Cabin.EnvironmentScale);
            PaintForeground(_cabinArt!,new Vector2[]{new(435,229),new(611,167),new(803,244),new(807,275),new(668,340),new(650,450),new(626,454),new(623,347),new(550,325),new(540,402),new(519,398),new(516,309),new(447,279)});
            DrawSetTransform(Offset+delta*Zoom,0,Vector2.One*Zoom);
        }));
    }
    private bool DrawCabinPrompt()
    {
        if(!_game.InCabin)return false;
        bool Near(NVec p)=>NVec.Distance(_game.Player,p)<72&&_game.ClearPath(_game.Player,p);
        var c=_game.CabinState;
        string label=Near(Cabin.Entry)?"E / B · Gå ut till landgången":Near(Cabin.Helm)?"E / B · Avsegla · "+(c.ReturnRoom==Uppsala.Court?"bryggan":"Uppsala"):Near(Cabin.Rest)?c.Rested?"E / B · Förbanden":"E / B · Lägg om såren":Near(Cabin.Talk)?"E / B · "+(c.Conversation==0?"Visa Ebba ordern":c.Conversation==1?"Kartan, gjutformen och stjärnplåten":c.Conversation==2?"Tala om nästa steg":"Tala med Ebba"):"";
        if(label!=""){Panel(new Rect2(285,430,710,47),.93f);Centered(label,640,459,16,Gold);}return true;
    }
}
