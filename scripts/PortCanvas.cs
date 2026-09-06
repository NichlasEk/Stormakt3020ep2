using Godot;
using System;

// Paints deterministic 2D modules once into a viewport texture, without any 3D scene.
public partial class PortCanvas : Node2D
{
    public Action<Node2D>? Paint;
    public override void _Draw()=>Paint?.Invoke(this);
}
