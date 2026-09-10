using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Salt
{
    public const string Loading="salt-loading",Stairs="salt-stairs",Archive="salt-archive",Spring="salt-spring";
    public static readonly string[] Ids={Loading,Stairs,Archive,Spring};
    public static bool Known(string id)=>Ids.Contains(id);
    public static string Name(string id)=>id switch{Loading=>"Saltkällans lastplats",Stairs=>"Salttrappan",Archive=>"Det dränkta registret",_=>"Saltkällans minnesverk"};
    public static readonly Vector2 Manifest=new(915,475),Wheel=new(790,470),Ledger=new(865,420),Cache=new(590,385),Boss=new(800,580),Release=new(800,510),Shortcut=new(905,440);
    public static readonly Vector2 Marta=Cabin.FromPainting(new(1230,660));
    public static readonly Vector2[] Chains={new(410,585),new(1120,585)};
    public static Vector2[] Ground=>new Vector2[]{new(65,460),new(300,405),new(780,420),new(1120,430),new(1460,460),new(1460,600),new(1200,805),new(790,920),new(300,800),new(65,575)};
    public static Vector2[] RoomGround(string id)=>id==Archive?new Vector2[]{new(65,365),new(280,350),new(780,360),new(1120,350),new(1460,365),new(1460,600),new(1200,805),new(790,920),new(300,800),new(65,575)}:Ground;
    public static Vector2[][] Obstacles(string id)
    {
        Vector2[] Box(float x,float y,float w,float h)=>new Vector2[]{new(x,y),new(x+w,y),new(x+w,y+h),new(x,y+h)};
        return id switch{Loading=>new[]{Box(510,345,170,65),Box(825,345,180,80),Box(1050,365,210,70)},Archive=>new[]{Box(700,295,175,70),Box(740,360,55,65)},Spring=>new[]{Box(700,375,145,85),Box(360,525,105,40),Box(1070,525,105,40)},_=>Array.Empty<Vector2[]>()};
    }
    public static readonly Vector2[][] FloodZones={new Vector2[]{new(185,635),new(280,610),new(390,600),new(575,610),new(640,800),new(565,825),new(340,850)},new Vector2[]{new(975,610),new(1140,600),new(1295,625),new(1330,675),new(1190,830),new(945,805)}};
    public static bool Flooded(Vector2 p,int chains)=>Enumerable.Range(0,2).Any(i=>(chains&(1<<i))==0&&Navigation.Contains(FloodZones[i],p));
}
public sealed class SaltRun
{
    public bool Reunited,ManifestRead,Drained,LedgerRead,CacheTaken,Awake,SecondPhase,Defeated,Released,Debriefed,ShortcutOpen,FilmSeen;
    public int Chains;
    public float Tide=8,WaterDamage;
}
public sealed partial class Combat
{
    [JsonIgnore] public SaltRun SaltState=>Rooms!.Salt;
    [JsonIgnore] public bool InSalt=>InConnectedWorld&&Salt.Known(Rooms!.Current);
    [JsonIgnore] public string SaltGoal=>SaltState.Released?"Återvänd till Ebba · underhållsgången är öppen":Rooms!.Current==Salt.Loading?SaltState.ManifestRead?"Fortsätt genom högra porten":"Säkra lastplatsen · läs manifestet":Rooms.Current==Salt.Stairs?SaltState.Drained?"Fortsätt till registret":"Stäng tillförseln vid hjulet":Rooms.Current==Salt.Archive?SaltState.LedgerRead?"Fortsätt till minnesverket":"Säkra registret · läs instruktionen":SaltState.Defeated?"Häv kvarhållningen vid pulpeten":"Besegra Saltväktaren · avlasta kedjespelen";
    [JsonIgnore] public Vector2 SaltObjective=>SaltState.Released?(Rooms!.Current==Salt.Spring?Salt.Shortcut:new(140,490)):Rooms!.Current==Salt.Loading?SaltState.ManifestRead?new(1400,490):Salt.Manifest:Rooms.Current==Salt.Stairs?SaltState.Drained?new(1400,490):Salt.Wheel:Rooms.Current==Salt.Archive?SaltState.LedgerRead?new(1400,490):Salt.Ledger:SaltState.Defeated?Salt.Release:Salt.Boss;
    public static Combat NewSaltPreview(Order order)
    {
        var g=NewWestPreview(order);g.EnterConnectedRoom(West.Control);foreach(var e in g.EncounterEnemies)e.Health=0;g.WestState.RegisterRead=true;
        g.EnterConnectedRoom(West.Hall);foreach(var e in g.EncounterEnemies)e.Health=0;g.WestState.Defeated=true;
        g.EnterConnectedRoom(West.Refuge);g.WestState.RecordRead=g.WestState.ElinMet=g.WestState.Debriefed=true;g.WestState.Conversation=2;
        g.EnterConnectedRoom(Cabin.Room);g.CabinState.ReturnRoom=Gamla.Landing;g.Player=Cabin.Talk;g.Health=g.Stamina=100;g.Events.Clear();g.ValidateRooms();return g;
    }
    private bool SaltCabin(Func<Vector2,bool> near)
    {
        if(!InCabin||!WestState.Debriefed)return false;
        if(!near(Cabin.Talk)&&!near(Salt.Marta))return false;
        var s=SaltState;
        if(!s.Reunited)
        {
            s.Reunited=true;
            foreach(var id in new[]{"salt-reunion-arrival","salt-reunion-marta","salt-reunion-elin","salt-reunion-answer","salt-reunion-route"})Emit("radio",Player,id);
            Emit("campaign",Player,"SYSTRARNA ÄR ÅTERFÖRENADE: Märta är hämtad från observatoriet. De stannar ombord. Återvänd till rummet bakom Västra vågen och följ dess högra port till Saltkällans lastplats.");
        }
        else if(s.Released&&!s.Debriefed){s.Debriefed=true;Emit("radio",Player,"salt-return");Emit("radio",Player,"salt-debrief");Emit("campaign",Player,"SALTKÄLLANS VITTNEN ÄR OMBORD: Namnbytena skulle ge kansliet tillträde till Kungaminnet. Beställningen finns hos Ebba. Expeditionen gör halt här tills nästa färd är förberedd.");}
        else Emit("radio",Player,s.Debriefed?"salt-debrief":"salt-reunion-route");
        Emit("checkpoint",Player);return true;
    }
    private void EnterSalt(string room)
    {
        if(room==Salt.Loading){Spawn(EnemyKind.Guard,new(720,640));Spawn(EnemyKind.Gunner,new(1110,550));Emit("radio",Player,"salt-loading");}
        if(room==Salt.Stairs){Spawn(EnemyKind.Pikeman,new(900,620));Emit("radio",Player,"salt-stairs");}
        if(room==Salt.Archive){Spawn(EnemyKind.Guard,new(630,650));Spawn(EnemyKind.Gunner,new(1120,520));}
        if(room==Salt.Spring){Spawn(EnemyKind.SaltWarden,Salt.Boss);SaltState.Awake=true;if(!SaltState.FilmSeen){SaltState.FilmSeen=true;Emit("cinematic",Player,"salt-reveal");}Emit("radio",Player,"salt-watcher");Emit("radio",Player,"salt-chains");}
    }
    private bool StepSalt(Func<Vector2,bool> near)
    {
        if(!InSalt)return false;var s=SaltState;string room=Rooms!.Current;
        bool Safe()=>!EncounterEnemies.Any(e=>!e.Dead);
        if(room==Salt.Loading&&near(Salt.Manifest))
        {if(!Safe()){Emit("room-notice",Player,"Säkra lastplatsen först.");return true;}if(!s.ManifestRead){s.ManifestRead=true;Emit("radio",Player,"salt-manifest");}Emit("campaign",Player,"LASTMANIFEST: Levande vittnen skall intyga de äldre identiteter som anges på namnplåtarna. Mottagare: Saltkällans minnesverk. Vittnena hålls i den inre slussen till avslutad överföring.");}
        else if(room==Salt.Stairs&&near(Salt.Wheel))
        {if(!Safe()){Emit("room-notice",Player,"Säkra trappan först.");return true;}if(!s.Drained){s.Drained=true;Emit("radio",Player,"salt-drained");Emit("room-sound",Player,"pump-drain");}}
        else if(room==Salt.Archive&&near(Salt.Ledger))
        {if(!Safe()){Emit("room-notice",Player,"Säkra registret först.");return true;}if(!s.LedgerRead){s.LedgerRead=true;Emit("radio",Player,"salt-archive");Emit("radio",Player,"salt-proof");}Emit("campaign",Player,"MINNESVERKETS INSTRUKTION: Kartan anger vägen. Gjutformen tillverkar stjärnplåten, som riktar verket. Ett levande intyg under äldre namn ger tillträde till Kungaminnet. Slussens kvarhållning hävs vid källans pulpet. Detta är kansliets försök att kringgå verkets kontroll.");}
        else if(room==Salt.Archive&&near(Salt.Cache))
        {if(!Safe()){Emit("room-notice",Player,"Säkra registret först.");return true;}if(!s.CacheTaken){s.CacheTaken=true;Potions+=2;DropItem("memory",Player+new Vector2(35,30));Emit("radio",Player,"salt-cache");}else Emit("room-notice",Player,"Underhållskistan är tom.");}
        else if(room==Salt.Spring)
        {
            for(int i=0;i<2;i++)if(near(Salt.Chains[i])&&!s.Defeated){s.Chains|=1<<i;Emit("room-sound",Player,"door-creak");Emit("inscription",Player,"LUCKAN STÄNGS");Emit("checkpoint",Player);return true;}
            if(near(Salt.Release))
            {
                if(!s.Defeated){Emit("room-notice",Player,"Saltväktaren håller pulpeten.");return true;}
                if(!s.Released){s.Released=s.ShortcutOpen=true;Emit("radio",Player,"salt-release");Emit("radio",Player,"salt-order");Emit("campaign",Player,"KVARTERETS VITTNEN ÄR FRIA: Skytteln möter dem vid lastplatsen. Beställningen bär Riksantikvariens kanslis signatur. Målet är Kungaminnets innersta valv. Ta handlingen till Ebba. Underhållsgången bakom pulpeten leder tillbaka till lastplatsen.");DropItem("crown-helm",Player+new Vector2(55,45));}
            }
        }
        Emit("checkpoint",Player);return true;
    }
    private void StepSaltClock(float dt)
    {
        if(!InSalt||Rooms!.Current!=Salt.Spring||SaltState.Defeated)return;var s=SaltState;
        s.Tide-=dt;s.WaterDamage=Math.Max(0,s.WaterDamage-dt);
        if(s.Tide<=0){s.Tide=12;s.Chains=0;Emit("radio",Player,"salt-rising");}
        // First three seconds are visibly marked before the water becomes dangerous.
        if(s.Tide<9&&Salt.Flooded(Player,s.Chains)&&s.WaterDamage<=0){s.WaterDamage=1;DamagePlayer(9,Player-new Vector2(0,30));}
    }
    private void StepSaltWarden(Fighter e,float dt)
    {
        if(!InSalt||Rooms!.Current!=Salt.Spring)return;var s=SaltState;
        if(!s.SecondPhase&&e.Health<e.MaxHealth*.5f){s.SecondPhase=true;Emit("radio",Player,"salt-second");}
        e.Facing=Normal(Player-e.Position,Vector2.UnitY);e.Moving=false;e.Cooldown=Math.Max(0,e.Cooldown-dt);
        if(e.State==0)
        {
            if(s.SecondPhase&&Vector2.Distance(e.Position,Player)>85){var next=MoveBody(e.Position,e.Position+e.Facing*65*dt);e.Moving=next!=e.Position;e.Walk+=Vector2.Distance(next,e.Position)/42;e.Position=next;}
            if(e.Cooldown>0)return;e.LockedAim=Player;e.State=1;e.Timer=s.SecondPhase?1.05f:1.5f;return;
        }
        e.Timer-=dt;if(e.Timer>0)return;
        if(e.State==1){if(Vector2.Distance(Player,e.LockedAim)<105)DamagePlayer(26,e.Position);Emit("impact",e.LockedAim);Emit("room-sound",e.LockedAim,"oath-impact");e.State=2;e.Timer=.7f;}
        else{e.State=0;e.Cooldown=s.SecondPhase?1.2f:1.8f;}
    }
    private bool SaltDeath(Fighter e)
    {
        if(!e.Dead||e.Kind!=EnemyKind.SaltWarden)return false;SaltState.Defeated=true;SaltState.Chains=3;Kills++;Emit("death",e.Position);Emit("radio",Player,"salt-fallen");Emit("checkpoint",Player);return true;
    }
    private void ValidateSalt()
    {
        if(!InConnectedWorld)return;var s=SaltState;
        if(s==null||s.Chains<0||s.Chains>3||!float.IsFinite(s.Tide)||s.Tide<0||s.Tide>12||!float.IsFinite(s.WaterDamage)||s.WaterDamage<0||s.WaterDamage>1||s.Reunited&&!WestState.Debriefed||s.ManifestRead&&!Rooms!.Rooms[Salt.Loading].Visited||s.Drained&&!s.ManifestRead||s.LedgerRead&&!s.Drained||s.Awake&&!s.LedgerRead||s.Defeated&&!s.Awake||s.Released&&!s.Defeated||s.Debriefed&&!s.Released||s.ShortcutOpen!=s.Released)throw new System.IO.InvalidDataException("Ogiltig Saltkälla");
        if(Salt.Ids.Any(id=>Rooms!.Rooms[id].Visited)&&!s.Reunited)throw new System.IO.InvalidDataException("Saltkällan före återföreningen");
        if(Rooms!.Rooms[Salt.Stairs].Visited&&!s.ManifestRead||Rooms.Rooms[Salt.Archive].Visited&&!s.Drained||s.Drained&&!Rooms.Rooms[Salt.Stairs].Visited||s.LedgerRead&&!Rooms.Rooms[Salt.Archive].Visited||s.CacheTaken&&!Rooms.Rooms[Salt.Archive].Visited||s.Awake!=Rooms.Rooms[Salt.Spring].Visited||s.SecondPhase&&!s.Awake||s.FilmSeen!=s.Awake)throw new System.IO.InvalidDataException("Ogiltig följd i Saltkällan");
        var bosses=ActorsInRoom(Salt.Spring).Where(e=>e.Kind==EnemyKind.SaltWarden).ToArray();if(s.Awake?(bosses.Length!=1||bosses[0].Dead!=s.Defeated):bosses.Length!=0)throw new System.IO.InvalidDataException("Ogiltig Saltväktare");
    }
}
