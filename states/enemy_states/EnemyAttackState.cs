// res://states/enemy_states/EnemyAttackState.cs
using Godot;

/// <summary>
/// Estado base de ataque para enemigos, en tres tiempos:
/// 1. Preparación (windup): el enemigo se queda quieto y parpadea en rojo para avisar.
///    Si el jugador le pega en este momento, el ataque se cancela (Enemy.TakeDamage).
/// 2. Golpe (Strike): al llegar al cuadro <see cref="StrikeFrame"/> de la animación se activa
///    la Hitbox o se lanza el proyectil. Solo ocurre una vez por ataque.
/// 3. Recuperación: termina la animación y vuelve a Guard/Alert con su tiempo de espera.
/// Las subclases cambian el golpe sobrescribiendo <see cref="Strike"/> y el movimiento
/// del ataque con <see cref="ActivePhysics"/>.
/// </summary>
public class EnemyAttackState : IState
{
    protected readonly Enemy Owner;
    protected string AnimName;
    protected readonly string ReturnState;
    private bool _listening;

    /// <summary>Cuadro de la animación de ataque en el que sale el golpe.</summary>
    protected int StrikeFrame { get; set; } = 1;

    private static readonly Color TelegraphColor = new(1f, 0.45f, 0.35f);
    private float _windupLeft;
    private bool _struck;

    /// <summary>True mientras el enemigo se prepara (todavía no hace daño).</summary>
    protected bool InWindup => _windupLeft > 0f;

    public EnemyAttackState(Enemy owner, string animName = "attack", string returnState = "Guard")
    {
        Owner = owner;
        AnimName = animName;
        ReturnState = returnState;
    }

    public virtual void Enter()
    {
        _struck = false;
        Owner.Velocity = new Vector2(0, Owner.Velocity.Y);
        Owner.Sprite.Play(AnimName);
        Listen(true);

        // Con la adrenalina la animación va más rápido, y el aviso también se acorta
        float speed = Mathf.Max(0.1f, Owner.Sprite.SpeedScale);
        _windupLeft = Owner.AttackWindup / speed;
        if (InWindup)
        {
            Owner.Sprite.Pause();
            Owner.Sprite.Frame = 0;
        }
        else
        {
            CheckStrike();
        }
    }

    public virtual void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        if (InWindup)
        {
            _windupLeft -= (float)delta;
            Telegraph(_windupLeft > 0f);
            if (!InWindup) Owner.Sprite.Play(AnimName);

            Vector2 velocity = Owner.Velocity;
            velocity.X = Mathf.MoveToward(velocity.X, 0, 900f * (float)delta);
            if (!Owner.IsOnFloor())
                velocity.Y += Character.Gravity * (float)delta;
            Owner.Velocity = velocity;
            Owner.MoveAndSlide();
            return;
        }

        CheckStrike();
        ActivePhysics(delta);
    }

    /// <summary>Movimiento durante el golpe (por defecto frena). La embestida lo sobrescribe.</summary>
    protected virtual void ActivePhysics(double delta)
    {
        Vector2 velocity = Owner.Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 600f * (float)delta);
        if (!Owner.IsOnFloor())
            velocity.Y += Character.Gravity * (float)delta;
        Owner.Velocity = velocity;
        Owner.MoveAndSlide();
    }

    /// <summary>El golpe en sí. Por defecto activa la Hitbox cuerpo a cuerpo.</summary>
    protected virtual void Strike()
    {
        Owner.Hitbox?.SetDeferred(Area2D.PropertyName.Monitoring, true);
    }

    private void CheckStrike()
    {
        if (_struck) return;
        int frames = Owner.Sprite.SpriteFrames?.GetFrameCount(AnimName) ?? 1;
        if (Owner.Sprite.Frame < Mathf.Min(StrikeFrame, frames - 1)) return;
        _struck = true;
        Strike();
    }

    /// <summary>Parpadeo rojo de aviso mientras se prepara el ataque.</summary>
    private void Telegraph(bool on)
    {
        if (Owner.IsInvulnerable) return; // el parpadeo de daño tiene prioridad
        bool blink = on && Mathf.PosMod(_windupLeft, 0.12f) > 0.05f;
        Owner.Sprite.Modulate = blink ? TelegraphColor : Colors.White;
    }

    public virtual void Exit()
    {
        Listen(false);
        _windupLeft = 0f;
        Telegraph(false);
        Owner.CooldownLeft = Owner.GuardCooldown;
        Owner.Hitbox?.SetDeferred(Area2D.PropertyName.Monitoring, false);
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
