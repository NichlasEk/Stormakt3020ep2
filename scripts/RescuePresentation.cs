using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private Texture2D? _ebbaFront,_ebbaBack,_ebbaWalk,_censorArt,_rescueSpring,_karlRescue;
    private bool _rescueSlot;
    private void LoadRescueArt()
    {
        _karlRescue??=SpriteCutout.Load("res://assets/art/serious-cast-v6.png",chromaKey:new Color(1,0,1));
        _ebbaFront??=SpriteCutout.Load("res://assets/art/ebba-play-front-v1.png",chromaKey:new Color(0,1,0));
        _ebbaWalk??=SpriteCutout.Load("res://assets/art/ebba-play-walk-v1.png",chromaKey:new Color(0,1,0));
        _ebbaBack??=SpriteCutout.Load("res://assets/art/ebba-play-back-v1.png",chromaKey:new Color(0,1,0));
        _censorArt??=SpriteCutout.Load("res://assets/art/rescue-censor-v1.png",chromaKey:new Color(0,1,0));
        _rescueSpring??=GD.Load<Texture2D>("res://assets/art/room-salt-rescue-open-v1.png");
    }
    private void StartRescue(bool fresh=false)
    {
        LoadWaterArt();_rescueSlot=true;_saltSlot=_westSlot=_gamlaSlot=_observatorySlot=_meridianSlot=_uppsalaSlot=_foundrySlot=_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;
        _boatTime=_shipTime=0;_pendingStoryFilm="";_radioBreath=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewRescuePreview(_order);ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(0,-60);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();_bannerTime=_campaignTextTime=_revealTime=0;RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Kungaminnet · separat prov · tala med Ebba");
    }
    private void DrawPlayableEbba()
    {
        var direction=_game.Moving?PlayerWalkFacing():G(_game.Facing);bool rear=direction.Y<-.15f,mirror=direction.X>0;
        int frame=_game.Guarding?6:_game.AttackTime>0?(_game.Weapon==Weapon.Pistol?7:5):_game.Moving?(int)(_game.Walk*2)%4:4;
        float scale=.39f*_game.PassageScale;var at=RenderPlayer;
        DrawColoredPolygon(Enumerable.Range(0,24).Select(i=>at+new Vector2(Mathf.Cos(i*Mathf.Tau/24)*24,Mathf.Sin(i*Mathf.Tau/24)*9)*_game.PassageScale).ToArray(),new Color(0,0,0,.4f));
        if(_game.Passage==null)DrawArc(at,23,0,Mathf.Tau,40,new Color(Teal,.5f),1.5f,true);
        DrawSetTransform(Offset+at*Zoom,_game.Dead?-.95f:0,new Vector2(mirror?-scale:scale,scale)*Zoom);
        bool walking=_game.Moving&&_game.AttackTime<=0&&!_game.Guarding&&!_game.Dead;
        DrawTextureRectRegion(walking?_ebbaWalk!:rear?_ebbaBack!:_ebbaFront!,new Rect2(-192,-490,384,512),new Rect2(frame%4*384,walking?(rear?512:0):frame/4*512,384,512),_game.Hurt>0?new Color(1.2f,1.05f,1):new Color(.82f,.80f,.76f));
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(_game.Guarding)DrawArc(at+new Vector2(0,-34),39,G(_game.Facing).Angle()-.9f,G(_game.Facing).Angle()+.9f,24,_game.GuardTime<.22f?Teal:Muted,3,true);
        if(_game.AttackTime>0&&_game.AttackContact&&_game.Weapon==Weapon.Saber)DrawArc(at+new Vector2(0,-25),89,G(_game.Facing).Angle()-1.1f,G(_game.Facing).Angle()+1.1f,25,new Color(Pale,.5f),3,true);
        if(_game.RescueState.PistolFlash>0){var s=_game.RescueState;DrawLine(G(s.ShotFrom)+new Vector2(0,-65),G(s.ShotTo)+new Vector2(0,-65),new Color(1,.75f,.3f,s.PistolFlash/.16f),2,true);DrawCircle(at+G(_game.Facing)*32+new Vector2(0,-65),7,new Color(1,.8f,.35f,.7f));}
    }
    private void DrawCensor(Fighter e)
    {
        int pose=e.Dead?3:e.State==1?1:e.State==2?2:0;float scale=.4f;var at=RenderPosition(e);
        DrawSetTransform(Offset+at*Zoom,0,Vector2.One*scale*Zoom);
        DrawTextureRectRegion(_censorArt!,new Rect2(-384,e.Dead?-390:-490,768,512),new Rect2(pose%2*768,pose/2*512,768,512),e.Hurt>0?new Color(1.2f,1.1f,1):Colors.White);
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(!e.Dead&&e.State==1){DrawLine(at+new Vector2(0,-65),G(e.LockedAim),new Color(.8f,.4f,.24f,.7f),2,true);DrawArc(G(e.LockedAim),27,0,Mathf.Tau,32,Gold,2,true);}
    }
    private void AddRescueLayers(List<(float Depth,Action Draw)> layers)
    {
        if(!Rescue.Known(PaintRoom))return;var art=RoomBackground;
        layers.Add((530,()=>PaintForeground(art,new Vector2[]{new(0,0),new(70,0),new(70,570),new(0,590)})));
        layers.Add((580,()=>PaintForeground(art,new Vector2[]{new(1470,0),new(1536,0),new(1536,680),new(1470,650)})));
        if(PaintRoom==Rescue.Prison&&_game.RescueState.Reunited&&!_game.RescueState.Debriefed){var at=new Vector2(945,565);layers.Add((at.Y,()=>{float scale=172f/1500;DrawTextureRect(_ebbaCabin!,new Rect2(at-new Vector2(520,1515)*scale,_ebbaCabin!.GetSize()*scale),false,new Color(.83f,.80f,.75f));}));}
        if(PaintRoom==Rescue.Prison&&_game.IsEbba)
        {var at=G(_game.RescueState.Released?new NVec(850,470):Rescue.Captive);layers.Add((at.Y,()=>DrawActor("karl-saber",at,Vector2.Down,0,0,false)));}
        if(PaintRoom==Rescue.Prison&&_game.RescueState.Trapped&&!_game.RescueState.Released)
        {
            layers.Add((340,()=>{for(int x=680;x<=875;x+=20){DrawLine(new(x,135),new(x,330),new Color(.06f,.065f,.06f),7,true);DrawLine(new(x-2,135),new(x-2,330),new Color(.23f,.22f,.18f),1,true);}DrawLine(new(675,325),new(885,325),new Color(.13f,.14f,.12f),9,true);}));
        }
        if(PaintRoom==Rescue.Machine)
        {foreach(var rect in new[]{new Rect2(390,245,140,390),new Rect2(1015,245,145,390)}){var b=rect;layers.Add((b.End.Y,()=>PaintForeground(art,new[]{b.Position,b.Position+new Vector2(b.Size.X,0),b.End,b.Position+new Vector2(0,b.Size.Y)})));}}
    }
    private bool DrawRescuePrompt()
    {
        string label="";var r=_game.Rooms?.Rescue;if(r==null)return false;
        bool Near(NVec p)=>NVec.Distance(_game.Player,p)<72&&_game.ClearPath(_game.Player,p);
        if(_game.Captured)label="Ta över Ebba ombord på fregatten";
        else if(_game.IsEbba&&_game.InCabin){if(Near(Cabin.Rest))label=r.Ready?"Fältutrustning · TAB byter vapen":"Hämta sabel och tjänstepistol";else if(Near(Cabin.Helm))label="Karl väntar under högen · använd landgången";else if(Near(Cabin.Talk))label="Hedvig följer dig över radion";}
        else if(_game.InRescue)
        {
            switch(_game.Rooms!.Current){case Rescue.Hall:if(Near(Rescue.Writ))label="Läs mottagningsordern";break;case Rescue.Prison:if(Near(Rescue.Talk))label=_game.IsEbba?"Tala med Karl":r.Reunited?"Tillbaka till fregatten":"Lägg originalet på läsbordet";break;case Rescue.Service:if(Near(Rescue.Lever))label=r.ServiceOpen?"Spärren är lossad":"Lossa den mekaniska spärren";break;case Rescue.Machine:if(Near(Rescue.Release))label=r.Released?"Karl är fri":"Återkalla kvarhållningen";break;}
        }
        if(label=="")return false;Panel(new Rect2(230,475,820,53),.93f);Centered("E / B · "+label,640,507,17,Gold);return true;
    }
}
