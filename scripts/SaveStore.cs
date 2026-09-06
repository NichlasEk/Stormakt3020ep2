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
        if(game.Schema!=1 || !float.IsFinite(game.Health) || game.Health<0 || game.Health>100 || !float.IsFinite(game.Player.X) || !float.IsFinite(game.Player.Y) || game.Enemies.Count>1000 || !Enum.IsDefined(game.Phase) || !Enum.IsDefined(game.Order))throw new InvalidDataException("Ogiltigt speltillstånd");
        return game;
    }
}
