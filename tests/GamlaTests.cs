using Atland;
using System.Numerics;
public static class GamlaTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var order in new[]{Order.Artillery,Order.Medicine})
        {
            var g=Combat.NewGamlaPreview(order);var file=Path.Combine(Path.GetTempPath(),"gamla-"+Guid.NewGuid()+".json");
            void Save(){SaveStore.Write(file,g);g=SaveStore.Read(file);g.ValidateRooms();}
            void Use(Vector2 at){g.Player=at;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            void Fight(){int ticks=0;while(!g.Dead&&g.Enemies.Any(e=>!e.Dead&&e.HomeRoom==g.Rooms!.Current)&&ticks++<16000)g.Step(ObservatoryPilot.Decide(g));check(!g.Dead&&ticks<16000,$"Gamla battle {g.Rooms!.Current}: {g.Health}hp, {ticks}ticks, at {g.Player}");for(int i=0;i<180;i++)g.Step(default);g.Inventory.Drops.Clear();}
            void Cross(string id,bool back=false)
            {
                var l=RoomLinks.All.Single(l=>l.Id==id);var route=ConnectedWorld.Route(l);if(back)Array.Reverse(route);
                g.Player=(back?l.ArrivalB:l.ArrivalA);for(int i=0;i<90;i++)g.Step(default);g.Player=route[0]-g.WorldOrigin;
                int ticks=0;while(g.Passage==null&&ticks++<200){var dir=Vector2.Normalize(route[1]-g.WorldOrigin-g.Player);g.Step(new(dir,dir,false,false,false,false,false,false,false,false));}
                check(g.Passage!=null,"Walk into painted portal starts transfer "+id+"/"+back);
                for(int i=0;i<24;i++)g.Step(default);check(g.PassageScale<.8f,"Karl recedes into doorway");
                var before=g.Player;float time=g.Passage!.Time;Save();check(g.Player==before&&g.Passage!.Time==time,"Mid-passage save retains position and animation");
                bool covered=false;while(g.Passage!=null){covered|=g.PassageCurtain>.95f;g.Step(default);}
                check(covered&&g.Rooms!.Current==(back?l.A:l.B)&&g.PassageScale==1,"Covered camera cut exits at human scale");Save();
            }
            try
            {
                foreach(var id in Gamla.Ids)g.Rooms!.Rooms.Remove(id);foreach(var l in RoomLinks.All.Where(l=>(l.Id.StartsWith("gamla-")||(l.Id.StartsWith("salt-")||l.Id.StartsWith("rescue-")))))g.Rooms!.Doors.Remove(l.Id);g.Rooms!.LayoutVersion=12;Save();
                check(g.Rooms!.LayoutVersion==16&&g.Rooms.Rooms.Count==41&&g.ObservatoryState.Debriefed,"Observatory saves gain Gamla Uppsala");
                Use(Cabin.Helm);check(g.Events.Any(e=>e.Kind=="gamla-travel"&&e.Text==Gamla.Landing),"Cabin helm offers new expedition");check(g.FinishGamlaFlight(Gamla.Landing),"Frigate lands at royal mounds");Fight();Use(Gamla.Camp);check(g.GamlaState.CampRead,"Camp distinguishes people from graves");Cross("gamla-mound");Fight();
                var gate=RoomLinks.All.Single(l=>l.Id=="gamla-registry");g.Player=gate.AtA;for(int i=0;i<110;i++)g.Step(new(-Vector2.UnitY,-Vector2.UnitY,false,false,false,false,false,false,false,false));check(g.Passage==null&&g.Rooms!.Current==Gamla.Passage,"Closed door prevents transition");
                Use(Gamla.Ledger);check(g.GamlaState.LedgerRead,"Receipt opens inner lock");Cross("gamla-registry");Fight();Use(Gamla.Talk);check(g.GamlaState.Conversation==1&&!g.GamlaState.WitnessMet,"Listen before asking about Elin");Use(Gamla.Talk);check(g.GamlaState.WitnessMet,"Nils witnessed Elin alive");
                int drops=g.Inventory.NextId;g.Inventory.Drops.Clear();Use(Gamla.Talk);check(g.Inventory.NextId==drops,"Receipt reward cannot duplicate");Save();Cross("gamla-registry",true);Cross("gamla-mound",true);
                Use(Gamla.Board);check(g.InCabin&&g.CabinState.ReturnRoom==Gamla.Landing,"Return aboard at new landing");Use(Cabin.Talk);check(g.GamlaState.Debriefed,"Ebba receives witness statement");Use(Cabin.Entry);check(g.Rooms!.Current==Gamla.Landing,"Cabin hatch returns to correct destination");Use(Gamla.Board);Use(Cabin.Helm);check(g.Events.Any(e=>e.Kind=="gamla-travel"&&e.Text==Uppsala.Court),"Can fly back to Instrument Court");check(g.FinishGamlaFlight(Uppsala.Court),"Return flight works");Save();Console.WriteLine($"GAMLA {order}: full round trip, {g.Health}hp, painted passages and saved debrief");
            }
            finally{File.Delete(file);File.Delete(file+".bak");}
        }
    }
}
