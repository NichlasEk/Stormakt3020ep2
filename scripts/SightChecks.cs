using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _fogChecks;
    private async void RunSightChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
            async Task<byte[]> Capture(string name)
            {
                _game.UpdateRoomSight(true);_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();
                _bannerTime=_noticeTime=_campaignTextTime=0;_particles.Clear();_floating.Clear();
                QueueRedraw();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();var path=ProjectSettings.GlobalizePath($"res://artifacts/{name}.png");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                Check(image.SavePng(path)==Error.Ok,"Fog screenshot saves");return image.GetData();
            }
            await Capture("fog-first-arrival");
            Check(!_game.ExploredRoomPoint(PortRooms.CourtDoor),"Unknown doorway is not mapped");
            _game.Enemies.Clear();_game.Rooms!.Current=PortRooms.Lodge;_game.Rooms.KeyTaken=_game.Rooms.DoorOpen=true;
            _game.Rooms.Rooms[PortRooms.Lodge].Visited=true;_game.Player=new(650,580);
            var baseline=await Capture("fog-crates-empty");
            _game.Spawn(EnemyKind.Collector,new(925,580));_game.DropItem("norn",new(925,580));
            Check(!_game.CanSeeRoomPoint(_game.Enemies.Single().Position),"Crates hide the boss");
            var hidden=await Capture("fog-crates-hidden");
            Check(baseline.SequenceEqual(hidden),"Unseen boss, loot, health bars and HUD must not change the rendered frame");
            _game.Player=new(970,660);await Capture("fog-discovered");
            Check(_game.CanSeeRoomPoint(_game.Enemies.Single().Position),"Walking around the crates reveals boss and loot");
            _game.Player=new(520,700);await Capture("fog-remembered");
            Check(_game.ExploredRoomPoint(new(925,580))&&!_game.CanSeeRoomPoint(new(925,580)),"Memory does not grant current sight");
            var path=ProjectSettings.GlobalizePath("res://artifacts/fog-save.json");SaveStore.Write(path,_game);_game=SaveStore.Read(path);
            await Capture("fog-reloaded");
            Check(_game.ExploredRoomPoint(new(925,580)),"Reload retains explored ground");
            GD.Print("FOG CHECK PASS: first discovery, exact hidden-frame equivalence, obstacle sight, remembered ground, save reload");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
