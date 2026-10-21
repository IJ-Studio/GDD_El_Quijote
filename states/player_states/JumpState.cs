// res://states/player_states/JumpState.cs
using Godot;

/// <summary>
/// Estado de salto (Jump) del jugador. Pasa a Fall al empezar a caer.
/// </summary>
public partial class JumpState : Node, IState
{
    private Character _owner = null!;
    [Export] private float _jumpForce = -470f;
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
        _owner.Sprite.Play("jump");
        Vector2 velocity = _owner.Velocity;
        velocity.Y = _jumpForce;
        _owner.Velocity = velocity;
        AudioManager.Instance?.PlaySfx("salto", -10f);
    }

    public void Update(double delta)
    {
        if (_owner.IsActionJustPressed("attack") || (_owner.InputPrefix == "player1_" && Input.IsMouseButtonPressed(MouseButton.Left)))
        {
            _owner.StateMachine.TransitionTo("Attack");
            return;
        }

        if (_owner.Velocity.Y >= 0f)
        {
            _owner.StateMachine.TransitionTo(_owner.IsOnFloor() ? "Idle" : "Fall");
        }
    }

    public void PhysicsUpdate(double delta)
    {
        float direction = _owner.GetAxis("left", "right");
        if (!Mathf.IsZeroApprox(direction)) _owner.SetFacing(direction < 0 ? -1 : 1);
        Vector2 velocity = _owner.Velocity;
        velocity.X = direction * _owner.Speed;
        velocity.Y += Character.Gravity * (float)delta;
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
