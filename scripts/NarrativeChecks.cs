using Godot;
using Atland;
using System;
using System.Linq;
using System.Text.Json;
public partial class Main
{
    private async void RunNarrativeChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new Exception(message);}
            async System.Threading.Tasks.Task Frames(int n){for(int i=0;i<n;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);}
            async System.Threading.Tasks.Task Capture(string name)
            {
                _bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                using var picture=GetViewport().GetTexture().GetImage();Check(picture.SavePng(path)==Error.Ok,"Narrative capture");
            }
            LoadWaterArt();_game=Combat.NewUppsalaPreview(Order.Medicine);_game.Rooms!.Current=Regiment.Parade;_game.Player=Regiment.Discharge;ChangeScreen(Screen.Game);SetPhysicsProcess(false);
            foreach(var(id,line) in JourneyDialogue.Continuity())Check(_sound.HasClip("voice-"+id)&&_sound.ClipDuration("voice-"+id)>2&&Radio[id].Text==line[1],"Narrative voice and card "+id);
            foreach(var id in new[]{"continuity-dead","continuity-rest","continuity-silence","continuity-plate"}){_radio=id;_radioTime=20;await Capture(id);}
            _radioTime=0;_radio="";QueueFarewell();Check(_pendingStoryFilm=="regiment-rest","Old completed save queues farewell");
            int frames=0;while(_screen==Screen.Game&&frames++<6000)await Frames(1);
            Check(_screen==Screen.Cinematic&&_activeFilm?.Id=="regiment-rest"&&_game.RegimentState.FarewellSeen,"Farewell voices lead to registered film and persistent state");
            await Frames(90);Check(_filmPlayer!.IsPlaying()&&_filmPlayer.StreamPosition>0,"Farewell Theora decoder advances");await Capture("farewell-film");
            frames=0;while(_screen==Screen.Cinematic&&frames++<1200)await Frames(1);
            Check(_screen==Screen.Game&&_filmElapsed<12&&!_sound.Cinematic,"Farewell ends naturally and restores sound");
            Check(_radio=="continuity-silence"||_radioQueue.Contains("continuity-silence"),"Ebba aftermath queued after film");
            Check(_radioQueue.Contains("continuity-names"),"Hedvig names follow silence");
            Check(RegimentVisibility==0&&_sound.Remembrance,"Discharged regiment remains absent and music gives way to ambience");await Capture("farewell-empty");QueueFarewell();Check(_pendingStoryFilm=="","Seen farewell is not restarted");
            _radioQueue.Clear();_radio="";_radioTime=0;_sound.StopVoice();ChangeScreen(Screen.Videos);await Capture("farewell-library");
            Check(_buttons.Any(b=>b.Id=="film:regiment-rest"),"Farewell appears in video library");
            string State()=>JsonSerializer.Serialize(_game,new JsonSerializerOptions{IncludeFields=true});var state=State();StartFilm("regiment-rest",true);await Frames(15);
            using(var key=new InputEventKey{PhysicalKeycode=Key.Escape,Pressed=true})_Input(key);
            Check(_screen==Screen.Videos&&State()==state&&_radioQueue.Count==0,"Library skip preserves save state and does not add story dialogue");
            ChangeScreen(Screen.Game);StartFilm("regiment-rest");await Frames(10);FinishGateFilm();Check(_screen==Screen.Game&&_radioQueue.Contains("continuity-names"),"Story skip retains aftermath");
            GD.Print("NARRATIVE CHECK PASS: nine voiced cards, resumed farewell, natural video end, empty regiment, library preview, skip and aftermath");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
