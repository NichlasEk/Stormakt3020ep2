using Godot;
using Atland;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Main
{
    private bool _doorSlot,_doorChecks;
    private Texture2D? _doorGround,_doorFace;
    private void LoadDoorArt(){_doorGround??=GD.Load<Texture2D>("res://assets/art/door-courtyard-v1.png");_doorFace??=GD.Load<Texture2D>("res://assets/art/door-oak-face-v1.png");}
    private void StartDoorTrial(bool fresh=false)
    {
        _foundrySlot=false;_mineSlot=false;_regimentSlot=false;
        LoadDoorArt();LoadWaterArt();_doorSlot=true;_roomsSlot=_portSlot=_atlandSlot=false;
        if(!fresh&&!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewDoorTrial(_order);ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_sound.StopVoice();_radio="";
        _bannerTime=_campaignTextTime=_revealTime=0;_camera=G(_game.Player)+new Vector2(60,-80);RememberRenderPositions();ChangeScreen(Screen.Game);Save();
    }
    private void DrawDoorMarkers()
    {
        if(!_game.DoorTest!.KeyTaken&&_game.CanSeeRoomPoint(DoorTrialLayout.Key))
        {
            var p=G(DoorTrialLayout.Key);DrawArc(p,18,0,Mathf.Tau,24,new Color(Gold,.5f),1,true);
            DrawArc(p-new Vector2(7,0),5,0,Mathf.Tau,16,Gold,2,true);DrawLine(p-new Vector2(2,0),p+new Vector2(13,0),Gold,3,true);DrawLine(p+new Vector2(9,0),p+new Vector2(9,5),Gold,2,true);
        }
    }
    private static Vector2[] ClipDoorStrip(Vector2[] poly,float boundary,bool greater)
    {
        var result=new List<Vector2>();var a=poly[^1];bool ai=greater?a.X>=boundary:a.X<=boundary;
        foreach(var b in poly){bool bi=greater?b.X>=boundary:b.X<=boundary;if(ai!=bi)result.Add(a+(b-a)*((boundary-a.X)/(b.X-a.X)));if(bi)result.Add(b);a=b;ai=bi;}return result.ToArray();
    }
    private void AddDoorLayers(List<(float Depth,Action Draw)> layers)
    {
        LoadDoorArt();
        // Repaint the original wall silhouette in narrow depth slices: an actor can stand either side.
        var walls=new[]{new Vector2[]{new(171,184),new(724,408),new(726,529),new(700,552),new(178,292)},new Vector2[]{new(850,490),new(872,472),new(1536,830),new(1536,1000),new(848,635)}};
        foreach(var wall in walls)for(float x=wall.Min(p=>p.X);x<wall.Max(p=>p.X);x+=16)
        {
            var strip=ClipDoorStrip(ClipDoorStrip(wall,x,true),x+16,false);if(strip.Length<3)continue;
            float depth=548+(x+8-714)*79/141;layers.Add((depth,()=>PaintForeground(_doorGround!,strip)));
        }
        var d=_game.DoorTest!.Door;var h=G(DoorTrialLayout.Hinge);var tip=G(d.Tip);
        AddOakFrame(layers);
        if(d.Broken)
        {
            for(int i=0;i<6;i++)
            {int n=i;var at=h+new Vector2(25+i*21,24+i*10);layers.Add((at.Y,()=>DrawPolygon(new[]{at,at+new Vector2(51,-13),at+new Vector2(60,-6),at+new Vector2(5,10)},new[]{new Color(.65f,.61f,.54f)},new[]{new Vector2(n/6f,1),new Vector2(n/6f,0),new Vector2((n+1)/6f,0),new Vector2((n+1)/6f,1)},_doorFace)));}return;
        }
        // Extrude the painted leaf in the same fixed isometric ground projection as its hinge.
        // The complete visual thickness stays inside the existing 30px collision envelope.
        float angle=d.Openness*Mathf.Pi/2;
        var thickness=new Vector2(-14*(Mathf.Cos(angle)-Mathf.Sin(angle)),7.85f*(Mathf.Cos(angle)+Mathf.Sin(angle)));
        var half=thickness/2;var up=new Vector2(0,-124);
        float tint=d.Health<90?.76f:.94f;
        // Paint each solid face as a whole. Independently sorting thin strips made
        // neighboring top/side faces overwrite one another and produced a sawtooth edge.
        var backA=h-half;var backB=tip-half;var frontA=h+half;var frontB=tip+half;
        var uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
        float leafDepth=(h.Y+tip.Y)/2;
        layers.Add((leafDepth-half.Y,()=>DrawPolygon(new[]{backA+up,backB+up,backB,backA},new[]{new Color(tint*.68f,tint*.65f,tint*.58f)},uv,_doorFace)));
        layers.Add((leafDepth+half.Y,()=>DrawPolygon(new[]{frontA+up,frontB+up,frontB,frontA},new[]{new Color(tint,tint*.96f,tint*.88f)},uv,_doorFace)));
        layers.Add((leafDepth+half.Y+.01f,()=>DrawPolygon(new[]{backA+up,backB+up,frontB+up,frontA+up},new[]{new Color(tint*.85f,tint*.81f,tint*.72f)},new[]{new Vector2(0,.94f),new Vector2(1,.94f),new Vector2(1,.98f),new Vector2(0,.98f)},_doorFace)));

        foreach(var end in new[]{h,tip})
        {
            var back=end-half;var front=end+half;
            layers.Add((front.Y+.02f,()=>DrawPolygon(new[]{back+up,front+up,front,back},new[]{new Color(tint*.64f,tint*.58f,tint*.49f)},new[]{new Vector2(.43f,0),new Vector2(.47f,0),new Vector2(.47f,1),new Vector2(.43f,1)},_doorFace)));
        }
    }
    private void AddOakFrame(List<(float Depth,Action Draw)> layers)
    {
        var h=G(DoorTrialLayout.Hinge);var end=G(DoorTrialLayout.ClosedTip);
        var along=(end-h).Normalized()*7;var across=new Vector2(-9,5);
        var wood=new[]{new Vector2(.49f,.08f),new Vector2(.55f,.08f),new Vector2(.55f,.94f),new Vector2(.49f,.94f)};
        foreach(var center in new[]{h-along*.25f,end+along*.25f})
        {
            var a=center-along-across;var b=center+along-across;var c=center+along+across;var d=center-along+across;var up=new Vector2(0,-137);
            layers.Add((center.Y+across.Y,()=>
            {
                DrawPolygon(new[]{d+up,c+up,c,d},new[]{new Color(.63f,.59f,.50f)},wood,_doorFace);
                DrawPolygon(new[]{b+up,c+up,c,b},new[]{new Color(.44f,.41f,.35f)},wood,_doorFace);
                DrawPolygon(new[]{a+up,b+up,c+up,d+up},new[]{new Color(.70f,.65f,.54f)},wood,_doorFace);
            }));
        }
        // Fixed iron hinge barrels remain attached to the jamb while the leaf turns.
        layers.Add((h.Y+9,()=>
        {
            foreach(float height in new[]{31f,95f})
            {
                var at=h+new Vector2(4,-height);DrawLine(at-new Vector2(0,7),at+new Vector2(0,7),new Color("181917"),6,true);
                DrawLine(at-new Vector2(1,6),at+new Vector2(-1,6),new Color("686251"),1.3f,true);
            }
        }));
    }
    private void DrawDoorJournal()
    {
        Panel(new Rect2(100,110,1080,490),.98f);Centered("Dörrprovet",640,160,32,Pale,true);
        Wrapped("Förgården och logementet ligger i samma spelmiljö. E / B öppnar eller stänger dörren. Gå genom öppningen med vanliga rörelsekontroller. Vakten är aktiv även på andra sidan och kan följa efter.",new Vector2(150,220),970,21,Pale,34);
        Wrapped("Nyckeln ligger på gården. Trädörren kan också slås sönder med sabel eller hammare. En förstörd dörr går inte att stänga igen. Dörrbladet stannar om någon står i dess väg.",new Vector2(150,350),970,21,Muted,34);
        Centered("Separat sparning för provet · Esc / B återgår till spelet",640,530,18,Gold);
        Button(new Rect2(460,555,360,38),"Tillbaka","back");
    }
    private void DrawDoorHelp()
    {
        var d=_game.DoorTest!.Door;bool near=DoorTrialLayout.Distance(_game.Player,DoorTrialLayout.Hinge,d.Tip)<85||DoorTrialLayout.Distance(_game.Player,DoorTrialLayout.Hinge,DoorTrialLayout.ClosedTip)<85;
        Panel(new Rect2(235,537,810,61),.92f);
        string label=near?(d.Broken?"Dörren är sönderslagen":d.Locked?"E / B · lås upp   |   Hugg / hammare · bryt upp":$"E / B · {(d.TargetOpen?"stäng":"öppna")}   |   Gå genom öppningen"):_game.DoorTest.EnteredLodge?"Vakten kan följa efter. Stäng dörren från öppningen.":d.TargetOpen?"Gå in genom öppningen. Vakten kan följa efter.":_game.DoorTest.KeyTaken?"Öppna dörren och gå igenom. Vakten kan följa efter.":"E / B vid nyckeln på gården · eller hugg sönder dörren";
        Centered(label,640,562,17,Gold);Centered("WASD · gå    Mus / J · hugg    Höger mus / K · tungt slag    Tab · byt vapen",640,586,13,Muted);
        if(near&&!d.Broken)WorldBar(new Vector2(542,525),196,d.Health/180,Gold);
    }
}
