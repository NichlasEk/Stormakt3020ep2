using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Gamla
{
    public const string Landing="gamla-landing",Passage="gamla-passage",Registry="gamla-registry";
    public static readonly string[] InitialIds={Landing,Passage,Registry};
    public static readonly string[] Ids=InitialIds.Concat(West.Ids).Concat(Salt.Ids).Concat(Rescue.Ids).ToArray();
    public static bool Known(string id)=>Array.IndexOf(Ids,id)>=0;
    public static string Name(string id)=>Rescue.Known(id)?Rescue.Name(id):Salt.Known(id)?Salt.Name(id):West.Known(id)?West.Name(id):id switch{Landing=>"Kungshögarnas uppställningsplats",Passage=>"Gången under högen",_=>"De överfördas väntrum"};
    public static Vector2 Origin(string id)=>new(60000+Array.IndexOf(Ids,id)*2100,1200+Array.IndexOf(Ids,id)*250);
    public static readonly Vector2 Board=new(365,485),Camp=new(320,655),Ledger=new(865,390),Nils=new(900,390),Talk=new(925,460);
    public static Vector2[] Ground(string id)=>Rescue.Known(id)?Rescue.Ground:Salt.Known(id)?Salt.RoomGround(id):West.Known(id)?West.Ground:id switch
    {
        Landing=>new Vector2[]{new(260,480),new(405,440),new(750,470),new(1175,455),new(1300,545),new(1360,780),new(1040,900),new(500,850),new(200,730)},
        Passage=>new Vector2[]{new(65,460),new(300,390),new(780,350),new(1000,380),new(1360,410),new(1440,550),new(1220,775),new(780,930),new(310,810),new(60,550)},
        _=>new Vector2[]{new(75,465),new(300,410),new(700,350),new(1010,355),new(1300,480),new(1380,690),new(1060,850),new(680,935),new(300,790),new(70,550)}
    };
    public static Vector2[][] Obstacles(string id)=>Rescue.Known(id)?Rescue.Obstacles(id):Salt.Known(id)?Salt.Obstacles(id):West.Known(id)?West.Obstacles(id):id switch
    {
        Passage=>new[]{new Vector2[]{new(620,425),new(740,360),new(960,415),new(975,530),new(875,570),new(620,495)}},
        Registry=>new[]{new Vector2[]{new(550,510),new(590,492),new(770,568),new(768,638),new(733,654),new(550,568)},new Vector2[]{new(880,380),new(900,372),new(923,383),new(923,405),new(900,415),new(880,403)}},
        _=>Array.Empty<Vector2[]>()
    };
}
public sealed class GamlaRun
{
    public bool Reached,CampRead,LedgerRead,WitnessMet,Debriefed;
    public int Conversation;
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InGamla=>InConnectedWorld&&Gamla.Known(Rooms!.Current);
    [JsonIgnore] public GamlaRun GamlaState=>Rooms!.Gamla;
    [JsonIgnore] public string GamlaGoal=>InRescue?RescueGoal:RescueState.Briefed&&!RescueState.Reunited?"Till Kungaminnet · följ Saltkällans högra port":InSalt?SaltGoal:WestState.Debriefed?(SaltState.Released?"Återvänd till Ebba":SaltState.Reunited?"Till porten bakom Västra vågen":"Till Ebba · systrarnas återförening"):InWest?WestGoal:WestState.ElinMet?"Återvänd med Elin till Ebba":GamlaState.Debriefed?"Fortsätt genom väntrummets västra port":GamlaState.WitnessMet?"Ta Nils vittnesmål till Ebba":Rooms!.Current==Gamla.Landing?!GamlaState.CampRead?"Undersök den övergivna uppställningen":"Följ trappan in under högen":Rooms.Current==Gamla.Passage?!GamlaState.LedgerRead?"Säkra gången · läs transportliggaren":"Fortsätt genom den inre porten":"Säkra väntrummet · tala med Nils";
    [JsonIgnore] public Vector2 GamlaObjective=>InRescue?RescueObjective:RescueState.Briefed&&!RescueState.Reunited?(Rooms!.Current==Gamla.Landing?new(1175,495):Rooms.Current==Gamla.Passage?new(1300,465):Rooms.Current==Gamla.Registry?new(1300,460):new(1400,490)):InSalt?SaltObjective:WestState.Debriefed&&SaltState.Reunited&&!SaltState.Released?(Rooms!.Current==Gamla.Landing?new(1175,495):Rooms.Current==Gamla.Passage?new(1300,465):Rooms.Current==Gamla.Registry?new(1300,460):new(1400,490)):InWest?WestObjective:GamlaState.Debriefed&&!WestState.ElinMet?(Rooms!.Current==Gamla.Registry?new(1300,460):Rooms.Current==Gamla.Passage?new(1300,465):new(1175,495)):GamlaState.WitnessMet?(Rooms!.Current==Gamla.Landing?Gamla.Board:new(100,490)):Rooms!.Current==Gamla.Landing?GamlaState.CampRead?new(1175,495):Gamla.Camp:Rooms.Current==Gamla.Passage?GamlaState.LedgerRead?new(1300,465):Gamla.Ledger:Gamla.Talk;
    public static Combat NewGamlaPreview(Order order)
    {
        var g=NewObservatoryPreview(order);g.ObservatoryState.EntryOpen=g.ObservatoryState.MartaMet=g.ObservatoryState.DiagramRead=g.ObservatoryState.Aligned=true;g.ObservatoryState.Brakes=(int[])Observatory.Target.Clone();
        foreach(var room in Observatory.Ids){g.EnterConnectedRoom(room);foreach(var e in g.ActorsInRoom(room))e.Health=0;}
        g.ObservatoryState.Awake=g.ObservatoryState.Defeated=g.ObservatoryState.OriginalTaken=g.ObservatoryState.Debriefed=true;
        g.EnterConnectedRoom(Cabin.Room);g.Player=Cabin.Helm;g.CabinState.ReturnRoom=Uppsala.Court;g.Health=g.Stamina=100;g.PaintedRooms=true;g.Events.Clear();g.ValidateRooms();return g;
    }
    public bool FinishGamlaFlight(string destination)
    {
        if(!InCabin||!ObservatoryState.Debriefed||destination is not (Gamla.Landing or Uppsala.Court))return false;
        bool first=!GamlaState.Reached;if(destination==Gamla.Landing)GamlaState.Reached=true;
        CabinState.ReturnRoom=destination;EnterConnectedRoom(destination);Player=destination==Gamla.Landing?Gamla.Board:Uppsala.Ramp;
        Emit("region",Player,RoomName);Emit("radio",Player,destination==Gamla.Landing?"gamla-arrival":"gamla-return");
        if(first&&destination==Gamla.Landing)Emit("cinematic",Player,"gamla-arrival");
        UpdateRoomSight(true);Emit("checkpoint",Player);return true;
    }
    private void EnterGamla(string room)
    {
        if(Rescue.Known(room)){EnterRescue(room);return;}if(Salt.Known(room)){EnterSalt(room);return;}
        if(West.Known(room)){EnterWest(room);return;}
        if(room==Gamla.Landing){Spawn(EnemyKind.Guard,new(720,600));Spawn(EnemyKind.Gunner,new(1080,665));}
        if(room==Gamla.Passage){Spawn(EnemyKind.Guard,new(930,600));Spawn(EnemyKind.Pikeman,new(1100,690));Spawn(EnemyKind.Gunner,new(430,640));Emit("radio",Player,"gamla-below");}
        if(room==Gamla.Registry){Spawn(EnemyKind.Guard,new(620,600));Spawn(EnemyKind.Pikeman,new(1150,640));Emit("radio",Player,"gamla-waiting");}
    }
    private bool StepGamla(Func<Vector2,bool> near)
    {
        if(StepRescue(near))return true;if(StepSalt(near))return true;if(StepWest(near))return true;if(!InGamla)return false;var s=GamlaState;bool safe=EncounterEnemies.All(e=>e.Dead);
        if(Rooms!.Current==Gamla.Landing)
        {
            if(near(Gamla.Board))
            {
                if(!safe){Emit("room-notice",Player,"Driv bort vakterna från landgången först.");return true;}
                CabinState.ReturnRoom=Gamla.Landing;EnterConnectedRoom(Cabin.Room);Player=Cabin.Entry;Emit("cabin-enter",Player);UpdateRoomSight(true);Emit("checkpoint",Player);return true;
            }
            if(near(Gamla.Camp))
            {if(!s.CampRead){s.CampRead=true;Emit("radio",Player,"gamla-camp");Emit("checkpoint",Player);}Emit("campaign",Player,"UPPSTÄLLNINGEN: Matkärl, skor och resväskor. Här väntade människor på transporten. De tomma platserna är inte gravar. Fotspåren fortsätter mot trappan under högen.");return true;}
        }
        if(Rooms.Current==Gamla.Passage&&near(Gamla.Ledger))
        {
            if(!safe){Emit("room-notice",Player,"Säkra gången innan du läser liggaren.");return true;}
            if(!s.LedgerRead){s.LedgerRead=true;Emit("radio",Player,"gamla-ledger");Emit("room-sound",Player,"door-unlock");Emit("checkpoint",Player);}Emit("campaign",Player,"TRANSPORTLIGGAREN: Märta Vinge står som ej inställd. Elin Vinge är mottagen och överförd till VÄSTRA VÅGEN. Kvittensen bär ett annat bläck än ordern. Den inre portens spärr släpper när originalets sigill läggs mot läsblecket.");return true;
        }
        if(Rooms.Current==Gamla.Registry&&near(Gamla.Talk))
        {
            if(!safe){Emit("room-notice",Player,"Håll vakterna borta från mannen vid väggen.");return true;}
            if(s.Conversation==0){s.Conversation=1;Emit("radio",Player,"gamla-nils");Emit("radio",Player,"gamla-counted");}
            else if(s.Conversation==1){s.Conversation=2;s.WitnessMet=true;DropItem("memory",new(965,485));Emit("radio",Player,"gamla-elin");Emit("radio",Player,"gamla-evidence");Emit("campaign",Player,"NILS VITTNESMÅL: Elin levde när hon fördes mot Västra vågen. Nils har sparat hennes kvittens. Vad som finns bortom porten vet han inte. Ta kvittensen och vittnesmålet till Ebba. Västra vågen blir expeditionens nästa etapp.");}
            else Emit("radio",Player,"gamla-nils-repeat");
            Emit("checkpoint",Player);return true;
        }
        return true;
    }
    private void ValidateGamla()
    {
        if(!InConnectedWorld)return;var s=GamlaState;
        if(s is null||s.Conversation<0||s.Conversation>2||(s.Reached&&!ObservatoryState.Debriefed)||(Rooms!.Rooms[Gamla.Landing].Visited&&!s.Reached)||(s.CampRead&&!Rooms.Rooms[Gamla.Landing].Visited)||(s.LedgerRead&&!Rooms.Rooms[Gamla.Passage].Visited)||(Rooms.Rooms[Gamla.Registry].Visited&&!s.LedgerRead)||(s.Conversation>0&&!Rooms.Rooms[Gamla.Registry].Visited)||s.WitnessMet!=(s.Conversation==2)||(s.Debriefed&&!s.WitnessMet))throw new System.IO.InvalidDataException("Ogiltig färd till Gamla Uppsala");
    }
}
