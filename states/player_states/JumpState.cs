// res://states/player_states/JumpState.cs
using Godot;

/// <summary>
/// Estado de salto (Jump) del jugador.
/// </summary>
public partial class JumpState : Node, IState
{
    private const string ANIM_JUMP = "jump";
    private Character _owner = null!;
    [Export] private float _jumpForce = -400f;

    public JumpState() { }
    public JumpState(Character owner) => _owner = owner;

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
        _owner.Sprite.Play(ANIM_JUMP);
        Vector2 velocity = _owner.Velocity;
        velocity.Y = _jumpForce;
        _owner.Velocity = velocity;
    }

    public void Update(double delta)
    {
        if (_owner.IsActionPressed("attack") || (_owner.InputPrefix == "player1_" && Input.IsMouseButtonPressed(MouseButton.Left)))
        {
            _owner.StateMachine.TransitionTo("Attack");
            return;
        }

        if (_owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }

    public void PhysicsUpdate(double delta)
    {
        float direction = _owner.GetAxis("left", "right");
        Vector2 velocity = _owner.Velocity;
        velocity.X = direction * _owner.Speed;
        velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
