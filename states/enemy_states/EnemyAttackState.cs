// res://states/enemy_states/EnemyAttackState.cs
using Godot;

/// <summary>
/// Estado base de ataque para enemigos. Maneja la animación de ataque y la
/// activación de la Hitbox. Transiciona de vuelta a Guard al terminar
/// y reinicia el tiempo de espera entre ataques.
/// </summary>
public class EnemyAttackState : IState
{
    protected readonly Enemy Owner;
    protected readonly string AnimName;
    protected readonly string ReturnState;
    private bool _listening;

    public EnemyAttackState(Enemy owner, string animName = "attack", string returnState = "Guard")
    {
        Owner = owner;
        AnimName = animName;
        ReturnState = returnState;
    }

    public virtual void Enter()
    {
        Owner.Velocity = new Vector2(0, Owner.Velocity.Y);
        Owner.Sprite.Play(AnimName);
        Listen(true);
        Owner.Hitbox?.SetDeferred(Area2D.PropertyName.Monitoring, true);
    }

    public virtual void Update(double delta) { }

    public virtual void PhysicsUpdate(double delta)
    {
        Vector2 velocity = Owner.Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 600f * (float)delta);
        if (!Owner.IsOnFloor())
            velocity.Y += Character.Gravity * (float)delta;
        Owner.Velocity = velocity;
        Owner.MoveAndSlide();
    }

    public virtual void Exit()
    {
        Listen(false);
        Owner.CooldownLeft = Owner.GuardCooldown;
        if (Owner.Hitbox != null)
        {
            Owner.Hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        }
    }

    /// <summary>Conecta/desconecta AnimationFinished sin duplicar ni quitar conexiones inexistentes.</summary>
    protected void Listen(bool on)
    {
        if (on == _listening) return;
        if (on) Owner.Sprite.AnimationFinished += OnAnimationFinished;
        else Owner.Sprite.AnimationFinished -= OnAnimationFinished;
        _listening = on;
    }

    protected virtual void OnAnimationFinished()
    {
        if (Owner.Sprite.Animation == AnimName)
        {
            Owner.StateMachine.TransitionTo(Owner.IsAdrenalineActive ? "Alert" : ReturnState);
        }
    }
}
