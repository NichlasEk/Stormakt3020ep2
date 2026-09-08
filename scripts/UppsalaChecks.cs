using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunUppsalaChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewUppsalaPreview(Order.Medicine);ChangeScreen(Screen.Game);SetPhysicsProcess(false);_radio="";_radioQueue.Clear();
            async System.Threading.Tasks.Task Capture(string name)
            {
                _bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var picture=GetViewport().GetTexture().GetImage();var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);if(picture.SavePng(path)!=Error.Ok)throw new Exception("Uppsala capture failed");
            }
            void Use(NVec p){_game.Player=p;_game.Step(default);_game.Step(new(default,NVec.UnitY,false,false,false,false,false,false,false,true));}
            await Capture("uppsala-departure");BeginShipTravel(Uppsala.Court);if(_shipTime<=0)throw new Exception("Ship did not start");await Capture("uppsala-flight");
            int frames=0;while(_shipTime>0&&frames++<9000)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            if(!_game.InUppsala)throw new Exception("Timed flight did not arrive");await Capture("uppsala-arrival");
            Use(Uppsala.Desk);_radio="uppsala-clue";_radioTime=20;await Capture("uppsala-clue");_radio="";_radioQueue.Clear();_sound.StopVoice();
            for(int i=0;i<3;i++)for(int turn=0;_game.UppsalaState.Rings[i]!=Uppsala.Target[i]&&turn<4;turn++)Use(Uppsala.Rings[i]);
            if(!_game.UppsalaState.Aligned)throw new Exception("Instruments failed");await Capture("uppsala-instruments");
            int ticks=0;while(!_game.Dead&&_game.EncounterEnemies.Any(e=>!e.Dead)&&ticks++<15000)_game.Step(OathCheckPilot.Decide(_game));_game.Step(default);
            if(_game.Dead||!_game.UppsalaState.Secured)throw new Exception("Uppsala native combat failed");
            for(int i=0;i<240;i++)_game.Step(default);Use(Uppsala.Seal);if(!_game.UppsalaState.KeyTaken)throw new Exception("Date clue missing");_radio="uppsala-secured";_radioTime=20;await Capture("uppsala-secured");
            foreach(var(id,line) in JourneyDialogue.Uppsala())if(!_sound.HasClip("voice-"+id)||_sound.ClipDuration("voice-"+id)<2||Radio[id].Text!=line[1])throw new Exception("Missing Uppsala voice "+id);
            if(!_sound.HasClip("ship-engine"))throw new Exception("Missing engine");_game.Player=Uppsala.Ramp;if(!_game.FinishShipTravel(Regiment.Quay))throw new Exception("Return failed");_game.ValidateRooms();
            _sound.SetProcess(false);foreach(var p in _sound.GetChildren().OfType<AudioStreamPlayer>()){p.Stop();p.Stream=null;}
            await ToSignal(GetTree().CreateTimer(.2),SceneTreeTimer.SignalName.Timeout);GD.Print("UPPSALA CHECK PASS: timed flight, nine voices, engine, instruments, combat, date, return");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
