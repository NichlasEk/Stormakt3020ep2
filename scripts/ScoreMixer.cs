using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
namespace Atland;
public sealed class MusicTrack
{
    public string id{get;set;}="";
    public string title{get;set;}="";
    public string file{get;set;}="";
}
public partial class Soundscape
{
    private readonly AudioStreamPlayer[] _scorePlayers={new(),new()};
    private readonly float[] _scoreGains={0,0};
    private readonly string[] _scoreIds={"",""};
    private readonly Dictionary<string,double> _scorePositions=new();
    private readonly Dictionary<string,AudioStreamOggVorbis> _scores=new();
    private int _scoreTarget;
    public string RequestedScore{get;private set;}="prologue-quay";
    public string CurrentScore=>_scoreIds[_scoreTarget];
    public float ScorePeakDb=>Math.Max(_scorePlayers[0].VolumeDb,_scorePlayers[1].VolumeDb);
    public double ScorePosition=>_scorePlayers[_scoreTarget].GetPlaybackPosition();
    public int ScoreCount=>_scores.Count;
    public bool HasScore(string id)=>_scores.ContainsKey(id);
    public int PlayingScores=>Array.FindAll(_scorePlayers,p=>p.Playing).Length;
    public void SelectScore(string id){if(_scores.ContainsKey(id))RequestedScore=id;}
    private void LoadScores()
    {
        foreach(var p in _scorePlayers){AddChild(p);p.VolumeDb=-80;}
        var tracks=JsonSerializer.Deserialize<MusicTrack[]>(FileAccess.GetFileAsString("res://assets/story/music.json"))!;
        foreach(var t in tracks){var clip=GD.Load<AudioStreamOggVorbis>(t.file);if(clip==null)throw new InvalidOperationException("Missing music: "+t.file);clip.Loop=true;_scores.Add(t.id,clip);}
    }
    private void StepScores(float dt,float db)
    {
        if(CurrentScore!=RequestedScore)
        {
            int next=1-_scoreTarget;
            // Finish the existing fade before replacing a still-audible stream.
            if(_scoreGains[next]<.001f)
            {
                var p=_scorePlayers[next];if(p.Playing&&_scoreIds[next]!="")_scorePositions[_scoreIds[next]]=p.GetPlaybackPosition();
                p.Stop();p.VolumeDb=-80;p.Stream=_scores[RequestedScore];_scoreIds[next]=RequestedScore;
                p.Play((float)(_scorePositions.GetValueOrDefault(RequestedScore)%p.Stream.GetLength()));_scoreTarget=next;
            }
        }
        float duck=Speaking&&!_voice.StreamPaused?-19:Discovery?-13:-10;
        for(int i=0;i<2;i++)
        {
            _scoreGains[i]=Mathf.MoveToward(_scoreGains[i],i==_scoreTarget?1:0,dt/2.4f);
            _scorePlayers[i].VolumeDb=db+duck+Mathf.LinearToDb(Math.Max(.0001f,Mathf.Sqrt(_scoreGains[i])));
            if(i!=_scoreTarget&&_scoreGains[i]==0&&_scorePlayers[i].Playing){_scorePositions[_scoreIds[i]]=_scorePlayers[i].GetPlaybackPosition();_scorePlayers[i].Stop();}
        }
    }
    private void DisposeScores()
    {foreach(var p in _scorePlayers){p.Stop();p.Stream=null;}foreach(var clip in _scores.Values)clip.Dispose();_scores.Clear();}
}
