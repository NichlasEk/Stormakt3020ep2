using Atland;
using System.Numerics;
public static class ObservatoryTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})foreach(var order in new[]{Order.Artillery,Order.Medicine})
        {
            var g=Combat.NewObservatoryPreview(order);g.Weapon=weapon;var path=Path.Combine(Path.GetTempPath(),"observatory-"+Guid.NewGuid()+".json");
            void Save(){SaveStore.Write(path,g);g=SaveStore.Read(path);g.ValidateRooms();}
            void Use(Vector2 p){g.Player=p;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            void Fight()
            {
                int ticks=0;while(!g.Dead&&g.Enemies.Any(e=>!e.Dead&&e.HomeRoom==g.Rooms!.Current)&&ticks++<20000)g.Step(ObservatoryPilot.Decide(g));
                check(!g.Dead&&ticks<20000,$"Observatory fight {g.Rooms!.Current} {weapon}/{order}: hp {g.Health}, ticks {ticks}, at {g.Player}, enemies {string.Join(';',g.Enemies.Where(e=>!e.Dead&&e.HomeRoom==g.Rooms.Current).Select(e=>$"{e.Kind}:{e.Health}@{e.Position}"))}");
                for(int i=0;i<180;i++)g.Step(default);g.Inventory.Drops.Clear();
            }
            void Walk(string id,bool back=false)
            {
                var l=RoomLinks.All.Single(l=>l.Id==id);var route=ConnectedWorld.Route(l);if(back)Array.Reverse(route);g.Player=route[0]-g.WorldOrigin;
                g.Player=(back?l.ArrivalB:l.ArrivalA);for(int i=0;i<90;i++)g.Step(default);g.Player=route[0]-g.WorldOrigin;
                var d=g.Rooms!.Doors[id];if(!d.TargetOpen&&!d.Broken){Use(ConnectedWorld.Center(l)-g.WorldOrigin);g.Player=route[0]-g.WorldOrigin;}
                // Stand clear of the leaf's sweep while it opens.
                var hold=g.Player;g.Player=(back?l.ArrivalB:l.ArrivalA);for(int i=0;i<90;i++)g.Step(default);g.Player=hold;
                foreach(var target in route.Skip(1))
                {int t=0;while(Vector2.Distance(g.Player+g.WorldOrigin,target)>5&&t++<3000){var before=g.Player+g.WorldOrigin;var dir=Vector2.Normalize(target-before);g.Step(new(dir,dir,false,false,false,false,false,false,false,false));check(Vector2.Distance(before,g.Player+g.WorldOrigin)<9,"Observatory physical passage does not teleport");}check(t<3000,$"Observatory passage {id}/{back}, at {g.Player+g.WorldOrigin}, target {target}, open {d.Openness}");}
                check(g.Rooms!.Current==(back?l.A:l.B),"Observatory passage arrives in correct room");Save();
            }
            try
            {
                g.Rooms!.LayoutVersion=11;foreach(var id in Observatory.Ids.Concat(Gamla.Ids))g.Rooms.Rooms.Remove(id);foreach(var l in RoomLinks.All.Where(l=>l.Id.StartsWith("observatory-")||(l.Id.StartsWith("gamla-")||(l.Id.StartsWith("salt-")||l.Id.StartsWith("rescue-")))))g.Rooms.Doors.Remove(l.Id);Save();check(g.Rooms!.LayoutVersion==16&&g.Rooms.Rooms.Count==41&&g.CabinState.Briefed,"Old cabin save migrates without resetting story");
                Use(Observatory.Gate);check(g.ObservatoryState.EntryOpen,"Ebba briefing unlocks observatory");Walk("observatory-entry");Fight();Use(Observatory.Talk);check(g.ObservatoryState.MartaMet,"Marta survives and gives the personal stakes");
                Walk("observatory-workshop");Fight();
                var workshopDoor=g.Rooms!.Doors["observatory-workshop"];var workshopLink=RoomLinks.All.Single(l=>l.Id=="observatory-workshop");
                workshopDoor.TargetOpen=false;g.Player=new(760,650);for(int i=0;i<100;i++)g.Step(default);
                var center=ConnectedWorld.Center(workshopLink)-g.WorldOrigin;var axis=Vector2.Normalize(ConnectedWorld.Route(workshopLink)[1]-ConnectedWorld.Route(workshopLink)[0]);
                g.Player=center+axis*48;
                int hits=0;while(!workshopDoor.Broken&&hits++<1200)g.Step(new(default,-axis,false,true,false,false,false,false,false,false));
                check(workshopDoor.Broken,"Workshop oak door can be smashed from the far side");
                for(int i=0;i<180;i++)g.Step(default);
                Use(Observatory.Diagram);Use(Observatory.Cabinet);check(g.ObservatoryState.DiagramRead&&g.ObservatoryState.KeyTaken,"Diagram and key acquired");
                g.Inventory.Drops.Clear();int drops=g.Inventory.NextId;Save();check(g.ObservatoryState.CabinetOpened&&g.ObservatoryState.KeyTaken,"Empty cabinet and its key persist after reload");Use(Observatory.Cabinet);check(g.Events.Any(e=>e.Text.Contains("Gömman är tömd")),"Empty cabinet explains key status");check(g.Inventory.NextId==drops,"Cache cannot duplicate loot");Walk("observatory-machine");Fight();
                Use(Observatory.Brakes[0]);Use(Observatory.Brakes[0]);Use(Observatory.Brakes[2]);check(g.ObservatoryState.Aligned,"Diagram solves physical brakes");Use(Observatory.Shortcut);check(g.ObservatoryState.ShortcutOpen,"Workshop key opens maintenance shortcut");
                Walk("observatory-shortcut");Walk("observatory-shortcut",true);Walk("observatory-dome");
                var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.ZenithGuardian);g.Player=Observatory.Boss+new Vector2(0,55);for(int i=0;i<100;i++)g.Step(new(default,-Vector2.UnitY,true,false,false,false,false,false,false,false));check(boss.Health==boss.MaxHealth,"Dormant machine waits for investigation");
                for(int i=0;i<90;i++)g.Step(default);Use(Observatory.Wake);check(g.ObservatoryState.Awake,"Investigation wakes the zenith mechanism");Fight();check(g.ObservatoryState.Defeated&&!g.DeveloperSurvival,"Both weapons can defeat all locks and the mechanism without dev mode");
                Use(Observatory.Original);check(g.ObservatoryState.OriginalTaken,"Original destination unlocked only after victory");Save();
                Walk("observatory-dome",true);Walk("observatory-shortcut");Walk("observatory-entry",true);Walk("meridian-hall",true);Walk("uppsala-clock",true);Use(Uppsala.Ramp);check(g.InCabin,"Physical return reaches Ebba aboard frigate");Use(Cabin.Talk);check(g.ObservatoryState.Debriefed,"Original has a personal aftermath with Ebba");Save();
                Console.WriteLine($"OBSERVATORY {weapon}/{order}: complete, {g.Health} hp, all passages and return");
            }
            finally{File.Delete(path);File.Delete(path+".bak");}
        }
    }
}
