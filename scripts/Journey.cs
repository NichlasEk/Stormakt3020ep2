using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public enum Region { Quay, Warehouse, Shore, Atland, Roots, Forge, Uppsala }

public sealed partial class Combat
{
    public bool ExtendedJourney;
    public bool Duel;
    public Region Region;
    public bool WhetstoneTaken,ManifestTaken,WinchOpened,AtlandRevealed;
    public bool[] Surveyed=new bool[3];
    public float SurveyProgress;
    public int SurveyIndex=-1;
    public float RiposteTime;
    public Vector2 MoveDirection=Vector2.UnitY;
    [JsonIgnore] private static readonly Vector2[] Crate=Navigation.Expand(JourneyLayout.WarehouseObstacle,22);
    [JsonIgnore] public Vector2[] Walkable=>InDoorTrial?DoorTrialLayout.Ground:Rooms?.Current==PortRooms.Roots?Rootway.Ground:Rooms?.Current==PortRooms.Grove?Rootway.GroveGround:Rooms?.Current==PortRooms.Archive?ArchiveRoom.Ground:Rooms?.Current==PortRooms.Gallery?PortRooms.GalleryGround:Rooms?.Current==PortRooms.Chamber?PortRooms.OathGround:Rooms?.Current==PortRooms.Court?PortRooms.CourtGround:Rooms?.Current==PortRooms.Pump?PortRooms.PumpGround:Rooms?.Current==PortRooms.Cistern?PortRooms.CisternGround:Region==Region.Quay?Ground:(Region==Region.Warehouse||Rooms?.Current==PortRooms.Lodge)?JourneyLayout.WarehouseGround:JourneyLayout.Ground;
    [JsonIgnore] public Vector2[] Obstacles=>Rooms?.Current==PortRooms.Grove?Rootway.Slab:Rooms?.Current==PortRooms.Archive?ArchiveRoom.Table:Rooms?.Current==PortRooms.Gallery?PortRooms.Lectern:Rooms?.Current==PortRooms.Pump?PortRooms.PumpBasin:Rooms?.Current==PortRooms.Cistern?PortRooms.CisternBasin:(Region==Region.Warehouse||Rooms?.Current==PortRooms.Lodge)?Crate:Array.Empty<Vector2>();
    [JsonIgnore] public Vector2[][] SolidObstacles=>InDoorTrial?DoorObstacles:Rooms?.Current==PortRooms.Chamber?PortRooms.OathObstacles:new[]{Obstacles};
    public bool OnWalkable(Vector2 p)=>InConnectedWorld?WorldWalkable(p+WorldOrigin):Navigation.Contains(Walkable,p)&&!SolidObstacles.Any(o=>Navigation.Contains(o,p));
    public Vector2 Bound(Vector2 p)=>InConnectedWorld?WorldBound(p):Region==Region.Quay?ClampToGround(p):Navigation.Clamp(Walkable,SolidObstacles,p);
    public bool ClearPath(Vector2 a,Vector2 b)=>InConnectedWorld?WorldClear(a,b):Region==Region.Quay||Navigation.Clear(Walkable,SolidObstacles,a,b);
    public Vector2 NextWaypoint(Vector2 from,Vector2 target)=>InConnectedWorld?WorldNext(from,target):Region==Region.Quay?target:Navigation.Next(Walkable,SolidObstacles,from,target);
    [JsonIgnore] public string RegionName=>InRooms?RoomName:InCampaign?Stage.Name:Region==Region.Warehouse?"Kronans magasin":Region==Region.Shore?"De tre vittnenas strand":Duel?"Sabelduell vid kajen":"Blekinges likvarv";
    [JsonIgnore] public Vector2 JourneyObjective=>InRooms?RoomObjective:InCampaign?CampaignObjective:Region==Region.Warehouse
        ?!WhetstoneTaken?JourneyLayout.Whetstone:!ManifestTaken?JourneyLayout.Manifest:!WinchOpened?JourneyLayout.Winch:JourneyLayout.WarehouseExit
        :AtlandRevealed?JourneyLayout.ShoreExit:Surveyed.All(s=>s)?JourneyLayout.Reveal
        :JourneyLayout.Survey.Where((_,i)=>!Surveyed[i]).OrderBy(p=>Vector2.DistanceSquared(p,Player)).First();

