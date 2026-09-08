using Atland;
using System.Numerics;

public static class RegimentTests
{
    public static void Run(Action<bool,string> check)
    {
        var legacy=Combat.NewRegimentPreview(Order.Medicine);
        foreach(var id in Regiment.Ids.Concat(Mine.Ids).Concat(Uppsala.Ids))legacy.Rooms!.Rooms.Remove(id);
        foreach(var link in RoomLinks.All.Where(l=>Regiment.Known(l.A)||Regiment.Known(l.B)||Mine.Known(l.A)||Mine.Known(l.B)))legacy.Rooms!.Doors.Remove(link.Id);
        legacy.Rooms!.LayoutVersion=5;legacy.Rooms.ConnectionRevision=3;legacy.Health=61;
        var migration=Path.Combine(Path.GetTempPath(),"regiment-migration-"+Guid.NewGuid()+".json");
        try
        {
            SaveStore.Write(migration,legacy);var restored=SaveStore.Read(migration);
            check(restored.Rooms!.LayoutVersion==9&&restored.Rooms.Rooms.Count==20,"Published nine-room saves gain six unvisited rooms");
            check(Regiment.Ids.All(id=>!restored.Rooms.Rooms[id].Visited)&&restored.Health==61&&restored.Rooms.GroveSecured,"Migration preserves earlier story and wounds");
        }
        finally{File.Delete(migration);}
        foreach(int choice in new[]{1,2})
        {
            var g=Combat.NewRegimentPreview(Order.Medicine);g.DeveloperSurvival=true;
            if(choice==2)
            {
                g.ArchiveChoice=2;g.Enemies.RemoveAll(e=>e.HomeRoom==PortRooms.Archive&&e!=g.ActorsInRoom(PortRooms.Archive).First());g.Rooms!.GroveWave=2;
                for(int i=0;i<2;i++){g.Spawn(EnemyKind.Guard,new(1050+i*30,600));g.Enemies[^1].Health=0;}
            }
            g.ValidateRooms();var path=Path.Combine(Path.GetTempPath(),"regiment-"+Guid.NewGuid()+".json");
            void Save(){SaveStore.Write(path,g);g=SaveStore.Read(path);g.ValidateRooms();}
            void Clear(){foreach(var e in g.Enemies.Where(e=>e.Kind!=EnemyKind.RootMarshal))e.Health=0;g.Hazards.Clear();g.Shots.Clear();g.Hurt=g.AttackTime=g.DodgeTime=0;}
            void Use(Vector2 at){g.Player=at;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            void Walk(string id,bool back=false)
            {
                var link=RoomLinks.All.First(l=>l.Id==id);var route=ConnectedWorld.Route(link);if(back)Array.Reverse(route);
                Clear();g.Player=route[0]-g.WorldOrigin;
                foreach(var target in route.Skip(1))
                {
                    int ticks=0;
                    while(Vector2.Distance(g.Player+g.WorldOrigin,target)>5&&ticks++<2500)
                    {var before=g.Player+g.WorldOrigin;var move=Vector2.Normalize(target-before);g.Step(new(move,move,false,false,false,false,false,false,false,false));check(Vector2.Distance(before,g.Player+g.WorldOrigin)<9,"No teleport in new chapter "+id);}
                    check(ticks<2500,$"Regiment passage {id} back={back} traversable, at {g.Player+g.WorldOrigin} target {target}");
                }
                check(g.Rooms!.Current==(back?link.A:link.B),"New chapter arrival "+id);Save();
            }
            try
            {
                Walk("regiment");check(g.Rooms!.Rooms[Regiment.Trail].Visited&&g.ActorsInRoom(Regiment.Trail).Count==2,"New ambush spawns once");
                Walk("barracks");Use(Regiment.Captain);check(g.RegimentState.CaptainMet,"Captain meeting persists");Use(Regiment.Orders);check(g.RegimentState.OrdersTaken,"Order recovered beside table");
                if(choice==1)Use(Regiment.Proof);
                Walk("flags");Use(Regiment.Checkpoint);check(g.RegimentState.MusterPassed,"Both archive decisions pass control");
                check(g.ActorsInRoom(Regiment.Flags).All(e=>e.Retired),"Soldiers lower arms rather than fake combat kills");
                Use(Regiment.Shortcut);for(int i=0;i<80;i++)g.Step(default);Walk("retreat");Walk("retreat",true);
                Walk("parade");check(g.ActorsInRoom(Regiment.Parade).Count(e=>e.Kind==EnemyKind.RootSoldier)==(choice==2?2:0),"Archive choice changes marshal reinforcements");var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.RootMarshal);boss.Health=0;g.RegimentState.MarshalDefeated=true;Clear();Use(Regiment.Discharge);
                check(g.RegimentState.Discharged,"Order completes discharge after marshal defeat");for(int i=0;i<80;i++)g.Step(default);
                Walk("regiment-quay");Clear();Use(Regiment.Boat);check(g.Events.Any(e=>e.Kind=="boat-travel")&&g.Rooms!.Current==Regiment.Quay,"Boarding starts visible crossing, not instant relocation");
                float hp=g.Health;int items=g.Inventory.Drops.Count;check(g.FinishBoatCrossing(Regiment.Farled),"Boat reaches farled");
                check(g.OnWalkable(g.Player)&&g.Health==hp&&g.Inventory.Drops.Count==items,"Boat preserves wounds and loot and lands on ground");Save();
                check(g.FinishBoatCrossing(Regiment.Quay),"Return boat remains usable");Save();Walk("regiment-quay",true);
                int drops=g.Inventory.NextId;Use(Regiment.Discharge);check(g.Inventory.NextId==drops,"Discharge reward cannot duplicate");
                Walk("parade",true);Walk("flags",true);Walk("barracks",true);Walk("regiment",true);
                Console.WriteLine($"REGIMENT ROUTE choice={choice}: all rooms, return shortcut, boat and saves");
            }
            finally{File.Delete(path);}
        }
        foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})foreach(var order in new[]{Order.Artillery,Order.Medicine})
        {
            var g=Combat.NewRegimentPreview(order);g.Rooms!.Current=Regiment.Parade;g.Rooms.Rooms[Regiment.Parade].Visited=true;g.Enemies.Clear();g.Player=new(750,780);g.Weapon=weapon;
            g.Spawn(EnemyKind.RootMarshal,new(800,520));var boss=g.Enemies.Single();int ticks=0,windows=0;bool exposed=false;
            for(;ticks<36000&&!g.Dead&&!boss.Dead;ticks++)
            {
                var target=g.RegimentState.Exposed>0?boss.Position:Regiment.Standards.Where((p,i)=>g.RegimentState.Standards[i]>0).OrderBy(p=>Vector2.DistanceSquared(p,g.Player)).FirstOrDefault(boss.Position);
                var delta=target-g.Player;var move=delta.Length()>63?Combat.Normal(g.NextWaypoint(g.Player,target)-g.Player,Vector2.UnitX):Vector2.Zero;
                var danger=g.Hazards.FirstOrDefault(h=>!h.Friendly&&h.Timer<.55f&&Vector2.Distance(g.Player,h.Position)<h.Radius+25);
                if(danger!=null)move=Combat.Normal(g.Player-danger.Position,Vector2.UnitX);
                bool attack=delta.Length()<85&&g.ClearPath(g.Player,target)&&danger==null;
                g.Step(new(move,Combat.Normal(delta,Vector2.UnitX),attack,false,danger!=null&&danger.Timer<.3f,false,false,g.Health<55,g.RegimentState.Exposed>0,false));
                if(g.RegimentState.Exposed>0&&!exposed)windows++;exposed=g.RegimentState.Exposed>0;
            }
            check(boss.Dead&&!g.Dead&&!g.DeveloperSurvival,$"Rotmarskalk playable {weapon}/{order} hp={g.Health} boss={boss.Health} ticks={ticks} pos={g.Player}");
            check(windows>0&&g.RegimentState.MarshalDefeated,"Breaking standards creates attack windows and real defeat");
            Console.WriteLine($"MARSHAL {weapon}/{order}: {ticks} ticks, hp {g.Health}, {windows} openings");
        }
    }
}
