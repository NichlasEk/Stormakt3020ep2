using Godot;
using Atland;
using System.Collections.Generic;

public partial class Main
{
    private Texture2D? _inventoryBackground;
    private readonly Dictionary<string,Texture2D> _inventoryItemArt=new();
    private void LoadInventoryArt()
    {
        if(_inventoryBackground!=null)return;
        _inventoryBackground=GD.Load<Texture2D>("res://assets/art/inventory-background-v1.png");
        foreach(var item in Items.All)
            _inventoryItemArt.Add(item.Id,GD.Load<Texture2D>($"res://assets/art/inventory-item-{item.Id}-v1.png"));
    }
    private void PaintedInventoryItem(ItemDefinition item,Vector2 center,float size)
    {
        var texture=_inventoryItemArt[item.Id];
        // Preserve the painted silhouette and its deliberate dark card background.
        var dimensions=texture.GetSize();var fitted=dimensions*(size/Mathf.Max(dimensions.X,dimensions.Y));
        DrawTextureRect(texture,new Rect2(center-fitted/2,fitted),false);
    }
    private void InventoryPanel(Rect2 area,float opacity)
    {
        DrawRect(area,new Color(.024f,.025f,.021f,opacity));
        DrawRect(area,new Color(.51f,.43f,.28f,.42f),false,1);
    }
}
