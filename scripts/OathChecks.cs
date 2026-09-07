using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _oathChecks;
    private async void RunOathChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
            async Task Capture(string name)
            {
                _game.UpdateRoomSight(true);_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();
                _bannerTime=_noticeTime=_campaignTextTime=0;_particles.Clear();_floating.Clear();_radioTime=0;_radioQueue.Clear();_radio="";_sound.StopVoice();QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();var path=ProjectSettings.GlobalizePath($"res://artifacts/{name}.png");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);Check(image.SavePng(path)==Error.Ok,"Save oath screenshot");
            }
            bool gallery=false,warning=false,exposed=false;
            int ticks=0;
            for(;ticks<40000&&!_game.Dead&&!_game.Rooms!.Completed;ticks++)
            {
                _game.Step(OathCheckPilot.Decide(_game,true));
                // Exercise actual native rendering and cue dispatch, without touching the user's save slot.
                foreach(var cue in _game.Events)HandleCue(cue);
                if(_game.Rooms!.Current==PortRooms.Gallery&&_game.Rooms.WitnessRead&&!gallery){gallery=true;await Capture("oath-gallery");}
                var boss=_game.Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.OathGuardian&&!e.Dead);
                if(boss!=null&&boss.State==1&&!warning)
                {
                    // The pilot baits from behind stone. Inspect the actual locked warning
                    // from an unobstructed nearby viewpoint, then restore simulation position.
                    var player=_game.Player;
                    _game.Player=_game.Bound(boss.Position+new System.Numerics.Vector2(-boss.Facing.Y,boss.Facing.X)*140);
                    Check(_game.CanSeeRoomPoint(boss.Position),"Warning inspection has unobstructed sight");
                    await Capture("oath-rush-warning");warning=true;_game.Player=player;_game.UpdateRoomSight(true);
                }
                if(boss!=null&&_game.CanSeeRoomPoint(boss.Position))
                {
                    if(boss.State==3&&!exposed){exposed=true;await Capture("oath-exposed");}
                }
                if(ticks%120==0)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            }
            Check(_game.Rooms!.Completed&&!_game.Dead&&!_game.DeveloperSurvival,"Native six-room route completes without survival mode");
            Check(gallery&&warning&&exposed,"Native renderer exercised clue and both boss states");
            await Capture("oath-port-open");
            var save=ProjectSettings.GlobalizePath("res://artifacts/oath-save.json");SaveStore.Write(save,_game);_game=SaveStore.Read(save);
            Check(_game.Rooms!.Completed&&_game.Rooms.OathDefeated,"Native completion reload");
            using(var key=new InputEventKey{PhysicalKeycode=Key.R,Pressed=true})_Input(key);
            Check(_screen==Screen.Journal,"Journal opens after completion");await Capture("oath-journal");
            if(_roomAudioChecks)await CheckRoomAudioPresentation();
            if(_archiveChecks)await CheckArchiveScenes();
            GD.Print($"OATH CHECK PASS: six rooms, native boss, pillar exposure, reward, saved completion, journal; {ticks} ticks, {_game.Health:0} health");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
