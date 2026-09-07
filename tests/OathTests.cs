using Atland;
using System.Numerics;
using System.Text.Json;

static class OathTests
{
    public static void Run(Action<bool,string> check)
    {
        var root=Path.Combine(Path.GetTempPath(),"atland-oath-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            Combat Save(Combat g,string name){var path=Path.Combine(root,name+".json");SaveStore.Write(path,g);return SaveStore.Read(path);}
            void Use(Combat g,Vector2 p){g.Player=p;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            var g=Combat.NewRooms(Order.Medicine);g.Enemies.Clear();g.Rooms!.KeyTaken=g.Rooms.DoorOpen=true;
            Use(g,PortRooms.CourtDoor);g.Enemies.Clear();Use(g,PortRooms.Cache);Use(g,RoomLinks.All[1].AtA);g.Enemies.Clear();
            Use(g,RoomLinks.All[4].AtA);check(g.Rooms.Current==PortRooms.Pump,"Gallery remains water-locked");
            Use(g,PortRooms.Pressure);Use(g,PortRooms.Wheel);Use(g,RoomLinks.All[4].AtA);
            check(g.Rooms.Current==PortRooms.Gallery&&g.Enemies.Count==0&&g.OnWalkable(g.Player),"Gallery is a peaceful discovery room with a clear entrance");
            Use(g,RoomLinks.All[5].AtA);check(g.Rooms.Current==PortRooms.Gallery,"Witness seal blocks chamber before reading");
            check(!g.OnWalkable(new(800,666)),"Lectern is solid");
            Use(g,PortRooms.Witness);g=Save(g,"witness");check(g.Rooms!.WitnessRead,"Witness clue survives reload");
            Use(g,RoomLinks.All[5].AtA);check(g.Rooms.Current==PortRooms.Chamber&&g.Enemies.Single().Kind==EnemyKind.OathGuardian,"Chamber spawns its own boss");
            Use(g,RoomLinks.All[5].AtB);check(g.Rooms.Current==PortRooms.Chamber,"Cannot bypass living guardian by leaving");
            var boss=g.Enemies.Single();boss.Alerted=true;boss.Position=new(570,450);boss.Cooldown=0;g.Player=new(570,750);g.HitStop=0;
            check(!g.ClearPath(boss.Position,g.Player)&&!g.OnWalkable(PortRooms.Pillars[0]),"Oath pillar blocks sight and attacks");
            check(g.OnWalkable(g.NextWaypoint(g.Player,boss.Position)),"Navigation supplies a walkable route around pillars");
            for(int i=0;i<250&&boss.State!=4;i++)g.Step(default);
            check(boss.State==4,"Guardian commits to a locked shield rush");
            g=Save(g,"mid-rush");boss=g.Enemies.Single();
            for(int i=0;i<120&&boss.State!=3;i++)g.Step(default);
            check(boss.State==3&&g.OnWalkable(boss.Position)&&g.Health==100,"Reloaded rush hits pillar and exposes guardian without tunnelling");
            float hit(Combat fight,Fighter enemy,int state)
            {
                fight.HitStop=fight.AttackTime=fight.AttackBuffer=0;fight.Combo=0;fight.Player=enemy.Position-new Vector2(70,0);enemy.State=state;enemy.Timer=4;enemy.Cooldown=4;
                float hp=enemy.Health;for(int i=0;i<18;i++)fight.Step(new(default,Vector2.UnitX,i==0,false,false,false,false,false,false,false));return hp-enemy.Health;
            }
            float protectedHit=hit(g,boss,2),exposedHit=hit(g,boss,3);
            check(exposedHit>protectedHit*8&&protectedHit>0,"Pillar exposure changes real saber damage, armor never becomes an absolute soft lock");
            boss.State=1;boss.Timer=.01f;boss.Position=new(800,450);boss.Facing=Vector2.UnitY;boss.ChargeHit=false;g.Player=new(800,560);g.Invulnerable=0;g.Health=100;g.HitStop=0;
            for(int i=0;i<80;i++)g.Step(default);
            check(g.Health<100&&g.Health>=70,"A missed bait causes at most one shield impact per rush");
            var old=Combat.NewRooms(Order.Artillery);old.Rooms!.LayoutVersion=2;old.Rooms.Rooms.Remove(PortRooms.Gallery);old.Rooms.Rooms.Remove(PortRooms.Chamber);old.Rooms.Rooms.Remove(PortRooms.Archive);old.Rooms.Rooms.Remove(PortRooms.Roots);old.Rooms.Rooms.Remove(PortRooms.Grove);
            old.Enemies[0].Health=31;old=Save(old,"water-migration");check(old.Rooms!.LayoutVersion==5&&old.Rooms.Rooms.Count==9&&old.Enemies[0].Health==31,"Four-room saves gain the last two rooms without resetting guards");
            foreach(var weapon in Enum.GetValues<Weapon>())foreach(var order in Enum.GetValues<Order>())foreach(bool optional in new[]{false,true})
            {
                var run=Combat.NewRooms(order);run.Weapon=weapon;int ticks=0,exposures=0,previous=0;
                for(;ticks<40000&&!run.Dead&&!run.Rooms!.Completed;ticks++)
                {
                    run.Step(OathCheckPilot.Decide(run,optional));
                    int state=run.Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.OathGuardian)?.State??0;
                    if(state==3&&previous!=3)exposures++;previous=state;
                }
                check(run.Rooms!.Completed&&!run.Dead&&!run.DeveloperSurvival&&exposures>0,$"Oath route {weapon}/{order}/{optional}: room={run.Rooms.Current}, hp={run.Health}, ticks={ticks}, at={run.Player}, boss={run.Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.OathGuardian)?.Health}, exposures={exposures}");
                check(run.Rooms.Rooms[PortRooms.Cistern].Visited==optional,"Optional cistern is not required for completion");
                run=Save(run,$"finished-{weapon}-{order}-{optional}");int id=run.Enemies.Single(e=>e.Kind==EnemyKind.OathGuardian).Id;
                int rewards=run.Inventory.Bag.Count(i=>i.Definition=="crown-helm")+run.LocalDrops.Count(d=>d.Item.Definition=="crown-helm");
                check(rewards==1,"Guardian grants exactly one helmet");
                Use(run,RoomLinks.All[5].AtB);Use(run,RoomLinks.All[5].AtA);
                check(run.Rooms!.Completed&&run.Enemies.Single(e=>e.Kind==EnemyKind.OathGuardian).Id==id&&run.Enemies.All(e=>e.Dead),"Completed chamber can be revisited without respawn");
                Console.WriteLine($"OATH ROUTE {weapon}/{order}, cistern={optional}: {ticks} ticks, {exposures} pillar impacts, {run.Health:0} health");
            }
        }
        finally{Directory.Delete(root,true);}
    }
}
