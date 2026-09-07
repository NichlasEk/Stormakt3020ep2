using Godot;
using Atland;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main
{
    private FilmDefinition[]? _films;
    private FilmDefinition? _activeFilm;
    private readonly Dictionary<string,VideoStream> _filmStreams=new();
    private VideoStreamPlayer? _filmPlayer;
    private Screen _filmReturn=Screen.Title;
    private float _filmElapsed;
    private int _videoPage;
    private bool _filmCheck,_filmPreview,_introPreview;
    private FilmDefinition[] Films=>_films??=FilmLibrary.Load();
    private void StartGateFilm(bool preview=false)=>StartFilm("archive-gate",preview);
    private void StartIntroFilm(bool preview=false)=>StartFilm("atland-intro",preview);
    private void StartFilm(string id,bool preview=false)
    {
        if(_screen==Screen.Cinematic)return;
        var film=Films.FirstOrDefault(f=>f.Id==id);
        if(film is null||!ResourceLoader.Exists(film.File)){Notice("Filmen kunde inte laddas. Du kan fortsätta.");return;}
        if(!_filmStreams.TryGetValue(id,out var stream))
        {
            stream=GD.Load<VideoStream>(film.File);if(stream is null)return;
            _filmStreams.Add(id,stream);
        }
        if(_filmPlayer is null)
        {
            _filmPlayer=new VideoStreamPlayer{Expand=true,Visible=false,MouseFilter=Control.MouseFilterEnum.Ignore};
            AddChild(_filmPlayer);_filmPlayer.Finished+=FinishGateFilm;
        }
        _activeFilm=film;_filmReturn=preview?(_screen==Screen.Videos?Screen.Videos:Screen.Title):Screen.Game;_filmElapsed=0;
        _filmPlayer.Stream=stream;_filmPlayer.Volume=_testMode?0:_volume;
        ChangeScreen(Screen.Cinematic);_sound.Cinematic=true;_filmPlayer.Play();
    }
    private void FinishGateFilm()
    {
        if(_screen!=Screen.Cinematic)return;
        _filmPlayer?.Stop();_sound.Cinematic=false;
        ChangeScreen(_filmReturn);_inventoryMouseRelease=true;
        if(_screen==Screen.Game){_bannerTime=5;_campaignTextTime=10;}
    }
    private void StepFilm(float dt)
    {
        if(_screen!=Screen.Cinematic)return;
        _filmElapsed+=dt;
        if(_filmPlayer!=null)_filmPlayer.Volume=_testMode?0:_volume;
        // Allow the full catalog duration plus decoder grace, including the longer intro.
        if(_filmElapsed>(_activeFilm?.Duration??8)+12)FinishGateFilm();
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
        Text(_activeFilm?.Title.ToUpperInvariant()??"",new Vector2(40,38),16,Gold,true);
        var caption=_activeFilm?.CaptionAt(_filmPlayer?.StreamPosition??0);
        if(caption!=null)
        {
            Panel(new Rect2(160,551,960,99),.93f);Text(caption.Speaker,new Vector2(186,576),12,Gold);
            Wrapped(caption.Text,new Vector2(186,605),905,18,Pale,25);
        }
        Button(new Rect2(910,666,330,38),"Hoppa över · Esc / B","film-skip");
    }
    private void DrawVideoMenu()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.02f,.027f,.025f,.98f));
        Centered("Videobibliotek",640,120,38,Pale,true);
        Centered("Utvecklarverktyg · spela upp mellansekvenser",640,160,17,Gold);
        _videoPage=Math.Clamp(_videoPage,0,Math.Max(0,(Films.Length-1)/3));
        int row=0;
        foreach(var film in Films.Skip(_videoPage*3).Take(3))
        {
            float y=204+row*120;Panel(new Rect2(185,y,910,107),.95f);
            Text(film.Title,new Vector2(210,y+29),21,Gold,true);
            Wrapped(film.Description,new Vector2(210,y+57),610,16,Pale,22);
            Button(new Rect2(858,y+31,210,45),$"Spela · {Math.Round(film.Duration)} s","film:"+film.Id,row==0);row++;
        }
        if(_videoPage>0)Button(new Rect2(185,580,160,36),"Föregående","film-prev");
        if((_videoPage+1)*3<Films.Length)Button(new Rect2(935,580,160,36),"Nästa","film-next");
        Centered("Uppspelning ändrar inte sparningen. Esc / B avslutar filmen.",640,611,15,Muted);
        Button(new Rect2(455,640,370,40),"Till inställningar","back");
    }
    private void DisposeFilm()
    {
        if(_filmPlayer!=null){_filmPlayer.Stop();_filmPlayer.Stream=null;}
        foreach(var stream in _filmStreams.Values)stream.Dispose();_filmStreams.Clear();
    }
}
