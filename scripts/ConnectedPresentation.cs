using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;
using NVec=System.Numerics.Vector2;

public partial class Main
{
    private Texture2D? _passageMaterials;
    private readonly Dictionary<string,ImageTexture> _worldFog=new();
    private int _worldFogRevision=-1;
    private Combat? _worldFogGame;
    private bool WorldRectVisible(Vector2 at,Vector2 size)=>new Rect2(at,size).Intersects(new Rect2(-Offset/Zoom,new Vector2(1280,720)/Zoom).Grow(180));
    private void DrawConnectedGround()
    {
        _passageMaterials??=GD.Load<Texture2D>("res://assets/art/passage-materials-v1.png");
        DrawRect(new Rect2(-Offset/Zoom,new Vector2(1280,720)/Zoom),new Color(3/255f,5/255f,6/255f));
        DrawConnectionFloors(true);var backWalls=new List<(float Depth,Action Draw)>();AddPassageWalls(backWalls,true);foreach(var wall in backWalls.OrderBy(w=>w.Depth))wall.Draw();
        foreach(var id in ConnectedWorld.RoomIds)
        {
            var delta=G(ConnectedWorld.Origin(id)-_game.WorldOrigin);if(!WorldRectVisible(delta,new(1536,1024)))continue;
            _paintRoom=id;DrawTextureRect(RoomBackground,new Rect2(delta,new Vector2(1536,1024)),false);
        }
        _paintRoom=null;DrawConnectionFloors(false);
    }
    private void DrawConnectionFloors(bool behindPaintings)
    {
        foreach(var l in RoomLinks.All)
        {
            if(ConnectedWorld.Painted(l)!=behindPaintings)continue;
            var route=ConnectedWorld.Route(l).Select(p=>G(p-_game.WorldOrigin)).ToArray();
            for(int i=1;i<route.Length;i++)
            {
                var a=route[i-1];var b=route[i];var direction=(b-a).Normalized();var side=G(ConnectedWorld.Side(N(a),N(b)));
                int count=Math.Max(1,(int)Mathf.Ceil(a.DistanceTo(b)/110));
                for(int j=0;j<count;j++)
                {
                    var p=a.Lerp(b,(float)j/count);var q=a.Lerp(b,(float)(j+1)/count);
                    if(!WorldRectVisible(p-new Vector2(160,180),new Vector2(400,400)))continue;
                    var polygon=new[]{p-side,q-side,q+side,p+side};
                    // Reuse the approved rough masonry, keeping the texel scale close to
                    // the surrounding paintings. No flat replacement room backgrounds.
                    var uv=new[]{new Vector2(.005f,.005f),new Vector2(.495f,.005f),new Vector2(.495f,.495f),new Vector2(.005f,.495f)};
                    DrawPolygon(polygon,new[]{new Color(.92f,.92f,.86f)},uv,_passageMaterials);
                    DrawLine(p-side,q-side,new Color("292c25"),3,true);DrawLine(p+side,q+side,new Color("242720"),3,true);
                    DrawLine(p-side+direction*4,p+side+direction*4,new Color(0,0,0,.16f),1,true);
                }
            }
        }
    }
    private void DrawConnectedActors()
    {
        var layers=new List<(float Depth,Action Draw)>();
        foreach(var enemy in _game.Enemies.Where(e=>_game.CanSeeRoomPoint(e.Position))){var actor=enemy;layers.Add((actor.Position.Y,()=>Actor(actor,actor.Dead)));}
        layers.Add((_game.Player.Y,DrawPlayer));
        foreach(var id in ConnectedWorld.RoomIds)
        {
            var delta=G(ConnectedWorld.Origin(id)-_game.WorldOrigin);if(!WorldRectVisible(delta,new(1536,1024)))continue;
            _paintRoom=id;var local=new List<(float Depth,Action Draw)>();
            if(id is PortRooms.Pump or PortRooms.Cistern)AddWaterLayers(local);
            AddOathLayers(local);AddArchiveLayers(local);AddRootwayLayers(local);AddRegimentLayers(local);AddMineLayers(local);AddFoundryLayers(local);AddOlderArchLayers(local);AddPaintedArchLayers(local);
            if(id==PortRooms.Lodge)
            {
                local.Add((623,()=>PaintForeground(_lodgePassageArt!,new Vector2[]{new(677,513),new(720,490),new(722,466),new(753,450),new(812,454),new(874,477),new(882,572),new(791,621),new(677,566)})));
                local.Add((647,()=>PaintForeground(_lodgePassageArt!,new Vector2[]{new(493,580),new(517,556),new(568,574),new(597,592),new(594,626),new(548,647),new(494,617)})));
            }
            foreach(var item in local)
            {
                var layer=item;var room=id;
                layers.Add((layer.Depth+delta.Y,()=>{_paintRoom=room;DrawSetTransform(Offset+delta*Zoom,0,Vector2.One*Zoom);layer.Draw();DrawSetTransform(Offset,0,Vector2.One*Zoom);_paintRoom=null;}));
            }
        }
        _paintRoom=null;LoadDoorArt();
        foreach(var l in RoomLinks.All)
        {
            if(l.Id is "chamber" or "archive" or "mine-entry" or "foundry")continue;
            var at=G(ConnectedWorld.Center(l)-_game.WorldOrigin);if(!WorldRectVisible(at-new Vector2(210,240),new(420,420)))continue;
            var center=ConnectedWorld.Center(l)-_game.WorldOrigin;
            var path=ConnectedWorld.Route(l);var approach=NVec.Normalize(ConnectedWorld.Painted(l)?path[1]-path[0]:path[2]-path[1])*65;
            // Sight stops at a closed leaf; sample the floor on either side rather
            // than hiding the very obstacle that blocked the sight ray.
            if(NVec.Distance(center,_game.Player)>350||!(_game.CanSeeRoomPoint(center-approach)||_game.CanSeeRoomPoint(center+approach)))continue;
            var door=_game.Rooms!.Doors[l.Id];bool blocked=!RoomLinks.Open(_game.Rooms,l);
            if(!blocked&&!Combat.HasWorldDoor(l))continue;
            var h=G(ConnectedWorld.Hinge(l)-_game.WorldOrigin);var end=G(ConnectedWorld.Tip(l,blocked?0:door.Openness)-_game.WorldOrigin);
            var closed=G(ConnectedWorld.Tip(l,0)-_game.WorldOrigin);
            foreach(var foot in ConnectedWorld.Painted(l)?Array.Empty<Vector2>():new[]{h,closed})
            {
                var f=foot;
                layers.Add((f.Y+8,()=>
                {
                    var a=f+new Vector2(-13,0);var b=f+new Vector2(0,-8);var c=f+new Vector2(13,0);var d=f+new Vector2(0,8);var up=new Vector2(0,-145);
                    StoneFace(new[]{a+up,b+up,c+up,d+up},.9f);
                    StoneFace(new[]{a+up,d+up,d,a},.65f);StoneFace(new[]{d+up,c+up,c,d},.83f);
                }));
            }
            if(door.Broken&&!blocked){layers.Add((at.Y,()=>{for(int j=0;j<5;j++)DrawLine(at+new Vector2(j*14-32,j*7),at+new Vector2(j*14-10,j*7-15),new Color("635440"),6,true);}));continue;}
            var axis=(end-h).Normalized();var half=new Vector2(-axis.Y,axis.X)*7;var upLeaf=new Vector2(0,-124);
            var uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
            layers.Add(((h.Y+end.Y)/2,()=>
            {
                var backA=h-half;var backB=end-half;var frontA=h+half;var frontB=end+half;
                DrawPolygon(new[]{backA+upLeaf,backB+upLeaf,backB,backA},new[]{new Color(.56f,.54f,.48f)},uv,_doorFace);
                DrawPolygon(new[]{frontA+upLeaf,frontB+upLeaf,frontB,frontA},new[]{new Color(.84f,.80f,.70f)},uv,_doorFace);
                DrawPolygon(new[]{backA+upLeaf,backB+upLeaf,frontB+upLeaf,frontA+upLeaf},new[]{new Color(.65f,.60f,.50f)},uv,_doorFace);
                DrawPolygon(new[]{backB+upLeaf,frontB+upLeaf,frontB,backB},new[]{new Color(.42f,.39f,.32f)},uv,_doorFace);
                if(blocked)DrawLine(h+upLeaf*.55f,end+upLeaf*.55f,new Color("282c29"),7,true);
            }));
        }
        AddPassageWalls(layers);
        foreach(var layer in layers.OrderBy(l=>l.Depth))layer.Draw();
    }


