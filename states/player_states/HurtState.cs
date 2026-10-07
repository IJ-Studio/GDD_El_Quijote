// res://states/player_states/HurtState.cs
using Godot;

/// <summary>
/// Estado de retroceso (Hurt) al recibir daño.
/// Reproduce la animación de daño y regresa a Idle al finalizar.
/// </summary>
public partial class HurtState : Node, IState
{
    private const string ANIM_HURT = "hurt";
    private Character _owner = null!;

    public HurtState() { }
    public HurtState(Character owner) => _owner = owner;

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
        if (_owner.Sprite.SpriteFrames?.HasAnimation(ANIM_HURT) == true)
        {
            _owner.Sprite.Play(ANIM_HURT);
        }
        else
        {
            _owner.Sprite.Play("idle");
        }
        _owner.Sprite.AnimationFinished += OnAnimationFinished;
        _owner.Velocity = new Vector2(-Mathf.Sign(_owner.Scale.X) * 50f, _owner.Velocity.Y);
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 200f * (float)delta);
        if (!_owner.IsOnFloor())
        {
            velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
        }
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit()
    {
        _owner.Sprite.AnimationFinished -= OnAnimationFinished;
    }

    private void OnAnimationFinished()
    {
        if (_owner.Sprite.Animation == ANIM_HURT || _owner.Sprite.Animation == "idle")
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }
}
