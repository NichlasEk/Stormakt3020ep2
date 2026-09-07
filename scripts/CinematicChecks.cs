using Godot;
using Atland;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

public partial class Main
{
    private async Task CheckCinematicScenes()
    {
        void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        async Task Capture(string name)
        {
            QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using var image=GetViewport().GetTexture().GetImage();Check(image.SavePng(ProjectSettings.GlobalizePath($"res://artifacts/{name}.png"))==Error.Ok,"Cinematic screenshot");
        }
        async Task Frames(int count){for(int i=0;i<count;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
        ChangeScreen(Screen.Pause);Activate("settings");await Capture("cinematic-settings");Activate("videos");
        Check(_screen==Screen.Videos,"Settings developer video menu opens");await Capture("cinematic-library");
        string State()=>JsonSerializer.Serialize(_game,new JsonSerializerOptions{IncludeFields=true});string state=State();
        Activate("gate-film");await Frames(90);
        Check(_screen==Screen.Cinematic&&_filmPlayer!.IsPlaying()&&_filmPlayer.StreamPosition>0,"Native Theora decoder advances");
        Check(_filmPlayer!.GetVideoTexture().GetWidth()==768,"Decoded film texture has expected width");
        Check(_sound.Cinematic&&_filmPlayer.Volume==0,"Film uses test mute and suppresses game mix");
        float volume=_volume;bool test=_testMode;_testMode=false;_volume=.25f;StepFilm(0);
        Check(Math.Abs(_filmPlayer.Volume-.25f)<.001f,"Film follows master volume");
        _volume=0;StepFilm(0);Check(_filmPlayer.Volume==0,"Master mute silences film");
        _volume=volume;_testMode=test;StepFilm(0);
        // Physics must remain frozen even outside the test driver's simulation bypass.
        bool checks=_sceneChecks;_sceneChecks=false;_PhysicsProcess(1.0/60);_sceneChecks=checks;
        Check(state==State(),"Video preview leaves combat, inventory, exploration and progress untouched");await Capture("cinematic-playing");
        using(var key=new InputEventKey{PhysicalKeycode=Key.Escape,Pressed=true})_Input(key);
        Check(_screen==Screen.Videos&&!_sound.Cinematic&&!_filmPlayer.IsPlaying(),"Escape returns to library and stops sound/video");
        Activate("gate-film");await Frames(10);
        using(var key=new InputEventJoypadButton{ButtonIndex=JoyButton.B,Pressed=true})_Input(key);
        Check(_screen==Screen.Videos,"Controller B skips back to library");
        Activate("gate-film");
        for(int frames=0;frames<1000&&_screen==Screen.Cinematic;frames++)await Frames(1);
        Check(_screen==Screen.Videos&&_filmElapsed<15&&!_sound.Cinematic,"Natural film end returns before watchdog");
        Check(state==State(),"Repeated playback does not write game progress");
        Back();Check(_screen==Screen.Settings,"Library back returns to settings");Back();Check(_screen==Screen.Pause,"Settings retains its original pause return");
        // Exercise the same cue handler as a real first room passage.
        ChangeScreen(Screen.Game);_sceneChecks=false;HandleCue(new Cue("cinematic",_game.Player,"archive-gate"));_sceneChecks=checks;
        Check(_screen==Screen.Cinematic,"Room cue starts the film");FinishGateFilm();Check(_screen==Screen.Game,"Story playback returns to gameplay");
        Check(Combat.NewRooms(Order.Artillery).Events.Count(e=>e.Kind=="cinematic"&&e.Text=="atland-intro")==1,"New room expedition has one intro event");
        Check(Combat.NewAtland(Order.Artillery).Events.Count(e=>e.Kind=="cinematic"&&e.Text=="atland-intro")==1,"New Atland campaign has one intro event");
        ChangeScreen(Screen.Pause);Activate("settings");Activate("videos");await Capture("intro-library");
        Check(_buttons.Any(b=>b.Id=="film:atland-intro")&&_buttons.Any(b=>b.Id=="film:archive-gate"),"Both films have selectable catalog buttons");
        var intro=Films.Single(f=>f.Id=="atland-intro");
        var lines=JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/intro-radio.json"))!;
        Check(intro.Captions.Length==3&&intro.Captions.Select(c=>c.Text).SequenceEqual(new[]{"ship","chart","coast"}.Select(k=>lines["intro-"+k][1])),"Intro subtitles match the generated Ebba script");
        Check(intro.CaptionAt(6)==null&&intro.CaptionAt(10)?.Text==lines["intro-chart"][1],"Caption gaps and chart timing");
        state=State();Activate("film:atland-intro");await Frames(60);
        using(var key=new InputEventKey{PhysicalKeycode=Key.Escape,Pressed=true})_Input(key);
        Check(_screen==Screen.Videos&&State()==state,"Intro skip is nondestructive");
        Activate("film:atland-intro");int captured=0;
        for(int frames=0;frames<3000&&_screen==Screen.Cinematic;frames++)
        {
            await Frames(1);double position=_filmPlayer!.StreamPosition;
            if(captured<3&&position>=new[]{2.0,10.0,18.0}[captured])
            {
                Check(_activeFilm==intro&&intro.CaptionAt(position)!=null,"Caption follows video clock on shot "+captured);
                await Capture("intro-shot-"+(++captured));
            }
        }
        Check(captured==3&&_screen==Screen.Videos&&_filmElapsed>22&&_filmElapsed<32,"Complete 24-second intro reaches natural end, not the old 20-second watchdog");
        Check(State()==state&&!_sound.Cinematic,"Full intro restores library and game state");
        Activate("film:archive-gate");await Frames(15);Check(_activeFilm?.Id=="archive-gate"&&_filmPlayer!.IsPlaying(),"Switch back to cached gate movie");FinishGateFilm();
        GD.Print("INTRO CHECK PASS: two-film catalog, three Ebba captions, frozen game state, skip/replay, 24-second natural end, switch back to gate");
        GD.Print("CINEMATIC CHECK PASS: decoded video, paused simulation, muted mix, settings library, replay without state changes, Escape/B skip, natural end, story cue return");
    }
}