    private void StoneFace(Vector2[] polygon,float light)
    {DrawPolygon(polygon,new[]{new Color(light,light*.98f,light*.90f)},new[]{new Vector2(.51f,.01f),new Vector2(.85f,.01f),new Vector2(.85f,.49f),new Vector2(.51f,.49f)},_passageMaterials);}
    private void AddPassageWalls(List<(float Depth,Action Draw)> layers,bool behindPaintings=false)
    {
        foreach(var l in RoomLinks.All)
        {
            if(ConnectedWorld.Painted(l)!=behindPaintings)continue;
            var route=ConnectedWorld.Route(l);
            for(int i=1;i<route.Length;i++)
            {
                var a=route[i-1];var b=route[i];var dir=NVec.Normalize(b-a);var normal=ConnectedWorld.Side(a,b)/100;
                int count=Math.Max(1,(int)MathF.Ceiling(NVec.Distance(a,b)/44));
                for(int j=0;j<count;j++)foreach(int sign in new[]{-1,1})
                {
                    var mid=NVec.Lerp(a,b,(j+.5f)/count);
                    if(!_game.CanSeeRoomPoint(mid-_game.WorldOrigin))continue;
                    var p=G(NVec.Lerp(a,b,(float)j/count)+normal*103*sign-_game.WorldOrigin);
                    var q=G(NVec.Lerp(a,b,(float)(j+1)/count)+normal*103*sign-_game.WorldOrigin);
                    var side=G(normal)*14*sign;var up=new Vector2(0,-48-(j%3)*2);
                    layers.Add(((p.Y+q.Y)/2,()=>
                    {
                        StoneFace(new[]{p+up,q+up,q,p},.78f);
                        StoneFace(new[]{p+up,q+up,q+side+up,p+side+up},.97f);
                        DrawLine(p+up,q+up,new Color(.55f,.52f,.42f,.22f),1,true);
                    }));
                }
            }
        }
    }
    private static bool InsideArchPainting(RoomLink link,NVec world)
        =>new Rect2(G(ConnectedWorld.Origin(link.A)),new Vector2(1536,1024)).HasPoint(G(world))||new Rect2(G(ConnectedWorld.Origin(link.B)),new Vector2(1536,1024)).HasPoint(G(world));
    private void AddPaintedArchLayers(List<(float Depth,Action Draw)> layers)
    {
        AddRootArchLayers(layers);
        if(PaintRoom is not (PortRooms.Gallery or PortRooms.Chamber))return;
        var art=RoomBackground;
        // Trace the actual stone silhouette. The open aperture is left untouched.
        var polygons=PaintRoom==PortRooms.Gallery?new[]{
            new Vector2[]{new(1259,0),new(1536,0),new(1536,378),new(1536,638),new(1280,466),new(1260,421),new(1266,350)},
            new Vector2[]{new(1145,318),new(1169,279),new(1194,290),new(1208,343),new(1204,409),new(1165,430),new(1145,407)},
            new Vector2[]{new(1160,292),new(1180,254),new(1212,226),new(1240,230),new(1273,269),new(1288,323),new(1264,351),new(1256,313),new(1238,288),new(1217,294),new(1193,324),new(1184,353)}
        }:new[]{
            new Vector2[]{new(0,0),new(169,0),new(169,190),new(203,293),new(205,443),new(0,571)},
            new Vector2[]{new(290,272),new(330,224),new(462,130),new(465,315),new(300,408),new(291,377)},
            new Vector2[]{new(175,329),new(189,290),new(238,248),new(275,235),new(303,256),new(313,302),new(290,327),new(285,293),new(270,287),new(245,303),new(217,332),new(205,366),new(182,382)}
        };
        foreach(var polygon in polygons)
        {
            // Narrow slices sort each jamb against feet, so either side remains playable.
            for(float x=polygon.Min(p=>p.X);x<polygon.Max(p=>p.X);x+=12)
            {
                var strip=ClipDoorStrip(ClipDoorStrip(polygon,x,true),x+12,false);if(strip.Length<3)continue;
                float depth=PaintRoom==PortRooms.Gallery?420+(x-1250)*.58f:430-(x-230)*.57f;
                layers.Add((depth,()=>PaintForeground(art,strip)));
            }
        }
    }
    private void DrawConnectedJournal()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.025f,.023f,.019f,.97f));
        Text("ATLAND · GÅNGVÄGARNA",new Vector2(80,90),28,Pale,true);
        Wrapped("Följ passagerna till fots genom Atland, regementets marker och gruvan. Sidovägar öppnar återtåg. Mellan regementets brygga och berget går färden med båt.",new Vector2(80,132),1100,19,Muted,28);
        Vector2 At(string id)
        {
            int index=Array.IndexOf(ConnectedWorld.RoomIds,id), row=index/7, column=index%7;
            return new Vector2(110+(row%2==0?column:6-column)*175,250+row*115);
        }
        var r=_game.Rooms!;
        foreach(var l in RoomLinks.All)
        {
            if(!r.Rooms[l.A].Visited&&!r.Rooms[l.B].Visited)continue;
            var a=At(l.A);var b=At(l.B);var color=RoomLinks.Open(r,l)?new Color("706c53"):new Color("383b32");
            if(Math.Abs(a.Y-b.Y)<1&&Math.Abs(a.X-b.X)>200)DrawPolyline(new[]{a,a-new Vector2(0,37),b-new Vector2(0,37),b},color,2,true);
            else DrawLine(a,b,color,3,true);
            if(!RoomLinks.Open(r,l))DrawCircle((a+b)/2,5,Gold);
        }
        if(r.Regiment.Discharged)
        {
            var a=At(Regiment.Quay);var b=At(Regiment.Farled);
            for(int i=0;i<10;i++)DrawLine(a.Lerp(b,i/10f),a.Lerp(b,(i+.5f)/10),Teal,2,true);
            Text("BÅT",(a+b)/2+new Vector2(-13,-10),11,Teal);
        }
        foreach(var id in ConnectedWorld.RoomIds)
        {
            bool visited=r.Rooms[id].Visited;var at=At(id);
            DrawCircle(at,9,id==r.Current?Gold:visited?Teal:new Color("34392f"));
            Wrapped(visited?PortRooms.Name(id):"Outforskat",at+new Vector2(-48,22),112,13,visited?Pale:Muted,18);
        }
        Text($"Nyckel: {(r.KeyTaken?"säkrad":"saknas")} · Vatten: {(r.WaterLowered?"sänkt":"högt")} · Genväg: {(r.ShortcutOpen?"öppen":"reglad")}",new Vector2(80,555),18,Gold);
        Wrapped("Längre färder följer farlederna med båt. När farleden tar slut återstår Karl CCLV. Här under porten går expeditionen till fots.",new Vector2(80,596),800,18,Muted,27);
        Button(new Rect2(930,625,270,49),"Tillbaka","back",true);
    }
    private void DrawConnectedMap()
    {
        var rect=new Rect2(1048,154,192,132);Panel(new Rect2(1038,128,212,168),.87f);Text("NÄROMRÅDE",new Vector2(1048,144),11,Gold);
        const float scale=.12f;var center=rect.GetCenter();var player=_game.Player+_game.WorldOrigin;
        Vector2 Point(NVec world)=>center+G(world-player)*scale;
        foreach(var id in ConnectedWorld.RoomIds)
        {
            var seen=_game.Rooms!.Rooms[id].Explored;
            for(int i=0;i<RoomSight.Count;i++)if(RoomSight.Seen(seen,i))
            {var world=RoomSight.Center(i)+ConnectedWorld.Origin(id);var p=Point(world);if(rect.HasPoint(p))DrawRect(new Rect2(p,new Vector2(3.8f,3.8f)),id==_game.Rooms.Current?new Color("77735e"):new Color("383d35"));}
        }
        foreach(var l in RoomLinks.All)
        {
            var route=ConnectedWorld.Route(l);
            for(int k=1;k<route.Length;k++)
            {int count=Math.Max(1,(int)MathF.Ceiling(NVec.Distance(route[k-1],route[k])/32));for(int j=0;j<count;j++)if(_game.Rooms!.CorridorSeen.Contains(l.Id+":"+k+":"+j))
                {var p=Point(NVec.Lerp(route[k-1],route[k],(j+.5f)/count));if(rect.HasPoint(p))DrawCircle(p,3,new Color("77735e"));}}
            var at=Point(ConnectedWorld.Center(l));if(rect.HasPoint(at)&&(_game.CanSeeRoomPoint(ConnectedWorld.Center(l)-_game.WorldOrigin)||_game.Rooms!.Rooms[l.A].Visited&&_game.Rooms.Rooms[l.B].Visited))DrawCircle(at,3,RoomLinks.Open(_game.Rooms!,l)?Teal:Gold);
        }
        var objective=Point(_game.RoomObjective+_game.WorldOrigin);if(rect.HasPoint(objective))DrawArc(objective,4,0,Mathf.Tau,16,Gold,1,true);
        DrawCircle(center,3,Pale);
    }
    private bool DrawConnectedPrompt()
    {
        var world=_game.Player+_game.WorldOrigin;
        var link=RoomLinks.All.FirstOrDefault(l=>NVec.Distance(world,ConnectedWorld.Center(l))<130);
        if(link==null)return false;
        if(_game.Rooms!.Current==PortRooms.Chamber&&!_game.Rooms.Completed&&NVec.Distance(_game.Player,PortRooms.OathExit)<72)return false;
        if(_game.Rooms!.Current==PortRooms.Archive&&!_game.Rooms.ArchiveSecured&&NVec.Distance(_game.Player,ArchiveRoom.Seal)<72)return false;
        var d=_game.Rooms!.Doors[link.Id];bool open=RoomLinks.Open(_game.Rooms,link);
        if(open&&!Combat.HasWorldDoor(link))return false;
        string label=!open?(link.Gate==PassageGate.Key&&_game.Rooms.KeyTaken?"E / B · Lås upp porten":link.Gate==PassageGate.Shortcut&&_game.Rooms.Current==PortRooms.Cistern?"E / B · Lyft regeln":RoomLinks.LockedReason(link))
            :d.Broken?"Sönderslagen port · gå igenom":Combat.HasWorldDoor(link)?$"E / B · {(d.TargetOpen?"Stäng":"Öppna")} porten":"Öppen passage · fortsätt till fots";
        Panel(new Rect2(390,475,500,53),.92f);Centered(label,640,506,17,Gold);return true;
    }
    private void DrawConnectedFog()
    {
        if(_worldFogGame!=_game||_worldFogRevision!=_game.SightRevision)
        {
            _worldFogGame=_game;_worldFogRevision=_game.SightRevision;
            foreach(var id in ConnectedWorld.RoomIds)
            {
                var shift=ConnectedWorld.Origin(id)-_game.WorldOrigin;
                if(!WorldRectVisible(G(shift),new(1536,1024)))continue;
                var data=new byte[RoomSight.Count*4];var seen=_game.Rooms!.Rooms[id].Explored;
                var feet=_game.Enemies.Where(e=>_game.CanSeeRoomPoint(e.Position)).Select(e=>e.Position-shift).Append(_game.Player-shift).ToArray();
                for(int y=0;y<RoomSight.Rows;y++)for(int x=0;x<RoomSight.Columns;x++)
                {
                    byte alpha=255;
                    for(int rise=0;rise<=5&&y+rise<RoomSight.Rows;rise++)
                    {
                        int i=(y+rise)*RoomSight.Columns+x;
                        if(_game.CanSeeRoomPoint(RoomSight.Center(i)+shift)){alpha=0;break;}
                        if(RoomSight.Seen(seen,i))alpha=158;
                    }
                    var pixel=RoomSight.Center(y*RoomSight.Columns+x);
                    if(feet.Any(p=>Math.Abs(pixel.X-p.X)<64&&pixel.Y>=p.Y-190&&pixel.Y<=p.Y+40))alpha=0;
                    int at=(y*RoomSight.Columns+x)*4;data[at]=3;data[at+1]=5;data[at+2]=6;data[at+3]=alpha;
                }
                using var img=Image.CreateFromData(RoomSight.Columns,RoomSight.Rows,false,Image.Format.Rgba8,data);
                if(_worldFog.TryGetValue(id,out var texture))texture.Update(img);else _worldFog[id]=ImageTexture.CreateFromImage(img);
            }
        }
        foreach(var id in ConnectedWorld.RoomIds)
        {
            var at=G(ConnectedWorld.Origin(id)-_game.WorldOrigin);if(!WorldRectVisible(at,new(1536,1024)))continue;
            if(_worldFog.TryGetValue(id,out var fog))DrawTextureRect(fog,new Rect2(at,new Vector2(1536,1024)),false);
        }
        // The inter-room masonry has its own persistent exploration cells.
        foreach(var l in RoomLinks.All)
        {
            var route=ConnectedWorld.Route(l);
            for(int i=1;i<route.Length;i++)
            {
                var a=route[i-1];var b=route[i];var dir=NVec.Normalize(b-a);var side=ConnectedWorld.Side(a,b)*1.04f;
                int count=Math.Max(1,(int)MathF.Ceiling(NVec.Distance(a,b)/32));
                for(int j=0;j<count;j++)
                {
                    var p=NVec.Lerp(a,b,(float)j/count);var q=NVec.Lerp(a,b,(float)(j+1)/count);var mid=(p+q)/2;
                    var screen=G(mid-_game.WorldOrigin);if(!WorldRectVisible(screen-new Vector2(180,220),new(360,360)))continue;
                    bool visible=_game.CanSeeRoomPoint(mid-_game.WorldOrigin)||NVec.Distance(mid,_game.Player+_game.WorldOrigin)<70;
                    if(visible)continue;
                    bool seen=_game.Rooms!.CorridorSeen.Contains(l.Id+":"+i+":"+j);
                    if(ConnectedWorld.Painted(l)&&InsideArchPainting(l,mid))continue;
                    var delta=_game.WorldOrigin;
                    DrawColoredPolygon(new[]{G(p-side-delta),G(q-side-delta),G(q+side-delta),G(p+side-delta)},new Color(3/255f,5/255f,6/255f,seen?.62f:1));
                }
            }
        }
    }
}
