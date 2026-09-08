using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunFoundryChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewFoundryPreview(Order.Medicine);ChangeScreen(Screen.Game);SetPhysicsProcess(false);_radio="";_radioQueue.Clear();
            async System.Threading.Tasks.Task Capture(string name)
            {
                _bannerTime=_noticeTime=_campaignTextTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var picture=GetViewport().GetTexture().GetImage();var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);if(picture.SavePng(path)!=Error.Ok)throw new Exception("Foundry capture failed");
            }
            void Use(NVec p){_game.Player=p;_game.Step(default);_game.Step(new(default,NVec.UnitY,false,false,false,false,false,false,false,true));}
            Use(Foundry.Gate);if(!_game.FoundryState.GateOpen)throw new Exception("Foundry gate failed");await Capture("foundry-open-gate");
            var path=ConnectedWorld.Route(RoomLinks.All.Last());_game.Player=path[0]-_game.WorldOrigin;
            foreach(var target in path.Skip(1))
            {
                int ticks=0;while(NVec.Distance(_game.Player+_game.WorldOrigin,target)>5&&ticks++<3000)
                {var move=NVec.Normalize(target-_game.Player-_game.WorldOrigin);_game.Step(new(move,move,false,false,false,false,false,false,false,false));foreach(var cue in _game.Events.Where(e=>e.Kind=="world-frame"))HandleCue(cue);}
                if(ticks>=3000)throw new Exception("Foundry entrance blocked");if(target==path[1])await Capture("foundry-threshold");
            }
            if(!_game.InFoundry)throw new Exception("Wrong foundry destination");_game.Player=new(680,690);_radio="foundry-bailiff";_radioTime=20;await Capture("foundry-bailiff-radio");_radio="";
            var boss=_game.Enemies.Single(e=>e.Kind==EnemyKind.CrownBailiff);bool warning=false,cooling=false;int frames=0;
            while(!boss.Dead&&!_game.Dead&&frames++<18000)
            {
                _game.Step(FoundryPilot.Decide(_game));foreach(var cue in _game.Events.Where(e=>e.Kind is "world-frame" or "room-sound"))HandleCue(cue);
                if(!warning&&boss.State==1){warning=true;await Capture("foundry-hammer-warning");}
                if(!cooling&&_game.FoundryState.Cooling>0){cooling=true;await Capture("foundry-cooling");}
            }
            if(_game.Dead||!boss.Dead||_game.DeveloperSurvival)throw new Exception("Native boss fight failed");await Capture("foundry-bailiff-defeated");
            for(int i=0;i<70;i++)_game.Step(default);Use(Foundry.Plate);if(!_game.FoundryState.PlateTaken)throw new Exception("Foundry plate failed");_radio="foundry-plate";_radioTime=20;await Capture("foundry-crown-imprint");
            foreach(var(id,line) in JourneyDialogue.Foundry())if(!_sound.HasClip("voice-"+id)||_sound.ClipDuration("voice-"+id)<2||Radio[id].Text!=line[1])throw new Exception("Foundry radio missing "+id);
            _game.ValidateRooms();_sound.SetProcess(false);foreach(var p in _sound.GetChildren().OfType<AudioStreamPlayer>()){p.Stop();p.Stream=null;}
            await ToSignal(GetTree().CreateTimer(.2),SceneTreeTimer.SignalName.Timeout);GD.Print($"FOUNDRY CHECK PASS: native combat, {_game.Health} hp, {_game.FoundryState.Quenches} quenches, gate, portrait, eight voices, crown imprint");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
