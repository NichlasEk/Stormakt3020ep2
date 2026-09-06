using Godot;
using System.Collections.Generic;
using System.Text.Json;
namespace Atland;
public static class JourneyDialogue
{
    public static Dictionary<string,string[]> Load()=>JsonSerializer.Deserialize<Dictionary<string,string[]>>(FileAccess.GetFileAsString("res://assets/story/journey-radio.json"))!;
}
