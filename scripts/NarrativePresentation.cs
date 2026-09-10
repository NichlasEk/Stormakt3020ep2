using Godot;
using Atland;
using System;
public partial class Main
{
    private string _pendingStoryFilm="";
    private float _restFade,_radioBreath;
    private float RegimentVisibility=>!_game.RegimentState.Discharged?1:_pendingStoryFilm=="regiment-rest"?1-_restFade:0;
    private void QueueFarewell()
    {
        if(_pendingStoryFilm!=""||!_game.InRegiment||_game.Rooms!.Current!=Regiment.Parade||!_game.RegimentState.Discharged||_game.RegimentState.FarewellSeen)return;
        _radioQueue.Clear();_sound.StopVoice();_radio="";_radioTime=0;_radioBreath=0;
        QueueRadio("continuity-dismiss");QueueRadio("regiment-freed");_pendingStoryFilm="regiment-rest";_restFade=0;
    }
    private void StepNarrative(float dt)
    {
        if(_screen!=Screen.Game)return;
        if(_radioTime<=0&&!_sound.Speaking)_radioBreath=Math.Max(0,_radioBreath-dt);
        if(_pendingStoryFilm==""&&_game.Rooms?.Rescue is {Reunited:true,KissSeen:false})_pendingStoryFilm="rescue-reunion";
        if(_pendingStoryFilm=="")return;
        _restFade=Math.Min(1,_restFade+dt/9);
        if(_radioTime>0||_sound.Speaking||_radioQueue.Count>0||_radioBreath>0)return;
        var id=_pendingStoryFilm;_pendingStoryFilm="";StartFilm(id);
        if(_screen==Screen.Cinematic)
        {if(id=="rescue-reunion")_game.RescueState.KissSeen=true;else _game.RegimentState.FarewellSeen=true;Save();}
        else if(id=="rescue-reunion"){_game.RescueState.KissSeen=true;QueueRadio("rescue-after-karl");QueueRadio("rescue-after-ebba");Save();}else {QueueRadio("continuity-silence");QueueRadio("continuity-names");}
    }
    private void FinishNarrativeFilm()
    {
        if(_activeFilm?.Id=="rescue-reunion"&&_filmReturn==Screen.Game){_radio="";_radioTime=0;_radioBreath=1.5f;QueueRadio("rescue-after-karl");QueueRadio("rescue-after-ebba");return;}
        if(_activeFilm?.Id!="regiment-rest"||_filmReturn!=Screen.Game)return;
        _radio="";_radioTime=0;_radioBreath=1.5f;
        QueueRadio("continuity-silence");QueueRadio("continuity-names");
    }
}
