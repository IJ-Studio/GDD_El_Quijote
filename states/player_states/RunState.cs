// res://states/player_states/RunState.cs
using Godot;

/// <summary>
/// Estado de carrera (Run) del jugador.
/// </summary>
public class RunState : IState
{
    private const string ANIM_RUN = "run";
    private readonly Character _owner;

    public RunState(Character owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_RUN);
    }

    public void Update(double delta)
    {
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

        float direction = Input.GetAxis("player_left", "player_right");
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
        float direction = Input.GetAxis("player_left", "player_right");
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
