using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Atland;
public enum GearSlot { Saber, Hammer, Armor, Helmet, Sigil }
public sealed record ItemDefinition(string Id,string Name,GearSlot Slot,string Description,int Damage=0,int Armor=0,int Recovery=0);
public static class Items
{
    public const int BagCapacity=24,StashCapacity=60;
    public static readonly ItemDefinition[] All={
        new("saber","Officerssabel",GearSlot.Saber,"Karls tjänstevapen. Välbalanserat stål från Ronneby."),
        new("hammer","Gruvhammare",GearSlot.Hammer,"Tungt järn för berg och envist motstånd."),
        new("coat","Sliten uniformsrock",GearSlot.Armor,"Blå vadmal. Bär spåren av landstigningen."),
        new("helmet","Karolinerhjälm",GearSlot.Helmet,"Mörknat stål med en enkel pannplåt.",Armor:4),
        new("padded","Vadderad vapenrock",GearSlot.Armor,"Lager av linne under den blå rocken.",Armor:6),
        new("ward","Väktarens sigill",GearSlot.Sigil,"En gammal ed håller handen stadig.",Recovery:3),
        new("quay-saber","Likvarvets klinga",GearSlot.Saber,"En smal klinga med varvets stämpel.",Damage:5),
        new("iron-cap","Bergsmannens järnhuva",GearSlot.Helmet,"En bucklig hjälm från gruvornas vaktlag.",Armor:7),
        new("brigandine","Kollegiets brigantin",GearSlot.Armor,"Järnplåtar sydda innanför mörkt läder.",Armor:12),
        new("memory","Minnets sigill",GearSlot.Sigil,"Tre namn ligger gömda i bronsens ådror.",Damage:3,Recovery:2),
        new("forge-hammer","Kronfogdens hammare",GearSlot.Hammer,"Järnet minns varje ed som slagits sönder.",Damage:10),
        new("atland-saber","Atlands edsklinga",GearSlot.Saber,"En åldrad klinga som fortfarande håller sin egg.",Damage:9),
        new("crown-helm","Den tomma kronans hjälm",GearSlot.Helmet,"Rangtecknet är bortslipat. Skyddet består.",Armor:10),
        new("norn","Nornans vittnessigill",GearSlot.Sigil,"En levande människas namn, ännu inte avslutat.",Armor:4,Recovery:6)
    };
    public static ItemDefinition Get(string id)=>All.First(i=>i.Id==id);
    public static bool Known(string? id)=>All.Any(i=>i.Id==id);
    public static string SlotName(GearSlot slot)=>slot switch {GearSlot.Saber=>"Sabel",GearSlot.Hammer=>"Hammare",GearSlot.Armor=>"Rustning",GearSlot.Helmet=>"Hjälm",_=>"Sigill"};
}
public sealed class GearItem { public int Id;public string Definition="";[JsonIgnore] public ItemDefinition Data=>Items.Get(Definition); }
public sealed class ItemDrop { public GearItem Item=new();public Vector2 Position;public Region Region;public int Stage=-1;public string Room=""; }
public sealed class InventoryState
{
    public int NextId=7;
    public List<GearItem> Bag=new(){new(){Id=4,Definition="helmet"},new(){Id=5,Definition="padded"},new(){Id=6,Definition="ward"}};
    public List<GearItem> Stash=new();
    public Dictionary<GearSlot,GearItem> Equipped=new(){[GearSlot.Saber]=new(){Id=1,Definition="saber"},[GearSlot.Hammer]=new(){Id=2,Definition="hammer"},[GearSlot.Armor]=new(){Id=3,Definition="coat"}};
    public List<ItemDrop> Drops=new();
    public GearItem Create(string definition)=>new(){Id=NextId++,Definition=definition};
    public void Validate()
    {
        if(Bag is null||Stash is null||Equipped is null||Drops is null||Bag.Count>Items.BagCapacity||Stash.Count>Items.StashCapacity||Equipped.Count>5||Drops.Count>500)
            throw new System.IO.InvalidDataException("Ogiltigt inventarium");
        if(!Equipped.ContainsKey(GearSlot.Saber)||!Equipped.ContainsKey(GearSlot.Hammer)||Drops.Any(d=>d is null))throw new System.IO.InvalidDataException("Saknad utrustning");
        var all=Bag.Concat(Stash).Concat(Equipped.Values).Concat(Drops.Select(d=>d.Item)).ToArray();
        if(all.Any(i=>i is null||i.Id<1||!Items.Known(i.Definition))||all.Select(i=>i.Id).Distinct().Count()!=all.Length||NextId<=all.Max(i=>i.Id)||NextId>1000000)
            throw new System.IO.InvalidDataException("Ogiltiga föremål");
        if(Equipped.Any(e=>!Enum.IsDefined(e.Key)||e.Value.Data.Slot!=e.Key)||Drops.Any(d=>d.Room is null||(d.Room!=""&&!PortRooms.Known(d.Room))||!Enum.IsDefined(d.Region)||d.Stage < -1||d.Stage>7||!float.IsFinite(d.Position.X)||!float.IsFinite(d.Position.Y)))
            throw new System.IO.InvalidDataException("Ogiltiga utrustningsplatser");
    }
}
public sealed partial class Combat
{
    public InventoryState Inventory=new();
    [JsonIgnore] public GearSlot ActiveWeaponSlot=>Weapon==Weapon.Saber?GearSlot.Saber:GearSlot.Hammer;
    [JsonIgnore] public string WeaponName=>Inventory.Equipped[ActiveWeaponSlot].Data.Name;
    [JsonIgnore] public float EquipmentArmor=>Math.Min(60,Inventory.Equipped.Values.Sum(i=>i.Data.Armor));
    [JsonIgnore] public float EquipmentRecovery=>Inventory.Equipped.Values.Sum(i=>i.Data.Recovery);
    [JsonIgnore] public float AttackDamage=>(Weapon==Weapon.Saber?25:40)+Inventory.Equipped.Where(e=>e.Key==ActiveWeaponSlot||e.Key==GearSlot.Sigil).Sum(e=>e.Value.Data.Damage);
    [JsonIgnore] public bool CanUseStash=>!Dead&&EncounterEnemies.All(e=>e.Dead)&&Shots.All(s=>s.Reflected)&&Hazards.All(h=>h.Friendly)&&AttackTime<=0&&DodgeTime<=0;
    [JsonIgnore] public IEnumerable<ItemDrop> LocalDrops=>Inventory.Drops.Where(d=>d.Region==Region&&d.Stage==CampaignStage&&(InConnectedWorld?d.Room!="":d.Room==(Rooms?.Current??"")));
    public string EquipItem(int id)
    {
        if(Dead||AttackTime>0||DodgeTime>0)return "Avsluta rörelsen innan du byter utrustning.";
        var item=Inventory.Bag.FirstOrDefault(i=>i.Id==id);if(item is null)return "Föremålet finns inte i väskan.";
        int index=Inventory.Bag.IndexOf(item);var slot=item.Data.Slot;
        if(Inventory.Equipped.TryGetValue(slot,out var previous))Inventory.Bag[index]=previous;else Inventory.Bag.RemoveAt(index);
        Inventory.Equipped[slot]=item;return "";
    }
    public string UnequipItem(GearSlot slot)
    {
        if(slot is GearSlot.Saber or GearSlot.Hammer)return "Byt vapen genom att utrusta ett annat i samma vapenslag.";
        if(Dead||AttackTime>0||DodgeTime>0)return "Avsluta rörelsen först.";
        if(!Inventory.Equipped.TryGetValue(slot,out var item))return "Platsen är redan tom.";
        if(Inventory.Bag.Count>=Items.BagCapacity)return "Väskan är full.";
        Inventory.Equipped.Remove(slot);Inventory.Bag.Add(item);return "";
    }
    public string TransferItem(int id,bool toStash)
    {
        if(!CanUseStash)return "Förrådet kan användas när området är säkrat.";
        var source=toStash?Inventory.Bag:Inventory.Stash;var target=toStash?Inventory.Stash:Inventory.Bag;
        var item=source.FirstOrDefault(i=>i.Id==id);if(item is null)return "Föremålet finns inte här.";
        if(target.Count>=(toStash?Items.StashCapacity:Items.BagCapacity))return toStash?"Förrådet är fullt.":"Väskan är full.";
        source.Remove(item);target.Add(item);return "";
    }
    public void DropItem(string definition,Vector2 at)
    {
        if(Duel)return;
        Inventory.Drops.Add(new(){Item=Inventory.Create(definition),Position=Bound(at),Region=Region,Stage=CampaignStage,Room=Rooms?.Current??""});
        Emit("loot",at,Items.Get(definition).Name);Emit("checkpoint",Player);
    }
    private void DropEnemyLoot(Fighter enemy)
    {
        if(Duel)return;
        if(enemy.Kind==EnemyKind.Collector)DropItem(CampaignStage==7?"norn":CampaignStage==5?"forge-hammer":"brigandine",enemy.Position);
        else if(Kills%3==0)
        {
            var ids=new[]{"quay-saber","iron-cap","memory","atland-saber","crown-helm","padded"};
            DropItem(ids[(Kills/3-1)%ids.Length],enemy.Position);
        }
    }
    public string PickUpItem(int id)
    {
        var drop=LocalDrops.FirstOrDefault(d=>d.Item.Id==id);
        if(drop is null||Vector2.Distance(Player,drop.Position)>65||!ClearPath(Player,drop.Position))return "Kom närmare fyndet.";
        if(Dead||Moving||AttackTime>0||DodgeTime>0||Guarding||Hurt>0)return "Stanna för att plocka upp fyndet.";
        if(Inventory.Bag.Count>=Items.BagCapacity)return "Väskan är full. Flytta något till förrådet.";
        Inventory.Bag.Add(drop.Item);Inventory.Drops.Remove(drop);Emit("pickup",Player,drop.Item.Data.Name);Emit("checkpoint",Player);return "";
    }
}
