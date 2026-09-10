using Godot;
using Atland;
using System;
using System.Text.Json;
public partial class Main
{
    private async void RunMusicChecks()
    {
        try
        {
            var before=JsonSerializer.Serialize(_game,new JsonSerializerOptions{IncludeFields=true});SetPhysicsProcess(false);
            _sound.SelectScore("prologue-quay");_sound._Process(3);
            _sound.SelectScore("prologue-shore");_sound._Process(.2);
            if(_sound.PlayingScores!=2)throw new Exception("Gameplay crossfade setup failed");
            Activate("music-library");_sound._Process(.01);
            if(_sound.PlayingScores!=0||_previewScore!="")throw new Exception("Background music leaked into library");
            if(_sound.ScoreCount!=MusicTracks.Length)throw new Exception("Music catalog incomplete");
            foreach(var t in MusicTracks)
            {
                Activate("music:"+t.id);if(_previewScore!=t.id)throw new Exception("Library selection failed");_sound.SelectScore(t.id);_sound._Process(.2);
                if(_sound.CurrentScore!=t.id||_sound.PlayingScores!=1)throw new Exception("Preview overlap: "+t.id);
                _sound._Process(3);_sound._Process(3);
                for(int i=0;i<3;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                if(_sound.CurrentScore!=t.id||_sound.PlayingScores!=1)throw new Exception("Track did not play: "+t.id);
            }
            float unvoiced=_sound.ScorePeakDb;_sound.PauseVoice(false);_sound.Speak("arrival");_sound._Process(3);if(_sound.Speaking&&_sound.ScorePeakDb>unvoiced-5)throw new Exception("Dialogue ducking failed");_sound.StopVoice();
            float volume=_sound.Volume;_sound.Volume=0;_sound._Process(3);if(_sound.ScorePeakDb>-79)throw new Exception("Mute failed");_sound.Volume=volume;_sound.Cinematic=true;_sound._Process(3);if(_sound.ScorePeakDb>-79)throw new Exception("Film mute failed");_sound.Cinematic=false;
            _musicPage=5;QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            var path=ProjectSettings.GlobalizePath("res://artifacts/music-library.png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);using(var img=GetViewport().GetTexture().GetImage())img.SavePng(path);
            Back();if(_sound.PlayingScores!=0)throw new Exception("Preview leaked on exit");
            _sound._Process(.1);if(_sound.CurrentScore!="prologue-shore"||_sound.PlayingScores!=1)throw new Exception("Gameplay music not restored");
            if(_screen!=Screen.Settings)throw new Exception("Music back navigation failed");
            if(before!=JsonSerializer.Serialize(_game,new JsonSerializerOptions{IncludeFields=true}))throw new Exception("Library changed campaign");GD.Print($"MUSIC CHECK PASS: {MusicTracks.Length} streams, exclusive previews, silent library entry, gameplay restore, crossfades, mute, cinematic silence, library and unchanged campaign");GetTree().Quit();
        }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
