using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public static class ArchiveRoom
{
    public static readonly Vector2 Door=new(155,500),Arrival=new(265,535),Desk=new(800,730),Seal=new(1390,525);
    public static readonly Vector2[] Ground={new(140,490),new(270,460),new(610,320),new(820,365),new(1290,465),new(1450,495),new(1490,545),new(1430,710),new(820,880),new(420,750),new(200,665),new(100,550)};
    public static readonly Vector2[] Table=Navigation.Expand(new Vector2[]{new(586,610),new(652,533),new(940,608),new(871,687)},12);
    public const string Ledger="KOLLEGIETS LIGGARE\n\nByggnadsåret har fastställts till rikets första år. Samtliga arbetare var kronans egendom. Inga civila har vistats under porten.\n\nAvlöningslistorna får därför utgå. Uppgiften om trettiosju saknade personer betraktas som en felsummering.";
    public const string Witness="VITTNESMÅLET\n\nVi bar sten medan vattnet steg. De lovade oss lön efter vintern. När porten stängdes var trettiosju kvar på andra sidan.\n\nJag skrev deras namn på baksidan av ritningen. Om någon finner detta: räkna människorna. Inte stenarna.\n\nIngrid, stenhuggarens dotter";
}

public sealed partial class Combat
{
    [JsonIgnore] public string ArchiveGoal=>!Rooms!.ArchiveRead?"Jämför handlingarna":ArchiveChoice==0?"Välj vad du för vidare":EncounterEnemies.Any(e=>!e.Dead)?"Möt arkivets kontroll":!Rooms.ArchiveSecured?"Säkra arkivets inre grind":"Fortsätt ut på Rotvägen";
    [JsonIgnore] public string ArchiveResult=>ArchiveChoice==1?"Vittnesmålet är bevarat. Hela patrullen kallades in.":ArchiveChoice==2?"Passersedeln är förfalskad. Bara kontrollanten stannade.":"Beslutet återstår.";
    private bool StepArchiveRoom(Func<Vector2,bool> near,bool peaceful)
    {
        if(Rooms!.Current!=PortRooms.Archive)return false;
        if(near(ArchiveRoom.Desk))
        {
            if(!peaceful){Emit("room-notice",Player,"Säkra läsbordet innan du läser.");return true;}
            if(!Rooms.ArchiveRead){Rooms.ArchiveRead=true;Emit("radio",Player,"archive-read");Emit("checkpoint",Player);}
            Emit("archive-open",Player);return true;
        }
        if(near(ArchiveRoom.Seal)&&!Rooms.ArchiveSecured)
        {
            if(ArchiveChoice==0){Emit("room-notice",Player,"Kollegiets kontroll väntar på en handling från läsbordet.");return true;}
            if(!peaceful){Emit("room-notice",Player,"Möt kontrollen innan du säkrar grinden.");return true;}
            if(!Rooms.ArchiveSecured)
            {
                Rooms.ArchiveSecured=true;Emit("inscription",Player,"MINNETS ARKIV ÄR SÄKRAT");Emit("room-sound",Player,"stone-door");
                Emit("radio",Player,"archive-secured");Emit("campaign",Player,"Bakom grinden tar rötterna vid. Arkivets handlingar följer expeditionen. Återvägen till porten och kvarlämnade fynd är öppen.");Emit("checkpoint",Player);
            }
            return true;
        }
        return false;
    }
    public bool ChooseRoomArchive(int choice)
    {
        if(choice is not (1 or 2)||Rooms?.Current!=PortRooms.Archive||!Rooms.ArchiveRead||ArchiveChoice!=0||Dead||Moving||Hurt>0||AttackTime>0||DodgeTime>0||Guarding
            ||Vector2.Distance(Player,ArchiveRoom.Desk)>=72||!ClearPath(Player,ArchiveRoom.Desk)||EncounterEnemies.Any(e=>!e.Dead)||Shots.Any(s=>!s.Reflected)||Hazards.Any(h=>!h.Friendly))return false;
        Events.Clear();ArchiveChoice=choice;
        Spawn(EnemyKind.Guard,Bound(new(1285,575)));
        if(choice==1){Spawn(EnemyKind.Pikeman,Bound(new(1210,530)));Spawn(EnemyKind.Gunner,Bound(new(1320,620)));}
        foreach(var e in EncounterEnemies.Where(e=>!e.Dead)){e.State=2;e.Timer=2.5f;e.Cooldown=1.5f;}
        Emit("radio",Player,choice==1?"archive-preserve":"archive-forge");Emit("campaign",Player,ArchiveResult);
        Emit("room-sound",ArchiveRoom.Seal,"stone-door");Emit("checkpoint",Player);return true;
    }
}
