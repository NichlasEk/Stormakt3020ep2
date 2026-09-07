using System;
using System.Numerics;
using System.Linq;
using System.Text.Json.Serialization;
namespace Atland;

public sealed class PhysicalDoor
{
    public bool Locked=true,TargetOpen;
    public float Openness,Health=180;
    [JsonIgnore] public bool Broken=>Health<=0;
    [JsonIgnore] public Vector2 Tip=>DoorTrialLayout.Hinge+DoorTrialLayout.Leaf(Openness);
    [JsonIgnore] public Vector2[] Solid=>Broken?Array.Empty<Vector2>():DoorTrialLayout.Bar(DoorTrialLayout.Hinge,Tip,15);
}
public sealed class DoorTrial
{
    public PhysicalDoor Door=new();
    public bool KeyTaken,EnteredLodge;
}
public static class DoorTrialLayout
{
    public static readonly Vector2 Hinge=new(724,533),ClosedTip=new(863,612),Center=(Hinge+ClosedTip)/2;
    public static readonly Vector2 Key=new(520,680),Start=new(620,740),Guard=new(1010,475);
    public static readonly Vector2[] Ground={new(125,385),new(340,280),new(980,85),new(1465,365),new(1465,928),new(1120,972),new(355,774),new(100,605)};
    public static readonly Vector2[][] Walls={Bar(new(150,230),new(710,535),21),Bar(new(864,620),new(1530,1005),21)};
    public static Vector2 Leaf(float open){float a=open*MathF.PI/2;var closed=ClosedTip-Hinge;return new(closed.X*(MathF.Cos(a)+MathF.Sin(a)),closed.Y*(MathF.Cos(a)-MathF.Sin(a)));}
    public static Vector2[] Bar(Vector2 a,Vector2 b,float width)
    {
        // Rounded end caps overlap adjoining jambs. Flat ends left a foot-sized seam
        // which the navigation graph could correctly (but undesirably) route through.
        var polygon=new Vector2[18];float heading=MathF.Atan2(b.Y-a.Y,b.X-a.X);
        for(int i=0;i<9;i++)
        {
            float angle=heading-MathF.PI/2+i*MathF.PI/8;
            polygon[i]=b+new Vector2(MathF.Cos(angle),MathF.Sin(angle))*width;
            angle+=MathF.PI;polygon[i+9]=a+new Vector2(MathF.Cos(angle),MathF.Sin(angle))*width;
        }
        return polygon;
    }
    public static float Side(Vector2 p)=>p.Y-(Hinge.Y+(p.X-Hinge.X)*(ClosedTip.Y-Hinge.Y)/(ClosedTip.X-Hinge.X));
    public static float Distance(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Math.Clamp(Vector2.Dot(p-a,d)/d.LengthSquared(),0,1));}
}
public sealed partial class Combat
{
    public DoorTrial? DoorTest;
    public bool PreferRoomRoute;
    [JsonIgnore] public bool InDoorTrial=>DoorTest!=null;
    [JsonIgnore] public Vector2[][] DoorObstacles=>DoorTrialLayout.Walls.Append(DoorTest!.Door.Solid).ToArray();
    private Vector2 MoveBody(Vector2 from,Vector2 target)
    {
        var bounded=Bound(target);
        if(!InDoorTrial||ClearPath(from,bounded))return bounded;
        // Check the entire displacement, including overlap separation and knockback.
        // Slide along an available axis instead of snapping to the far face of a solid.
        var x=Bound(new(target.X,from.Y));var y=Bound(new(from.X,target.Y));
        bool canX=ClearPath(from,x),canY=ClearPath(from,y);
        if(canX&&canY)return Vector2.DistanceSquared(x,target)<Vector2.DistanceSquared(y,target)?x:y;
        return canX?x:canY?y:from;
    }
    public static Combat NewDoorTrial(Order order)
    {
        var g=NewRooms(order);g.DoorTest=new();g.Enemies.Clear();g.Events.Clear();g.Player=DoorTrialLayout.Start;
        g.Spawn(EnemyKind.Guard,DoorTrialLayout.Guard);g.UpdateRoomSight(true);return g;
    }
    private void StepDoorTrial(Controls input,float dt)
    {
        var t=DoorTest!;var d=t.Door;
        bool pressed=input.Interact&&!_roomInteractHeld;_roomInteractHeld=input.Interact;
        if(pressed&&!Dead&&AttackTime<=0&&DodgeTime<=0)
        {
            if(!t.KeyTaken&&Vector2.Distance(Player,DoorTrialLayout.Key)<65&&ClearPath(Player,DoorTrialLayout.Key))
            {t.KeyTaken=true;Emit("inscription",Player,"LOGEMENTETS NYCKEL");Emit("checkpoint",Player);}
            else if(DoorTrialLayout.Distance(Player,DoorTrialLayout.Hinge,d.Tip)<80||DoorTrialLayout.Distance(Player,DoorTrialLayout.Hinge,DoorTrialLayout.ClosedTip)<80)
            {
                if(d.Broken)Emit("room-notice",Player,"Dörren är sönderslagen och kan inte stängas.");
                else if(d.Locked&&!t.KeyTaken)Emit("room-notice",Player,"Låst. Sök nyckeln eller slå sönder träet.");
                else
                {
                    if(d.Locked){d.Locked=false;Emit("room-sound",Player,"door-unlock");}
                    d.TargetOpen=!d.TargetOpen;Emit("room-sound",DoorTrialLayout.Center,"door-creak");Emit("checkpoint",Player);
                }
            }
        }
        if(!d.Broken)
        {
            float next=Math.Clamp(d.Openness+(d.TargetOpen?1:-1)*dt*1.6f,0,1);
            if(next!=d.Openness)
            {
                // Reject a swept closing/opening motion occupied by an actor; never push through a jamb.
                bool blocked=false;
                for(int i=1;i<=4;i++)
                {
                    var tip=DoorTrialLayout.Hinge+DoorTrialLayout.Leaf(d.Openness+(next-d.Openness)*i/4);
                    if(Enemies.Where(e=>!e.Dead).Select(e=>e.Position).Append(Player).Any(p=>DoorTrialLayout.Distance(p,DoorTrialLayout.Hinge,tip)<31)){blocked=true;break;}
                }
                if(!blocked){d.Openness=next;UpdateRoomSight(true);}
            }
        }
        if(!t.EnteredLodge&&DoorTrialLayout.Side(Player)<-45){t.EnteredLodge=true;Emit("inscription",Player,"VÄKTARNAS LOGEMENT");Emit("checkpoint",Player);}
        UpdateRoomSight();
    }
    private void HitPhysicalDoor(float range,float damage,float arc)
    {
        if(!InDoorTrial||DoorTest!.Door.Broken)return;
        var d=DoorTest.Door;var delta=(DoorTrialLayout.Hinge+d.Tip)/2-Player;
        if(delta.Length()<range+20&&Vector2.Dot(Normal(delta,Facing),Facing)>arc&&Navigation.Clear(DoorTrialLayout.Ground,DoorTrialLayout.Walls,Player,Player+delta))
        {
            d.Health=Math.Max(0,d.Health-damage*(Weapon==Weapon.Hammer?1.6f:.65f));
            Emit("room-sound",DoorTrialLayout.Center,d.Broken?"door-break":"door-hit");Emit("hit",DoorTrialLayout.Center,"TRÄET SPLITTRAS");
            foreach(var e in Enemies.Where(e=>!e.Dead&&Vector2.Distance(e.Position,DoorTrialLayout.Center)<600))e.Alerted=true;
            if(d.Broken){d.TargetOpen=true;d.Locked=false;UpdateRoomSight(true);Emit("checkpoint",Player);}
        }
    }
    public void ValidateDoorTrial()
    {
        if(DoorTest==null)return;
        var d=DoorTest.Door;
        if(Rooms?.Current!=PortRooms.Court||d==null||!float.IsFinite(d.Health)||d.Health<0||d.Health>180||!float.IsFinite(d.Openness)||d.Openness<0||d.Openness>1
            ||(d.Locked&&(d.TargetOpen||d.Openness!=0||d.Broken))||(!d.Locked&&!d.Broken&&!DoorTest.KeyTaken))throw new System.IO.InvalidDataException("Ogiltig fysisk dörr");
    }
    public void EnterRoomRoute()
    {
        // Move the existing expedition into the room route; retain the player's equipment, resources and history.
        Rooms=new(){LayoutVersion=5};foreach(var id in PortRooms.Ids.Skip(2))Rooms.Rooms.Add(id,new());
        AtlandCampaign=true;CampaignStage=0;CampaignProgress=CampaignMask=CampaignWave=0;CampaignFinished=false;
        Region=Region.Atland;Phase=Phase.Campaign;Enemies.Clear();Shots.Clear();Hazards.Clear();Seals.Clear();
        Player=new(768,805);Spawn(EnemyKind.Guard,new(650,625));AttackTime=AttackBuffer=DodgeTime=HitStop=Hurt=GuardTime=0;Guarding=Moving=false;
        Events.Clear();Emit("region",Player,RoomName);Emit("radio",Player,"rooms-entry");Emit("cinematic",Player,"atland-intro");Emit("checkpoint",Player);UpdateRoomSight(true);
    }
}
