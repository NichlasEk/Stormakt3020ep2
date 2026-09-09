using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
namespace Atland;
public static class Cabin
{
    public const string Room="ship-cabin";
    // One environment transform keeps painted furniture, floor, collisions and interaction points aligned.
    // Actors remain at the same human scale as in the rest of the game.
    public const float EnvironmentScale=.65f;
    public static Vector2 FromPainting(Vector2 p)=>new Vector2(1070,420)+(p-new Vector2(1070,420))*EnvironmentScale;
    public static readonly Vector2 Entry=FromPainting(new(1070,420)),Ebba=FromPainting(new(855,430)),Talk=FromPainting(new(870,490)),Rest=FromPainting(new(1240,565)),Helm=FromPainting(new(380,425));
    public static readonly Vector2[] Ground=new Vector2[]{new(90,535),new(415,375),new(815,315),new(1010,375),new(1090,330),new(1200,400),new(1235,520),new(1440,625),new(1220,965),new(365,980)}.Select(FromPainting).ToArray();
    public static readonly Vector2[][] Obstacles=new Vector2[][]{new Vector2[]{new(440,285),new(620,220),new(815,305),new(815,370),new(640,467),new(465,390)},new Vector2[]{new(833,420),new(855,412),new(877,420),new(877,440),new(855,450),new(833,440)}}.Select(poly=>poly.Select(FromPainting).ToArray()).ToArray();
}
public sealed class CabinRun
{
    public string ReturnRoom=Uppsala.Court;
    public int Conversation;
    public int EnvironmentVersion;
    public bool Rested;
    [JsonIgnore] public bool Briefed=>Conversation==3;
}
public sealed partial class Combat
{
    [JsonIgnore] public bool InCabin=>InConnectedWorld&&Rooms!.Current==Cabin.Room;
    [JsonIgnore] public CabinRun CabinState=>Rooms!.Cabin;
    [JsonIgnore] public string CabinGoal=>ObservatoryState.Debriefed?"Gamla Uppsala är nästa mål":ObservatoryState.OriginalTaken?"Visa originalet för Ebba":!CabinState.Briefed?"Tala med Ebba vid bordet":!CabinState.Rested?"Lägg om såren ombord":"Ordern är hos Ebba";
    [JsonIgnore] public Vector2 CabinObjective=>ObservatoryState.OriginalTaken&&!ObservatoryState.Debriefed?Cabin.Talk:!CabinState.Briefed?Cabin.Talk:!CabinState.Rested?Cabin.Rest:Cabin.Entry;
    public bool BoardCabin()
    {
        if(!InConnectedWorld||!MeridianState.OrderTaken||!CanShipTravel(Rooms!.Current==Uppsala.Court?Regiment.Quay:Uppsala.Court))return false;
        CabinState.ReturnRoom=Rooms.Current;bool first=!Rooms.Rooms[Cabin.Room].Visited;
        EnterConnectedRoom(Cabin.Room);Player=Cabin.Entry;AttackTime=AttackBuffer=DodgeTime=0;UpdateRoomSight(true);
        Emit("cabin-enter",Player);if(first)Emit("radio",Player,"cabin-welcome");Emit("region",Player,RoomName);Emit("checkpoint",Player);return true;
    }
    public bool LeaveCabin(bool sail=false)
    {
        if(!InCabin||Dead||Vector2.Distance(Player,sail?Cabin.Helm:Cabin.Entry)>=72)return false;
        var port=CabinState.ReturnRoom;EnterConnectedRoom(port);Player=port==Uppsala.Court?Uppsala.Ramp:Uppsala.Board;UpdateRoomSight(true);
        Emit("cabin-enter",Player);Emit("checkpoint",Player);if(sail)Emit("ship-travel",Player,port==Uppsala.Court?Regiment.Quay:Uppsala.Court);return true;
    }
    private bool StepCabin(Func<Vector2,bool> near)
    {
        if(!InCabin)return false;var c=CabinState;
        if(near(Cabin.Entry)){LeaveCabin();return true;}
        if(near(Cabin.Helm)){LeaveCabin(true);return true;}
        if(near(Cabin.Talk))
        {
            if(c.Conversation==0){c.Conversation=1;Emit("radio",Player,"cabin-order");Emit("radio",Player,"cabin-soldiers");}
            else if(c.Conversation==1){c.Conversation=2;Emit("radio",Player,"cabin-plate");}
            else if(c.Conversation==2){c.Conversation=3;Emit("radio",Player,"cabin-original");Emit("campaign",Player,"ORDERN HOS EBBA: Förflyttningen omfattade hela kvarteret. Destinationen är struken. Originalet i övre observatoriet är nästa spår. Ebba behåller handlingen medan expeditionen förbereder nästa färd.");}
            else if(ObservatoryState.OriginalTaken&&!ObservatoryState.Debriefed){ObservatoryState.Debriefed=true;Emit("radio",Player,"observatory-debrief");Emit("radio",Player,"observatory-next");Emit("campaign",Player,"GAMLA UPPSALA: Originalet är säkrat ombord. Nästa färd går till mottagningsanläggningen under kungshögarna. Märta och Elin står på listan. Deras öde återstår att ta reda på.");}
            else Emit("radio",Player,ObservatoryState.Debriefed?"observatory-next":"cabin-repeat");
            Emit("room-sound",Player,"paper");Emit("checkpoint",Player);return true;
        }
        if(near(Cabin.Rest))
        {
            if(!c.Briefed){Emit("room-notice",Player,"Visa Ebba ordern först.");return true;}
            if(!c.Rested){c.Rested=true;Health=100;Stamina=100;Emit("radio",Player,"cabin-rest");Emit("heal",Player,"Förband · fullt liv",100);Emit("checkpoint",Player);}
            else Emit("radio",Player,"cabin-well");
        }
        return true;
    }
    // Native/pure-test fixture: a completed Meridian chapter, still on the quay side of the ramp.
    public static Combat NewCabinPreview(Order order)
    {
        var g=NewMeridianPreview(order);var m=g.MeridianState;m.CourtOpen=true;g.EnterConnectedRoom(Meridian.Clock);
        m.ClockStarted=m.LedgerRead=m.ClockAnchored=true;m.Cycles=2;g.EnterConnectedRoom(Meridian.Hall);
        m.PlateSet=m.WardenDefeated=m.OrderTaken=true;m.Breaks=3;
        var boss=g.Enemies.Single(e=>e.Kind==EnemyKind.MeridianWarden);boss.Health=0;boss.Retired=true;
        g.EnterConnectedRoom(Uppsala.Court);g.Player=Uppsala.Ramp;g.Health=21;g.Inventory.Drops.Clear();g.Events.Clear();g.UpdateRoomSight(true);g.ValidateRooms();return g;
    }
    private void ValidateCabin()
    {
        if(!InConnectedWorld)return;var c=CabinState;
        if(c is null||c.EnvironmentVersion<0||c.EnvironmentVersion>1||c.Conversation<0||c.Conversation>3||c.ReturnRoom is not (Uppsala.Court or Regiment.Quay)||(Rooms!.Rooms[Cabin.Room].Visited&&!MeridianState.OrderTaken)||(c.Conversation>0&&!Rooms.Rooms[Cabin.Room].Visited)||(c.Rested&&!c.Briefed))throw new System.IO.InvalidDataException("Ogiltigt kajutmöte");
        if(c.EnvironmentVersion==0)
        {
            if(InCabin)Player=Cabin.FromPainting(Player);
            var shift=ConnectedWorld.Origin(Cabin.Room)-WorldOrigin;
            foreach(var drop in Inventory.Drops.Where(d=>d.Room==Cabin.Room))drop.Position=Cabin.FromPainting(drop.Position-shift)+shift;
            c.EnvironmentVersion=1;
        }
    }
}
