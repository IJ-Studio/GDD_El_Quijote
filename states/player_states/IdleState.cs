using Godot;

/// <summary>
/// Estado base para los estados de Don Quijote para compartir constantes de animación.
/// </summary>
public abstract class PlayerState : IState
{
    protected const string ANIM_IDLE = "idle";
    protected const string ANIM_RUN = "run";
    protected const string ANIM_JUMP = "jump";
    protected const string ANIM_CROUCH = "crouch";
    protected const string ANIM_FALL = "fall";

    public abstract void Enter();
    public abstract void Exit();
    public abstract void PhysicsUpdate(double delta);
    public abstract void Update(double delta);
}

// res://states/player_states/IdleState.cs
public class IdleState : PlayerState
{
    private readonly Character _owner;

    public IdleState(Character owner) => _owner = owner;

    public override void Enter()
    {
        _owner.Sprite.Play(ANIM_IDLE);
    }

    public override void Update(double delta)
    {
        if (Input.IsActionPressed("Player1_move_left") || Input.IsActionPressed("Player1_move_right"))
        {
            _owner.StateMachine.TransitionTo("Run");
            return;
        }

        if (Input.IsActionJustPressed("Player1_jump") && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Jump");
            return;
        }

        if (Input.IsActionPressed("Player1_crouch") && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Crouch");
            return;
        }

        if (!_owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Jump");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        // Aplicar fricción o detener movimiento
        Vector2 velocity = _owner.Velocity;
        velocity.X = Mathf.MoveToward(_owner.Velocity.X, 0, _owner.Speed);
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public override void Exit() { }
}
