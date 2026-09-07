using Atland;
using System.Numerics;
using System.Text.Json;

int checks=0;
void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
Controls Input(Vector2 move=default,Vector2 aim=default,bool attack=false,bool heavy=false,bool dodge=false,bool guard=false,bool swap=false,bool heal=false,bool support=false,bool interact=false)=>new(move,aim,attack,heavy,dodge,guard,swap,heal,support,interact);
if(args.Contains("--doors-only")){DoorTests.Run(Check);Console.WriteLine($"PASS DOORS · {checks}");return;}
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

// Development survival preserves actual feedback and has no effect when disabled.
var protectedKarl=Combat.New(Order.Medicine);protectedKarl.DeveloperSurvival=true;protectedKarl.Health=40;
protectedKarl.Shots.Add(new(){Position=protectedKarl.Player+new Vector2(10,0),Velocity=new(-200,0)});
protectedKarl.Step(Input());
Check(protectedKarl.Health==25&&protectedKarl.Hurt>0&&protectedKarl.Events.Any(e=>e.Kind=="hurt"),"Dev protection still loses health and emits damage feedback");
protectedKarl.Health=3;protectedKarl.Invulnerable=0;
protectedKarl.Shots.Add(new(){Position=protectedKarl.Player+new Vector2(10,0),Velocity=new(-200,0)});protectedKarl.Step(Input());
Check(protectedKarl.Health==1&&!protectedKarl.Dead&&protectedKarl.Events.All(e=>e.Kind!="playerdeath"),"Dev protection prevents a lethal bullet without a death event");
protectedKarl.Invulnerable=0;protectedKarl.Hazards.Add(new(){Position=protectedKarl.Player,Radius=100,Timer=0});protectedKarl.Step(Input());
Check(protectedKarl.Health==1&&protectedKarl.Hurt>0,"Boss-style area damage also preserves one life and feedback");
Check(!JsonSerializer.Serialize(protectedKarl,new JsonSerializerOptions{IncludeFields=true}).Contains("DeveloperSurvival"),"Developer toggle is a setting, never smuggled into campaign saves");
protectedKarl.DeveloperSurvival=false;protectedKarl.Invulnerable=0;
protectedKarl.Shots.Add(new(){Position=protectedKarl.Player+new Vector2(10,0),Velocity=new(-200,0)});protectedKarl.Step(Input());
Check(protectedKarl.Dead&&protectedKarl.Events.Any(e=>e.Kind=="playerdeath"),"Turning off development protection restores lethal damage immediately");
Console.WriteLine($"PASS DEV SURVIVAL · {checks} assertions");

// Weapon tips crossing the regular grid are owned by the contact frame only.
var northeastTip=AnimationAtlasLayout.Cell("karl","attack",1,2,1254,1254);
var northeastRecovery=AnimationAtlasLayout.Cell("karl","attack",1,3,1254,1254);
Check(northeastTip.Left<970&&northeastTip.Right>970&&northeastRecovery.Left>970,"Karl's full NE sword tip is retained in contact and excluded from recovery");
var southwestTip=AnimationAtlasLayout.Cell("karl","attack",3,2,1254,1254);
var southwestWindup=AnimationAtlasLayout.Cell("karl","attack",3,1,1254,1254);
Check(southwestTip.Left<590&&southwestWindup.Right<590,"SW tip is not a stray fragment behind the winding-up Karl");
foreach(var direction in new[]{1f,-1f})
{
 var walking=Combat.New(Order.Medicine);walking.Phase=Phase.Duel;walking.Enemies.Clear();walking.Seals.Clear();walking.Player=new(740,740);
 walking.Spawn(EnemyKind.Gunner,new(740+(direction>0?350:100),740));var shooter=walking.Enemies[0];shooter.Cooldown=10;
 var before=shooter.Position;walking.Step(Input());float travelled=Vector2.Distance(before,shooter.Position);
 Check(shooter.Moving&&travelled>0&&Math.Abs(shooter.Walk-travelled*7/195)<.001f,"Gunner steps track travelled distance during approach and retreat");
 Check(Vector2.Dot(shooter.MoveDirection,shooter.Position-before)>0,"Walking faces displacement even when retreating from Karl");
 shooter.State=1;shooter.Timer=1;float stopped=shooter.Walk;walking.Step(Input());
 Check(!shooter.Moving&&shooter.Walk==stopped,"Aiming stops the gunner walk cycle");
}
Console.WriteLine($"PASS DANISH MOTION AND SWORD FRAMES · {checks} assertions");

