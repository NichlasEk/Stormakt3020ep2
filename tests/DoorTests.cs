using Atland;
using System.Numerics;
using System.Text.Json;
static class DoorTests
{
 public static void Run(Action<bool,string> check)
 {
  Controls C(Vector2 move=default,bool use=false,bool hit=false)=>new(move,new(0,-1),hit,false,false,false,false,false,false,use);
  void Frames(Combat g,int n){for(int i=0;i<n;i++)g.Step(C());}
  foreach(int side in new[]{-1,1})
  {
   var blocked=Combat.NewDoorTrial(Order.Artillery);blocked.Enemies.Clear();blocked.DeveloperSurvival=true;
   var normal=Vector2.Normalize(new Vector2(-(DoorTrialLayout.ClosedTip.Y-DoorTrialLayout.Hinge.Y),DoorTrialLayout.ClosedTip.X-DoorTrialLayout.Hinge.X));
   blocked.Player=DoorTrialLayout.Center+normal*side*100;
   blocked.Spawn(EnemyKind.Guard,DoorTrialLayout.Center-normal*side*100);blocked.Enemies[0].Alerted=true;
   for(int i=0;i<1200;i++)
   {
    blocked.Step(C());
    check(DoorTrialLayout.Side(blocked.Enemies[0].Position)*side<0,$"Closed door stops pursuing guard from side {side}, tick {i}, at {blocked.Enemies[0].Position}");
   }
  }
  foreach(var jamb in new[]{DoorTrialLayout.Hinge,DoorTrialLayout.ClosedTip})
  foreach(int side in new[]{-1,1})foreach(bool dodge in new[]{false,true})
  {
   var seam=Combat.NewDoorTrial(Order.Artillery);seam.Enemies.Clear();var n=Vector2.Normalize(new Vector2(-(DoorTrialLayout.ClosedTip.Y-DoorTrialLayout.Hinge.Y),DoorTrialLayout.ClosedTip.X-DoorTrialLayout.Hinge.X));seam.Player=jamb+n*side*55;
   for(int i=0;i<100;i++)
   {
    seam.Step(new(-n*side,default,false,false,dodge&&i%30==0,false,false,false,false,false));
    check(DoorTrialLayout.Side(seam.Player)*side>0,$"Karl stays on his side of jamb {jamb}, side {side}, dodge {dodge}, tick {i}");
   }
  }
  foreach(int side in new[]{-1,1})foreach(bool broken in new[]{false,true})
  {
   var passage=Combat.NewDoorTrial(Order.Artillery);passage.Enemies.Clear();passage.DeveloperSurvival=true;passage.DoorTest!.KeyTaken=true;
   var leaf=passage.DoorTest.Door;leaf.Locked=false;leaf.TargetOpen=true;leaf.Openness=1;if(broken)leaf.Health=0;
   var yard=new Vector2(749,700);var lodge=new Vector2(1030,480);
   passage.Player=side>0?yard:lodge;passage.Spawn(EnemyKind.Guard,side>0?lodge:yard);passage.Enemies[0].Alerted=true;
   Frames(passage,500);check(DoorTrialLayout.Side(passage.Enemies[0].Position)*side>0,$"Guard can still pursue through {(broken?"broken":"open")} door from side {side}");
  }
  foreach(int side in new[]{-1,1})
  {
   var crowd=Combat.NewDoorTrial(Order.Artillery);crowd.Enemies.Clear();crowd.DeveloperSurvival=true;var n=Vector2.Normalize(new Vector2(-(DoorTrialLayout.ClosedTip.Y-DoorTrialLayout.Hinge.Y),DoorTrialLayout.ClosedTip.X-DoorTrialLayout.Hinge.X));
   crowd.Player=DoorTrialLayout.Center-n*side*90;
   for(int j=0;j<3;j++){crowd.Spawn(EnemyKind.Guard,DoorTrialLayout.Center+n*side*(16+j*5));crowd.Enemies[^1].Alerted=true;}
   for(int i=0;i<180;i++){crowd.Step(C());check(crowd.Enemies.All(e=>DoorTrialLayout.Side(e.Position)*side>0),"Crowd separation cannot push a guard through a closed leaf");}
  }
  var g=Combat.NewDoorTrial(Order.Artillery);g.Enemies.Clear();var d=g.DoorTest!.Door;
  var outside=DoorTrialLayout.Center+new Vector2(-35,65);var inside=DoorTrialLayout.Center+new Vector2(35,-65);
  check(!g.ClearPath(outside,inside),"Closed leaf blocks sight and shots through the real threshold");
  g.Player=outside;g.Step(C());g.Step(C(use:true));check(d.Locked&&!d.TargetOpen,"Locked door rejects use without key");
  g.Player=DoorTrialLayout.Key;g.Step(C());g.Step(C(use:true));check(g.DoorTest.KeyTaken,"Physical key pickup");
  g.Player=outside;g.Step(C());g.Step(C(use:true));Frames(g,55);check(!d.Locked&&d.Openness==1,"Key unlocks and opens real animated leaf");
  check(g.ClearPath(outside,inside),"Open leaf reveals a traversable opening");
  g.Player=outside;var initialTick=g.Tick;for(int i=0;i<50;i++)g.Step(C(Vector2.Normalize(inside-outside)));
  check(DoorTrialLayout.Side(g.Player)<-20&&g.DoorTest.EnteredLodge&&g.Tick>initialTick,"Movement crosses continuously without interact or room teleport");
  // A living guard remains active across the opening and can pursue the player.
  g.Player=outside+new Vector2(-60,110);g.Spawn(EnemyKind.Guard,inside);var foe=g.Enemies.Single();foe.Alerted=true;g.DeveloperSurvival=true;
  Frames(g,180);check(DoorTrialLayout.Side(foe.Position)>20,"Guard pursues across same physical threshold during combat");
  g.Enemies.Clear();g.Player=outside;g.Step(C());g.Step(C(use:true));Frames(g,55);check(d.Openness==0&&!g.ClearPath(outside,inside),"Door closes again and restores collision and sight blocking");
  g.Player=outside;g.Step(C());g.Step(C(use:true));Frames(g,55);g.Player=DoorTrialLayout.Center;g.Step(C());g.Step(C(use:true));Frames(g,55);
  check(d.Openness>0&&g.OnWalkable(g.Player),"Closing motion stalls on an occupied sweep instead of crushing or ejecting Karl");
  // Neither ordinary movement nor a projectile may tunnel through a closed leaf.
  var closed=Combat.NewDoorTrial(Order.Artillery);closed.Enemies.Clear();closed.Player=outside;
  for(int i=0;i<35;i++)closed.Step(C(Vector2.Normalize(inside-outside)));
  check(DoorTrialLayout.Side(closed.Player)>0,"Walking into a closed door stays outside");
  closed.Player=outside;closed.Shots.Add(new(){Position=outside,Velocity=Vector2.Normalize(inside-outside)*350,Reflected=true});Frames(closed,35);
  check(closed.Shots.Count==0,"Actual projectile collides with closed door");
  closed.Player=outside;closed.Step(new(Vector2.Normalize(inside-outside),default,false,false,true,false,false,false,false,false));Frames(closed,20);
  check(DoorTrialLayout.Side(closed.Player)>0,"Dodge cannot tunnel through closed leaf");
  var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".json");SaveStore.Write(path,g);var restored=SaveStore.Read(path);File.Delete(path);
  check(restored.InDoorTrial&&restored.DoorTest!.KeyTaken&&restored.DoorTest.Door.Openness==d.Openness,"Key, damage and animation position roundtrip in dedicated save");
  g=Combat.NewDoorTrial(Order.Artillery);g.Enemies.Clear();g.Player=outside;g.Weapon=Weapon.Hammer;
  for(int i=0;i<240;i++)g.Step(new(default,Vector2.Normalize(DoorTrialLayout.Center-g.Player),i%40==0,false,false,false,false,false,false,false));
  check(g.DoorTest!.Door.Broken&&!g.DoorTest.KeyTaken&&g.ClearPath(outside,inside),"Hammer breaks locked door without fabricating a key");
  g.Step(C());g.Step(C(use:true));check(g.DoorTest.Door.Broken,"Broken door cannot be closed again");
  path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".json");SaveStore.Write(path,g);check(SaveStore.Read(path).DoorTest!.Door.Broken,"Destroyed door persists across reload");File.Delete(path);
  var trip=Combat.NewAtland(Order.Medicine);trip.CampaignStage=-1;trip.Region=Region.Shore;trip.Phase=Phase.Complete;trip.PreferRoomRoute=true;trip.Health=39;trip.Potions=1;
  var inv=JsonSerializer.Serialize(trip.Inventory,new JsonSerializerOptions{IncludeFields=true});
  check(trip.ContinueJourney()&&trip.InRooms&&trip.Rooms!.Rooms.Count==9,"Standard shore continuation enters nine-room route");
  check(trip.Health==39&&trip.Potions==1&&inv==JsonSerializer.Serialize(trip.Inventory,new JsonSerializerOptions{IncludeFields=true}),"Standard route preserves equipment, stash, health and consumables");
  path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".json");SaveStore.Write(path,trip);check(SaveStore.Read(path).InRooms,"Standard route continuation remains a valid expedition save");File.Delete(path);
 }
}
