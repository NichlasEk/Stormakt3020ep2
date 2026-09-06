using Godot;
using Atland;
using System.Collections.Generic;

public partial class Main
{
    private Vector2 _previousPlayer;
    private readonly Dictionary<Fighter,Vector2> _previousActors=new();
    private void RememberRenderPositions()
    {
        _previousPlayer=G(_game.Player);_previousActors.Clear();
        foreach(var actor in _game.Enemies)_previousActors[actor]=G(actor.Position);
    }
    private Vector2 BlendPosition(Vector2 before,Vector2 now)=>before.DistanceSquaredTo(now)>6400?now:before.Lerp(now,(float)Engine.GetPhysicsInterpolationFraction());
    private Vector2 RenderPlayer=>_screen==Screen.Game?BlendPosition(_previousPlayer,G(_game.Player)):G(_game.Player);
    private Vector2 RenderPosition(Fighter actor)=>_screen==Screen.Game&&_previousActors.TryGetValue(actor,out var before)?BlendPosition(before,G(actor.Position)):G(actor.Position);
}
