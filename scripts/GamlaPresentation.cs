using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private readonly Dictionary<string,Texture2D> _gamlaArt=new();
    private Texture2D? _nilsArt;
    private bool _gamlaSlot;
    private void LoadGamlaArt()
    {
        LoadWestArt();if(_nilsArt!=null)return;foreach(var id in Gamla.Ids)_gamlaArt[id]=GD.Load<Texture2D>("res://assets/art/room-"+id+"-v1.png");
        _nilsArt=SpriteCutout.Load("res://assets/art/nils-berg-v1.png",chromaKey:new Color(0,1,0));
    }
    private void StartGamla(bool fresh=false)
    {
        LoadWaterArt();_westSlot=false;_gamlaSlot=true;_observatorySlot=_meridianSlot=_uppsalaSlot=_foundrySlot=_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;
        _boatTime=_shipTime=0;_pendingStoryFilm="";_radioBreath=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewGamlaPreview(_order);ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(0,-60);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_radioTime=0;_sound.StopVoice();_bannerTime=_campaignTextTime=_revealTime=0;
        RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Separat Gamla Uppsala-prov · E vid rodret för avfärd");
    }
    private void StartGamlaTravel(string destination)
    {
        if(!_game.InCabin||!_game.ObservatoryState.Debriefed||_shipTime>0)return;
        _shipDestination=destination;_shipMidpoint=true;_radioQueue.Clear();_radio="";_radioTime=0;_sound.StopVoice();
        _shipDuration=_shipTime=(float)Math.Max(12,_sound.ClipDuration("voice-gamla-depart")+4);
        HandleCue(new("radio",_game.Player,destination==Gamla.Landing?"gamla-depart":"gamla-return"));_sound.Play("ship-engine");ClearPresses();
    }
    private void AddGamlaLayers(List<(float Depth,Action Draw)> layers)
    {
        var room=PaintRoom;if(!Gamla.Known(room))return;var art=RoomBackground;
        void Wall(Vector2[] polygon,float depth)=>layers.Add((depth,()=>PaintForeground(art,polygon)));
        if(room==Gamla.Landing)
        {
            Wall(new[]{new Vector2(1030,180),new(1280,180),new(1280,320),new(1230,320),new(1200,303),new(1160,309),new(1125,350),new(1110,390),new(1030,390)},465);
            Wall(new[]{new Vector2(1230,310),new(1305,330),new(1310,520),new(1230,485)},488);
            Wall(new[]{new Vector2(1040,355),new(1125,355),new(1125,467),new(1080,495),new(1040,470)},465);
        }
        if(room is Gamla.Passage or Gamla.Registry)
        {
            Wall(new[]{new Vector2(0,0),new(255,0),new(255,235),new(220,250),new(90,285),new(75,275),new(0,285)},460);
            Wall(new[]{new Vector2(0,270),new(75,270),new(75,480),new(0,560)},500);
            Wall(new[]{new Vector2(210,230),new(290,220),new(300,450),new(210,485)},480);
        }
        if(room==Gamla.Passage)
        {
            Wall(new[]{new Vector2(1180,0),new(1490,0),new(1490,150),new(1410,115),new(1360,88),new(1280,88),new(1220,145),new(1180,200)},405);
            Wall(new[]{new Vector2(1390,120),new(1500,160),new(1510,460),new(1390,402)},420);
            Wall(new[]{new Vector2(1110,170),new(1220,145),new(1220,355),new(1180,405),new(1110,365)},390);
            Wall(new[]{new Vector2(635,402),new(722,364),new(958,400),new(960,508),new(910,546),new(630,483)},525);
        }
        if(room==Gamla.Registry)
        {
            Wall(new[]{new Vector2(550,510),new(590,490),new(760,568),new(755,633),new(731,645),new(548,555)},622);
            layers.Add((Gamla.Nils.Y,()=>
            {
                if(!_game.CanSeeRoomPoint(Gamla.Talk+ConnectedWorld.Origin(room)-_game.WorldOrigin))return;
                var at=G(Gamla.Nils);float scale=172f/1370;DrawTextureRect(_nilsArt!,new Rect2(at-new Vector2(450,1436)*scale,_nilsArt!.GetSize()*scale),false);
            }));
        }
    }
    private bool DrawGamlaPrompt()
    {
        if(DrawWestPrompt())return true;if(!_game.InGamla||_game.InWest)return false;string text="";var s=_game.GamlaState;
        bool Near(NVec p)=>NVec.Distance(_game.Player,p)<72&&_game.ClearPath(_game.Player,p);
        if(_game.Rooms!.Current==Gamla.Landing){if(Near(Gamla.Board))text="Gå ombord · Ebba väntar";else if(Near(Gamla.Camp))text="Undersök uppställningen";}
        if(_game.Rooms.Current==Gamla.Passage&&Near(Gamla.Ledger))text="Läs transportliggaren";
        if(_game.Rooms.Current==Gamla.Registry&&Near(Gamla.Talk))text=s.Conversation==0?"Tala med Nils Berg":s.Conversation==1?"Fråga om Elin":"Nils vittnesmål";
        if(text!=""){Panel(new Rect2(260,475,760,53),.93f);Centered("E / B · "+text,640,507,17,Gold);}return text!="";
    }
    private void DisposeGamlaArt(){foreach(var image in _gamlaArt.Values)image.Dispose();_nilsArt?.Dispose();_elinArt?.Dispose();_officerArt?.Dispose();_registryWestOpen?.Dispose();}
}
