using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _waterChecks;
    private async void RunWaterChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
            async Task Capture(string name,System.Numerics.Vector2 at)
            {
                _game.Player=at;_game.UpdateRoomSight(true);_camera=G(at)+new Vector2(0,-60);RememberRenderPositions();
                _bannerTime=_noticeTime=_campaignTextTime=0;_particles.Clear();_floating.Clear();QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var image=GetViewport().GetTexture().GetImage();var path=ProjectSettings.GlobalizePath($"res://artifacts/{name}.png");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);Check(image.SavePng(path)==Error.Ok,"Save water screenshot");
            }
            void Use(System.Numerics.Vector2 at)
            {
                _game.Player=at;_game.Step(default);_game.Step(new(default,System.Numerics.Vector2.UnitY,false,false,false,false,false,false,false,true));
                foreach(var cue in _game.Events)HandleCue(cue);
            }
            foreach(var e in _game.Enemies)e.Health=0;
            Use(PortRooms.Key);Use(PortRooms.CourtDoor);Use(PortRooms.CourtDoor);
            foreach(var e in _game.Enemies)e.Health=0;
            Use(PortRooms.Cache);Use(RoomLinks.All[1].AtA);Check(_game.Rooms!.Current==PortRooms.Pump,"Enter painted pump room");
            await Capture("water-pump-entry",_game.Player);
            foreach(var e in _game.Enemies)e.Health=0;
            await Capture("water-pump-high",new(760,710));
            Use(PortRooms.Wheel);Check(!_game.Rooms.WaterLowered,"Pressure lock remains");
            Use(PortRooms.Pressure);Use(PortRooms.Wheel);Check(_game.Rooms.WaterLowered,"Pump drains");
            await Capture("water-pump-low",new(760,710));
            Use(RoomLinks.All[2].AtA);Check(_game.Rooms.Current==PortRooms.Cistern,"Reach cistern");
            await Capture("water-cistern-entry",_game.Player);
            Use(PortRooms.Relic);await Capture("water-cistern-relic",_game.Player);
            Use(RoomLinks.All[3].AtA);Check(_game.Rooms.ShortcutOpen,"Unbar shortcut");
            await Capture("water-cistern-shortcut",RoomLinks.All[3].ArrivalA);
            Use(RoomLinks.All[3].AtA);Check(_game.Rooms.Current==PortRooms.Court,"Shortcut returns to courtyard");
            await Capture("water-loop-complete",_game.Player);
            var save=ProjectSettings.GlobalizePath("res://artifacts/water-save.json");SaveStore.Write(save,_game);_game=SaveStore.Read(save);
            Use(RoomLinks.All[3].AtB);Check(_game.Rooms!.Current==PortRooms.Cistern&&_game.Rooms.RelicTaken&&_game.LocalDrops.Any(),"Saved reverse shortcut preserves relic");
            using(var key=new InputEventKey{PhysicalKeycode=Key.R,Pressed=true})_Input(key);
            Check(_screen==Screen.Journal,"R opens the updated room journal");await Capture("water-journal",_game.Player);
            GD.Print("WATER CHECK PASS: four rooms, pressure lock, changed water art, optional relic, shortcut, save/revisit");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
