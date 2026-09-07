using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;
using NVec=System.Numerics.Vector2;

public partial class Main : Node2D
{
    private enum Screen { Title, Briefing, Game, Pause, Settings, Death, Ending, Testimony, Journal }
    private Screen _screen=Screen.Title;
    private Screen _settingsReturn=Screen.Title;
    private Combat _game=Combat.New(Order.Artillery);
    private Order _order;
    private Font _serif=null!,_sans=null!;
    private Texture2D _background=null!;
    private Texture2D _radioPortraits=null!;
    private Texture2D _ebbaPortrait=null!;
    private PaintedCast _cast=null!;
    private Soundscape _sound=null!;
    private readonly List<(Rect2 Rect,string Id)> _buttons=new();
    private readonly List<Particle> _particles=new();
    private readonly List<Floating> _floating=new();
    private readonly Queue<string> _radioQueue=new();
    private Vector2 _camera=new(730,700);
    private float _clock,_shake,_radioTime,_bannerTime,_noticeTime;
    private string _radio="",_banner="",_notice="";
    private bool _attack,_heavy,_dodge,_swap,_heal,_support,_interact;
    private bool _controller;
    private int _menuSelection;
    private bool _menuKeyboard;
    private float _volume=.25f;
    private float _unmutedVolume=.25f;
    private bool _adjustingVolume;
    private bool _uiChecks,_uiChecked;
    private string SettingsPath=>_uiChecks?"res://artifacts/ui-settings.cfg":"user://settings.cfg";
    private bool _cameraShake=true;
    private bool _developerSurvival;
    private bool _testMode;
    private bool _smokeCapture;
    private bool _integration;
    private bool _campaignCheck;
    private readonly HashSet<int> _capturedStages=new();
    private bool _bossCaptured;
    private bool _namesCaptured,_choiceCaptured;
    private int _journalPage;
    private float _choiceDelay;
    private bool _capturePending;
    private int _testTicks;
    private bool _atlandSlot,_portSlot,_portChecks;
    private string SavePath=>ProjectSettings.GlobalizePath(_portSlot?"user://port-save.json":_atlandSlot?"user://atland-save.json":"user://quay-save.json");
    private string ManualPath=>ProjectSettings.GlobalizePath(_portSlot?"user://port-manual-save.json":_atlandSlot?"user://atland-manual-save.json":"user://manual-save.json");
    private const float Zoom=1.12f;
    private static readonly Color Gold=new("b99a64"), Pale=new("ddd6c5"), Muted=new("9b9a8d"), Teal=new("93aaa0"), Red=new("c57761");
    private static readonly Dictionary<string,(string Speaker,string Text)> Radio=new()
    {
        ["arrival"]=("RIKSAMIRAL EBBA GRIP","Karl. Två sigill håller kajen stängd. Bryt dem. Och håll ett öga på skytten vid porten."),
        ["cannon"]=("RIKSAMIRAL EBBA GRIP","Batteriet svarar. Håll undan från nedslaget."),
        ["collector"]=("VARVETS INDRIVARE","Fyra män. Två brutna sigill. Er skuld växer, Karl. Jag tar betalningen personligen."),
        ["rage"]=("VARVETS INDRIVARE","Även havet står i skuld till kronan."),
        ["fallen"]=("RIKSAMIRAL EBBA GRIP","Vänta. Det ligger något vid porten. En karta, gjuten i brons. Den hör inte hemma här."),
        ["atland"]=("RIKSAMIRAL EBBA GRIP","Atland. Det är vad Rudbeck kallade det. Karl, ta kartan ombord. Vi måste tala ostört."),
        ["names-intro"]=("RIKSAMIRAL EBBA GRIP","Kartan följer vår egen kust. Men namnen är andra. Karl, det finns skrift under kajens sigill. Frilägg den. Jag tar fram liggaren."),
        ["names-warning"]=("RIKSAMIRAL EBBA GRIP","Rörelse vid porten. De kommer för stenarna. Lägg undan avtrycket och möt dem."),
        ["name-0"]=("RIKSAMIRAL EBBA GRIP",FieldNotes.Radio[0]),
        ["name-1"]=("RIKSAMIRAL EBBA GRIP",FieldNotes.Radio[1]),
        ["name-2"]=("RIKSAMIRAL EBBA GRIP",FieldNotes.Radio[2]),
        ["broadcast"]=("RIKSAMIRAL EBBA GRIP","Jag sänder namnen på öppen frekvens. Nu finns de hos fler än oss. Hela hamnen hörde det, Karl. Ta dig tillbaka till båten."),
        ["cipher"]=("RIKSAMIRAL EBBA GRIP","Avtrycken är säkrade. Jag skickar dem krypterat och håller båten redo med förband. Kom tillbaka, Karl. Vi behöver ett levande vittne också."),
        ["homebound"]=("RIKSAMIRAL EBBA GRIP","Alla tre namnen är ombord. På bronskartan står Uppsala där våra sjökort visar inland. Vi följer spåret i gryningen."),
        ["hedvig-karta"]=("ANTIKVARIE HEDVIG RÅLAMB","Hedvig Rålamb här. Kartans linjer följer gamla vadställen och gravhögar. Karl, vi behöver avtryck av inskrifterna."),
        ["hedvig-minne"]=("ANTIKVARIE HEDVIG RÅLAMB","Rudbecks äpplen är minne, tal och skrift. Stenarna bevarar gärningar som kronan har strukit. Ta med avtrycken.")
    };
    private sealed class Particle {public Vector2 P,V;public float Life,Max;public Color C;public float Size;}
    private sealed class Floating {public Vector2 P;public string Text="";public float Life=1;public Color C;}
    private static Vector2 G(NVec v)=>new(v.X,v.Y);
    private static NVec N(Vector2 v)=>new(v.X,v.Y);
    public override void _Ready()
    {
        var args=OS.GetCmdlineUserArgs();_portChecks=args.Contains("--port-check");_sceneChecks=args.Contains("--scene-check")||_portChecks;_uiChecks=args.Contains("--ui-check");
        _serif=GD.Load<Font>("res://assets/fonts/NotoSerif-Regular.ttf");_sans=GD.Load<Font>("res://assets/fonts/NotoSans-Regular.ttf");
        _background=GD.Load<Texture2D>("res://assets/art/likvarvet-scale-v5.png");
        _radioPortraits=GD.Load<Texture2D>("res://assets/art/radio-cast-v1.png");
        _ebbaPortrait=GD.Load<Texture2D>("res://assets/art/ebba-radio-v3.png");
        _cast=new PaintedCast();LoadJourney();
        _sound=new Soundscape();AddChild(_sound);LoadSettings();
        GetWindow().MinSize=new Vector2I(960,540);
        _campaignCheck=args.Contains("--campaign-check");_integration=args.Contains("--integration")||_campaignCheck;_testMode=args.Contains("--smoke")||_integration||_uiChecks||_sceneChecks;
        if(_testMode||args.Contains("--capture-title"))_sound.Volume=0;
        if(_integration)Engine.TimeScale=3;
        if(_testMode)StartNew();
        if(args.Contains("--duel"))StartDuel();
        if(args.Contains("--port")||_portChecks){_portSlot=true;StartAtland();}
        else if(args.Contains("--atland")||_campaignCheck)StartAtland();
        if(args.Contains("--capture-title"))_smokeCapture=true;
        if(_portChecks)RunPortChecks();else if(_sceneChecks)RunSceneChecks();
    }
    public override void _Input(InputEvent ev)
    {
        // Handle the volume strip before combat or menus, so dragging it never attacks.
        if(ev is InputEventMouseMotion volumeMotion && _adjustingVolume){SetVolumeFromPointer(volumeMotion.Position);return;}
        if(ev is InputEventMouseButton {ButtonIndex:MouseButton.Left} volumeMouse)
        {
            if(!volumeMouse.Pressed&&_adjustingVolume){_adjustingVolume=false;SaveSettings();return;}
            var p=volumeMouse.Position;
            if(volumeMouse.Pressed&&VolumePanel.HasPoint(p))
            {
                ClearPresses();
                if(p.X<VolumePanel.Position.X+38)ToggleMute();
                else{_adjustingVolume=true;SetVolumeFromPointer(p);}
                return;
            }
        }
        if(ev is InputEventMouseMotion){_controller=false;_menuKeyboard=false;}
        if(ev is InputEventJoypadMotion motion && Math.Abs(motion.AxisValue)>.25f)_controller=true;
        if(ev is InputEventKey key && key.Pressed && !key.Echo)
        {
            if(key.PhysicalKeycode==Key.M){ToggleMute();return;}
            if(key.PhysicalKeycode is Key.Minus or Key.KpSubtract){SetVolume(_volume-.05f);return;}
            if(key.PhysicalKeycode is Key.Equal or Key.KpAdd || key.Keycode==Key.Plus){SetVolume(_volume+.05f);return;}
            if(key.PhysicalKeycode==Key.F11){DisplayServer.WindowSetMode(DisplayServer.WindowGetMode()==DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);return;}
            if(key.PhysicalKeycode==Key.Escape){Back();return;}
            if(_screen!=Screen.Game)
            {
                if(key.PhysicalKeycode is Key.Down or Key.S)SelectMenu(1);
                if(key.PhysicalKeycode is Key.Up or Key.W)SelectMenu(-1);
                if(key.PhysicalKeycode==Key.Enter)ActivateSelected();
                return;
            }
            switch(key.PhysicalKeycode)
            {
                case Key.J:_attack=true;break;case Key.K:_heavy=true;break;
                case Key.Space:_dodge=true;break;case Key.Tab:_swap=true;break;
                case Key.Q:_heal=true;break;case Key.F:_support=true;break;case Key.E:_interact=true;break;
                case Key.R:_journalPage=0;_sound.Play("paper");ChangeScreen(Screen.Journal);break;
                case Key.F5:Save(true);break;case Key.F9:ResumeSave(true);break;
            }
        }
        if(ev is InputEventMouseButton mb && mb.Pressed)
        {
            _controller=false;
            if(_screen!=Screen.Game && mb.ButtonIndex==MouseButton.Left)
            {foreach(var b in _buttons)if(b.Rect.HasPoint(GetGlobalMousePosition())){Activate(b.Id);break;}return;}
            if(_screen==Screen.Game){if(mb.ButtonIndex==MouseButton.Left)_attack=true;if(mb.ButtonIndex==MouseButton.Right)_heavy=true;}
        }
        if(ev is InputEventJoypadButton jb && jb.Pressed)
        {
            _controller=true;
            if(jb.ButtonIndex==JoyButton.Start){Back();return;}
            if(_screen!=Screen.Game)
            {
                if(jb.ButtonIndex==JoyButton.DpadDown)SelectMenu(1);
                if(jb.ButtonIndex==JoyButton.DpadUp)SelectMenu(-1);
                if(jb.ButtonIndex==JoyButton.A)ActivateSelected();
                if(jb.ButtonIndex==JoyButton.B)Back();return;
            }
            if(jb.ButtonIndex==JoyButton.Back){_journalPage=0;_sound.Play("paper");ChangeScreen(Screen.Journal);return;}
            switch(jb.ButtonIndex)
            {case JoyButton.X:_attack=true;break;case JoyButton.Y:_heavy=true;break;case JoyButton.A:_dodge=true;break;case JoyButton.B:_interact=true;break;case JoyButton.RightShoulder:_swap=true;break;case JoyButton.DpadUp:_heal=true;break;case JoyButton.DpadDown:_support=true;break;}
        }
    }
    private void SelectMenu(int step){_menuKeyboard=true;_menuSelection=(_menuSelection+step+Math.Max(1,_buttons.Count))%Math.Max(1,_buttons.Count);}
    private void ActivateSelected(){if(_buttons.Count>0)Activate(_buttons[Math.Clamp(_menuSelection,0,_buttons.Count-1)].Id);}
    private void ChangeScreen(Screen screen){_screen=screen;_menuSelection=0;ClearPresses();_sound.PauseVoice(screen is not (Screen.Game or Screen.Ending or Screen.Testimony));}
    private void Back()
    {
        if(_screen==Screen.Game)ChangeScreen(Screen.Pause);
        else if(_screen==Screen.Pause)ChangeScreen(_game.Phase==Phase.Testimony?Screen.Testimony:Screen.Game);
        else if(_screen==Screen.Settings)ChangeScreen(_settingsReturn);
        else if(_screen==Screen.Briefing)ChangeScreen(Screen.Title);
        else if(_screen==Screen.Journal)ChangeScreen(Screen.Game);
        else if(_screen==Screen.Testimony)ChangeScreen(Screen.Pause);
        else if(_screen==Screen.Title)GetTree().Quit();
    }
    private void Activate(string id)
    {
        switch(id)
        {
            case "duel":StartDuel();break;
            case "journey":if(_game.ContinueJourney()){ChangeScreen(Screen.Game);foreach(var cue in _game.Events)HandleCue(cue);Save();}break;
            case "port":_portSlot=true;StartAtland();break;
            case "atland":_portSlot=false;StartAtland();break;
            case "new":ChangeScreen(Screen.Briefing);break;
            case "continue":_portSlot=false;_atlandSlot=false;ResumeSave();break;
            case "artillery":_order=Order.Artillery;break;
            case "medicine":_order=Order.Medicine;break;
            case "land":StartNew();break;
            case "broadcast":ChooseTestimony(TestimonyChoice.Broadcast);break;
            case "cipher":ChooseTestimony(TestimonyChoice.Cipher);break;
            case "journal-next":_journalPage=(_journalPage+1)%4;_sound.Play("paper");break;
            case "journal-prev":_journalPage=(_journalPage+3)%4;_sound.Play("paper");break;
            case "resume":ChangeScreen(_game.Phase==Phase.Testimony?Screen.Testimony:Screen.Game);break;
            case "save":Save(true);break;
            case "settings":_settingsReturn=_screen;ChangeScreen(Screen.Settings);break;
            case "volume-":SetVolume(_volume-.05f);break;
            case "volume+":SetVolume(_volume+.05f);break;
            case "dev-survival":_developerSurvival=!_developerSurvival;ApplyDeveloperSettings();SaveSettings();break;
            case "shake":_cameraShake=!_cameraShake;SaveSettings();break;
            case "fullscreen":DisplayServer.WindowSetMode(DisplayServer.WindowGetMode()==DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);break;
            case "back":Back();break;
            case "title":_portSlot=false;_atlandSlot=false;_sound.StopVoice();_radioQueue.Clear();_radio="";ChangeScreen(Screen.Title);break;
            case "retry":if(_game.Duel){StartDuel();break;}if(System.IO.File.Exists(SavePath))ResumeSave();else StartNew();break;
            case "quit":GetTree().Quit();break;
        }
    }
    private void ChooseTestimony(TestimonyChoice choice)
    {
        _game.Events.Clear();
        if(!_game.ChooseTestimony(choice))return;
        _radioQueue.Clear();_sound.StopVoice();_radioTime=0;
        foreach(var cue in _game.Events)HandleCue(cue);
        ChangeScreen(Screen.Game);_banner=_game.ExtendedJourney?"GENOM KRONANS MAGASIN":"TILLBAKA TILL BÅTEN";_bannerTime=5;
    }
    private void StartNew()
    {
        _portSlot=false;_atlandSlot=false;_game=Combat.New(_order,true);_game.AtlandCampaign=!_integration;ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(85,-80);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        ChangeScreen(Screen.Game);_banner="BLEKINGES LIKVARV";_bannerTime=5;Save();
    }
    private void Save(bool manual=false)
    {
        if(_testMode||_game.Duel)return;
        try{SaveStore.Write(manual?ManualPath:SavePath,_game);Notice(manual?"Manuellt läge sparat  ·  F9 laddar":"Kontrollpunkt sparad");}catch(Exception e){GD.PushError(e.Message);Notice("Kunde inte spara fältdagboken");}
    }
    private void ResumeSave(bool manual=false)
    {
        try
        {
            _game=SaveStore.Read(manual?ManualPath:SavePath);ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(85,-80);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
            if(_game.InCampaign){_campaignText=_game.Stage.Intro;_campaignTextTime=10;_revealTime=0;}
            ChangeScreen(_game.Dead?Screen.Death:_game.Phase==Phase.Complete?Screen.Ending:_game.Phase==Phase.Testimony?Screen.Testimony:Screen.Game);Notice("Fältdagboken återupptagen");
        }
        catch(Exception e){GD.PushWarning(e.Message);Notice("Sparfilen kunde inte läsas. Du kan starta en ny landstigning.");}
    }
    private void Notice(string text){_notice=text;_noticeTime=4;}
    private void LoadSettings()
    {
        var config=new ConfigFile();if(config.Load(SettingsPath)==Error.Ok){_volume=Math.Clamp((float)config.GetValue("audio","volume",.25f),0,1);_unmutedVolume=Math.Clamp((float)config.GetValue("audio","unmuted_volume",.25f),.001f,1);_cameraShake=(bool)config.GetValue("display","shake",true);_developerSurvival=(bool)config.GetValue("developer","survival",false);}_sound.Volume=_volume;
        if(_volume>0)_unmutedVolume=_volume;ApplyDeveloperSettings();
    }
    private void ApplyDeveloperSettings()
    {
        _game.DeveloperSurvival=_developerSurvival&&(!_testMode||_uiChecks);
        if(_game.DeveloperSurvival)_game.Health=Math.Max(1,_game.Health);
    }
    private Rect2 VolumePanel=>new(_screen==Screen.Game&&_revealTime<=0?new Vector2(20,88):new Vector2(1020,18),new Vector2(240,36));
    private void SetVolume(float volume)
    { _volume=Math.Clamp(volume,0,1);if(_volume>0)_unmutedVolume=_volume;SaveSettings(); }
    private void ToggleMute()=>SetVolume(_volume>0?0:_unmutedVolume);
    private void SetVolumeFromPointer(Vector2 pointer)
    { _volume=Math.Clamp((pointer.X-VolumePanel.Position.X-49)/137,0,1);if(_volume>0)_unmutedVolume=_volume;_sound.Volume=_testMode?0:_volume; }
    private void SaveSettings(){_sound.Volume=_testMode?0:_volume;var c=new ConfigFile();c.SetValue("audio","volume",_volume);c.SetValue("audio","unmuted_volume",_unmutedVolume);c.SetValue("display","shake",_cameraShake);c.SetValue("developer","survival",_developerSurvival);c.Save(SettingsPath);}
    private void ClearPresses(){_attack=_heavy=_dodge=_swap=_heal=_support=_interact=false;}
    public override void _PhysicsProcess(double delta)
    {
        if(_screen!=Screen.Game||_sceneChecks)return;
        if(_revealTime>5){ClearPresses();return;}
        var move=new Vector2((Input.IsPhysicalKeyPressed(Key.D)||Input.IsPhysicalKeyPressed(Key.Right)?1:0)-(Input.IsPhysicalKeyPressed(Key.A)||Input.IsPhysicalKeyPressed(Key.Left)?1:0),(Input.IsPhysicalKeyPressed(Key.S)||Input.IsPhysicalKeyPressed(Key.Down)?1:0)-(Input.IsPhysicalKeyPressed(Key.W)||Input.IsPhysicalKeyPressed(Key.Up)?1:0));
        var aim=ScreenToWorld(GetGlobalMousePosition())-G(_game.Player);
        bool guard=Input.IsPhysicalKeyPressed(Key.Shift);
        if(_controller)
        {
            var stick=new Vector2(Input.GetJoyAxis(0,JoyAxis.LeftX),Input.GetJoyAxis(0,JoyAxis.LeftY));if(stick.Length()>.18f)move=stick;
            var right=new Vector2(Input.GetJoyAxis(0,JoyAxis.RightX),Input.GetJoyAxis(0,JoyAxis.RightY));
            aim=right.Length()>.22f?right:move.Length()>.1f?move:G(_game.Facing);
            guard=Input.IsJoyButtonPressed(0,JoyButton.LeftShoulder);
        }
        if(_testMode){_testTicks++;move=_testTicks<55?new Vector2(.6f,-.3f):Vector2.Zero;aim=new Vector2(1,-.2f);_attack=_testTicks%30==0;}
        if(_integration)
        {
            var target=_game.Enemies.Where(e=>!e.Dead).OrderBy(e=>NVec.DistanceSquared(e.Position,_game.Player)).FirstOrDefault();
            NVec goal=target?.Position??_game.Seals.FirstOrDefault(s=>s.Health>0)?.Position??_game.ObjectivePosition;
            var d=goal-_game.Player;float distance=d.Length();var navigation=_game.NextWaypoint(_game.Player,goal)-_game.Player;
            guard=target!=null&&target.State==1&&target.Timer<.16f&&distance<150;
            move=(distance>60||!_game.ClearPath(_game.Player,goal))?G(Combat.Normal(navigation,NVec.UnitX)):Vector2.Zero;aim=G(d);
            bool fighting=target!=null||_game.Seals.Any(s=>s.Health>0);
            _attack=fighting&&distance<95&&!guard;_heavy=fighting&&distance<105&&(_testTicks-1)%47==0&&!guard;
            _heal=_game.Health<48;_support=target!=null;_interact=true;
        }
        bool reading=_interact||Input.IsPhysicalKeyPressed(Key.E)||(_controller&&Input.IsJoyButtonPressed(0,JoyButton.B));
        var controls=new Controls(N(move),N(aim),_attack||(!_controller&&!_adjustingVolume&&!VolumePanel.HasPoint(GetGlobalMousePosition())&&Input.IsMouseButtonPressed(MouseButton.Left)),_heavy,_dodge,guard,_swap,_heal,_support,reading);
        RememberRenderPositions();
        _game.Step(controls);ClearPresses();
        if(_campaignCheck&&_testTicks%600==0)GD.Print($"CAMPAIGN CHECK stage={_game.CampaignStage+1} progress={_game.CampaignProgress} foes={_game.Enemies.Count(e=>!e.Dead)} hp={_game.Health} elapsed={_game.Elapsed} at={_game.Player} goal={_game.CampaignObjective}");
        foreach(var cue in _game.Events)HandleCue(cue);
        if(_game.Dead)ChangeScreen(Screen.Death);
        if(_game.Phase==Phase.Complete)ChangeScreen(Screen.Ending);
        if(_game.Phase==Phase.Testimony){ChangeScreen(Screen.Testimony);_choiceDelay=0;}
    }
    private void HandleCue(Cue cue)
    {
        var p=G(cue.Position);
        switch(cue.Kind)
        {
            case "radio":QueueRadio(cue.Text);break;
            case "campaign":_campaignText=cue.Text;_campaignTextTime=10;break;
            case "region":_camera=G(_game.Player)+new Vector2(0,-30);_particles.Clear();_floating.Clear();_radioQueue.Clear();_sound.StopVoice();_radioTime=0;_banner=cue.Text.ToUpperInvariant();_bannerTime=5;break;
            case "reveal":_radioQueue.Clear();_sound.StopVoice();_radioTime=0;_revealTime=9;_banner="VÄGEN LIGGER KVAR";_bannerTime=5;_sound.Play("seal",.65f);break;
            case "checkpoint":Save();break;
            case "hit":Burst(p,Gold,12,100);_floating.Add(new(){P=p+new Vector2(0,-70),Text=cue.Text,C=Gold});_sound.Play("hit",.94f+(float)(_game.Tick%6)*.025f);_shake=3;break;
            case "hurt":Burst(p,Red,13,100);_sound.Play("hit",.65f);_shake=7;break;
            case "parry":Burst(p,Teal,30,190);_floating.Add(new(){P=p-new Vector2(0,110),Text=cue.Text,C=Teal});_sound.Play("parry");_shake=6;break;
            case "swing":_sound.Play(cue.Value>0?"hammer":"swing");break;
            case "shot":_sound.Play("shot");Burst(p,Gold,7,90);break;
            case "seal":Burst(p,Teal,45,210);_sound.Play("seal");_floating.Add(new(){P=p-new Vector2(0,60),Text=cue.Text,C=Teal});break;
            case "step":_sound.Play("footstep",.93f+(_game.Tick%7)*.025f);break;
            case "scrape":_sound.Play("scrape",.96f+(float)(_game.Tick%5)*.02f);break;
            case "inscription":Burst(p,Gold,18,65);_sound.Play("inscription");_floating.Add(new(){P=p-new Vector2(0,70),Text=cue.Text,C=Gold});break;
            case "sealhit":Burst(p,Teal,12,100);_sound.Play("hit");break;
            case "heal":Burst(p,Teal,24,70);_sound.Play("heal");_floating.Add(new(){P=p-new Vector2(0,80),Text=cue.Text,C=Teal});break;
            case "cannon":case "slam":Burst(p,Gold,60,260);_sound.Play("cannon");_shake=12;break;
            case "death":Burst(p,Muted,22,110);_sound.Play("death");break;
            case "dodge":Burst(p,new Color(.45f,.57f,.6f,.5f),8,30);_sound.Play("dodge");break;
            case "block":_sound.Play("parry",.7f);if(cue.Text!="")_floating.Add(new(){P=p-new Vector2(0,80),Text=cue.Text,C=Muted});break;
        }
    }
    private void QueueRadio(string id)
    {
        // Clear obsolete combat orders at the scene change. A slain enemy
        // should not continue threatening Karl over a new discovery.
        if(id is "fallen" or "names-intro")
        {
            var retained=_radioQueue.Where(key=>key.StartsWith("name-")||key.StartsWith("hedvig-")).ToArray();
            _radioQueue.Clear();foreach(var key in retained)_radioQueue.Enqueue(key);
            _sound.StopVoice();_radioTime=0;_radio="";
        }
        if(id=="names-warning"&&(_radio==id&&(_radioTime>0||_sound.Speaking)))return;
        if(!_radioQueue.Contains(id))_radioQueue.Enqueue(id);
    }
    private void Burst(Vector2 p,Color c,int count,float speed)
    {
        for(int i=0;i<count;i++){float a=i*2.39996f+_clock;float life=.25f+(i%7)*.07f;_particles.Add(new(){P=p+new Vector2(0,-25),V=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*speed*(.3f+(i%5)*.17f),Life=life,Max=life,C=c,Size=1+(i%3)});}
    }
    public override void _Process(double delta)
    {
        float dt=(float)delta;_clock+=dt;_noticeTime=Math.Max(0,_noticeTime-dt);if(_screen==Screen.Game)_campaignTextTime=Math.Max(0,_campaignTextTime-dt);
        _sound.Boss=(_game.Phase==Phase.Collector||(_game.Phase==Phase.Extraction&&_game.Enemies.Any(e=>!e.Dead))||(_game.InCampaign&&_game.Enemies.Any(e=>!e.Dead&&e.Kind==EnemyKind.Collector))) && _screen is Screen.Game or Screen.Pause;
        _sound.Discovery=_game.Phase is Phase.Names or Phase.Testimony || _game.Region==Region.Shore;
        if(_screen is Screen.Game or Screen.Ending or Screen.Testimony)
        {
            _revealTime=Math.Max(0,_revealTime-dt);_bannerTime=Math.Max(0,_bannerTime-dt);_shake=Math.Max(0,_shake-dt*22);
            if(_screen==Screen.Game)
            {
                var target=G(_game.Player)+new Vector2(70,-70);target.X=Math.Clamp(target.X,610,990);target.Y=Math.Clamp(target.Y,535,740);
                if(_game.Region!=Region.Quay){target=G(_game.Player)+new Vector2(0,-60);target.X=Math.Clamp(target.X,580,956);target.Y=Math.Clamp(target.Y,415,730);}
                if(_revealTime<=5&&_game.Moving)_revealTime=0;
                if(_revealTime>0)target=new Vector2(600,320);
                _camera=_camera.Lerp(target,1-Mathf.Exp(-dt*5));
            }
            if(_radioTime>0)_radioTime-=dt;
            if(_radioTime<=0 && !_sound.Speaking && _radioQueue.Count>0){_radio=_radioQueue.Dequeue();_radioTime=Radio.TryGetValue(_radio,out var line)?Math.Max(8,line.Text.Length/15f):8;_sound.Speak(_radio);}
            foreach(var p in _particles){p.P+=p.V*dt;p.V*=Mathf.Exp(-dt*3);p.Life-=dt;}
            _particles.RemoveAll(p=>p.Life<=0);
            foreach(var f in _floating){f.P.Y-=dt*25;f.Life-=dt;}_floating.RemoveAll(f=>f.Life<=0);
        }
        QueueRedraw();
        if(_uiChecks&&!_uiChecked&&_testTicks>90){_uiChecked=true;RunUiChecks();return;}
        if(_integration)
        {
            if(_game.InCampaign&&_capturedStages.Add(_game.CampaignStage))CaptureFrame($"campaign-{_game.CampaignStage+1:00}.png",false);
            if(_game.Region!=Region.Quay&&_capturedRegions.Add(_game.Region))CaptureFrame(_game.Region.ToString().ToLowerInvariant()+".png",false);
            if(_game.AtlandRevealed&&!_revealCaptured&&_revealTime<7){_revealCaptured=true;CaptureFrame("reveal.png",false);}
            if(_game.Phase==Phase.Collector&&!_bossCaptured){_bossCaptured=true;CaptureFrame("boss.png",false);}
            if(_game.Phase==Phase.Names&&!_namesCaptured){_namesCaptured=true;CaptureFrame("names.png",false);}
            if(_screen==Screen.Testimony)
            {
                if(!_choiceCaptured){_choiceCaptured=true;CaptureFrame("testimony.png",false);}
                _choiceDelay+=dt;if(_choiceDelay>2)ChooseTestimony(TestimonyChoice.Broadcast);
            }
            if(_game.Phase==Phase.Complete&&!_capturePending){_capturePending=true;GD.Print($"INTEGRATION COMPLETE health={_game.Health} kills={_game.Kills} parries={_game.Parries}");CaptureFrame("ending.png",true);}
            if(_game.Dead||_game.Elapsed>600){GD.PushError("Integration encounter failed");GetTree().Quit(1);}
        }
        else if(!_sceneChecks&&((_testMode && _testTicks>125)||(_smokeCapture && _clock>1.5f)))CaptureAndQuit();
    }
    private async void RunUiChecks()
    {
        try
        {
            void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
            SetVolume(.25f);
            _Input(new InputEventKey{PhysicalKeycode=Key.M,Pressed=true});Check(_volume==0,"M must mute");
            _Input(new InputEventKey{PhysicalKeycode=Key.M,Pressed=true});Check(Math.Abs(_volume-.25f)<.001f,"M must restore previous level");
            _Input(new InputEventKey{PhysicalKeycode=Key.Minus,Pressed=true});Check(Math.Abs(_volume-.20f)<.001f,"Minus must lower volume");
            var p=VolumePanel.Position+new Vector2(49+137*.4f,18);
            _Input(new InputEventMouseButton{ButtonIndex=MouseButton.Left,Pressed=true,Position=p});
            Check(_adjustingVolume&&!_attack&&Math.Abs(_volume-.4f)<.001f,"Volume click must adjust without attacking");
            _Input(new InputEventMouseMotion{Position=p+new Vector2(500,0)});Check(_volume==1&&!_attack,"Dragging outside clamps without combat input");
            _Input(new InputEventMouseMotion{Position=p-new Vector2(500,0)});Check(_volume==0,"Dragging left clamps to silence");
            _Input(new InputEventMouseButton{ButtonIndex=MouseButton.Left,Pressed=false,Position=p});Check(!_adjustingVolume,"Release must end drag");
            SetVolume(.25f);_volume=.9f;LoadSettings();Check(Math.Abs(_volume-.25f)<.001f,"Volume must survive settings reload");
            SetVolume(.4f);ToggleMute();_unmutedVolume=.1f;LoadSettings();ToggleMute();
            Check(Math.Abs(_volume-.4f)<.001f,"Muted settings must preserve the restoration level across reload");SetVolume(.25f);
            _sound.Volume=0;
            _Input(new InputEventKey{PhysicalKeycode=Key.R,Pressed=true});Check(_screen==Screen.Journal,"R opens field notes");
            Back();Check(_screen==Screen.Game,"Escape returns from field notes");
            Activate("dev-survival");Check(_developerSurvival&&_game.DeveloperSurvival,"Settings toggle applies survival immediately");
            _developerSurvival=false;LoadSettings();Check(_developerSurvival&&_game.DeveloperSurvival,"Developer preference survives settings reload");
            StartDuel();Check(_game.DeveloperSurvival,"New duel inherits developer preference");
            StartNew();Check(_game.DeveloperSurvival,"New campaign inherits developer preference");
            Activate("dev-survival");Check(!_game.DeveloperSurvival,"Toggle off restores normal damage");
            _settingsReturn=Screen.Game;ChangeScreen(Screen.Settings);QueueRedraw();
            await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using(var settingsImage=GetViewport().GetTexture().GetImage())settingsImage.SavePng("res://artifacts/developer-settings.png");
            Back();
            GD.Print("DEV SETTINGS CHECK PASS: toggle, reload, new campaign and duel");
            GD.Print("UI CHECK PASS: mute, restore, shortcut, slider, clamp, no attack, persistence, journal");
            QueueRedraw();await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            GetViewport().GetTexture().GetImage().SavePng(ProjectSettings.GlobalizePath("res://artifacts/volume-controls.png"));
            // Inspect every speaker at the actual radio-card size.
            foreach(var key in new[]{"arrival","hedvig-karta","collector"})
            {
                Check(ResourceLoader.Exists($"res://assets/audio/voice-{key}.ogg"),$"Missing voice asset: {key}");
                _radio=key;_radioTime=30;_radioQueue.Clear();QueueRedraw();
                await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var frame=GetViewport().GetTexture().GetImage();
                frame.SavePng(ProjectSettings.GlobalizePath($"res://artifacts/radio-{key}.png"));
            }
            GD.Print("RADIO CHECK PASS: all three portrait/voice pairs");GetTree().Quit();
        }
        catch(Exception error){GD.PushError(error.ToString());GetTree().Quit(1);}
    }
    private async void CaptureFrame(string name,bool quit)
    {
        await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        var path=ProjectSettings.GlobalizePath("res://artifacts/"+name);System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!);using var capture=GetViewport().GetTexture().GetImage();var result=capture.SavePng(path);if(result!=Error.Ok){GD.PushError("Capture failed: "+result);GetTree().Quit(1);return;}GD.Print("CAPTURE "+path);if(quit)GetTree().Quit();
    }
    private async void CaptureAndQuit()
    {
        if(!_testMode&&!_smokeCapture)return;_testMode=false;_smokeCapture=false;
        await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        var path=ProjectSettings.GlobalizePath("res://artifacts/"+(_screen==Screen.Title?"title.png":"gameplay.png"));
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!);using var capture=GetViewport().GetTexture().GetImage();var result=capture.SavePng(path);if(result!=Error.Ok){GD.PushError("Capture failed: "+result);GetTree().Quit(1);return;}GD.Print("CAPTURE "+path);GetTree().Quit();
    }
    private Vector2 Offset=>new Vector2(640,360)-_camera*Zoom+(_cameraShake?new Vector2(Mathf.Sin(_clock*65),Mathf.Cos(_clock*71))*_shake:Vector2.Zero);
    private Vector2 ScreenToWorld(Vector2 p)=>(p-Offset)/Zoom;
    public override void _Draw()
    {
        if(_background==null)return;
        _buttons.Clear();
        if(_screen is Screen.Title or Screen.Briefing)
        {
            DrawTextureRect(_background,new Rect2(-30,-170,1340,894),false,new Color(.5f,.61f,.65f));
            DrawRect(new Rect2(0,0,1280,720),new Color(.012f,.035f,.045f,.38f));
            DrawRect(new Rect2(0,0,760,720),new Color(.025f,.05f,.062f,.78f));
        }
        else DrawWorld();
        if(_screen==Screen.Game)DrawHud();
        else if(_screen==Screen.Title)DrawTitle();
        else if(_screen==Screen.Briefing)DrawBriefing();
        else if(_screen==Screen.Ending)DrawEnding();
        else if(_screen==Screen.Testimony)DrawTestimony();
        else if(_screen==Screen.Journal)DrawJournal();
        else
        {
            DrawRect(new Rect2(0,0,1280,720),new Color(.015f,.03f,.043f,.88f));
            if(_screen==Screen.Pause)DrawPause();else if(_screen==Screen.Settings)DrawSettings();else DrawDeath();
        }
        if(_noticeTime>0&&_revealTime<=0){Panel(new Rect2(350,16,580,40),.96f);Text(_notice,new Vector2(370,43),16,Teal);}
        DrawVolume();
        if(_screen==Screen.Game && !_controller&&_revealTime<=0)
        {var p=GetGlobalMousePosition();DrawArc(p,7,0,Mathf.Tau,20,new Color(1,.87f,.61f,.7f),1,true);DrawLine(p-new Vector2(11,0),p-new Vector2(5,0),Gold,1,true);DrawLine(p+new Vector2(5,0),p+new Vector2(11,0),Gold,1,true);}
    }
    private void DrawVolume()
    {
        var r=VolumePanel;Panel(r,.97f);var p=r.Position;
        Text(_volume<=0?"AV":"M",p+new Vector2(11,24),13,_volume<=0?Red:Gold);
        var start=p+new Vector2(49,18);var end=start+new Vector2(137,0);
        DrawLine(start,end,new Color(.25f,.27f,.25f),4,true);
        DrawLine(start,start+new Vector2(137*_volume,0),Gold,4,true);
        DrawCircle(start+new Vector2(137*_volume,0),5,Pale);
        Text($"{Math.Round(_volume*100)}%",p+new Vector2(197,24),12,Pale);
        if(r.HasPoint(GetGlobalMousePosition()))
        {Panel(new Rect2(p+new Vector2(0,39),new Vector2(240,26)),.96f);Text("Ljud · dra reglaget · M tyst · +/−",p+new Vector2(8,57),11,Muted);}
    }
    private void DrawWorld()
    {
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        if(_game.Region==Region.Quay)DrawTextureRectRegion(_background,new Rect2(18,30,1500,946),new Rect2(18,30,1500,946),Colors.White);
        else {DrawTextureRect(_game.InCampaign?_campaignWorlds[_game.Stage.World]:_game.Region==Region.Warehouse?_warehouse:_game.AtlandRevealed?_shoreRevealed:_shore,new Rect2(0,0,1536,1024),false);if(_game.InCampaign)DrawCampaignMarkers();else DrawJourneyMarkers();}
        foreach(var seal in _game.Seals)
        {
            var p=G(seal.Position);bool alive=seal.Health>0;var c=alive?Teal:Muted;
            DrawArc(p,31,0,Mathf.Tau,60,new Color(c,alive?.75f:.25f),2,true);
            if(alive){DrawArc(p,38,_clock*.4f,_clock*.4f+Mathf.Pi*1.5f,40,new Color(Teal,.45f),1.5f,true);DrawLine(p-new Vector2(0,10),p-new Vector2(0,49),Teal,2,true);DrawLine(p-new Vector2(0,42),p+new Vector2(12,-32),Teal,2,true);DrawLine(p-new Vector2(0,30),p+new Vector2(-12,-20),Teal,2,true);WorldBar(p+new Vector2(-24,15),48,seal.Health/80,Teal);}
        }
        foreach(var h in _game.Hazards)
        {var c=h.Friendly?Gold:Red;DrawCircle(G(h.Position),h.Radius,new Color(c,.10f));DrawArc(G(h.Position),h.Radius,0,Mathf.Tau,70,c,2,true);DrawArc(G(h.Position),h.Radius*Math.Clamp(1-h.Timer/1.5f,0,1),0,Mathf.Tau,60,new Color(c,.5f),2,true);}
        foreach(var e in _game.Enemies.Where(e=>!e.Dead && e.State==1))DrawTelegraph(e);
        DrawDepthSortedActors();
        foreach(var shot in _game.Shots){var p=G(shot.Position);var v=G(shot.Velocity).Normalized();DrawLine(p-v*17,p,shot.Reflected?Teal:Gold,3,true);DrawCircle(p,3,Pale);}
        if(_game.Phase==Phase.Discovery)
        {var p=G(Combat.ChartPosition);DrawCircle(p,30,new Color(Gold,.13f));DrawArc(p,25,0,Mathf.Tau,40,Gold,1.5f,true);DrawRect(new Rect2(p-new Vector2(14,18),new Vector2(28,24)),Gold);DrawLine(p-new Vector2(10,4),p+new Vector2(10,-10),new Color(.15f,.23f,.24f),2,true);}
        if(_game.Phase is Phase.Names or Phase.Testimony or Phase.Extraction)
        {
            for(int i=0;i<_game.Inscriptions.Count;i++)
            {
                var stone=_game.Inscriptions[i];var p=G(stone.Position);
                DrawArc(p,33,0,Mathf.Tau,48,new Color(stone.Read?Gold:Teal,.65f),1.3f,true);
                if(stone.Progress>0&&!stone.Read)DrawArc(p,38,-Mathf.Pi/2,-Mathf.Pi/2+Mathf.Tau*stone.Progress/Combat.ReadingDuration,48,Gold,3,true);
                Text(stone.Read?FieldNotes.Names[i]:(i+1).ToString(),p+new Vector2(stone.Read?-65:-4,29),11,stone.Read?Gold:Muted);
            }
        }
        if(_game.Phase==Phase.Extraction)
        {var p=G(_game.ObjectivePosition);DrawArc(p,40,0,Mathf.Tau,48,Gold,2,true);Text(_game.ExtendedJourney?"MAGASINET":"BÅTEN",p+new Vector2(-22,58),12,Gold);}
        foreach(var p in _particles)DrawCircle(p.P,p.Size,new Color(p.C,Math.Clamp(p.Life/p.Max,0,1)));
        foreach(var f in _floating)Text(f.Text,f.P,15,new Color(f.C,Math.Clamp(f.Life*2,0,1)));
        DrawSetTransform(Vector2.Zero);
        // Restrained edge framing.
        DrawRect(new Rect2(0,0,1280,6),new Color(.015f,.035f,.04f,.8f));
        if(_game.Hurt>0)DrawRect(new Rect2(0,0,1280,720),new Color(.6f,.05f,.035f,_game.Hurt*.32f));
    }
    private void DrawTelegraph(Fighter e)
    {
        var p=G(e.Position);var f=G(e.Facing);
        if(e.Kind==EnemyKind.Gunner){DrawLine(p,G(e.LockedAim),new Color(Red,.6f),2,true);DrawArc(G(e.LockedAim),17,0,Mathf.Tau,30,Red,1,true);return;}
        float range=e.Kind==EnemyKind.Collector?145:e.Kind==EnemyKind.Pikeman?135:88;
        float a=f.Angle();float spread=e.Kind==EnemyKind.Pikeman?.22f:.95f;
        var points=new List<Vector2>{p};for(int i=0;i<=20;i++)points.Add(p+Vector2.FromAngle(a-spread+spread*2*i/20)*range);
        DrawColoredPolygon(points.ToArray(),new Color(Red,.16f+Mathf.Sin(_clock*15)*.04f));
        DrawArc(p,range,a-spread,a+spread,30,new Color(Red,.8f),2,true);
    }
    private void Actor(Fighter e,bool dead)
    {
        string kind=e.Kind.ToString().ToLowerInvariant();if(kind=="guard")kind="guard";
        int pose=e.State==1?3:e.State==2?4:e.State==3?5:(int)e.Walk%2+1;
        if(dead)pose=6;
        DrawActor(kind,RenderPosition(e),G(e.Moving&&e.State==0&&e.MoveDirection.LengthSquared()>.01f?e.MoveDirection:e.Facing),pose,e.Hurt,dead,false,e.Moving,e.Walk,e.State==2&&e.Timer>.37f&&e.Timer<=.5f);
        if(!dead && e.Health<e.MaxHealth)WorldBar(G(e.Position)+new Vector2(-24,-160),48,e.Health/e.MaxHealth,e.Kind==EnemyKind.Collector?Gold:Red);
    }
    private void DrawPlayer()
    {
        int pose=_game.Guarding?5:_game.AttackTime>0?(_game.AttackContact?4:3):_game.Moving?(int)_game.Walk%2+1:0;
        DrawActor(_game.Weapon==Weapon.Saber?"karl-saber":"karl-hammer",RenderPlayer,G(_game.Facing),pose,_game.Hurt,_game.Dead,true,_game.Moving,_game.Walk,_game.AttackContact&&_game.AttackTime<_game.ContactTime+.09f);
        if(_game.AttackTime>0 && _game.AttackContact)
        {
            float a=G(_game.Facing).Angle();float r=_game.Weapon==Weapon.Hammer?97:89;
            DrawArc(RenderPlayer+new Vector2(0,-25),r,a-1.1f,a+1.1f,25,new Color(_game.HeavyAttack?Gold:Pale,.55f),_game.Weapon==Weapon.Hammer?5:3,true);
        }
        if(_game.Guarding)DrawArc(RenderPlayer+new Vector2(0,-34),39,G(_game.Facing).Angle()-.9f,G(_game.Facing).Angle()+.9f,24,_game.GuardTime<.22f?Teal:Muted,3,true);
    }
    private void DrawActor(string kind,Vector2 p,Vector2 facing,int pose,float hurt,bool dead,bool player=false,bool moving=false,float walk=0,bool contact=false)
    {
        var shadow=new Vector2[24];for(int i=0;i<24;i++)shadow[i]=p+new Vector2(Mathf.Cos(i*Mathf.Tau/24)*25,Mathf.Sin(i*Mathf.Tau/24)*10);
        DrawColoredPolygon(shadow,new Color(.01f,.02f,.025f,dead?.2f:.42f));
        if(player)DrawArc(p,23,0,Mathf.Tau,40,new Color(Teal,.5f),1.5f,true);
        if(kind is "karl-saber" or "guard")
        {
            string action="attack";int frame=pose==3?1:pose==4?(contact?2:3):0;
            if(moving&&pose<3){action="walk";frame=(int)walk%4;if(player)facing=G(_game.MoveDirection);}
            if(pose==5){action="react";frame=player?0:1;}
            if(hurt>0){action="react";frame=1;}
            if(player&&_game.DodgeTime>0){action="react";frame=2;facing=G(_game.DodgeDirection);}
            if(dead){action="react";frame=3;}
            _animated.Draw(this,player?"karl":"guard",p,facing,action,frame,hurt,dead,Offset,Zoom);
        }
        else if(kind is "karl-hammer" or "collector" or "pikeman" or "gunner" && moving&&pose<3&&hurt<=0&&!dead)
            _animated.Draw(this,kind,p,player?G(_game.MoveDirection):facing,"walk",(int)walk%4,0,false,Offset,Zoom);
        else _cast.Draw(this,kind,p,facing,pose,hurt,dead,Offset,Zoom);
    }
    private void DrawHud()
    {
        if(_revealTime>0)
        {
            Centered("VÄGEN UNDER VATTNET",730,74,27,Pale,true);
            Centered("Kartan följer landskapet.",730,103,16,Gold);
            if((_radioTime>0||_sound.Speaking)&&Radio.ContainsKey(_radio))DrawRadio();
            return;
        }
        if(_game.DeveloperSurvival){Panel(new Rect2(20,132,240,29),.9f);Text("DEV · Karl överlever på 1 liv",new Vector2(30,152),12,Gold);}
        Panel(new Rect2(20,18,294,60),.87f);Text("STORMAKT 3020",new Vector2(38,41),12,Gold);Text(_game.RegionName,new Vector2(38,65),20,Pale,true);
        Panel(new Rect2(928,18,332,102),.91f);Text(_game.Duel?"ÖVNING  /  SABEL":_game.InCampaign?$"ATLAND  /  BANA {_game.CampaignStage+1} AV 8":$"EXPEDITION  /  {(int)_game.Region+1:00}",new Vector2(946,42),12,Gold);
        string objective=_game.Phase switch
        {
            Phase.Quay=>$"Bryt kajens sigill  ·  {_game.Seals.Count(s=>s.Health<=0)}/2",
            Phase.Collector=>"Besegra varvets indrivare",
            Phase.Names=>$"Frilägg namnen  ·  {_game.Inscriptions.Count(i=>i.Read)}/3",
            Phase.Extraction=>_game.ExtendedJourney?"Följ kartan genom magasinet":"Ta vittnesmålen till båten",
            Phase.Warehouse or Phase.Shore or Phase.Reveal=>JourneyGoal,
            Phase.Duel=>"Besegra sabelvakten",
            Phase.Campaign=>_game.CampaignGoal,
            _=>"Undersök bronskartan vid porten"
        };
        Text(objective,new Vector2(946,69),17,Pale);
        int foes=_game.Enemies.Count(e=>!e.Dead);
        Text(foes>0?$"Vakter kvar: {foes}":_game.Phase==Phase.Names?"Håll E vid en sten  ·  R Fynd":"E Undersök  ·  R Fynd",new Vector2(946,96),14,Muted);
        if(_game.Phase is Phase.Names or Phase.Extraction || _game.Region!=Region.Quay)DrawObjectiveDirection();
        DrawJourneyPrompt(foes);
        if(_game.InCampaign)DrawCampaignStory();
        var boss=_game.Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.Collector&&!e.Dead);
        if(boss!=null){Panel(new Rect2(354,20,542,59),.91f);Centered(_game.InCampaign?(_game.CampaignStage==7?"KOLLEGIETS VÄKTARE":"KRONFOGDEN"):"VARVETS INDRIVARE",625,42,14,Gold);WorldBar(new Vector2(378,56),490,boss.Health/boss.MaxHealth,Red);}
        Panel(new Rect2(20,623,381,77),.96f);Text("KARL CCLV",new Vector2(38,646),13,Gold);Text($"{Math.Ceiling(_game.Health)} / 100",new Vector2(302,646),13,Pale);
        WorldBar(new Vector2(38,657),345,_game.Health/100,new Color("b6574d"),11);WorldBar(new Vector2(38,677),345,_game.Stamina/100,Teal,5);
        Panel(new Rect2(417,623,470,77),.96f);Text(_game.Weapon==Weapon.Saber?"OFFICERSSABEL":"GRUVHAMMARE",new Vector2(435,647),16,Gold);
        Text(_controller?"RB  Byt vapen     ↑  Tinktur ×"+_game.Potions:"TAB  Byt vapen     Q  Tinktur ×"+_game.Potions,new Vector2(435,674),14,Muted);
        Panel(new Rect2(903,623,357,77),.96f);Text(_game.Order==Order.Artillery?"ÖRLOGSBATTERI":"FÄLTSJUKVÅRD",new Vector2(921,647),14,Gold);
        Text(_game.SupportCooldown<=0?(_controller?"↓  Understöd redo":"F  Understöd redo"):$"Redo om {Math.Ceiling(_game.SupportCooldown)} s",new Vector2(921,674),16,_game.SupportCooldown<=0?Teal:Muted);
        if(_bannerTime>0)
        {float alpha=Math.Clamp(Math.Min(_bannerTime,5-_bannerTime),0,1);Centered(_banner,640,180,32,new Color(Pale,alpha),true);Centered(_game.InCampaign?_game.Stage.Goal:_game.AtlandRevealed?"Kartan följer landskapet.":_game.Region==Region.Warehouse?"Kollegiets förråd. Kollegiets hemligheter.":_game.Region==Region.Shore?"Gravhögen · vadstället · farleden":"En kust som inte längre räknar sina döda.",640,211,16,new Color(Muted,alpha));}
        if(_game.Phase==Phase.Names)
        {
            int index=_game.Inscriptions.FindIndex(i=>!i.Read&&NVec.Distance(_game.Player,i.Position)<Combat.ReadingRange);
            if(index>=0)
            {
                var stone=_game.Inscriptions[index];
                string prompt=_game.ReadingBlocked(index)?"Skapa arbetsro kring stenen":_game.ReadingIndex==index?"Frilägger inskriften…":(_controller?"Håll B":"Håll E")+"  Frilägg inskriften";
                Panel(new Rect2(415,413,450,62),.94f);Centered(prompt,640,440,16,Gold);
                WorldBar(new Vector2(439,454),402,stone.Progress/Combat.ReadingDuration,Teal,5);
            }
        }
        if(_game.Phase==Phase.Extraction&&NVec.Distance(_game.Player,_game.ObjectivePosition)<100)
        {Panel(new Rect2(430,425,420,42),.95f);Centered(foes>0?"Slå tillbaka förföljarna":(_controller?"B":"E")+(_game.ExtendedJourney?"  Gå in i magasinet":"  Gå ombord"),640,453,17,Gold);}
        if((_radioTime>0 || _sound.Speaking) && Radio.ContainsKey(_radio))DrawRadio();
        else
        {
            if(_game.Phase==Phase.Discovery && NVec.Distance(_game.Player,Combat.ChartPosition)<100){Panel(new Rect2(430,526,420,52),.94f);Centered(_controller?"B  Undersök bronskartan":"E  Undersök bronskartan",640,558,19,Gold);}
            else if(_game.Elapsed<35&&(!_game.InCampaign||_campaignTextTime<=0)){Panel(new Rect2(282,552,716,43),.85f);Centered(_controller?"X Hugg · Y Tungt · A Undanmanöver · LB Parad":"WASD Gå · Mus Sikta · Vänster Hugg · Höger Tungt · Space Undan · Shift Parad",640,578,14,Muted);}
        }
        Text(_controller?"START Paus · BACK Fynd":"ESC Paus · R Fynd",new Vector2(24,608),12,Muted);
    }
    private void DrawObjectiveDirection()
    {
        var world=G(_game.ObjectivePosition);var p=world*Zoom+Offset;
        if(new Rect2(130,150,1020,295).HasPoint(p))return;
        var pin=new Vector2(Math.Clamp(p.X,58,1222),Math.Clamp(p.Y,150,390));
        var direction=(p-new Vector2(640,360)).Normalized();
        DrawCircle(pin,17,new Color(.025f,.025f,.02f,.85f));
        DrawLine(pin-direction*6,pin+direction*7,Gold,2,true);
        DrawLine(pin+direction*7,pin-direction.Rotated(.65f)*4,Gold,2,true);
        DrawLine(pin+direction*7,pin-direction.Rotated(-.65f)*4,Gold,2,true);
        Text(_game.Region!=Region.Quay?"NÄSTA FYND":_game.Phase==Phase.Extraction?(_game.ExtendedJourney?"MAGASINET":"BÅTEN"):"INSKRIFT",pin+new Vector2(-25,33),10,Gold);
    }
    private void DrawRadio()
    {
        var (speaker,text)=Radio[_radio];Panel(new Rect2(186,492,908,110),.97f);
        int portrait=speaker.StartsWith("RIKSAMIRAL")?0:speaker.StartsWith("ANTIKVARIE")?1:2;
        float cell=_radioPortraits.GetWidth()/3f;
        var portraitRect=new Rect2(195,501,92,92);
        if(portrait==0)DrawTextureRect(_ebbaPortrait,portraitRect,false);
        else DrawTextureRectRegion(_radioPortraits,portraitRect,new Rect2(portrait*cell,0,cell,_radioPortraits.GetHeight()));
        DrawRect(portraitRect,new Color(Gold,.65f),false,1);
        DrawLine(new Vector2(186,492),new Vector2(186,602),portrait==1?Teal:Gold,3);
        for(int i=0;i<7;i++){float height=_sound.Speaking?3+Mathf.Abs(Mathf.Sin(_clock*5+i*.9f))*9:2;DrawLine(new Vector2(1048+i*3,512-height/2),new Vector2(1048+i*3,512+height/2),new Color(Teal,.7f),1.5f);}
        Text(speaker,new Vector2(304,517),12,Gold);Wrapped(text,new Vector2(304,544),770,16,Pale,23);
    }
    private void DrawTitle()
    {
        Text("STORMAKT 3020    /    EPISOD II",new Vector2(78,119),14,Gold);
        Text("ATLANDS",new Vector2(72,204),67,Pale,true);Text("ARV",new Vector2(75,275),67,Pale,true);
        DrawLine(new Vector2(80,302),new Vector2(486,302),new Color(Gold,.6f),1);
        Wrapped("Det finns ett rike under riket.\nOch någon håller ännu dess hamnljus tända.",new Vector2(80,341),530,18,Muted,28);
        bool hasSave=System.IO.File.Exists(ProjectSettings.GlobalizePath("user://quay-save.json"));
        Button(new Rect2(80,430,355,49),hasSave?"Återuppta fältdagboken":"Gå i land",hasSave?"continue":"new",true);
        if(hasSave)Button(new Rect2(80,490,355,43),"Ny landstigning","new");
        Button(new Rect2(80,hasSave?544:490,171,43),"Inställningar","settings");Button(new Rect2(264,hasSave?544:490,171,43),"Avsluta","quit");
        Button(new Rect2(80,600,355,43),"Öva sabelduell","duel");
        Button(new Rect2(842,614,350,43),"Spela nästa del · åtta banor","atland");
        Text("VÄGEN TILL ATLAND",new Vector2(842,533),19,Gold,true);Button(new Rect2(842,554,350,43),"Spela Atlands port","port");
        Text("VÄGEN UNDER VATTNET  ·  SPELPROV 0.4",new Vector2(80,673),12,Muted);Text("WASD + mus  /  Handkontroll",new Vector2(970,673),12,Muted);
    }
    private void DrawBriefing()
    {
        Text("KRIGSRÅD  /  BLEKINGE",new Vector2(78,105),14,Gold);Text("Innan vi går i land",new Vector2(74,165),42,Pale,true);
        Wrapped("En stängd kaj håller expeditionen kvar i dimman. Karl tar sig fram till porten. Ebba kan avvara en del av skeppets förråd — välj vad som följer honom.",new Vector2(80,214),610,19,Muted,29);
        Choice(new Rect2(80,334,530,112),"Krut till örlogsbatteriet","F markerar ett nedslag framför Karl.\nSlår hårt mot grupper och indrivaren.","artillery",_order==Order.Artillery);
        Choice(new Rect2(80,464,530,112),"Förband till landstigningen","Två extra tinkturer och återkommande fältvård.\nGer längre uthållighet i närstriden.","medicine",_order==Order.Medicine);
        Button(new Rect2(80,613,350,49),"Gå i land","land",true);Button(new Rect2(448,613,162,49),"Tillbaka","back");
        Panel(new Rect2(802,344,397,232),.84f);Text("EBBA GRIPS ORDER",new Vector2(827,377),13,Gold);
        Wrapped("Bryt båda förtöjningssigillen.\nTysta vakterna.\nUndersök porten.\n\nOch kom tillbaka, Karl.",new Vector2(827,414),340,19,Pale,28);
    }
    private void DrawPause()
    {
        Centered("Fältdagboken",640,157,40,Pale,true);Centered("Striden väntar.",640,194,17,Muted);
        Button(new Rect2(455,249,370,49),"Fortsätt","resume",true);Button(new Rect2(455,310,370,49),"Spara fältdagboken","save");Button(new Rect2(455,371,370,49),"Inställningar","settings");Button(new Rect2(455,432,370,49),"Till huvudmenyn","title");
        Centered("WASD: gå   Mus: sikta   J / vänsterklick: hugg   K / högerklick: tungt",640,552,15,Muted);
        Centered("Space: undanmanöver   Shift: parad   Tab: vapen   Q: tinktur   E: undersök   F: understöd",640,581,14,Muted);
        Centered("Handkontroll: vänster spak gå · höger spak sikta · X hugg · Y tungt · A undan · LB parad",640,610,14,Muted);
        Centered("RB vapen · B undersök · styrkors upp tinktur / ned understöd · Start paus",640,637,14,Muted);
    }
    private void DrawSettings()
    {
        Centered("Inställningar",640,165,40,Pale,true);Centered($"Ljudvolym  {Math.Round(_volume*100)} %",640,239,20,Gold);
        Button(new Rect2(455,264,177,45),"Sänk","volume-");Button(new Rect2(648,264,177,45),"Höj","volume+");
        Button(new Rect2(455,329,370,49),"Kameraskakning: "+(_cameraShake?"på":"av"),"shake");
        Button(new Rect2(455,394,370,49),"Växla helskärm  ·  F11","fullscreen");Button(new Rect2(420,459,440,49),"Utvecklarläge: överlevnad "+(_developerSurvival?"PÅ":"AV"),"dev-survival");
        Centered("Karl tar skada men överlever på minst 1 liv.",640,534,15,Muted);
        Button(new Rect2(455,567,370,49),"Tillbaka","back",true);
        Centered("M tyst/ljud · +/− volym · dra reglaget uppe till höger",640,653,15,Gold);
        Centered("Undertexter visas alltid. Inställningarna sparas automatiskt.",640,684,15,Muted);
    }
    private void DrawDeath()
    {
        Centered("Havet väntar",640,235,46,Pale,true);Centered("Karl föll. Fältdagboken finns kvar.",640,279,18,Muted);
        Button(new Rect2(445,345,390,52),"Återvänd till sparat läge","retry",true);Button(new Rect2(445,414,390,49),"Till huvudmenyn","title");
    }
    private void DrawEnding()
    {
        if(_game.CampaignFinished){DrawCampaignEnding();return;}
        DrawRect(new Rect2(0,0,1280,720),new Color(.012f,.032f,.044f,.86f));
        Text(_game.Duel?"ÖVNING  /  SABEL":"FÄLTDAGBOK  /  FÖRSTA FYNDET",new Vector2(100,113),14,Gold);Text(_game.Duel?"DUELLEN VUNNEN":"ATLAND",new Vector2(94,200),64,Pale,true);
        bool found=_game.Testimony!=TestimonyChoice.None;
        string consequence=_game.Testimony==TestimonyChoice.Broadcast
            ?"Namnen hördes över hela redden. Nu bär fler deras berättelse."
            :"Avtrycken ligger förseglade ombord. Vittnena kom levande tillbaka.";
        Wrapped(_game.Duel?"Sabelvakten är besegrad. Öva paraden, välj avstånd och hugg i luckan efter hans anfall.":_game.AtlandRevealed?"Kartan och landskapet stämmer. En väg leder under vattnet mot en gammal port.\n\nKollegiets färska mätband sitter redan längs stranden. Någon skickade oss för att upptäcka en plats de redan kände till.":found?$"Ingrid Jonsdotter. Mats Eriksson. Siri Nilsdotter.\n{consequence}\n\nBronskartan följer Sveriges kust. Nästa märke ligger vid Uppsala.":"Kartan visar en kust som inte finns.\nI bronsen står ett namn som borde ha stannat i böckerna.\n\nLångt under kölen svarar något med tre långsamma slag.",new Vector2(100,254),1030,21,Muted,35);
        Text($"Vakter fällda  {_game.Kills}     Parader  {_game.Parries}     Tid  {(int)_game.Elapsed/60}:{(int)_game.Elapsed%60:00}",new Vector2(100,443),16,Gold);
        if(_radioTime>0 || _sound.Speaking)DrawRadio();
        Button(new Rect2(100,624,360,49),"Till huvudmenyn","title",true);
        if(!_game.Duel&&!_game.AtlandRevealed&&_game.Inscriptions.All(i=>i.Read))Button(new Rect2(510,563,440,49),"Fortsätt genom magasinet","journey");
        if(!_game.Duel&&_game.AtlandRevealed&&!_game.CampaignFinished)Button(new Rect2(510,563,440,49),"Fortsätt in i Atland","journey");
        Text(_game.Duel?"Övningen avslutad.":_game.AtlandRevealed?"Vägen är funnen. Expeditionen fortsätter.":"Landstigningen avslutad. Uppsala väntar.",new Vector2(510,655),17,Muted);
    }
    private void DrawTestimony()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.025f,.023f,.019f,.95f));
        Text("FÄLTDAGBOK  /  DE STRUKNA NAMNEN",new Vector2(70,53),13,Gold);
        Text("Vilka ska få höra?",new Vector2(66,107),40,Pale,true);
        Wrapped("Tre inskrifter. Tre människor som saknas i kronans berättelse. Ebba väntar vid radion.",new Vector2(70,143),1100,18,Muted,26);
        for(int i=0;i<3;i++)
        {
            float x=70+i*390;Panel(new Rect2(x,171,365,181),.92f);
            Text(FieldNotes.Names[i],new Vector2(x+18,202),15,Gold);
            Wrapped(FieldNotes.Stones[i],new Vector2(x+18,235),328,16,Pale,25);
        }
        Choice(new Rect2(70,373,555,115),"Sänd namnen öppet","Starkare förföljare. +30 liv nu.\nPerfekta parader återger 4 liv under återtåget.","broadcast",false);
        Choice(new Rect2(650,373,555,115),"Säkra en krypterad rapport","Färre förföljare. +15 liv och en tinktur.\nUnderstödet blir redo direkt.","cipher",false);
        if((_radioTime>0||_sound.Speaking)&&Radio.ContainsKey(_radio))DrawRadio();
        Wrapped("Namnen finns kvar i fältdagboken oavsett väg. Kartans nästa spår finns bortom magasinets port.",new Vector2(70,632),1100,16,Muted,24);
        Text("ESC Paus",new Vector2(70,682),12,Muted);
    }
    private void DrawJournal()
    {
        if(_game.InCampaign){DrawCampaignJournal();return;}
        DrawRect(new Rect2(0,0,1280,720),new Color(.025f,.023f,.019f,.96f));
        Text("FÄLTDAGBOK  /  VITTNESMÅL",new Vector2(95,78),13,Gold);
        if(_journalPage==3){DrawExpeditionJournal();return;}
        var stone=_game.Inscriptions[_journalPage];
        Text(stone.Read?FieldNotes.Names[_journalPage]:"En oläst inskrift",new Vector2(90,155),36,Pale,true);
        Text($"{_journalPage+1} / 4",new Vector2(1100,78),14,Gold);
        Text("STENENS VITTNESMÅL",new Vector2(95,216),13,Gold);
        Wrapped(stone.Read?FieldNotes.Stones[_journalPage]:"Skrift syns under kajens beslag. Frilägg inskriften för att läsa den.",new Vector2(95,255),1040,23,Pale,36);
        DrawLine(new Vector2(95,359),new Vector2(1170,359),new Color(Gold,.4f),1);
        Text("EBBAS JÄMFÖRELSE",new Vector2(95,399),13,Gold);
        Wrapped(stone.Read?FieldNotes.Ledger[_journalPage]:"Ingen jämförelse antecknad ännu.",new Vector2(95,440),1040,21,Muted,33);
        Text("Minne · tal · skrift",new Vector2(95,560),18,Gold,true);
        Button(new Rect2(95,614,210,49),"Föregående","journal-prev");
        Button(new Rect2(320,614,210,49),"Nästa","journal-next");
        Button(new Rect2(830,614,340,49),"Tillbaka","back",true);
    }
    private void Panel(Rect2 r,float alpha){DrawRect(r,new Color(.027f,.055f,.068f,alpha));DrawRect(r,new Color(.57f,.51f,.34f,.30f),false,1);}
    private void Button(Rect2 r,string label,string id,bool primary=false)
    {
        bool hover=r.HasPoint(GetGlobalMousePosition())||((_controller||_menuKeyboard) && _menuSelection==_buttons.Count);_buttons.Add((r,id));
        DrawRect(r,hover?new Color(.21f,.26f,.25f,.97f):primary?new Color(.15f,.20f,.20f,.98f):new Color(.04f,.075f,.09f,.94f));
        DrawRect(r,new Color(Gold,hover?.95f:primary?.64f:.28f),false,1);
        Text(label,r.Position+new Vector2(18,r.Size.Y/2+6),17,primary||hover?Gold:Pale);
        if(primary)Text("›",r.End-new Vector2(29,r.Size.Y/2-7),26,Gold);
    }
    private void Choice(Rect2 r,string title,string detail,string id,bool selected)
    {
        _buttons.Add((r,id));bool hover=r.HasPoint(GetGlobalMousePosition())||((_controller||_menuKeyboard)&&_menuSelection==_buttons.Count-1);
        Panel(r,.95f);DrawRect(r,new Color(selected?Teal:Gold,selected?.8f:hover?.7f:.25f),false,selected?2:1);
        Text(selected?"◆":"◇",r.Position+new Vector2(20,36),21,selected?Teal:Muted);Text(title,r.Position+new Vector2(53,34),20,Pale,true);Wrapped(detail,r.Position+new Vector2(53,62),450,15,Muted,23);
    }
    private void Text(string text,Vector2 at,int size,Color color,bool serif=false)=>DrawString(serif?_serif:_sans,at,text,HorizontalAlignment.Left,-1,size,color);
    private void Centered(string text,float x,float y,int size,Color color,bool serif=false){var f=serif?_serif:_sans;Text(text,new Vector2(x-f.GetStringSize(text,HorizontalAlignment.Left,-1,size).X/2,y),size,color,serif);}
    private void Wrapped(string text,Vector2 p,float width,int size,Color color,float lineHeight)
    {
        foreach(var paragraph in text.Split('\n'))
        {string line="";foreach(var word in paragraph.Split(' ')){string next=line==""?word:line+" "+word;if(_sans.GetStringSize(next,HorizontalAlignment.Left,-1,size).X>width && line!=""){Text(line,p,size,color);p.Y+=lineHeight;line=word;}else line=next;}Text(line,p,size,color);p.Y+=lineHeight;}
    }
    private void WorldBar(Vector2 p,float width,float amount,Color color,float height=4){DrawRect(new Rect2(p,new Vector2(width,height)),new Color(.015f,.028f,.034f,.9f));DrawRect(new Rect2(p,new Vector2(width*Math.Clamp(amount,0,1),height)),color);}
    public override void _ExitTree()
    {
        foreach(var texture in _campaignWorlds)texture?.Dispose();
        _cast?.Dispose();_animated?.Dispose();_warehouse?.Dispose();if(_shoreRevealed!=_shore)_shoreRevealed?.Dispose();_shore?.Dispose();
        _portProps?.Dispose();
        _background?.Dispose();_radioPortraits?.Dispose();_ebbaPortrait?.Dispose();_serif?.Dispose();_sans?.Dispose();
    }
}
