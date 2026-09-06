using Atland;
using System.Numerics;
using System.Text.Json;

int checks=0;
void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
Controls Input(Vector2 move=default,Vector2 aim=default,bool attack=false,bool heavy=false,bool dodge=false,bool guard=false,bool swap=false,bool heal=false,bool support=false,bool interact=false)=>new(move,aim,attack,heavy,dodge,guard,swap,heal,support,interact);
var arena=Combat.New(Order.Artillery);
arena.Enemies.Clear();arena.Player=new(740,740);arena.Spawn(EnemyKind.Guard,new(795,740));
arena.Enemies[0].Cooldown=10;
arena.Step(Input(aim:Vector2.UnitX,attack:true));
for(int i=0;i<30;i++)arena.Step(Input(aim:Vector2.UnitX));
Check(arena.Enemies[0].Health==60,"One saber swing must hit exactly once");
arena.Step(Input(aim:Vector2.UnitX,swap:true,heavy:true));
Check(arena.Weapon==Weapon.Hammer && arena.Stamina<70,"Heavy hammer costs stamina and swaps only when idle");

var parry=Combat.New(Order.Artillery);parry.Enemies.Clear();parry.Player=new(740,740);
parry.Shots.Add(new(){Position=new(760,740),Velocity=new(-200,0)});
parry.Step(Input(aim:Vector2.UnitX,guard:true));
Check(parry.Parries==1 && parry.Shots.Single().Reflected && parry.Health==100,"Timed guard reflects a real incoming projectile");

var dodge=Combat.New(Order.Artillery);dodge.Enemies.Clear();dodge.Player=new(740,740);
dodge.Step(Input(move:Vector2.UnitX,dodge:true));
Check(dodge.Invulnerable>0 && dodge.Stamina<80 && dodge.Player.X>740,"Dodge spends stamina and moves with invulnerability");
for(int i=0;i<15;i++)dodge.Step(Input());
Check(dodge.Invulnerable==0,"Dodge invulnerability expires");

var medicine=Combat.New(Order.Medicine);Check(medicine.Potions==4,"Medical order has two additional potions");
medicine.Health=30;medicine.Step(Input(support:true));Check(medicine.Health==60 && medicine.SupportCooldown>0,"Medical support heals once and enters cooldown");
medicine.Step(Input(support:true));Check(medicine.Health==60,"Support cannot repeat during cooldown");

var artillery=Combat.New(Order.Artillery);artillery.Step(Input(aim:Vector2.UnitX,support:true));
Check(artillery.Hazards.Count==1 && artillery.Hazards[0].Friendly,"Artillery order creates a warned friendly strike");

var random=new Random(3020);var roam=Combat.New(Order.Artillery);roam.Enemies.Clear();
for(int i=0;i<8000;i++)
{
 var move=new Vector2((float)random.NextDouble()*2-1,(float)random.NextDouble()*2-1);
 roam.Step(Input(move,move,attack:i%43==0,dodge:i%101==0,swap:i%227==0));
 Check(float.IsFinite(roam.Player.X)&&float.IsFinite(roam.Player.Y)&&Combat.OnGround(roam.Player),"Player stays on visible quay");
 Check(roam.Stamina>=0&&roam.Stamina<=100,"Stamina stays bounded");
}

string folder=Path.Combine(Path.GetTempPath(),"atland-tests-"+Guid.NewGuid());Directory.CreateDirectory(folder);
try
{
 string path=Path.Combine(folder,"save.json");SaveStore.Write(path,artillery);
 var restored=SaveStore.Read(path);var options=new JsonSerializerOptions{IncludeFields=true};
 for(int i=0;i<120;i++)
 {
  var input=Input(move:new(.4f,-.2f),aim:Vector2.UnitX,attack:i%23==0,guard:i%60>50);
  artillery.Step(input);restored.Step(input);
 }
 Check(JsonSerializer.Serialize(artillery,options)==JsonSerializer.Serialize(restored,options),"Mid-encounter save continues deterministically with active hazard");
 SaveStore.Write(path,restored);File.WriteAllText(path,"broken");var recovered=SaveStore.Read(path);
 Check(recovered.Tick==1,"Corrupt save recovers prior backup");
}
finally{Directory.Delete(folder,true);}

