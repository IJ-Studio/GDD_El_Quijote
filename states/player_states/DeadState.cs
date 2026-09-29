// res://states/player_states/DeadState.cs
using Godot;

/// <summary>
/// Estado de muerte (Dead) del personaje.
/// Emite PlayerDied al EventBus.
/// </summary>
public class DeadState : IState
{
    private const string ANIM_DEAD = "death";
    private readonly Character _owner;

    public DeadState(Character owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_DEAD);
        _owner.Velocity = Vector2.Zero;
        EventBus.Instance.EmitPlayerDied(_owner);
    }

    public void Update(double delta) { }
    public void PhysicsUpdate(double delta) { }
    public void Exit() { }
}
