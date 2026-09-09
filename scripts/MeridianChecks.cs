using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunMeridianChecks()
    {
        try
        {
            void Check(bool ok,string message){if(!ok)throw new Exception(message);}
            StartMeridian();Check(SavePath.EndsWith("meridian-preview-save.json")&&ManualPath.EndsWith("meridian-preview-manual-save.json")&&!_uppsalaSlot,"Dedicated Meridian preview slot");SetPhysicsProcess(false);_radioQueue.Clear();_radio="";_sound.StopVoice();
            async System.Threading.Tasks.Task Capture(string name)
            {
                _bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);using var picture=GetViewport().GetTexture().GetImage();Check(picture.SavePng(path)==Error.Ok,"Meridian capture");
            }
            void Use(NVec at){_game.Player=at;_game.Step(default);_game.Step(new(default,NVec.UnitY,false,false,false,false,false,false,false,true));}
            async System.Threading.Tasks.Task Walk(string id,bool back=false)
            {
                var l=RoomLinks.All.Single(l=>l.Id==id);var route=ConnectedWorld.Route(l);if(back)Array.Reverse(route);_game.Player=route[0]-_game.WorldOrigin;
                foreach(var target in route.Skip(1))
                {
                    int ticks=0;while(NVec.Distance(_game.Player+_game.WorldOrigin,target)>5&&ticks++<3000){var dir=NVec.Normalize(target-_game.Player-_game.WorldOrigin);_game.Step(new(dir,dir,false,false,false,false,false,false,false,false));}
                    Check(ticks<3000,"Native passage "+id);await Capture("meridian-passage-"+id+"-"+Array.IndexOf(route,target)+(back?"-back":""));
                }
                Check(_game.Rooms!.Current==(back?l.A:l.B),"Native arrival "+id);
            }
            _game.Inventory.Drops.Clear();Use(Meridian.CourtGate);await Capture("meridian-court-open");await Walk("uppsala-clock");
            Use(Meridian.Ledger);_game.Player=new(1330,550);await Capture("meridian-clock-locked");Use(Meridian.Bell);for(int i=0;i<300;i++)_game.Step(default);_game.Player=new(700,650);_radio="meridian-repeat";_radioTime=20;await Capture("meridian-clock-echo");
            for(int i=0;i<1180;i++)_game.Step(default);Check(_game.MeridianState.Cycles>=2,"Native repeated morning");Use(Meridian.Bell);await Walk("meridian-hall");
            _game.Player=new(800,570);_radio="meridian-warden";_radioTime=20;await Capture("meridian-first-meeting");Use(Meridian.Plate);
            var boss=_game.Enemies.Single(e=>e.Kind==EnemyKind.MeridianWarden);int ticks=0;bool warned=false,exposed=false;
            while(!_game.Dead&&!boss.Dead&&ticks++<24000)
            {
                _game.Step(MeridianPilot.Decide(_game));
                if(!warned&&boss.State==1){warned=true;await Capture("meridian-metric-strike");}
                if(!exposed&&_game.MeridianState.Exposed>0){exposed=true;await Capture("meridian-instrument-break");}
            }
            Check(!_game.Dead&&boss.Retired&&_game.MeridianState.Breaks>=3,"Native instrument boss");
            for(int i=0;i<180;i++)_game.Step(default);Use(Meridian.Order);_radio="meridian-order";_radioTime=20;await Capture("meridian-order-found");
            Check(_game.MeridianState.OrderTaken,"Native persistent endpoint");await Walk("meridian-hall",true);await Walk("uppsala-clock",true);
            _game.Player=Uppsala.Ramp;Check(_game.FinishShipTravel(Regiment.Quay),"Native return to ship");_game.ValidateRooms();
            foreach(var(id,line) in JourneyDialogue.Meridian())Check(Radio[id].Text==line[1]&&_sound.HasClip("voice-"+id)&&_sound.ClipDuration("voice-"+id)>2,"Meridian voice "+id);
            _radioQueue.Clear();QueueRadio("meridian-repeat");QueueRadio("meridian-repeat");Check(_radioQueue.Count==2,"Identical recording repeats instead of being deduplicated");
            _radioQueue.Clear();QueueRadio("meridian-tactic");QueueRadio("meridian-fight");QueueRadio("meridian-fallen");Check(_radioQueue.SequenceEqual(new[]{"meridian-fallen"}),"Obsolete battle dialogue removed on defeat");
            Check(_sound.HasClip("meridian-bell")&&_sound.HasClip("meridian-turn"),"Clock sounds loaded");
            ChangeScreen(Screen.Journal);await Capture("meridian-route-map");
            GD.Print("MERIDIAN CHECK PASS: painted passages both directions, repeated guard, sixteen voices, portrait, three instruments, nonlethal boss and return");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
