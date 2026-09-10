using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private readonly Dictionary<string,Texture2D> _observatoryArt=new();
    private Texture2D? _meridianOpen,_martaArt,_zenithArt,_workshopEmpty;
    private readonly Vector2[] _zenithFeet=new Vector2[4];
    private readonly float[] _zenithScale=new float[4];
    private bool _observatorySlot;
    private void StartObservatory(bool fresh=false)
    {
        LoadWaterArt();_saltSlot=false;_westSlot=false;_gamlaSlot=false;_observatorySlot=true;_meridianSlot=_uppsalaSlot=_foundrySlot=_mineSlot=_regimentSlot=_doorSlot=_roomsSlot=_portSlot=_atlandSlot=false;
        _boatTime=_shipTime=0;_pendingStoryFilm="";_radioBreath=0;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewObservatoryPreview(_order);ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(0,-60);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_radioTime=0;_sound.StopVoice();
        _bannerTime=_campaignTextTime=_revealTime=0;RememberRenderPositions();ChangeScreen(Screen.Game);Save();Notice("Separat observatorieprov · E öppnar Meridiansalens högra port");
    }
    private void LoadObservatoryArt()
    {
        if(_zenithArt!=null)return;
        foreach(var id in Observatory.Ids)_observatoryArt[id]=GD.Load<Texture2D>("res://assets/art/room-"+id+"-v1.png");
        _workshopEmpty=GD.Load<Texture2D>("res://assets/art/room-observatory-workshop-empty-v1.png");
        _meridianOpen=GD.Load<Texture2D>("res://assets/art/room-meridian-observatory-open-v1.png");
        _martaArt=SpriteCutout.Load("res://assets/art/marta-vinge-v1.png",chromaKey:new Color(0,1,0));
        _zenithArt=GD.Load<Texture2D>("res://assets/art/zenith-guardian-v1.png");
        using var image=_zenithArt.GetImage();if(image.IsCompressed())image.Decompress();int cell=image.GetWidth()/2;
        for(int pose=0;pose<4;pose++)
        {
            int top=cell,bottom=0;for(int y=0;y<cell;y++)for(int x=0;x<cell;x++)if(image.GetPixel(pose%2*cell+x,pose/2*cell+y).A>.5f){top=Math.Min(top,y);bottom=Math.Max(bottom,y);}
            long sum=0,count=0;for(int y=Math.Max(top,bottom-18);y<=bottom;y++)for(int x=0;x<cell;x++)if(image.GetPixel(pose%2*cell+x,pose/2*cell+y).A>.5f){sum+=x;count++;}
            _zenithFeet[pose]=new(count>0?(float)sum/count:cell*.5f,bottom);_zenithScale[pose]=(pose==0?215f:pose==2?75f:95f)/Math.Max(1,bottom-top);
        }
    }
    private void DrawZenith(Fighter e)
    {
        int pose=e.Dead?3:e.Kind==EnemyKind.ZenithLock?2:0;float cell=_zenithArt!.GetWidth()/2f;var at=RenderPosition(e);float scale=_zenithScale[pose]*(e.Dead&&e.Kind==EnemyKind.ZenithLock?.45f:1);
        DrawSetTransform(Offset+at*Zoom,0,Vector2.One*scale*Zoom);
        DrawTextureRectRegion(_zenithArt,new Rect2(-_zenithFeet[pose],new(cell,cell)),new Rect2(pose%2*cell,pose/2*cell,cell,cell),e.Hurt>0?new Color(1.2f,1.05f,.9f):Colors.White);
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(e.Dead||e.Kind==EnemyKind.ZenithLock)return;
        float angle=e.State==1?G(e.LockedAim-e.Position).Angle():_game.ObservatoryState.Awake?_clock*.5f:0;
        for(int i=0;i<2;i++)
        {
            DrawSetTransform(Offset+(at+new Vector2(0,-100))*Zoom,angle+i*Mathf.Pi,new Vector2(.32f,.20f)*Zoom);
            DrawTextureRectRegion(_zenithArt,new Rect2(new(-50,-310),new(cell,cell)),new Rect2(cell,0,cell,cell));
        }
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(e.State==1){var aim=G(e.LockedAim);DrawLine(at,aim,new Color(.78f,.48f,.20f,.6f),2,true);DrawArc(aim,58,0,Mathf.Tau,40,new Color(.85f,.52f,.22f,.8f),2,true);}
    }
    private void AddObservatoryLayers(List<(float Depth,Action Draw)> layers)
    {
        string room=PaintRoom;if(!Observatory.Known(room)&&room!=Meridian.Hall)return;var art=RoomBackground;
        void Wall(Vector2[] polygon,float depth){layers.Add((depth,()=>PaintForeground(art,polygon)));}
        if(room==Meridian.Hall)
        {
            Wall(new[]{new Vector2(1275,40),new(1340,40),new(1340,470),new(1275,440)},445);
            Wall(new[]{new Vector2(1420,70),new(1536,70),new(1536,540),new(1420,490)},490);return;
        }
        if(room==Observatory.Quarters)
        {
            var at=G(Observatory.Marta);layers.Add((at.Y,()=>
            {
                if(_game.WestState.Debriefed||!_game.CanSeeRoomPoint(Observatory.Talk+ConnectedWorld.Origin(room)-_game.WorldOrigin))return;
                DrawColoredPolygon(new[]{at+new Vector2(-22,0),at+new Vector2(0,-9),at+new Vector2(25,0),at+new Vector2(0,9)},new Color(0,0,0,.3f));
                float scale=172f/1365;DrawTextureRect(_martaArt!,new Rect2(at-new Vector2(520,1420)*scale,_martaArt!.GetSize()*scale),false);
            }));
            Wall(new[]{new Vector2(0,0),new(45,0),new(45,370),new(0,395)},365);
            Wall(new[]{new Vector2(1380,0),new(1410,0),new(1410,440),new(1380,422)},427);
        }
        if(room==Observatory.Workshop)
        {
            foreach(var shape in Observatory.Obstacles(room))Wall(shape.Select(G).Select(p=>p-new Vector2(0,22)).ToArray(),shape.Max(p=>p.Y)-8);
            Wall(new[]{new Vector2(0,0),new(35,0),new(35,460),new(0,485)},460);
            Wall(new[]{new Vector2(1355,0),new(1410,0),new(1410,475),new(1355,450)},470);
        }
        if(room==Observatory.Machine)
        {
            Wall(new[]{new Vector2(680,0),new(850,0),new(850,245),new(818,245),new(792,217),new(745,211),new(705,245),new(680,245)},390);
            Wall(new[]{new Vector2(655,245),new(706,245),new(706,390),new(655,410)},395);
            Wall(new[]{new Vector2(816,245),new(870,245),new(870,420),new(816,390)},400);
            for(int i=0;i<3;i++)
            {
                int brake=i;var at=G(Observatory.Brakes[i]);layers.Add((at.Y,()=>
                {
                    var delta=ConnectedWorld.Origin(room)-_game.WorldOrigin;if(!_game.CanSeeRoomPoint(Observatory.Brakes[brake]+delta))return;
                    Text(new[]{"0","I","II"}[_game.ObservatoryState.Brakes[brake]],at+new Vector2(-6,-14),17,Gold);
                }));
            }
        }
    }
    private bool DrawObservatoryPrompt()
    {
        if(!_game.InUppsala)return false;string label="";var o=_game.ObservatoryState;
        bool Near(NVec p)=>NVec.Distance(_game.Player,p)<75&&_game.ClearPath(_game.Player,p);
        if(_game.Rooms!.Current==Meridian.Hall&&_game.CabinState.Briefed&&Near(Observatory.Gate)&&!o.EntryOpen)label="Öppna vägen till observatoriet";
        if(_game.Rooms.Current==Observatory.Quarters&&Near(Observatory.Talk))label="Tala med Märta Vinge";
        if(_game.Rooms.Current==Observatory.Workshop){if(Near(Observatory.Diagram))label="Läs stjärndiagrammet";else if(Near(Observatory.Cabinet))label=o.CabinetOpened?"Gömman är tömd · verkstadsnyckeln tagen":"Öppna Märtas gömma";}
        if(_game.Rooms.Current==Observatory.Machine)
        {
            if(Near(Observatory.Shortcut)&&!o.ShortcutOpen)label="Lås upp underhållsdörren";
            else for(int i=0;i<3;i++)if(Near(Observatory.Brakes[i]))label=o.Aligned?"Motvikten är säkrad":"Vrid broms "+(i+1)+" · "+new[]{"0","I","II"}[o.Brakes[i]];
        }
        if(_game.Rooms.Current==Observatory.Dome){if(Near(Observatory.Wake)&&!o.Awake)label="Undersök drivverket";else if(Near(Observatory.Original))label=o.Defeated?"Läs den strukna destinationen":"Originalet är bevakat";}
        if(label=="")return false;Panel(new Rect2(260,475,760,53),.93f);Centered("E / B · "+label,640,507,17,Gold);return true;
    }
    private void DisposeObservatoryArt(){foreach(var t in _observatoryArt.Values)t.Dispose();_workshopEmpty?.Dispose();_meridianOpen?.Dispose();_martaArt?.Dispose();_zenithArt?.Dispose();}
}
