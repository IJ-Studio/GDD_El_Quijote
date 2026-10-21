// res://states/player_states/IdleState.cs
using Godot;

/// <summary>
/// Estado de reposo (Idle) del jugador.
/// Transiciona a Run, Jump, Fall, Crouch, Attack o Block según el input.
/// </summary>
public partial class IdleState : Node, IState
{
    private Character _owner = null!;

    public IdleState() { }
    public IdleState(Character owner) => _owner = owner;

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
        _owner.Sprite.Play("idle");
        _owner.Velocity = new Vector2(0, _owner.Velocity.Y);
    }

    public void Update(double delta)
    {
        if (_owner.IsActionJustPressed("attack") || (_owner.InputPrefix == "player1_" && Input.IsMouseButtonPressed(MouseButton.Left)))
        {
            _owner.StateMachine.TransitionTo("Attack");
            return;
        }

        if (_owner.IsActionPressed("block") || (_owner.InputPrefix == "player1_" && Input.IsMouseButtonPressed(MouseButton.Right)))
        {
            _owner.StateMachine.TransitionTo("Block");
            return;
        }

        if (_owner.IsActionJustPressed("jump") && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Jump");
            return;
        }

        if (!_owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Fall");
            return;
        }

        if (_owner.IsActionPressed("crouch"))
        {
            _owner.StateMachine.TransitionTo("Crouch");
            return;
        }

        float direction = _owner.GetAxis("left", "right");
        if (!Mathf.IsZeroApprox(direction))
        {
            _owner.StateMachine.TransitionTo("Run");
        }
    }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 1200f * (float)delta);
        if (!_owner.IsOnFloor())
        {
            velocity.Y += Character.Gravity * (float)delta;
        }
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
