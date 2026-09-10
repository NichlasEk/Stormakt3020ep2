using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private Texture2D? _officerArt,_elinArt,_registryWestOpen;
    private bool _westSlot;
    private readonly Vector2[] _officerFeet={new(395,495),new(1155-768,500),new(390,995-512),new(1130-768,865-512)};
    private void LoadWestArt()
    {
        if(_elinArt!=null)return;
        _registryWestOpen=GD.Load<Texture2D>("res://assets/art/room-gamla-registry-open-v1.png");
        _elinArt=SpriteCutout.Load("res://assets/art/elin-vinge-v1.png",chromaKey:new Color(0,1,0));
        _officerArt=SpriteCutout.Load("res://assets/art/muster-officer-v1.png",chromaKey:new Color(0,1,0));
    }
    private void StartWest(bool fresh=false)
    {
        LoadWaterArt();_rescueSlot=false;_saltSlot=false;_westSlot=true;_gamlaSlot=_observatorySlot=_meridianSlot=_uppsalaSlot=_foundrySlot=_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;
        _boatTime=_shipTime=0;_pendingStoryFilm="";_radioBreath=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewWestPreview(_order);ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(0,-60);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();_bannerTime=_campaignTextTime=_revealTime=0;RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Västra vågen · separat prov · öppna väntrummets högra port");
    }
    private void DrawMusterOfficer(Fighter e)
    {
        int pose=e.Dead?3:e.State==1?1:_game.WestState.Exposed>0?2:0;float scale=.38f;var at=RenderPosition(e);
        DrawSetTransform(Offset+at*Zoom,0,Vector2.One*scale*Zoom);
        DrawTextureRectRegion(_officerArt!,new Rect2(-_officerFeet[pose],new(768,512)),new Rect2(pose%2*768,pose/2*512,768,512),e.Hurt>0?new Color(1.25f,1.1f,1):Colors.White);
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(e.State==1&&!e.Dead)
        {var aim=G(e.LockedAim);DrawArc(aim,78,0,Mathf.Tau,48,new Color(.86f,.39f,.17f,.85f),3,true);DrawLine(at,aim,new Color(.7f,.35f,.15f,.5f),1,true);DrawArc(aim,78,0,Mathf.Tau*Math.Clamp(1-e.Timer/1.5f,0,1),48,Gold,4,true);}
    }
    private void DrawElin(Vector2 at,float height=170)
    {
        float scale=height/1380;DrawTextureRect(_elinArt!,new Rect2(at-new Vector2(520,1450)*scale,_elinArt!.GetSize()*scale),false);
    }
    private void AddWestLayers(List<(float Depth,Action Draw)> layers)
    {
        var room=PaintRoom;
        if(room==Cabin.Room&&_game.WestState.ElinMet){var at=West.Aboard;layers.Add((at.Y,()=>DrawElin(G(at))));return;}
        if(!West.Known(room))return;var art=RoomBackground;
        void Wall(Vector2[] polygon,float depth)=>layers.Add((depth,()=>PaintForeground(art,polygon)));
        // Measured jambs hide the receding body while its feet follow the painted threshold.
        if(room==West.Control)
        {
            Wall(new[]{new Vector2(0,0),new(360,0),new(360,200),new(270,160),new(200,135),new(120,170),new(100,225),new(0,260)},420);
            Wall(new[]{new Vector2(250,185),new(350,215),new(350,425),new(250,395)},440);
            Wall(new[]{new Vector2(1470,200),new(1536,200),new(1536,535),new(1470,495)},530);
        }
        if(room==West.Hall)
        {
            Wall(new[]{new Vector2(125,300),new(200,320),new(200,480),new(150,490)},495);
            Wall(new[]{new Vector2(1460,300),new(1536,300),new(1536,610),new(1460,550)},555);
            foreach(var p in West.Brakes){var b=p;layers.Add((b.Y-22,()=>{var c=(_game.WestState.Brakes&(1<<Array.IndexOf(West.Brakes,b)))!=0?Teal:Gold;DrawArc(G(b)+new Vector2(0,-45),20,0,Mathf.Tau,30,new Color(c,.65f),2,true);}));}
        }
        if(room==West.Refuge)
        {
            Wall(new[]{new Vector2(200,265),new(295,255),new(295,470),new(200,490)},495);
            Wall(new[]{new Vector2(775,455),new(875,420),new(975,478),new(937,575),new(832,566),new(776,530)},560);
            if(!_game.WestState.Debriefed)layers.Add((West.Elin.Y,()=>{if(_game.CanSeeRoomPoint(West.Talk+ConnectedWorld.Origin(room)-_game.WorldOrigin))DrawElin(G(West.Elin));}));
        }
    }
    private bool DrawWestPrompt()
    {
        if(!_game.InWest)return false;string text="";var s=_game.WestState;bool Near(NVec p)=>NVec.Distance(_game.Player,p)<72&&_game.ClearPath(_game.Player,p);
        if(_game.Rooms!.Current==West.Control&&Near(West.Register))text="Läs vågens instruktion";
        if(_game.Rooms.Current==West.Hall&&!s.Defeated)for(int i=0;i<2;i++)if(Near(West.Brakes[i]))text=(s.Brakes&(1<<i))!=0?"Bromsen är lossad":"Lossa bromsen";
        if(_game.Rooms.Current==West.Refuge){if(Near(West.Record))text="Läs Elins handling";else if(Near(West.Talk))text="Tala med Elin Vinge";}
        if(text=="")return false;Panel(new Rect2(260,475,760,53),.93f);Centered("E / B · "+text,640,507,17,Gold);return true;
    }
}
