using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunConnectedChecks(bool arch=false,bool rootArches=false)
    {
        try
        {
            LoadWaterArt();_game=Combat.NewRooms(_order);_game.Events.Clear();_game.EnableConnectedWorld();_game.DeveloperSurvival=true;_game.Rooms!.KeyTaken=_game.Rooms.DoorOpen=true;
            var door=_game.Rooms.Doors["lodge"];door.Locked=false;door.TargetOpen=true;door.Openness=1;
            foreach(var e in _game.Enemies)e.Health=0;
            _game.Player=ConnectedWorld.Route(RoomLinks.All[0])[0]-_game.WorldOrigin;_game.UpdateRoomSight(true);
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
            if(rootArches)
            {
                _game.Enemies.Clear();_game.Rooms.ArchiveSecured=_game.Rooms.RootGateOpen=true;
                foreach(var snapshot in _game.Rooms.Rooms.Values)snapshot.Visited=true;
                foreach(var id in new[]{"roots","grove"})
                {
                    var link=RoomLinks.All.First(l=>l.Id==id);var leaf=_game.Rooms.Doors[id];
                    leaf.Locked=false;leaf.TargetOpen=false;leaf.Openness=0;
                    _game.Rooms.Current=link.A;var route=ConnectedWorld.Route(link);
                    _game.Player=route[0]-_game.WorldOrigin;_game.UpdateRoomSight(true);
                    await Capture(id+"-arch-closed");
                    leaf.TargetOpen=true;
                    for(int tick=0;tick<80;tick++)_game.Step(default);
                    if(leaf.Openness<.99f)throw new Exception("Door failed to open: "+id);
                    await Capture(id+"-arch-open");
                    foreach(var target in route.Skip(1))
                    {
                        int steps=0;while(NVec.Distance(_game.Player+_game.WorldOrigin,target)>5&&steps++<2000)
                        {var move=NVec.Normalize(target-_game.WorldOrigin-_game.Player);_game.Step(new(move,move,false,false,false,false,false,false,false,false));foreach(var cue in _game.Events.Where(c=>c.Kind=="world-frame"))HandleCue(cue);}
                        if(steps>=2000)throw new Exception("Blocked root arch: "+id+" at "+target);
                        if(target==route[1])await Capture(id+"-arch-under-vault");
                    }
                    if(_game.Rooms.Current!=link.B)throw new Exception("Wrong room after arch: "+id);
                    await Capture(id+"-arch-arrival");
                }
                await SettleAudio();GD.Print("ROOT ARCH CHECK PASS: archive and root doors, opening, vaults and continuous arrivals");GetTree().Quit();return;
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
            foreach(var target in ConnectedWorld.Route(RoomLinks.All[0]).Skip(1))
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
