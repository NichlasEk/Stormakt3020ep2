using Atland;
using System.Numerics;
public static class NarrativeTests
{
    public static void Run(Action<bool,string> check)
    {
        void Use(Combat g,Vector2 p){g.Player=p;g.Hurt=g.AttackTime=g.DodgeTime=0;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
        string[] Lines(Combat g)=>g.Events.Where(e=>e.Kind=="radio").Select(e=>e.Text).ToArray();
        var captain=Combat.NewRegimentPreview(Order.Medicine);captain.Rooms!.Current=Regiment.Barracks;captain.Rooms.Rooms[Regiment.Barracks].Visited=true;captain.Enemies.Clear();
        Use(captain,Regiment.Captain);
        check(Lines(captain).SequenceEqual(new[]{"regiment-captain","continuity-dead","continuity-rest"}),"Captain names death before Hedvig explains rest");
        Use(captain,Regiment.Captain);check(Lines(captain).Length==0,"Captain clarification does not repeat");
        captain.RegimentState.DeathExplained=false;Use(captain,Regiment.Captain);
        check(Lines(captain).SequenceEqual(new[]{"continuity-dead","continuity-rest"}),"Older captain meeting receives only missing clarification");
        var g=Combat.NewUppsalaPreview(Order.Medicine);g.Rooms!.Current=Regiment.Parade;g.RegimentState.Discharged=false;g.Spawn(EnemyKind.RootSoldier,Regiment.Discharge+new Vector2(200,0));
        Use(g,Regiment.Discharge);
        check(g.RegimentState.Discharged&&g.Enemies.Where(e=>e.Kind==EnemyKind.RootSoldier).All(e=>e.Dead&&e.Retired),"Discharge releases remaining soldiers");
        check(Lines(g).SequenceEqual(new[]{"continuity-dismiss","regiment-freed"})&&g.Events.Any(e=>e.Kind=="story-film"&&e.Text=="regiment-rest"),"Farewell dialogue precedes film cue");
        int drops=g.Inventory.NextId;g.Inventory.Drops.Clear();Use(g,Regiment.Discharge);check(g.Inventory.NextId==drops&&g.Events.Any(e=>e.Kind=="story-film"),"Unseen farewell can resume without a duplicate reward");
        g.RegimentState.FarewellSeen=true;Use(g,Regiment.Discharge);check(!g.Events.Any(e=>e.Kind=="story-film"),"Seen farewell does not replay on interaction");
        var path=Path.Combine(Path.GetTempPath(),"narrative-"+Guid.NewGuid()+".json");
        try{SaveStore.Write(path,g);g=SaveStore.Read(path);check(g.RegimentState.FarewellSeen,"Farewell persists through save and load");}finally{File.Delete(path);}
        g.Rooms!.Current=Mine.Coolway;g.Enemies.Clear();Use(g,Mine.Imprint);
        check(g.MineState.FormExplained&&Lines(g).SequenceEqual(new[]{"continuity-mould"}),"Older recovered imprint clarifies the stationary mould");
        drops=g.Inventory.NextId;Use(g,Mine.Imprint);check(Lines(g).Length==0&&g.Inventory.NextId==drops,"Mould clarification cannot duplicate loot");
        g.Rooms.Current=Foundry.Room;Use(g,Foundry.Plate);
        check(Lines(g).SequenceEqual(new[]{"continuity-plate","continuity-home"}),"Recovered plate is distinguished from mould and chart");
        drops=g.Inventory.NextId;Use(g,Foundry.Plate);check(Lines(g).Length==0&&g.Inventory.NextId==drops,"Plate clarification is once-only");
        g.Rooms.Current=PortRooms.Court;Use(g,new(700,600));check(Lines(g).Contains("continuity-chart"),"Older chart gets clarification on court interaction");
        Use(g,new(700,600));check(!Lines(g).Contains("continuity-chart"),"Chart clarification is once-only");
    }
}
