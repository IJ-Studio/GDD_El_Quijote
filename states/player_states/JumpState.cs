// res://states/player_states/JumpState.cs
using Godot;

/// <summary>
/// Estado de salto (Jump) del jugador.
/// </summary>
public class JumpState : IState
{
    private const string ANIM_JUMP = "jump";
    private readonly Character _owner;
    [Export] private float _jumpForce = -400f;

    public JumpState(Character owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_JUMP);
        Vector2 velocity = _owner.Velocity;
        velocity.Y = _jumpForce;
        _owner.Velocity = velocity;
    }

    public void Update(double delta)
    {
        if (Input.IsActionPressed("player_attack") || Input.IsMouseButtonPressed(MouseButton.Left))
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
        float direction = Input.GetAxis("player_left", "player_right");
        Vector2 velocity = _owner.Velocity;
        velocity.X = direction * _owner.Speed;
        velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
