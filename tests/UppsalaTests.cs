using Atland;
using System.Numerics;
public static class UppsalaTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})foreach(var order in new[]{Order.Artillery,Order.Medicine})
        {
            var g=Combat.NewUppsalaPreview(order);g.Weapon=weapon;
            var path=Path.Combine(Path.GetTempPath(),"uppsala-"+Guid.NewGuid()+".json");
            void Save(){SaveStore.Write(path,g);g=SaveStore.Read(path);g.ValidateRooms();}
            void Walk(Vector2 at)
            {int ticks=0;while(Vector2.Distance(g.Player,at)>5&&ticks++<3000){var direction=Combat.Normal(g.NextWaypoint(g.Player,at)-g.Player,Vector2.UnitX);g.Step(new(direction,direction,false,false,false,false,false,false,false,false));}check(ticks<3000,"Walkable instrument route "+at);}
            void Use(Vector2 at){g.Player=at;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            try
            {
                g.Rooms!.Rooms.Remove(Uppsala.Court);g.Rooms.LayoutVersion=8;g.Health=73;Save();check(g.Rooms!.LayoutVersion==9&&g.Rooms.Rooms.Count==20&&g.Health==73,"Published foundry save gains Uppsala without healing");
                g.FoundryState.PlateTaken=false;check(!g.FinishShipTravel(Uppsala.Court),"Plate required for flight");g.FoundryState.PlateTaken=true;
                g.Player=Regiment.Boat;check(!g.FinishShipTravel(Uppsala.Court),"Cannot board from the wrong point");g.Player=Uppsala.Board;
                check(g.RoomRadioRelevant("uppsala-arrival"),"Arrival radio survives region preparation");
                check(g.FinishShipTravel(Uppsala.Court)&&g.InUppsala&&g.Health==73,"Ship arrival preserves health");Save();
                foreach(var p in Uppsala.Rings.Append(Uppsala.Desk).Append(Uppsala.Seal).Append(Uppsala.Ramp))check(g.OnWalkable(p),"Uppsala interaction lies on floor "+p);
                Use(Uppsala.Rings[0]);check(!g.UppsalaState.Aligned&&g.UppsalaState.Rings[0]==0,"Clue required before manipulating instrument");
                foreach(var at in Uppsala.Rings.Append(Uppsala.Desk).Append(Uppsala.Seal).Append(Uppsala.Ramp))Walk(at);
                Use(Uppsala.Desk);check(g.UppsalaState.ClueRead,"Astronomer clue read");
                for(int i=0;i<3;i++)for(int turns=0;g.UppsalaState.Rings[i]!=Uppsala.Target[i]&&turns<4;turns++){Use(Uppsala.Rings[i]);Save();}
                check(g.UppsalaState.Aligned&&g.EncounterEnemies.Count(e=>!e.Dead)==3,"Alignment summons one ambush");
                g.Player=Uppsala.Ramp;check(!g.FinishShipTravel(Regiment.Quay),"Cannot flee active ambush by boarding");
                int ticks=0;while(!g.Dead&&g.EncounterEnemies.Any(e=>!e.Dead)&&ticks++<15000)g.Step(OathCheckPilot.Decide(g));g.Step(default);
                check(!g.Dead&&g.UppsalaState.Secured&&!g.DeveloperSurvival,$"Uppsala combat {weapon}/{order}: hp {g.Health}, ticks {ticks}");
                while(g.AttackTime>0||g.Hurt>0||g.DodgeTime>0||g.Shots.Count>0||g.Hazards.Count>0)g.Step(default);
                Use(Uppsala.Seal);check(g.UppsalaState.KeyTaken,"Meridian date secured");int count=g.Inventory.NextId;Use(Uppsala.Seal);check(g.Inventory.NextId==count,"Reward cannot duplicate");Save();
                var hp=g.Health;var explored=g.Rooms!.Rooms[Uppsala.Court].Explored.ToArray();g.Player=Uppsala.Ramp;check(g.FinishShipTravel(Regiment.Quay),"Return flight");Save();check(g.Health==hp,"Return never heals");
                check(g.FinishShipTravel(Uppsala.Court),"Revisit by ship");check(g.EncounterEnemies.Count()==3&&g.EncounterEnemies.All(e=>e.Dead)&&g.UppsalaState.KeyTaken,"No respawn or reset on return");
                check(explored.Zip(g.Rooms!.Rooms[Uppsala.Court].Explored).All(p=>(p.First&p.Second)==p.First),"Exploration preserved");Save();
                Console.WriteLine($"UPPSALA {weapon}/{order}: {ticks} ticks, {g.Health} hp, flight, rings, battle, return, migration");
            }
            finally{File.Delete(path);File.Delete(path+".bak");}
        }
    }
}
