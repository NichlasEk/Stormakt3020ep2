using Atland;
using System.Numerics;
using System.Reflection;
public static class RescueTests
{
    public static void Run(Action<bool,string> check)
    {
        foreach(var weapon in new[]{Weapon.Saber,Weapon.Hammer})
        {
            var g=Combat.NewRescuePreview(Order.Artillery);g.Weapon=weapon;g.Health=73;g.Potions=2;g.Inventory.Stash.Add(g.Inventory.Create("crown-helm"));
            int karlSaber=g.Inventory.Equipped[GearSlot.Saber].Id,karlStash=g.Inventory.Stash[0].Id;
            var file=Path.Combine(Path.GetTempPath(),"ebba-"+Guid.NewGuid()+".json");
            var enter=typeof(Combat).GetMethod("EnterConnectedRoom",BindingFlags.NonPublic|BindingFlags.Instance)!;
            void Save(){SaveStore.Write(file,g);g=SaveStore.Read(file);g.ValidateRooms();}
            void Enter(string id){enter.Invoke(g,new object[]{id});}
            void Use(Vector2 p){g.Player=p;g.Step(default);g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));}
            void Cross(string id,bool reverse=false)
            {
                var l=RoomLinks.All.Single(l=>l.Id==id);var path=ConnectedWorld.Route(l);if(reverse)Array.Reverse(path);
                g.Player=reverse?l.ArrivalB:l.ArrivalA;for(int i=0;i<90;i++)g.Step(default);g.Player=path[0]-g.WorldOrigin;int ticks=0;
                while(g.Passage==null&&ticks++<220){var d=Combat.Normal(path[1]-g.WorldOrigin-g.Player,Vector2.UnitX);g.Step(new(d,d,false,false,false,false,false,false,false,false));}
                check(g.Passage!=null,"Rescue portal entry "+id+" "+g.Player);Save();while(g.Passage!=null)g.Step(default);check(g.Rooms!.Current==(reverse?l.A:l.B),"Rescue portal exit "+id);Save();
            }
            void Fight()
            {
                int ticks=0;while(!g.Dead&&g.EncounterEnemies.Any(e=>!e.Dead)&&ticks++<24000)
                {
                    var boss=g.EncounterEnemies.FirstOrDefault(e=>e.Kind==EnemyKind.Censor&&!e.Dead);
                    if(boss!=null){g.Weapon=Weapon.Pistol;var aim=Combat.Normal(boss.Position-g.Player,Vector2.UnitY);bool firing=boss.State!=1||boss.Timer>.3f;g.Step(new(default,aim,firing,false,false,boss.State==1&&boss.Timer<.3f,false,g.Health<50,true,false));}
                    else g.Step(ObservatoryPilot.Decide(g));
                }
                check(!g.Dead&&ticks<24000,"Rescue fight "+g.Rooms!.Current+" hp="+g.Health+" ticks="+ticks);for(int i=0;i<120;i++)g.Step(default);Save();
            }
            try
            {
                foreach(var id in Rescue.Ids)g.Rooms!.Rooms.Remove(id);foreach(var l in RoomLinks.All.Where(l=>l.Id.StartsWith("rescue-")))g.Rooms!.Doors.Remove(l.Id);g.Rooms!.LayoutVersion=15;Save();
                check(g.Rooms.Rooms.Count==41&&g.Health==73&&g.SaltState.Debriefed,"Salt save migrates without healing or resetting");
                Use(Cabin.Talk);check(g.RescueState.Briefed,"Ebba gives rescue prelude");Enter(Salt.Spring);Cross("rescue-entry");Fight();Use(Rescue.Writ);check(g.RescueState.WritRead,"Writ reveals access");Cross("rescue-prison");Fight();g.Health=73;g.Potions=2;
                g.DropItem("memory",new(1000,700));int dropId=g.Inventory.Drops.Last().Item.Id;var dropWorld=g.Inventory.Drops.Last().Position+g.WorldOrigin;
                Use(Rescue.Reader);check(g.Captured&&!g.IsEbba,"Karl trapped alive");Save();check(Vector2.Distance(g.Player,Rescue.Captive)<.01f,"Captured save stays inside holding alcove");var captive=g.Player;g.Step(new(Vector2.One,Vector2.One,true,false,true,false,true,true,true,false));check(g.Player==captive&&g.Health==73,"Captivity cannot be escaped by movement or combat");
                g.Step(new(default,Vector2.UnitY,false,false,false,false,false,false,false,true));check(g.IsEbba&&g.InCabin&&g.HeroName=="EBBA GRIP","Explicit handover to Ebba aboard");Save();
                check(g.Inventory.Equipped.ContainsKey(GearSlot.Pistol)&&!g.Inventory.Equipped.ContainsKey(GearSlot.Hammer)&&g.RescueState.Karl!.Health==73&&g.RescueState.Karl.Weapon==weapon,"Separate equipment and Karl health");
                check(Vector2.Distance(g.Inventory.Drops.Single(d=>d.Item.Id==dropId).Position+g.WorldOrigin,dropWorld)<.1f,"Loot survives hero and coordinate-frame switch");
                g.Player=Cabin.Entry;check(!g.LeaveCabin(),"Must collect field gear");Use(Cabin.Rest);check(g.RescueState.Ready,"Field gear collected");g.Health=84;Save();check(g.IsEbba&&g.Health==84&&g.RescueState.Karl!.Health==73,"Mid-rescue save retains both heroes");
                g.Health=0;Save();check(g.IsEbba&&g.Dead,"Ebba death save never revives Karl or swaps gear");g.Health=84;Save();
                Enter(Rescue.Hall);check(!RoomLinks.Open(g.Rooms!,RoomLinks.All.Single(l=>l.Id=="rescue-prison")),"Karl route stays locked");Cross("rescue-service");Fight();Use(Rescue.Lever);check(g.RescueState.ServiceOpen,"Mechanical service lock opens");Cross("rescue-machine");
                var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.Censor);g.Player=new(450,770);boss.Position=new(450,470);boss.Cooldown=10;g.Weapon=Weapon.Pistol;float wallHealth=boss.Health;g.Step(new(default,-Vector2.UnitY,true,false,false,false,false,false,false,false));for(int i=0;i<110;i++)g.Step(default);check(boss.Health==wallHealth,"Pistol cannot shoot through cover column");boss.Position=Rescue.Boss;g.Player=new(850,850);g.Weapon=Weapon.Pistol;boss.State=1;boss.Timer=1.4f;boss.LockedAim=g.Player;float before=boss.Health;
                g.Step(new(default,-Vector2.UnitY,true,false,false,false,false,false,false,false));for(int i=0;i<15;i++)g.Step(default);
                check(boss.Health<before&&boss.State==2&&g.RescueState.PistolReload>0,"Pistol interrupts aimed volley");float after=boss.Health;for(int i=0;i<20;i++)g.Step(new(default,-Vector2.UnitY,true,false,false,false,false,false,false,false));check(boss.Health==after,"Pistol respects reload");
                Fight();check(g.RescueState.Defeated&&g.RescueState.SecondPhase&&!g.RescueState.Released,"Boss has second phase and release remains deliberate");Use(Rescue.Release);check(g.RescueState.Released,"Guaranteed boss sigil opens retention");Cross("rescue-release");
                Use(Rescue.Talk);check(g.IsEbba&&g.RescueState.Conversation==1,"Reunion first conversation leaves Ebba playable");Save();Use(Rescue.Talk);check(!g.IsEbba&&g.RescueState.Reunited&&g.Events.Any(e=>e.Kind=="rescue-reunion"),"Second conversation reunites and cues film");
                check(g.Health==73&&g.Weapon==weapon&&g.Potions==2&&g.Inventory.Equipped[GearSlot.Saber].Id==karlSaber&&g.Inventory.Stash[0].Id==karlStash,"Karl restores exact saved equipment health and stash");Save();check(g.RescueState.Ebba!=null&&g.RescueState.Karl==null,"Ebba equipment retained without shared inventory aliases");
                Cross("rescue-prison",true);Cross("rescue-entry",true);Enter(Cabin.Room);Use(Cabin.Talk);check(g.RescueState.Debriefed,"Together aboard aftermath");Save();Console.WriteLine("RESCUE "+weapon+": captured, switched, fought, reunited and returned");
            }
            finally{File.Delete(file);File.Delete(file+".bak");}
        }
    }
}
