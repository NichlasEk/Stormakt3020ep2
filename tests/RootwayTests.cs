using Atland;
using System.Numerics;
using System.Text.Json;

static class RootwayTests
{
    public static void Continue(Combat g,Action<bool,string> check)
    {
        var path=Path.Combine(Path.GetTempPath(),"rootway-"+Guid.NewGuid()+".json");
        void Use(Vector2 at){g.Player=at;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
        Combat Reload(){SaveStore.Write(path,g);return SaveStore.Read(path);}
        try
        {
            g.Rooms!.Rooms.Remove(PortRooms.Roots);g.Rooms.Rooms.Remove(PortRooms.Grove);g.Rooms.LayoutVersion=4;g=Reload();
            check(g.Rooms!.LayoutVersion==5&&g.Rooms.Rooms.Count==9&&g.Rooms.ArchiveSecured,"Old completed archive save gains two unopened rootway rooms");
            Use(RoomLinks.All[2].AtB);Use(RoomLinks.All[4].AtA);Use(RoomLinks.All[5].AtA);Use(PortRooms.OathExit);
            check(g.Rooms.Current==PortRooms.Archive,"Return from cistern to the archive before continuing");
            float hp=g.Health;int potions=g.Potions,choice=g.ArchiveChoice;string inventory=JsonSerializer.Serialize(g.Inventory,new JsonSerializerOptions{IncludeFields=true});
            Use(ArchiveRoom.Seal);
            check(g.Events.Count(e=>e.Kind=="cinematic"&&e.Text=="archive-gate")==1,"First archive passage emits one cinematic cue before checkpoint");
            check(g.Rooms.Current==PortRooms.Roots&&g.OnWalkable(g.Player)&&g.Enemies.Count==0,"Archive gate crosses into quiet rootway");
            check(hp==g.Health&&potions==g.Potions&&inventory==JsonSerializer.Serialize(g.Inventory,new JsonSerializerOptions{IncludeFields=true}),"New world entry does not reset inventory, health, stash or potions");
            Use(Rootway.Exit);check(g.Rooms.Current==PortRooms.Roots,"Counterweight gate blocks the grove until released");
            int ticks=0;
            for(;ticks<9000&&!g.Dead&&g.Rooms.GroveWave==0;ticks++)g.Step(OathCheckPilot.Decide(g));
            check(g.Rooms.Current==PortRooms.Grove&&g.Rooms.RootGateOpen&&g.Rooms.GroveWave==1,"Pilot walks to winch, enters grove and starts rubbing");
            check(!g.OnWalkable(new(790,580))&&!g.ClearPath(new(780,450),Rootway.Memorial),"Memorial has collision and blocks straight shots");
            g=Reload();check(g.Rooms!.GroveWave==1&&g.Enemies.Count==2&&g.ArchiveChoice==choice,"Reload active first wave and canonical archive decision");
            int wave=1;
            for(;ticks<21000&&!g.Dead&&!g.Rooms.GroveSecured;ticks++)
            {
                g.Step(OathCheckPilot.Decide(g));
                if(g.Rooms.GroveWave!=wave){wave=g.Rooms.GroveWave;g=Reload();check(g.Rooms!.GroveWave==wave,"Second wave checkpoint reloads");}
            }
            check(g.Rooms.GroveSecured&&!g.Dead&&!g.DeveloperSurvival,$"Rootway completes with {g.Weapon}/{g.Order}, choice={choice}, hp={g.Health}, pos={g.Player}, wave={g.Rooms.GroveWave}");
            check(g.Rooms.GroveWave==(choice==1?1:2)&&g.Enemies.Count==2*g.GroveWaves,"Archive choice changes the actual number of reinforcements");
            var reward=g.LocalDrops.Single(d=>d.Item.Definition=="norn");int id=reward.Item.Id;int next=g.NextId;
            check(g.Inventory.Bag.Count==Items.BagCapacity,"Reward remains accessible with full bag");
            g=Reload();Use(Rootway.GroveDoor);Use(Rootway.Door);
            check(g.Rooms!.Current==PortRooms.Archive&&g.Inventory.Stash.Any(i=>i.Definition=="iron-cap")&&g.ArchiveChoice==choice,"Return to archive preserves stash and decision");
            Use(ArchiveRoom.Seal);check(!g.Events.Any(e=>e.Kind=="cinematic"),"Revisit does not replay the cinematic");Use(Rootway.Exit);Use(Rootway.Memorial);
            check(g.Rooms.GroveSecured&&g.Enemies.All(e=>e.Dead)&&g.NextId==next&&g.LocalDrops.Count(d=>d.Item.Definition=="norn")==1&&g.LocalDrops.Any(d=>d.Item.Id==id),"No repeated waves or duplicated reward after revisiting");
            g=Reload();g.Rooms!.GroveWave=0;
            bool invalid=false;try{g.ValidateRooms();}catch(InvalidDataException){invalid=true;}
            check(invalid,"Reject completed grove with missing wave history");
            Console.WriteLine($"ROOTWAY ROUTE {g.Weapon}/{g.Order}, choice={choice}: {ticks} ticks, {g.Health:0} health, return and reward intact");
        }
        finally{File.Delete(path);}
    }
}
