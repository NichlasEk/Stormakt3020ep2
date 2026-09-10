using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public enum Weapon { Saber, Hammer }
public enum EnemyKind { Guard, Pikeman, Gunner, Collector, OathGuardian, RootMarshal, RootSoldier, CrownBailiff, MeridianWarden, ZenithGuardian, ZenithLock, MusterOfficer, SaltWarden }
// Keep the first four values stable for existing saves.
public enum Phase { Quay, Collector, Discovery, Complete, Names, Testimony, Extraction, Warehouse, Shore, Reveal, Duel, Campaign }
public enum TestimonyChoice { None, Broadcast, Cipher }
public enum Order { Artillery, Medicine }
public readonly record struct Controls(Vector2 Move, Vector2 Aim, bool Attack, bool Heavy, bool Dodge, bool Guard, bool Swap, bool Heal, bool Support, bool Interact);
public readonly record struct Cue(string Kind, Vector2 Position, string Text = "", float Value = 0);

public sealed class Fighter
{
    public string HomeRoom="";
    public int Id;
    public EnemyKind Kind;
    public Vector2 Position;
    public Vector2 Facing = Vector2.UnitY;
    public Vector2 LockedAim;
    public float Health;
    public float MaxHealth;
    public float Timer;
    public float Cooldown;
    public float Hurt;
    public float Walk;
    [JsonIgnore] public bool Moving;
    [JsonIgnore] public Vector2 MoveDirection;
    public int State; // 0 approach, 1 telegraph, 2 recovery, 3 stagger
    public int Pattern;
    public bool Alerted,ChargeHit,Retired;
    public bool Dead => Health <= 0;
}
public sealed class Shot
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Life = 4;
    public bool Reflected;
}
public sealed class Hazard
{
    public Vector2 Position;
    public float Timer;
    public float Radius;
    public bool Friendly,Roots,Steam,Forge,Meridian,Zenith;
}
public sealed class Seal
{
    public Vector2 Position;
    public float Health = 80;
}
public sealed class Inscription
{
    public Vector2 Position;
    public float Progress;
    public bool Disturbed;
    public bool Read;
}

/// <summary>Fixed-tick encounter rules. No renderer, scene tree or external service dependencies.</summary>
public sealed partial class Combat
{
    public int Schema = 1;
    public long Tick;
    public Vector2 Player = new(470, 855);
    public Vector2 Facing = new(0.7f, -0.7f);
    public float Health = 100;
    [JsonIgnore] public bool DeveloperSurvival;
    public float Stamina = 100;
    public int Potions = 2;
    public Weapon Weapon;
    public Order Order;
    public Phase Phase;
    public List<Fighter> Enemies = new();
    public List<Shot> Shots = new();
    public List<Hazard> Hazards = new();
    public List<Seal> Seals = new() { new() { Position = new(395, 775) }, new() { Position = new(825, 545) } };
    public float AttackTime;
    public float AttackLength;
    public float ContactTime;
    public bool HeavyAttack;
    public bool AttackContact;
    public float AttackBuffer;
    public int Combo;
    public float ComboWindow;
    public float DodgeTime;
    public Vector2 DodgeDirection;
    public float Invulnerable;
    public float GuardTime;
    public bool Guarding;
    public float Hurt;
    public float HitStop;
    public float SupportCooldown;
    public float Walk;
    public float Elapsed;
    public int Kills;
    public int Parries;
    public bool IntroPlayed;
    public bool BossEnraged;
    public List<Inscription> Inscriptions = new()
    {
        new() { Position = new(395,775) },
        new() { Position = new(825,545) },
        new() { Position = new(1135,490) }
    };
    public TestimonyChoice Testimony;
    [JsonIgnore] public int ReadingIndex = -1;
    public int NextId = 1;
    [JsonIgnore] public List<Cue> Events = new();
    [JsonIgnore] public bool Dead => Health <= 0;
    [JsonIgnore] public bool Moving;
    [JsonIgnore] public static readonly Vector2[] Ground = {
        new(230,940), new(335,805), new(564,681), new(796,565), new(950,466),
        new(1107,410), new(1370,430), new(1360,528), new(1125,657),
        new(985,771), new(765,953), new(470,959)
    };
    public static readonly Vector2 ChartPosition=new(1210,470);
    public static readonly Vector2 LandingPosition=new(470,855);
    public const float ReadingDuration=2.8f;
    public const float ReadingRange=78;
    public const float ReadingSafety=160;

