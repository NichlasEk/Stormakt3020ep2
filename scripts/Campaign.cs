using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;

public enum ExpeditionTask { Sequence, Archive, Beacons, Siege, Vents, ForgeBoss, Stars, Tribunal }
public sealed record ExpeditionStage(string Name,int World,ExpeditionTask Task,string Intro,string Goal,string Clue,string[] Labels,int[] Sequence);
public static class Expedition
{
    public static readonly Vector2 Entry=new(768,852),Exit=new(768,376);
    public static readonly Vector2[] Nodes={new(510,612),new(768,510),new(1026,612)};
    public static readonly Vector2[] Spawns={new(495,660),new(1026,635),new(768,450),new(850,738)};
    public static readonly Vector2 Preserve=new(620,715),Forge=new(916,715);
    public static readonly ExpeditionStage[] Stages={
        new("Atlands port",0,ExpeditionTask.Sequence,"Vägen slutar vid tre stenar. Porten öppnas för en berättelse, inte för en kung.","Öppna portens tre minnen","Landet bär vattnet. Vattnet bär minnet. Land → Vatten → Minne.",new[]{"VATTEN","MINNE","LAND"},new[]{2,0,1}),
        new("Minnets arkiv",0,ExpeditionTask.Archive,"Kollegiet har börjat katalogisera människorna som aldrig fick finnas. Deras passersedlar ligger bland vittnesmålen.","Samla arkivets tre avtryck","Efter fynden: bevara arkivet eller förfalska en passersedel. Valet följer expeditionen.",new[]{"NAMN","VITTNEN","PASSERSEDLAR"},new[]{0,1,2}),
        new("Rotvägen",1,ExpeditionTask.Beacons,"En rot från djupet håller den gamla landsvägen uppe. Någon har bundit regementets fanor kring den.","Tänd tre väglyktor","Håll E ostört vid varje lykta. Ljuset väcker rotvägens vakter.",new[]{"SÖDRA LYKTAN","ROTLYKTAN","NORRA LYKTAN"},new[]{0,1,2}),
        new("Regementet som stannar",1,ExpeditionTask.Siege,"De döda har fått order att återvända till tjänst. Deras kapten ber bara om att få stanna i jorden.","Bryt regementets tre mönstringar","Slå tillbaka tre vågor. Ring därefter i avlösningens klocka.",new[]{"VÄSTRA LEDEN","AVLÖSNING","ÖSTRA LEDEN"},new[]{1,0,2}),
        new("Järnets lungor",2,ExpeditionTask.Vents,"Bergslagens malm har en puls. Tre gamla ventiler matar en ugn som inte längre tillverkar järn.","Stäng de tre tryckventilerna","De röda cirklarna varnar för ångslag. Stäng ventilerna mellan slagen.",new[]{"VÄSTVENTIL","HJÄRTVENTIL","ÖSTVENTIL"},new[]{0,2,1}),
        new("Den tomma kronan",2,ExpeditionTask.ForgeBoss,"Kronfogden väger de fallnas eder i stället för malmen. En tom krona går runt i gjutformen.","Besegra kronfogden","Rensa smedjan, besegra kronfogden och ta kronans avtryck.",new[]{"SLAGG","KRONANS AVTRYCK","KEDJOR"},new[]{1,0,2}),
        new("Uppsalas felvända himmel",3,ExpeditionTask.Stars,"Instrumenten visar stjärnorna under horisonten. Hedvigs karta blir läsbar när hon vänder den upp och ned.","Rikta tre himmelsringar","Nordstjärnan först, sedan Månen och sist Solen. Läs namnen vid ringarna.",new[]{"SOLEN","NORDSTJÄRNAN","MÅNEN"},new[]{1,2,0}),
        new("Nornornas protokoll",3,ExpeditionTask.Tribunal,"Kollegiet har lämnat in en ansökan om att få äga rikets framtid. Nornorna har begärt komplettering: ett levande vittne.","Bryt kollegiets sista mönstring","Slå tillbaka väktarna och lägg expeditionens bevis vid vittnesbordet.",new[]{"EDER","VITTNESBORDET","DOMAR"},new[]{1,0,2})
    };
}

