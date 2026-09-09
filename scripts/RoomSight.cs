using System;
using System.Numerics;
using System.Linq;
using System.Text.Json.Serialization;

namespace Atland;

public static class RoomSight
{
    public const int Cell=32,Columns=48,Rows=32,Count=Columns*Rows,Bytes=Count/8;
    public const float Radius=330;
    public static Vector2 Center(int index)=>new((index%Columns+.5f)*Cell,(index/Columns+.5f)*Cell);
    public static int Index(Vector2 at)=>at.X<0||at.Y<0||at.X>=Columns*Cell||at.Y>=Rows*Cell?-1:(int)(at.Y/Cell)*Columns+(int)(at.X/Cell);
    public static bool Seen(byte[] bits,int index)=>index>=0&&index<Count&&(bits[index/8]&(1<<(index%8)))!=0;
    public static void Reveal(byte[] bits,int index)=>bits[index/8]|=(byte)(1<<(index%8));
}

public sealed partial class Combat
{
    [JsonIgnore] public bool[] RoomVisible=new bool[RoomSight.Count];
    [JsonIgnore] public int SightRevision;
    [JsonIgnore] private long _sightTick=-100;
    [JsonIgnore] private string _sightRoom="";
    [JsonIgnore] private Vector2 _sightPosition;
    // Movement, bullets and sight use the same measured walk polygon and obstacle.
    public bool CanSeeRoomPoint(Vector2 at)=>!InRooms||(InCabin&&RoomSight.Index(at)>=0)||(Vector2.DistanceSquared(Player,at)<=RoomSight.Radius*RoomSight.Radius&&ClearPath(Player,at));
    public bool ExploredRoomPoint(Vector2 at)=>!InRooms||RoomSight.Seen(Rooms!.Rooms[Rooms.Current].Explored,RoomSight.Index(at));
    public void UpdateRoomSight(bool force=false)
    {
        if(!InRooms)return;
        if(!force&&_sightRoom==Rooms!.Current&&Tick-_sightTick<6&&Vector2.DistanceSquared(Player,_sightPosition)<32*32)return;
        _sightRoom=Rooms!.Current;_sightTick=Tick;_sightPosition=Player;
        var seen=Rooms.Rooms[Rooms.Current].Explored;
        for(int i=0;i<RoomSight.Count;i++)
        {
            RoomVisible[i]=CanSeeRoomPoint(RoomSight.Center(i));
            if(RoomVisible[i])RoomSight.Reveal(seen,i);
        }
        // Preserve landmarks at polygon edges even if their cell center lies just outside.
        foreach(var at in RoomLinks.From(Rooms.Current).Select(l=>l.At(Rooms.Current)).Concat(new[]{Player,InDoorTrial?DoorTrialLayout.Center:RoomObjective,Rooms.Current==PortRooms.Pump?PortRooms.Wheel:RoomObjective}))
        {
            int index=RoomSight.Index(at);
            if(index>=0&&CanSeeRoomPoint(at)){RoomSight.Reveal(seen,index);RoomVisible[index]=true;}
        }
        if(InConnectedWorld&&!PaintedRooms)
        {
            foreach(var pair in Rooms.Rooms.Where(p=>p.Value.Visited&&p.Key!=Rooms.Current))
            {var shift=ConnectedWorld.Origin(pair.Key)-WorldOrigin;for(int i=0;i<RoomSight.Count;i++)if(CanSeeRoomPoint(RoomSight.Center(i)+shift))RoomSight.Reveal(pair.Value.Explored,i);}
            foreach(var l in RoomLinks.All)
            {var route=ConnectedWorld.Route(l);for(int k=1;k<route.Length;k++)
                {int count=Math.Max(1,(int)MathF.Ceiling(Vector2.Distance(route[k-1],route[k])/32));for(int j=0;j<count;j++)if(CanSeeRoomPoint(Vector2.Lerp(route[k-1],route[k],(j+.5f)/count)-WorldOrigin))Rooms.CorridorSeen.Add(l.Id+":"+k+":"+j);}}
        }
        SightRevision++;
    }
}
