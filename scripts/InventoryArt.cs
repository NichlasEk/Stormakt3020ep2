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
        {
            int column=item.Id=="admiral-saber"?0:item.Id=="service-pistol"?1:item.Id=="admiral-coat"?2:-1;
            _inventoryItemArt.Add(item.Id,column<0?GD.Load<Texture2D>($"res://assets/art/inventory-item-{item.Id}-v1.png"):new AtlasTexture{Atlas=GD.Load<Texture2D>("res://assets/art/inventory-ebba-v1.png"),Region=column==0?new Rect2(10,105,490,740):column==1?new Rect2(515,250,500,620):new Rect2(1030,185,500,600)});
        }
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
