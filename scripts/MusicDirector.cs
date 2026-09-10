using System.Linq;
namespace Atland;
/// Selects a scene cue; an engaged boss keeps its cue until death or room exit.
public sealed class MusicDirector
{
    private string _room="";
    private int _bossId=-1;
    public static string BossCue(EnemyKind kind,bool tribunal=false)=>kind switch
    {EnemyKind.OathGuardian=>"boss-oath",EnemyKind.RootMarshal=>"boss-marshal",EnemyKind.CrownBailiff=>"boss-bailiff",EnemyKind.MeridianWarden=>"boss-meridian",EnemyKind.ZenithGuardian=>"boss-zenith",EnemyKind.MusterOfficer=>"boss-muster",EnemyKind.Collector=>tribunal?"boss-tribunal":"boss-collector",_=>""};
    public string Select(Combat g,bool bossSeen=false,bool boat=false,bool ship=false,bool menu=false)
    {
        if(menu){_room="";_bossId=-1;return "prologue-quay";}
        if(ship)return "travel-ship";if(boat)return "travel-boat";
        string room=g.Rooms?.Current??g.Region+":"+g.CampaignStage;
        if(room!=_room){_room=room;_bossId=-1;}
        var bosses=g.Enemies.Where(e=>!e.Dead&&(!g.InRooms||e.HomeRoom==g.Rooms!.Current)&&BossCue(e.Kind)!=""&&
            (e.Kind!=EnemyKind.ZenithGuardian||g.ObservatoryState.Awake)&&
            (e.Kind!=EnemyKind.MeridianWarden||g.MeridianState.PlateSet)).ToArray();
        if(bossSeen&&_bossId<0)_bossId=bosses.FirstOrDefault()?.Id??-1;
        var boss=bosses.FirstOrDefault(e=>e.Id==_bossId);
        if(boss!=null)
        {
            if(g.InCampaign&&!g.InRooms&&boss.Kind==EnemyKind.Collector)return g.CampaignStage switch{3=>"boss-marshal",5=>"boss-bailiff",7=>"boss-tribunal",_=>"boss-collector"};
            return BossCue(boss.Kind);
        }
        _bossId=-1;
        if(g.InRooms)
        {
            if(g.InRegiment&&g.RegimentState.Discharged&&g.Rooms!.Current is Regiment.Parade or Regiment.Flags or Regiment.Barracks or Regiment.Trail)return "remembrance";
            return g.Rooms!.Current;
        }
        if(g.InCampaign)return g.CampaignStage switch{0=>"court",1=>"archive",2=>"roots",3=>"regiment-parade",4=>"mine-mouth",5=>"mine-foundry",6=>"uppsala-court",_=>"uppsala-meridian"};
        return g.Region==Region.Warehouse?"prologue-warehouse":g.Region==Region.Shore?"prologue-shore":"prologue-quay";
    }
}