    public bool ReadingBlocked(int index)=>Enemies.Any(e=>!e.Dead && Vector2.Distance(e.Position,Inscriptions[index].Position)<ReadingSafety);

    // Returns a useful destination for the UI and the integration player.
    [JsonIgnore] public Vector2 ObjectivePosition=>Region!=Region.Quay?JourneyObjective:Phase==Phase.Names
        ? Inscriptions.Where(i=>!i.Read).OrderBy(i=>Vector2.DistanceSquared(i.Position,Player)).FirstOrDefault()?.Position??ChartPosition
        : Phase==Phase.Extraction&&!ExtendedJourney?LandingPosition:ChartPosition;

    public bool ChooseTestimony(TestimonyChoice choice)
    {
        if(Dead || Phase!=Phase.Testimony || Testimony!=TestimonyChoice.None || !Inscriptions.All(i=>i.Read)
            || choice is not (TestimonyChoice.Broadcast or TestimonyChoice.Cipher))return false;
        Testimony=choice;Phase=Phase.Extraction;Stamina=100;
        // Both routes have an immediate, inspectable combat consequence.
        Health=Math.Min(100,Health+(choice==TestimonyChoice.Broadcast?30:15));
        if(choice==TestimonyChoice.Cipher) { Potions++;SupportCooldown=0; }
        Spawn(EnemyKind.Guard,new(640,720));Spawn(EnemyKind.Pikeman,new(790,657));
        if(choice==TestimonyChoice.Broadcast)Spawn(EnemyKind.Gunner,new(1008,635));
        Emit("radio",Player,choice==TestimonyChoice.Broadcast?(ExtendedJourney?"broadcast-route":"broadcast"):(ExtendedJourney?"cipher-route":"cipher"));
        Emit("checkpoint",Player);return true;
    }

