using Atland;
using System.Numerics;
public static class MineTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var room in Mine.Ids.Take(2))foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})foreach(var order in new[]{Order.Artillery,Order.Medicine})
        {
            var fight=Combat.NewMinePreview(order);fight.Rooms!.Current=room;fight.Player=new(360,660);fight.Weapon=weapon;fight.Enemies.Clear();
            fight.Spawn(room==Mine.Mouth?EnemyKind.Guard:EnemyKind.Pikeman,new(850,720));fight.Spawn(room==Mine.Mouth?EnemyKind.Gunner:EnemyKind.Guard,new(1130,630));
            int ticks=0;
            while(!fight.Dead&&fight.Enemies.Any(e=>!e.Dead)&&ticks++<9000)
            {
                var input=OathCheckPilot.Decide(fight);var danger=fight.Hazards.FirstOrDefault(h=>!h.Friendly&&h.Timer<.5f&&Vector2.Distance(h.Position,fight.Player)<h.Radius+20);
                if(danger!=null)input=input with{Move=Combat.Normal(fight.Player-danger.Position,Vector2.UnitX),Dodge=danger.Timer<.3f,Attack=false,Guard=false};
                fight.Step(input);
            }
            check(!fight.Dead&&fight.Enemies.All(e=>e.Dead)&&!fight.DeveloperSurvival,$"Mine combat {room} {weapon}/{order}: ticks={ticks}, health={fight.Health}");
            Console.WriteLine($"MINE COMBAT {room} {weapon}/{order}: {ticks} ticks, {fight.Health} health");
        }
        var g=Combat.NewMinePreview(Order.Medicine);var path=Path.Combine(Path.GetTempPath(),"mine-"+Guid.NewGuid()+".json");
        void Save(){SaveStore.Write(path,g);g=SaveStore.Read(path);g.ValidateRooms();}
        void Use(Vector2 at){g.Player=at;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
        void Clear(){foreach(var e in g.Enemies)e.Health=0;g.Hazards.Clear();g.Shots.Clear();g.AttackTime=g.Hurt=g.DodgeTime=0;}
        void Walk(string id,bool back=false)
        {
            var l=RoomLinks.All.First(l=>l.Id==id);var route=ConnectedWorld.Route(l);if(back)Array.Reverse(route);Clear();g.Player=route[0]-g.WorldOrigin;
            foreach(var target in route.Skip(1))
            {
                int ticks=0;while(Vector2.Distance(g.Player+g.WorldOrigin,target)>5&&ticks++<3000)
                {var before=g.Player+g.WorldOrigin;var move=Vector2.Normalize(target-before);g.Step(new(move,move,false,false,false,false,false,false,false,false));check(Vector2.Distance(before,g.Player+g.WorldOrigin)<9,"Mine walking never teleports");}
                check(ticks<3000,$"Mine path {id} back={back} at {g.Player+g.WorldOrigin} target {target}");
            }
            check(g.Rooms!.Current==(back?l.A:l.B),"Mine arrival "+id);Save();
        }
        try
        {
            // Upgrade the published 15-room shape while preserving its completed regiment.
            foreach(var id in Mine.Ids.Concat(Uppsala.Ids))g.Rooms!.Rooms.Remove(id);
            foreach(var l in RoomLinks.All.Skip(15))g.Rooms!.Doors.Remove(l.Id);
            foreach(var fresh in Observatory.Ids.Concat(Gamla.Ids))g.Rooms!.Rooms.Remove(fresh);foreach(var fresh in RoomLinks.All.Where(l=>l.Id.StartsWith("observatory-")||(l.Id.StartsWith("gamla-")||(l.Id.StartsWith("salt-")||l.Id.StartsWith("rescue-")))))g.Rooms!.Doors.Remove(fresh.Id);g.Rooms!.LayoutVersion=6;g.Health=73;Save();
            check(g.Rooms!.LayoutVersion==16&&g.Rooms.Rooms.Count==41&&g.Health==73&&g.RegimentState.FarledReached,"Published regiment saves gain unexplored mine without resetting story or health");
            check(Mine.Ids.All(id=>!g.Rooms.Rooms[id].Visited),"New mine starts unexplored");
            var gate=RoomLinks.All.First(l=>l.Id=="mine-entry");var center=ConnectedWorld.Center(gate)-g.WorldOrigin;
            check(!g.ClearPath(center-new Vector2(0,60),center+new Vector2(0,60)),"Closed painted mine gate blocks movement and sight");
            Use(Mine.Latch);check(g.MineState.EntranceOpen,"Landing latch opens mine");Walk("mine-entry");
            Clear();Use(Mine.Ledger);check(g.MineState.LedgerRead,"Ledger read and saved");Walk("mine-bellows");Clear();
            Use(Mine.Relief);check(!g.MineState.PressureReleased,"Wrong valve order cannot release pressure");
            g.Player=Mine.Vents[0];g.MineState.Pulse=.01f;g.MineState.Vent=0;g.Health=100;g.Spawn(EnemyKind.Guard,g.Player+new Vector2(10,0));var guard=g.Enemies[^1];guard.State=2;guard.Timer=3;var old=guard.Health;
            g.Step(default);check(g.Hazards.Any(h=>h.Steam&&h.Timer>.9f),"Steam announces itself before impact");
            for(int i=0;i<70;i++)g.Step(default);
            check(g.Health<100&&guard.Health<old,"Steam hurts player and guards");Clear();
            Use(Mine.Feed);check(g.MineState.FeedClosed&&!g.MineState.PressureReleased,"Feed closure is separate from venting");Save();
            Use(Mine.Relief);check(g.MineState.PressureReleased&&!g.Hazards.Any(h=>h.Steam),"Relief clears remaining steam warnings");
            for(int i=0;i<350;i++)g.Step(default);check(!g.Hazards.Any(h=>h.Steam),"Released pressure stays safe");
            Walk("mine-coolway");Use(Mine.Imprint);check(g.MineState.ImprintTaken,"Crown clue reached");int item=g.Inventory.NextId;Use(Mine.Imprint);check(g.Inventory.NextId==item,"Crown reward cannot duplicate");
            Use(Mine.Shortcut);for(int i=0;i<80;i++)g.Step(default);check(g.MineState.ShortcutOpen,"Maintenance return unlocked");Walk("mine-return");Walk("mine-return",true);
            Walk("mine-coolway",true);Walk("mine-bellows",true);Walk("mine-entry",true);check(g.MineState.ImprintTaken,"Story persists on walking return to boat");
            g.Player=Regiment.LandingBoat;check(g.FinishBoatCrossing(Regiment.Quay),"Return boat works after mine");Save();
            check(g.FinishBoatCrossing(Regiment.Farled),"Mine remains reachable by boat");Save();
            Console.WriteLine("MINE ROUTE: pressure, both-way traversal, shortcut, crown clue, boat and save migration");
        }
        finally{File.Delete(path);File.Delete(path+".bak");}
    }
}
