// res://states/player_states/IdleState.cs
using Godot;

/// <summary>
/// Estado de reposo (Idle) del jugador.
/// Transiciona a Run, Jump, Crouch, Attack o Block según el input.
/// </summary>
public partial class IdleState : Node, IState
{
    private const string ANIM_IDLE = "idle";
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
        _owner.Sprite.Play(ANIM_IDLE);
        _owner.Velocity = new Vector2(0, _owner.Velocity.Y);
    }

    public void Update(double delta)
    {
        if (_owner.IsActionPressed("attack") || (_owner.InputPrefix == "player1_" && Input.IsMouseButtonPressed(MouseButton.Left)))
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

        if (_owner.IsActionPressed("crouch") && _owner.IsOnFloor())
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
        if (!_owner.IsOnFloor())
        {
            velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
        }
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