// Eight-stage campaign: both strategic branches, ordinary combat, and save/reload at each boundary.
foreach(var order in new[]{Order.Medicine,Order.Artillery})foreach(int archiveChoice in new[]{1,2})
{
 var campaign=Combat.NewAtland(order);var visited=new HashSet<int>();int previous=-1;
 string path=Path.Combine(Path.GetTempPath(),"atland-campaign-"+Guid.NewGuid()+".json");
 try
 {
  for(int tick=0;tick<72000&&!campaign.Dead&&!campaign.CampaignFinished;tick++)
  {
   if(campaign.CampaignStage!=previous)
   {
    previous=campaign.CampaignStage;visited.Add(previous);SaveStore.Write(path,campaign);campaign=SaveStore.Read(path);
    Check(campaign.OnWalkable(campaign.Player),"Campaign entry remains on traversable ground");
    foreach(var point in Expedition.Nodes.Append(Expedition.Entry).Append(Expedition.Exit).Append(Expedition.Forge).Append(Expedition.Preserve))Check(campaign.OnWalkable(point),"Campaign objectives are reachable on the painted floor");
   }
   var target=campaign.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,campaign.Player)).FirstOrDefault();
   var objective=campaign.Stage.Task==ExpeditionTask.Archive&&campaign.CampaignProgress==3&&campaign.ArchiveChoice==0&&archiveChoice==2?Expedition.Forge:campaign.CampaignObjective;
   var goal=target?.Position??objective;var delta=goal-campaign.Player;float distance=delta.Length();
   bool guard=target!=null&&target.State==1&&target.Timer<.16f&&distance<150;
   var move=distance>58?Combat.Normal(campaign.NextWaypoint(campaign.Player,goal)-campaign.Player,Vector2.UnitX):Vector2.Zero;
   campaign.Step(Input(move,delta,attack:target!=null&&distance<95&&!guard,heavy:target!=null&&distance<105&&tick%47==0&&!guard,guard:guard,heal:campaign.Health<48,support:target!=null,interact:true));
  }
  Console.WriteLine($"CAMPAIGN {order}/archive-{archiveChoice}: stage={campaign.CampaignStage} progress={campaign.CampaignProgress} phase={campaign.Phase} hp={campaign.Health} kills={campaign.Kills} elapsed={campaign.Elapsed} pos={campaign.Player} goal={campaign.CampaignObjective}");
  Check(campaign.CampaignFinished&&!campaign.Dead&&visited.Count==8,"All eight stages finish with normal damage and ordinary controls");
  Check(campaign.ArchiveChoice==archiveChoice,"Archive choice survives all following worlds");
  SaveStore.Write(path,campaign);var resumed=SaveStore.Read(path);Check(resumed.CampaignFinished&&resumed.CampaignEnding==campaign.CampaignEnding,"Branch-specific ending survives reload");
 }
 finally{File.Delete(path);File.Delete(path+".bak");}
}
var door=Combat.NewAtland(Order.Medicine);door.Enemies.Clear();door.Player=Expedition.Nodes[0];
for(int i=0;i<70;i++)door.Step(Input(interact:true));
Check(door.CampaignProgress==0,"Wrong port sequence cannot open the gate");
door.Player=Expedition.Nodes[2];for(int i=0;i<120;i++)door.Step(Input(interact:true));
Check(door.CampaignProgress==1&&door.CampaignMask==4,"Correct first stone advances exactly once while held");
door.Player=Expedition.Exit;for(int i=0;i<60;i++)door.Step(Input(interact:true));
Check(door.CampaignStage==0,"Exit remains locked until its mechanism is complete");
var previousEnding=Combat.NewAtland(Order.Medicine);previousEnding.CampaignStage=-1;previousEnding.CampaignProgress=previousEnding.CampaignMask=0;previousEnding.AtlandCampaign=false;previousEnding.Region=Region.Shore;previousEnding.Phase=Phase.Complete;
Check(previousEnding.ContinueJourney()&&previousEnding.CampaignStage==0,"Existing shore ending continues directly into Atland");
Console.WriteLine($"PASS EIGHT-STAGE CAMPAIGN · {checks} assertions");
var segment=Combat.NewAtland(Order.Medicine);segment.Enemies.Clear();segment.Player=Expedition.Nodes[2];
for(int i=0;i<15;i++)segment.Step(Input(interact:true));
Check(segment.CampaignChannel>0&&segment.CampaignProgress==0,"Mechanism interaction has partial progress");
string segmentPath=Path.Combine(Path.GetTempPath(),"atland-mechanism-"+Guid.NewGuid()+".json");
try
{
 SaveStore.Write(segmentPath,segment);var copy=SaveStore.Read(segmentPath);
 for(int i=0;i<60;i++){segment.Step(Input(interact:true));copy.Step(Input(interact:true));}
 Check(segment.CampaignProgress==copy.CampaignProgress&&segment.CampaignMask==copy.CampaignMask&&Math.Abs(segment.CampaignChannel-copy.CampaignChannel)<.0001f,"Partial mechanism resumes without duplicate progress");
}
finally{File.Delete(segmentPath);}
Console.WriteLine($"PASS CAMPAIGN CHECKPOINT · {checks} assertions");

