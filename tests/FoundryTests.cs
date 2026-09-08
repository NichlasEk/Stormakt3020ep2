using Atland;
using System.Numerics;
public static class FoundryTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})foreach(var order in new[]{Order.Artillery,Order.Medicine})foreach(bool cooling in new[]{true,false})
        {
            var g=Combat.NewFoundryPreview(order);g.Rooms!.Current=Foundry.Room;g.Rooms.Rooms[Foundry.Room].Visited=true;g.FoundryState.GateOpen=true;g.Player=new(500,680);g.Weapon=weapon;g.Spawn(EnemyKind.CrownBailiff,Foundry.Spawn);
            var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.CrownBailiff);int ticks=0;
            while(!g.Dead&&!boss.Dead&&ticks++<18000)g.Step(FoundryPilot.Decide(g,cooling));
            check(boss.Dead&&!g.Dead&&!g.DeveloperSurvival,$"Bailiff {weapon}/{order} cooling={cooling} hp={g.Health} boss={boss.Health} ticks={ticks} pos={g.Player} bosspos={boss.Position} state={boss.State} water={g.FoundryState.CoolingCooldown} quenches={g.FoundryState.Quenches}");
            check(g.FoundryState.BailiffDefeated&&g.FoundryState.SecondPhase,"Boss phase and defeat persist");
            if(cooling)check(g.FoundryState.Quenches>0,"Cooling used by pilot");
            g.Step(default);check(!g.Hazards.Any(h=>h.Forge),"No stamping damage after victory");g.ValidateRooms();
            Console.WriteLine($"BAILIFF {weapon}/{order} cooling={cooling}: {ticks} ticks, {g.Health} hp, {g.Parries} parries");
        }
        var run=Combat.NewFoundryPreview(Order.Medicine);var file=Path.Combine(Path.GetTempPath(),"foundry-"+Guid.NewGuid()+".json");
        void Save(){SaveStore.Write(file,run);run=SaveStore.Read(file);run.ValidateRooms();}
        void Use(Vector2 p){run.Player=p;run.Step(default);run.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
        void Walk(bool back)
        {
            var route=ConnectedWorld.Route(RoomLinks.All.Last());if(back)Array.Reverse(route);run.Player=route[0]-run.WorldOrigin;
            foreach(var target in route.Skip(1))
            {int t=0;while(Vector2.Distance(run.Player+run.WorldOrigin,target)>5&&t++<3000){var before=run.Player+run.WorldOrigin;var move=Vector2.Normalize(target-before);run.Step(new(move,move,false,false,false,false,false,false,false,false));check(Vector2.Distance(before,run.Player+run.WorldOrigin)<9,"Foundry crossing never teleports");}check(t<3000,"Foundry route traversable "+back);}
            check(run.Rooms!.Current==(back?Mine.Coolway:Foundry.Room),"Foundry correct arrival");Save();
        }
        try
        {
            run.Rooms!.Rooms.Remove(Foundry.Room);run.Rooms.Doors.Remove("foundry");run.Rooms.LayoutVersion=7;run.Health=68;Save();
            check(run.Rooms!.LayoutVersion==8&&run.Rooms.Rooms.Count==19&&!run.Rooms.Rooms[Foundry.Room].Visited&&run.Health==68,"Published mine save gains unopened foundry");
            run.Rooms.ConnectionRevision=4;run.Player=new(1100,390);Save();check(Vector2.Distance(run.Player,new(1180,650))<1,"Old wall-strip position recovered onto main floor");
            var link=RoomLinks.All.Last();var route=ConnectedWorld.Route(link);var shift=run.WorldOrigin;
            check(!run.ClearPath(route[0]-shift,route[1]-shift),"Closed foundry gate blocks the arch");
            var across=Vector2.Normalize(ConnectedWorld.Side(route[0],route[1]));
            for(int offset=-100;offset<=100;offset+=10)check(!run.ClearPath(route[0]-shift+across*offset,route[1]-shift+across*offset),"Foundry jambs prevent slipping around closed gate "+offset);
            run.MineState.ImprintTaken=false;Use(Foundry.Gate);check(!run.FoundryState.GateOpen,"Crown clue required before gate");run.MineState.ImprintTaken=true;Use(Foundry.Gate);check(run.FoundryState.GateOpen,"Gate releases after clue");Walk(false);
            var boss=run.Enemies.Single(e=>e.Kind==EnemyKind.CrownBailiff);run.Player=new(700,700);for(int i=0;i<70;i++)run.Step(default);Save();
            Use(Foundry.Valve);check(run.FoundryState.Cooling==4&&run.FoundryState.Quenches==1,"Cooling interrupts active boss");Save();int n=run.FoundryState.Quenches;Use(Foundry.Valve);check(run.FoundryState.Quenches==n,"Cooling cannot be spammed");
            boss=run.Enemies.Single(e=>e.Kind==EnemyKind.CrownBailiff);boss.Health=0;run.FoundryState.BailiffDefeated=true;run.Hazards.Clear();Use(Foundry.Plate);check(run.FoundryState.PlateTaken,"Crown imprint collected after victory");int items=run.Inventory.NextId;Use(Foundry.Plate);check(run.Inventory.NextId==items,"Crown reward cannot duplicate");Save();Walk(true);Walk(false);check(run.Enemies.Count(e=>e.Kind==EnemyKind.CrownBailiff)==1,"Boss does not respawn on return");
        }
        finally{System.IO.File.Delete(file);System.IO.File.Delete(file+".bak");}
    }
}
