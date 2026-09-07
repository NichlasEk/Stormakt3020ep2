using Atland;
using System.Numerics;

static class RoomTests
{
    public static void Run(Action<bool,string> check)
    {
        var root=Path.Combine(Path.GetTempPath(),"atland-rooms-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            Combat RoundTrip(Combat game,string name)
            {var path=Path.Combine(root,name+".json");SaveStore.Write(path,game);return SaveStore.Read(path);}
            void Use(Combat game,Vector2 at)
            {game.Player=at;game.Step(default);game.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            var door=PortRooms.CourtDoor+new Vector2(0,45);
            var game=Combat.NewRooms(Order.Medicine);
            game.Enemies[0].Health=37;game=RoundTrip(game,"injured");
            check(game.Enemies.Single().Health==37,"Injured room enemy survives saving");
            Use(game,door);check(!game.Rooms!.DoorOpen&&game.Rooms.Current==PortRooms.Court,"Locked door rejects passage");
            Use(game,PortRooms.Key);check(!game.Rooms.KeyTaken,"Key cannot be searched during combat");
            foreach(var e in game.Enemies)e.Health=0;
            while(game.Inventory.Bag.Count<Items.BagCapacity)game.Inventory.Bag.Add(game.Inventory.Create("helmet"));
            Use(game,PortRooms.Key);check(game.Rooms.KeyTaken,"Quest key cannot be blocked by full inventory");
            game=RoundTrip(game,"key");check(game.Rooms!.KeyTaken&&!game.Rooms.DoorOpen,"Key and lock save independently");
            Use(game,door);check(game.Rooms.DoorOpen&&game.Rooms.Current==PortRooms.Court,"Unlock does not also cross threshold");
            for(int i=0;i<30;i++)game.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));
            check(game.Rooms.Current==PortRooms.Court,"Held interact cannot cross after unlock");
            game=RoundTrip(game,"unlocked");
            game.Health=43;int potions=game.Potions;int itemCount=game.Inventory.Bag.Count;
            Use(game,door);check(game.Rooms!.Current==PortRooms.Lodge&&game.Enemies.Count==2,"First entry creates the lodge encounter");
            check(game.Health==43&&game.Potions==potions&&game.Inventory.Bag.Count==itemCount,"Passage grants no free healing or items");
            check(game.OnWalkable(game.Player)&&game.ClearPath(game.Player,PortRooms.LodgeDoor),"Lodge arrival has clear doorway access");
            check(!game.ClearPath(new(650,580),new(925,580)),"Painted central obstacle blocks attacks and projectiles");
            Use(game,PortRooms.LodgeDoor+new Vector2(-25,35));check(game.Rooms.Current==PortRooms.Lodge,"Active encounter prevents doorway escape");
            foreach(var e in game.Enemies)e.Health=0;
            game.Hurt=game.HitStop=0;game.Shots.Clear();game.Hazards.Clear();
            Use(game,PortRooms.Cache);check(game.Rooms.CacheTaken&&game.LocalDrops.Count()==1,"Cache creates one ground item with full bag");
            var drop=game.LocalDrops.Single();int id=drop.Item.Id;
            check(game.PickUpItem(id)!=""&&game.LocalDrops.Count()==1,"Full bag leaves room loot in place");
            game=RoundTrip(game,"cache");check(game.LocalDrops.Single().Item.Id==id,"Ground loot identity survives load");
            Use(game,PortRooms.Cache);check(game.LocalDrops.Count()==1,"Cache cannot duplicate its reward");
            Use(game,PortRooms.LodgeDoor+new Vector2(-25,35));
            check(game.Rooms!.Current==PortRooms.Court&&!game.LocalDrops.Any()&&game.Enemies.Single().Dead,"Return restores cleared court and hides lodge loot");
            game=RoundTrip(game,"return");
            Use(game,door);check(game.Enemies.Count==2&&game.Enemies.All(e=>e.Dead)&&game.LocalDrops.Single().Item.Id==id,"Revisit restores cleared lodge without respawning loot");
            game.Inventory.Bag.RemoveAt(game.Inventory.Bag.Count-1);game.Player=PortRooms.Cache;
            check(game.PickUpItem(id)==""&&!game.LocalDrops.Any(),"Return permits retrieving previously abandoned item");
            var restored=RoundTrip(game,"picked-up");check(restored.Inventory.Bag.Any(i=>i.Id==id)&&!restored.LocalDrops.Any(),"Picked-up item has exactly one owner after saving");
            var old=RoundTrip(Combat.NewAtland(Order.Artillery),"old");check(!old.InRooms,"Existing campaign does not acquire room mode");
            void Reject(Action<Combat> corrupt,string name)
            {
                var invalid=RoundTrip(restored,"copy-"+name);corrupt(invalid);
                try{RoundTrip(invalid,"invalid-"+name);check(false,"Reject corrupt rooms: "+name);}
                catch(InvalidDataException){check(true,"Reject corrupt rooms: "+name);}
            }
            Reject(g=>g.Rooms!.Current="missing","unknown-room");
            Reject(g=>g.Rooms!.Rooms[PortRooms.Court].Enemies.Add(g.Enemies[0]),"duplicate-actor");
            Reject(g=>g.Rooms!.KeyTaken=false,"impossible-lock");
            Reject(g=>g.Rooms!.Rooms.Remove(PortRooms.Court),"missing-snapshot");
            foreach(var order in Enum.GetValues<Order>())
            {
                var run=Combat.NewRooms(order);int ticks=0;
                for(;ticks<24000&&!run.Dead;ticks++)
                {
                    if(run.Rooms!.CacheTaken&&run.Rooms.Current==PortRooms.Court)break;
                    var foe=run.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,run.Player)).FirstOrDefault();
                    var target=foe?.Position??run.RoomObjective;var delta=target-run.Player;float distance=delta.Length();
                    bool guard=foe is not null&&foe.State==1&&foe.Timer<.16f&&distance<150;
                    var waypoint=run.NextWaypoint(run.Player,target)-run.Player;
                    var move=distance>55||!run.ClearPath(run.Player,target)?Combat.Normal(waypoint,Vector2.UnitX):Vector2.Zero;
                    run.Step(new(move,Combat.Normal(delta,Vector2.UnitX),foe is not null&&distance<95&&!guard,
                        foe is not null&&distance<105&&ticks%47==0&&!guard,false,guard,false,run.Health<48,foe is not null,ticks%30==0));
                }
                check(!run.DeveloperSurvival&&!run.Dead&&run.Rooms!.CacheTaken&&run.Rooms.Current==PortRooms.Court,
                    $"Complete room route with normal combat ({order}, hp={run.Health}, ticks={ticks}, room={run.Rooms!.Current})");
                Console.WriteLine($"ROOM ROUTE {order}: {ticks} ticks, {run.Kills} kills, {run.Health:0} health");
            }
        }
        finally{Directory.Delete(root,true);}
    }
}
