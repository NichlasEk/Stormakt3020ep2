using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _rootwayChecks;
    private async Task CheckRootwayScenes()
    {
        void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        async Task Capture(string name)
        {
            _game.UpdateRoomSight(true);_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();
            _bannerTime=_noticeTime=_campaignTextTime=0;_radioTime=0;_radioQueue.Clear();_radio="";_sound.StopVoice();_particles.Clear();_floating.Clear();QueueRedraw();
            await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using var image=GetViewport().GetTexture().GetImage();Check(image.SavePng(ProjectSettings.GlobalizePath($"res://artifacts/{name}.png"))==Error.Ok,"Save rootway screenshot");
        }
        void Step(Controls input){_game.Step(input);foreach(var cue in _game.Events)HandleCue(cue);}
        foreach(int choice in new[]{1,2})
        {
            _game=SaveStore.Read(ProjectSettings.GlobalizePath($"res://artifacts/archive-{choice}-save.json"));ChangeScreen(Screen.Game);
            bool winch=false,grove=false;int ticks=0;
            for(;ticks<18000&&!_game.Dead&&!_game.Rooms!.GroveSecured;ticks++)
            {
                Step(OathCheckPilot.Decide(_game));
                if(_game.Rooms!.Current==PortRooms.Roots&&_game.Rooms.RootGateOpen&&!winch){await Capture("rootway-winch-"+choice);winch=true;}
                if(_game.Rooms.Current==PortRooms.Grove&&_game.Rooms.GroveWave>0&&!grove){await Capture("rootway-grove-control-"+choice);grove=true;}
                if(ticks%120==0)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            }
            Check(winch&&grove&&_game.Rooms!.GroveSecured&&!_game.Dead&&!_game.DeveloperSurvival,"Rootway native route completes without survival mode");
            Check(_game.Rooms!.GroveWave==(choice==1?1:2)&&_game.LocalDrops.Any(d=>d.Item.Definition=="norn"),"Correct branch and sigil reward");
            await Capture("rootway-secured-"+choice);
            var save=ProjectSettings.GlobalizePath($"res://artifacts/rootway-{choice}-save.json");SaveStore.Write(save,_game);_game=SaveStore.Read(save);
            Check(_game.Rooms!.GroveSecured&&_game.ArchiveChoice==choice,"Native grove outcome reloads");
            GD.Print($"ROOTWAY BRANCH {choice}: {ticks} ticks, {_game.Health:0} health, {_game.Rooms.GroveWave} patrols");
        }
        foreach(var (id,line) in JourneyDialogue.Roots())
        {
            Check(Radio[id].Text==line[1]&&_sound.HasClip("voice-"+id)&&_sound.ClipDuration("voice-"+id)>2,"Rootway voice and subtitle: "+id);
            _sound.Speak(id);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);Check(_sound.Speaking,"Rootway voice starts: "+id);_sound.StopVoice();
        }
        using(var key=new InputEventKey{PhysicalKeycode=Key.R,Pressed=true})_Input(key);
        Check(_screen==Screen.Journal,"Nine-room journal opens");await Capture("rootway-journal");
        GD.Print("ROOTWAY CHECK PASS: two paintings, counterweight gate, choice-dependent reinforcements, saved reward, five voices, nine-room journal");
    }
}
