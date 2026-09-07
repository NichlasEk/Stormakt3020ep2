using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace Atland;

public static class SaveStore
{
    private static readonly JsonSerializerOptions Options=new() { IncludeFields=true,WriteIndented=true };
    private sealed record Envelope(int Version,string Checksum,string Payload);
    public static void Write(string path,Combat game)
    {
        var data=JsonSerializer.Serialize(game,Options);
        var envelope=new Envelope(1,Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data))),data);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary=path+".tmp";
        using(var file=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None))
        {var bytes=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope,Options));file.Write(bytes);file.Flush(true);}
        if(File.Exists(path))File.Copy(path,path+".bak",true);
        File.Move(temporary,path,true);
    }
    public static Combat Read(string path)
    {
        try{return ReadOne(path);}
        catch(Exception e) when(e is IOException or JsonException or InvalidDataException or ArgumentException)
        {if(File.Exists(path+".bak"))return ReadOne(path+".bak");throw;}
    }
    private static Combat ReadOne(string path)
    {
        var env=JsonSerializer.Deserialize<Envelope>(File.ReadAllText(path),Options)??throw new InvalidDataException("Tom sparfil");
        if(env.Version!=1)throw new InvalidDataException("Okänd sparversion");
        if(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(env.Payload)))!=env.Checksum)throw new InvalidDataException("Skadad sparfil");
        var game=JsonSerializer.Deserialize<Combat>(env.Payload,Options)??throw new InvalidDataException("Saknat speltillstånd");
        if(game.Inventory is null)throw new InvalidDataException("Saknat inventarium");
        game.Inventory.Validate();
        if(game.Schema!=1 || !float.IsFinite(game.Health) || game.Health<0 || game.Health>100 || !float.IsFinite(game.Player.X) || !float.IsFinite(game.Player.Y) || game.Enemies.Count>1000 || !Enum.IsDefined(game.Phase) || !Enum.IsDefined(game.Order))throw new InvalidDataException("Ogiltigt speltillstånd");
        if(game.Inscriptions is null || game.Inscriptions.Count!=3 || !Enum.IsDefined(game.Testimony)
            || game.Inscriptions.Any(i=>i is null || !float.IsFinite(i.Progress) || i.Progress<0 || i.Progress>Combat.ReadingDuration
                || !float.IsFinite(i.Position.X) || !float.IsFinite(i.Position.Y) || (i.Read&&i.Progress<Combat.ReadingDuration)))
            throw new InvalidDataException("Ogiltiga vittnesmål");
        if(game.Phase is Phase.Testimony or Phase.Extraction && !game.Inscriptions.All(i=>i.Read))throw new InvalidDataException("Saknade vittnesmål");
        if(game.Phase==Phase.Extraction && game.Testimony==TestimonyChoice.None)throw new InvalidDataException("Saknat vägval");
        if(!Enum.IsDefined(game.Region)||game.Surveyed is null||game.Surveyed.Length!=3
            ||!float.IsFinite(game.SurveyProgress)||game.SurveyProgress<0||game.SurveyProgress>2
            ||game.SurveyIndex < -1||game.SurveyIndex>2||!float.IsFinite(game.RiposteTime)||game.RiposteTime<0||game.RiposteTime>2.6f)
            throw new InvalidDataException("Ogiltig expedition");
        if(game.Region!=Region.Quay&&(!game.ExtendedJourney||!game.Inscriptions.All(i=>i.Read)||game.Testimony==TestimonyChoice.None))
            throw new InvalidDataException("Saknat underlag för expeditionen");
        if(game.AtlandRevealed&&((game.Region!=Region.Shore&&!game.InCampaign)||!game.Surveyed.All(s=>s)))throw new InvalidDataException("Ofullständig mätning");
        if(game.CampaignStage < -1||game.CampaignStage>=Expedition.Stages.Length||game.CampaignProgress<0||game.CampaignProgress>3
            ||game.CampaignMask<0||game.CampaignMask>7||game.CampaignWave<0||game.CampaignWave>3||game.ArchiveChoice<0||game.ArchiveChoice>2
            ||!float.IsFinite(game.CampaignChannel)||game.CampaignChannel<0||game.CampaignChannel>2.5f
            ||!float.IsFinite(game.CampaignClock)||game.CampaignClock<0||!float.IsFinite(game.CampaignCooldown)||game.CampaignCooldown<0||game.CampaignCooldown>1.1f)
            throw new InvalidDataException("Ogiltig Atland-expedition");
        if(game.InCampaign&&System.Numerics.BitOperations.PopCount((uint)game.CampaignMask)!=game.CampaignProgress)throw new InvalidDataException("Motsägande expeditionsfynd");
        if(game.InCampaign&&(!game.AtlandCampaign||!game.AtlandRevealed||game.Region!=(Region)((int)Region.Atland+game.Stage.World)
            ||(game.CampaignStage>=2&&game.ArchiveChoice==0)||(!game.CampaignFinished&&game.Phase!=Phase.Campaign)))throw new InvalidDataException("Ofullständig Atland-expedition");
        if(game.CampaignFinished&&(!game.InCampaign||game.CampaignStage!=7||game.Phase!=Phase.Complete||!game.CampaignReady))throw new InvalidDataException("Ogiltigt expeditionsslut");
        // Older port snapshots may stand beyond the restored painted courtyard bounds.
        if(game.CampaignStage==0){game.Player=game.Bound(game.Player);foreach(var foe in game.Enemies)foe.Position=game.Bound(foe.Position);}
        return game;
    }
}
