using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunRegimentChecks(bool oldPorts=false)
    {
        try
        {
            LoadWaterArt();_game=Combat.NewRegimentPreview(_order);_game.DeveloperSurvival=true;
            ChangeScreen(Screen.Game);SetPhysicsProcess(false);_radio="";_radioQueue.Clear();_bannerTime=_noticeTime=_campaignTextTime=0;
            async Task Capture(string name)
            {
                _camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var picture=GetViewport().GetTexture().GetImage();var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                if(picture.SavePng(path)!=Error.Ok)throw new Exception("Regiment capture failed");
            }
            var links=oldPorts?RoomLinks.All.Take(9).ToArray():RoomLinks.All.Skip(9).Take(5).ToArray();
            foreach(var link in links)
            {
                _game.Enemies.Clear();_game.Rooms!.Current=link.A;_game.Rooms.Rooms[link.A].Visited=true;
                _game.RegimentState.Discharged=true;var leaf=_game.Rooms.Doors[link.Id];leaf.Locked=false;leaf.TargetOpen=true;leaf.Openness=1;
                var route=ConnectedWorld.Route(link);_game.Player=route[0]-_game.WorldOrigin;
                await Capture("route-"+link.Id+"-approach");
                foreach(var target in route.Skip(1))
                {
                    int steps=0;while(NVec.Distance(_game.Player+_game.WorldOrigin,target)>5&&steps++<3500)
                    {var move=NVec.Normalize(target-_game.WorldOrigin-_game.Player);_game.Step(new(move,move,false,false,false,false,false,false,false,false));foreach(var cue in _game.Events.Where(c=>c.Kind=="world-frame"))HandleCue(cue);}
                    if(steps>=3500)throw new Exception("Blocked passage "+link.Id+" at "+target);
                    if(target==route[1])await Capture("route-"+link.Id+"-threshold");
                }
                if(_game.Rooms.Current!=link.B)throw new Exception("Wrong destination "+link.Id);
                await Capture("route-"+link.Id+"-arrival");
                if(link.B==Regiment.Barracks)
                {
                    _game.Player=Regiment.Captain+new NVec(90,40);_radio="regiment-captain";_radioTime=20;
                    await Capture("regiment-captain-radio");_radio="";_radioTime=0;
                }
                if(link.B==Regiment.Parade)
                {
                    _game.Player=new(750,650);_game.Enemies.Clear();_game.Spawn(EnemyKind.RootMarshal,new(800,530));_game.RegimentState.Discharged=false;
                    await Capture("regiment-marshal-formation");_game.RegimentState.Exposed=8;Array.Fill(_game.RegimentState.Standards,0);_game.Enemies.Single().State=3;
                    await Capture("regiment-marshal-exposed");_game.RegimentState.MarshalDefeated=true;_game.RegimentState.Discharged=true;
                }
            }
            if(!oldPorts)
            {
                foreach(var(id,line) in JourneyDialogue.Regiment())if(!_sound.HasClip("voice-"+id)||_sound.ClipDuration("voice-"+id)<2||Radio[id].Text!=line[1])throw new Exception("Missing voice/subtitle "+id);
                _game.Player=Regiment.Boat;_boatDestination=Regiment.Farled;_boatTime=3;await Capture("regiment-boat-crossing");_boatTime=0;
                if(!_game.FinishBoatCrossing(Regiment.Farled))throw new Exception("Boat arrival failed");foreach(var cue in _game.Events.ToArray())HandleCue(cue);await Capture("regiment-farled-arrival");
            }
            _sound.SetProcess(false);foreach(var player in _sound.GetChildren().OfType<AudioStreamPlayer>()){player.Stop();player.Stream=null;}
            await ToSignal(GetTree().CreateTimer(.2),SceneTreeTimer.SignalName.Timeout);
            GD.Print(oldPorts?"PORTS CHECK PASS: all nine painted connections traversed":"REGIMENT CHECK PASS: five places, captain portrait, marshal poses, nine voices and boat arrival");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
