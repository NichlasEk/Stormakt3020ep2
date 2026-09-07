using Godot;
using Atland;
using System;
using System.Linq;

public partial class Main
{
    private ImageTexture? _roomFog;
    private Combat? _fogGame;
    private int _fogRevision=-1;
    private void DrawRoomFog()
    {
        if(_fogGame!=_game||_fogRevision!=_game.SightRevision)
        {
            var data=new byte[RoomSight.Count*4];var seen=_game.Rooms!.Rooms[_game.Rooms.Current].Explored;
            var feet=_game.Enemies.Where(e=>_game.CanSeeRoomPoint(e.Position)).Select(e=>e.Position).Append(_game.Player).ToArray();
            var props=_game.Rooms.Current==PortRooms.Pump
                ?new[]{(Access:PortRooms.Wheel,Foot:new System.Numerics.Vector2(300,565)),(Access:PortRooms.Pressure,Foot:new System.Numerics.Vector2(1250,595))}
                :_game.Rooms.Current==PortRooms.Cistern?new[]{(Access:PortRooms.Relic,Foot:new System.Numerics.Vector2(230,675))}:_game.Rooms.Current==PortRooms.Archive?new[]{(Access:ArchiveRoom.Desk,Foot:new System.Numerics.Vector2(800,680))}
                :_game.Rooms.Current==PortRooms.Gallery?new[]{(Access:PortRooms.Witness,Foot:new System.Numerics.Vector2(800,700))}
                :_game.Rooms.Current==PortRooms.Chamber?PortRooms.Pillars.Select(p=>(Access:p+new System.Numerics.Vector2(0,42),Foot:p)).ToArray():Array.Empty<(System.Numerics.Vector2 Access,System.Numerics.Vector2 Foot)>();
            for(int y=0;y<RoomSight.Rows;y++)for(int x=0;x<RoomSight.Columns;x++)
            {
                // Ground visibility projects upwards over painted actors/walls, avoiding cut-off heads.
                // Actor/loot eligibility still uses exact ground-level sight, never this visual dilation.
                byte alpha=255;
                for(int rise=0;rise<=5&&y+rise<RoomSight.Rows;rise++)
                {
                    int sample=(y+rise)*RoomSight.Columns+x;
                    if(_game.RoomVisible[sample]){alpha=0;break;}
                    if(RoomSight.Seen(seen,sample))alpha=158;
                }
                // A valid footpoint can lie on a cell whose center falls beyond a wall.
                // Preserve complete visible silhouettes even at those sub-cell boundaries.
                var pixel=RoomSight.Center(y*RoomSight.Columns+x);
                if(feet.Any(p=>Math.Abs(pixel.X-p.X)<64&&pixel.Y>=p.Y-190&&pixel.Y<=p.Y+40))alpha=0;
                // Controls sit outside the walk polygon; reveal their painted bodies from the accessible interaction point.
                foreach(var prop in props)
                    if(Math.Abs(pixel.X-prop.Foot.X)<96&&pixel.Y>=prop.Foot.Y-190&&pixel.Y<=prop.Foot.Y+30)
                    {
                        if(_game.CanSeeRoomPoint(prop.Access))alpha=0;
                        else if(_game.ExploredRoomPoint(prop.Access))alpha=Math.Min(alpha,(byte)158);
                    }
                int at=(y*RoomSight.Columns+x)*4;
                data[at]=3;data[at+1]=5;data[at+2]=6;data[at+3]=alpha;
            }
            using var image=Image.CreateFromData(RoomSight.Columns,RoomSight.Rows,false,Image.Format.Rgba8,data);
            if(_roomFog is null)_roomFog=ImageTexture.CreateFromImage(image);else _roomFog.Update(image);
            _fogGame=_game;_fogRevision=_game.SightRevision;
        }
        DrawTextureRect(_roomFog,new Rect2(0,0,1536,1024),false);
    }
    private void DrawExplorationMap()
    {
        var origin=new Vector2(1048,140);const float cell=4;
        Panel(new Rect2(origin-new Vector2(10,22),new Vector2(212,166)),.87f);
        Text("UTFORSKAT",origin+new Vector2(0,-7),11,Gold);
        var seen=_game.Rooms!.Rooms[_game.Rooms.Current].Explored;
        for(int i=0;i<RoomSight.Count;i++)if(RoomSight.Seen(seen,i))
            DrawRect(new Rect2(origin+new Vector2(i%RoomSight.Columns,i/RoomSight.Columns)*cell,new Vector2(cell,cell)),_game.RoomVisible[i]?new Color("77735e"):new Color("383d35"));
        Vector2 Point(System.Numerics.Vector2 p)=>origin+G(p)*(cell/RoomSight.Cell);
        foreach(var link in RoomLinks.From(_game.Rooms.Current))
        {var door=link.At(_game.Rooms.Current);if(_game.ExploredRoomPoint(door))DrawCircle(Point(door),3,RoomLinks.Open(_game.Rooms,link)?Teal:Gold);}
        DrawCircle(Point(_game.Player),3,Pale);
    }
}
