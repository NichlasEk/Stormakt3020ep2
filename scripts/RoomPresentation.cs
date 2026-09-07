using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _roomsSlot,_roomChecks;
    private void StartRooms()
    {
        _roomsSlot=true;_atlandSlot=_portSlot=false;
        if(!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewRooms(_order);ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        _revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();ChangeScreen(Screen.Game);
        foreach(var cue in _game.Events)HandleCue(cue);Save();
    }
    private void DrawRoomMarkers()
    {
        var run=_game.Rooms!;var door=G(PortRooms.Door(run.Current));
        Text(run.DoorOpen?run.Current==PortRooms.Court?"TILL LOGEMENTET":"TILL FÖRGÅRDEN":"LÅST DÖRR",door+new Vector2(-70,38),12,Gold);
        if(run.Current==PortRooms.Court&&!run.KeyTaken)
        {var p=G(PortRooms.Key);DrawCircle(p,17,new Color(.03f,.04f,.03f,.9f));DrawArc(p,7,0,Mathf.Tau,20,Gold,2,true);DrawLine(p+new Vector2(5,5),p+new Vector2(17,17),Gold,3,true);Text("VÄKTARENS PACKNING",p+new Vector2(-80,38),12,Gold);}
        if(run.Current==PortRooms.Lodge&&!run.CacheTaken)Text("FÖRSEGLAD KISTA",G(PortRooms.Cache)+new Vector2(-60,38),12,Gold);
    }
    private void AddRoomLayers(List<(float Depth,Action Draw)> layers)
    {
        var run=_game.Rooms!;var door=G(PortRooms.Door(run.Current));
        // Use the architectural doorways already present in the paintings.
        // The bronze gate has an explicit lock indicator; final moving leaves are a later art pass.
        if(run.Current==PortRooms.Court)
        {
            layers.Add((door.Y-100,()=>
            {
                var lockAt=new Vector2(490,158);
                DrawCircle(lockAt,13,new Color(.035f,.04f,.03f,.92f));
                DrawArc(lockAt+new Vector2(0,-5),6,Mathf.Pi,run.DoorOpen?Mathf.Pi*1.75f:Mathf.Tau,18,run.DoorOpen?Teal:Gold,2,true);
                DrawRect(new Rect2(lockAt+new Vector2(-6,-5),new Vector2(12,12)),run.DoorOpen?Teal:Gold,false,2);
            }));
        }
        if(run.Current==PortRooms.Lodge)
        {
            var at=G(PortRooms.Cache);layers.Add((at.Y,()=>
            {var source=new Rect2(671,895,225,212);var size=source.Size*(55/source.Size.Y);
                DrawTextureRectRegion(_portProps,new Rect2(at-new Vector2(size.X/2,size.Y-3),size),source,run.CacheTaken?new Color(.5f,.5f,.46f):Colors.White);
            }));
        }
    }
    private void DrawRoomPrompt()
    {
        var r=_game.Rooms!;string prompt="";
        bool Near(System.Numerics.Vector2 p)=>System.Numerics.Vector2.Distance(_game.Player,p)<72&&_game.ClearPath(_game.Player,p);
        if(Near(PortRooms.Door(r.Current)))prompt=!r.KeyTaken?"Låst · sök väktarens nyckel":!r.DoorOpen?"E / B · Lås upp dörren":"E / B · Gå genom dörren";
        else if(r.Current==PortRooms.Court&&!r.KeyTaken&&Near(PortRooms.Key))prompt="E / B · Sök väktarens packning";
        else if(r.Current==PortRooms.Lodge&&!r.CacheTaken&&Near(PortRooms.Cache))prompt="E / B · Undersök kistan";
        if(prompt=="")return;
        if(_game.Enemies.Any(e=>!e.Dead))prompt="Slå tillbaka rummets väktare";
        Panel(new Rect2(390,412,500,59),.94f);Centered(prompt,640,442,17,Gold);
    }
    private void DrawRoomJournal()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.025f,.023f,.019f,.96f));
        Text("ATLAND · DE FÖRSEGLADE RUMMEN",new Vector2(95,100),26,Pale,true);
        Wrapped("En gammal logementdörr leder in under porten. Väktarens packning i förgården kan innehålla nyckeln. Säkra rummet och undersök kistan innanför.",new Vector2(95,170),1000,21,Muted,34);
        var r=_game.Rooms!;
        Text("Förgården  ↔  "+(r.Rooms[PortRooms.Lodge].Visited?"Väktarnas logement":"Oundersökt rum"),new Vector2(95,340),24,Gold);
        Text($"Nyckel: {(r.KeyTaken?"säkrad":"saknas")} · Dörr: {(r.DoorOpen?"öppen":"låst")} · Vittnessigill: {(r.CacheTaken?"funnet":"återstår")}",new Vector2(95,400),19,Pale);
        Wrapped(r.CacheTaken?"Ritningen visar en cistern under logementet. Den delen av expeditionen återstår i nästa byggpass. Du kan återvända mellan de två rummen och hämta kvarlämnade fynd.":"Återbesök minns besegrade vakter och kvarlämnade fynd. Nyckeln förvaras tillsammans med expeditionens handlingar och tar ingen väskplats.",new Vector2(95,480),1000,19,Muted,30);
        Button(new Rect2(830,614,340,49),"Tillbaka","back",true);
    }

    private async void RunRoomChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
            async Task Capture(string name)
            {
                _camera=G(_game.Player)+new Vector2(0,-50);RememberRenderPositions();_bannerTime=_noticeTime=_campaignTextTime=0;
                QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();
                var path=ProjectSettings.GlobalizePath($"res://artifacts/{name}.png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                Check(image.SavePng(path)==Error.Ok,"Save room screenshot");
            }
            void Use(System.Numerics.Vector2 at)
            {
                _game.Player=at;_game.Step(default);_game.Step(new(default,System.Numerics.Vector2.UnitY,false,false,false,false,false,false,false,true));
                foreach(var cue in _game.Events)HandleCue(cue);
            }
            foreach(var e in _game.Enemies)e.Health=0;
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(!_game.Rooms!.DoorOpen,"Locked before key");await Capture("rooms-locked");
            Use(PortRooms.Key);Check(_game.Rooms.KeyTaken,"Key found");
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(_game.Rooms.DoorOpen,"Door opens");await Capture("rooms-open");
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(_game.Rooms.Current==PortRooms.Lodge,"Arrive in lodge");await Capture("rooms-lodge");
            foreach(var e in _game.Enemies)e.Health=0;
            Use(PortRooms.Cache);Check(_game.Rooms.CacheTaken&&_game.LocalDrops.Any(),"Chest drops a room-local item");await Capture("rooms-cache");
            Use(PortRooms.LodgeDoor+new System.Numerics.Vector2(-25,35));Check(_game.Rooms.Current==PortRooms.Court&&!_game.LocalDrops.Any(),"Return hides other room loot");
            Use(PortRooms.CourtDoor+new System.Numerics.Vector2(0,45));Check(_game.Enemies.All(e=>e.Dead)&&_game.LocalDrops.Any(),"Revisit preserves dead guards and loot");
            OpenInventory(2);await Capture("rooms-stats");Back();
            GD.Print("ROOM CHECK PASS: locked/open door, key, passage, distinct rooms, cache, revisit, inventory");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
