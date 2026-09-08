using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunGaitChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewFoundryPreview(Order.Medicine);_game.Rooms!.Current=Foundry.Room;_game.FoundryState.GateOpen=true;_game.Rooms.Rooms[Foundry.Room].Visited=true;
            _game.Enemies.Clear();_game.Spawn(EnemyKind.CrownBailiff,new(900,650));var boss=_game.Enemies.Single();boss.Alerted=true;
            ChangeScreen(Screen.Game);SetPhysicsProcess(false);_radio="";_radioQueue.Clear();_bannerTime=_noticeTime=_revealTime=0;
            var directions=new NVec[]{new(1,1),new(1,-1),new(-1,-1),new(-1,1)};
            var path=ProjectSettings.GlobalizePath("res://artifacts/gait/");System.IO.Directory.CreateDirectory(path);
            for(int direction=0;direction<4;direction++)for(int frame=0;frame<24;frame++)
            {
                float distance=frame*4;var facing=NVec.Normalize(directions[direction]);
                _game.Player=new NVec(600,640)+facing*(distance-48);_game.Moving=true;_game.MoveDirection=_game.Facing=facing;_game.Walk=distance*7/195;
                boss.Position=new NVec(860,650)+facing*(distance-48);boss.Moving=true;boss.MoveDirection=boss.Facing=facing;boss.State=0;boss.Walk=_game.Walk;
                _camera=new Vector2(735,570);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var picture=GetViewport().GetTexture().GetImage();if(picture.SavePng(path+$"{direction*24+frame:000}.png")!=Error.Ok)throw new Exception("Gait capture failed");
            }
            GD.Print("GAIT CHECK PASS: 96 native frames, Karl and bailiff, four directions");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
