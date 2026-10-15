// res://states/enemy_states/EnemyDeadState.cs
using Godot;

/// <summary>
/// Estado de muerte para enemigos. Reproduce la animación de muerte,
/// notifica al EventBus y elimina al enemigo de la escena.
/// </summary>
public class EnemyDeadState : IState
{
    private readonly Enemy _owner;
    private const string ANIM_DEAD = "death";
    private bool _notified = false;

    public EnemyDeadState(Enemy owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_DEAD);
        _owner.Sprite.AnimationFinished += OnAnimationFinished;

        // Desactivar colisiones con jugadores; se queda sobre el piso
        _owner.CollisionLayer = 0;

        if (_owner.Hitbox != null) _owner.Hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        if (_owner.Hurtbox != null) _owner.Hurtbox.SetDeferred(Area2D.PropertyName.Monitorable, false);
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 500f * (float)delta);
        if (!_owner.IsOnFloor()) velocity.Y += Character.Gravity * (float)delta;
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit()
    {
        _owner.Sprite.AnimationFinished -= OnAnimationFinished;
    }

    private void OnAnimationFinished()
    {
        if (_owner.Sprite.Animation != ANIM_DEAD || _notified) return;
        _notified = true;
        EventBus.Instance.EmitEnemyKilled(_owner);

        // se desvanece antes de desaparecer
        var tween = _owner.CreateTween();
        tween.TweenInterval(0.6);
        tween.TweenProperty(_owner, "modulate:a", 0f, 0.4);
        tween.TweenCallback(Callable.From(_owner.QueueFree));
    }
}
