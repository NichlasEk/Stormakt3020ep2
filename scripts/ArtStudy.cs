using Godot;
using System;
using System.Linq;

// Material/identity review at the game's camera and actor scale, not a combat demo.
public partial class ArtStudy : Node2D
{
    private Texture2D _quay=null!,_karl=null!,_guard=null!;
    private Font _font=null!;
    private Sprite2D _hero=null!,_enemy=null!;
    private Shader _key=null!;
    private ShaderMaterial _mat=null!;
    private int _pose;
    private bool _capture,_capturing;
    private double _elapsed;
    private const float Zoom=1.12f;
    private static readonly Vector2 Offset=new(640-730*Zoom,360-700*Zoom);
    private static Vector2 Screen(Vector2 world)=>world*Zoom+Offset;

    public override void _Ready()
    {
        _quay=GD.Load<Texture2D>("res://assets/art/likvarvet-matte-v4.png");
        _karl=GD.Load<Texture2D>("res://assets/art/karl-combat-matte-v5.png");
        _guard=GD.Load<Texture2D>("res://assets/art/guard-combat-matte-v5.png");
        _font=GD.Load<Font>("res://assets/fonts/NotoSans-Regular.ttf");
        _key=new Shader { Code="""
            shader_type canvas_item;
            render_mode unshaded;
            void fragment() {
                vec4 c = texture(TEXTURE, UV);
                // Generated RGB sheets contain a pale checker backdrop, not alpha.
                // Temporary review matte; final animation assets need extracted alpha.
                float white = min(c.r,min(c.g,c.b));
                float matte = 1.0-smoothstep(0.76,0.88,white);
                COLOR=vec4(c.rgb,c.a*matte);
            }
            """ };
        _mat=new ShaderMaterial {Shader=_key};
        _enemy=MakeSprite(_guard,new Vector2(925,666));
        _hero=MakeSprite(_karl,new Vector2(720,755));
        _capture=OS.GetCmdlineUserArgs().Contains("--capture-study");
        UpdatePoses();
    }
    private Sprite2D MakeSprite(Texture2D texture,Vector2 feet)
    {
        var s=new Sprite2D {Texture=texture,RegionEnabled=true,Centered=false,Material=_mat,TextureFilter=TextureFilterEnum.Linear};
        // The first-row reference figures occupy about 72% of cell height.
        // 112 world-pixel bodies match the current gameplay actors, with no enlarged portrait trick.
        float scale=112f/(texture.GetHeight()*.5f*.72f);
        s.Scale=Vector2.One*scale*Zoom;
        s.Position=Screen(feet)-new Vector2(texture.GetWidth()/8f,texture.GetHeight()*.5f*.85f)*scale*Zoom;
        AddChild(s);return s;
    }
    private void UpdatePoses()
    {
        // Exclude the lower-row hammer head from the sword follow-through cell.
        _hero.RegionRect=new Rect2(_pose*_karl.GetWidth()/4f,0,_karl.GetWidth()/4f,_karl.GetHeight()*.45f);
        // The lower-row pike enters the nominal top-left grid cell: crop above its tip.
        _enemy.RegionRect=new Rect2(_pose*_guard.GetWidth()/4f,0,_guard.GetWidth()/4f,_guard.GetHeight()*.44f);
        var heroAnchor=new Vector2(_karl.GetWidth()/4f*new[]{.60f,.59f,.68f}[_pose],_karl.GetHeight()*new[]{.415f,.429f,.428f}[_pose]);
        var enemyAnchor=new Vector2(_guard.GetWidth()/4f*new[]{.49f,.51f,.54f}[_pose],_guard.GetHeight()*new[]{.395f,.420f,.386f}[_pose]);
        _hero.Position=Screen(new Vector2(720,755))-heroAnchor*_hero.Scale;
        _enemy.Position=Screen(new Vector2(925,666))-enemyAnchor*_enemy.Scale;
        QueueRedraw();
    }
    public override void _Input(InputEvent ev)
    {
        if(ev is not InputEventKey {Pressed:true,Echo:false} k)return;
        if(k.PhysicalKeycode==Key.Escape){GetTree().Quit();return;}
        if(k.PhysicalKeycode==Key.Space){_pose=(_pose+1)%3;UpdatePoses();}
        if(k.PhysicalKeycode==Key.F11)DisplayServer.WindowSetMode(DisplayServer.WindowGetMode()==DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);
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
            _pose=i;UpdatePoses();
            await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using var image=GetViewport().GetTexture().GetImage();
            var path=folder+(i==0?"/rough-art-study.png":$"/rough-art-study-pose-{i}.png");
            var error=image.SavePng(path);
            if(error!=Error.Ok){GD.PushError($"Study capture failed: {error}");GetTree().Quit(1);return;}
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
            var points=new Vector2[24];for(int i=0;i<24;i++)points[i]=feet+new Vector2(Mathf.Cos(i*Mathf.Tau/24)*24,Mathf.Sin(i*Mathf.Tau/24)*8);
            DrawColoredPolygon(points,new Color(.015f,.012f,.01f,.45f));
        }
        DrawSetTransform(Vector2.Zero);
        DrawRect(new Rect2(20,18,450,62),new Color(.035f,.028f,.022f,.94f));
        DrawString(_font,new Vector2(38,44),"ATLANDS ARV  ·  BILDPROV",fontSize:18,modulate:new Color("c1ad88"));
        DrawString(_font,new Vector2(38,65),"Målade figurer · mattare kaj · spelets kameraskala",fontSize:14,modulate:new Color("b1ada5"));
        DrawRect(new Rect2(20,646,780,54),new Color(.035f,.028f,.022f,.94f));
        DrawString(_font,new Vector2(38,670),$"Mellanslag: byt pose ({new[]{"beredskap","upptakt","hugg"}[_pose]})   ·   F11: helskärm   ·   Esc: avsluta",fontSize:16,modulate:new Color("d1c8b9"));
        DrawString(_font,new Vector2(38,690),"Figurstudie – ännu inte komplett riktningsanimation eller spelbar strid.",fontSize:13,modulate:new Color("a19b90"));
    }
    public override void _ExitTree()
    {
        _hero.Texture=null;_hero.Material=null;_enemy.Texture=null;_enemy.Material=null;
        _mat.Shader=null;_mat.Dispose();_key.Dispose();
        _quay.Dispose();_karl.Dispose();_guard.Dispose();_font.Dispose();
    }
}
