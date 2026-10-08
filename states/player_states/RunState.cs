// res://states/player_states/RunState.cs
using Godot;

/// <summary>
/// Estado de carrera (Run) del jugador.
/// </summary>
public partial class RunState : Node, IState
{
    private const string ANIM_RUN = "run";
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
        _owner.Sprite.Play(ANIM_RUN);
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

        float direction = _owner.GetAxis("left", "right");
        if (Mathf.IsZeroApprox(direction))
        {
            _owner.StateMachine.TransitionTo("Idle");
            return;
        }

        if (direction < 0) _owner.Sprite.FlipH = true;
        else if (direction > 0) _owner.Sprite.FlipH = false;
    }

    public void PhysicsUpdate(double delta)
    {
        float direction = _owner.GetAxis("left", "right");
        Vector2 velocity = _owner.Velocity;
        velocity.X = direction * _owner.Speed;

        if (!_owner.IsOnFloor())
        {
            velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
        }

        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
