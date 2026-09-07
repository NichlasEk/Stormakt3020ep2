using Atland;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

static class SightTests
{
    public static void Run(Action<bool,string> check)
    {
        var root=Path.Combine(Path.GetTempPath(),"atland-sight-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            var game=Combat.NewRooms(Order.Artillery);game.Enemies.Clear();
            game.Rooms!.Rooms[PortRooms.Court].Enemies=new();game.Rooms.Current=PortRooms.Lodge;
            game.Rooms.Rooms[PortRooms.Lodge].Visited=true;game.Rooms.KeyTaken=game.Rooms.DoorOpen=true;
            game.Player=new(650,580);game.UpdateRoomSight(true);
            var hidden=new Vector2(925,580);var nearby=new Vector2(600,650);
            check(!game.CanSeeRoomPoint(hidden),"Sight stops at the painted crate island");
            check(game.CanSeeRoomPoint(nearby),"Unobstructed nearby floor is visible");
            check(!game.CanSeeRoomPoint(new(1190,670)),"Distant ground is not visible");
            check(!game.ExploredRoomPoint(hidden),"Obstacle does not reveal hidden cells");
            check(game.ExploredRoomPoint(nearby),"Visible ground enters exploration memory");
            game.Spawn(EnemyKind.Gunner,hidden);var enemy=game.Enemies.Single();var position=enemy.Position;
            for(int i=0;i<90;i++)game.Step(default);
            check(!enemy.Alerted&&enemy.Position==position&&!game.Shots.Any(),"Unseen guard does not detect Karl through cover");
            game.Player=new(970,630);game.UpdateRoomSight(true);game.Step(default);
            check(enemy.Alerted&&game.CanSeeRoomPoint(enemy.Position),"Guard detects Karl once line of sight opens");
            game.Enemies.Clear();game.DropItem("memory",hidden);
            check(game.ExploredRoomPoint(hidden),"Walking around cover discovers the other side");
            game.Player=new(520,700);game.UpdateRoomSight(true);
            check(!game.CanSeeRoomPoint(hidden)&&game.ExploredRoomPoint(hidden),"Explored memory survives leaving current sight");
            var path=Path.Combine(root,"sight.json");var before=game.Rooms.Rooms[PortRooms.Lodge].Explored.ToArray();
            SaveStore.Write(path,game);var restored=SaveStore.Read(path);
            check(before.SequenceEqual(restored.Rooms!.Rooms[PortRooms.Lodge].Explored),"Exploration bits survive real save/load");
            check(!restored.CanSeeRoomPoint(hidden)&&restored.LocalDrops.Single().Position==hidden,"Saving does not make remembered loot currently visible");
            var courtBefore=restored.Rooms.Rooms[PortRooms.Court].Explored.ToArray();restored.UpdateRoomSight(true);
            check(courtBefore.SequenceEqual(restored.Rooms.Rooms[PortRooms.Court].Explored),"Looking in lodge cannot reveal court cells");
            var fresh=Combat.NewRooms(Order.Medicine);fresh.Rooms!.KeyTaken=fresh.Rooms.DoorOpen=true;
            fresh.Player=PortRooms.CourtDoor;fresh.UpdateRoomSight(true);
            check(fresh.Rooms.Rooms[PortRooms.Lodge].Explored.All(b=>b==0),"Opening door does not reveal a separate unvisited room");
            var options=new JsonSerializerOptions{IncludeFields=true};var node=JsonSerializer.SerializeToNode(fresh,options)!;
            foreach(var snapshot in node["Rooms"]!["Rooms"]!.AsObject())snapshot.Value!.AsObject().Remove("Explored");
            var migrated=node.Deserialize<Combat>(options)!;migrated.ValidateRooms();migrated.UpdateRoomSight(true);
            check(migrated.Rooms!.Rooms[PortRooms.Court].Explored.Any(b=>b!=0)&&migrated.Rooms.Rooms[PortRooms.Lodge].Explored.All(b=>b==0),"Older room save gains only present sight, not a revealed map");
            migrated.Rooms.Rooms[PortRooms.Court].Explored=new byte[1];
            try{migrated.ValidateRooms();check(false,"Reject malformed exploration");}catch(InvalidDataException){check(true,"Reject malformed exploration");}
        }
        finally{Directory.Delete(root,true);}
    }
}