    public static Combat New(Order order,bool extendedJourney=false)
    {
        var game = new Combat { ExtendedJourney=extendedJourney, Order = order, Potions = order == Order.Medicine ? 4 : 2 };
        game.Spawn(EnemyKind.Guard, new(640, 720));
        game.Spawn(EnemyKind.Pikeman, new(790, 657));
        game.Spawn(EnemyKind.Gunner, new(1008, 635));
        game.Spawn(EnemyKind.Guard, new(900, 724));
        return game;
    }
    public void Spawn(EnemyKind kind, Vector2 pos)
    {
        float hp = kind == EnemyKind.SaltWarden ? 600 : kind == EnemyKind.MusterOfficer ? 520 : kind == EnemyKind.ZenithGuardian ? 680 : kind == EnemyKind.ZenithLock ? 115 : kind == EnemyKind.MeridianWarden ? 720 : kind == EnemyKind.CrownBailiff ? 880 : kind == EnemyKind.RootMarshal ? 820 : kind == EnemyKind.RootSoldier ? 115 : kind == EnemyKind.OathGuardian ? 720 : kind == EnemyKind.Collector ? 620 : kind == EnemyKind.Pikeman ? 105 : kind == EnemyKind.Gunner ? 65 : 85;
        Enemies.Add(new Fighter { HomeRoom=Rooms?.Current??"", Id = NextId++, Kind = kind, Position = pos, Health = hp, MaxHealth = hp, Cooldown = .8f + NextId * .17f });
    }
    public void Emit(string kind, Vector2 at, string text = "", float value = 0) => Events.Add(new(kind, at, text, value));
    public void Step(Controls input, float dt = 1f / 60)
    {
        Events.Clear(); ReadingIndex=-1; Tick++;
        if(StepPaintedPassage(input,dt))return;
        if(InCabin)input=input with {Attack=false,Heavy=false,Support=false};
        if (Dead || Phase is Phase.Complete or Phase.Testimony) return;
        Elapsed += dt;
        if (!IntroPlayed) { IntroPlayed = true; Emit("radio", Player, "arrival"); }
        if (HitStop > 0) { HitStop -= dt; return; }
        Invulnerable = Math.Max(0, Invulnerable - dt); Hurt = Math.Max(0, Hurt - dt);
        RootSnare=Math.Max(0,RootSnare-dt);
        SupportCooldown = Math.Max(0, SupportCooldown - dt);
        ComboWindow = Math.Max(0, ComboWindow - dt);
        RiposteTime=Math.Max(0,RiposteTime-dt);
        if(input.Guard&&AttackContact&&AttackTime>ContactTime+.08f)AttackTime=0;
        if (ComboWindow == 0 && AttackTime == 0) Combo = 0;
        if (input.Swap && AttackTime <= 0 && DodgeTime <= 0) { Weapon = Weapon == Weapon.Saber ? Weapon.Hammer : Weapon.Saber; Emit("swap",Player); }
        if (input.Heal && Potions > 0 && Health < 100) { Potions--; Health = Math.Min(100, Health + 55); Emit("heal",Player,"+55",55); }
        if (input.Support && SupportCooldown <= 0)
        {
            if (Order == Order.Artillery)
            {
                var target = Player + Normal(input.Aim, Facing) * 145;
                Hazards.Add(new Hazard { Position = Bound(target), Timer = 1.15f, Radius = 115, Friendly = true });
                SupportCooldown = 18; Emit("radio",Player,"cannon");
            }
            else if (Health < 100) { Health = Math.Min(100,Health + 30); SupportCooldown = 22; Emit("heal",Player,"FÄLTFÖRBAND",30); }
        }
        bool wasGuarding = Guarding;
        Guarding = input.Guard && AttackTime <= 0 && DodgeTime <= 0 && Stamina > 0;
        GuardTime = Guarding ? wasGuarding ? GuardTime + dt : 0 : 0;
        if (input.Dodge && DodgeTime <= 0 && Stamina >= 25 && (AttackTime <= 0 || AttackContact))
        {
            AttackTime = 0; Guarding = false; DodgeTime = .23f; Invulnerable = .23f;
            Stamina -= 25; DodgeDirection = Normal(input.Move,Facing); Emit("dodge",Player);
        }
        if (input.Aim.LengthSquared() > .01f && AttackTime <= 0 && DodgeTime <= 0) Facing = Vector2.Normalize(input.Aim);
        if (input.Attack && AttackTime > 0) AttackBuffer = .16f;
        else AttackBuffer = Math.Max(0,AttackBuffer-dt);
        if (AttackTime <= 0 && DodgeTime <= 0 && !Guarding && (input.Attack || input.Heavy || AttackBuffer > 0)) StartAttack(input.Heavy);
        Moving = false;
        if (DodgeTime > 0) { DodgeTime = Math.Max(0,DodgeTime-dt); MovePlayer(DodgeDirection*540*dt); }
        else if (AttackTime <= 0)
        {
            var move = input.Move.LengthSquared() > 1 ? Vector2.Normalize(input.Move) : input.Move;
            var before=Player;MovePlayer(move*(Guarding?85:RootSnare>0?115:195)*dt);float travelled=Vector2.Distance(before,Player);Moving=travelled>.001f;
            if(Moving){MoveDirection=Normal(move,Facing);float previousWalk=Walk;Walk+=travelled*7/195; if(Gait.Footfall(Walk)!=Gait.Footfall(previousWalk))Emit("step",Player);}
        }
        Stamina = Math.Min(100,Stamina+dt*((Guarding?5:AttackTime>0?10:29)+EquipmentRecovery));
        if (AttackTime > 0)
        {
            AttackTime += dt;
            if (!AttackContact && AttackTime >= ContactTime) { AttackContact = true; ResolveAttack(); }
            if (AttackTime >= AttackLength) { AttackTime = 0; ComboWindow = .65f; }
        }
        foreach (var enemy in Enemies) StepEnemy(enemy,dt);
        SeparateEnemies();
        foreach (var shot in Shots)
        {
            shot.Life -= dt; var before=shot.Position;shot.Position += shot.Velocity*dt;
            if(!ClearPath(before,shot.Position)){shot.Life=0;continue;}
            if (!shot.Reflected && Vector2.DistanceSquared(shot.Position,Player)<22*22)
            {
                if (Guarding && GuardTime < .22f && Vector2.Dot(Facing,-Normal(shot.Velocity,Facing))>.1f)
                {
                    shot.Reflected=true;shot.Velocity=-shot.Velocity*1.4f;Parries++;RememberedParry();Stamina=Math.Min(100,Stamina+15);Emit("parry",Player,"ÅTER TILL AVSÄNDAREN");
                }
                else { DamagePlayer(15,shot.Position); shot.Life=0; }
            }
            if (shot.Reflected)
                foreach (var enemy in Enemies.Where(e=>!e.Dead))
                    if (Vector2.DistanceSquared(shot.Position,enemy.Position)<30*30) { DamageEnemy(enemy,65,shot.Position,true);shot.Life=0;break; }
        }
        Shots.RemoveAll(s=>s.Life<=0);
        StepFoundryClock(dt);StepMeridianClock(dt);StepObservatoryClock(dt);StepWestClock(dt);StepSaltClock(dt);
        StepMinePressure(dt);
        foreach (var hazard in Hazards)
        {
            if(hazard.Zenith&&hazard.Timer<=0)continue;
            if(hazard.Meridian&&MeridianState.WardenDefeated){hazard.Timer=0;continue;}
            if(hazard.Forge&&FoundryState.BailiffDefeated){hazard.Timer=0;continue;}
            hazard.Timer-=dt;
            if(hazard.Timer<=0)
            {
                Emit(hazard.Steam||hazard.Forge?"room-sound":hazard.Friendly?"cannon":"slam",hazard.Position,hazard.Steam?"mine-steam":hazard.Forge?"oath-impact":"",hazard.Radius);
                if(hazard.Steam)foreach(var e in Enemies.Where(e=>!e.Dead&&Vector2.Distance(e.Position,hazard.Position)<hazard.Radius&&ClearPath(e.Position,hazard.Position)))DamageEnemy(e,38,hazard.Position,true);
                if(hazard.Friendly)
                    foreach(var e in Enemies.Where(e=>!e.Dead && Vector2.Distance(e.Position,hazard.Position)<hazard.Radius)) DamageEnemy(e,110,hazard.Position,true);
                else if(Vector2.Distance(Player,hazard.Position)<hazard.Radius&&ClearPath(hazard.Position,Player)){if(hazard.Roots&&DodgeTime<=0)RootSnare=.9f;DamagePlayer(23,hazard.Position);}
            }
        }
        Hazards.RemoveAll(h=>h.Timer<=0);
        if(Dead)return;
        if(input.Interact)
        {
            var nearby=LocalDrops.Where(d=>Vector2.Distance(Player,d.Position)<=65&&ClearPath(Player,d.Position)).OrderBy(d=>Vector2.DistanceSquared(Player,d.Position)).FirstOrDefault();
            if(nearby!=null&&PickUpItem(nearby.Item.Id)=="")input=input with{Interact=false};
        }
        if (Phase==Phase.Quay && Seals.All(s=>s.Health<=0) && Enemies.All(e=>e.Dead))
        {
            Phase=Phase.Collector;Health=Math.Max(Health,75);Stamina=100;Shots.Clear();
            Spawn(EnemyKind.Collector,new(1200,515));Emit("checkpoint",Player);Emit("radio",Player,"collector");
        }
        if(Phase==Phase.Collector && Enemies.All(e=>e.Dead))
        {
            Phase=Phase.Discovery;Hazards.Clear();Shots.Clear();Emit("checkpoint",Player);Emit("radio",Player,"fallen");
        }
        if(Phase==Phase.Discovery && input.Interact && Vector2.Distance(Player,ChartPosition)<100)
        {
            Phase=Phase.Names;Health=Math.Max(Health,75);Stamina=100;Potions++;
            Emit("radio",Player,"names-intro");Emit("radio",Player,"hedvig-karta");Emit("checkpoint",Player);return;
        }
        if(Phase==Phase.Names)ReadInscription(input,dt);
        if(Phase==Phase.Names && Inscriptions.All(i=>i.Read) && Enemies.All(e=>e.Dead))
        {Phase=Phase.Testimony;Shots.Clear();Hazards.Clear();Emit("checkpoint",Player);}
        if(Phase==Phase.Extraction && Enemies.All(e=>e.Dead) && input.Interact && Vector2.Distance(Player,ExtendedJourney?ChartPosition:LandingPosition)<100)
        { if(ExtendedJourney){EnterWarehouse();return;} Phase=Phase.Complete;Emit("radio",Player,"homebound");Emit("checkpoint",Player); }
        StepJourney(input,dt);
    }
    private void ReadInscription(Controls input,float dt)
    {
        if(!input.Interact || Moving || AttackTime>0 || DodgeTime>0 || Guarding || Hurt>0)return;
        int index=Inscriptions.FindIndex(i=>!i.Read && Vector2.Distance(Player,i.Position)<ReadingRange);
        if(index<0)return;
        var stone=Inscriptions[index];
        if(!stone.Disturbed)
        {
            stone.Disturbed=true;
            // The patrol enters well away from Karl. A readable arrival delay
            // gives the player time to release the inscription and turn.
            Vector2 entry=Player.X>800?new(570,790):new(1130,520);
            Spawn(EnemyKind.Guard,entry);
            Spawn(index==1?EnemyKind.Gunner:EnemyKind.Pikeman,Bound(entry+new Vector2(85,-10)));
            foreach(var e in Enemies.Where(e=>!e.Dead)){e.State=2;e.Timer=1.5f;}
            Emit("radio",Player,"names-warning");Emit("checkpoint",Player);return;
        }
        if(ReadingBlocked(index))return;
        ReadingIndex=index;stone.Progress=Math.Min(ReadingDuration,stone.Progress+dt);
        if(Tick%18==0)Emit("scrape",stone.Position);
        if(stone.Progress<ReadingDuration)return;
        stone.Read=true;ReadingIndex=-1;
        Emit("inscription",stone.Position,"NAMN ÅTERFUNNET");Emit("radio",Player,$"name-{index}");Emit("checkpoint",Player);
        if(Inscriptions.Count(i=>i.Read)==1)Emit("radio",Player,"hedvig-minne");
    }
    private void RememberedParry()
    {
        if(WhetstoneTaken){RiposteTime=2.6f;Emit("inscription",Player,"RIPOST KLAR");}
        if(Testimony==TestimonyChoice.Broadcast){Health=Math.Min(100,Health+4);Emit("heal",Player,"MINNET BÄR  +4",4);}
    }
    private void StartAttack(bool heavy)
    {
        float cost = heavy ? Weapon==Weapon.Hammer?32:23 : 0;
        if(Stamina<cost)return;
        Stamina-=cost;HeavyAttack=heavy;AttackContact=false;AttackTime=.001f;AttackBuffer=0;
        Combo=Combo%3+1;
        ContactTime=Weapon==Weapon.Hammer?(heavy?.38f:.26f):(heavy?.25f:.13f);
        AttackLength=ContactTime+(Weapon==Weapon.Hammer?.28f:.18f);
        Emit("swing",Player,"",Weapon==Weapon.Hammer?1:0);
    }
    private void ResolveAttack()
    {
        float range=Weapon==Weapon.Hammer?100:91;
        if(HeavyAttack)range+=15;
        float damage=AttackDamage*(HeavyAttack?1.8f:1)*(Combo==3?1.25f:1);
        if(RiposteTime>0&&Weapon==Weapon.Saber){damage*=1.6f;RiposteTime=0;}
        float arc=HeavyAttack?-.1f:.05f;
        Emit("slash",Player,"",range);
        HitRegimentStandards(range,damage,arc);HitPhysicalDoor(range,damage,arc);HitConnectedDoor(range,damage,arc);
        foreach(var e in Enemies.Where(e=>!e.Dead))
        {
            var delta=e.Position-Player;
            if(delta.Length()<range && Vector2.Dot(Normal(delta,Facing),Facing)>arc && ClearPath(Player,e.Position))
                DamageEnemy(e,damage,Player,HeavyAttack || Weapon==Weapon.Hammer || Combo==3);
        }
        foreach(var seal in Seals.Where(s=>s.Health>0))
        {
            var delta=seal.Position-Player;
            if(delta.Length()<range && Vector2.Dot(Normal(delta,Facing),Facing)>arc)
            {
                seal.Health=Math.Max(0,seal.Health-damage);Emit("sealhit",seal.Position);
                if(seal.Health<=0)Emit("seal",seal.Position,"SIGILL BRUTET");
            }
        }
    }
    private void StepEnemy(Fighter e,float dt)
    {
        if(PaintedRooms&&InConnectedWorld&&e.HomeRoom!=Rooms!.Current)return;
        e.Moving=false;
        if(e.Dead)return;
        if(InConnectedWorld&&Vector2.DistanceSquared(e.Position,Player)>800*800)return;
        if(InRooms&&!e.Alerted)
        {
            if(e.Health>=e.MaxHealth&&!CanSeeRoomPoint(e.Position))return;
            e.Alerted=true;e.Cooldown=Math.Max(e.Cooldown,.8f);
        }
        if(e.Kind==EnemyKind.SaltWarden){StepSaltWarden(e,dt);return;}
        if(e.Kind==EnemyKind.MusterOfficer){StepMusterOfficer(e,dt);return;}
        if(e.Kind is EnemyKind.ZenithGuardian or EnemyKind.ZenithLock){StepZenith(e,dt);return;}
        if(e.Kind==EnemyKind.MeridianWarden){StepMeridianWarden(e,dt);return;}
        if(e.Kind==EnemyKind.CrownBailiff){StepCrownBailiff(e,dt);return;}
        if(e.Kind==EnemyKind.RootMarshal){StepRootMarshal(e,dt);return;}
        if(e.Kind==EnemyKind.RootSoldier&&e.State==0&&e.Cooldown<=0&&Vector2.Distance(e.Position,Player)<300&&ClearPath(e.Position,Player)){Hazards.Add(new(){Position=Player,Timer=1.3f,Radius=48,Roots=true});e.Cooldown=5;}
        if(e.Kind==EnemyKind.OathGuardian){StepOathGuardian(e,dt);return;}
        e.Hurt=Math.Max(0,e.Hurt-dt);e.Cooldown=Math.Max(0,e.Cooldown-dt);
        var to=Player-e.Position;float distance=to.Length();
        if(e.State==0)
        {
            e.Facing=Normal(to,e.Facing);
            float reach=e.Kind==EnemyKind.Gunner?290:e.Kind==EnemyKind.Pikeman?128:e.Kind==EnemyKind.Collector?142:78;
            float speed=e.Kind==EnemyKind.Collector?76:e.Kind==EnemyKind.Gunner?65:e.Kind==EnemyKind.Pikeman?83:106;
            var before=e.Position;
            if(distance>reach*.82f||!ClearPath(e.Position,Player))
            {var direction=Normal(NextWaypoint(e.Position,Player)-e.Position,e.Facing);e.Position=MoveBody(e.Position,e.Position+direction*speed*dt);}
            else if(e.Kind==EnemyKind.Gunner && distance<170)e.Position=MoveBody(e.Position,e.Position-e.Facing*speed*dt);
            var displacement=e.Position-before;float travelled=displacement.Length();e.Moving=travelled>.001f;
            if(e.Moving){e.MoveDirection=displacement/travelled;e.Walk+=travelled*7/195;}
            if(distance<reach && e.Cooldown<=0 && ClearPath(e.Position,Player))
            {
                e.State=1;e.LockedAim=Player;e.Timer=e.Kind==EnemyKind.Gunner?1.1f:e.Kind==EnemyKind.Collector?1.0f:e.Kind==EnemyKind.Pikeman?.75f:.55f;
                Emit("warning",e.Position);
            }
        }
        else
        {
            e.Timer-=dt;
            if(e.Timer<=0)
            {
                if(e.State==1)
                {
                    if(e.Kind==EnemyKind.Gunner)
                    {
                        Shots.Add(new Shot {Position=e.Position,Velocity=Normal(e.LockedAim-e.Position,e.Facing)*350});Emit("shot",e.Position);
                    }
                    else if(e.Kind==EnemyKind.Collector && e.Pattern%3==2)
                    {
                        Hazards.Add(new Hazard {Position=e.LockedAim,Timer=.8f,Radius=95});
                        if(e.Health<e.MaxHealth*.5f)
                        {Hazards.Add(new Hazard {Position=e.LockedAim+new Vector2(130,0),Timer=1.15f,Radius=78});Hazards.Add(new Hazard {Position=e.LockedAim-new Vector2(130,0),Timer=1.5f,Radius=78});}
                    }
                    else
                    {
                        float range=e.Kind==EnemyKind.Collector?145:e.Kind==EnemyKind.Pikeman?135:88;
                        Emit("enemystrike",e.Position,"",range);
                        if(distance<range && Vector2.Dot(Normal(to,e.Facing),e.Facing)>.25f && ClearPath(e.Position,Player))
                        {
                            bool parry=Guarding && GuardTime<.22f && Vector2.Dot(Facing,-e.Facing)>.0f;
                            if(parry){Parries++;RememberedParry();Stamina=Math.Min(100,Stamina+22);e.State=3;e.Timer=1.2f;e.Cooldown=1.3f;Emit("parry",Player,"PERFEKT PARAD");HitStop=.07f;return;}
                            DamagePlayer(e.Kind==EnemyKind.Collector?27:e.Kind==EnemyKind.Pikeman?19:14,e.Position);
                        }
                    }
                    e.Pattern++;e.State=2;e.Timer=e.Kind==EnemyKind.Collector?.8f:.5f;e.Cooldown=e.Kind==EnemyKind.Gunner?2.2f:1.0f;
                }
                else e.State=0;
            }
        }
        if(e.Kind==EnemyKind.Collector && e.Health<e.MaxHealth*.5f && !BossEnraged)
        {BossEnraged=true;if(InCampaign)Emit("campaign",e.Position,"Väktaren samlar kraft. Läs markeringarna och gå undan för nästa slag.");else Emit("radio",e.Position,"rage");}
    }
    private void DamageEnemy(Fighter e,float damage,Vector2 source,bool stagger)
    {
        if(e.Dead)return;
        if(e.Kind==EnemyKind.MusterOfficer&&WestState.Exposed<=0){Emit("block",e.Position,"LOSSA BÅDA BROMSARNA");return;}
        if(e.Kind is EnemyKind.ZenithGuardian or EnemyKind.ZenithLock)
        {
            if(!ObservatoryState.Awake)return;
            if(e.Kind==EnemyKind.ZenithGuardian&&Enemies.Any(v=>v.Kind==EnemyKind.ZenithLock&&!v.Dead&&v.HomeRoom==Observatory.Dome)){Emit("block",e.Position,"BRYT LÅSNINGARNA");return;}
        }
        if(e.Kind==EnemyKind.Pikeman && e.State==0 && !stagger && Vector2.Dot(e.Facing,Normal(source-e.Position,e.Facing))>.4f)
        {damage*=.3f;Emit("block",e.Position,"BRYT GARDEN");}
        if(e.Kind==EnemyKind.MeridianWarden){if(!MeridianState.PlateSet||MeridianState.Breaks==0)return;damage*=MeridianState.Exposed>0?1.5f:.2f;
            float floor=e.MaxHealth*Math.Clamp(3-MeridianState.Breaks,0,2)/3f;damage=Math.Min(damage,Math.Max(0,e.Health-floor));if(damage<=0)return;
            if(e.Health-damage<=floor&&floor>0){MeridianState.Exposed=0;e.State=0;e.Cooldown=1.5f;Emit("inscription",Player,"NÄSTA INSTRUMENT SVARAR");}}
        if(e.Kind==EnemyKind.CrownBailiff)damage*=e.State is 2 or 3?1.35f:.65f;
        if(e.Kind==EnemyKind.RootMarshal)damage*=RegimentState.Exposed>0?1.45f:.28f;
        if(e.Kind==EnemyKind.OathGuardian)damage*=e.State==3?1.65f:.18f;
        e.Health=Math.Max(0,e.Health-damage);e.Hurt=.16f;
        if(stagger && e.Kind is not (EnemyKind.Collector or EnemyKind.OathGuardian or EnemyKind.RootMarshal or EnemyKind.CrownBailiff or EnemyKind.MeridianWarden or EnemyKind.ZenithGuardian or EnemyKind.ZenithLock or EnemyKind.MusterOfficer or EnemyKind.SaltWarden)){e.State=3;e.Timer=.5f;e.Position=MoveBody(e.Position,e.Position+Normal(e.Position-source,Vector2.UnitX)*12);}
        HitStop=Weapon==Weapon.Hammer?.055f:.035f;Emit("hit",e.Position,((int)damage).ToString(),damage);
        if(SaltDeath(e)||WestDeath(e)||ZenithDeath(e))return;
        if(e.Dead&&e.Kind==EnemyKind.MeridianWarden){e.Retired=true;MeridianState.WardenDefeated=true;foreach(var h in Hazards.Where(h=>h.Meridian))h.Timer=0;Emit("radio",e.Position,"meridian-fallen");Emit("radio",e.Position,"meridian-after");Emit("inscription",e.Position,"MORGONEN GÅR VIDARE");Emit("checkpoint",Player);return;}
        if(e.Dead){Kills++;Stamina=Math.Min(100,Stamina+10);Emit("death",e.Position);if(e.Kind==EnemyKind.CrownBailiff&&Rooms!=null){FoundryState.BailiffDefeated=true;Emit("radio",Player,"foundry-fallen");Emit("checkpoint",Player);}else if(e.Kind==EnemyKind.RootMarshal&&Rooms!=null){RegimentState.MarshalDefeated=true;Emit("inscription",e.Position,"MARSKALKENS ED ÄR BRUTEN");Emit("checkpoint",Player);}else if(e.Kind==EnemyKind.OathGuardian&&Rooms!=null){Rooms.OathDefeated=true;Emit("radio",Player,"rooms-fallen");DropItem("crown-helm",e.Position);Emit("inscription",e.Position,"EDEN ÄR BRUTEN");Emit("checkpoint",Player);}else DropEnemyLoot(e);}
    }
    private void DamagePlayer(float damage,Vector2 source)
    {
        if(Invulnerable>0 || Dead)return;
        if(Guarding && Vector2.Dot(Facing,Normal(source-Player,Facing))>.1f && Stamina>=18)
        {Stamina-=18;damage*=.22f;Emit("block",Player);}
        damage*=1-EquipmentArmor/100;
        Health=Math.Max(DeveloperSurvival?1:0,Health-damage);Invulnerable=.55f;Hurt=.22f;Emit("hurt",Player,"",damage);
        if(Dead)Emit("playerdeath",Player);
    }
    private void MovePlayer(Vector2 delta)
    {
        var target=MoveBody(Player,Player+delta);
        foreach(var e in Enemies.Where(e=>!e.Dead))
        {var d=target-e.Position;float l=d.Length();if(l<29 && l>.01f)target=e.Position+d/l*29;}
        Player=MoveBody(Player,target);
    }
    private void SeparateEnemies()
    {
        for(int i=0;i<Enemies.Count;i++)for(int j=i+1;j<Enemies.Count;j++)
        {var a=Enemies[i];var b=Enemies[j];if(a.Dead||b.Dead)continue;var d=b.Position-a.Position;float l=d.Length();if(l<37){var n=Normal(d,Vector2.UnitX);float push=(37-l)*.5f;a.Position=MoveBody(a.Position,a.Position-n*push);b.Position=MoveBody(b.Position,b.Position+n*push);}}
    }
    public static Vector2 Normal(Vector2 v,Vector2 fallback)=>v.LengthSquared()>.0001f?Vector2.Normalize(v):fallback;
    public static bool OnGround(Vector2 p)
    {
        bool inside=false;
        for(int i=0,j=Ground.Length-1;i<Ground.Length;j=i++)
        {var a=Ground[i];var b=Ground[j];if((a.Y>p.Y)!=(b.Y>p.Y) && p.X<(b.X-a.X)*(p.Y-a.Y)/(b.Y-a.Y)+a.X)inside=!inside;}
        return inside;
    }
    public static Vector2 ClampToGround(Vector2 p)
    {
        if(OnGround(p))return p;
        var best=Ground[0];float distance=float.MaxValue;
        for(int i=0;i<Ground.Length;i++)
        {var a=Ground[i];var b=Ground[(i+1)%Ground.Length];var d=b-a;var q=a+d*Math.Clamp(Vector2.Dot(p-a,d)/d.LengthSquared(),0,1);float ds=Vector2.DistanceSquared(q,p);if(ds<distance){best=q;distance=ds;}}
        return Vector2.Lerp(best,new Vector2(730,740),.003f);
    }
}
