using Godot;

// res://states/player_states/RunState.cs
public class RunState : PlayerState
{
    private readonly Character _owner;

    public RunState(Character owner) => _owner = owner;

    public override void Enter()
    {
        _owner.Sprite.Play(ANIM_RUN);
    }

    public override void Update(double delta)
    {
        float inputAxis = Input.GetAxis("Player1_move_left", "Player1_move_right");

        if (Mathf.IsZeroApprox(inputAxis))
        {
            _owner.StateMachine.TransitionTo("Idle");
            return;
        }

        if (Input.IsActionJustPressed("Player1_jump") && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Jump");
            return;
        }

        if (!_owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Jump");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        float inputAxis = Input.GetAxis("Player1_move_left", "Player1_move_right");
        Vector2 velocity = _owner.Velocity;

        velocity.X = inputAxis * _owner.Speed;
        
        if (inputAxis != 0)
        {
            _owner.Sprite.FlipH = inputAxis < 0;
        }

        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public override void Exit() { }
}
