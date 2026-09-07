using Atland;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

static class WaterTests
{
    public static void Run(Action<bool,string> check)
    {
        var root=Path.Combine(Path.GetTempPath(),"atland-water-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            Combat Save(Combat game,string name){var path=Path.Combine(root,name+".json");SaveStore.Write(path,game);return SaveStore.Read(path);}
            void Use(Combat game,Vector2 at){game.Player=at;game.Step(default);game.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            var game=Combat.NewRooms(Order.Medicine);game.Enemies.Clear();game.Rooms!.KeyTaken=game.Rooms.DoorOpen=true;
            Use(game,RoomLinks.All[3].AtB);check(game.Rooms.Current==PortRooms.Court&&!game.Rooms.ShortcutOpen,"Shortcut is locked from courtyard side");
            Use(game,PortRooms.CourtDoor);foreach(var e in game.Enemies)e.Health=0;
            Use(game,RoomLinks.All[1].AtA);check(game.Rooms.Current==PortRooms.Lodge,"Plans gate blocks leaving without clue");
            Use(game,PortRooms.Cache);Use(game,RoomLinks.All[1].AtA);
            check(game.Rooms.Current==PortRooms.Pump&&game.Enemies.Count==2,"Pump encounter spawns once");
            check(game.OnWalkable(game.Player),"Pump arrival is on measured floor");
            check(!game.OnWalkable(new(760,485))&&!game.ClearPath(new(555,470),new(975,485)),"Pump basin blocks movement and straight attacks");
            foreach(var e in game.Enemies)e.Health=0;
            Use(game,PortRooms.Wheel);check(!game.Rooms.WaterLowered,"Wheel cannot drain a pressurised system");
            Use(game,RoomLinks.All[2].AtA);check(game.Rooms.Current==PortRooms.Pump,"Flooded stair blocks cistern entry");
            Use(game,PortRooms.Pressure);game=Save(game,"pressure");check(game.Rooms!.PressureReleased&&!game.Rooms.WaterLowered,"Partial mechanism state survives saving");
            Use(game,PortRooms.Wheel);game=Save(game,"drained");check(game.Rooms!.WaterLowered,"Drained state survives saving");
            Use(game,RoomLinks.All[2].AtA);check(game.Rooms.Current==PortRooms.Cistern&&game.Enemies.Count==0,"Cistern is a quiet optional room");
            check(game.OnWalkable(game.Player),"Cistern arrival is clear");
            check(!game.OnWalkable(new(780,535))&&!game.ClearPath(new(485,530),new(1080,530)),"Cistern basin blocks movement and straight attacks");
            Use(game,RoomLinks.All[3].AtA);check(game.Rooms.ShortcutOpen&&game.Rooms.Current==PortRooms.Cistern,"Shortcut unbars from inside without requiring optional relic");
            Use(game,RoomLinks.All[3].AtA);check(game.Rooms.Current==PortRooms.Court&&!game.Rooms.RelicTaken,"Optional relic can be skipped");
            game=Save(game,"shortcut");Use(game,RoomLinks.All[3].AtB);check(game.Rooms!.Current==PortRooms.Cistern,"Shortcut works in reverse after reload");
            while(game.Inventory.Bag.Count<Items.BagCapacity)game.Inventory.Bag.Add(game.Inventory.Create("helmet"));
            Use(game,PortRooms.Relic);int id=game.LocalDrops.Single().Item.Id;
            Use(game,PortRooms.Relic);check(game.Rooms.RelicTaken&&game.LocalDrops.Count()==1,"Optional reward cannot duplicate with full bag");
            Use(game,RoomLinks.All[3].AtA);check(!game.LocalDrops.Any(),"Cistern reward is not drawn in courtyard");
            Use(game,RoomLinks.All[3].AtB);check(game.LocalDrops.Single().Item.Id==id,"Optional reward stays available on return");
            var old=Combat.NewRooms(Order.Artillery);old.Enemies[0].Health=42;
            var options=new JsonSerializerOptions{IncludeFields=true};var node=JsonSerializer.SerializeToNode(old,options)!;
            node["Rooms"]!.AsObject().Remove("LayoutVersion");var dict=node["Rooms"]!["Rooms"]!.AsObject();dict.Remove(PortRooms.Pump);dict.Remove(PortRooms.Cistern);dict.Remove(PortRooms.Gallery);dict.Remove(PortRooms.Chamber);
            var migrated=Save(node.Deserialize<Combat>(options)!,"legacy");
            check(migrated.Rooms!.LayoutVersion==3&&migrated.Rooms.Rooms.Count==6&&migrated.Enemies[0].Health==42&&!migrated.Rooms.Rooms[PortRooms.Pump].Visited,"Old two-room save gains unopened rooms without resetting progress");
            foreach(var order in Enum.GetValues<Order>())
            {
                var run=Combat.NewRooms(order);int ticks=0;
                for(;ticks<30000&&!run.Dead;ticks++)
                {
                    if(run.Rooms!.ShortcutOpen&&run.Rooms.Current==PortRooms.Court)break;
                    var foe=run.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,run.Player)).FirstOrDefault();
                    var target=foe?.Position??(run.Rooms!.Current==PortRooms.Pump&&run.Rooms.WaterLowered?RoomLinks.All[2].AtA:run.RoomObjective);var delta=target-run.Player;float distance=delta.Length();
                    bool guard=foe is not null&&foe.State==1&&foe.Timer<.16f&&distance<150;
                    var move=distance>48||!run.ClearPath(run.Player,target)?Combat.Normal(run.NextWaypoint(run.Player,target)-run.Player,Vector2.UnitX):Vector2.Zero;
                    run.Step(new(move,Combat.Normal(delta,Vector2.UnitX),foe is not null&&distance<95&&!guard,foe is not null&&distance<105&&ticks%47==0&&!guard,false,guard,false,run.Health<48,foe is not null,ticks%30==0));
                }
                check(!run.Dead&&!run.DeveloperSurvival&&run.Rooms!.WaterLowered&&run.Rooms.RelicTaken&&run.Rooms.ShortcutOpen&&run.Rooms.Current==PortRooms.Court,$"Complete four-room loop: {order}, room={run.Rooms!.Current}, hp={run.Health}, ticks={ticks}, at={run.Player}, goal={run.RoomObjective}");
                Console.WriteLine($"WATER ROUTE {order}: {ticks} ticks, {run.Kills} kills, {run.Health:0} health");
            }
        }
        finally{Directory.Delete(root,true);}
    }
}