public sealed partial class Combat
{
    public bool AtlandCampaign,CampaignFinished;
    public int CampaignStage=-1,CampaignProgress,CampaignMask,CampaignWave,ArchiveChoice;
    public float CampaignChannel,CampaignClock,CampaignCooldown;
    [JsonIgnore] public bool InCampaign=>CampaignStage>=0;
    [JsonIgnore] public ExpeditionStage Stage=>Expedition.Stages[Math.Clamp(CampaignStage,0,Expedition.Stages.Length-1)];
    [JsonIgnore] public bool CampaignReady=>CampaignProgress>=3&&(Stage.Task!=ExpeditionTask.Archive||ArchiveChoice!=0);
    [JsonIgnore] public Vector2 CampaignObjective=>CampaignReady?Expedition.Exit:Stage.Task==ExpeditionTask.Archive&&CampaignProgress>=3?Expedition.Preserve:Stage.Task is ExpeditionTask.Siege or ExpeditionTask.ForgeBoss or ExpeditionTask.Tribunal?Expedition.Nodes[1]:Expedition.Nodes[Stage.Sequence[Math.Min(CampaignProgress,2)]];
    [JsonIgnore] public string CampaignGoal=>CampaignReady?CampaignStage==7?"Lämna det sista vittnesmålet":"Fortsätt till nästa område":Stage.Task==ExpeditionTask.Archive&&CampaignProgress>=3?"Välj arkivets öde":Stage.Task==ExpeditionTask.Siege?$"Bryt mönstringen · våg {CampaignWave}/3":$"{Stage.Goal} · {Math.Min(CampaignProgress,3)}/3";
    [JsonIgnore] public string CampaignEnding=>ArchiveChoice==1?"Arkivets namn finns hos fler än kronan. Nornorna tar emot de levandes vittnesmål och avslår kollegiets anspråk. Under Uppsala slocknar mönstringsljusen, ett efter ett.":"Passersedeln öppnade kollegiets portar. Nu ligger den falska handlingen bredvid de äkta vittnesmålen. Nornorna låter båda stå kvar: även lögnen berättar vem som försökte äga framtiden.";

