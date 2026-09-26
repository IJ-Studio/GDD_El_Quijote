// res://states/player_states/IdleState.cs
using Godot;

/// <summary>
/// Estado de reposo (Idle) del jugador.
/// Transiciona a Run, Jump, Crouch, Attack o Block según el input.
/// </summary>
public class IdleState : IState
{
    private const string ANIM_IDLE = "idle";
    private readonly Character _owner;

    public IdleState(Character owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_IDLE);
        _owner.Velocity = new Vector2(0, _owner.Velocity.Y);
    }

    public void Update(double delta)
    {
        // Transiciones por input
        if (Input.IsActionPressed("player_attack") || Input.IsMouseButtonPressed(MouseButton.Left))
        {
            _owner.StateMachine.TransitionTo("Attack");
            return;
        }

        if (Input.IsActionPressed("player_block") || Input.IsMouseButtonPressed(MouseButton.Right))
        {
            _owner.StateMachine.TransitionTo("Block");
            return;
        }

        if (Input.IsActionJustPressed("player_jump") && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Jump");
            return;
        }

        if (Input.IsActionPressed("player_crouch") && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Crouch");
            return;
        }

        float direction = Input.GetAxis("player_left", "player_right");
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
