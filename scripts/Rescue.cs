using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Rescue
{
    public const string Hall="rescue-hall",Prison="rescue-prison",Service="rescue-service",Machine="rescue-machine";
    public static readonly string[] Ids={Hall,Prison,Service,Machine};
    public static bool Known(string id)=>Ids.Contains(id);
    public static string Name(string id)=>id switch{Hall=>"Kungaminnets förhall",Prison=>"Kontrollslussen",Service=>"Kontrasignens gång",_=>"Sigillmaskineriet"};
    public static readonly Vector2 Writ=new(800,485),Reader=new(840,535),Captive=new(780,315),Talk=new(800,535),Lever=new(850,500),Boss=new(850,650),Release=new(800,490);
    public static Vector2[] Ground=>new Vector2[]{new(65,460),new(300,410),new(720,420),new(720,270),new(880,270),new(880,420),new(1120,425),new(1460,460),new(1460,600),new(1200,800),new(790,920),new(300,800),new(65,570)};
    public static Vector2[][] Obstacles(string id)
    {
        Vector2[] Box(float x,float y,float w,float h)=>new Vector2[]{new(x,y),new(x+w,y),new(x+w,y+h),new(x,y+h)};
        return id switch{Hall=>new[]{Box(705,360,160,85)},Prison=>new[]{Box(700,410,135,50)},Service=>new[]{Box(770,370,140,85)},Machine=>new[]{Box(390,550,135,80),Box(1015,550,135,85),Box(700,355,170,90)},_=>Array.Empty<Vector2[]>()};
    }
}
public sealed class HeroLoadout
{
    public InventoryState Inventory=new();
    public float Health=100,Stamina=100,SupportCooldown;
    public int Potions=3,Kills,Parries;
    public Weapon Weapon;
    public Order Order;
    public void Validate()
    {
        if(Inventory==null||Inventory.Drops==null||Inventory.Drops.Count!=0||!float.IsFinite(Health)||Health<=0||Health>100||!float.IsFinite(Stamina)||Stamina<0||Stamina>100||Potions<0||!Enum.IsDefined(Weapon)||!Enum.IsDefined(Order)||!float.IsFinite(SupportCooldown)||SupportCooldown<0)throw new System.IO.InvalidDataException("Ogiltig vilande utrustning");
        Inventory.Validate();
    }
}
public sealed class RescueRun
{
    public bool Briefed,WritRead,Trapped,PlayingEbba,Ready,ServiceOpen,Awake,SecondPhase,Defeated,Released,Reunited,Debriefed,KissSeen;
    public int Conversation;
    public HeroLoadout? Karl,Ebba;
    public float PistolReload;
    [JsonIgnore] public float PistolFlash;
    [JsonIgnore] public Vector2 ShotFrom,ShotTo;
}
public sealed partial class Combat
{
    [JsonIgnore] public RescueRun RescueState=>Rooms!.Rescue;
    [JsonIgnore] public bool IsEbba=>Rooms?.Rescue.PlayingEbba==true;
    [JsonIgnore] public string HeroName=>IsEbba?"EBBA GRIP":"KARL CCLV";
    [JsonIgnore] public bool InRescue=>InConnectedWorld&&Rescue.Known(Rooms!.Current);
    [JsonIgnore] public bool Captured=>Rooms?.Rescue is {Trapped:true,PlayingEbba:false,Reunited:false};
    [JsonIgnore] public string RescueGoal=>RescueState.Reunited?"Tillbaka till fregatten tillsammans":Captured?"Radion är tyst · ta över Ebba":Rooms!.Current==Rescue.Hall?IsEbba?"Följ serviceporten i bakre väggen":RescueState.WritRead?"Fortsätt till kontrollslussen":"Säkra förhallen · läs mottagningsordern":Rooms.Current==Rescue.Prison?IsEbba?"Tala med Karl":"Säkra slussen · lägg originalet på läsbordet":Rooms.Current==Rescue.Service?RescueState.ServiceOpen?"Fortsätt genom maskineriets port":"Säkra gången · lossa den mekaniska spärren":RescueState.Defeated?RescueState.Released?"Gå till Karl genom den högra porten":"Återkalla kvarhållningen vid pulpeten":"Besegra Sigillmästaren · bryt hans sikte";
    [JsonIgnore] public Vector2 RescueObjective=>RescueState.Reunited?new(140,490):Rooms!.Current==Rescue.Hall?IsEbba?new(1000,400):RescueState.WritRead?new(1400,490):Rescue.Writ:Rooms.Current==Rescue.Prison?Rescue.Talk:Rooms.Current==Rescue.Service?RescueState.ServiceOpen?new(1400,490):Rescue.Lever:RescueState.Defeated?RescueState.Released?new(1400,490):Rescue.Release:Rescue.Boss;
    public static Combat NewRescuePreview(Order order)
    {
        var g=NewSaltPreview(order);var s=g.SaltState;s.Reunited=true;g.EnterConnectedRoom(Salt.Loading);foreach(var e in g.EncounterEnemies)e.Health=0;s.ManifestRead=true;
        g.EnterConnectedRoom(Salt.Stairs);foreach(var e in g.EncounterEnemies)e.Health=0;s.Drained=true;
        g.EnterConnectedRoom(Salt.Archive);foreach(var e in g.EncounterEnemies)e.Health=0;s.LedgerRead=true;
        g.EnterConnectedRoom(Salt.Spring);foreach(var e in g.EncounterEnemies)e.Health=0;s.Defeated=s.Released=s.ShortcutOpen=s.Debriefed=true;s.Chains=3;
        g.EnterConnectedRoom(Cabin.Room);g.CabinState.ReturnRoom=Gamla.Landing;g.Player=Cabin.Talk;g.Health=g.Stamina=100;g.Events.Clear();g.ValidateRooms();return g;
    }
    private HeroLoadout CaptureHero()
    {return new(){Inventory=Inventory,Health=Health,Stamina=Stamina,Potions=Potions,Weapon=Weapon,Order=Order,SupportCooldown=SupportCooldown,Kills=Kills,Parries=Parries};}
    private void ExchangeHero(HeroLoadout next)
    {
        var drops=Inventory.Drops;Inventory.Drops=new();int nextId=Inventory.NextId;
        Inventory=next.Inventory;Inventory.Drops=drops;Inventory.NextId=Math.Max(nextId,Math.Max(Inventory.NextId,drops.Select(d=>d.Item.Id+1).DefaultIfEmpty(1).Max()));
        Health=next.Health;Stamina=next.Stamina;Potions=next.Potions;Weapon=next.Weapon;Order=next.Order;SupportCooldown=next.SupportCooldown;Kills=next.Kills;Parries=next.Parries;
        AttackTime=AttackBuffer=DodgeTime=HitStop=Hurt=Invulnerable=RiposteTime=RootSnare=ComboWindow=0;Guarding=Moving=false;Combo=0;Shots.Clear();Hazards.Clear();Walk=0;
    }
    private void TakeOverEbba()
    {
        var r=RescueState;r.Karl=CaptureHero();
        var gear=new InventoryState{NextId=Inventory.NextId,Bag=new(),Stash=new(),Equipped=new(),Drops=new()};
        foreach(var id in new[]{"admiral-saber","service-pistol","admiral-coat","ward"}){var item=gear.Create(id);gear.Equipped[item.Data.Slot]=item;}
        r.Ebba=new(){Inventory=gear,Weapon=Weapon.Saber,Order=Order.Medicine};r.PlayingEbba=true;ExchangeHero(r.Ebba);r.Ebba=null;
        CabinState.ReturnRoom=Gamla.Landing;EnterConnectedRoom(Cabin.Room);Player=Cabin.Rest;Emit("hero-change",Player);
        Emit("radio",Player,"rescue-trap-hedvig");Emit("radio",Player,"rescue-takeover");Emit("radio",Player,"rescue-armory");Emit("checkpoint",Player);
    }
    private bool StepRescueCaptivity(Controls input)
    {
        if(!Captured)return false;bool pressed=input.Interact&&!_roomInteractHeld;_roomInteractHeld=input.Interact;
        if(pressed)TakeOverEbba();return true;
    }
    private bool RescueCabin(Func<Vector2,bool> near)
    {
        if(!InCabin||!SaltState.Debriefed)return false;var r=RescueState;
        if(IsEbba)
        {
            if(near(Cabin.Rest)){if(!r.Ready){r.Ready=true;Emit("radio",Player,"rescue-ready");Emit("checkpoint",Player);}else Emit("room-notice",Player,"Sabel och tjänstepistol · TAB byter vapen.");return true;}
            if(near(Cabin.Helm)){Emit("room-notice",Player,"Karl är under högen. Använd landgången.");return true;}
            if(near(Cabin.Talk)||near(Salt.Marta)){Emit("radio",Player,"rescue-armory");return true;}
            return false;
        }
        if(!near(Cabin.Talk))return false;
        if(!r.Briefed){r.Briefed=true;Emit("radio",Player,"rescue-brief");Emit("campaign",Player,"KUNGAMINNET: Följ Saltkällans högra gallerport. Kansliets original ger tillträde till mottagningen. Undersök förhallen och kontrollslussen.");}
        else if(r.Reunited&&!r.Debriefed){r.Debriefed=true;Emit("radio",Player,"rescue-debrief");Emit("radio",Player,"rescue-rest");Emit("campaign",Player,"BÅDA ÄR OMBORD: Kansliets sigill är säkrat och Karl är fri. Innersta valvet väntar, men i kväll får resan vila.");}
        else Emit("radio",Player,r.Debriefed?"rescue-rest":"rescue-brief");Emit("checkpoint",Player);return true;
    }
    private void EnterRescue(string room)
    {
        if(room==Rescue.Hall){Spawn(EnemyKind.Guard,new(730,650));Spawn(EnemyKind.Gunner,new(1120,570));Emit("radio",Player,"rescue-entry");}
        if(room==Rescue.Prison){Spawn(EnemyKind.Guard,new(1020,665));}
        if(room==Rescue.Service){Spawn(EnemyKind.Gunner,new(1060,555));Spawn(EnemyKind.Pikeman,new(700,675));Emit("radio",Player,"rescue-service");}
        if(room==Rescue.Machine){Spawn(EnemyKind.Censor,Rescue.Boss);RescueState.Awake=true;Emit("radio",Player,"rescue-censor");Emit("radio",Player,"rescue-aim");}
    }
    private bool StepRescue(Func<Vector2,bool> near)
    {
        if(!InRescue)return false;var r=RescueState;string room=Rooms!.Current;bool Safe()=>EncounterEnemies.All(e=>e.Dead);
        if(room==Rescue.Hall&&near(Rescue.Writ))
        {if(!Safe()){Emit("room-notice",Player,"Säkra förhallen först.");return true;}if(!r.WritRead){r.WritRead=true;Emit("radio",Player,"rescue-writ");}Emit("campaign",Player,"MOTTAGNING: Originalhandlingens sigill skall läsas av i kontrollslussen. Handlingen anger ingen kvarhållning av bäraren. Underhållsritningen visar en sidoväg bakom den bakre serviceporten.");}
        else if(room==Rescue.Prison&&near(Rescue.Reader)&&!IsEbba&&!r.Trapped)
        {if(!Safe()){Emit("room-notice",Player,"Säkra slussen först.");return true;}r.Trapped=true;var door=Rooms.Doors["rescue-prison"];door.Locked=true;door.TargetOpen=false;door.Openness=0;Player=Rescue.Captive;Emit("radio",Player,"rescue-trap-karl");Emit("campaign",Player,"KVARHÅLLNING: Portarna går i lås. Karl lever, men kan inte nå återkallelsen utifrån. E / B: ta över Ebba på fregatten.");}
        else if(room==Rescue.Service&&near(Rescue.Lever))
        {if(!Safe()){Emit("room-notice",Player,"Säkra gången först.");return true;}if(!r.ServiceOpen){r.ServiceOpen=true;Emit("radio",Player,"rescue-lever");Emit("room-sound",Player,"door-unlock");}}
        else if(room==Rescue.Machine&&near(Rescue.Release))
        {if(!r.Defeated){Emit("room-notice",Player,"Sigillmästaren bär återkallelsens sigill.");return true;}if(!r.Released){r.Released=true;Emit("radio",Player,"rescue-open");Emit("room-sound",Player,"stone-door");}}
        else if(room==Rescue.Prison&&IsEbba&&r.Released&&near(Rescue.Talk))
        {
            if(r.Conversation==0){r.Conversation=1;Emit("radio",Player,"rescue-karl-found");Emit("radio",Player,"rescue-ebba-found");Emit("campaign",Player,"KARL ÄR FRI: Tala med honom igen när ni har fått en stund tillsammans.");}
            else
            {
                r.Conversation=2;r.Reunited=true;r.PlayingEbba=false;r.Ebba=CaptureHero();ExchangeHero(r.Karl!);r.Karl=null;Player=Rescue.Talk;Emit("hero-change",Player);
                Emit("radio",Player,"rescue-karl-close");Emit("radio",Player,"rescue-ebba-close");Emit("rescue-reunion",Player);Emit("checkpoint",Player);
            }
        }
        Emit("checkpoint",Player);return true;
    }
    private void StepRescueClock(float dt)
    {if(!InConnectedWorld)return;var r=RescueState;r.PistolReload=Math.Max(0,r.PistolReload-dt);r.PistolFlash=Math.Max(0,r.PistolFlash-dt);}
    private void FireServicePistol()
    {
        var r=RescueState;r.PistolReload=HeavyAttack?1.8f:1.25f;r.PistolFlash=.16f;r.ShotFrom=Player;r.ShotTo=Player+Facing*520;
        Fighter? target=null;float closest=520;
        foreach(var e in EncounterEnemies.Where(e=>!e.Dead))
        {
            var d=e.Position-Player;float along=Vector2.Dot(d,Facing);float side=Math.Abs(d.X*Facing.Y-d.Y*Facing.X);
            if(along>0&&along<closest&&side<30&&ClearPath(Player,e.Position)){closest=along;target=e;}
        }
        if(target!=null)
        {
            r.ShotTo=target.Position;DamageEnemy(target,AttackDamage*(HeavyAttack?1.5f:1),Player,true);
            if(target.Kind==EnemyKind.Censor&&!target.Dead&&target.State==1){target.State=2;target.Timer=1.3f;Emit("inscription",target.Position,"SIKTET BRYTS");}
        }
        // Stop visible tracers at scenery too; shots never hit through walls or closed doors.
        for(float d=8;d<Vector2.Distance(Player,r.ShotTo);d+=8)if(!ClearPath(Player,Player+Facing*d)){r.ShotTo=Player+Facing*(d-8);break;}
        Emit("room-sound",Player,"shot");Emit("pistol",r.ShotTo);Emit("checkpoint",Player);
    }
    private void StepCensor(Fighter e,float dt)
    {
        if(!InRescue||Rooms!.Current!=Rescue.Machine)return;var r=RescueState;e.Hurt=Math.Max(0,e.Hurt-dt);e.Moving=false;e.Cooldown=Math.Max(0,e.Cooldown-dt);e.Facing=Normal(Player-e.Position,Vector2.UnitY);
        if(!r.SecondPhase&&e.Health<e.MaxHealth*.5f){r.SecondPhase=true;Emit("radio",Player,"rescue-second");}
        if(e.State==0){if(e.Cooldown>0)return;e.State=1;e.Timer=r.SecondPhase?.95f:1.45f;e.LockedAim=Player;return;}
        e.Timer-=dt;if(e.Timer>0)return;
        if(e.State==1)
        {
            var aim=Normal(e.LockedAim-e.Position,Vector2.UnitY);
            foreach(float a in new[]{-.16f,0,.16f}){var dir=new Vector2(aim.X*MathF.Cos(a)-aim.Y*MathF.Sin(a),aim.X*MathF.Sin(a)+aim.Y*MathF.Cos(a));Shots.Add(new(){Position=e.Position+dir*35,Velocity=dir*(r.SecondPhase?380:300)});}
            Emit("room-sound",e.Position,"shot");e.State=2;e.Timer=r.SecondPhase?.7f:1.1f;
        }
        else{e.State=0;e.Cooldown=.9f;}
    }
    private bool CensorDeath(Fighter e)
    {
        if(e.Kind!=EnemyKind.Censor||!e.Dead)return false;RescueState.Defeated=true;Kills++;Emit("death",e.Position);Emit("radio",Player,"rescue-defeated");Emit("radio",Player,"rescue-release");Emit("checkpoint",Player);return true;
    }
    private void ValidateRescue()
    {
        if(!InConnectedWorld)return;var r=RescueState;
        if(r==null||r.Briefed&&!SaltState.Debriefed||r.WritRead&&!r.Briefed||r.Trapped&&!r.WritRead||r.PlayingEbba&&(!r.Trapped||r.Reunited)||r.Ready&&!r.Trapped||r.ServiceOpen&&!r.Ready||r.Awake&&!r.ServiceOpen||r.Defeated&&!r.Awake||r.Released&&!r.Defeated||r.Reunited&&!r.Released||r.Debriefed&&!r.Reunited||r.KissSeen&&!r.Reunited||r.Conversation<0||r.Conversation>2||r.Reunited!=(r.Conversation==2)||!float.IsFinite(r.PistolReload)||r.PistolReload<0||r.PistolReload>1.8f)throw new System.IO.InvalidDataException("Ogiltig räddningsexpedition");
        if(r.Karl!=null)r.Karl.Validate();if(r.Ebba!=null)r.Ebba.Validate();
        if(r.PlayingEbba!=(r.Karl!=null)||r.Reunited!=(r.Ebba!=null)||IsEbba&&Weapon==Weapon.Hammer||!IsEbba&&Weapon==Weapon.Pistol||!Inventory.Equipped.ContainsKey(ActiveWeaponSlot))throw new System.IO.InvalidDataException("Ogiltigt karaktärsbyte");
        if(Rescue.Ids.Any(id=>Rooms!.Rooms[id].Visited)&&!r.Briefed||Rooms!.Rooms[Rescue.Prison].Visited&&!r.WritRead||Rooms.Rooms[Rescue.Service].Visited&&!r.Ready||r.Awake!=Rooms.Rooms[Rescue.Machine].Visited)throw new System.IO.InvalidDataException("Ogiltig räddningsväg");
        var bosses=ActorsInRoom(Rescue.Machine).Where(e=>e.Kind==EnemyKind.Censor).ToArray();if(r.Awake?(bosses.Length!=1||bosses[0].Dead!=r.Defeated):bosses.Length!=0)throw new System.IO.InvalidDataException("Ogiltig sigillmästare");
    }
}
