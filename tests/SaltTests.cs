using Atland;
using System.Numerics;
public static class SaltTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var order in new[]{Order.Artillery,Order.Medicine})foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})
        {
            var g=Combat.NewSaltPreview(order);g.Weapon=weapon;var file=Path.Combine(Path.GetTempPath(),"salt-"+Guid.NewGuid()+".json");
            var enter=typeof(Combat).GetMethod("EnterConnectedRoom",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            void Save(){SaveStore.Write(file,g);g=SaveStore.Read(file);g.ValidateRooms();}
            void Use(Vector2 p){g.Player=p;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            void Cross(string id,bool back=false)
            {
                var l=RoomLinks.All.Single(l=>l.Id==id);var path=ConnectedWorld.Route(l);if(back)Array.Reverse(path);g.Player=back?l.ArrivalB:l.ArrivalA;for(int i=0;i<90;i++)g.Step(default);g.Player=path[0]-g.WorldOrigin;int ticks=0;
                while(g.Passage==null&&ticks++<220){var d=Vector2.Normalize(path[1]-g.WorldOrigin-g.Player);g.Step(new(d,d,false,false,false,false,false,false,false,false));}
                check(g.Passage!=null,"Salt portal entry "+id+" "+g.Player);Save();while(g.Passage!=null)g.Step(default);
                check(g.Rooms!.Current==(back?l.A:l.B),"Salt portal destination "+id);Save();
            }
            void Fight()
            {
                int ticks=0;while(!g.Dead&&g.EncounterEnemies.Any(e=>!e.Dead)&&ticks++<24000)g.Step(SaltPilot.Decide(g));
                check(!g.Dead&&ticks<24000,$"Salt fight {g.Rooms!.Current}/{order}/{weapon}: ticks={ticks} hp={g.Health} at={g.Player}");
                for(int i=0;i<120;i++)g.Step(default);g.Inventory.Drops.Clear();Save();
            }
            try
            {
                foreach(var id in Salt.Ids.Concat(Rescue.Ids))g.Rooms!.Rooms.Remove(id);foreach(var l in RoomLinks.All.Where(l=>(l.Id.StartsWith("salt-")||l.Id.StartsWith("rescue-"))))g.Rooms!.Doors.Remove(l.Id);g.Rooms!.LayoutVersion=14;g.Health=87;Save();
                check(g.Rooms.LayoutVersion==16&&g.Rooms.Rooms.Count==41&&g.WestState.Debriefed&&g.Health==87,"Published west save migrates without resetting story or health");
                check(!RoomLinks.Open(g.Rooms,RoomLinks.All.Single(l=>l.Id=="salt-entry")),"Salt entry requires reunion");Use(Cabin.Talk);check(g.SaltState.Reunited&&g.Events.Any(e=>e.Text=="salt-reunion-marta"),"Marta and Elin reunite aboard");Save();
                enter.Invoke(g,new object[]{West.Refuge});Cross("salt-entry");Fight();Use(Salt.Manifest);check(g.SaltState.ManifestRead,"Read cargo manifest");Cross("salt-stairs");Fight();Use(Salt.Wheel);check(g.SaltState.Drained,"Drain staircase");Cross("salt-register");Fight();Use(Salt.Cache);int potions=g.Potions;Use(Salt.Cache);check(g.Potions==potions,"Cache cannot duplicate medicine");Use(Salt.Ledger);check(g.SaltState.LedgerRead,"Read actual evidence");Cross("salt-spring");
                var keeper=g.Enemies.Single(e=>e.Kind==EnemyKind.SaltWarden);keeper.Cooldown=4;
                g.Player=new(350,700);g.SaltState.Tide=8;g.SaltState.Chains=0;float beforeWater=g.Health;g.Step(default);check(g.Health<beforeWater,"Flooded floor damages Karl");
                g.Player=new(790,750);for(int i=0;i<45;i++)g.Step(default);float dryHealth=g.Health;g.Step(default);check(g.Health==dryHealth,"Dry central aisle is safe from water");
                g.Health=beforeWater;Use(Salt.Chains[0]);Use(Salt.Chains[1]);check(g.SaltState.Chains==3,$"Chain winches drain both sides: {g.SaltState.Chains}, player={g.Player}, attack={g.AttackTime}, dodge={g.DodgeTime}, dead={g.Dead}, events={string.Join(",",g.Events.Select(e=>e.Text))}");
                Fight();check(g.SaltState.Defeated&&g.SaltState.SecondPhase&&!g.SaltState.Released,"Two phase victory still requires release action");Use(Salt.Release);check(g.SaltState.Released&&g.SaltState.ShortcutOpen,"Release witnesses and open return passage");int items=g.Inventory.NextId;Use(Salt.Release);check(g.Inventory.NextId==items,"Release reward cannot duplicate");Save();Cross("salt-return");check(g.Rooms!.Current==Salt.Loading,"Shortcut leads to loading station");Cross("salt-entry",true);
                Cross("gamla-west-exit",true);Cross("gamla-west-scale",true);Cross("gamla-west-entry",true);Cross("gamla-registry",true);Cross("gamla-mound",true);Use(Gamla.Board);check(g.InCabin,"Return aboard from Saltkalla");Use(Cabin.Talk);check(g.SaltState.Debriefed&&g.Events.Any(e=>e.Text=="salt-debrief"),"Rescue has an aftermath aboard");Save();Console.WriteLine($"SALT {order}/{weapon}: complete rescue and return, {g.Health}hp");
            }finally{File.Delete(file);File.Delete(file+".bak");}
        }
    }
}
