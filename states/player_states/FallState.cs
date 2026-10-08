// res://states/player_states/FallState.cs
using Godot;

/// <summary>
/// Estado de caída (Fall) del jugador.
/// </summary>
public partial class FallState : Node, IState
{
    private const string ANIM_FALL = "fall";
    private Character _owner = null!;

    public FallState() { }
    public FallState(Character owner) => _owner = owner;

    public override void _Ready()
    {
        if (_owner == null)
        {
            Node p = GetParent();
            while (p != null && p is not Character)
            {
                p = p.GetParent();
            }
            if (p is Character c) _owner = c;
        }
    }

    public void Enter()
    {
        if (_owner?.Sprite.SpriteFrames?.HasAnimation(ANIM_FALL) == true)
        {
            _owner.Sprite.Play(ANIM_FALL);
        }
        else
        {
            _owner?.Sprite.Play("jump");
        }
    }

    public void Update(double delta)
    {
        if (_owner?.IsOnFloor() == true)
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }

    public void PhysicsUpdate(double delta)
    {
        if (_owner == null) return;
        float direction = _owner.GetAxis("left", "right");
        Vector2 velocity = _owner.Velocity;
        velocity.X = direction * _owner.Speed;
        velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
