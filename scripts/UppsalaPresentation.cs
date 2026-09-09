using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private Texture2D? _uppsalaArt,_quayFrigate,_flightArt;
    private bool _uppsalaSlot,_shipMidpoint;
    private float _shipTime,_shipDuration;
    private string _shipDestination="";
    private void LoadUppsalaArt()
    {
        LoadMeridianArt();LoadCabinArt();
        _uppsalaArt??=GD.Load<Texture2D>("res://assets/art/room-uppsala-court-v1.png");
        _quayFrigate??=GD.Load<Texture2D>("res://assets/art/room-quay-frigate-v1.png");
        _flightArt??=GD.Load<Texture2D>("res://assets/art/flight-uppsala-v1.png");
    }
    private void StartUppsala(bool fresh=false)
    {
        LoadWaterArt();_meridianSlot=false;_uppsalaSlot=true;_foundrySlot=_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;_boatTime=_shipTime=0;_pendingStoryFilm="";_radioBreath=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewUppsalaPreview(_order);ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_radioTime=0;_sound.StopVoice();
        _bannerTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();ChangeScreen(Screen.Game);Save();HandleCue(new("radio",_game.Player,"uppsala-ready"));
    }
    private void BeginShipTravel(string destination)
    {
        if(_shipTime>0||_boatTime>0||!_game.CanShipTravel(destination))return;
        _shipDestination=destination;_shipMidpoint=false;
        _radioQueue.Clear();_radio="";_radioTime=0;_sound.StopVoice();
        var line=destination==Uppsala.Court?"uppsala-depart":"uppsala-return";
        _shipDuration=_shipTime=(float)Math.Max(16,_sound.ClipDuration("voice-"+line)+_sound.ClipDuration("voice-uppsala-flight")+6);
        HandleCue(new("radio",_game.Player,line));_sound.Play("ship-engine");ClearPresses();
    }
    private void StepShip(float dt)
    {
        if(_shipTime<=0||_screen!=Screen.Game)return;
        _shipTime=Math.Max(0,_shipTime-dt);
        if(!_shipMidpoint&&_shipDuration-_shipTime>_sound.ClipDuration("voice-uppsala-depart")+2)
        {_shipMidpoint=true;if(_shipDestination==Uppsala.Court)HandleCue(new("radio",_game.Player,"uppsala-flight"));}
        if(_shipTime<=0)
        {
            if(_sound.Speaking||_radioQueue.Count>0){_shipTime=.1f;return;}
            if(_game.FinishShipTravel(_shipDestination))foreach(var cue in _game.Events.ToArray())HandleCue(cue);
            _shipDestination="";ClearPresses();
        }
    }
    private void DrawShipTravel()
    {
        if(_shipTime<=0)return;float progress=Math.Clamp(1-_shipTime/_shipDuration,0,1);
        DrawRect(new Rect2(0,0,1280,720),Colors.Black);
        DrawTextureRect(_flightArt!,new Rect2(-25-progress*45,-110+progress*28,1380,920),false);
        DrawRect(new Rect2(0,0,1280,65),new Color(0,0,0,.82f));DrawRect(new Rect2(0,550,1280,170),new Color(0,0,0,.84f));
        Centered("KARL CCLV",640,40,25,Gold,true);
        Centered(_shipDestination==Uppsala.Court?"UPPSALAS FELVÄNDA HIMMEL":"ÅTERFÄRD TILL BRYGGAN",640,690,18,Pale);
        if(_radio!="")DrawRadio();
    }
    private bool DrawUppsalaPrompt()
    {
        if(DrawCabinPrompt())return true;
        if(DrawMeridianPrompt())return true;
        if(!_game.InConnectedWorld||_game.InMeridian||_game.InCabin)return false;string label="";
        bool Near(NVec p)=>NVec.Distance(p,_game.Player)<80&&_game.ClearPath(p,_game.Player);
        if(_game.Rooms!.Current==Regiment.Quay&&_game.FoundryState.PlateTaken&&Near(Uppsala.Board))label=_game.MeridianState.OrderTaken?"E / B · Gå ombord · möt Ebba i kajutan":"E / B · Ombord på Karl CCLV · Uppsala";
        if(_game.InUppsala)
        {
            if(Near(Uppsala.Ramp))label=_game.MeridianState.OrderTaken?"E / B · Gå ombord · möt Ebba i kajutan":"E / B · Karl CCLV · tillbaka till bryggan";
            else if(Near(Uppsala.Desk))label="E / B · Läs astronomens anvisning";
            else if(Near(Uppsala.Seal))label="E / B · Undersök portens datum";
            else for(int i=0;i<3;i++)if(Near(Uppsala.Rings[i]))label=_game.UppsalaState.Aligned?"Instrumentet står rätt":"E / B · Vrid "+Uppsala.Names[i].ToLowerInvariant()+" · "+Uppsala.Directions[_game.UppsalaState.Rings[i]];
        }
        if(label=="")return false;Panel(new Rect2(285,475,710,53),.93f);Centered(label,640,506,17,Gold);return true;
    }
    private void AddUppsalaLayers(List<(float Depth,Action Draw)> layers)
    {
        if(PaintRoom!=Uppsala.Court)return;
        for(int i=0;i<3;i++)
        {
            int ring=i;var at=G(Uppsala.Centers[i]);
            layers.Add((at.Y+38,()=>
            {
                PaintForeground(_game.MeridianState.CourtOpen?_uppsalaOpen!:_uppsalaArt!,new Vector2[]{at+new Vector2(-56,-47),at+new Vector2(56,-47),at+new Vector2(56,35),at+new Vector2(-56,35)});
                if(!_game.CanSeeRoomPoint(Uppsala.Rings[ring]+Uppsala.Origin-_game.WorldOrigin))return;
                int direction=_game.UppsalaState.Rings[ring];var v=new Vector2[]{new(0,-18),new(30,0),new(0,18),new(-30,0)}[direction];
                DrawLine(at+new Vector2(0,-15),at+new Vector2(0,-15)+v,_game.UppsalaState.Aligned?Teal:Gold,3,true);
                Text(Uppsala.Names[ring]+" · "+Uppsala.Directions[direction],at+new Vector2(-62,65),10,Gold);
            }));
        }
        layers.Add((430,()=>PaintForeground(_game.MeridianState.CourtOpen?_uppsalaOpen!:_uppsalaArt!,new Vector2[]{new(630,302),new(780,302),new(780,429),new(630,429)})));
    }
}
