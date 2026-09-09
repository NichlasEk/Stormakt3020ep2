using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunCabinChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new Exception(message);}
            LoadWaterArt();_game=Combat.NewCabinPreview(_order);ChangeScreen(Screen.Game);SetPhysicsProcess(false);
            void Use(NVec at){_game.Player=at;_game.Step(default);_game.Step(new(default,NVec.UnitY,false,false,false,false,false,false,false,true));foreach(var cue in _game.Events.ToArray())HandleCue(cue);}
            async System.Threading.Tasks.Task Capture(string name)
            {
                _bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);using var picture=GetViewport().GetTexture().GetImage();Check(picture.SavePng(path)==Error.Ok,"Cabin capture");
            }
            Use(Uppsala.Ramp);Check(_game.InCabin,"Ordinary ramp leads aboard");await Capture("cabin-arrival");
            Use(Cabin.Talk);_radioQueue.Clear();_radio="cabin-order";_radioTime=20;await Capture("cabin-ebba-order");
            Use(Cabin.Talk);Use(Cabin.Talk);Check(_game.CabinState.Briefed,"Three-part conversation completes");
            Use(Cabin.Rest);Check(_game.Health==100,"Medical rest");_radioQueue.Clear();_radio="cabin-rest";_radioTime=20;await Capture("cabin-bandages");
            _radio="";_sound.StopVoice();_game.Player=new(820,355);Check(_game.OnWalkable(_game.Player),"Depth sample stands on reachable floor");await Capture("cabin-table-depth");
            Use(Cabin.Entry);Check(_game.Rooms!.Current==Uppsala.Court,"Hatch returns to Uppsala");_game.ValidateRooms();
            foreach(var(id,line) in JourneyDialogue.Cabin())Check(Radio[id].Text==line[1]&&_sound.HasClip("voice-"+id)&&_sound.ClipDuration("voice-"+id)>2,"Direct cabin voice "+id);
            using(var sprite=_ebbaCabin!.GetImage()){Check(sprite.GetPixel(0,0).A==0&&sprite.GetPixel(500,700).A>.9,"Chroma removed, costume opaque");}
            Check(_sound.HasClip("cabin-hum"),"Quiet ship ambience loaded");
            GD.Print("CABIN CHECK PASS: physical Ebba, eight direct voices, three-step debrief, medical rest, hatch, depth and transparent sprite");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
