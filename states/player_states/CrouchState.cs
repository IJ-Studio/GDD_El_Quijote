using Godot;

// res://states/player_states/CrouchState.cs
public class CrouchState : PlayerState
{
    private readonly Character _owner;

    public CrouchState(Character owner) => _owner = owner;

    public override void Enter()
    {
        _owner.Sprite.Play(ANIM_CROUCH);
    }

    public override void Update(double delta)
    {
        if (!Input.IsActionPressed("Player1_crouch"))
        {
            _owner.StateMachine.TransitionTo("Idle");
            return;
        }

        if (Input.IsActionJustPressed("Player1_jump") && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Jump");
            return;
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        // Reducir velocidad horizontal al estar agachado (opcional)
        Vector2 velocity = _owner.Velocity;
        velocity.X = Mathf.MoveToward(_owner.Velocity.X, 0, _owner.Speed * 0.1f);
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public override void Exit() { }
}
