using Godot;
using Atland;
using System.Collections.Generic;

public partial class Main
{
    private Vector2 _previousPlayer;
    private Vector2 _playerWalkFacing=new(1,1);
    private readonly Dictionary<Fighter,Vector2> _walkFacing=new();
    private Vector2 WalkFacing(Fighter actor)
    {
        var old=_walkFacing.TryGetValue(actor,out var value)?value:G(actor.Facing);
        var wanted=actor.MoveDirection.LengthSquared()>.001f?actor.MoveDirection:actor.Facing;
        return _walkFacing[actor]=G(Gait.Facing(wanted,N(old)));
    }
    private Vector2 PlayerWalkFacing()=>_playerWalkFacing=G(Gait.Facing(_game.MoveDirection,N(_playerWalkFacing)));
    private readonly Dictionary<Fighter,Vector2> _previousActors=new();
    private void RememberRenderPositions()
    {
        _previousPlayer=G(_game.Player);_previousActors.Clear();
        foreach(var actor in new List<Fighter>(_walkFacing.Keys))if(!_game.Enemies.Contains(actor))_walkFacing.Remove(actor);
        foreach(var actor in _game.Enemies)_previousActors[actor]=G(actor.Position);
    }
    private Vector2 BlendPosition(Vector2 before,Vector2 now)=>before.DistanceSquaredTo(now)>6400?now:before.Lerp(now,(float)Engine.GetPhysicsInterpolationFraction());
    private Vector2 RenderPlayer=>_screen==Screen.Game?BlendPosition(_previousPlayer,G(_game.Player)):G(_game.Player);
    private Vector2 RenderPosition(Fighter actor)=>_screen==Screen.Game&&_previousActors.TryGetValue(actor,out var before)?BlendPosition(before,G(actor.Position)):G(actor.Position);
}
