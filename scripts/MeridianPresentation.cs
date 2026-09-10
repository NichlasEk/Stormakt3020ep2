using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private Texture2D? _clockArt,_meridianArt,_uppsalaOpen,_meridianWarden,_meridianPortrait;
    private readonly Vector2[] _meridianFeet=new Vector2[4];
    private float _meridianScale;
    private void LoadMeridianArt()
    {
        if(_clockArt!=null)return;
        _clockArt=GD.Load<Texture2D>("res://assets/art/room-clockwalk-v1.png");_meridianArt=GD.Load<Texture2D>("res://assets/art/room-meridian-hall-v1.png");
        _uppsalaOpen=GD.Load<Texture2D>("res://assets/art/room-uppsala-open-v1.png");_meridianWarden=GD.Load<Texture2D>("res://assets/art/meridian-warden-v1.png");_meridianPortrait=GD.Load<Texture2D>("res://assets/art/meridian-radio-v1.png");
        using var image=_meridianWarden.GetImage();if(image.IsCompressed())image.Decompress();
        int cell=image.GetWidth()/2;
        for(int pose=0;pose<4;pose++)
        {
            int top=cell,bottom=0;for(int y=0;y<cell;y++)for(int x=0;x<cell;x++)if(image.GetPixel(pose%2*cell+x,pose/2*cell+y).A>.5f){top=Math.Min(top,y);bottom=Math.Max(bottom,y);}
            long sum=0,count=0;for(int y=Math.Max(top,bottom-15);y<=bottom;y++)for(int x=0;x<cell;x++)if(image.GetPixel(pose%2*cell+x,pose/2*cell+y).A>.5f){sum+=x;count++;}
            _meridianFeet[pose]=new Vector2(count>0?(float)sum/count:cell*.5f,bottom-2);if(pose==0)_meridianScale=170f/Math.Max(1,bottom-top);
        }
    }
    private bool _meridianSlot;
    private void StartMeridian(bool fresh=false)
    {
        LoadWaterArt();_saltSlot=false;_westSlot=false;_gamlaSlot=false;_observatorySlot=false;_meridianSlot=true;_uppsalaSlot=false;_foundrySlot=_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;
        _boatTime=_shipTime=0;_pendingStoryFilm="";_radioBreath=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewMeridianPreview(_order);ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(0,-60);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_radioTime=0;_sound.StopVoice();
        _bannerTime=_campaignTextTime=_revealTime=0;RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Separat meridianprov · öppna gårdens port med datumavtrycket");
    }
    private void DrawMeridianWarden(Fighter e)
    {
        int pose=e.Dead?3:e.State==1?1:e.Facing.Y<0?2:0;float cell=_meridianWarden!.GetWidth()/2f;
        var at=RenderPosition(e);DrawSetTransform(Offset+at*Zoom,0,new Vector2(e.Facing.X<0?-_meridianScale:_meridianScale,_meridianScale)*Zoom);
        DrawTextureRectRegion(_meridianWarden,new Rect2(-_meridianFeet[pose],new Vector2(cell,cell)),new Rect2(pose%2*cell,pose/2*cell,cell,cell),e.Hurt>0?new Color(1.2f,1.1f,1):Colors.White);
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
    }
    private bool DrawMeridianPrompt()
    {
        if(!_game.InUppsala||_game.InObservatory)return false;var m=_game.MeridianState;string label="";
        bool Near(NVec p)=>NVec.Distance(_game.Player,p)<80&&_game.ClearPath(_game.Player,p);
        if(_game.Rooms!.Current==Uppsala.Court)
        {if(_game.UppsalaState.KeyTaken&&!m.CourtOpen&&Near(Meridian.CourtGate))label="E / B · Öppna porten med datumavtrycket";}
        else if(_game.Rooms.Current==Meridian.Clock)
        {
            if(Near(Meridian.Ledger))label="E / B · Läs slagverkets liggare";
            else if(Near(Meridian.Bell))label=m.ClockAnchored?"Slagverket går vidare":!m.ClockStarted?"E / B · Starta slagverket":m.Cycles<2?"Följ vakten genom nästa klockslag":"E / B · Håll plåten mot spärren";
        }
        else
        {
            if(Near(Meridian.Plate))label="E / B · Jämför stjärnplåtens märken";
            else if(Near(Meridian.Order))label=m.WardenDefeated?"E / B · Läs förflyttningsordern":"Övre observatoriet · förseglat";
            else for(int i=0;i<3;i++)if(Near(Meridian.Controls[i]))label="E / B · "+Uppsala.Names[i]+" · "+Uppsala.Directions[m.Angles[i]];
        }
        if(label=="")return false;Panel(new Rect2(260,475,760,53),.93f);Centered(label,640,507,17,Gold);return true;
    }
    private void AddMeridianLayers(List<(float Depth,Action Draw)> layers)
    {
        var room=PaintRoom;var m=_game.MeridianState;var art=RoomBackground;
        void Wall(Vector2[] polygon,float x0,float y0,float slope)
        {for(float x=polygon.Min(p=>p.X);x<polygon.Max(p=>p.X);x+=10){var strip=ClipDoorStrip(ClipDoorStrip(polygon,x,true),x+10,false);if(strip.Length>=3)layers.Add((y0+(x+5-x0)*slope,()=>PaintForeground(art,strip)));}}
        if(room==Uppsala.Court&&m.CourtOpen)
        {
            Wall(new[]{new Vector2(900,0),new(990,0),new(990,190),new(970,245),new(970,402),new(900,381)},970,400,.5f);
            Wall(new[]{new Vector2(1085,0),new(1175,0),new(1175,460),new(1085,410)},1085,410,.5f);
            layers.Add((403,()=>PaintForeground(art,new Vector2[]{new(970,0),new(1095,0),new(1095,248),new(1080,200),new(1050,176),new(1008,195),new(982,230),new(970,250)})));
        }
        if(room==Meridian.Clock)
        {
            Wall(new[]{new Vector2(0,0),new(300,0),new(300,440),new(195,505),new(190,283),new(152,235),new(100,253),new(50,310),new(50,550),new(0,575)},150,525,-.5f);
            Wall(new[]{new Vector2(1180,0),new(1536,0),new(1536,570),new(1450,517),new(1450,241),new(1390,190),new(1335,205),new(1275,274),new(1275,445),new(1180,420)},1370,465,.5f);
            if(m.ClockStarted&&(!m.ClockAnchored||m.Departure<8))
            {
                var p=G(_game.ClockEchoPosition);layers.Add((p.Y,()=>
                {
                    var delta=G(ConnectedWorld.Origin(Meridian.Clock)-_game.WorldOrigin);var wp=_game.ClockEchoPosition+ConnectedWorld.Origin(Meridian.Clock)-_game.WorldOrigin;
                    if(!_game.CanSeeRoomPoint(wp))return;
                    float time=m.ClockAnchored?m.Departure*1.5f:m.ClockTime;
                    _animated.Draw(this,"guard",p,new Vector2(m.ClockAnchored||m.ClockTime<=6?1:-1,0),"walk",Gait.Frame(time*88*7/195),0,false,Offset+delta*Zoom,Zoom);
                    DrawSetTransform(Offset+delta*Zoom,0,Vector2.One*Zoom);
                }));
            }
            var clock=new Vector2(510,195);layers.Add((430,()=>
            {
                float angle=(m.ClockAnchored?(_clock*.055f):m.ClockTime*.018f)-Mathf.Pi/2;
                DrawLine(clock,clock+new Vector2(Mathf.Cos(angle)*55,Mathf.Sin(angle)*90),new Color("a48b54"),3,true);
                Text(m.ClockAnchored?"SLAGVERKET GÅR":"VI · XVII",new Vector2(770,640),12,Gold);
            }));
        }
        if(room!=Meridian.Hall)return;
        if(m.WardenDefeated)layers.Add((0,()=>{float drift=m.Dawn*3;DrawColoredPolygon(new[]{new Vector2(190+drift,468),new(275+drift,426),new(1000+drift,740),new(865+drift,802)},new Color(.95f,.77f,.43f,.065f));}));
        Wall(new[]{new Vector2(0,0),new(290,0),new(290,402),new(212,463),new(204,248),new(172,193),new(120,180),new(75,241),new(65,510),new(0,565)},120,490,-.5f);
        for(int i=0;i<3;i++)
        {
            int ring=i;var at=G(Meridian.Centers[i]);layers.Add((at.Y+30,()=>
            {
                var outline=ring==0?new Vector2[]{new(-40,-72),new(-20,-91),new(20,-91),new(41,-65),new(29,-33),new(15,-16),new(38,4),new(30,27),new(-28,27),new(-37,8),new(-17,-17),new(-30,-36)}:ring==1?new Vector2[]{new(-40,-62),new(-35,-76),new(-20,-76),new(-15,-58),new(17,-66),new(25,-57),new(19,-40),new(4,-31),new(7,-10),new(35,2),new(40,19),new(24,29),new(-25,29),new(-40,13),new(-29,-5),new(-19,-12),new(-19,-47)}:new Vector2[]{new(-48,-66),new(30,-83),new(38,-74),new(9,-55),new(9,-23),new(40,-2),new(47,18),new(25,31),new(-29,30),new(-43,14),new(-33,-7),new(-10,-25),new(-13,-47),new(-48,-51)};
                PaintForeground(_meridianArt!,outline.Select(p=>p+at).ToArray());
                if(!_game.CanSeeRoomPoint(Meridian.Controls[ring]+ConnectedWorld.Origin(Meridian.Hall)-_game.WorldOrigin))return;
                int d=m.Angles[ring];var axis=new Vector2[]{new(0,-20),new(32,0),new(0,20),new(-32,0)}[d];bool active=m.PlateSet&&!m.WardenDefeated&&m.Exposed<=0&&m.Breaks%3==ring;
                DrawLine(at+new Vector2(0,-8),at+new Vector2(0,-8)+axis,active?Gold:Muted,3,true);
                Text(Uppsala.Names[ring]+" · "+Uppsala.Directions[d],at+new Vector2(-55,52),11,active?Gold:Muted);
                if(active)DrawArc(at+new Vector2(0,8),42,0,Mathf.Tau,48,new Color(Gold,.6f),1.5f,true);
            }));
        }
    }
    private void DisposeMeridianArt(){foreach(var t in new[]{_clockArt,_meridianArt,_uppsalaOpen,_meridianWarden,_meridianPortrait})t?.Dispose();}
}
