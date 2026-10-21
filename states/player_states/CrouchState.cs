// res://states/player_states/CrouchState.cs
using Godot;

/// <summary>
/// Estado de agachado (Crouch) del jugador.
/// </summary>
public partial class CrouchState : Node, IState
{
    private Character _owner = null!;

    public CrouchState() { }
    public CrouchState(Character owner) => _owner = owner;

    public override void _Ready()
    {
        if (_owner == null)
        {
            Node p = GetParent();
            while (p != null && p is not Character) p = p.GetParent();
            if (p is Character c) _owner = c;
        }
    }

    public void Enter()
    {
        _owner.Sprite.Play("crouch");
        _owner.Velocity = new Vector2(0, _owner.Velocity.Y);
    }

    public void Update(double delta)
    {
        if (!_owner.IsActionPressed("crouch") || !_owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        velocity.X = 0f;
        if (!_owner.IsOnFloor()) velocity.Y += Character.Gravity * (float)delta;
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
