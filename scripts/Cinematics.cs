using Godot;
using System;

public partial class Main
{
    private const string GateFilm="res://assets/video/archive-gate-v1.ogv";
    private VideoStreamPlayer? _filmPlayer;
    private VideoStream? _filmStream;
    private Screen _filmReturn=Screen.Title;
    private float _filmElapsed;
    private bool _filmCheck,_filmPreview;
    private void StartGateFilm(bool preview=false)
    {
        if(_screen==Screen.Cinematic)return;
        if(!ResourceLoader.Exists(GateFilm)){Notice("Filmen kunde inte laddas. Vägen är öppen.");return;}
        _filmStream??=GD.Load<VideoStream>(GateFilm);
        if(_filmStream is null)return;
        if(_filmPlayer is null)
        {
            _filmPlayer=new VideoStreamPlayer{Expand=true,Visible=false,MouseFilter=Control.MouseFilterEnum.Ignore};
            AddChild(_filmPlayer);_filmPlayer.Finished+=FinishGateFilm;
        }
        _filmReturn=preview?(_screen==Screen.Videos?Screen.Videos:Screen.Title):Screen.Game;_filmElapsed=0;
        _filmPlayer.Stream=_filmStream;_filmPlayer.Volume=_testMode?0:_volume;
        ChangeScreen(Screen.Cinematic);_sound.Cinematic=true;_filmPlayer.Play();
    }
    private void FinishGateFilm()
    {
        if(_screen!=Screen.Cinematic)return;
        _filmPlayer?.Stop();_sound.Cinematic=false;
        ChangeScreen(_filmReturn);_inventoryMouseRelease=true;
        // The queued rootway radio begins after the film; room banners get their normal reading time.
        if(_screen==Screen.Game){_bannerTime=5;_campaignTextTime=10;}
    }
    private void StepFilm(float dt)
    {
        if(_screen!=Screen.Cinematic)return;
        _filmElapsed+=dt;
        if(_filmPlayer!=null)_filmPlayer.Volume=_testMode?0:_volume;
        // Missing decoder/end signals must never strand the player in a transition.
        if(_filmElapsed>20)FinishGateFilm();
    }
    private void DrawGateFilm()
    {
        DrawRect(new Rect2(0,0,1280,720),Colors.Black);
        var texture=_filmPlayer?.GetVideoTexture();
        if(texture!=null&&texture.GetWidth()>0)
        {
            var size=texture.GetSize();float scale=Math.Min(1280/size.X,640/size.Y);
            var fitted=size*scale;DrawTextureRect(texture,new Rect2((new Vector2(1280,720)-fitted)/2,fitted),false);
        }
        Text("ARKIVETS PORT",new Vector2(40,38),16,Gold,true);
        Button(new Rect2(910,666,330,38),"Hoppa över · Esc / B","film-skip");
    }
    private void DrawVideoMenu()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.02f,.027f,.025f,.98f));
        Centered("Videobibliotek",640,140,38,Pale,true);
        Centered("Utvecklarverktyg · spela upp mellansekvenser",640,183,17,Gold);
        Panel(new Rect2(245,255,790,210),.95f);
        Text("ARKIVETS PORT",new Vector2(280,298),22,Gold,true);
        Wrapped("Kedjan spänns, stenen viker undan och dagsljuset når arkivet. Första filmprovet · cirka 8 sekunder.",new Vector2(280,336),695,18,Pale,27);
        Button(new Rect2(280,397,320,45),"Spela upp","gate-film",true);
        Centered("Uppspelning påverkar inte sparningen. Esc / B avslutar filmen.",640,519,16,Muted);
        Button(new Rect2(455,587,370,45),"Till inställningar","back");
    }
    private void DisposeFilm()
    {
        if(_filmPlayer!=null){_filmPlayer.Stop();_filmPlayer.Stream=null;}
        _filmStream?.Dispose();
    }
}
