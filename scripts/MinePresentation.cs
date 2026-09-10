using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private readonly Dictionary<string,Texture2D> _mineArt=new();
    private Texture2D? _mineFarledOpen;
    private Texture2D? _mineValve;
    private bool _mineSlot;
    private void LoadMineArt()
    {
        if(_mineArt.Count>0)return;LoadFoundryArt();
        foreach(var id in Mine.Ids)_mineArt[id]=GD.Load<Texture2D>($"res://assets/art/room-{id}-v1.png");
        _mineFarledOpen=GD.Load<Texture2D>("res://assets/art/room-farled-open-v1.png");
        _mineValve=GD.Load<Texture2D>("res://assets/art/mine-valve-v1.png");
    }
    private void StartMine(bool fresh=false)
    {
        LoadWaterArt();_westSlot=false;_gamlaSlot=false;_observatorySlot=false;_meridianSlot=false;_uppsalaSlot=false;_shipTime=0;_pendingStoryFilm="";_radioBreath=0;_foundrySlot=false;_mineSlot=true;_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;_boatTime=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewMinePreview(_order);ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        _bannerTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Separat gruvexpedition · följ trappan från kajen");
    }
    private bool DrawMinePrompt()
    {
        if(!_game.InConnectedWorld)return false;string label="";var m=_game.MineState;
        bool Near(NVec p)=>NVec.Distance(_game.Player,p)<80&&_game.ClearPath(_game.Player,p);
        if(_game.Rooms!.Current==Regiment.Farled&&Near(Mine.Latch))label="E / B · "+(m.EntranceOpen?"Gruvportens spärr är lossad":"Lossa gruvportens spärr");
        if(_game.Rooms.Current==Mine.Mouth&&Near(Mine.Ledger))label="E / B · Läs driftboken";
        if(_game.Rooms.Current==Mine.Bellows)label=Near(Mine.Feed)?"E / B · "+(m.FeedClosed?"Matningen är stängd":"Stäng matningen"):Near(Mine.Relief)?"E / B · "+(m.PressureReleased?"Trycket är avlastat":"Öppna avlastningen"):"";
        if(_game.Rooms.Current==Mine.Coolway)label=Near(Mine.Imprint)?"E / B · Undersök kronans gjutform":Near(Mine.Shortcut)&&!m.ShortcutOpen?"E / B · Öppna underhållstrappan":Near(Mine.Cache)&&!m.CacheTaken?"E / B · Undersök förrådskistan":"";
        if(label=="")return false;Panel(new Rect2(330,475,620,53),.93f);Centered(label,640,506,17,Gold);return true;
    }
    private void AddMineLayers(List<(float Depth,Action Draw)> layers)
    {
        var room=PaintRoom;var art=RoomBackground;
        void Wall(Vector2[] polygon,float x0,float y0,float slope)
        {
            for(float x=polygon.Min(p=>p.X);x<polygon.Max(p=>p.X);x+=10)
            {var strip=ClipDoorStrip(ClipDoorStrip(polygon,x,true),x+10,false);if(strip.Length>=3)layers.Add((y0+(x+5-x0)*slope,()=>PaintForeground(art,strip)));}
        }
        if(room==Regiment.Farled)
        {
            Wall(new Vector2[]{new(1130,0),new(1430,0),new(1430,300),new(1315,230),new(1310,90),new(1290,60),new(1240,60),new(1210,100),new(1210,220),new(1130,220)},1260,250,.25f);return;
        }
        if(!Mine.Known(room))return;
        Wall(new Vector2[]{new(0,0),new(360,0),new(360,340),new(225,410),new(222,190),new(180,150),new(130,170),new(92,225),new(92,420),new(0,485)},150,400,-.5f);
        if(room!=Mine.Coolway)Wall(new Vector2[]{new(1170,0),new(1536,0),new(1536,540),new(1450,485),new(1450,270),new(1415,220),new(1360,205),new(1300,255),new(1300,405),new(1170,365)},1390,450,.5f);
        if(room==Mine.Mouth)layers.Add((585,()=>PaintForeground(art,new Vector2[]{new(708,430),new(802,440),new(819,533),new(780,563),new(712,540)})));
        if(room==Mine.Bellows)
        {
            layers.Add((545,()=>PaintForeground(art,new Vector2[]{new(510,0),new(1040,0),new(1090,430),new(1020,479),new(764,520),new(514,400)})));
            foreach(var pair in new[]{(Mine.Feed,_game.MineState.FeedClosed,"MATNING"),(Mine.Relief,_game.MineState.PressureReleased,"AVLASTNING")})
            {
                var entry=pair;var p=G(entry.Item1);layers.Add((p.Y,()=>
                {
                    var size=_mineValve!.GetSize();float scale=105/size.Y;DrawTextureRect(_mineValve,new Rect2(p-new Vector2(size.X*.5f,size.Y*.95f)*scale,size*scale),false,entry.Item2?new Color(.65f,.72f,.65f):Colors.White);
                    Text(entry.Item3,p+new Vector2(-40,25),11,entry.Item2?Teal:Gold);
                }));
            }
        }
        if(room==Mine.Coolway)layers.Add((625,()=>PaintForeground(art,new Vector2[]{new(850,480),new(945,460),new(1045,527),new(1030,585),new(973,625),new(864,582)})));
    }
    private void DisposeMineArt(){foreach(var t in _mineArt.Values)t.Dispose();_mineFarledOpen?.Dispose();_mineValve?.Dispose();}
}
