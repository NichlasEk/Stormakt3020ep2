using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunSaltChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewSaltPreview(_order);ChangeScreen(Screen.Game);SetPhysicsProcess(false);
            var enter=typeof(Combat).GetMethod("EnterConnectedRoom",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            void Enter(string id,NVec at){enter.Invoke(_game,new object[]{id});_game.Player=at;}
            void Use(NVec p){_game.Player=p;_game.Step(default);_game.Step(new(default,NVec.UnitY,false,false,false,false,false,false,false,true));}
            async System.Threading.Tasks.Task Capture(string name)
            {
                _radio="";_radioQueue.Clear();_sound.StopVoice();_bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);using var img=GetViewport().GetTexture().GetImage();if(img.SavePng(path)!=Error.Ok)throw new Exception("Capture failed");
            }
            Use(Cabin.Talk);if(!_game.SaltState.Reunited)throw new Exception("Reunion missing");await Capture("salt-reunion");
            Enter(West.Refuge,new(1240,560));await Capture("salt-entry");
            Enter(Salt.Loading,new(750,620));await Capture("salt-loading");foreach(var e in _game.EncounterEnemies)e.Health=0;Use(Salt.Manifest);
            Enter(Salt.Stairs,new(750,610));await Capture("salt-stairs");foreach(var e in _game.EncounterEnemies)e.Health=0;Use(Salt.Wheel);
            Enter(Salt.Archive,new(750,580));await Capture("salt-register");foreach(var e in _game.EncounterEnemies)e.Health=0;Use(Salt.Ledger);Use(Salt.Cache);
            Enter(Salt.Spring,new(740,700));var boss=_game.Enemies.Single(e=>e.Kind==EnemyKind.SaltWarden);boss.State=1;boss.Timer=1;boss.LockedAim=_game.Player;_game.SaltState.Tide=5;await Capture("salt-boss");
            foreach(var(id,line) in JourneyDialogue.Salt())if(Radio[id].Text!=line[1]||!_sound.HasClip("voice-"+id))throw new Exception("Missing salt voice "+id);
            if(!_sound.HasScore("boss-salt")||Salt.Ids.Any(id=>!_sound.HasScore(id)))throw new Exception("Missing salt score");
            _game.ValidateRooms();
            ChangeScreen(Screen.Videos);var state=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});StartFilm("salt-reveal",true);
            for(int i=0;i<15;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            if(_screen!=Screen.Cinematic||_filmPlayer==null||!_filmPlayer.IsPlaying()||_filmPlayer.StreamPosition<=0)throw new Exception("Salt film decoder did not advance");
            FinishGateFilm();if(_screen!=Screen.Videos||_sound.Cinematic||state!=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true}))throw new Exception("Film preview changed story");
            GD.Print("SALT CHECK PASS: reunion, four paintings, boss warning, voices, music, video playback/skip and valid campaign");GetTree().Quit();
        }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
