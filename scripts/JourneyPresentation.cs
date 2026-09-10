using Godot;
using Atland;
using System;
using System.Linq;
using NVec=System.Numerics.Vector2;

public partial class Main
{
    private AnimatedCast _animated=null!;
    private Texture2D _warehouse=null!,_shore=null!,_shoreRevealed=null!;
    private float _revealTime;
    private readonly System.Collections.Generic.HashSet<Region> _capturedRegions=new();
    private bool _revealCaptured;
    private void LoadJourney()
    {
        LoadCampaignWorlds();
        _animated=new AnimatedCast();
        _warehouse=GD.Load<Texture2D>("res://assets/art/warehouse-v1.png");
        _shore=GD.Load<Texture2D>("res://assets/art/shore-v1.png");
        _shoreRevealed=ResourceLoader.Exists("res://assets/art/shore-revealed-v1.png")?GD.Load<Texture2D>("res://assets/art/shore-revealed-v1.png"):_shore;
        foreach(var (id,line) in JourneyDialogue.Load().Concat(JourneyDialogue.Rooms()).Concat(JourneyDialogue.Archive()).Concat(JourneyDialogue.Roots()).Concat(JourneyDialogue.Regiment()).Concat(JourneyDialogue.Mine()).Concat(JourneyDialogue.Foundry()).Concat(JourneyDialogue.Uppsala()).Concat(JourneyDialogue.Continuity()).Concat(JourneyDialogue.Salt()).Concat(JourneyDialogue.West()).Concat(JourneyDialogue.Gamla()).Concat(JourneyDialogue.Observatory()).Concat(JourneyDialogue.Cabin()).Concat(JourneyDialogue.Meridian()))Radio[id]=(line[0]=="ebba"?"RIKSAMIRAL EBBA GRIP":line[0]=="elin"?"ELIN VINGE":line[0]=="saltwarden"?"SALTVÄKTAREN":line[0]=="officer"?"MÖNSTRINGSFÖRRÄTTAREN":line[0]=="nils"?"NILS BERG":line[0]=="marta"?"MÄRTA VINGE":line[0]=="meridian"?"MERIDIANVÄKTAREN":line[0]=="bailiff"?"KRONFOGDEN":line[0]=="arvid"?"KAPTEN ARVID SILFVERGREN":"ANTIKVARIE HEDVIG RÅLAMB",line[1]);
    }
    private void StartDuel()
    {
        _game=Combat.NewDuel();ApplyDeveloperSettings();_camera=G(_game.Player)+new Vector2(70,-70);
        _particles.Clear();_floating.Clear();_radioQueue.Clear();_radio="";_sound.StopVoice();
        ChangeScreen(Screen.Game);_banner="SABELDUELL";_bannerTime=5;
    }
    private string JourneyGoal=>_game.Region==Region.Warehouse
        ?!_game.WhetstoneTaken?"Undersök förrådskistan":!_game.ManifestTaken?"Läs kollegiets mätorder":!_game.WinchOpened?"Öppna strandgrinden":"Ta dig ut till stranden"
        :_game.AtlandRevealed?"Följ vägen till upphämtningen":_game.Surveyed.All(s=>s)?"Rikta kartan vid mittstenen":$"Mät strandens riktningar · {_game.Surveyed.Count(s=>s)}/3";
    private void DrawJourneyMarkers()
    {
        if(_revealTime>0)return;
        void Mark(NVec at,string name,bool done=false)
        {
            var p=G(at);DrawArc(p,25,0,Mathf.Tau,40,new Color(done?Muted:Gold,done?.3f:.8f),1.5f,true);
            Text(name,p+new Vector2(-40,38),11,done?Muted:Gold);
        }
        if(_game.Region==Region.Warehouse)
        {
            if(!_game.WhetstoneTaken)Mark(JourneyLayout.Whetstone,"FÖRRÅD");
            if(!_game.ManifestTaken)
            {
                var p=G(JourneyLayout.Manifest);DrawRect(new Rect2(p-new Vector2(12,15),new Vector2(24,18)),new Color("b7aa88"));
                for(int i=0;i<3;i++)DrawLine(p+new Vector2(-8,-10+i*4),p+new Vector2(6,-10+i*4),new Color("594f3e"),1);
                Mark(JourneyLayout.Manifest,"MÄTORDER");
            }
            Mark(JourneyLayout.Winch,"VINSCH",_game.WinchOpened);
            if(_game.WinchOpened)Mark(JourneyLayout.WarehouseExit,"STRANDEN");
        }
        else if(_game.Region==Region.Shore)
        {
            for(int i=0;i<3;i++)
            {
                Mark(JourneyLayout.Survey[i],new[]{"GRAVHÖGEN","VADSTÄLLET","FARLEDEN"}[i],_game.Surveyed[i]);
                if(_game.Surveyed[i])DrawLine(G(JourneyLayout.Survey[i]),G(JourneyLayout.Reveal),new Color(Gold,.28f),1,true);
            }
            if(_game.Surveyed.All(s=>s)&&!_game.AtlandRevealed)Mark(JourneyLayout.Reveal,"RIKTA KARTAN");
            if(_game.AtlandRevealed)Mark(JourneyLayout.ShoreExit,"UPPHÄMTNING");
        }
    }
    private void DrawJourneyPrompt(int foes)
    {
        if(_game.InRooms){DrawRoomPrompt();return;}
        if(_game.InCampaign){DrawCampaignPrompt(foes);return;}
        if(_game.Region==Region.Quay)return;
        if(NVec.Distance(_game.Player,_game.JourneyObjective)<90)
        {
            string prompt=foes>0?"Slå tillbaka vakterna":_game.Region==Region.Shore&&!_game.Surveyed.All(s=>s)?"Håll E / B · Mät riktningen":"E / B · "+JourneyGoal;
            Panel(new Rect2(405,411,470,64),.94f);Centered(prompt,640,439,16,Gold);
            if(_game.SurveyProgress>0)WorldBar(new Vector2(429,454),422,_game.SurveyProgress/2,Teal,5);
        }
        if(_game.RiposteTime>0)Centered("RIPOST REDO · NÄSTA SABELHUGG +60 %",640,391,15,Gold);
    }
    private void DrawExpeditionJournal()
    {
        Text("EXPEDITIONEN  /  VÄGEN UNDER VATTNET",new Vector2(95,78),13,Gold);
        Text("Kollegiets försprång",new Vector2(90,155),36,Pale,true);
        Wrapped(_game.ManifestTaken?"Mätordern anger tre riktningar: gravhögen, vadstället och farleden. Datumet är struket, men kollegiets sigill är färskt.":"Bronskartans nästa spår går genom kronans magasin till stranden.",new Vector2(95,222),1040,22,Pale,35);
        Wrapped(_game.WhetstoneTaken?"Ronnebys brynstål: en perfekt parad gör nästa sabelhugg 60 procent starkare i 2,6 sekunder.":"Förrådskistan kan innehålla något användbart för närstriden.",new Vector2(95,342),1040,20,Gold,32);
        Wrapped(_game.AtlandRevealed?"De tre riktningarna stämmer. Under vattnet löper en byggd väg mot en port. Nya röda mätband visar att kollegiet redan har varit här. Det här är expeditionens upptäckt — inte ett bevis för varje anspråk i Rudbecks bok.":$"Riktningar uppmätta: {_game.Surveyed.Count(s=>s)} av 3. Hedvig vill pröva kartan mot själva landskapet.",new Vector2(95,438),1040,20,Muted,32);
        Button(new Rect2(95,614,210,49),"Föregående","journal-prev");Button(new Rect2(320,614,210,49),"Nästa","journal-next");Button(new Rect2(830,614,340,49),"Tillbaka","back",true);
    }
}
