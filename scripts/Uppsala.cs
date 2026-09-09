using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Uppsala
{
    public const string Court="uppsala-court";
    public static readonly string[] Ids=new[]{Court,Meridian.Clock,Meridian.Hall,Cabin.Room}.Concat(Observatory.Ids).ToArray();
    public static bool Known(string id)=>Array.IndexOf(Ids,id)>=0;
    public static readonly Vector2 Origin=new(42000,1000),Board=new(930,490),Ramp=new(280,455),Desk=new(700,440),Seal=new(1040,455);
    // Painted non-walkable landmark; the fog may reveal the hull from its boarding point.
    public static readonly Vector2[] DockedHull={new(690,0),new(1536,0),new(1536,480),new(1270,430),new(1160,355),new(955,460),new(885,450),new(975,340),new(745,240),new(710,110)};
    public static readonly Vector2[] Rings={new(465,550),new(795,610),new(1120,705)};
    public static readonly Vector2[] Centers={new(465,487),new(795,550),new(1120,640)};
    public static readonly int[] Target={2,0,3};
    public static readonly string[] Names={"SOLEN","ÄPPLET","NYCKELN"},Directions={"NORR","ÖSTER","SÖDER","VÄSTER"};
    public static readonly Vector2[] Ground={new(95,440),new(560,270),new(880,355),new(1440,535),new(1370,740),new(1270,920),new(830,795),new(260,565),new(95,490)};
    public static readonly Vector2[][] Obstacles=Centers.Select(p=>new[]{p+new Vector2(-47,-12),p+new Vector2(0,-35),p+new Vector2(47,-12),p+new Vector2(47,18),p+new Vector2(0,39),p+new Vector2(-47,18)}).Append(new Vector2[]{new(635,355),new(710,325),new(770,355),new(770,415),new(700,431),new(635,395)}).ToArray();
}
public sealed class UppsalaRun
{
    public bool Reached,ClueRead,Aligned,Secured,KeyTaken;
    public int Flights;
    public int[] Rings={0,1,0};
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InUppsala=>InConnectedWorld&&Uppsala.Known(Rooms!.Current);
    [JsonIgnore] public UppsalaRun UppsalaState=>Rooms!.Uppsala;
    [JsonIgnore] public string UppsalaGoal=>InObservatory?ObservatoryGoal:InCabin?CabinGoal:InMeridian?MeridianGoal:!UppsalaState.ClueRead?"Läs astronomens anvisning":!UppsalaState.Aligned?"Rikta gårdens tre instrument":!UppsalaState.Secured?"Skydda stjärnplattan":!UppsalaState.KeyTaken?"Undersök portens daterade sigill":MeridianState.OrderTaken?(CabinState.Briefed?(ObservatoryState.OriginalTaken?"Återvänd med originalet till Ebba":"Fortsätt till observatoriet"):"Till landgången · möt Ebba"):MeridianState.CourtOpen?"Fortsätt genom porten till klockgången":"Öppna porten med datumavtrycket";
    [JsonIgnore] public Vector2 UppsalaObjective=>InObservatory?ObservatoryObjective:InCabin?CabinObjective:InMeridian&&MeridianState.OrderTaken&&CabinState.Briefed?(ObservatoryState.OriginalTaken?new Vector2(110,490):Observatory.Gate):InMeridian?(Rooms!.Current==Meridian.Clock?(!MeridianState.LedgerRead?Meridian.Ledger:!MeridianState.ClockAnchored?Meridian.Bell:Meridian.ClockExit):!MeridianState.PlateSet?Meridian.Plate:MeridianState.WardenDefeated?Meridian.Order:MeridianState.Exposed>0?Meridian.Warden:Meridian.Controls[MeridianState.Breaks%3]):UppsalaState.KeyTaken?(MeridianState.OrderTaken&&(!CabinState.Briefed||ObservatoryState.OriginalTaken)?Uppsala.Ramp:Meridian.CourtGate):!UppsalaState.ClueRead?Uppsala.Desk:!UppsalaState.Aligned?Uppsala.Rings[Enumerable.Range(0,3).First(i=>UppsalaState.Rings[i]!=Uppsala.Target[i])]:Uppsala.Seal;
    public static Combat NewUppsalaPreview(Order order)
    {
        var g=NewFoundryPreview(order);g.FoundryState.GateOpen=true;g.EnterConnectedRoom(Foundry.Room);
        g.Enemies.Single(e=>e.Kind==EnemyKind.CrownBailiff).Health=0;g.FoundryState.BailiffDefeated=g.FoundryState.PlateTaken=true;
        g.EnterConnectedRoom(Regiment.Quay);g.Player=Uppsala.Board;g.Events.Clear();g.UpdateRoomSight(true);g.ValidateRooms();return g;
    }
    public bool CanShipTravel(string destination)=>InConnectedWorld&&!Dead&&FoundryState.PlateTaken&&
        ((Rooms!.Current==Regiment.Quay&&destination==Uppsala.Court&&Vector2.Distance(Player,Uppsala.Board)<72)||(Rooms!.Current==Uppsala.Court&&destination==Regiment.Quay&&Vector2.Distance(Player,Uppsala.Ramp)<72))&&EncounterEnemies.All(e=>e.Dead)&&Shots.All(s=>s.Reflected)&&Hazards.All(h=>h.Friendly);
    public bool FinishShipTravel(string destination)
    {
        if(!CanShipTravel(destination))return false;
        Events.Clear();bool first=!UppsalaState.Reached;UppsalaState.Reached=true;UppsalaState.Flights++;
        EnterConnectedRoom(destination);Player=destination==Uppsala.Court?Uppsala.Ramp:Uppsala.Board;UpdateRoomSight(true);
        if(destination==Uppsala.Court&&first)Emit("radio",Player,"uppsala-arrival");
        Emit("region",Player,RoomName);Emit("checkpoint",Player);return true;
    }
    private void AdvanceUppsala()
    {
        if(!InUppsala||Rooms!.Current!=Uppsala.Court||!UppsalaState.Aligned||UppsalaState.Secured||EncounterEnemies.Any(e=>!e.Dead))return;
        UppsalaState.Secured=true;Emit("room-notice",Player,"Gården är säkrad · undersök portens sigill");Emit("checkpoint",Player);
    }
    private bool StepUppsala(Func<Vector2,bool> near)
    {
        if(!InConnectedWorld)return false;
        if(StepObservatory(near))return true;
        if(StepCabin(near))return true;
        if(StepMeridian(near))return true;
        if((Rooms!.Current==Regiment.Quay&&near(Uppsala.Board)&&FoundryState.PlateTaken)||(Rooms!.Current==Uppsala.Court&&near(Uppsala.Ramp)))
        {
            var destination=InUppsala?Regiment.Quay:Uppsala.Court;
            if(!CanShipTravel(destination)){Emit("room-notice",Player,"Säkra platsen innan du går ombord.");return true;}
            if(MeridianState.OrderTaken){BoardCabin();return true;}
            Emit("ship-travel",Player,destination);return true;
        }
        if(!InUppsala)return false;var u=UppsalaState;
        if(near(Uppsala.Desk))
        {if(!u.ClueRead){u.ClueRead=true;Emit("radio",Player,"uppsala-clue");Emit("checkpoint",Player);}Emit("campaign",Player,"ASTRONOMENS ANVISNING: Sol mot söder. Äpple mot norr. Nyckel mot väster. Vrid ringarna enligt namnen, inte enligt solen över muren.");return true;}
        for(int i=0;i<3;i++)if(near(Uppsala.Rings[i]))
        {
            if(!u.ClueRead){Emit("room-notice",Player,"Läs anvisningen vid astronomens bord först.");return true;}
            if(u.Aligned){Emit("room-notice",Player,"Instrumenten är låsta vid stjärnplattan.");return true;}
            u.Rings[i]=(u.Rings[i]+1)%4;Emit("room-sound",Player,"stone-door");Emit("room-notice",Player,$"{Uppsala.Names[i]} · {Uppsala.Directions[u.Rings[i]]}");
            if(u.Rings.SequenceEqual(Uppsala.Target))
            {u.Aligned=true;Spawn(EnemyKind.Guard,new(985,500));Spawn(EnemyKind.Pikeman,new(1230,580));Spawn(EnemyKind.Gunner,new(1290,700));Emit("radio",Player,"uppsala-aligned");Emit("radio",Player,"uppsala-ambush");}
            Emit("checkpoint",Player);return true;
        }
        if(near(Uppsala.Seal))
        {
            if(!u.Secured){Emit("room-notice",Player,"Portens datum saknar en morgon. Undersök gårdens instrument.");return true;}
            if(!u.KeyTaken){u.KeyTaken=true;DropItem("memory",Uppsala.Seal);Emit("radio",Player,"uppsala-secured");Emit("checkpoint",Player);}
            Emit("campaign",Player,"MERIDIANSALENS DATUM: Nyckelavtrycket är säkrat i fältdagboken. Datumet anger portens öppningstid. För in avtrycket i låset vid porten för att nå klockgången.");return true;
        }
        return true;
    }
    private void ValidateUppsala()
    {
        if(!InConnectedWorld)return;var u=UppsalaState;
        if(u is null||u.Rings is null||u.Rings.Length!=3||u.Rings.Any(v=>v<0||v>3)||u.Flights<0||(u.Reached&&(!FoundryState.PlateTaken||u.Flights<1))||Rooms!.Rooms[Uppsala.Court].Visited!=u.Reached||(u.ClueRead&&!u.Reached)||(u.Aligned&&(!u.ClueRead||!u.Rings.SequenceEqual(Uppsala.Target)))||(u.Secured&&!u.Aligned)||(u.KeyTaken&&!u.Secured))throw new System.IO.InvalidDataException("Ogiltig Uppsalafärd");
        var foes=ActorsInRoom(Uppsala.Court);if(foes.Count!=(u.Aligned?3:0)||(u.Secured&&foes.Any(e=>!e.Dead)))throw new System.IO.InvalidDataException("Ogiltig Uppsalavakt");
    }
}
