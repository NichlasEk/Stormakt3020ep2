using Atland;
using System.Numerics;
using System.Text.Json;

static class ArchiveTests
{
    public static void Run(Action<bool,string> check)
    {
        var root=Path.Combine(Path.GetTempPath(),"atland-archive-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            Combat Save(Combat g,string name){string path=Path.Combine(root,name+".json");SaveStore.Write(path,g);return SaveStore.Read(path);}
            void Use(Combat g,Vector2 at){g.Player=at;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            foreach(var weapon in Enum.GetValues<Weapon>())foreach(var order in Enum.GetValues<Order>())foreach(int choice in new[]{1,2})
            {
                var g=Combat.NewRooms(order);g.Weapon=weapon;int ticks=0;
                for(;ticks<18000&&!g.Dead&&!g.Rooms!.Completed;ticks++)g.Step(OathCheckPilot.Decide(g,choice==1));
                check(g.Rooms!.Completed&&!g.Dead,"Reach opened oath port before archive");
                g.Rooms.LayoutVersion=3;g.Rooms.Rooms.Remove(PortRooms.Archive);g.Rooms.Rooms.Remove(PortRooms.Roots);g.Rooms.Rooms.Remove(PortRooms.Grove);
                g.Inventory.Stash.Add(g.Inventory.Create("iron-cap"));
                g=Save(g,$"legacy-{weapon}-{order}-{choice}");
                check(g.Rooms!.LayoutVersion==5&&g.Rooms.Rooms.Count==9&&!g.Rooms.Rooms[PortRooms.Archive].Visited,"Completed v3 save gains unopened archive");
                int oldNextId=g.NextId;float health=g.Health;int potions=g.Potions;string inventory=JsonSerializer.Serialize(g.Inventory,new JsonSerializerOptions{IncludeFields=true});
                Use(g,PortRooms.OathExit);
                check(g.Rooms.Current==PortRooms.Archive&&g.OnWalkable(g.Player),"Separate E crosses open port to valid arrival");
                check(g.Health==health&&g.Potions==potions&&JsonSerializer.Serialize(g.Inventory,new JsonSerializerOptions{IncludeFields=true})==inventory,"Archive entry preserves health, equipment, stash, drops and potions");
                check(g.NextId==oldNextId&&g.Enemies.Count==0,"Archive starts peacefully");
                check(!g.ChooseRoomArchive(choice)&&!g.ChooseRoomArchive(9),"Unread and invalid decisions are rejected");
                check(!g.OnWalkable(new(780,610))&&!g.ClearPath(new(760,450),new(800,750)),"Reading table is solid and blocks straight attacks");
                Use(g,ArchiveRoom.Desk);check(g.Rooms.ArchiveRead&&g.Events.Any(c=>c.Kind=="archive-open"),"Reading desk opens comparison");
                g=Save(g,$"read-{weapon}-{order}-{choice}");check(g.Rooms!.ArchiveRead&&g.ArchiveChoice==0,"Unchosen reading state survives reload");
                Use(g,ArchiveRoom.Door);Use(g,PortRooms.OathExit);check(g.Rooms.Current==PortRooms.Archive&&g.Enemies.Count==0,"Can leave and return before choosing");
                Use(g,ArchiveRoom.Desk);
                while(g.Inventory.Bag.Count<Items.BagCapacity)g.Inventory.Bag.Add(g.Inventory.Create("helmet"));
                check(g.ChooseRoomArchive(choice),"Decision works with a full bag");
                int[] ids=g.Enemies.Select(e=>e.Id).ToArray();check(ids.Length==(choice==1?3:1),"Decision changes actual patrol composition");
                check(!g.ChooseRoomArchive(3-choice)&&g.Enemies.Select(e=>e.Id).SequenceEqual(ids),"Decision cannot change or duplicate a patrol");
                Use(g,ArchiveRoom.Door);check(g.Rooms.Current==PortRooms.Archive,"Active control cannot be bypassed via return door");
                g=Save(g,$"patrol-{weapon}-{order}-{choice}");check(g.ArchiveChoice==choice&&g.Enemies.Select(e=>e.Id).SequenceEqual(ids),"Patrol identity and decision survive reload");
                for(int battle=0;battle<12000&&!g.Dead&&!g.Rooms!.ArchiveSecured;battle++,ticks++)g.Step(OathCheckPilot.Decide(g));
                check(g.Rooms!.ArchiveSecured&&!g.Dead&&!g.DeveloperSurvival,$"Archive route {weapon}/{order}/{choice}, at={g.Player}, hp={g.Health}, enemies={g.Enemies.Count(e=>!e.Dead)}");
                g=Save(g,$"secured-{weapon}-{order}-{choice}");Use(g,ArchiveRoom.Door);Use(g,PortRooms.OathExit);
                check(g.Rooms!.ArchiveSecured&&g.ArchiveChoice==choice&&g.Enemies.All(e=>e.Dead)&&g.Enemies.Select(e=>e.Id).SequenceEqual(ids),"Secured archive revisits without respawning or resetting choice");
                Use(g,ArchiveRoom.Door);Use(g,RoomLinks.All[5].AtB);Use(g,RoomLinks.All[4].AtB);Use(g,RoomLinks.All[2].AtA);
                check(g.Rooms.Current==PortRooms.Cistern&&g.Inventory.Stash.Any(i=>i.Definition=="iron-cap"),"Return through all old rooms keeps stash");
                if(!g.Rooms.RelicTaken)Use(g,PortRooms.Relic);
                check(g.Rooms.RelicTaken&&g.LocalDrops.Any(d=>d.Item.Definition=="atland-saber"),"Uncollected cistern loot survives the archive decision and full bag");
                RootwayTests.Continue(g,check);
                Console.WriteLine($"ARCHIVE ROUTE {weapon}/{order}, choice={choice}: {ticks} ticks, {g.Health:0} health, decision and return intact");
            }
        }
        finally{Directory.Delete(root,true);}
    }
}
