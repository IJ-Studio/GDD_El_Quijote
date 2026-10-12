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

    public EnemyDeadState(Enemy owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_DEAD);
        _owner.Sprite.AnimationFinished += OnAnimationFinished;
        
        // Desactivar colisiones para evitar que el cadáver estorbe
        _owner.CollisionLayer = 0;
        _owner.CollisionMask = 0;
        
        if (_owner.Hitbox != null) _owner.Hitbox.Monitoring = false;
        if (_owner.Hurtbox != null) _owner.Hurtbox.Monitoring = false;
    }

    public void Update(double delta) { }
    public void PhysicsUpdate(double delta) { }

    public void Exit()
    {
        _owner.Sprite.AnimationFinished -= OnAnimationFinished;
    }

    private void OnAnimationFinished()
    {
        if (_owner.Sprite.Animation == ANIM_DEAD)
        {
            EventBus.Instance.EmitEnemyKilled(_owner);
            _owner.QueueFree();
        }
    }
}