// The restored painting has no invisible divider from the rejected modular version.
var port=Combat.NewAtland(Order.Medicine);port.Enemies.Clear();
Check(port.ClearPath(new(483.2f,631.2f),new(387.2f,583.2f)),"Restored courtyard has no invisible modular wall");
Check(port.OnWalkable(PortLayout.Cache),"Existing optional cache remains on the painted floor");
port.Player=PortLayout.Cache;int beforeCache=port.Potions;port.Health=60;
for(int tick=0;tick<100;tick++)port.Step(Input(interact:true));
Check(port.PortCacheTaken&&port.Potions==beforeCache+2&&port.Health==80,"Restored scene retains the one-time cache reward");
string portSave=Path.Combine(Path.GetTempPath(),"atland-port-"+Guid.NewGuid()+".json");
try
{
 SaveStore.Write(portSave,port);port=SaveStore.Read(portSave);
 for(int tick=0;tick<100;tick++)port.Step(Input(interact:true));
 Check(port.PortCacheTaken&&port.Potions==beforeCache+2,"Reload keeps the optional find without duplicate supplies");
 port.Player=new(1390,620);SaveStore.Write(portSave,port);port=SaveStore.Read(portSave);
 Check(port.OnWalkable(port.Player),"Old modular-edge save relocates inside restored courtyard");
}
finally{File.Delete(portSave);File.Delete(portSave+".bak");}
Console.WriteLine($"PASS PAINTED PORT RESTORATION · {checks} assertions");

// Equipment changes actual combat; transfers are atomic and identities survive reload.
var geared=Combat.NewAtland(Order.Medicine);geared.Enemies.Clear();
Check(geared.EquipmentArmor==0&&geared.AttackDamage==25,"Starter equipment preserves existing combat balance");
Check(geared.EquipItem(4)==""&&geared.EquipmentArmor==4,"Helmet occupies its own slot and adds protection");
Check(geared.EquipItem(5)==""&&geared.EquipmentArmor==10,"Armor and helmet protection combine");
Check(geared.EquipItem(6)==""&&geared.EquipmentRecovery==3,"Sigil affects stamina recovery");
geared.Invulnerable=0;geared.Shots.Add(new(){Position=geared.Player+new Vector2(10,0),Velocity=new(-200,0)});geared.Step(Input());
Check(Math.Abs(geared.Health-86.5f)<.001f,"Ten percent armor reduces a real 15-damage bullet to 13.5");
geared.Hurt=0;geared.Stamina=50;geared.Step(Input());Check(Math.Abs(geared.Stamina-(50+32/60f))<.01f,"Sigil recovery is applied by fixed-tick simulation");
var blade=geared.Inventory.Create("atland-saber");geared.Inventory.Bag.Add(blade);
Check(geared.EquipItem(blade.Id)==""&&geared.AttackDamage==34,"Better saber changes actual attack damage");
geared.Weapon=Weapon.Hammer;Check(geared.AttackDamage==40,"Inactive saber bonus never leaks into hammer damage");
geared.Weapon=Weapon.Saber;
while(geared.Inventory.Bag.Count<Items.BagCapacity)geared.Inventory.Bag.Add(geared.Inventory.Create("helmet"));
var replacement=geared.Inventory.Bag.First(i=>i.Definition=="helmet");int fullCount=geared.Inventory.Bag.Count;
Check(geared.EquipItem(replacement.Id)==""&&geared.Inventory.Bag.Count==fullCount,"Replacing equipment in a full bag swaps atomically");
Check(geared.UnequipItem(GearSlot.Helmet)!=""&&geared.Inventory.Equipped.ContainsKey(GearSlot.Helmet),"Full bag cannot lose an unequipped item");
geared.Spawn(EnemyKind.Guard,geared.Player+new Vector2(140,0));int transfer=geared.Inventory.Bag[0].Id;
Check(geared.TransferItem(transfer,true)!=""&&geared.Inventory.Bag.Any(i=>i.Id==transfer),"Combat locks stash transfers without removing the item");
geared.Enemies.Clear();Check(geared.TransferItem(transfer,true)==""&&geared.Inventory.Stash.Single().Id==transfer,"Secured area allows deposit");
Check(geared.TransferItem(transfer,false)==""&&geared.Inventory.Stash.Count==0,"Withdraw restores the same identity");
geared.DropItem("memory",geared.Player);var loot=geared.Inventory.Drops.Last();
Check(geared.PickUpItem(loot.Item.Id)!=""&&geared.Inventory.Drops.Contains(loot),"Full bag leaves loot on the ground");
geared.TransferItem(geared.Inventory.Bag[0].Id,true);geared.Moving=false;geared.Hurt=0;
Check(geared.PickUpItem(loot.Item.Id)==""&&!geared.Inventory.Drops.Contains(loot),"Loot can be picked up after making room");
Check(geared.PickUpItem(loot.Item.Id)!="","Repeated pickup cannot duplicate loot");
geared.Inventory.Validate();
string gearSave=Path.Combine(Path.GetTempPath(),"atland-inventory-"+Guid.NewGuid()+".json");
try
{
 SaveStore.Write(gearSave,geared);var loaded=SaveStore.Read(gearSave);
 Check(loaded.AttackDamage==34&&loaded.Inventory.Stash.Count==1&&loaded.Inventory.Bag.Count==24,"Equipment, bag, stash and stats survive save/load");
 var oldOptions=new JsonSerializerOptions{IncludeFields=true};var oldNode=System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(geared,oldOptions))!;oldNode.AsObject().Remove("Inventory");
 string legacyPayload=oldNode.ToJsonString();
 File.WriteAllText(gearSave,JsonSerializer.Serialize(new{Version=1,Checksum=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(legacyPayload))),Payload=legacyPayload}));
 var migrated=SaveStore.Read(gearSave);migrated.Inventory.Validate();
 Check(migrated.AttackDamage==25&&migrated.Inventory.Bag.Count==3,"Old saves receive standard equipment without losing journey state");
 var duplicate=loaded.Inventory.Bag[0];loaded.Inventory.Stash.Add(duplicate);bool rejected=false;
 try{loaded.Inventory.Validate();}catch(InvalidDataException){rejected=true;}Check(rejected,"Duplicate item identity is rejected");
}
finally{File.Delete(gearSave);File.Delete(gearSave+".bak");}
Console.WriteLine($"PASS INVENTORY · {checks} assertions");

