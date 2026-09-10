using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class West
{
    public const string Control="gamla-west-control",Hall="gamla-west-hall",Refuge="gamla-west-refuge";
    public static readonly string[] Ids={Control,Hall,Refuge};
    public static bool Known(string id)=>Ids.Contains(id);
    public static string Name(string id)=>id==Control?"Den västra kontrollgången":id==Hall?"Västra vågen":"Rummet bakom vågen";
    public static readonly Vector2 Aboard=Cabin.FromPainting(new(1100,560));
    public static readonly Vector2 Register=new(790,450),Boss=new(790,565),Elin=new(1180,570),Talk=new(1140,625),Record=new(850,605);
    public static readonly Vector2[] Brakes={new(415,585),new(1090,590)};
    public static Vector2[] Ground=>new Vector2[]{new(65,460),new(280,400),new(700,390),new(1100,410),new(1410,425),new(1460,570),new(1200,790),new(790,920),new(310,790),new(65,560)};
    public static Vector2[][] Obstacles(string id)=>id==Hall?Brakes.Select(p=>new[]{p+new Vector2(-26,-75),p+new Vector2(26,-75),p+new Vector2(26,-30),p+new Vector2(-26,-30)}).ToArray():id==Refuge?new[]{new Vector2[]{new(1160,555),new(1200,555),new(1200,585),new(1160,585)},new Vector2[]{new(775,460),new(875,420),new(975,480),new(930,575),new(835,565),new(775,530)}}:Array.Empty<Vector2[]>();
}
public sealed class WestRun
{
    public bool RegisterRead,Awake,Reinforced,Defeated,RecordRead,ElinMet,Debriefed;
    public int Conversation,Brakes;
    public float Exposed;
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InWest=>InConnectedWorld&&West.Known(Rooms!.Current);
    [JsonIgnore] public WestRun WestState=>Rooms!.West;
    [JsonIgnore] public string WestGoal=>WestState.ElinMet?"Tillbaka till fregatten med Elin":Rooms!.Current==West.Control?WestState.RegisterRead?"Fortsätt till vågsalen":"Säkra kontrollen · läs instruktionen":Rooms.Current==West.Hall?WestState.Defeated?"Öppna porten bakom vågen":WestState.Exposed>0?"Vågen är avlastad · angrip förrättaren":"Lossa båda bromsarna · undvik nedslagen":!WestState.RecordRead?"Läs den kvarhållnas handling":"Tala med Elin Vinge";
    [JsonIgnore] public Vector2 WestObjective=>WestState.ElinMet?new(140,490):Rooms!.Current==West.Control?WestState.RegisterRead?new(1400,490):West.Register:Rooms.Current==West.Hall?WestState.Defeated?new(1400,490):WestState.Exposed>0?West.Boss:West.Brakes[(WestState.Brakes&1)==0?0:1]:WestState.RecordRead?West.Talk:West.Record;
    public static Combat NewWestPreview(Order order)
    {
        var g=NewGamlaPreview(order);g.FinishGamlaFlight(Gamla.Landing);g.GamlaState.CampRead=g.GamlaState.LedgerRead=true;
        g.EnterConnectedRoom(Gamla.Passage);g.EnterConnectedRoom(Gamla.Registry);foreach(var e in g.Enemies)e.Health=0;
        g.GamlaState.Conversation=2;g.GamlaState.WitnessMet=g.GamlaState.Debriefed=true;g.Player=new(1210,530);g.Events.Clear();g.Health=g.Stamina=100;g.ValidateRooms();return g;
    }
    private void EnterWest(string room)
    {
        if(room==West.Control){Spawn(EnemyKind.Guard,new(640,595));Spawn(EnemyKind.Gunner,new(1060,490));Spawn(EnemyKind.Pikeman,new(1050,700));Emit("radio",Player,"west-entry");}
        if(room==West.Hall){Spawn(EnemyKind.MusterOfficer,West.Boss);WestState.Awake=true;Emit("radio",Player,"west-officer");Emit("radio",Player,"west-brakes");}
        if(room==West.Refuge)Emit("radio",Player,"west-breath");
    }
    private bool StepWest(Func<Vector2,bool> near)
    {
        if(!InWest)return false;var s=WestState;
        if(Rooms!.Current==West.Control&&near(West.Register))
        {
            if(EncounterEnemies.Any(e=>!e.Dead)){Emit("room-notice",Player,"Säkra kontrollgången först.");return true;}
            if(!s.RegisterRead){s.RegisterRead=true;Emit("radio",Player,"west-register");Emit("checkpoint",Player);}
            Emit("campaign",Player,"VÅGENS INSTRUKTION: Två bromsar håller motvikterna belastade. Lossa båda för att avlasta vågen. Förrättarens skydd är kopplat till belastningen. Nils kvittens ger tillträde till kontrollgången, men den säger inte vart de vägda fördes.");return true;
        }
        if(Rooms.Current==West.Hall&&!s.Defeated)
        {
            for(int i=0;i<2;i++)if(near(West.Brakes[i]))
            {
                if(s.Exposed>0){Emit("room-notice",Player,"Bromsarna är lossade · angrip nu.");return true;}
                s.Brakes|=1<<i;Emit("room-sound",Player,"door-unlock");
                if(s.Brakes==3){s.Exposed=9;Emit("inscription",Player,"VÅGEN AVLASTAD");Emit("radio",Player,"west-open");}
                Emit("checkpoint",Player);return true;
            }
        }
        if(Rooms.Current==West.Refuge)
        {
            if(near(West.Record))
            {if(!s.RecordRead){s.RecordRead=true;Emit("radio",Player,"west-record");Emit("checkpoint",Player);}Emit("campaign",Player,"ELINS HANDLING: Kvarhållen efter vägran att intyga ett annat namn. Övriga i hennes grupp har sänts till SALTKÄLLAN. Handlingen bär ett mottagningssigill men inget namn på beställaren.");return true;}
            if(near(West.Talk))
            {
                if(!s.RecordRead){Emit("radio",Player,"west-elin-first");Emit("room-notice",Player,"Läs handlingen på bordet innan ni lämnar rummet.");return true;}
                if(s.Conversation==0){s.Conversation=1;Emit("radio",Player,"west-elin-first");Emit("radio",Player,"west-elin-name");}
                else if(s.Conversation==1){s.Conversation=2;s.ElinMet=true;DropItem("memory",new(1000,535));Emit("radio",Player,"west-elin-route");Emit("radio",Player,"west-rescue");Emit("campaign",Player,"ELIN ÄR ÅTERFUNNEN: Hon lever. Hon vägrade intyga ett främmande namn och blev kvarhållen. Övriga fördes mot Saltkällan. Vägen tillbaka är säkrad; återvänd med henne till Ebba ombord på fregatten.");}
                else Emit("radio",Player,"west-elin-repeat");
                Emit("checkpoint",Player);return true;
            }
        }
        return true;
    }
    private void StepWestClock(float dt)
    {
        if(!InWest)return;var s=WestState;var e=Enemies.FirstOrDefault(e=>e.Kind==EnemyKind.MusterOfficer&&!e.Dead&&e.HomeRoom==Rooms!.Current);
        if(e!=null&&!s.Reinforced&&e.Health<e.MaxHealth*.5f){s.Reinforced=true;Spawn(EnemyKind.Guard,new(350,650));Spawn(EnemyKind.Guard,new(1210,710));Emit("radio",Player,"west-reinforce");}
if(s.Exposed>0){s.Exposed=Math.Max(0,s.Exposed-dt);if(s.Exposed==0){s.Brakes=0;Emit("inscription",Player,"BROMSARNA GRIPER IGEN");}}
    }
    private void StepMusterOfficer(Fighter e,float dt)
    {
        e.Moving=false;if(!InWest||Rooms!.Current!=West.Hall)return;var s=WestState;
        e.Facing=Normal(Player-e.Position,Vector2.UnitY);e.Cooldown=Math.Max(0,e.Cooldown-dt);

        if(e.State==0){if(e.Cooldown>0)return;e.State=1;e.Timer=e.Health<e.MaxHealth*.5f?1.1f:1.5f;e.LockedAim=Player;Emit("warning",e.Position);return;}
        e.Timer-=dt;if(e.Timer>0)return;
        if(e.State==1){if(Vector2.Distance(Player,e.LockedAim)<78)DamagePlayer(24,e.LockedAim+new Vector2(0,-20));Emit("impact",e.LockedAim);Emit("room-sound",e.LockedAim,"mine-warning");e.State=2;e.Timer=1.1f;}
        else{e.State=0;e.Cooldown=.65f;}
    }
    private bool WestDeath(Fighter e)
    {
        if(!e.Dead||e.Kind!=EnemyKind.MusterOfficer)return false;WestState.Defeated=true;WestState.Exposed=0;Kills++;DropItem("crown-helm",e.Position);Emit("death",e.Position);Emit("radio",Player,"west-fallen");Emit("inscription",Player,"VÅGEN STANNAR");Emit("checkpoint",Player);return true;
    }
    private void ValidateWest()
    {
        if(!InConnectedWorld)return;var s=WestState;
        if(s is null||s.Brakes<0||s.Brakes>3||!float.IsFinite(s.Exposed)||s.Exposed<0||s.Exposed>9||s.Conversation<0||s.Conversation>2||s.ElinMet!=(s.Conversation==2)||(s.Debriefed&&!s.ElinMet)||(Rooms!.Rooms[West.Control].Visited&&!GamlaState.Debriefed)||(s.RegisterRead&&!Rooms.Rooms[West.Control].Visited)||(Rooms.Rooms[West.Hall].Visited&&!s.RegisterRead)||(s.Awake!=Rooms.Rooms[West.Hall].Visited)||(s.Defeated&&!s.Awake)||(Rooms.Rooms[West.Refuge].Visited&&!s.Defeated)||(s.RecordRead&&!Rooms.Rooms[West.Refuge].Visited)||(s.Conversation>0&&!s.RecordRead))throw new System.IO.InvalidDataException("Ogiltig Västra vågen");
        var bosses=ActorsInRoom(West.Hall).Where(e=>e.Kind==EnemyKind.MusterOfficer).ToArray();if(s.Awake?(bosses.Length!=1||bosses[0].Dead!=s.Defeated):bosses.Length!=0)throw new System.IO.InvalidDataException("Ogiltig mönstringsförrättare");
    }
}
