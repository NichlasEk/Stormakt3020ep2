using Godot;
using System.Collections.Generic;
using System.Text.Json;
namespace Atland;
public static class JourneyDialogue
{
    public static Dictionary<string,string[]> Meridian()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/meridian-radio.json"))!;
    public static Dictionary<string,string[]> Continuity()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/continuity-radio.json"))!;
    public static Dictionary<string,string[]> Uppsala()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/uppsala-radio.json"))!;
    public static Dictionary<string,string[]> Foundry()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/foundry-radio.json"))!;
    public static Dictionary<string,string[]> Mine()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/mine-radio.json"))!;
    public static Dictionary<string,string[]> Regiment()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/regiment-radio.json"))!;
    public static Dictionary<string,string[]> Roots()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/rootway-radio.json"))!;
    public static Dictionary<string,string[]> Archive()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/archive-radio.json"))!;
    public static Dictionary<string,string[]> Rooms()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/rooms-radio.json"))!;
    public static Dictionary<string,string[]> Load()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/journey-radio.json"))!;
}
