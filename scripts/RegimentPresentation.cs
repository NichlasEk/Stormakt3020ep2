using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private readonly Dictionary<string,Texture2D> _regimentArt=new();
    private ImageTexture? _marshalArt,_soldierArt,_captainArt;
    private Texture2D? _standardArt;
    private bool _regimentSlot;
    private float _boatTime;
    private string _boatDestination="";
    private void LoadRegimentArt()
    {
        if(_regimentArt.Count>0)return;
        foreach(var id in Regiment.Ids)_regimentArt[id]=GD.Load<Texture2D>($"res://assets/art/room-{id}-v1.png");
        _marshalArt=SpriteCutout.Load("res://assets/art/root-marshal-v1.png",chromaKey:new Color(1,0,1));
        _soldierArt=SpriteCutout.Load("res://assets/art/root-soldier-v1.png",chromaKey:new Color(1,0,1));
        _captainArt=SpriteCutout.Load("res://assets/art/captain-arvid-v1.png",chromaKey:new Color(1,0,1));
        _standardArt=GD.Load<Texture2D>("res://assets/art/regiment-standard-v1.png");
    }
    private void StartRegiment(bool fresh=false)
    {
        LoadWaterArt();_meridianSlot=false;_uppsalaSlot=false;_shipTime=0;_pendingStoryFilm="";_radioBreath=0;_foundrySlot=false;_mineSlot=false;_regimentSlot=true;_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewRegimentPreview(_order);ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        _camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Separat provexpedition · följ stigen från lunden");
    }
    private void DrawRegimentActor(Fighter e)
    {
        if(RegimentVisibility<=0)return;
        bool boss=e.Kind==EnemyKind.RootMarshal;var art=boss?_marshalArt!:_soldierArt!;
        int pose=e.Retired?0:e.Dead?5:e.State==1?3:e.State==2?4:boss&&e.State==3?5:e.Moving?1+(int)(e.Walk/1.8f)%2:0;
        var feet=boss?new Vector2[]{new(260,447),new(270,438),new(270,438),new(245,440),new(205,425),new(235,370)}:new Vector2[]{new(255,443),new(265,438),new(265,438),new(245,442),new(205,425),new(260,363)};
        var at=RenderPosition(e);float scale=(boss?165f:146f)/420;
        DrawSetTransform(Offset+at*Zoom,0,new Vector2((e.Moving?WalkFacing(e).X:e.Facing.X)<0?-scale:scale,scale)*Zoom);
        var tint=e.Retired?new Color(.65f,.68f,.65f,.8f):e.Hurt>0?new Color(1.3f,1.15f,1):Colors.White;tint.A*=RegimentVisibility;
        DrawTextureRectRegion(art,new Rect2(-feet[pose],new Vector2(512,512)),new Rect2(new Vector2(pose%3,pose/3)*512,new Vector2(512,512)),tint);
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(!e.Dead&&e.Health<e.MaxHealth)WorldBar(at+new Vector2(-24,-170),48,e.Health/e.MaxHealth,boss?Gold:Red);
    }
    private void AddRegimentLayers(List<(float Depth,Action Draw)> layers)
    {
        if(!Regiment.Known(PaintRoom))return;var art=RoomBackground;string room=PaintRoom;
        void Foreground(float depth,Vector2[] polygon)=>layers.Add((depth,()=>PaintForeground(art,polygon)));
        void Wall(Vector2[] polygon,float x0,float y0,float slope)
        {
            for(float x=polygon.Min(p=>p.X);x<polygon.Max(p=>p.X);x+=10)
            {var strip=ClipDoorStrip(ClipDoorStrip(polygon,x,true),x+10,false);if(strip.Length>=3)layers.Add((y0+(x+5-x0)*slope,()=>PaintForeground(art,strip)));}
        }
        if(room==Regiment.Trail)Foreground(600,new Vector2[]{new(510,495),new(640,347),new(770,340),new(905,355),new(1005,465),new(950,575),new(750,620)});
        if(room==Regiment.Flags)Foreground(585,new Vector2[]{new(398,435),new(550,290),new(770,210),new(925,260),new(1090,490),new(973,550),new(650,600)});
        if(room==Regiment.Barracks)
        {
            Foreground(577,new Vector2[]{new(620,450),new(742,390),new(875,440),new(874,532),new(752,577),new(620,540)});
            Wall(new Vector2[]{new(0,0),new(320,0),new(320,315),new(195,387),new(193,216),new(90,252),new(90,431),new(0,495)},145,425,-.5f);
            Wall(new Vector2[]{new(1270,0),new(1536,0),new(1536,508),new(1480,465),new(1480,262),new(1416,236),new(1415,419),new(1270,375)},1450,452,.5f);
            if(!_game.RegimentState.Discharged&&_game.CanSeeRoomPoint(Regiment.Captain+Regiment.Origin(room)-_game.WorldOrigin))layers.Add((Regiment.Captain.Y,()=>
            {var size=_captainArt!.GetSize();float scale=153/(size.Y*.92f);DrawTextureRect(_captainArt,new Rect2(G(Regiment.Captain)-new Vector2(size.X*.5f,size.Y*.96f)*scale,size*scale),false);}));
        }
        if(room is Regiment.Parade or Regiment.Quay)
            Wall(new Vector2[]{new(0,0),new(335,0),new(335,340),new(193,403),new(189,304),new(164,279),new(135,283),new(104,325),new(104,411),new(0,467)},150,417,-.48f);
        if(room==Regiment.Parade)
        {
            Wall(new Vector2[]{new(1250,0),new(1536,0),new(1536,517),new(1468,465),new(1465,320),new(1440,284),new(1418,286),new(1394,315),new(1394,412),new(1250,365)},1425,445,.48f);
            for(int i=0;i<3;i++)
            {
                int index=i;var at=G(Regiment.Standards[i]);bool broken=_game.RegimentState.Standards[i]<=0||_game.RegimentState.MarshalDefeated;
                var shift=Regiment.Origin(room)-_game.WorldOrigin;if(!_game.CanSeeRoomPoint(Regiment.Standards[i]+shift))continue;
                layers.Add((at.Y,()=>
                {
                    const float scale=.18f;var anchor=broken?new Vector2(380,850):new Vector2(285,950);
                    DrawTextureRectRegion(_standardArt!,new Rect2(at-anchor*scale,new Vector2(768,1024)*scale),new Rect2(broken?768:0,0,768,1024));
                    if(!broken){DrawArc(at,24,0,Mathf.Tau,30,new Color(Gold,.75f),1.5f,true);WorldBar(at+new Vector2(-22,13),44,_game.RegimentState.Standards[index]/65,Gold);}
                }));
            }
        }
    }
    private bool DrawRegimentPrompt()
    {
        if(!_game.InRegiment)return false;string label="";var r=_game.RegimentState;
        bool Near(NVec at)=>NVec.Distance(_game.Player,at)<80&&_game.ClearPath(_game.Player,at);
        switch(_game.Rooms!.Current)
        {
            case Regiment.Barracks:label=Near(Regiment.Captain)?"E / B · Tala med kapten Silfvergren":Near(Regiment.Orders)?"E / B · Läs avlösningsordern":Near(Regiment.Proof)?"E / B · Undersök sjukrullan":"";break;
            case Regiment.Flags:label=Near(Regiment.Checkpoint)?"E / B · Visa din handling":Near(Regiment.Shortcut)?"E / B · Öppna återtågsvägen":"";break;
            case Regiment.Parade:if(r.MarshalDefeated&&Near(Regiment.Discharge)&&!r.FarewellSeen)label=r.Discharged?"E / B · Minns avlösningen":"E / B · Läs avlösningen";break;
            case Regiment.Quay:label=Near(Regiment.Boat)?"E / B · Stig ombord · farleden mot berget":Near(new(640,465))&&!r.QuayCacheTaken?"E / B · Kaptenens kvarlåtenskap":"";break;
            case Regiment.Farled:if(Near(Regiment.LandingBoat))label="E / B · Båt tillbaka till regementets brygga";break;
        }
        if(label=="")return false;Panel(new Rect2(330,475,620,53),.93f);Centered(label,640,506,17,Gold);return true;
    }
    private void StepBoat(float dt)
    {
        if(_boatTime<=0||_screen!=Screen.Game)return;_boatTime=Math.Max(0,_boatTime-dt);
        if(_boatTime==0){if(_game.FinishBoatCrossing(_boatDestination))foreach(var cue in _game.Events.ToArray())HandleCue(cue);_boatDestination="";ClearPresses();}
    }
    private void DrawBoatCrossing()
    {
        if(_boatTime<=0)return;float progress=1-_boatTime/6;
        DrawRect(new Rect2(0,0,1280,720),new Color("070e10"));
        DrawTextureRect(_regimentArt[_boatDestination],new Rect2(-30-progress*35,-110+progress*20,1400,933),false,new Color(.62f,.67f,.69f));
        DrawRect(new Rect2(0,0,1280,130),new Color(0,0,0,.8f));DrawRect(new Rect2(0,555,1280,165),new Color(0,0,0,.84f));
        Centered("ÖVERFART",640,606,30,Pale,true);Centered(_boatDestination==Regiment.Farled?"Längs den glömda farleden mot berget":"Tillbaka till regementets brygga",640,647,18,Gold);
        DrawRect(new Rect2(430,680,420*progress,2),Gold);
    }
    private void DisposeRegimentArt(){foreach(var t in _regimentArt.Values)t.Dispose();_marshalArt?.Dispose();_soldierArt?.Dispose();_captainArt?.Dispose();_standardArt?.Dispose();}
}
