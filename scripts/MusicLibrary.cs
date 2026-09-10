using Godot;
using Atland;
using System;
using System.Linq;
using System.Text.Json;
public partial class Main
{
    private int _musicPage;
    private string _previewScore="";
    private MusicTrack[]? _musicTracks;
    private MusicTrack[] MusicTracks=>_musicTracks??=JsonSerializer.Deserialize<MusicTrack[]>(FileAccess.GetFileAsString("res://assets/story/music.json"))!;
    private void DrawMusicMenu()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.02f,.027f,.025f,.98f));
        Centered("Musikbibliotek",640,75,36,Pale,true);Centered("Miljöer, bossar och färder · samma volymkontroll som spelet",640,112,16,Gold);
        int pageCount=(MusicTracks.Length+7)/8;_musicPage=Math.Clamp(_musicPage,0,pageCount-1);
        int row=0;foreach(var t in MusicTracks.Skip(_musicPage*8).Take(8))
        {float y=145+row*52;bool playing=_previewScore==t.id;Button(new Rect2(195,y,890,43),(playing?"▶ ":"")+t.title+"  ·  "+(t.id.StartsWith("boss-")?"Boss":"Miljö / färd"),"music:"+t.id,playing);row++;}
        Button(new Rect2(195,577,190,38),"Föregående","music-prev");Centered($"{_musicPage+1} / {pageCount}",640,603,16,Muted);Button(new Rect2(895,577,190,38),"Nästa","music-next");
        Button(new Rect2(195,635,190,40),"Sänk volym","volume-");Button(new Rect2(895,635,190,40),"Höj volym","volume+");Button(new Rect2(455,635,370,40),"Till inställningar","back");
    }
}
