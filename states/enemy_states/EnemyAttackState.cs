// res://states/enemy_states/EnemyAttackState.cs
using Godot;

/// <summary>
/// Estado base de ataque para enemigos. Maneja la animación de ataque y la
/// activación de la Hitbox. Transiciona de vuelta a Guard al terminar.
/// </summary>
public class EnemyAttackState : IState
{
    protected readonly Enemy Owner;
    protected readonly string AnimName;
    protected readonly string ReturnState;

    public EnemyAttackState(Enemy owner, string animName = "attack", string returnState = "Guard")
    {
        Owner = owner;
        AnimName = animName;
        ReturnState = returnState;
    }

    public virtual void Enter()
    {
        Owner.Sprite.Play(AnimName);
        Owner.Sprite.AnimationFinished += OnAnimationFinished;
        if (Owner.Hitbox != null)
        {
            Owner.Hitbox.Monitoring = true;
        }
    }

    public virtual void Update(double delta) { }

    public virtual void PhysicsUpdate(double delta)
    {
        // Aplicar gravedad básica si es necesario
        if (!Owner.IsOnFloor())
        {
            Vector2 velocity = Owner.Velocity;
            velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
            Owner.Velocity = velocity;
            Owner.MoveAndSlide();
        }
    }

    public virtual void Exit()
    {
        Owner.Sprite.AnimationFinished -= OnAnimationFinished;
        if (Owner.Hitbox != null)
        {
            Owner.Hitbox.Monitoring = false;
        }
    }

    protected virtual void OnAnimationFinished()
    {
        if (Owner.Sprite.Animation == AnimName)
        {
            Owner.StateMachine.TransitionTo(ReturnState);
        }
    }
}