var run=Combat.New(Order.Artillery);
for(int i=0;i<18000&&!run.Dead&&run.Phase!=Phase.Complete;i++)
{
 if(run.Phase==Phase.Testimony){Check(run.ChooseTestimony(TestimonyChoice.Broadcast),"Public testimony is available after all names");}
 var target=run.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,run.Player)).FirstOrDefault();
 Vector2 goal=target?.Position??run.Seals.FirstOrDefault(s=>s.Health>0)?.Position??run.ObjectivePosition;
 var delta=goal-run.Player;float distance=delta.Length();
 bool guard=target!=null && target.State==1 && target.Timer<.16f && distance<150;
 var move=distance>60?Combat.Normal(delta,Vector2.UnitX):Vector2.Zero;
 bool fighting=target!=null||run.Seals.Any(s=>s.Health>0);
 run.Step(Input(move,delta,attack:fighting&&distance<95&&!guard,heavy:fighting&&distance<105&&i%47==0&&!guard,guard:guard,heal:run.Health<48,support:target!=null,interact:true));
}
Console.WriteLine($"Campaign bot: phase={run.Phase}, health={run.Health:0}, kills={run.Kills}, parries={run.Parries}, seconds={run.Elapsed:0}");
Check(run.Phase==Phase.Complete,"Full encounter is winnable through ordinary controls");
Check(run.Inscriptions.All(i=>i.Read)&&run.Testimony==TestimonyChoice.Broadcast&&run.Kills==14,"Full route preserves all three names and defeats the patrols");
var cipherRun=Combat.New(Order.Medicine);
for(int i=0;i<18000&&!cipherRun.Dead&&cipherRun.Phase!=Phase.Complete;i++)
{
 if(cipherRun.Phase==Phase.Testimony)cipherRun.ChooseTestimony(TestimonyChoice.Cipher);
 var target=cipherRun.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,cipherRun.Player)).FirstOrDefault();
 var delta=(target?.Position??cipherRun.Seals.FirstOrDefault(s=>s.Health>0)?.Position??cipherRun.ObjectivePosition)-cipherRun.Player;
 float distance=delta.Length();bool guard=target!=null&&target.State==1&&target.Timer<.16f&&distance<150;
 bool fighting=target!=null||cipherRun.Seals.Any(s=>s.Health>0);
 cipherRun.Step(Input(distance>60?Combat.Normal(delta,Vector2.UnitX):Vector2.Zero,delta,
  attack:fighting&&distance<95&&!guard,heavy:fighting&&distance<105&&i%47==0&&!guard,guard:guard,
  heal:cipherRun.Health<48,support:target!=null,interact:true));
}
Check(cipherRun.Phase==Phase.Complete&&cipherRun.Testimony==TestimonyChoice.Cipher&&cipherRun.Kills==13,"Medical/cipher route is winnable through ordinary controls");
Console.WriteLine($"Cipher bot: health={cipherRun.Health:0}, kills={cipherRun.Kills}, seconds={cipherRun.Elapsed:0}");

