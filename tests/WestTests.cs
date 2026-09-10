using Atland;
using System.Numerics;
public static class WestTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var order in new[]{Order.Artillery,Order.Medicine})foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})
        {
            var g=Combat.NewWestPreview(order);g.Weapon=weapon;var file=Path.Combine(Path.GetTempPath(),"west-"+Guid.NewGuid()+".json");
            void Save(){SaveStore.Write(file,g);g=SaveStore.Read(file);g.ValidateRooms();}
            void Use(Vector2 p){g.Player=p;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            void Cross(string id,bool back=false)
            {
                var l=RoomLinks.All.Single(l=>l.Id==id);var path=ConnectedWorld.Route(l);if(back)Array.Reverse(path);g.Player=back?l.ArrivalB:l.ArrivalA;for(int i=0;i<90;i++)g.Step(default);g.Player=path[0]-g.WorldOrigin;int ticks=0;
                while(g.Passage==null&&ticks++<200){var d=Vector2.Normalize(path[1]-g.WorldOrigin-g.Player);g.Step(new(d,d,false,false,false,false,false,false,false,false));}
                check(g.Passage!=null,"Portal entry "+id);while(g.Passage!=null){g.Step(default);if(g.Passage is {Arrived:true,Time:>1.1f and <1.12f})Save();}
                check(g.Rooms!.Current==(back?l.A:l.B),"Portal destination "+id);Save();
            }
            void Fight()
            {
                int ticks=0;while(!g.Dead&&g.Enemies.Any(e=>!e.Dead&&e.HomeRoom==g.Rooms!.Current)&&ticks++<24000)g.Step(WestPilot.Decide(g));
                check(!g.Dead&&ticks<24000,$"West fight {g.Rooms!.Current}/{order}/{weapon} ticks={ticks} hp={g.Health} player={g.Player}, brakes={g.WestState.Brakes}, exposed={g.WestState.Exposed}");
                for(int i=0;i<120;i++)g.Step(default);g.Inventory.Drops.Clear();Save();
            }
            try
            {
                foreach(var id in West.Ids.Concat(Salt.Ids).Concat(Rescue.Ids))g.Rooms!.Rooms.Remove(id);foreach(var l in RoomLinks.All.Where(l=>(l.Id.StartsWith("gamla-west-")||(l.Id.StartsWith("salt-")||l.Id.StartsWith("rescue-")))))g.Rooms!.Doors.Remove(l.Id);g.Rooms!.LayoutVersion=13;Save();check(g.Rooms.LayoutVersion==16&&g.Rooms.Rooms.Count==41&&g.GamlaState.Debriefed,"Published Gamla save migrated");
                Cross("gamla-west-entry");Fight();var gate=RoomLinks.All.Single(l=>l.Id=="gamla-west-scale");check(!RoomLinks.Open(g.Rooms!,gate),"Scale locked before instructions");Use(West.Register);check(g.WestState.RegisterRead,"Instructions read");Cross("gamla-west-scale");
                var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.MusterOfficer);float hp=boss.Health;g.Player=boss.Position+new Vector2(0,65);for(int i=0;i<55;i++)g.Step(new(default,-Vector2.UnitY,true,false,false,false,false,false,false,false));check(boss.Health==hp,"Boss protected while weights loaded");
                Fight();check(g.WestState.Defeated&&g.WestState.Reinforced,"Defeat and reinforcement phase persisted");Cross("gamla-west-exit");Use(West.Record);Use(West.Talk);check(!g.WestState.ElinMet,"Listen before leaving");Use(West.Talk);check(g.WestState.ElinMet,"Elin rescued");int loot=g.Inventory.NextId;Use(West.Talk);check(g.Inventory.NextId==loot,"No duplicate rescue reward");Save();
                Cross("gamla-west-exit",true);Cross("gamla-west-scale",true);Cross("gamla-west-entry",true);Cross("gamla-registry",true);Cross("gamla-mound",true);Use(Gamla.Board);check(g.InCabin,"Back aboard");Use(Cabin.Talk);check(g.WestState.Debriefed&&g.Events.Any(e=>e.Text=="west-aboard"),"Elin speaks aboard with Ebba");Use(West.Aboard);check(g.Events.Any(e=>e.Text=="west-aboard"),"Elin can be addressed aboard");Save();Console.WriteLine($"WEST {order}/{weapon}: rescued Elin, returned aboard, {g.Health}hp");
            }finally{File.Delete(file);File.Delete(file+".bak");}
        }
    }
}
