using Atland;
using System.Numerics;
using System.Reflection;
public static class PaintedPassageTests
{
    public static void Run(Action<bool,string> check)
    {
        int exercised=0;
        foreach(var link in RoomLinks.All.Where(l=>!(l.Id.StartsWith("gamla-")||l.Id.StartsWith("salt-"))))foreach(bool back in new[]{false,true})
        {
            var g=Combat.NewGamlaPreview(Order.Artillery);if(!RoomLinks.Open(g.Rooms!,link))continue;
            string from=back?link.B:link.A;typeof(Combat).GetMethod("EnterConnectedRoom",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(g,new object[]{from});
            foreach(var e in g.Enemies)e.Health=0;
            var door=g.Rooms!.Doors[link.Id];door.Locked=false;door.TargetOpen=true;door.Openness=1;
            var path=ConnectedWorld.Route(link);if(back)Array.Reverse(path);g.Player=path[0]-g.WorldOrigin;
            int ticks=0;while(g.Passage==null&&ticks++<400){var dir=Combat.Normal(path[1]-g.WorldOrigin-g.Player,Vector2.UnitY);g.Step(new(dir,dir,false,false,false,false,false,false,false,false));}
            check(g.Passage!=null,$"Legacy painted doorway {link.Id}/{back} starts, at {g.Player}, target {path[1]-g.WorldOrigin}");
            float hp=g.Health;int during=0;while(g.Passage!=null&&during++<140)g.Step(new(default,Vector2.UnitX,true,true,true,false,false,false,true,true));
            check(during<140&&g.Rooms!.Current==(back?link.A:link.B)&&g.PassageScale==1&&g.Health==hp,$"Legacy passage {link.Id}/{back} arrives safely and restores scale");
            check(g.OnWalkable(g.Player),"Exit lands on reachable floor "+link.Id);exercised++;
        }
        check(exercised>=30,"Both sides of at least fifteen existing passages exercised");Console.WriteLine($"PAINTED PASSAGES: {exercised} existing doorway directions");
    }
}
