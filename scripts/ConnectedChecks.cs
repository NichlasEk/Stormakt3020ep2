using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunConnectedChecks(bool arch=false)
    {
        try
        {
            LoadWaterArt();_game=Combat.NewRooms(_order);_game.Events.Clear();_game.EnableConnectedWorld();_game.DeveloperSurvival=true;_game.Rooms!.KeyTaken=_game.Rooms.DoorOpen=true;
            var door=_game.Rooms.Doors["lodge"];door.Locked=false;door.TargetOpen=true;door.Openness=1;
            foreach(var e in _game.Enemies)e.Health=0;
            _game.Player=ConnectedWorld.Center(RoomLinks.All[0])-new NVec(70,0);_game.UpdateRoomSight(true);
            ChangeScreen(Screen.Game);SetPhysicsProcess(false);_radio="";_radioQueue.Clear();_bannerTime=_noticeTime=_campaignTextTime=0;
            async Task SettleAudio()
            {
                // Let the audio thread retire its looping playbacks before engine teardown.
                _sound.SetProcess(false);foreach(var player in _sound.GetChildren().OfType<AudioStreamPlayer>()){player.Stop();player.Stream=null;}
                await ToSignal(GetTree().CreateTimer(.2),SceneTreeTimer.SignalName.Timeout);
            }
            async Task Capture(string name)
            {
                _camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();var p=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(p)!);
                if(image.SavePng(p)!=Error.Ok)throw new Exception("Capture failed");
            }
            if(arch)
            {
                _game.Rooms.Current=PortRooms.Gallery;_game.Rooms.Rooms[PortRooms.Gallery].Visited=true;_game.Rooms.WitnessRead=true;_game.Enemies.Clear();
                var route=ConnectedWorld.Route(RoomLinks.All[5]);_game.Player=route[0]-_game.WorldOrigin;_game.UpdateRoomSight(true);await Capture("painted-arch-approach");
                foreach(var target in route.Skip(1))
                {
                    int steps=0;while(NVec.Distance(_game.Player+_game.WorldOrigin,target)>5&&steps++<2000)
                    {var move=NVec.Normalize(target-_game.WorldOrigin-_game.Player);_game.Step(new(move,move,false,false,false,false,false,false,false,false));foreach(var cue in _game.Events.Where(c=>c.Kind=="world-frame"))HandleCue(cue);}
                    if(steps>=2000)throw new Exception("Blocked painted arch: "+target);
                    if(target==route[1])await Capture("painted-arch-under-vault");
                }
                if(_game.Rooms.Current!=PortRooms.Chamber)throw new Exception("Painted doorway did not lead to chamber");
                await Capture("painted-arch-arrival");await SettleAudio();GD.Print("ARCH CHECK PASS: painted doorway, vault occlusion and continuous chamber arrival");GetTree().Quit();return;
            }
            await Capture("connected-door");
            foreach(var target in ConnectedWorld.Route(RoomLinks.All[0]).Skip(2))
            {
                for(int i=0;i<1600&&NVec.Distance(_game.Player+_game.WorldOrigin,target)>5;i++)
                {
                    var cameraWorld=_camera+G(_game.WorldOrigin);var move=NVec.Normalize(target-_game.WorldOrigin-_game.Player);_game.Step(new(move,move,false,false,false,false,false,false,false,false));
                    foreach(var cue in _game.Events.Where(c=>c.Kind=="world-frame"))HandleCue(cue);
                    if((_camera+G(_game.WorldOrigin)).DistanceTo(cameraWorld)>.01f)throw new Exception("Camera jumped at room boundary");
                }
            }
            if(_game.Rooms.Current!=PortRooms.Lodge)throw new Exception("Could not walk to lodge");
            await Capture("connected-lodge");
            await SettleAudio();GD.Print("CONNECTED CHECK PASS: continuous walking, camera frame, painted adjoining rooms and physical door");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
