// res://states/player_states/RunState.cs
using Godot;

/// <summary>
/// Estado de carrera (Run) del jugador.
/// </summary>
public partial class RunState : Node, IState
{
    private Character _owner = null!;

    public RunState() { }
    public RunState(Character owner) => _owner = owner;

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
        _owner.Sprite.Play("run");
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

        if (_owner.ConsumeJumpRequest())
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
        if (Mathf.IsZeroApprox(direction))
        {
            _owner.StateMachine.TransitionTo("Idle");
            return;
        }

        _owner.SetFacing(direction < 0 ? -1 : 1);
    }

    public void PhysicsUpdate(double delta)
    {
        float direction = _owner.GetAxis("left", "right");
        Vector2 velocity = _owner.Velocity;
        velocity.X = direction * _owner.Speed;

        if (!_owner.IsOnFloor())
        {
            velocity.Y += Character.Gravity * (float)delta;
        }

        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
