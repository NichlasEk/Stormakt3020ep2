using Atland;
using System.Text.Json;
public static class MusicTests
{
    public static void Run(Action<bool,string> check)
    {
        var root=Directory.GetCurrentDirectory();while(!File.Exists(Path.Combine(root,"project.godot")))root=Directory.GetParent(root)!.FullName;
        var tracks=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"assets/story/music.json"))).RootElement.EnumerateArray().ToArray();var ids=tracks.Select(t=>t.GetProperty("id").GetString()!).ToHashSet();
        check(ids.Count==tracks.Length,"Unique soundtrack IDs");foreach(var room in ConnectedWorld.RoomIds)check(ids.Contains(room),"Every room has its own music: "+room);
        foreach(var kind in Enum.GetValues<EnemyKind>()){var cue=MusicDirector.BossCue(kind);if(cue!="")check(ids.Contains(cue),"Every boss has its own music: "+kind);}
        var g=Combat.NewWestPreview(Order.Artillery);var d=new MusicDirector();check(d.Select(g)==Gamla.Registry,"Room music selected");
        var enter=typeof(Combat).GetMethod("EnterConnectedRoom",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;enter.Invoke(g,new object[]{West.Control});g.WestState.RegisterRead=true;enter.Invoke(g,new object[]{West.Hall});
        check(d.Select(g)==West.Hall,"Boss room ambience before engagement");check(d.Select(g,true)=="boss-muster","Seen boss engages own score");check(d.Select(g,false)=="boss-muster","Boss score survives loss of sight");g.Enemies.Single(e=>e.Kind==EnemyKind.MusterOfficer).Health=0;check(d.Select(g)==West.Hall,"Death returns to room theme");
        check(d.Select(g,ship:true)=="travel-ship"&&d.Select(g,boat:true)=="travel-boat","Travel cues override scene");
        check(d.Select(g,menu:true)=="prologue-quay","Menu theme resets battle latch");
        var legacy=Combat.NewAtland(Order.Artillery);var legacyEnter=typeof(Combat).GetMethod("EnterCampaign",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
        foreach(var (stage,cue) in new[]{(3,"boss-marshal"),(5,"boss-bailiff"),(7,"boss-tribunal")}){legacyEnter.Invoke(legacy,new object[]{stage});legacy.Enemies.Clear();legacy.Spawn(EnemyKind.Collector,legacy.Player);check(d.Select(legacy,true)==cue,"Older campaign boss identity "+stage);}
        foreach(var t in tracks)check(t.GetProperty("bpm").GetInt32()>0&&t.GetProperty("bars").GetInt32()>0,"Loop metadata valid");
    }
}
