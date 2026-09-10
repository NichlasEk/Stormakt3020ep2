using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private ImageTexture? _bailiffArt,_bailiffWalk;
    private Texture2D? _coolwayOpen;
    private bool _foundrySlot;
    private void LoadFoundryArt()
    {
        _bailiffArt??=SpriteCutout.Load("res://assets/art/crown-bailiff-v1.png",chromaKey:new Color(1,0,1));
        _bailiffWalk??=SpriteCutout.Load("res://assets/art/crown-bailiff-walk-v1.png",chromaKey:new Color(1,0,1));
        _coolwayOpen??=GD.Load<Texture2D>("res://assets/art/room-coolway-open-v1.png");
    }
    private void StartFoundry(bool fresh=false)
    {
        LoadWaterArt();_westSlot=false;_gamlaSlot=false;_observatorySlot=false;_meridianSlot=false;_uppsalaSlot=false;_shipTime=0;_pendingStoryFilm="";_radioBreath=0;_foundrySlot=true;_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;_boatTime=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewFoundryPreview(_order);ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        _bannerTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Separat gjuteriprov · lossa porten till höger");
    }
    private void DrawBailiff(Fighter e)
    {
        if(e.Moving&&e.State==0&&!e.Dead)
        {
            var facing=WalkFacing(e);int row=facing.Y<0?1:0,frame=Gait.Frame(e.Walk,96);
            // Belt centers keep the heavy torso steady; no limb stretching.
            var anchors=new Vector2[]{new(240,465),new(222,465),new(209,465),new(225,465),new(223,457),new(229,457),new(220,457),new(235,457)};
            var walkAt=RenderPosition(e);const float walkScale=.40f;
            DrawSetTransform(Offset+walkAt*Zoom,0,new Vector2(facing.X<0?-walkScale:walkScale,walkScale)*Zoom);
            DrawTextureRectRegion(_bailiffWalk!,new Rect2(-anchors[row*4+frame],new Vector2(384,512)),new Rect2(frame*384,row*512,384,512),e.Hurt>0?new Color(1.3f,1.15f,1):Colors.White);
            DrawSetTransform(Offset,0,Vector2.One*Zoom);return;
        }
        int pose=e.Dead||e.State==3?5:e.State==1?3:e.State==2?4:e.Moving?1+(int)(e.Walk/1.8f)%2:0;
        var feet=new Vector2[]{new(256,450),new(256,450),new(256,450),new(256,450),new(256,450),new(256,430)};
        const float scale=.39f;var at=RenderPosition(e);
        DrawSetTransform(Offset+at*Zoom,0,new Vector2(e.Facing.X<0?-scale:scale,scale)*Zoom);
        DrawTextureRectRegion(_bailiffArt!,new Rect2(-feet[pose],new Vector2(512,512)),new Rect2(new Vector2(pose%3,pose/3)*512,new Vector2(512,512)),e.Hurt>0?new Color(1.3f,1.15f,1):Colors.White);
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
    }
    private bool DrawFoundryPrompt()
    {
        if(!_game.InConnectedWorld)return false;string label="";
        bool Near(NVec p)=>NVec.Distance(p,_game.Player)<80&&_game.ClearPath(p,_game.Player);
        if(_game.Rooms!.Current==Mine.Coolway&&Near(Foundry.Gate))label="E / B · "+(_game.FoundryState.GateOpen?"Gjuteriets port är öppen":"Lossa gjuteriets spärr");
        if(_game.InFoundry)
        {
            if(Near(Foundry.Valve)&&!_game.FoundryState.BailiffDefeated)label=_game.FoundryState.CoolingCooldown>0?$"Kylledningen fylls · {MathF.Ceiling(_game.FoundryState.CoolingCooldown)} s":"E / B · Släpp fram kylvattnet";
            else if(Near(Foundry.Plate))label="E / B · Undersök kronans avtryck";
        }
        if(label=="")return false;Panel(new Rect2(330,475,620,53),.93f);Centered(label,640,506,17,Gold);return true;
    }
    private void AddFoundryLayers(List<(float Depth,Action Draw)> layers)
    {
        var room=PaintRoom;var art=RoomBackground;
        void Wall(Vector2[] polygon,float x0,float y0,float slope)
        {for(float x=polygon.Min(p=>p.X);x<polygon.Max(p=>p.X);x+=10){var strip=ClipDoorStrip(ClipDoorStrip(polygon,x,true),x+10,false);if(strip.Length>=3)layers.Add((y0+(x+5-x0)*slope,()=>PaintForeground(art,strip)));}}
        if(room==Mine.Coolway)Wall(new Vector2[]{new(1170,0),new(1536,0),new(1536,625),new(1465,557),new(1460,340),new(1420,290),new(1370,280),new(1320,325),new(1310,510),new(1170,444)},1390,535,.5f);
        if(room!=Foundry.Room)return;
        Wall(new Vector2[]{new(0,0),new(330,0),new(330,350),new(195,410),new(190,215),new(153,190),new(105,220),new(65,290),new(65,425),new(0,470)},135,395,-.5f);
        layers.Add((510,()=>PaintForeground(art,new Vector2[]{new(910,320),new(1030,305),new(1105,362),new(1100,427),new(1010,475),new(908,426)})));
        var at=G(Foundry.Valve);layers.Add((at.Y,()=>
        {var size=_mineValve!.GetSize();float scale=105/size.Y;DrawTextureRect(_mineValve,new Rect2(at-new Vector2(size.X*.5f,size.Y*.95f)*scale,size*scale),false);Text("KYLVATTEN",at+new Vector2(-38,25),11,_game.FoundryState.Cooling>0?Teal:Gold);}));
    }
}
