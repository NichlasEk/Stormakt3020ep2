using Godot;
using Atland;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Main
{
    private bool _roomAudioChecks;
    private async Task CheckRoomAudioPresentation()
    {
        void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        var lines=JourneyDialogue.Rooms();
        Check(lines.Count==8,"Eight room dialogue assets");
        foreach(var (id,line) in lines)
        {
            Check(Radio.TryGetValue(id,out var displayed)&&displayed.Text==line[1],"Matching subtitle: "+id);
            Check(_sound.HasClip("voice-"+id)&&_sound.ClipDuration("voice-"+id)>2,"Playable voice: "+id);
            _sound.Speak(id);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            Check(_sound.Speaking,"Native voice player starts: "+id);_sound.StopVoice();
        }
        foreach(var id in new[]{"pump-pressure","pump-drain","stone-door","oath-lock","oath-rush","oath-impact","vault-ambience"})
            Check(_sound.HasClip(id)&&_sound.ClipDuration(id)>.2,"Room sound loaded: "+id);
        _sound.Underground=true;_sound._Process(3);Check(_sound.VaultDominates,"Underground fades out harbor ambience");
        _sound.Underground=false;_sound._Process(3);Check(!_sound.VaultDominates,"Courtyard restores harbor ambience");
        Back(); // Close the journal opened by the full route check.
        _radio="rooms-witness";_radioTime=10;_sound.Speak(_radio);_radioQueue.Clear();_radioQueue.Enqueue("cannon");
        PrepareRegionRadio();Check(_radio=="rooms-witness"&&_sound.Speaking&&!_radioQueue.Any(),"Doorway retains witness speech and discards stale cannon order");
        _radio="rooms-rush";_radioTime=10;_sound.Speak(_radio);_radioQueue.Enqueue("rooms-rush");_radioQueue.Enqueue("cannon");
        QueueRadio("rooms-fallen");Check(_radio==""&&!_sound.Speaking&&_radioQueue.SequenceEqual(new[]{"rooms-fallen"}),"Boss death clears obsolete advice and artillery");
        _radioQueue.Clear();_radio="rooms-port";_radioTime=10;_sound.Speak(_radio);
        _campaignTextTime=_bannerTime=_noticeTime=0;QueueRedraw();
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
        using var image=GetViewport().GetTexture().GetImage();
        Check(image.SavePng(ProjectSettings.GlobalizePath("res://artifacts/rooms-radio-hedvig.png"))==Error.Ok,"Native radio portrait and subtitles screenshot");
        _sound.StopVoice();
        GD.Print("ROOM AUDIO CHECK PASS: 8 voices/subtitles, 7 room sounds, ambience crossfade, speech across doors, obsolete advice removed, radio portrait");
    }
}
