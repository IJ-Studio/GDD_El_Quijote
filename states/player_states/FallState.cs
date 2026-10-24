// res://states/player_states/FallState.cs
using Godot;

/// <summary>
/// Estado de caída (Fall) del jugador. Al tocar el piso vuelve a Idle.
/// </summary>
public partial class FallState : Node, IState
{
    private Character _owner = null!;

    public FallState() { }
    public FallState(Character owner) => _owner = owner;

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
        if (_owner.Sprite.SpriteFrames?.HasAnimation("fall") == true)
            _owner.Sprite.Play("fall");
        else
            _owner.Sprite.Play("jump");
    }

    public void Update(double delta)
    {
        if (_owner.IsActionJustPressed("attack") || (_owner.InputPrefix == "player1_" && Input.IsMouseButtonPressed(MouseButton.Left)))
        {
            _owner.StateMachine.TransitionTo("Attack");
            return;
        }

        // Coyote time: saltar justo después de dejar el borde todavía cuenta
        if (_owner.ConsumeJumpRequest())
        {
            _owner.StateMachine.TransitionTo("Jump");
            return;
        }

        if (_owner.IsOnFloor())
        {
            AudioManager.Instance?.PlaySfx("aterrizar", -12f);
            _owner.StateMachine.TransitionTo("Idle");
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
