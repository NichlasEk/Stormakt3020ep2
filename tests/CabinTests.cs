using Atland;
using System.Numerics;
public static class CabinTests
{
    public static void Run(Action<bool,string> check)
    {
        var early=Combat.NewMeridianPreview(Order.Medicine);check(!early.BoardCabin(),"Order is required before meeting Ebba");
        var g=Combat.NewCabinPreview(Order.Medicine);var path=Path.Combine(Path.GetTempPath(),"cabin-"+Guid.NewGuid()+".json");
        void Save(){SaveStore.Write(path,g);g=SaveStore.Read(path);g.ValidateRooms();}
        void Use(Vector2 at){g.Player=at;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
        try
        {
            g.Rooms!.Rooms.Remove(Cabin.Room);foreach(var fresh in Observatory.Ids.Concat(Gamla.Ids))g.Rooms.Rooms.Remove(fresh);foreach(var fresh in RoomLinks.All.Where(l=>l.Id.StartsWith("observatory-")||l.Id.StartsWith("gamla-")))g.Rooms.Doors.Remove(fresh.Id);g.Rooms.LayoutVersion=10;Save();check(g.Rooms!.Rooms.Count==33&&g.Health==21&&g.MeridianState.OrderTaken,"Published Meridian endpoint migrates without resetting wounds or story");
            g.Player=Uppsala.Ramp+new Vector2(200,0);check(!g.BoardCabin(),"No remote boarding");g.Player=Uppsala.Ramp;
            g.Hazards.Add(new(){Position=g.Player,Timer=1,Radius=50});check(!g.BoardCabin(),"Unsafe boarding blocked");g.Hazards.Clear();
            Use(Uppsala.Ramp);check(g.InCabin&&g.Events.Any(e=>e.Text=="cabin-welcome")&&g.Health==21,"Normal ramp interaction enters physical cabin with greeting");Save();
            g.CabinState.EnvironmentVersion=0;g.Player=new(870,490);g.DropItem("helmet",new(1240,565));Save();
            check(g.Player==Cabin.Talk&&g.Health==21,"Old cabin save moves with scaled furniture without healing");check(g.Inventory.Drops.Single().Position==Cabin.Rest,"Dropped gear follows cabin scale");var migrated=g.Player;Save();check(g.Player==migrated&&g.Inventory.Drops.Single().Position==Cabin.Rest,"Cabin scale migration happens once");g.Inventory.Drops.Clear();
            check(g.OnWalkable(Cabin.Entry)&&g.OnWalkable(Cabin.Talk)&&g.OnWalkable(Cabin.Rest)&&g.OnWalkable(Cabin.Helm),"Every cabin interaction has walkable footing");
            foreach(var at in new[]{Cabin.Talk,Cabin.Rest,Cabin.Helm,Cabin.Entry})
            {
                int ticks=0;while(Vector2.Distance(g.Player,at)>5&&ticks++<3000){var dir=Combat.Normal(g.NextWaypoint(g.Player,at)-g.Player,Vector2.UnitX);g.Step(new(dir,dir,false,false,false,false,false,false,false,false));}
                check(ticks<3000,"Can physically walk between cabin interactions "+at);
            }
            Use(Cabin.Rest);check(g.Health==21&&!g.CabinState.Rested,"Debrief before medical rest");
            for(int i=1;i<=3;i++){Use(Cabin.Talk);check(g.CabinState.Conversation==i,"Persistent conversation step "+i);Save();}
            check(g.CabinState.Briefed&&!g.RoomGoal.Contains("återvänd till Ebba"),"Meeting resolves return objective");
            Use(Cabin.Rest);check(g.Health==100&&g.CabinState.Rested,"Ebba dresses wounds once");Save();g.Health=70;Use(Cabin.Rest);check(g.Health==70,"Rest cannot be farmed");
            int drops=g.Inventory.NextId;Use(Cabin.Talk);check(g.Inventory.NextId==drops&&g.Events.Any(e=>e.Text=="cabin-repeat"),"Repeat dialogue has no duplicate rewards");
            g.Step(new(default,Vector2.UnitY,true,true,false,false,false,false,true,false));check(g.AttackTime==0,"Karl keeps weapons away in cabin");
            Use(Cabin.Entry);check(g.Rooms!.Current==Uppsala.Court&&g.Player==Uppsala.Ramp,"Hatch returns to correct mooring");Use(Uppsala.Ramp);check(g.InCabin&&!g.Events.Any(e=>e.Text=="cabin-welcome"),"Return preserves meeting without replaying greeting");
            Use(Cabin.Helm);check(g.Rooms!.Current==Uppsala.Court&&g.Events.Any(e=>e.Kind=="ship-travel"&&e.Text==Regiment.Quay),"Cabin navigation retains ship travel");
            check(g.FinishShipTravel(Regiment.Quay),"Existing flight finishes");Use(Uppsala.Board);check(g.InCabin&&g.CabinState.ReturnRoom==Regiment.Quay,"Cabin can be boarded at original quay");Use(Cabin.Entry);check(g.Rooms!.Current==Regiment.Quay,"Cabin exit remembers quay");Save();
        }
        finally{File.Delete(path);File.Delete(path+".bak");}
    }
}
