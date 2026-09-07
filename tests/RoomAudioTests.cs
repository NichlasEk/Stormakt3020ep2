using Atland;
using System.Numerics;

static class RoomAudioTests
{
    public static void Run(Action<bool,string> check)
    {
        var g=Combat.NewRooms(Order.Artillery);
        var events=new List<Cue>(g.Events);
        for(int i=0;i<10000&&!g.Dead&&!g.Rooms!.Completed;i++)
        {g.Step(OathCheckPilot.Decide(g,true));events.AddRange(g.Events);}
        check(g.Rooms!.Completed,"Audio route reaches completion");
        check(!events.Any(c=>c.Kind=="radio"&&c.Text=="arrival"),"Room route never replays the old quay briefing");
        foreach(var id in new[]{"rooms-entry","rooms-pump","rooms-drained","rooms-witness","rooms-rush","rooms-fallen","rooms-port","rooms-cistern"})
            check(events.Count(c=>c.Kind=="radio"&&c.Text==id)==1,"Narrative cue occurs exactly once: "+id);
        foreach(var id in new[]{"pump-pressure","pump-drain"})check(events.Count(c=>c.Kind=="room-sound"&&c.Text==id)==1,"Mechanism sound occurs once: "+id);
        check(events.Any(c=>c.Kind=="room-sound"&&c.Text=="oath-rush")&&events.Any(c=>c.Kind=="oath-break"),"Rush and exposure have distinct sound cues");
        check(!g.RoomRadioRelevant("rooms-rush")&&!g.RoomRadioRelevant("rooms-pump")&&g.RoomRadioRelevant("rooms-witness"),"Completed state removes obsolete directions but retains witness meaning");
        var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.OathGuardian);
        check(boss.Pattern>1,"Rush narration does not repeat on subsequent attacks");
        g.Player=RoomLinks.All[5].AtB;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));
        check(!g.Events.Any(c=>c.Kind=="radio"),"Room revisit does not restart narrative");
    }
}