    public static Combat NewAtland(Order order)
    {
        var game=New(order,true);game.AtlandCampaign=true;game.Testimony=TestimonyChoice.Cipher;
        foreach(var inscription in game.Inscriptions){inscription.Read=true;inscription.Progress=ReadingDuration;}
        game.Surveyed=new[]{true,true,true};game.AtlandRevealed=true;game.WhetstoneTaken=game.ManifestTaken=game.WinchOpened=true;
        game.EnterCampaign(0);return game;
    }
    private void EnterCampaign(int index)
    {
        AtlandCampaign=true;CampaignStage=index;CampaignProgress=CampaignMask=CampaignWave=0;
        CampaignChannel=CampaignClock=CampaignCooldown=0;BossEnraged=false;
        Region=Stage.World switch{0=>Region.Atland,1=>Region.Roots,2=>Region.Forge,_=>Region.Uppsala};Phase=Phase.Campaign;
        EnterRegion(Expedition.Entry,"");Health=Math.Max(Health,85);Potions=Math.Min(8,Potions+1);
        if(Stage.Task==ExpeditionTask.Siege)SpawnCampaignWave();
        else
        {
            SpawnExpedition(EnemyKind.Guard,Expedition.Spawns[0]);SpawnExpedition(EnemyKind.Gunner,Expedition.Spawns[1]);
            if(index>1&&ArchiveChoice!=2)SpawnExpedition(EnemyKind.Pikeman,Expedition.Spawns[2]);
            if(Stage.Task is ExpeditionTask.ForgeBoss or ExpeditionTask.Tribunal)
            {
                SpawnExpedition(EnemyKind.Collector,Expedition.Spawns[2]);var boss=Enemies[^1];boss.Health=boss.MaxHealth=Stage.Task==ExpeditionTask.Tribunal?520:440;
            }
        }
        Emit("campaign",Player,Stage.Intro);Emit("checkpoint",Player);
    }
    private void SpawnExpedition(EnemyKind kind,Vector2 at)
    {
        Spawn(kind,at);Enemies[^1].Cooldown=1.2f+(Enemies[^1].Id%4)*.18f;
    }
    private void SpawnCampaignWave()
    {
        CampaignWave++;
        SpawnExpedition(EnemyKind.Guard,Expedition.Spawns[(CampaignWave-1)%4]);SpawnExpedition(CampaignWave==2?EnemyKind.Gunner:EnemyKind.Pikeman,Expedition.Spawns[(CampaignWave+1)%4]);
        if(CampaignWave==3&&ArchiveChoice!=2)SpawnExpedition(EnemyKind.Guard,Expedition.Spawns[1]);
        Emit("inscription",Player,$"MÖNSTRING {CampaignWave}/3");Emit("checkpoint",Player);
    }
    private void StepCampaign(Controls input,float dt)
    {
        if(!InCampaign||CampaignFinished)return;
        CampaignClock+=dt;CampaignCooldown=Math.Max(0,CampaignCooldown-dt);
        bool peaceful=Enemies.All(e=>e.Dead);
        bool use=input.Interact&&!Moving&&AttackTime<=0&&DodgeTime<=0&&!Guarding&&Hurt<=0&&CampaignCooldown<=0;
        bool Near(Vector2 p)=>Vector2.Distance(Player,p)<72;
        if(Stage.Task==ExpeditionTask.Siege&&peaceful&&CampaignWave<3){SpawnCampaignWave();return;}
        if(Stage.Task==ExpeditionTask.Tribunal&&!peaceful&&CampaignWave==0&&Enemies.Any(e=>e.Kind==EnemyKind.Collector&&e.Health<e.MaxHealth*.5f))
        {CampaignWave=1;SpawnExpedition(EnemyKind.Guard,Expedition.Spawns[0]);if(ArchiveChoice!=2)SpawnExpedition(EnemyKind.Gunner,Expedition.Spawns[3]);Emit("campaign",Player,"Kollegiet kallar sitt sista vittne. Håll vägen till bordet fri.");Emit("checkpoint",Player);}
        if(Stage.Task==ExpeditionTask.Vents&&!CampaignReady&&(int)(CampaignClock/4)!=(int)((CampaignClock-dt)/4))
        {int valve=(int)(CampaignClock/4)%3;if((CampaignMask&(1<<valve))==0)Hazards.Add(new(){Position=Expedition.Nodes[valve],Timer=1.4f,Radius=76});}
        if(!use){CampaignChannel=0;return;}
        if(CampaignReady&&peaceful&&Near(Expedition.Exit))
        {
            if(CampaignStage+1<Expedition.Stages.Length)EnterCampaign(CampaignStage+1);
            else{CampaignFinished=true;Phase=Phase.Complete;Shots.Clear();Hazards.Clear();Emit("campaign",Player,CampaignEnding);Emit("checkpoint",Player);}
            return;
        }
        if(!peaceful){CampaignChannel=0;return;}
        if(Stage.Task==ExpeditionTask.Archive&&CampaignProgress>=3&&ArchiveChoice==0)
        {
            if(Near(Expedition.Preserve))ArchiveChoice=1;else if(Near(Expedition.Forge))ArchiveChoice=2;else return;
            if(ArchiveChoice==1){Health=Math.Min(100,Health+15);Potions++;}else SupportCooldown=0;
            Emit("campaign",Player,ArchiveChoice==1?"Arkivet förblir helt. Ebba sprider namnen; fler kommer att försöka stoppa er.":"Passersedeln bär nu kollegiets eget sigill. Färre vakter känner igen expeditionen framöver.");
            Emit("checkpoint",Player);return;
        }
        if(CampaignReady)return;
        if(Stage.Task is ExpeditionTask.Siege or ExpeditionTask.ForgeBoss or ExpeditionTask.Tribunal)
        {
            if(!Near(Expedition.Nodes[1]))return;
            CampaignProgress=3;CampaignMask=7;Emit("inscription",Player,Stage.Labels[1]);Emit("checkpoint",Player);return;
        }
        int node=Array.FindIndex(Expedition.Nodes,Near);if(node<0||(CampaignMask&(1<<node))!=0)return;
        if(node!=Stage.Sequence[CampaignProgress])
        {
            if(Stage.Task is ExpeditionTask.Sequence or ExpeditionTask.Stars)
            {CampaignProgress=CampaignMask=0;CampaignCooldown=1;Emit("campaign",Player,"Mekanismen svarar inte. "+Stage.Clue);}
            return;
        }
        CampaignChannel+=dt;float duration=Stage.Task is ExpeditionTask.Beacons or ExpeditionTask.Vents?2.4f:.65f;
        if(CampaignChannel<duration)return;
        CampaignChannel=0;CampaignProgress++;CampaignMask|=1<<node;CampaignCooldown=.25f;
        Emit("inscription",Player,Stage.Labels[node]);
        if(Stage.Task is ExpeditionTask.Beacons or ExpeditionTask.Vents)
        {SpawnExpedition(EnemyKind.Guard,Expedition.Spawns[(node+1)%4]);if(CampaignProgress==2)SpawnExpedition(EnemyKind.Gunner,Expedition.Spawns[node]);}
        Emit("checkpoint",Player);
    }
}
