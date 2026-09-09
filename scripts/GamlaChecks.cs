using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunGamlaChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewGamlaPreview(_order);_game.FinishGamlaFlight(Gamla.Landing);ChangeScreen(Screen.Game);SetPhysicsProcess(false);
            void Cues(){foreach(var cue in _game.Events.ToArray())HandleCue(cue);}
            async System.Threading.Tasks.Task Capture(string name)
            {
                _radio="";_radioQueue.Clear();_sound.StopVoice();_bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);using var image=GetViewport().GetTexture().GetImage();if(image.SavePng(path)!=Error.Ok)throw new Exception("Capture failed");
            }
            await Capture("gamla-landing");
            foreach(var id in new[]{"gamla-mound","gamla-registry"})
            {
                foreach(var e in _game.Enemies.Where(e=>e.HomeRoom==_game.Rooms!.Current))e.Health=0;
                _game.GamlaState.LedgerRead=id=="gamla-registry";
                var link=RoomLinks.All.Single(l=>l.Id==id);_game.Player=link.ArrivalA;for(int i=0;i<90;i++)_game.Step(default);
                _game.Player=link.AtA;await Capture(id+"-approach");var target=ConnectedWorld.Route(link)[1]-_game.WorldOrigin;
                int ticks=0;while(_game.Passage==null&&ticks++<180){var d=NVec.Normalize(target-_game.Player);_game.Step(new(d,d,false,false,false,false,false,false,false,false));Cues();}
                if(_game.Passage==null)throw new Exception("No painted passage: "+id);
                for(int i=0;i<25;i++){_game.Step(default);Cues();}await Capture(id+"-receding");
                while(_game.Passage!=null){_game.Step(default);Cues();}await Capture(id+"-exit");
            }
            _game.Player=Gamla.Talk;await Capture("gamla-nils");
            foreach(var(id,line) in JourneyDialogue.Gamla())if(Radio[id].Text!=line[1]||!_sound.HasClip("voice-"+id))throw new Exception("Missing Gamla line "+id);
            _game.ValidateRooms();
            var state=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});
            StartFilm("gamla-arrival");for(int i=0;i<90&&(_filmPlayer is null||_filmPlayer.StreamPosition<=0);i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            if(_filmPlayer is null||!_filmPlayer.IsPlaying()||_filmPlayer.StreamPosition<=0)throw new Exception("Arrival movie did not decode");FinishGateFilm();
            if(state!=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true}))throw new Exception("Movie altered campaign");
            GD.Print("GAMLA CHECK PASS: three paintings, receding doorway walks, covered scene transfers, Nils and thirteen voices");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
