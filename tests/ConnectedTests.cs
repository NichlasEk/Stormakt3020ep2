using Atland;
using System.Numerics;
public static class ConnectedTests
{
    public static void Run(Action<bool,string> check)
    {
        var g=Combat.NewRooms(Order.Artillery);g.EnableConnectedWorld();g.DeveloperSurvival=true;
        check(g.InConnectedWorld&&g.Rooms!.Doors.Count==RoomLinks.All.Length,"All nine connections have persistent gates");
        void Clear(){foreach(var e in g.Enemies)e.Health=0;g.Shots.Clear();g.Hazards.Clear();g.Hurt=g.AttackTime=g.DodgeTime=0;}
        void Use(Vector2 p){Clear();g.Player=p;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
        void Walk(RoomLink link,string destination)
        {
            var route=ConnectedWorld.Route(link);if(destination==link.A)Array.Reverse(route);
            g.Player=route[0]-g.WorldOrigin;var originalInventory=g.Inventory;
            foreach(var target in route.Skip(1))
            {
                int steps=0;
                while(Vector2.Distance(g.Player+g.WorldOrigin,target)>5&&steps++<3000)
                {
                    var before=g.Player+g.WorldOrigin;var move=Vector2.Normalize(target-before);
                    g.Step(new(move,move,false,false,false,false,false,false,false,false));
                    check(Vector2.Distance(before,g.Player+g.WorldOrigin)<9,"Crossing a room boundary never teleports Karl");
                }
                check(steps<3000,"Physical passage is traversable: "+link.Id+" -> "+destination+" at "+(g.Player+g.WorldOrigin)+" target "+target);
            }
            check(g.Rooms!.Current==destination,"Walk arrives in "+destination);
            check(ReferenceEquals(originalInventory,g.Inventory),"Inventory survives continuous traversal");
        }
        Use(PortRooms.Key);check(g.Rooms!.KeyTaken,"Key still works in connected route");
        var first=RoomLinks.All[0];var center=ConnectedWorld.Center(first);
        check(!g.ClearPath(center-g.WorldOrigin-new Vector2(60,0),center-g.WorldOrigin+new Vector2(60,0)),"Locked gate blocks passage");
        Use(center-g.WorldOrigin+new Vector2(0,85));
        for(int i=0;i<70;i++)g.Step(default);
        check(g.Rooms.DoorOpen&&g.Rooms.Doors["lodge"].Openness>.99f,"Key opens physical oak door");
        g.Shots.Add(new(){Position=g.Player,Velocity=Vector2.Zero,Life=1000,Reflected=true});var shotWorld=g.Shots[0].Position+g.WorldOrigin;
        Walk(first,PortRooms.Lodge);check(g.Shots.Count==1&&Vector2.Distance(g.Shots[0].Position+g.WorldOrigin,shotWorld)<1,"Room boundary preserves live projectile position and lifetime");Clear();Use(PortRooms.Cache);
        Walk(RoomLinks.All[1],PortRooms.Pump);Clear();Use(PortRooms.Pressure);Use(PortRooms.Wheel);
        Walk(RoomLinks.All[2],PortRooms.Cistern);Use(PortRooms.Relic);
        var shortcut=RoomLinks.All[3];Use(ConnectedWorld.Route(shortcut)[0]-g.WorldOrigin);for(int i=0;i<75;i++)g.Step(default);
        Walk(shortcut,PortRooms.Court);Walk(shortcut,PortRooms.Cistern);Walk(RoomLinks.All[2],PortRooms.Pump);
        Walk(RoomLinks.All[4],PortRooms.Gallery);Use(PortRooms.Witness);Walk(RoomLinks.All[5],PortRooms.Chamber);
        Clear();g.Rooms.OathDefeated=true;Walk(RoomLinks.All[5],PortRooms.Gallery);Walk(RoomLinks.All[5],PortRooms.Chamber);Use(PortRooms.OathExit);for(int i=0;i<80;i++)g.Step(default);
        Walk(RoomLinks.All[6],PortRooms.Archive);Use(ArchiveRoom.Desk);check(g.ChooseRoomArchive(1),"Archive choice works");Clear();Use(ArchiveRoom.Seal);for(int i=0;i<80;i++)g.Step(default);
        Walk(RoomLinks.All[7],PortRooms.Roots);Use(Rootway.Winch);for(int i=0;i<80;i++)g.Step(default);
        Walk(RoomLinks.All[8],PortRooms.Grove);Clear();Walk(RoomLinks.All[8],PortRooms.Roots);Walk(RoomLinks.All[7],PortRooms.Archive);Walk(RoomLinks.All[7],PortRooms.Roots);Walk(RoomLinks.All[8],PortRooms.Grove);check(PortRooms.Ids.All(id=>g.Rooms.Rooms[id].Visited),"Every room is connected and visited");
        // Save halfway along a passage, with actors and loot belonging to distant rooms.
        var path=Path.Combine(Path.GetTempPath(),"atland-connected-"+Guid.NewGuid()+".json");
        g.Player=ConnectedWorld.Center(RoomLinks.All[8])-g.WorldOrigin;var saved=g.Player;SaveStore.Write(path,g);var restored=SaveStore.Read(path);
        check(Vector2.Distance(saved,restored.Player)<1,"Resume keeps position inside corridor");
        check(restored.Enemies.Count==g.Enemies.Count&&restored.Inventory.Drops.Count==g.Inventory.Drops.Count,"Resume preserves all room actors and drops");
        check(restored.Rooms!.CorridorSeen.SetEquals(g.Rooms.CorridorSeen),"Passage exploration persists");File.Delete(path);
        var frame=g.WorldOrigin-ConnectedWorld.Origin(PortRooms.Gallery);foreach(var e in g.Enemies){e.Position+=frame;e.LockedAim+=frame;}foreach(var d in g.Inventory.Drops)d.Position+=frame;
        g.Rooms.Current=PortRooms.Gallery;g.Rooms.ConnectionRevision=1;g.Player=new(1750,800);var oldHealth=g.Health;
        var movedDrop=g.Inventory.Drops.First(d=>d.Room==PortRooms.Gallery||d.Room==PortRooms.Lodge);movedDrop.Room=PortRooms.Gallery;movedDrop.Position=g.Player;
        SaveStore.Write(path,g);restored=SaveStore.Read(path);
        check(restored.Rooms!.ConnectionRevision==ConnectedWorld.Revision&&restored.OnWalkable(restored.Player),"Old bridge save migrates to walkable room floor");
        check(restored.Health==oldHealth&&restored.Inventory.Drops.All(d=>restored.OnWalkable(d.Position)),"Moved bridge preserves wounds and reachable loot");File.Delete(path);
        // Revision two moved only the first arch. Moving the later passages must
        // preserve its exploration while recovering a player and loot on the old bridge.
        var oldFrame=g.WorldOrigin-ConnectedWorld.Origin(PortRooms.Archive);
        foreach(var foe in g.Enemies){foe.Position+=oldFrame;foe.LockedAim+=oldFrame;}
        foreach(var drop in g.Inventory.Drops)drop.Position+=oldFrame;
        g.Rooms.Current=PortRooms.Archive;g.Rooms.ConnectionRevision=2;
        g.Player=new Vector2(12355,3092.5f)-g.WorldOrigin;
        movedDrop.Room=PortRooms.Archive;movedDrop.Position=g.Player;
        g.Rooms.CorridorSeen.Add("chamber:2:0");g.Rooms.CorridorSeen.Add("roots:2:0");
        SaveStore.Write(path,g);restored=SaveStore.Read(path);
        check(restored.OnWalkable(restored.Player)&&restored.Inventory.Drops.All(d=>restored.OnWalkable(d.Position)),"Revision two recovers player and loot from removed root bridge");
        check(restored.Rooms!.CorridorSeen.Contains("chamber:2:0")&&!restored.Rooms.CorridorSeen.Contains("roots:2:0"),"Migration preserves unchanged arch exploration");File.Delete(path);
        // Migrate a legacy two-room save without respawning actors or changing resources.
        var old=Combat.NewRooms(Order.Artillery);old.Rooms!.KeyTaken=old.Rooms.DoorOpen=true;old.Rooms.Rooms[PortRooms.Lodge].Visited=true;
        old.Rooms.Rooms[PortRooms.Lodge].Enemies.Add(new(){Id=old.NextId++,Kind=EnemyKind.Guard,Position=new(650,735),Health=17,MaxHealth=85});old.Health=47;
        old.EnableConnectedWorld();check(old.Enemies.Count==2&&old.Health==47&&old.Enemies[1].Health==17,"Migration preserves actors and wounds");old.ValidateRooms();
        var chase=Combat.NewRooms(Order.Artillery);chase.EnableConnectedWorld();chase.DeveloperSurvival=true;chase.Enemies.Clear();
        chase.Rooms!.KeyTaken=chase.Rooms.DoorOpen=true;var chaseDoor=chase.Rooms.Doors["lodge"];chaseDoor.Locked=false;chaseDoor.Openness=1;chaseDoor.TargetOpen=true;
        var gate=ConnectedWorld.Center(first);chase.Player=ConnectedWorld.Route(first)[^1]-chase.WorldOrigin;chase.Step(default);
        foreach(var e in chase.Enemies)e.Health=0;
        chase.Spawn(EnemyKind.Guard,ConnectedWorld.Route(first)[^2]-chase.WorldOrigin);var pursuer=chase.Enemies[^1];pursuer.HomeRoom=PortRooms.Court;pursuer.Alerted=true;
        for(int i=0;i<900;i++)chase.Step(default);
        check(pursuer.Position.X+chase.WorldOrigin.X>ConnectedWorld.Origin(PortRooms.Lodge).X+150,"Danish guard follows across room boundary");
        // An occupied leaf sweep cannot close through an actor.
        chase.Player=gate-chase.WorldOrigin+new Vector2(0,30);chaseDoor.Openness=1;chaseDoor.TargetOpen=false;
        var mid=(ConnectedWorld.Hinge(first)+ConnectedWorld.Tip(first,1))/2;
        pursuer.Position=mid-chase.WorldOrigin;pursuer.State=2;pursuer.Timer=10;
        chase.Step(default);check(chaseDoor.Openness==1,"Connected door stops its closing sweep at a guard");
        // Each story gate seals the full corridor from both approaches.
        var locked=Combat.NewRooms(Order.Artillery);locked.EnableConnectedWorld();
        foreach(var l in RoomLinks.All.Where(l=>l.Gate!=PassageGate.Free))
        {var route=ConnectedWorld.Route(l);var axis=Vector2.Normalize(ConnectedWorld.Painted(l)?route[1]-route[0]:route[2]-route[1]);var c=ConnectedWorld.Center(l);
            check(!locked.ClearPath(c-axis*55,c+axis*55)&&!locked.ClearPath(c+axis*55,c-axis*55),"Story gate blocks both directions: "+l.Id);}
        foreach(var link in RoomLinks.All.Where(l=>l.Id is "roots" or "grove"))
        {
            var route=ConnectedWorld.Route(link);var axis=Vector2.Normalize(route[1]-route[0]);
            var c=ConnectedWorld.Center(link);var side=ConnectedWorld.Side(route[0],route[1])*ConnectedWorld.MouthWidth(link);
            for(int lane=-8;lane<=8;lane++)
            {
                var offset=side*(lane/4f);
                check(!locked.ClearPath(c+offset-axis*45,c+offset+axis*45),"Painted gate blocks offset approach: "+link.Id+" "+lane);
                check(!locked.ClearPath(c+offset+axis*45,c+offset-axis*45),"Painted gate blocks reverse offset approach: "+link.Id+" "+lane);
            }
        }
        foreach(var link in RoomLinks.All.Where(l=>l.Id is "roots" or "grove"))foreach(int direction in new[]{-1,1})
        {
            var route=ConnectedWorld.Route(link);var axis=Vector2.Normalize(route[1]-route[0])*direction;
            var c=ConnectedWorld.Center(link);g.Enemies.Clear();g.Shots.Clear();g.Hazards.Clear();
            g.Player=c+axis*155-g.WorldOrigin;g.Health=100;g.DeveloperSurvival=true;
            var leaf=g.Rooms.Doors[link.Id];leaf.Locked=false;leaf.TargetOpen=false;leaf.Openness=0;
            g.Spawn(EnemyKind.Guard,c-axis*85-g.WorldOrigin);var guard=g.Enemies[^1];guard.Alerted=true;
            for(int tick=0;tick<110;tick++)g.Step(default);
            check(Vector2.Dot(guard.Position+g.WorldOrigin-c,axis)<0,"Guard cannot cross closed painted door from side "+direction+": "+link.Id);
            // Give the leaf its physical clearance; an actor pressed against it
            // correctly stops the sweep, just as in the original door study.
            guard.Position=c-axis*110-g.WorldOrigin;guard.State=2;guard.Timer=1.1f;
            leaf.TargetOpen=true;
            for(int tick=0;tick<420;tick++)g.Step(default);
            Console.WriteLine($"ARCH CHASE {link.Id}/{direction} door={leaf.Openness} guard={guard.Position+g.WorldOrigin-c} player={g.Player+g.WorldOrigin-c} state={guard.State} walk={g.OnWalkable(guard.Position)} clear={g.ClearPath(guard.Position,g.Player)}");
            check(Vector2.Dot(guard.Position+g.WorldOrigin-c,axis)>15,"Guard follows through open painted door from side "+direction+": "+link.Id);
        }
        locked.Rooms!.Doors["lodge"].Health=0;locked.Rooms.DoorOpen=true;locked.ValidateRooms();
        check(!locked.Rooms.KeyTaken,"Breaking a door does not invent a key");
    }
}
