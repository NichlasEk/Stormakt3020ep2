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
 var target=run.Enemies.Where(e=>!e.Dead).OrderBy(e=>Vector2.DistanceSquared(e.Position,run.Player)).FirstOrDefault();
 Vector2 goal=target?.Position??run.Seals.FirstOrDefault(s=>s.Health>0)?.Position??Combat.ChartPosition;
 var delta=goal-run.Player;float distance=delta.Length();
 bool guard=target!=null && target.State==1 && target.Timer<.16f && distance<150;
 var move=distance>60?Combat.Normal(delta,Vector2.UnitX):Vector2.Zero;
 run.Step(Input(move,delta,attack:distance<95&&!guard,heavy:distance<105&&i%47==0&&!guard,guard:guard,heal:run.Health<48,support:target!=null,interact:true));
}
Console.WriteLine($"Campaign bot: phase={run.Phase}, health={run.Health:0}, kills={run.Kills}, parries={run.Parries}, seconds={run.Elapsed:0}");
Check(run.Phase==Phase.Complete,"Full encounter is winnable through ordinary controls");
Console.WriteLine($"PASS · {checks} assertions · combat, orders, boundaries, save recovery, full encounter");
