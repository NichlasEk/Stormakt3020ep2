using Atland;
using System.Numerics;
public static class MeridianTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})foreach(var order in new[]{Order.Artillery,Order.Medicine})
        {
            var g=Combat.NewMeridianPreview(order);g.Weapon=weapon;var path=Path.Combine(Path.GetTempPath(),"meridian-"+Guid.NewGuid()+".json");
            void Save(){SaveStore.Write(path,g);g=SaveStore.Read(path);g.ValidateRooms();}
            void Use(Vector2 p){g.Player=p;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            void Walk(string id,bool back=false)
            {
                var l=RoomLinks.All.Single(l=>l.Id==id);var route=ConnectedWorld.Route(l);if(back)Array.Reverse(route);g.Player=route[0]-g.WorldOrigin;
                foreach(var target in route.Skip(1))
                {int t=0;while(Vector2.Distance(g.Player+g.WorldOrigin,target)>5&&t++<3000){var before=g.Player+g.WorldOrigin;var dir=Vector2.Normalize(target-before);g.Step(new(dir,dir,false,false,false,false,false,false,false,false));check(Vector2.Distance(before,g.Player+g.WorldOrigin)<9,"Meridian passage does not teleport");}check(t<3000,$"Meridian passage {id}/{back}, at {g.Player+g.WorldOrigin}, target {target}");}
                check(g.Rooms!.Current==(back?l.A:l.B),"Meridian passage arrives");Save();
            }
            try
            {
                g.Health=81;g.Rooms!.Rooms.Remove(Cabin.Room);g.Rooms.Rooms.Remove(Meridian.Clock);g.Rooms.Rooms.Remove(Meridian.Hall);g.Rooms.Doors.Remove("uppsala-clock");g.Rooms.Doors.Remove("meridian-hall");foreach(var fresh in Observatory.Ids.Concat(Gamla.Ids))g.Rooms.Rooms.Remove(fresh);foreach(var fresh in RoomLinks.All.Where(l=>l.Id.StartsWith("observatory-")||(l.Id.StartsWith("gamla-")||(l.Id.StartsWith("salt-")||l.Id.StartsWith("rescue-")))))g.Rooms.Doors.Remove(fresh.Id);g.Rooms.LayoutVersion=9;Save();
                check(g.Rooms!.LayoutVersion==16&&g.Rooms.Rooms.Count==41&&g.Health==81&&g.UppsalaState.KeyTaken,"Published Uppsala save gains two unvisited rooms without losing progress");
                var link=RoomLinks.All.Single(l=>l.Id=="uppsala-clock");var route=ConnectedWorld.Route(link);
                check(!g.ClearPath(route[0]-g.WorldOrigin,route[1]-g.WorldOrigin),"Court gate blocks before using date");
                g.Inventory.Drops.Clear();Use(Meridian.CourtGate);check(g.MeridianState.CourtOpen,"Date opens existing painted gate");Walk("uppsala-clock");
                Use(Meridian.Ledger);Use(Meridian.Bell);check(g.Events.Any(e=>e.Text=="meridian-repeat"),"First bell has repeated radio");
                var inventory=g.Inventory.NextId;var health=g.Health;bool repeated=false,noticed=false;
                for(int i=0;i<1460;i++){g.Step(default);repeated|=g.Events.Any(e=>e.Text=="meridian-repeat");noticed|=g.Events.Any(e=>e.Text=="meridian-noticed");}
                check(g.MeridianState.Cycles>=2&&repeated&&noticed,"Two observed cycles repeat radio then acknowledge it");
                check(g.Inventory.NextId==inventory&&g.Health==health&&g.ActorsInRoom(Meridian.Clock).Count==0,"Clock echo does not heal, spawn combatants or duplicate loot");Save();
                Use(Meridian.Bell);check(g.MeridianState.ClockAnchored,"Plate anchors final stroke");Walk("meridian-hall");
                var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.MeridianWarden);g.Player=new(800,470);for(int i=0;i<90;i++)g.Step(new(default,Vector2.UnitY,true,false,false,false,false,false,false,false));
                check(boss.Health==boss.MaxHealth&&!g.MeridianState.PlateSet,"Dormant encounter waits for plate investigation");
                for(int i=0;i<90;i++)g.Step(default);Use(Meridian.Plate);check(g.MeridianState.PlateSet,"Plate begins playable trial");
                int ticks=0;while(!g.Dead&&!boss.Dead&&ticks++<24000)g.Step(MeridianPilot.Decide(g));
                check(!g.Dead&&boss.Dead&&boss.Retired&&!g.DeveloperSurvival,$"Meridian {weapon}/{order}, hp {g.Health}, boss {boss.Health}, ticks {ticks}, breaks {g.MeridianState.Breaks}, pos {g.Player}");
                check(g.MeridianState.WardenDefeated&&g.MeridianState.SecondPhase&&g.MeridianState.Breaks>=3,"Instrument breaks and second phase lead to nonlethal defeat");
                for(int i=0;i<180;i++)g.Step(default);Use(Meridian.Order);check(g.MeridianState.OrderTaken,"Order creates persistent chapter endpoint");
                int drops=g.Inventory.NextId;g.Inventory.Drops.Clear();Use(Meridian.Order);check(g.Inventory.NextId==drops,"Order reward cannot duplicate");Save();
                Walk("meridian-hall",true);Walk("uppsala-clock",true);g.Player=Uppsala.Ramp;check(g.FinishShipTravel(Regiment.Quay),"Return to Ebba remains available");Save();
                Console.WriteLine($"MERIDIAN {weapon}/{order}: {ticks} ticks, {g.Health} hp, {g.MeridianState.Breaks} instrument breaks, both passages and return");
            }
            finally{File.Delete(path);File.Delete(path+".bak");}
        }
    }
}
