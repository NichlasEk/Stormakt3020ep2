using Godot;
using System;
using System.Collections.Generic;

namespace Atland;

public partial class Soundscape : Node
{
    private AudioStreamPlayer _music=new();
    private AudioStreamPlayer _bossMusic=new();
    private AudioStreamPlayer _namesMusic=new();
    private AudioStreamPlayer _voice=new();
    private AudioStreamPlayer _ambience=new();
    private readonly List<AudioStreamPlayer> _effects=new();
    private readonly Dictionary<string,AudioStream> _clips=new();
    private int _next;
    private float _bossMix;
    private float _namesMix;
    public bool Boss;
    public bool Discovery;
    public float Volume=.75f;
    public bool Speaking=>_voice.Playing;
    public override void _Ready()
    {
        AddChild(_music);AddChild(_bossMusic);AddChild(_namesMusic);AddChild(_voice);AddChild(_ambience);
        _music.VolumeDb=-30;_bossMusic.VolumeDb=-80;_namesMusic.VolumeDb=-80;_ambience.VolumeDb=-30;
        for(int i=0;i<12;i++){var p=new AudioStreamPlayer();AddChild(p);_effects.Add(p);}
        foreach(var name in new[]{"score","boss-score","ambience","swing","hammer","hit","parry","shot","cannon","seal","heal","dodge","death","voice-arrival","voice-cannon","voice-collector","voice-rage","voice-fallen","voice-atland"})
            if(ResourceLoader.Exists($"res://assets/audio/{name}.ogg"))_clips[name]=GD.Load<AudioStream>($"res://assets/audio/{name}.ogg");
        foreach(var name in new[]{"names-intro","names-warning","name-0","name-1","name-2","broadcast","cipher","homebound","hedvig-karta","hedvig-minne"})
            if(ResourceLoader.Exists($"res://assets/audio/voice-{name}.ogg"))_clips["voice-"+name]=GD.Load<AudioStream>($"res://assets/audio/voice-{name}.ogg");
        foreach(var id in JourneyDialogue.Load().Keys)
            if(ResourceLoader.Exists($"res://assets/audio/voice-{id}.ogg"))_clips["voice-"+id]=GD.Load<AudioStream>($"res://assets/audio/voice-{id}.ogg");
        foreach(var name in new[]{"scrape","inscription","paper","footstep"})_clips[name]=GD.Load<AudioStream>($"res://assets/audio/{name}.ogg");
        if(ResourceLoader.Exists("res://assets/audio/names-score.ogg"))_clips["names-score"]=GD.Load<AudioStream>("res://assets/audio/names-score.ogg");
        Loop(_music,"score");Loop(_bossMusic,"boss-score");Loop(_namesMusic,"names-score");Loop(_ambience,"ambience");
    }
    private void Loop(AudioStreamPlayer player,string name)
    {
        if(!_clips.TryGetValue(name,out var clip))return;
        if(clip is AudioStreamOggVorbis ogg)ogg.Loop=true;
        player.Stream=clip;player.Play();
    }
    public override void _Process(double delta)
    {
        float db=Volume<=0?-80:Mathf.LinearToDb(Volume);
        _bossMix=Mathf.MoveToward(_bossMix,Boss?1:0,(float)delta*.7f);
        _namesMix=Mathf.MoveToward(_namesMix,Discovery&&_clips.ContainsKey("names-score")?1:0,(float)delta*.45f);
        float duck=Speaking?-19:Discovery?-15:-10;
        _music.VolumeDb=db+duck+Mathf.LinearToDb(Math.Max(.0001f,(1-_bossMix)*(1-_namesMix)));
        _bossMusic.VolumeDb=db+duck+Mathf.LinearToDb(Math.Max(.0001f,_bossMix));
        _namesMusic.VolumeDb=db+duck+Mathf.LinearToDb(Math.Max(.0001f,_namesMix*(1-_bossMix)));
        _ambience.VolumeDb=db-17;_voice.VolumeDb=db;
        foreach(var p in _effects)p.VolumeDb=db-8;
    }
    public void Speak(string name)
    {
        if(!_clips.TryGetValue("voice-"+name,out var clip))return;
        _voice.Stop();_voice.Stream=clip;_voice.Play();
    }
    public void StopVoice()=>_voice.Stop();
    public void PauseVoice(bool pause)=>_voice.StreamPaused=pause;
    public void Play(string name,float pitch=1)
    {
        if(!_clips.TryGetValue(name,out var clip))return;
        var p=_effects[_next++%_effects.Count];p.Stop();p.Stream=clip;p.PitchScale=pitch;p.Play();
    }
    public override void _ExitTree()
    {
        foreach(var player in _effects){player.Stop();player.Stream=null;}
        foreach(var player in new[]{_music,_bossMusic,_namesMusic,_voice,_ambience}){player.Stop();player.Stream=null;}
        foreach(var clip in _clips.Values)clip.Dispose();
        _clips.Clear();_effects.Clear();
    }
}
