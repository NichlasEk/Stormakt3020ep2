using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunMineChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewMinePreview(_order);_game.DeveloperSurvival=true;ChangeScreen(Screen.Game);SetPhysicsProcess(false);_radio="";_radioQueue.Clear();
            async System.Threading.Tasks.Task Capture(string name)
            {
                _bannerTime=_noticeTime=_campaignTextTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var picture=GetViewport().GetTexture().GetImage();var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);if(picture.SavePng(path)!=Error.Ok)throw new Exception("Mine screenshot failed");
            }
            void Use(NVec at){_game.Player=at;_game.Step(default);_game.Step(new(default,NVec.UnitY,false,false,false,false,false,false,false,true));}
            _game.Player=Mine.Latch;await Capture("mine-gate-closed");Use(Mine.Latch);if(!_game.MineState.EntranceOpen)throw new Exception("Mine gate did not release");await Capture("mine-gate-open");
            foreach(var l in RoomLinks.All.Skip(15).Take(3))
            {
                var route=ConnectedWorld.Route(l);_game.Player=route[0]-_game.WorldOrigin;
                foreach(var target in route.Skip(1))
                {
                    int ticks=0;while(NVec.Distance(_game.Player+_game.WorldOrigin,target)>5&&ticks++<3000)
                    {var move=NVec.Normalize(target-_game.Player-_game.WorldOrigin);_game.Step(new(move,move,false,false,false,false,false,false,false,false));foreach(var cue in _game.Events.Where(e=>e.Kind=="world-frame"))HandleCue(cue);}
                    if(ticks>=3000)throw new Exception("Mine path blocked "+l.Id+" at "+target);if(target==route[1])await Capture(l.Id+"-threshold");
                }
                if(_game.Rooms!.Current!=l.B)throw new Exception("Mine arrival failed "+l.Id);
                await Capture(l.Id+"-arrival");foreach(var e in _game.Enemies)e.Health=0;_game.Hazards.Clear();_game.Shots.Clear();
                if(l.B==Mine.Mouth){Use(Mine.Ledger);_radio="mine-ledger";_radioTime=20;await Capture("mine-ledger");_radio="";}
                if(l.B==Mine.Bellows)
                {
                    _game.Player=new(770,690);_game.MineState.Pulse=.01f;_game.Step(default);await Capture("mine-steam-warning");
                    Use(Mine.Feed);Use(Mine.Relief);if(!_game.MineState.PressureReleased)throw new Exception("Valve interactions failed");await Capture("mine-pressure-safe");
                }
                if(l.B==Mine.Coolway){Use(Mine.Imprint);if(!_game.MineState.ImprintTaken)throw new Exception("Crown investigation failed");await Capture("mine-crown-mold");}
            }
            _screen=Screen.Journal;await Capture("mine-world-journal");_screen=Screen.Game;
            foreach(var (id,line) in JourneyDialogue.Mine())if(!_sound.HasClip("voice-"+id)||_sound.ClipDuration("voice-"+id)<2||Radio[id].Text!=line[1])throw new Exception("Missing mine radio "+id);
            foreach(var id in new[]{"mine-valve","mine-warning","mine-steam"})if(!_sound.HasClip(id))throw new Exception("Missing mine sound "+id);
            _sound.SetProcess(false);foreach(var p in _sound.GetChildren().OfType<AudioStreamPlayer>()){p.Stop();p.Stream=null;}
            await ToSignal(GetTree().CreateTimer(.2),SceneTreeTimer.SignalName.Timeout);GD.Print("MINE CHECK PASS: gate, three rooms, steam, valves, crown, journal and eight voices");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