var equippedHit=Combat.NewAtland(Order.Medicine);equippedHit.Enemies.Clear();equippedHit.Player=new(768,650);
var testBlade=equippedHit.Inventory.Create("quay-saber");equippedHit.Inventory.Bag.Add(testBlade);equippedHit.EquipItem(testBlade.Id);
equippedHit.Spawn(EnemyKind.Guard,new(825,650));var equipmentVictim=equippedHit.Enemies[0];equipmentVictim.State=2;equipmentVictim.Timer=5;float beforeEquipmentHit=equipmentVictim.Health;
for(int tick=0;tick<16;tick++)equippedHit.Step(Input(aim:Vector2.UnitX,attack:tick==0));
Check(Math.Abs(beforeEquipmentHit-equipmentVictim.Health-30)<.001f,"Equipped weapon bonus reaches real enemy damage resolution");
while(equippedHit.Inventory.Stash.Count<Items.StashCapacity)equippedHit.Inventory.Stash.Add(equippedHit.Inventory.Create("helmet"));
equippedHit.Enemies.Clear();equippedHit.AttackTime=0;int heldItem=equippedHit.Inventory.Bag[0].Id;
Check(equippedHit.TransferItem(heldItem,true)!=""&&equippedHit.Inventory.Bag.Any(i=>i.Id==heldItem),"Full stash rejects deposit without losing ownership");
Console.WriteLine($"PASS INVENTORY COMBAT AND CAPACITY · {checks} assertions");
RoomTests.Run(Check);
Console.WriteLine($"PASS PERSISTENT ROOMS · {checks} assertions");
SightTests.Run(Check);
Console.WriteLine($"PASS EXPLORATION AND SIGHT · {checks} assertions");
WaterTests.Run(Check);
Console.WriteLine($"PASS WATER AND SHORTCUT · {checks} assertions");

OathTests.Run(Check);
Console.WriteLine($"PASS OATH AND SIX ROOMS · {checks} assertions");
RoomAudioTests.Run(Check);
Console.WriteLine($"PASS ROOM AUDIO CUES · {checks} assertions");
ArchiveTests.Run(Check);
Console.WriteLine($"PASS ARCHIVE AND RETURN · {checks} assertions");
DoorTests.Run(Check);
Console.WriteLine($"PASS PHYSICAL DOORS AND STANDARD ROUTE · {checks} assertions");
