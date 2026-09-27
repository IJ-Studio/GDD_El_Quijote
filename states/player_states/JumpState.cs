using Godot;

// res://states/player_states/JumpState.cs
public class JumpState : PlayerState
{
    private readonly Character _owner;
    private const float JUMP_VELOCITY = -400.0f;
    private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public JumpState(Character owner) => _owner = owner;

    public override void Enter()
    {
        if (_owner.IsOnFloor())
        {
            Vector2 velocity = _owner.Velocity;
            velocity.Y = JUMP_VELOCITY;
            _owner.Velocity = velocity;
        }
        _owner.Sprite.Play(ANIM_JUMP);
    }

    public override void Update(double delta)
    {
        if (_owner.IsOnFloor() && _owner.Velocity.Y >= 0)
        {
            if (Mathf.IsZeroApprox(Input.GetAxis("Player1_move_left", "Player1_move_right")))
                _owner.StateMachine.TransitionTo("Idle");
            else
                _owner.StateMachine.TransitionTo("Run");
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;

        // Gravedad
        if (!_owner.IsOnFloor())
        {
            velocity.Y += _gravity * (float)delta;
            
            // Animación de caída
            if (velocity.Y > 0 && _owner.Sprite.Animation != ANIM_FALL)
            {
                _owner.Sprite.Play(ANIM_FALL);
            }
        }

        // Movimiento aéreo (opcionalmente más lento o con inercia)
        float inputAxis = Input.GetAxis("Player1_move_left", "Player1_move_right");
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
