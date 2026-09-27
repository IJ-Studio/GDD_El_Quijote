// res://states/player_states/AttackState.cs
using Godot;

/// <summary>
/// Estado de ataque del jugador. Activa la Hitbox solo durante los frames de ataque
/// y transiciona automáticamente a Idle cuando termina la animación.
/// </summary>
public class AttackState : IState
{
    private const string ANIM_ATTACK = "attack";
    private readonly Character _owner;

    public AttackState(Character owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_ATTACK);
        _owner.Sprite.AnimationFinished += OnAnimationFinished;
        if (_owner.Hitbox is not null)
        {
            _owner.Hitbox.Monitoring = true;
        }
        _owner.Velocity = new Vector2(0, _owner.Velocity.Y);
    }

    public void Update(double delta) { }
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

    public void Exit()
    {
        _owner.Sprite.AnimationFinished -= OnAnimationFinished;
        if (_owner.Hitbox is not null)
        {
            _owner.Hitbox.Monitoring = false;
        }
    }

    private void OnAnimationFinished()
    {
        if (_owner.Sprite.Animation == ANIM_ATTACK)
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }
}
