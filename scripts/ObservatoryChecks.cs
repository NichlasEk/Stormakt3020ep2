using Godot;
using Atland;
using System;
using System.Linq;
public partial class Main
{
    private async void RunObservatoryChecks()
    {
        try
        {
            System.IO.Directory.CreateDirectory(ProjectSettings.GlobalizePath("res://artifacts"));LoadWaterArt();_game=Combat.NewObservatoryPreview(_order);ChangeScreen(Screen.Game);SetPhysicsProcess(false);
            foreach(var room in Observatory.Ids)
            {
                _game.ObservatoryState.EntryOpen=_game.ObservatoryState.MartaMet=_game.ObservatoryState.DiagramRead=true;
                _game.ObservatoryState.Aligned=true;_game.ObservatoryState.Brakes=(int[])Observatory.Target.Clone();
                typeof(Combat).GetMethod("EnterConnectedRoom",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(_game,new object[]{room});_game.Player=room==Observatory.Quarters?new(825,440):room==Observatory.Machine?new(760,490):room==Observatory.Workshop?new(905,355):new(780,620);_game.UpdateRoomSight(true);if(room==Observatory.Quarters&&!_game.CanSeeRoomPoint(Observatory.Talk))throw new Exception("Marta must be visible from her conversation point");
                _radio=room==Observatory.Quarters?"observatory-marta":"";_radioTime=20;_bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-90);RememberRenderPositions();QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();image.SavePng(ProjectSettings.GlobalizePath("res://artifacts/"+room+".png"));
                if(room==Observatory.Workshop)
                {
                    if(RoomBackground==_workshopEmpty)throw new Exception("Unopened cabinet painted empty");
                    _game.ObservatoryState.CabinetOpened=_game.ObservatoryState.KeyTaken=true;
                    if(RoomBackground!=_workshopEmpty)throw new Exception("Empty cabinet still painted closed");
                    QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                    using var opened=GetViewport().GetTexture().GetImage();opened.SavePng(ProjectSettings.GlobalizePath("res://artifacts/observatory-cabinet-empty.png"));
                }
            }
            foreach(var(id,line) in JourneyDialogue.Observatory())if(Radio[id].Text!=line[1]||!_sound.HasClip("voice-"+id))throw new Exception("Missing observatory voice "+id);
            _game.ValidateRooms();
            var state=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});
            StartFilm("observatory-dome");
            for(int i=0;i<90&&(_filmPlayer is null||_filmPlayer.StreamPosition<=0);i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            if(_filmPlayer is null||!_filmPlayer.IsPlaying()||_filmPlayer.StreamPosition<=0)throw new Exception("Observatory movie did not decode");
            FinishGateFilm();
            if(state!=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true}))throw new Exception("Movie changed campaign progress");
            GD.Print("OBSERVATORY CHECK PASS: four rooms, painted actors and eighteen voices");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
