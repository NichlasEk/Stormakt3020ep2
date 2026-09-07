using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _archiveChecks;
    private async Task CheckArchiveScenes()
    {
        void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        async Task Capture(string name)
        {
            _game.UpdateRoomSight(true);_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_bannerTime=_noticeTime=_campaignTextTime=0;
            _radioTime=0;_radioQueue.Clear();_radio="";_sound.StopVoice();_particles.Clear();_floating.Clear();QueueRedraw();
            await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using var image=GetViewport().GetTexture().GetImage();
            Check(image.SavePng(ProjectSettings.GlobalizePath($"res://artifacts/{name}.png"))==Error.Ok,"Save archive screenshot");
        }
        void Step(Controls input){_game.Step(input);foreach(var cue in _game.Events)HandleCue(cue);}
        foreach(int choice in new[]{1,2})
        {
            _game=SaveStore.Read(ProjectSettings.GlobalizePath("res://artifacts/oath-save.json"));ChangeScreen(Screen.Game);
            int ticks=0;
            for(;ticks<12000&&!_game.Dead&&!_game.Rooms!.ArchiveRead;ticks++)
            {Step(OathCheckPilot.Decide(_game));if(ticks%120==0)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
            Check(_game.Rooms!.Current==PortRooms.Archive&&_game.Rooms.ArchiveRead&&_screen==Screen.Archive,"Real E opens the document comparison");
            await Capture("archive-documents-"+choice);
            Check(_buttons.Any(b=>b.Id=="archive-preserve")&&_buttons.Any(b=>b.Id=="archive-forge"),"Both decisions have actual menu buttons");
            long tick=_game.Tick;_PhysicsProcess(1.0/60);Check(_game.Tick==tick,"Reading pauses simulation");
            using(var esc=new InputEventKey{PhysicalKeycode=Key.Escape,Pressed=true})_Input(esc);
            Check(_screen==Screen.Game&&_game.ArchiveChoice==0,"Escape closes comparison without choosing");
            await Capture("archive-reading-table");
            Step(default);Step(new(default,System.Numerics.Vector2.UnitY,false,false,false,false,false,false,false,true));
            Check(_screen==Screen.Archive,"Desk can be reopened");
            Activate(choice==1?"archive-preserve":"archive-forge");
            Check(_screen==Screen.Game&&_game.ArchiveChoice==choice&&_game.Enemies.Count==(choice==1?3:1),"UI decision creates the correct control patrol");
            for(int fight=0;fight<12000&&!_game.Dead&&!_game.Rooms!.ArchiveSecured;fight++)
            {Step(OathCheckPilot.Decide(_game));if(fight%120==0)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
            Check(!_game.Dead&&_game.Rooms!.ArchiveSecured,"Native archive branch completes without survival mode");
            await Capture("archive-secured-"+choice);
            var save=ProjectSettings.GlobalizePath($"res://artifacts/archive-{choice}-save.json");SaveStore.Write(save,_game);_game=SaveStore.Read(save);
            Check(_game.ArchiveChoice==choice&&_game.Rooms!.ArchiveSecured,"Archive result reload");
        }
        foreach(var (id,line) in JourneyDialogue.Archive())
        {
            Check(Radio[id].Text==line[1]&&_sound.HasClip("voice-"+id)&&_sound.ClipDuration("voice-"+id)>2,"Archive voice and subtitle: "+id);
            _sound.Speak(id);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);Check(_sound.Speaking,"Archive voice starts: "+id);_sound.StopVoice();
        }
        using(var journal=new InputEventKey{PhysicalKeycode=Key.R,Pressed=true})_Input(journal);
        Check(_screen==Screen.Journal,"Archive journal opens");await Capture("archive-journal");
        GD.Print("ARCHIVE CHECK PASS: seventh room, painted document UI, Escape/reopen, both decisions and real patrols, saved results, four voices, seven-room journal");
    }
}
