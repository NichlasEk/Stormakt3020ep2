using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;
using NVec=System.Numerics.Vector2;

public partial class Main : Node2D
{
    private enum Screen { Title, Briefing, Game, Pause, Settings, Death, Ending }
    private Screen _screen=Screen.Title;
    private Screen _settingsReturn=Screen.Title;
    private Combat _game=Combat.New(Order.Artillery);
    private Order _order;
    private Font _serif=null!,_sans=null!;
    private Texture2D _background=null!;
    private readonly Dictionary<string,Texture2D> _actors=new();
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
    private float _volume=.75f;
    private bool _cameraShake=true;
    private bool _testMode;
    private bool _smokeCapture;
    private bool _integration;
    private bool _bossCaptured;
    private bool _capturePending;
    private int _testTicks;
    private string SavePath=>ProjectSettings.GlobalizePath("user://quay-save.json");
    private string ManualPath=>ProjectSettings.GlobalizePath("user://manual-save.json");
    private const float Zoom=1.12f;
    private static readonly Color Gold=new("b99a64"), Pale=new("ddd6c5"), Muted=new("9b9a8d"), Teal=new("93aaa0"), Red=new("c57761");
    private static readonly Dictionary<string,(string Speaker,string Text)> Radio=new()
    {
        ["arrival"]=("RIKSAMIRAL EBBA GRIP","Karl. Två sigill håller kajen stängd. Bryt dem. Och håll ett öga på skytten vid porten."),
        ["cannon"]=("RIKSAMIRAL EBBA GRIP","Batteriet svarar. Håll undan från nedslaget."),
        ["collector"]=("VARVETS INDRIVARE","Fyra män. Två brutna sigill. Er skuld växer, Karl. Jag tar betalningen personligen."),
        ["rage"]=("VARVETS INDRIVARE","Även havet står i skuld till kronan."),
        ["fallen"]=("RIKSAMIRAL EBBA GRIP","Vänta. Det ligger något vid porten. En karta, gjuten i brons. Den hör inte hemma här."),
        ["atland"]=("RIKSAMIRAL EBBA GRIP","Atland. Det är vad Rudbeck kallade det. Karl, ta kartan ombord. Vi måste tala ostört.")
    };
    private sealed class Particle {public Vector2 P,V;public float Life,Max;public Color C;public float Size;}
    private sealed class Floating {public Vector2 P;public string Text="";public float Life=1;public Color C;}
    private static Vector2 G(NVec v)=>new(v.X,v.Y);
    private static NVec N(Vector2 v)=>new(v.X,v.Y);
    public override void _Ready()
    {
        _serif=GD.Load<Font>("res://assets/fonts/NotoSerif-Regular.ttf");_sans=GD.Load<Font>("res://assets/fonts/NotoSans-Regular.ttf");
        _background=GD.Load<Texture2D>("res://assets/art/likvarvet-oil-v2.png");
        foreach(string kind in new[]{"karl-saber","karl-hammer","guard","pikeman","gunner","collector"})_actors[kind]=GD.Load<Texture2D>($"res://assets/art/{kind}.png");
        _sound=new Soundscape();AddChild(_sound);LoadSettings();
        GetWindow().MinSize=new Vector2I(960,540);
        var args=OS.GetCmdlineUserArgs();_integration=args.Contains("--integration");_testMode=args.Contains("--smoke")||_integration;
        if(_integration)Engine.TimeScale=3;
        if(_testMode)StartNew();
        if(args.Contains("--capture-title"))_smokeCapture=true;
    }
    public override void _Input(InputEvent ev)
    {
        if(ev is InputEventMouseMotion){_controller=false;_menuKeyboard=false;}
        if(ev is InputEventJoypadMotion motion && Math.Abs(motion.AxisValue)>.25f)_controller=true;
        if(ev is InputEventKey key && key.Pressed && !key.Echo)
        {
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
            switch(jb.ButtonIndex)
            {case JoyButton.X:_attack=true;break;case JoyButton.Y:_heavy=true;break;case JoyButton.A:_dodge=true;break;case JoyButton.B:_interact=true;break;case JoyButton.RightShoulder:_swap=true;break;case JoyButton.DpadUp:_heal=true;break;case JoyButton.DpadDown:_support=true;break;}
        }
    }
    private void SelectMenu(int step){_menuKeyboard=true;_menuSelection=(_menuSelection+step+Math.Max(1,_buttons.Count))%Math.Max(1,_buttons.Count);}
    private void ActivateSelected(){if(_buttons.Count>0)Activate(_buttons[Math.Clamp(_menuSelection,0,_buttons.Count-1)].Id);}
    private void ChangeScreen(Screen screen){_screen=screen;_menuSelection=0;ClearPresses();_sound.PauseVoice(screen!=Screen.Game && screen!=Screen.Ending);}
    private void Back()
    {
        if(_screen==Screen.Game)ChangeScreen(Screen.Pause);
        else if(_screen==Screen.Pause)ChangeScreen(Screen.Game);
        else if(_screen==Screen.Settings)ChangeScreen(_settingsReturn);
        else if(_screen==Screen.Briefing)ChangeScreen(Screen.Title);
        else if(_screen==Screen.Title)GetTree().Quit();
    }
    private void Activate(string id)
    {
        switch(id)
        {
            case "new":ChangeScreen(Screen.Briefing);break;
            case "continue":ResumeSave();break;
            case "artillery":_order=Order.Artillery;break;
            case "medicine":_order=Order.Medicine;break;
            case "land":StartNew();break;
            case "resume":ChangeScreen(Screen.Game);break;
            case "save":Save(true);break;
            case "settings":_settingsReturn=_screen;ChangeScreen(Screen.Settings);break;
            case "volume-":_volume=Math.Max(0,_volume-.1f);SaveSettings();break;
            case "volume+":_volume=Math.Min(1,_volume+.1f);SaveSettings();break;
            case "shake":_cameraShake=!_cameraShake;SaveSettings();break;
            case "fullscreen":DisplayServer.WindowSetMode(DisplayServer.WindowGetMode()==DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);break;
            case "back":Back();break;
            case "title":_sound.StopVoice();_radioQueue.Clear();_radio="";ChangeScreen(Screen.Title);break;
            case "retry":if(System.IO.File.Exists(SavePath))ResumeSave();else StartNew();break;
            case "quit":GetTree().Quit();break;
        }
    }
    private void StartNew()
    {
        _game=Combat.New(_order);_camera=G(_game.Player)+new Vector2(85,-80);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        ChangeScreen(Screen.Game);_banner="BLEKINGES LIKVARV";_bannerTime=5;Save();
    }
    private void Save(bool manual=false)
    {
        if(_testMode)return;
        try{SaveStore.Write(manual?ManualPath:SavePath,_game);Notice(manual?"Manuellt läge sparat  ·  F9 laddar":"Kontrollpunkt sparad");}catch(Exception e){GD.PushError(e.Message);Notice("Kunde inte spara fältdagboken");}
    }
    private void ResumeSave(bool manual=false)
    {
        try
        {
            _game=SaveStore.Read(manual?ManualPath:SavePath);_camera=G(_game.Player)+new Vector2(85,-80);_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
            ChangeScreen(_game.Dead?Screen.Death:_game.Phase==Phase.Complete?Screen.Ending:Screen.Game);Notice("Fältdagboken återupptagen");
        }
        catch(Exception e){GD.PushWarning(e.Message);Notice("Sparfilen kunde inte läsas. Du kan starta en ny landstigning.");}
    }
    private void Notice(string text){_notice=text;_noticeTime=4;}
    private void LoadSettings()
    {
        var config=new ConfigFile();if(config.Load("user://settings.cfg")==Error.Ok){_volume=Math.Clamp((float)config.GetValue("audio","volume",.75f),0,1);_cameraShake=(bool)config.GetValue("display","shake",true);}_sound.Volume=_volume;
    }
    private void SaveSettings(){_sound.Volume=_volume;var c=new ConfigFile();c.SetValue("audio","volume",_volume);c.SetValue("display","shake",_cameraShake);c.Save("user://settings.cfg");}
    private void ClearPresses(){_attack=_heavy=_dodge=_swap=_heal=_support=_interact=false;}
    public override void _PhysicsProcess(double delta)
    {
        if(_screen!=Screen.Game)return;
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
            NVec goal=target?.Position??_game.Seals.FirstOrDefault(s=>s.Health>0)?.Position??Combat.ChartPosition;
            var d=goal-_game.Player;float distance=d.Length();
            guard=target!=null&&target.State==1&&target.Timer<.16f&&distance<150;
            move=distance>60?G(Combat.Normal(d,NVec.UnitX)):Vector2.Zero;aim=G(d);
            _attack=distance<95&&!guard;_heavy=distance<105&&(_testTicks-1)%47==0&&!guard;
            _heal=_game.Health<48;_support=target!=null;_interact=true;
        }
        var controls=new Controls(N(move),N(aim),_attack||(!_controller && Input.IsMouseButtonPressed(MouseButton.Left)),_heavy,_dodge,guard,_swap,_heal,_support,_interact);
        _game.Step(controls);ClearPresses();
        foreach(var cue in _game.Events)HandleCue(cue);
        if(_game.Dead)ChangeScreen(Screen.Death);
        if(_game.Phase==Phase.Complete)ChangeScreen(Screen.Ending);
    }
    private void HandleCue(Cue cue)
    {
        var p=G(cue.Position);
        switch(cue.Kind)
        {
            case "radio":if(!_radioQueue.Contains(cue.Text))_radioQueue.Enqueue(cue.Text);break;
            case "checkpoint":Save();break;
            case "hit":Burst(p,Gold,12,100);_floating.Add(new(){P=p+new Vector2(0,-70),Text=cue.Text,C=Gold});_sound.Play("hit",.94f+(float)(_game.Tick%6)*.025f);_shake=3;break;
            case "hurt":Burst(p,Red,13,100);_sound.Play("hit",.65f);_shake=7;break;
            case "parry":Burst(p,Teal,30,190);_floating.Add(new(){P=p-new Vector2(0,110),Text=cue.Text,C=Teal});_sound.Play("parry");_shake=6;break;
            case "swing":_sound.Play(cue.Value>0?"hammer":"swing");break;
            case "shot":_sound.Play("shot");Burst(p,Gold,7,90);break;
            case "seal":Burst(p,Teal,45,210);_sound.Play("seal");_floating.Add(new(){P=p-new Vector2(0,60),Text=cue.Text,C=Teal});break;
            case "sealhit":Burst(p,Teal,12,100);_sound.Play("hit");break;
            case "heal":Burst(p,Teal,24,70);_sound.Play("heal");_floating.Add(new(){P=p-new Vector2(0,80),Text=cue.Text,C=Teal});break;
            case "cannon":case "slam":Burst(p,Gold,60,260);_sound.Play("cannon");_shake=12;break;
            case "death":Burst(p,Muted,22,110);_sound.Play("death");break;
            case "dodge":Burst(p,new Color(.45f,.57f,.6f,.5f),8,30);_sound.Play("dodge");break;
            case "block":_sound.Play("parry",.7f);if(cue.Text!="")_floating.Add(new(){P=p-new Vector2(0,80),Text=cue.Text,C=Muted});break;
        }
    }
    private void Burst(Vector2 p,Color c,int count,float speed)
    {
        for(int i=0;i<count;i++){float a=i*2.39996f+_clock;float life=.25f+(i%7)*.07f;_particles.Add(new(){P=p+new Vector2(0,-25),V=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*speed*(.3f+(i%5)*.17f),Life=life,Max=life,C=c,Size=1+(i%3)});}
    }
    public override void _Process(double delta)
    {
        float dt=(float)delta;_clock+=dt;_noticeTime=Math.Max(0,_noticeTime-dt);
        _sound.Boss=_game.Phase==Phase.Collector && _screen is Screen.Game or Screen.Pause;
        if(_screen is Screen.Game or Screen.Ending)
        {
            _bannerTime=Math.Max(0,_bannerTime-dt);_shake=Math.Max(0,_shake-dt*22);
            if(_screen==Screen.Game)
            {
                var target=G(_game.Player)+new Vector2(70,-70);target.X=Math.Clamp(target.X,610,990);target.Y=Math.Clamp(target.Y,535,740);
                _camera=_camera.Lerp(target,1-Mathf.Exp(-dt*5));
            }
            if(_radioTime>0)_radioTime-=dt;
            if(_radioTime<=0 && !_sound.Speaking && _radioQueue.Count>0){_radio=_radioQueue.Dequeue();_radioTime=_radio=="atland"?11:8;_sound.Speak(_radio);}
            foreach(var p in _particles){p.P+=p.V*dt;p.V*=Mathf.Exp(-dt*3);p.Life-=dt;}
            _particles.RemoveAll(p=>p.Life<=0);
            foreach(var f in _floating){f.P.Y-=dt*25;f.Life-=dt;}_floating.RemoveAll(f=>f.Life<=0);
        }
        QueueRedraw();
        if(_integration)
        {
            if(_game.Phase==Phase.Collector&&!_bossCaptured){_bossCaptured=true;CaptureFrame("boss.png",false);}
            if(_game.Phase==Phase.Complete&&!_capturePending){_capturePending=true;GD.Print($"INTEGRATION COMPLETE health={_game.Health} kills={_game.Kills} parries={_game.Parries}");CaptureFrame("ending.png",true);}
            if(_game.Dead||_game.Elapsed>300){GD.PushError("Integration encounter failed");GetTree().Quit(1);}
        }
        else if((_testMode && _testTicks>125)||(_smokeCapture && _clock>1.5f))CaptureAndQuit();
    }
    private async void CaptureFrame(string name,bool quit)
    {
        await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        var path=ProjectSettings.GlobalizePath("res://artifacts/"+name);GetViewport().GetTexture().GetImage().SavePng(path);GD.Print("CAPTURE "+path);if(quit)GetTree().Quit();
    }
    private async void CaptureAndQuit()
    {
        if(!_testMode&&!_smokeCapture)return;_testMode=false;_smokeCapture=false;
        await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        var path=ProjectSettings.GlobalizePath("res://artifacts/"+(_screen==Screen.Title?"title.png":"gameplay.png"));
        GetViewport().GetTexture().GetImage().SavePng(path);GD.Print("CAPTURE "+path);GetTree().Quit();
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
        else
        {
            DrawRect(new Rect2(0,0,1280,720),new Color(.015f,.03f,.043f,.88f));
            if(_screen==Screen.Pause)DrawPause();else if(_screen==Screen.Settings)DrawSettings();else DrawDeath();
        }
        if(_noticeTime>0){Panel(new Rect2(350,16,580,40),.96f);Text(_notice,new Vector2(370,43),16,Teal);}
        if(_screen==Screen.Game && !_controller)
        {var p=GetGlobalMousePosition();DrawArc(p,7,0,Mathf.Tau,20,new Color(1,.87f,.61f,.7f),1,true);DrawLine(p-new Vector2(11,0),p-new Vector2(5,0),Gold,1,true);DrawLine(p+new Vector2(5,0),p+new Vector2(11,0),Gold,1,true);}
    }
    private void DrawWorld()
    {
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        DrawTextureRectRegion(_background,new Rect2(18,30,1500,946),new Rect2(18,30,1500,946),new Color(.87f,.85f,.80f));
        // Sea glints and drifting motes remain decorative and never obscure attack warnings.
        for(int i=0;i<25;i++)
        {float x=50+(i*19)%240;float y=330+(i*17)%200;var p=new Vector2(x+Mathf.Sin(_clock*.23f+i)*5,y);if(!Combat.OnGround(N(p)))DrawLine(p,p+new Vector2(12+Mathf.Sin(_clock+i)*4,-2),new Color(.54f,.58f,.52f,.07f),1.4f,true);}
        foreach(var seal in _game.Seals)
        {
            var p=G(seal.Position);bool alive=seal.Health>0;var c=alive?Teal:Muted;
            DrawArc(p,31,0,Mathf.Tau,60,new Color(c,alive?.75f:.25f),2,true);
            if(alive){DrawArc(p,38,_clock*.4f,_clock*.4f+Mathf.Pi*1.5f,40,new Color(Teal,.45f),1.5f,true);DrawLine(p-new Vector2(0,10),p-new Vector2(0,49),Teal,2,true);DrawLine(p-new Vector2(0,42),p+new Vector2(12,-32),Teal,2,true);DrawLine(p-new Vector2(0,30),p+new Vector2(-12,-20),Teal,2,true);WorldBar(p+new Vector2(-24,15),48,seal.Health/80,Teal);}
        }
        foreach(var h in _game.Hazards)
        {var c=h.Friendly?Gold:Red;DrawCircle(G(h.Position),h.Radius,new Color(c,.10f));DrawArc(G(h.Position),h.Radius,0,Mathf.Tau,70,c,2,true);DrawArc(G(h.Position),h.Radius*Math.Clamp(1-h.Timer/1.5f,0,1),0,Mathf.Tau,60,new Color(c,.5f),2,true);}
        foreach(var e in _game.Enemies.Where(e=>!e.Dead && e.State==1))DrawTelegraph(e);
        foreach(var e in _game.Enemies.Where(e=>e.Dead))Actor(e,true);
        var sorted=_game.Enemies.Where(e=>!e.Dead).OrderBy(e=>e.Position.Y).ToList();bool playerDrawn=false;
        foreach(var e in sorted){if(!playerDrawn && _game.Player.Y<e.Position.Y){DrawPlayer();playerDrawn=true;}Actor(e,false);}if(!playerDrawn)DrawPlayer();
        foreach(var shot in _game.Shots){var p=G(shot.Position);var v=G(shot.Velocity).Normalized();DrawLine(p-v*17,p,shot.Reflected?Teal:Gold,3,true);DrawCircle(p,3,Pale);}
        if(_game.Phase>=Phase.Discovery)
        {var p=G(Combat.ChartPosition);DrawCircle(p,30,new Color(Gold,.13f));DrawArc(p,25,0,Mathf.Tau,40,Gold,1.5f,true);DrawRect(new Rect2(p-new Vector2(14,18),new Vector2(28,24)),Gold);DrawLine(p-new Vector2(10,4),p+new Vector2(10,-10),new Color(.15f,.23f,.24f),2,true);}
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
        DrawActor(kind,G(e.Position),G(e.Facing),pose,e.Hurt,dead);
        if(!dead && e.Health<e.MaxHealth)WorldBar(G(e.Position)+new Vector2(-24,-111),48,e.Health/e.MaxHealth,e.Kind==EnemyKind.Collector?Gold:Red);
    }
    private void DrawPlayer()
    {
        int pose=_game.Guarding?5:_game.AttackTime>0?(_game.AttackContact?4:3):_game.Moving?(int)_game.Walk%2+1:0;
        DrawActor(_game.Weapon==Weapon.Saber?"karl-saber":"karl-hammer",G(_game.Player),G(_game.Facing),pose,_game.Hurt,false,true);
        if(_game.AttackTime>0 && _game.AttackContact)
        {
            float a=G(_game.Facing).Angle();float r=_game.Weapon==Weapon.Hammer?97:89;
            DrawArc(G(_game.Player)+new Vector2(0,-25),r,a-1.1f,a+1.1f,25,new Color(_game.HeavyAttack?Gold:Pale,.55f),_game.Weapon==Weapon.Hammer?5:3,true);
        }
        if(_game.Guarding)DrawArc(G(_game.Player)+new Vector2(0,-34),39,G(_game.Facing).Angle()-.9f,G(_game.Facing).Angle()+.9f,24,_game.GuardTime<.22f?Teal:Muted,3,true);
    }
    private void DrawActor(string kind,Vector2 p,Vector2 facing,int pose,float hurt,bool dead,bool player=false)
    {
        int direction=((int)Mathf.Round(Mathf.Atan2(facing.X,facing.Y)/(Mathf.Pi/4))+8)%8;
        var shadow=new Vector2[24];for(int i=0;i<24;i++)shadow[i]=p+new Vector2(Mathf.Cos(i*Mathf.Tau/24)*25,Mathf.Sin(i*Mathf.Tau/24)*10);
        DrawColoredPolygon(shadow,new Color(.01f,.02f,.025f,dead?.2f:.42f));
        if(player)DrawArc(p,23,0,Mathf.Tau,40,new Color(Teal,.5f),1.5f,true);
        var rect=new Rect2(p-new Vector2(83,151),new Vector2(166,208));
        var tint=dead?new Color(.5f,.55f,.57f,.8f):hurt>0?new Color(1.8f,1.6f,1.3f):Colors.White;
        DrawTextureRectRegion(_actors[kind],rect,new Rect2(direction*256,pose*320,256,320),tint);
    }
    private void DrawHud()
    {
        Panel(new Rect2(20,18,294,60),.87f);Text("STORMAKT 3020",new Vector2(38,41),12,Gold);Text("Blekinges likvarv",new Vector2(38,65),20,Pale,true);
        Panel(new Rect2(928,18,332,102),.91f);Text("LANDSTIGNING  /  01",new Vector2(946,42),12,Gold);
        string objective=_game.Phase==Phase.Quay?$"Bryt kajens sigill  ·  {_game.Seals.Count(s=>s.Health<=0)}/2":_game.Phase==Phase.Collector?"Besegra varvets indrivare":"Undersök bronskartan vid porten";
        Text(objective,new Vector2(946,69),17,Pale);Text(_game.Phase==Phase.Quay?$"Vakter kvar: {_game.Enemies.Count(e=>!e.Dead)}":"Håll dig i rörelse. Läs upptakterna.",new Vector2(946,96),14,Muted);
        var boss=_game.Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.Collector&&!e.Dead);
        if(boss!=null){Panel(new Rect2(354,20,542,59),.91f);Centered("VARVETS INDRIVARE",625,42,14,Gold);WorldBar(new Vector2(378,56),490,boss.Health/boss.MaxHealth,Red);}
        Panel(new Rect2(20,623,381,77),.96f);Text("KARL CCLV",new Vector2(38,646),13,Gold);Text($"{Math.Ceiling(_game.Health)} / 100",new Vector2(302,646),13,Pale);
        WorldBar(new Vector2(38,657),345,_game.Health/100,new Color("b6574d"),11);WorldBar(new Vector2(38,677),345,_game.Stamina/100,Teal,5);
        Panel(new Rect2(417,623,470,77),.96f);Text(_game.Weapon==Weapon.Saber?"OFFICERSSABEL":"GRUVHAMMARE",new Vector2(435,647),16,Gold);
        Text(_controller?"RB  Byt vapen     ↑  Tinktur ×"+_game.Potions:"TAB  Byt vapen     Q  Tinktur ×"+_game.Potions,new Vector2(435,674),14,Muted);
        Panel(new Rect2(903,623,357,77),.96f);Text(_game.Order==Order.Artillery?"ÖRLOGSBATTERI":"FÄLTSJUKVÅRD",new Vector2(921,647),14,Gold);
        Text(_game.SupportCooldown<=0?(_controller?"↓  Understöd redo":"F  Understöd redo"):$"Redo om {Math.Ceiling(_game.SupportCooldown)} s",new Vector2(921,674),16,_game.SupportCooldown<=0?Teal:Muted);
        if(_bannerTime>0)
        {float alpha=Math.Clamp(Math.Min(_bannerTime,5-_bannerTime),0,1);Centered(_banner,640,180,32,new Color(Pale,alpha),true);Centered("En kust som inte längre räknar sina döda.",640,211,16,new Color(Muted,alpha));}
        if((_radioTime>0 || _sound.Speaking) && Radio.ContainsKey(_radio))DrawRadio();
        else
        {
            if(_game.Phase==Phase.Discovery && NVec.Distance(_game.Player,Combat.ChartPosition)<100){Panel(new Rect2(430,526,420,52),.94f);Centered(_controller?"B  Undersök bronskartan":"E  Undersök bronskartan",640,558,19,Gold);}
            else if(_game.Elapsed<35){Panel(new Rect2(282,552,716,43),.85f);Centered(_controller?"X Hugg · Y Tungt · A Undanmanöver · LB Parad":"WASD Gå · Mus Sikta · Vänster Hugg · Höger Tungt · Space Undan · Shift Parad",640,578,14,Muted);}
        }
        Text(_controller?"START  Paus":"ESC  Paus",new Vector2(24,608),12,Muted);
    }
    private void DrawRadio()
    {
        var (speaker,text)=Radio[_radio];Panel(new Rect2(246,492,788,102),.95f);
        DrawLine(new Vector2(246,492),new Vector2(246,594),Gold,3);
        for(int i=0;i<13;i++){float height=5+Mathf.Abs(Mathf.Sin(_clock*5+i*.9f))*18;DrawLine(new Vector2(269+i*3,549-height/2),new Vector2(269+i*3,549+height/2),new Color(Teal,.7f),1.5f);}
        Text(speaker,new Vector2(324,517),12,Gold);Wrapped(text,new Vector2(324,544),674,16,Pale,23);
    }
    private void DrawTitle()
    {
        Text("STORMAKT 3020    /    EPISOD II",new Vector2(78,119),14,Gold);
        Text("ATLANDS",new Vector2(72,204),67,Pale,true);Text("ARV",new Vector2(75,275),67,Pale,true);
        DrawLine(new Vector2(80,302),new Vector2(486,302),new Color(Gold,.6f),1);
        Wrapped("Det finns ett rike under riket.\nOch någon håller ännu dess hamnljus tända.",new Vector2(80,341),530,18,Muted,28);
        bool hasSave=System.IO.File.Exists(SavePath);
        Button(new Rect2(80,430,355,49),hasSave?"Återuppta fältdagboken":"Gå i land",hasSave?"continue":"new",true);
        if(hasSave)Button(new Rect2(80,490,355,43),"Ny landstigning","new");
        Button(new Rect2(80,hasSave?544:490,171,43),"Inställningar","settings");Button(new Rect2(264,hasSave?544:490,171,43),"Avsluta","quit");
        Text("BLEKINGES LIKVARV",new Vector2(842,533),19,Gold,true);Wrapped("En förlorad kaj. Två sigill.\nEtt fynd som ingen karta tillåter.",new Vector2(842,564),350,16,Pale,26);
        Text("FÖRSTA LANDSTIGNINGEN  ·  SPELPROV 0.1",new Vector2(80,673),12,Muted);Text("WASD + mus  /  Handkontroll",new Vector2(970,673),12,Muted);
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
        Button(new Rect2(455,394,370,49),"Växla helskärm  ·  F11","fullscreen");Button(new Rect2(455,475,370,49),"Tillbaka","back",true);
        Centered("Undertexter visas alltid. Inställningarna sparas automatiskt.",640,575,15,Muted);
    }
    private void DrawDeath()
    {
        Centered("Havet väntar",640,235,46,Pale,true);Centered("Karl föll. Fältdagboken finns kvar.",640,279,18,Muted);
        Button(new Rect2(445,345,390,52),"Återvänd till sparat läge","retry",true);Button(new Rect2(445,414,390,49),"Till huvudmenyn","title");
    }
    private void DrawEnding()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.012f,.032f,.044f,.86f));
        Text("FÄLTDAGBOK  /  FÖRSTA FYNDET",new Vector2(100,113),14,Gold);Text("ATLAND",new Vector2(94,200),64,Pale,true);
        Wrapped("Kartan visar en kust som inte finns.\nI bronsen står ett namn som borde ha stannat i böckerna.\n\nLångt under kölen svarar något med tre långsamma slag.",new Vector2(100,254),880,22,Muted,35);
        Text($"Vakter fällda  {_game.Kills}     Parader  {_game.Parries}     Tid  {(int)_game.Elapsed/60}:{(int)_game.Elapsed%60:00}",new Vector2(100,443),16,Gold);
        if(_radioTime>0 || _sound.Speaking)DrawRadio();
        Button(new Rect2(100,624,360,49),"Tillbaka till expeditionen","title",true);
        Text("Fortsättningen börjar vid nästa kust.",new Vector2(510,655),17,Muted);
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
        foreach(var texture in _actors.Values)texture.Dispose();_actors.Clear();
        _background?.Dispose();_serif?.Dispose();_sans?.Dispose();
    }
}
