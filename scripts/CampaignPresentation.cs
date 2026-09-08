using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;

public partial class Main
{
    private readonly Texture2D[] _campaignWorlds=new Texture2D[4];
    private string _campaignText="";
    private float _campaignTextTime;
    private void LoadCampaignWorlds()
    {
        LoadPort();
        var names=new[]{"atland","roots","forge","uppsala"};
        for(int i=0;i<4;i++)_campaignWorlds[i]=GD.Load<Texture2D>($"res://assets/art/world-{names[i]}-v1.png");
    }
    private void StartAtland()
    {
        _foundrySlot=false;_mineSlot=false;_regimentSlot=false;
        _doorSlot=false;
        _atlandSlot=true;
        if(!_testMode&&System.IO.File.Exists(SavePath)){ResumeSave();return;}
        _game=Combat.NewAtland(_order);ApplyDeveloperSettings();_particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        _revealTime=0;_camera=G(_game.Player)+new Vector2(0,-60);RememberRenderPositions();ChangeScreen(Screen.Game);
        foreach(var cue in _game.Events)HandleCue(cue);Save();
    }
    private void DrawCampaignMarkers()
    {
        void Marker(NVec at,string label,bool done,Color color)
        {
            var p=G(at);var rim=new[]{p+new Vector2(-27,0),p+new Vector2(0,-16),p+new Vector2(27,0),p+new Vector2(0,16)};
            DrawColoredPolygon(rim,new Color(.06f,.066f,.063f,.92f));DrawPolyline(rim.Append(rim[0]).ToArray(),new Color(color,done?.35f:.8f),2,true);
            DrawArc(p,12,0,Mathf.Tau,24,new Color(color,done?.35f:.8f),1,true);
            Text(label,p+new Vector2(-55,38),11,done?Muted:color);
        }
        bool single=_game.Stage.Task is ExpeditionTask.Siege or ExpeditionTask.ForgeBoss or ExpeditionTask.Tribunal;
        for(int i=0;i<3;i++)if(!single||i==1)Marker(Expedition.Nodes[i],_game.Stage.Labels[i],(_game.CampaignMask&(1<<i))!=0,Gold);
        if(_game.Stage.Task==ExpeditionTask.Archive&&_game.CampaignProgress==3&&_game.ArchiveChoice==0)
        {Marker(Expedition.Preserve,"BEVARA ARKIVET",false,Teal);Marker(Expedition.Forge,"FÖRFALSKA PASSET",false,Gold);}
        if(_game.CampaignReady)Marker(Expedition.Exit,_game.CampaignStage==7?"VITTNESMÅLET":"VÄGEN VIDARE",false,Teal);
    }
    private void DrawCampaignPrompt(int foes)
    {
        bool nearby=NVec.Distance(_game.Player,_game.CampaignObjective)<90;
        if(_game.Stage.Task==ExpeditionTask.Archive&&_game.CampaignProgress==3&&_game.ArchiveChoice==0)
            nearby|=NVec.Distance(_game.Player,Expedition.Forge)<90;
        bool cache=_game.CampaignStage==0&&!_game.PortCacheTaken&&NVec.Distance(_game.Player,PortLayout.Cache)<72&&_game.ClearPath(_game.Player,PortLayout.Cache);
        if(!nearby&&!cache)return;
        string message=foes>0?"Slå tillbaka väktarna":_game.CampaignReady?"E / B · Följ vägen vidare":"Håll E / B · Undersök";
        if(foes==0&&_game.Stage.Task==ExpeditionTask.Archive&&_game.CampaignProgress==3&&_game.ArchiveChoice==0)
            message=NVec.Distance(_game.Player,Expedition.Forge)<90?"E / B · Förfalska passet · färre vakter":"E / B · Bevara arkivet · extra förråd";
        if(cache)message=foes>0?"Skapa arbetsro vid gömman":"E / B · Undersök murarens gömma";
        Panel(new Rect2(390,412,500,59),.94f);Centered(message,640,442,17,Gold);
        if(_game.CampaignChannel>0)WorldBar(new Vector2(410,455),460,_game.CampaignChannel/(_game.Stage.Task is ExpeditionTask.Beacons or ExpeditionTask.Vents?2.4f:.65f),Teal,4);
    }
    private void DrawCampaignStory()
    {
        if(_campaignTextTime<=0||string.IsNullOrEmpty(_campaignText))return;
        Panel(new Rect2(255,535,795,97),.93f);Text("EXPEDITIONENS ANTECKNINGAR",new Vector2(278,558),12,Gold);
        Wrapped(_campaignText,new Vector2(278,583),750,16,Pale,23);
    }
    private void DrawCampaignJournal()
    {
        if(_game.InRooms){DrawRoomJournal();return;}
        DrawRect(new Rect2(0,0,1280,720),new Color(.025f,.023f,.019f,.96f));
        Text($"EXPEDITIONEN / {_game.CampaignStage+1} AV {Expedition.Stages.Length}",new Vector2(95,78),13,Gold);
        Text(_game.Stage.Name,new Vector2(90,145),34,Pale,true);
        Wrapped(_game.Stage.Intro,new Vector2(95,205),1040,22,Pale,34);
        Text("LEDTRÅD",new Vector2(95,333),13,Gold);Wrapped(_game.Stage.Clue,new Vector2(95,373),1040,22,Gold,34);
        Wrapped(_game.ArchiveChoice==0?"Arkivets öde är ännu inte avgjort.":_game.ArchiveChoice==1?"Arkivet är bevarat. Expeditionen fick extra förråd; kollegiet skickar fler väktare.":"Det förfalskade passet avleder patruller. Färre förstärkningar väntar längs färden.",new Vector2(95,487),1040,19,Muted,30);
        if(_game.CampaignStage==0)Wrapped(_game.PortCacheTaken?"Murarens gömma: avtrycket och två tinkturer är säkrade.":"En liten förrådskista står vid gårdens västra kant. Någon har lämnat den där.",new Vector2(95,566),1040,16,Gold,24);
        Button(new Rect2(830,614,340,49),"Tillbaka","back",true);
    }
    private void DrawCampaignEnding()
    {
        DrawRect(new Rect2(0,0,1280,720),new Color(.012f,.032f,.044f,.94f));
        Text("EXPEDITIONEN / NORNORNAS PROTOKOLL",new Vector2(95,100),14,Gold);
        Text("ETT LEVANDE VITTNE",new Vector2(90,193),46,Pale,true);
        Wrapped(_game.CampaignEnding,new Vector2(95,262),1050,24,Pale,38);
        Text($"Åtta banor genom fyra världar · {_game.Kills} besegrade · {_game.Parries} parader",new Vector2(95,458),18,Gold);
        Wrapped("Riket finns kvar. Anspråket på dess framtid gör det inte. Ebba ber Karl komma upp till ytan innan någon hinner inventera honom.",new Vector2(95,506),1050,19,Muted,30);
        Button(new Rect2(95,620,360,49),"Till huvudmenyn","title",true);
    }
}