var reading=Combat.New(Order.Medicine);reading.Enemies.Clear();reading.Seals.Clear();reading.Phase=Phase.Names;
reading.Player=reading.Inscriptions[0].Position;
Check(!reading.ChooseTestimony(TestimonyChoice.Cipher),"Cannot choose before finding the testimony");
reading.Step(Input(interact:true));int spawned=reading.Enemies.Count;
Check(spawned==2&&reading.Inscriptions[0].Disturbed,"Disturbing the first stone attracts one patrol");
reading.Enemies[0].Position=reading.Player+new Vector2(80,0);
for(int i=0;i<50;i++)reading.Step(Input(interact:true));
Check(reading.Inscriptions[0].Progress==0&&reading.Enemies.Count==spawned,"Nearby threats prevent reading without spawning duplicate patrols");
reading.Enemies.Clear();reading.Hurt=0;reading.HitStop=0;
for(int i=0;i<60;i++)reading.Step(Input(interact:true));
float partial=reading.Inscriptions[0].Progress;
Check(partial>0&&!reading.Inscriptions[0].Read,"Inscription requires sustained work");
reading.Step(Input());Check(reading.Inscriptions[0].Progress==partial,"Releasing E preserves work");
reading.Step(Input(move:Vector2.UnitX,interact:true));Check(reading.Inscriptions[0].Progress==partial,"Cannot read while moving");
string notesFolder=Path.Combine(Path.GetTempPath(),"atland-names-"+Guid.NewGuid());
try
{
 string path=Path.Combine(notesFolder,"save.json");SaveStore.Write(path,reading);var copy=SaveStore.Read(path);
 for(int i=0;i<180;i++){reading.Step(Input(interact:true));copy.Step(Input(interact:true));}
 var options=new JsonSerializerOptions{IncludeFields=true};
 Check(JsonSerializer.Serialize(reading,options)==JsonSerializer.Serialize(copy,options),"Partial inscription resumes deterministically without repeating patrol");
 Check(copy.Inscriptions[0].Read,"Resumed reading reaches completion");
 // A player can finish reading while a distant patrol lives. Killing it later
 // must still unlock the decision instead of leaving the mission stuck.
 foreach(var stone in copy.Inscriptions){stone.Disturbed=true;stone.Read=true;stone.Progress=Combat.ReadingDuration;}
 copy.Spawn(EnemyKind.Guard,new(1200,500));copy.Step(Input());
 Check(copy.Phase==Phase.Names,"Decision waits for the remaining threat");
 copy.Enemies[0].Health=0;copy.Step(Input());Check(copy.Phase==Phase.Testimony,"Last distant threat unlocks completed testimony");
 SaveStore.Write(path,copy);copy=SaveStore.Read(path);
 int potions=copy.Potions;copy.SupportCooldown=20;
 Check(copy.ChooseTestimony(TestimonyChoice.Cipher)&&copy.Enemies.Count(e=>!e.Dead)==2&&copy.Potions==potions+1&&copy.SupportCooldown==0,"Cipher route has distinct immediate supplies and fewer pursuers");
 Check(!copy.ChooseTestimony(TestimonyChoice.Broadcast),"Decision cannot be repeated or changed for duplicate rewards");
 SaveStore.Write(path,copy);copy=SaveStore.Read(path);
 Check(copy.Testimony==TestimonyChoice.Cipher&&copy.Phase==Phase.Extraction,"Decision and live retreat survive reload");
 copy.Player=Combat.LandingPosition;copy.Step(Input(interact:true));Check(copy.Phase==Phase.Extraction,"Cannot leave while pursuers live");
 foreach(var e in copy.Enemies)e.Health=0;
 copy.Step(Input(interact:true));Check(copy.Phase==Phase.Complete,"Cipher route can board after the retreat");
 // Remove new fields from an actual valid envelope to exercise a legacy save.
 var legacy=new Combat{Phase=Phase.Complete};SaveStore.Write(path,legacy);
 var envelope=System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(path))!;
 var payload=System.Text.Json.Nodes.JsonNode.Parse(envelope["Payload"]!.GetValue<string>())!;
 payload.AsObject().Remove("Inscriptions");payload.AsObject().Remove("Testimony");
 string legacyText=payload.ToJsonString();envelope["Payload"]=legacyText;
 envelope["Checksum"]=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(legacyText)));
 File.WriteAllText(path,envelope.ToJsonString());
 Check(SaveStore.Read(path).Phase==Phase.Complete,"Old completed saves remain completed");
}
finally{Directory.Delete(notesFolder,true);}
Console.WriteLine($"PASS · {checks} assertions · combat, orders, boundaries, save recovery, full encounter");

// End-to-end extended journey: real combat, cover navigation and all interactions.
foreach(var order in new[]{Order.Artillery,Order.Medicine})foreach(var choice in new[]{TestimonyChoice.Broadcast,TestimonyChoice.Cipher})
{
 var journey=Combat.New(order,true);var visited=new HashSet<Region>();
 for(int i=0;i<36000&&!journey.Dead&&journey.Phase!=Phase.Complete;i++)
 {
  visited.Add(journey.Region);
  if(journey.Phase==Phase.Testimony)journey.ChooseTestimony(choice);
  var target=journey.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,journey.Player)).FirstOrDefault();
  Vector2 goal=target?.Position??journey.Seals.FirstOrDefault(s=>s.Health>0)?.Position??journey.ObjectivePosition;
  var delta=goal-journey.Player;float distance=delta.Length();var next=journey.NextWaypoint(journey.Player,goal)-journey.Player;
  bool guard=target!=null&&target.State==1&&target.Timer<.16f&&distance<150;
  var move=distance>60||!journey.ClearPath(journey.Player,goal)?Combat.Normal(next,Vector2.UnitX):Vector2.Zero;
  bool fighting=target!=null||journey.Seals.Any(s=>s.Health>0);
  journey.Step(Input(move,delta,attack:fighting&&distance<95&&!guard,heavy:fighting&&distance<105&&i%47==0&&!guard,guard:guard,heal:journey.Health<48,support:target!=null,interact:true));
  if(journey.Region!=Region.Quay)Check(journey.OnWalkable(journey.Player),$"Journey player stays on floor outside cover {journey.Player}");
 }
 Console.WriteLine($"JOURNEY {order}/{choice}: phase={journey.Phase} region={journey.Region} hp={journey.Health} kills={journey.Kills} elapsed={journey.Elapsed} position={journey.Player} goal={journey.ObjectivePosition}");
 Check(journey.Phase==Phase.Complete&&!journey.Dead&&journey.AtlandRevealed,"Extended journey completes through ordinary controls");
 Check(visited.Count==3&&journey.WhetstoneTaken&&journey.ManifestTaken&&journey.WinchOpened&&journey.Surveyed.All(s=>s),"All regions, useful finds and survey participate in the journey");
 string path=Path.Combine(Path.GetTempPath(),"atland-journey-"+Guid.NewGuid()+".json");
 try{SaveStore.Write(path,journey);var copy=SaveStore.Read(path);Check(copy.AtlandRevealed&&copy.Testimony==choice&&copy.Region==Region.Shore,"Extended ending survives reload");}finally{File.Delete(path);}
}
var cover=Combat.New(Order.Medicine,true);cover.Region=Region.Warehouse;cover.Phase=Phase.Warehouse;cover.Enemies.Clear();cover.Player=new(768,475);
Check(!cover.ClearPath(cover.Player,new(768,700)),"Crates obstruct sight and attacks");
var waypoint=cover.NextWaypoint(cover.Player,new(768,700));
Check(waypoint!=cover.Player&&cover.ClearPath(cover.Player,waypoint),"Navigation returns reachable way around crates");
cover.Shots.Add(new(){Position=new(768,490),Velocity=new(0,350)});
for(int i=0;i<30;i++)cover.Step(Input());
Check(cover.Shots.Count==0,"Projectiles stop at actual cover");
var duel=Combat.NewDuel();duel.Enemies[0].Health=0;duel.Step(Input());Check(duel.Phase==Phase.Complete&&duel.Duel,"Standalone duel ends without campaign gates");

