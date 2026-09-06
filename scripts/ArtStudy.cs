using Godot;
using System;
using System.Linq;

// Same painted actors, scale and camera as the playable encounter.
public partial class ArtStudy : Node2D
{
    private Texture2D _quay=null!;
    private PaintedCast _cast=null!;
    private Font _font=null!;
    private int _pose;
    private bool _capture,_capturing,_ruler;
    private double _elapsed;
    private const float Zoom=1.12f;
    private static readonly Vector2 Offset=new(640-730*Zoom,360-700*Zoom);
    public override void _Ready()
    {
        _quay=GD.Load<Texture2D>("res://assets/art/likvarvet-scale-v5.png");
        _cast=new PaintedCast();
        _capture=OS.GetCmdlineUserArgs().Contains("--capture-study");
        _ruler=OS.GetCmdlineUserArgs().Contains("--scale-guide");
        _font=GD.Load<Font>("res://assets/fonts/NotoSans-Regular.ttf");
    }
    public override void _Input(InputEvent ev)
    {
        if(ev is not InputEventKey {Pressed:true,Echo:false} k)return;
        if(k.PhysicalKeycode==Key.Escape){GetTree().Quit();return;}
        if(k.PhysicalKeycode==Key.Space)_pose=(_pose+1)%3;
        if(k.PhysicalKeycode==Key.G)_ruler=!_ruler;
        if(k.PhysicalKeycode==Key.F11)DisplayServer.WindowSetMode(DisplayServer.WindowGetMode()==DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);
        QueueRedraw();
    }
    public override void _Process(double delta)
    {
        _elapsed+=delta;
        if(_capture&&!_capturing&&_elapsed>1.5){_capturing=true;Capture();}
    }
    private async void Capture()
    {
        var folder=ProjectSettings.GlobalizePath("res://artifacts");DirAccess.MakeDirRecursiveAbsolute(folder);
        for(int i=0;i<3;i++)
        {
            _pose=i;QueueRedraw();
            await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using var image=GetViewport().GetTexture().GetImage();
            var path=folder+(i==0?"/rough-art-study.png":$"/rough-art-study-pose-{i}.png");
            if(image.SavePng(path)!=Error.Ok){GetTree().Quit(1);return;}
            GD.Print("CAPTURE "+path);
        }
        GetTree().Quit();
    }
    public override void _Draw()
    {
        if(_quay==null)return;
        DrawSetTransform(Offset,0,Vector2.One*Zoom);
        DrawTextureRectRegion(_quay,new Rect2(18,30,1500,946),new Rect2(18,30,1500,946),Colors.White);
        foreach(var feet in new[]{new Vector2(720,755),new Vector2(925,666)})
        {
            var points=new Vector2[24];for(int i=0;i<24;i++)points[i]=feet+new Vector2(Mathf.Cos(i*Mathf.Tau/24)*25,Mathf.Sin(i*Mathf.Tau/24)*10);
            DrawColoredPolygon(points,new Color(.015f,.012f,.01f,.45f));
        }
        _cast.Draw(this,"guard",new Vector2(925,666),Vector2.Right,new[]{0,3,4}[_pose],0,false,Offset,Zoom);
        _cast.Draw(this,"karl-saber",new Vector2(720,755),Vector2.Right,new[]{0,3,4}[_pose],0,false,Offset,Zoom);
        if(_ruler)
        {
            var feet=new Vector2(675,755);var head=feet-new Vector2(0,PaintedCast.BodyHeight);
            DrawLine(head,feet,new Color("b9aa84"),1,true);
            DrawLine(head-new Vector2(7,0),head+new Vector2(7,0),Colors.White,1,true);
            DrawLine(feet-new Vector2(7,0),feet+new Vector2(7,0),Colors.White,1,true);
            DrawString(_font,head-new Vector2(45,12),"1,78 m / 150 px",fontSize:12,modulate:new Color("d1c8b9"));
        }
        DrawSetTransform(Vector2.Zero);
        DrawRect(new Rect2(20,18,510,62),new Color(.035f,.028f,.022f,.94f));
        DrawString(_font,new Vector2(38,44),"ATLANDS ARV  ·  BILDPROV",fontSize:18,modulate:new Color("c1ad88"));
        DrawString(_font,new Vector2(38,65),"Gemensam kroppsskala · mindre kajföremål · G måttreferens",fontSize:14,modulate:new Color("b1ada5"));
        DrawRect(new Rect2(20,646,840,54),new Color(.035f,.028f,.022f,.94f));
        DrawString(_font,new Vector2(38,670),$"Mellanslag: byt pose ({new[]{"beredskap","upptakt","hugg"}[_pose]})   ·   F11: helskärm   ·   Esc: avsluta",fontSize:16,modulate:new Color("d1c8b9"));
        DrawString(_font,new Vector2(38,690),"Samma figurer används i spelet. Tre stridsposer; fulla gång- och riktningsanimationer återstår.",fontSize:13,modulate:new Color("a19b90"));
    }
    public override void _ExitTree(){_quay?.Dispose();_cast?.Dispose();_font?.Dispose();}
}
