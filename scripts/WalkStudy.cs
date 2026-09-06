using Godot;
using System;
using System.Linq;

/// In-engine motion review over the approved shore at the actual gameplay scale.
public partial class WalkStudy : Node2D
{
    private Texture2D _shore=null!;
    private BakedKarl _karl=null!;
    private Font _font=null!;
    private double _time;
    private float _distance;
    private int _direction;
    private bool _paused,_capture,_capturing;
    private const float Zoom=1.12f;
    private static readonly Vector2 Offset=new(640-768*Zoom,360-680*Zoom);
    private static readonly Vector2[] Directions={new(1,1),new(1,-1),new(-1,-1),new(-1,1)};
    public override void _Ready()
    {
        _shore=GD.Load<Texture2D>("res://assets/art/shore-v1.png");_karl=new BakedKarl();
        _font=GD.Load<Font>("res://assets/fonts/NotoSans-Regular.ttf");
        _capture=OS.GetCmdlineUserArgs().Contains("--capture-walk");
    }
    public override void _Process(double delta)
    {
        _time+=delta;
        if(!_paused&&!_capturing)_distance=(_distance+(float)delta*130)%(BakedKarl.CycleDistance*3);
        QueueRedraw();if(_capture&&!_capturing&&_time>1){_capturing=true;Capture();}
    }
    public override void _Input(InputEvent ev)
    {
        if(ev is not InputEventKey {Pressed:true,Echo:false} key)return;
        if(key.PhysicalKeycode==Key.Escape)GetTree().Quit();
        if(key.PhysicalKeycode==Key.Space)_paused=!_paused;
        if(key.PhysicalKeycode==Key.Right)_direction=(_direction+1)%4;
        if(key.PhysicalKeycode==Key.Left)_direction=(_direction+3)%4;
        if(key.PhysicalKeycode==Key.F11)DisplayServer.WindowSetMode(DisplayServer.WindowGetMode()==DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);
    }
    public override void _Draw()
    {
        if(_shore==null)return;
        DrawSetTransform(Offset,0,Vector2.One*Zoom);DrawTextureRect(_shore,new Rect2(0,0,1536,1024),false);
        var feet=new Vector2(768,710)+Directions[_direction].Normalized()*(_distance-BakedKarl.CycleDistance*1.5f);
        var shadow=new Vector2[24];for(int i=0;i<24;i++)shadow[i]=feet+new Vector2(Mathf.Cos(i*Mathf.Tau/24)*22,Mathf.Sin(i*Mathf.Tau/24)*8);
        DrawColoredPolygon(shadow,new Color(.01f,.015f,.012f,.42f));
        _karl.Draw(this,feet,_direction,_distance,Offset,Zoom);DrawSetTransform(Vector2.Zero);
        DrawRect(new Rect2(20,18,690,74),new Color(.025f,.034f,.035f,.95f));
        DrawString(_font,new Vector2(38,46),"KARL · MÅLAT 2D-GÅNGPROV",fontSize:20,modulate:new Color("c1ad88"));
        DrawString(_font,new Vector2(38,72),"30 renderade bilder per riktning · spelets kameraskala",fontSize:16,modulate:new Color("b1ada5"));
        DrawRect(new Rect2(20,656,1080,46),new Color(.025f,.034f,.035f,.95f));
        DrawString(_font,new Vector2(38,686),"← → riktning · Mellanslag pausa · F11 helskärm · Esc avsluta · Utseende under arbete",fontSize:17,modulate:new Color("c1ad88"));
    }
    private async void Capture()
    {
        DirAccess.MakeDirRecursiveAbsolute("res://artifacts/shore-walk");
        for(int direction=0;direction<4;direction++)for(int frame=0;frame<60;frame++)
        {
            _direction=direction;_distance=BakedKarl.CycleDistance*(.5f+frame/30f);QueueRedraw();
            await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using var image=GetViewport().GetTexture().GetImage();
            if(image.SavePng($"res://artifacts/shore-walk/{direction}-{frame:000}.png")!=Error.Ok)throw new InvalidOperationException("Cannot save shore walk capture");
        }
        GD.Print("SHORE WALK CAPTURE PASS · four directions · 240 frames");GetTree().Quit();
    }
    public override void _ExitTree(){_shore?.Dispose();_karl?.Dispose();_font?.Dispose();}
}