var riposte=Combat.New(Order.Medicine);riposte.Enemies.Clear();riposte.Seals.Clear();riposte.Phase=Phase.Duel;riposte.Player=new(740,740);riposte.WhetstoneTaken=true;
riposte.Shots.Add(new(){Position=new(760,740),Velocity=new(-200,0)});
riposte.Step(Input(aim:Vector2.UnitX,guard:true));
Check(riposte.RiposteTime>2.5f,"Brynsteel charges a riposte after a real projectile parry");
riposte.Shots.Clear();while(riposte.HitStop>0)riposte.Step(Input());
riposte.Spawn(EnemyKind.Guard,new(795,740));var victim=riposte.Enemies[0];victim.Cooldown=10;float beforeHit=victim.Health;
for(int i=0;i<30&&!riposte.AttackContact;i++)riposte.Step(Input(aim:Vector2.UnitX,attack:true));
Check(Math.Abs(beforeHit-victim.Health-25*1.6f)<.01f&&riposte.RiposteTime==0,"Riposte increases the next saber contact once by sixty percent");
var legacyRoute=Combat.New(Order.Medicine);legacyRoute.Phase=Phase.Complete;legacyRoute.Testimony=TestimonyChoice.Cipher;
foreach(var stone in legacyRoute.Inscriptions){stone.Read=true;stone.Progress=Combat.ReadingDuration;}
Check(legacyRoute.ContinueJourney()&&legacyRoute.Region==Region.Warehouse,"Completed 0.2 testimony continues into the new mission without replaying the quay");
var partialSurvey=legacyRoute;partialSurvey.Region=Region.Shore;partialSurvey.Phase=Phase.Shore;partialSurvey.Enemies.Clear();partialSurvey.Player=JourneyLayout.Survey[1];
for(int i=0;i<40;i++)partialSurvey.Step(Input(interact:true));
Check(partialSurvey.SurveyProgress>0&&partialSurvey.SurveyProgress<2&&!partialSurvey.Surveyed[1],"Survey requires a sustained reading");
string partialPath=Path.Combine(Path.GetTempPath(),"atland-survey-"+Guid.NewGuid()+".json");
try
{
 SaveStore.Write(partialPath,partialSurvey);var resumed=SaveStore.Read(partialPath);
 for(int i=0;i<160;i++){partialSurvey.Step(Input(interact:true));resumed.Step(Input(interact:true));}
 var options=new JsonSerializerOptions{IncludeFields=true};
 Check(JsonSerializer.Serialize(partialSurvey,options)==JsonSerializer.Serialize(resumed,options)&&resumed.Surveyed[1],"Partial survey resumes deterministically and finishes exactly once");
 Check(resumed.Surveyed.Count(s=>s)==1,"Holding interact never surveys a distant plate");
}
finally{File.Delete(partialPath);}
Console.WriteLine($"PASS EXTENDED · {checks} assertions");
