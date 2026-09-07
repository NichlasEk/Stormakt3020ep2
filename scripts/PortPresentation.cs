using Godot;
using Atland;
using System;
using System.Collections.Generic;

public partial class Main
{
    private Texture2D _portProps=null!;
    private void LoadPort()=>_portProps=GD.Load<Texture2D>("res://assets/art/port-props-v1.png");
    private void AddPortLayers(List<(float Depth,Action Draw)> layers)
    {
        // Small 2D find on the original painted courtyard; the background supplies the architecture.
        var at=G(PortLayout.Cache);
        layers.Add((at.Y,()=>
        {
            var source=new Rect2(671,895,225,212);var size=source.Size*(55/source.Size.Y);
            DrawTextureRectRegion(_portProps,new Rect2(at-new Vector2(size.X/2,size.Y-3),size),source,
                _game.PortCacheTaken?new Color(.5f,.5f,.46f):Colors.White);
        }));
    }
}