    public static Combat NewDuel()
    {
        var game=new Combat {Duel=true,Phase=Phase.Duel,Player=new(720,755),Order=Order.Medicine,Potions=2,IntroPlayed=true};
        game.Seals.Clear();game.Spawn(EnemyKind.Guard,new(925,666));game.Enemies[0].Health=game.Enemies[0].MaxHealth=240;
        return game;
    }
    public bool ContinueJourney()
    {
        if(!Dead&&!Duel&&Phase==Phase.Complete&&AtlandRevealed&&!CampaignFinished&&!InCampaign){if(PreferRoomRoute)EnterRoomRoute();else EnterCampaign(0);return true;}
        if(Dead||Duel||Phase!=Phase.Complete||AtlandRevealed||!Inscriptions.All(i=>i.Read))return false;
        ExtendedJourney=true;EnterWarehouse();return true;
    }
    private void EnterWarehouse()
    {
        Region=Region.Warehouse;Phase=Phase.Warehouse;EnterRegion(JourneyLayout.WarehouseEntry,"warehouse-entry");
        Spawn(EnemyKind.Guard,JourneyLayout.WarehouseSpawns[0]);Spawn(EnemyKind.Gunner,JourneyLayout.WarehouseSpawns[1]);
        Spawn(EnemyKind.Guard,JourneyLayout.WarehouseSpawns[2]);Emit("checkpoint",Player);
    }
    private void EnterShore()
    {
        Region=Region.Shore;Phase=Phase.Shore;EnterRegion(JourneyLayout.ShoreEntry,"shore-entry");
        Spawn(EnemyKind.Guard,JourneyLayout.ShoreSpawns[0]);Spawn(EnemyKind.Pikeman,JourneyLayout.ShoreSpawns[1]);
        Emit("checkpoint",Player);
    }
    private void EnterRegion(Vector2 at,string radio)
    {
        Player=at;Facing=Vector2.UnitY;MoveDirection=Facing;
        Enemies.Clear();Seals.Clear();Shots.Clear();Hazards.Clear();AttackTime=AttackBuffer=DodgeTime=HitStop=Hurt=GuardTime=0;
        Guarding=Moving=false;Health=Math.Max(Health,70);Stamina=100;Invulnerable=1;
        Emit("region",Player,RegionName);Emit("radio",Player,radio);
    }
    private void StepJourney(Controls input,float dt)
    {
        if(InDoorTrial){StepDoorTrial(input,dt);return;}
        if(InRooms){StepRooms(input,dt);return;}
        if(InCampaign){StepCampaign(input,dt);return;}
        if(Duel&&Enemies.All(e=>e.Dead)){Phase=Phase.Complete;return;}
        if(Region==Region.Quay)return;
        bool peaceful=Enemies.All(e=>e.Dead);
        bool use=input.Interact&&!Moving&&AttackTime<=0&&DodgeTime<=0&&!Guarding&&Hurt<=0;
        bool Near(Vector2 p)=>Vector2.Distance(Player,p)<78;
        if(Region==Region.Warehouse&&use)
        {
            if(!WhetstoneTaken&&Near(JourneyLayout.Whetstone))
            {WhetstoneTaken=true;Potions++;Emit("inscription",Player,"RONNEBYS BRYNSTÅL");Emit("radio",Player,"whetstone");Emit("checkpoint",Player);return;}
            if(!ManifestTaken&&Near(JourneyLayout.Manifest)&&peaceful)
            {ManifestTaken=true;Emit("inscription",Player,"KOLLEGIETS MÄTORDER");Emit("radio",Player,"manifest");Emit("checkpoint",Player);return;}
            if(!WinchOpened&&Near(JourneyLayout.Winch)&&peaceful)
            {
                WinchOpened=true;Spawn(EnemyKind.Guard,JourneyLayout.WarehouseEntry);Spawn(EnemyKind.Pikeman,JourneyLayout.WarehouseSpawns[0]);
                foreach(var e in Enemies.Where(e=>!e.Dead)){e.State=2;e.Timer=1.3f;}
                Emit("radio",Player,"winch");Emit("checkpoint",Player);return;
            }
            if(ManifestTaken&&WinchOpened&&peaceful&&Near(JourneyLayout.WarehouseExit)){EnterShore();return;}
        }
        if(Region!=Region.Shore)return;
        if(!AtlandRevealed&&Surveyed.All(s=>s)&&peaceful&&use&&Near(JourneyLayout.Reveal))
        {
            AtlandRevealed=true;Phase=Phase.Reveal;Emit("inscription",Player,"VÄGEN LIGGER KVAR");
            Emit("reveal",Player);Emit("radio",Player,"atland-reveal");Emit("radio",Player,"already-here");Emit("checkpoint",Player);return;
        }
        if(AtlandRevealed&&peaceful&&use&&Near(JourneyLayout.ShoreExit))
        {if(AtlandCampaign){if(PreferRoomRoute)EnterRoomRoute();else EnterCampaign(0);return;}Phase=Phase.Complete;Emit("radio",Player,"journey-end");Emit("checkpoint",Player);return;}
        if(!use||!peaceful||AtlandRevealed)return;
        int index=Enumerable.Range(0,3).FirstOrDefault(i=>!Surveyed[i]&&Near(JourneyLayout.Survey[i]),-1);
        if(index<0)return;
        if(SurveyIndex!=index){SurveyIndex=index;SurveyProgress=0;}
        SurveyProgress=Math.Min(2,SurveyProgress+dt);
        if(SurveyProgress<2)return;
        Surveyed[index]=true;SurveyProgress=0;SurveyIndex=-1;
        Emit("inscription",Player,"RIKTNING FASTSTÄLLD");Emit("radio",Player,$"survey-{index}");
        if(Surveyed.All(s=>s))
        {
            Spawn(EnemyKind.Guard,JourneyLayout.ShoreSpawns[0]);Spawn(EnemyKind.Gunner,JourneyLayout.ShoreSpawns[2]);
            Spawn(EnemyKind.Guard,JourneyLayout.ShoreEntry);foreach(var e in Enemies.Where(e=>!e.Dead)){e.State=2;e.Timer=1.5f;}
            Emit("radio",Player,"survey-ambush");
        }
        Emit("checkpoint",Player);
    }
}
