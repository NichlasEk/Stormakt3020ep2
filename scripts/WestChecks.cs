using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunWestChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewWestPreview(_order);ChangeScreen(Screen.Game);SetPhysicsProcess(false);
            var enter=typeof(Combat).GetMethod("EnterConnectedRoom",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            void Enter(string id,NVec at){enter.Invoke(_game,new object[]{id});_game.Player=at;}
            async System.Threading.Tasks.Task Capture(string name)
            {
                _radio="";_radioQueue.Clear();_sound.StopVoice();_bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);using var img=GetViewport().GetTexture().GetImage();if(img.SavePng(path)!=Error.Ok)throw new Exception("Capture failed");
            }
            Enter(Gamla.Registry,new(1240,600));var entry=RoomLinks.All.Single(l=>l.Id=="gamla-west-entry");var entryDoor=_game.Rooms!.Doors[entry.Id];entryDoor.Locked=false;entryDoor.TargetOpen=true;entryDoor.Openness=1;
            await Capture("registry-door-approach");
            for(int i=0;i<200&&_game.Passage==null;i++)_game.Step(new(-NVec.UnitY,-NVec.UnitY,false,false,false,false,false,false,false,false));
            if(_game.Passage==null)throw new Exception("Registry entry unreachable from left side of painted opening");
            for(int i=0;i<20;i++)_game.Step(default);await Capture("registry-door-entering");while(_game.Passage!=null)_game.Step(default);
            if(_game.Rooms.Current!=West.Control)throw new Exception("Registry entry led to wrong room");
            Enter(West.Control,new(770,575));await Capture("west-control");foreach(var e in _game.Enemies)e.Health=0;_game.WestState.RegisterRead=true;
            Enter(West.Hall,new(750,665));var boss=_game.Enemies.Single(e=>e.Kind==EnemyKind.MusterOfficer);boss.State=1;boss.Timer=.9f;boss.LockedAim=_game.Player;await Capture("west-boss");
            _game.WestState.Exposed=7;_game.WestState.Brakes=3;boss.State=2;await Capture("west-boss-exposed");boss.Health=0;_game.WestState.Defeated=true;_game.WestState.Exposed=0;
            var link=RoomLinks.All.Single(l=>l.Id=="gamla-west-exit");_game.Player=link.ArrivalA;for(int i=0;i<90;i++)_game.Step(default);_game.Player=link.AtA;var target=ConnectedWorld.Route(link)[1]-_game.WorldOrigin;
            for(int i=0;i<180&&_game.Passage==null;i++){var d=NVec.Normalize(target-_game.Player);_game.Step(new(d,d,false,false,false,false,false,false,false,false));}
            if(_game.Passage==null)throw new Exception("West painted exit not triggered");for(int i=0;i<25;i++)_game.Step(default);await Capture("west-door-receding");while(_game.Passage!=null)_game.Step(default);
            _game.Player=West.Talk;await Capture("west-elin");_game.WestState.RecordRead=_game.WestState.ElinMet=true;_game.WestState.Conversation=2;Enter(Cabin.Room,Cabin.Talk);await Capture("west-aboard");_game.ValidateRooms();
            foreach(var(id,line) in JourneyDialogue.West())if(Radio[id].Text!=line[1]||!_sound.HasClip("voice-"+id))throw new Exception("Missing west line "+id);
            GD.Print("WEST CHECK PASS: three painted rooms, boss poses, threshold, Elin, cabin and seventeen voices");GetTree().Quit();
        }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
