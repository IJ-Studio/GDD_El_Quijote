// res://states/player_states/CrouchState.cs
using Godot;

/// <summary>
/// Estado de agachado (Crouch) del jugador.
/// </summary>
public class CrouchState : IState
{
    private const string ANIM_CROUCH = "crouch";
    private readonly Character _owner;

    public CrouchState(Character owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_CROUCH);
        _owner.Velocity = Vector2.Zero;
    }

    public void Update(double delta)
    {
        if (!Input.IsActionPressed("player_crouch") || !_owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }

    public void PhysicsUpdate(double delta) { }
    public void Exit() { }
}
