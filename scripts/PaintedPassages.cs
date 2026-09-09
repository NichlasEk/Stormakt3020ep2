using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public sealed class PaintedPassage
{
    public string Link="",From="";
    public float Time;
    public Vector2 Start;
    public bool Arrived;
}
public sealed partial class Combat
{
    // The legacy continuous corridors remain available for geometry checks and old tooling.
    public bool PaintedRooms;
    public PaintedPassage? Passage;
    [JsonIgnore] public float PassageScale=>Passage==null?1:Passage.Time<.85f?1-.46f*Math.Clamp(Passage.Time/.7f,0,1):.54f+.46f*Math.Clamp((Passage.Time-1.05f)/.75f,0,1);
    [JsonIgnore] public float PassageCurtain=>Passage==null?0:Passage.Time<.85f?Math.Clamp((Passage.Time-.55f)/.25f,0,1):1-Math.Clamp((Passage.Time-1.0f)/.3f,0,1);
    public void EnablePaintedRooms()
    {
        if(!PaintedRooms&&InConnectedWorld&&Passage==null&&!Navigation.Contains(ConnectedWorld.Ground(Rooms!.Current),Player))
        {
            var link=RoomLinks.From(Rooms.Current).OrderBy(l=>Vector2.DistanceSquared(l.At(Rooms.Current),Player)).FirstOrDefault();
            if(link!=null)Player=link.A==Rooms.Current?link.ArrivalA:link.ArrivalB;
        }
        PaintedRooms=true;
    }
    private static Vector2[] OrientedPassage(RoomLink link,string from)
    {var route=ConnectedWorld.Route(link);if(from==link.B)Array.Reverse(route);return route;}
    private bool StepPaintedPassage(Controls input,float dt)
    {
        if(!InConnectedWorld||!PaintedRooms)return false;
        if(Passage==null)
        {
            if(Dead||AttackTime>0||DodgeTime>0||input.Move.LengthSquared()<.01f)return false;
            foreach(var link in RoomLinks.From(Rooms!.Current))
            {
                if(!RoomLinks.Open(Rooms,link))continue;
                var door=Rooms.Doors[link.Id];if(HasWorldDoor(link)&&!door.Broken&&door.Openness<.85f)continue;
                var route=OrientedPassage(link,Rooms.Current);var mouth=route[1]-WorldOrigin;
                if(Vector2.Distance(Player,mouth)>55||Vector2.Dot(input.Move,route[1]-route[0])<=0)continue;
                Passage=new(){Link=link.Id,From=Rooms.Current,Start=Player};AttackTime=AttackBuffer=DodgeTime=0;Emit("checkpoint",Player);break;
            }
            if(Passage==null)return false;
        }
        var p=Passage;var l=RoomLinks.All.Single(l=>l.Id==p.Link);var path=OrientedPassage(l,p.From);p.Time+=dt;
        if(!p.Arrived&&p.Time>=.85f)
        {
            EnterConnectedRoom(l.Other(p.From));p.Arrived=true;Player=path[^2]-WorldOrigin;
            Emit("painted-arrival",Player);UpdateRoomSight(true);
        }
        var from=p.Arrived?path[^2]-WorldOrigin:p.Start;
        var to=p.Arrived?path[^1]-WorldOrigin:path[1]-WorldOrigin+(p.From==Gamla.Landing?new Vector2(0,40):Vector2.Zero);
        float t=p.Arrived?Math.Clamp((p.Time-1.05f)/.75f,0,1):Math.Clamp(p.Time/.7f,0,1);
        Player=Vector2.Lerp(from,to,t);Facing=Normal(to-from,Facing);Moving=t>0&&t<1;Walk+=dt*5;Elapsed+=dt;
        if(p.Time>=1.8f){Player=to;Passage=null;Moving=false;_roomInteractHeld=true;UpdateRoomSight(true);Emit("checkpoint",Player);}
        return true;
    }
    private void ValidatePaintedPassage()
    {
        if(Passage==null)return;var p=Passage;var link=RoomLinks.All.FirstOrDefault(l=>l.Id==p.Link);
        if(!PaintedRooms||!InConnectedWorld||link==null||p.From!=link.A&&p.From!=link.B||!float.IsFinite(p.Time)||p.Time<0||p.Time>1.8f||!float.IsFinite(p.Start.X)||!float.IsFinite(p.Start.Y)||Rooms!.Current!=(p.Arrived?link.Other(p.From):p.From)||!RoomLinks.Open(Rooms,link))throw new System.IO.InvalidDataException("Ogiltig målad övergång");
    }
}
