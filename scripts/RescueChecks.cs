using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;
public partial class Main
{
    private async void RunRescueChecks()
    {
        try
        {
            LoadWaterArt();_game=Combat.NewRescuePreview(_order);ChangeScreen(Screen.Game);SetPhysicsProcess(false);
            var enter=typeof(Combat).GetMethod("EnterConnectedRoom",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            void Enter(string id,NVec p){enter.Invoke(_game,new object[]{id});_game.Player=p;}
            void Use(NVec p){_game.Player=p;_game.Step(default);_game.Step(new(default,NVec.UnitY,false,false,false,false,false,false,false,true));}
            async System.Threading.Tasks.Task Capture(string name)
            {
                _radio="";_radioQueue.Clear();_sound.StopVoice();_bannerTime=_noticeTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();_game.UpdateRoomSight(true);QueueRedraw();
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                var path=ProjectSettings.GlobalizePath("res://artifacts/"+name+".png");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);using var img=GetViewport().GetTexture().GetImage();if(img.SavePng(path)!=Error.Ok)throw new Exception("Capture failed");
            }
            Use(Cabin.Talk);Enter(Salt.Spring,new(1240,560));await Capture("rescue-entry");Enter(Rescue.Hall,new(850,610));await Capture("rescue-hall");foreach(var e in _game.EncounterEnemies)e.Health=0;Use(Rescue.Writ);
            Enter(Rescue.Prison,new(850,610));foreach(var e in _game.EncounterEnemies)e.Health=0;Use(Rescue.Reader);await Capture("rescue-captive");Use(_game.Player);
            if(!_game.IsEbba||!_game.InCabin)throw new Exception("Ebba takeover failed");Use(Cabin.Rest);_game.Facing=new(-1,1);await Capture("rescue-ebba-aboard");
            OpenInventory();await Capture("rescue-ebba-inventory");ChangeScreen(Screen.Game);
            Enter(Rescue.Hall,new(850,610));_game.Moving=true;
            foreach(bool rear in new[]{false,true})for(int frame=0;frame<4;frame++){_game.MoveDirection=new(-1,rear?-1:1);_game.Walk=frame/2f+.01f;await Capture("rescue-walk-"+(rear?"back":"front")+frame);}_game.Moving=false;
            Enter(Rescue.Service,new(850,610));_game.Facing=new(-1,-1);_game.MoveDirection=new(-1,-1);_game.Moving=true;_game.Walk=.6f;await Capture("rescue-service");foreach(var e in _game.EncounterEnemies)e.Health=0;_game.Moving=false;Use(Rescue.Lever);
            Enter(Rescue.Machine,new(850,810));var boss=_game.Enemies.Single(e=>e.Kind==EnemyKind.Censor);boss.State=1;boss.Timer=1;boss.LockedAim=_game.Player;_game.Weapon=Weapon.Pistol;_game.Facing=NVec.Normalize(boss.Position-_game.Player);_game.AttackTime=.2f;await Capture("rescue-boss");_game.AttackTime=0;
            foreach(var(id,line) in JourneyDialogue.Rescue())if(Radio[id].Text!=line[1]||!_sound.HasClip("voice-"+id))throw new Exception("Missing rescue voice "+id);
            if(!_sound.HasScore("boss-censor")||Rescue.Ids.Any(id=>!_sound.HasScore(id)))throw new Exception("Missing rescue score");
            if(_inventoryItemArt.Count!=Items.All.Length||_inventoryItemArt.Values.Any(t=>t==null))throw new Exception("Missing inventory art");_game.ValidateRooms();
            ChangeScreen(Screen.Videos);var state=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});StartFilm("rescue-reunion",true);
            for(int i=0;i<18;i++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            if(_screen!=Screen.Cinematic||_filmPlayer==null||!_filmPlayer.IsPlaying()||_filmPlayer.StreamPosition<=0)throw new Exception("Reunion decoder did not advance");
            FinishGateFilm();if(_screen!=Screen.Videos||_sound.Cinematic||state!=System.Text.Json.JsonSerializer.Serialize(_game,new System.Text.Json.JsonSerializerOptions{IncludeFields=true}))throw new Exception("Film preview changed story");
            ChangeScreen(Screen.Game);boss.Health=0;_game.RescueState.Defeated=true;Use(Rescue.Release);Enter(Rescue.Prison,Rescue.Talk);Use(Rescue.Talk);foreach(var cue in _game.Events.ToArray())HandleCue(cue);Use(Rescue.Talk);
            foreach(var cue in _game.Events.ToArray())HandleCue(cue);
            if(!_radioQueue.Contains("rescue-ebba-found")&&_radio!="rescue-ebba-found")throw new Exception("Fast reunion interaction discarded earlier dialogue");
            if(_pendingStoryFilm!="rescue-reunion"||_game.IsEbba)throw new Exception("Reunion not queued after real handover");
            _radioQueue.Clear();_sound.StopVoice();_radioTime=_radioBreath=0;StepNarrative(1);
            if(_screen!=Screen.Cinematic||!_game.RescueState.KissSeen)throw new Exception("Story film did not persist its seen flag");
            FinishGateFilm();if(_screen!=Screen.Game||(_radio!="rescue-after-karl"&&!_radioQueue.Contains("rescue-after-karl"))||!_radioQueue.Contains("rescue-after-ebba"))throw new Exception("Skip lost reunion aftermath");
            _game.ValidateRooms();
            GD.Print("RESCUE CHECK PASS: four rooms, playable Ebba, inventory, boss, 24 voices, 5 scores and reunion decoder/skip");GetTree().Quit();
        }catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
