using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private Texture2D? _saltArt,_saltWalkArt,_saltRefugeOpen;
    private bool _saltSlot;
    private void LoadSaltArt()
    {
        if(_saltArt!=null)return;
        _saltArt=SpriteCutout.Load("res://assets/art/salt-warden-v1.png",chromaKey:new Color(0,1,0));
        _saltWalkArt=SpriteCutout.Load("res://assets/art/salt-warden-walk-v1.png",chromaKey:new Color(0,1,0));
        _saltRefugeOpen=GD.Load<Texture2D>("res://assets/art/room-west-salt-open-v1.png");
    }
    private void StartSalt(bool fresh=false)
    {
        LoadWaterArt();_rescueSlot=false;_saltSlot=true;_westSlot=_gamlaSlot=_observatorySlot=_meridianSlot=_uppsalaSlot=_foundrySlot=_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;
        _boatTime=_shipTime=0;_pendingStoryFilm="";_radioBreath=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewSaltPreview(_order);ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(0,-60);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();_bannerTime=_campaignTextTime=_revealTime=0;RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Saltkällan · separat prov · tala med Ebba");
    }
    private void DrawSaltWarden(Fighter e)
    {
        int pose=e.Dead?3:e.State==1?1:e.State==2?2:0;
        bool walk=e.Moving&&!e.Dead&&e.State==0;if(walk)pose=(int)(e.Walk*4)%4;
        float scale=.40f;var at=RenderPosition(e);
        DrawSetTransform(Offset+at*Zoom,0,Vector2.One*scale*Zoom);
        DrawTextureRectRegion(walk?_saltWalkArt!:_saltArt!,new Rect2(new Vector2(-384,walk?-490:-495),new(768,512)),new Rect2(pose%2*768,pose/2*512,768,512),e.Hurt>0?new Color(1.2f,1.1f,1):Colors.White);
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(e.State==1&&!e.Dead)
        {
            var aim=G(e.LockedAim);DrawArc(aim,105,0,Mathf.Tau,48,new Color(.85f,.55f,.24f,.8f),3,true);
            DrawLine(at+new Vector2(0,-60),aim,new Color(.55f,.55f,.42f,.75f),3,true);
        }
    }
    private void AddSaltLayers(List<(float Depth,Action Draw)> layers)
    {
        string room=PaintRoom;
        if(room==Cabin.Room&&_game.WestState.Debriefed)
        {
            var at=G(Salt.Marta);layers.Add((at.Y,()=>{float scale=172f/1365;DrawTextureRect(_martaArt!,new Rect2(at-new Vector2(520,1420)*scale,_martaArt!.GetSize()*scale),false);}));return;
        }
        if(!Salt.Known(room))return;
        var art=RoomBackground;
        // Foreground jambs cover the figure during the painted depth transition.
        layers.Add((495,()=>PaintForeground(art,new[]{new Vector2(0,0),new(65,0),new(65,570),new(0,590)})));
        layers.Add((560,()=>PaintForeground(art,new[]{new Vector2(1470,0),new(1536,0),new(1536,680),new(1470,650)})));
        if(room==Salt.Spring)
        {
            var s=_game.SaltState;
            layers.Add((-100,()=>
            {
                if(s.Defeated)return;
                for(int i=0;i<2;i++)
                {
                    if((s.Chains&(1<<i))!=0)continue;
                    var polygon=Salt.FloodZones[i].Select(G).ToArray();var center=polygon.Aggregate(Vector2.Zero,(a,b)=>a+b)/polygon.Length;
                    // Soft irregular banks follow the painted floor rather than rectangular HUD zones.
                    for(int edge=0;edge<6;edge++)
                    {
                        float inset=1-edge*.025f;
                        var points=polygon.Select(p=>center+(p-center)*inset).ToArray();
                        DrawColoredPolygon(points,s.Tide>=9?new Color(.6f,.49f,.25f,.025f):new Color(.015f,.10f,.12f,.095f));
                    }
                    if(s.Tide<9)for(int n=0;n<9;n++)
                    {
                        var start=center+new Vector2(-85+n%3*42,-65+n*17+(float)(_game.Elapsed*9%13));
                        DrawLine(start,start+new Vector2(35,5),new Color(.55f,.65f,.6f,.18f),1,true);
                    }
                }
            }));
            foreach(var p in Salt.Chains){var point=p;layers.Add((p.Y,()=>{if(_game.CanSeeRoomPoint(point)){var used=(s.Chains&(1<<Array.IndexOf(Salt.Chains,point)))!=0;DrawArc(G(point),24,0,Mathf.Tau,32,new Color(used?Teal:Gold,.55f),2,true);}}));}
        }
    }
    private bool DrawSaltPrompt()
    {
        if(!_game.InSalt)return false;string room=_game.Rooms!.Current;string label="";var s=_game.SaltState;
        bool Near(NVec p)=>NVec.Distance(_game.Player,p)<72&&_game.ClearPath(_game.Player,p);
        if(room==Salt.Loading&&Near(Salt.Manifest))label="Läs lastmanifestet";
        if(room==Salt.Stairs&&Near(Salt.Wheel))label=s.Drained?"Tillförseln är stängd":"Stäng tillförseln";
        if(room==Salt.Archive){if(Near(Salt.Ledger))label="Läs minnesverkets instruktion";else if(Near(Salt.Cache))label=s.CacheTaken?"Kistan är tom":"Öppna underhållskistan";}
        if(room==Salt.Spring){if(Near(Salt.Release))label=s.Released?"Vittnena är fria · underhållsgång bakom pulpeten":"Häv kvarhållningen";else if(!s.Defeated)for(int i=0;i<2;i++)if(Near(Salt.Chains[i]))label=(s.Chains&(1<<i))!=0?"Luckan är stängd":"Dra kedjespelet · sänk vattnet";}
        if(label=="")return false;Panel(new Rect2(230,475,820,53),.93f);Centered("E / B · "+label,640,507,17,Gold);return true;
    }
}
