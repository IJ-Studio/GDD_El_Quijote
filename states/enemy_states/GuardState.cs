// res://states/enemy_states/GuardState.cs
using Godot;

/// <summary>
/// Estado de guardia para enemigos. Patrulla una distancia corta y, cuando
/// un jugador entra en su zona de guardia (GuardZone), lo encara y ataca
/// en cuanto termina su tiempo de espera (GuardCooldown).
/// </summary>
public class GuardState : IState
{
    private readonly Enemy _owner;
    private readonly string _attackState;
    private int _patrolDir = -1;

    public GuardState(Enemy owner, string attackState = "EnemyAttack")
    {
        _owner = owner;
        _attackState = attackState;
    }

    public void Enter()
    {
        _owner.Sprite.Play("idle");
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        if (!_owner.IsOnFloor())
            velocity.Y += Character.Gravity * (float)delta;

        Character? target = _owner.FindTarget();
        if (target != null)
        {
            _owner.FaceTowards(target);
            velocity.X = Mathf.MoveToward(velocity.X, 0, 600f * (float)delta);
            PlayIfExists("idle");
            if (_owner.CooldownLeft <= 0f && _owner.IsOnFloor())
            {
                _owner.Velocity = velocity;
                _owner.StateMachine.TransitionTo(_attackState);
                return;
            }
        }
        else if (_owner.PatrolDistance > 0f && _owner.IsOnFloor())
        {
            float offset = _owner.GlobalPosition.X - _owner.PatrolOriginX;
            if (offset > _owner.PatrolDistance) _patrolDir = -1;
            else if (offset < -_owner.PatrolDistance) _patrolDir = 1;
            if (_owner.IsOnWall()) _patrolDir = -_patrolDir;
            _owner.SetFacing(_patrolDir);
            velocity.X = _patrolDir * _owner.Speed * 0.35f;
            PlayIfExists("walk");
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, 600f * (float)delta);
            PlayIfExists("idle");
        }

        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    private void PlayIfExists(string anim)
    {
        if (_owner.Sprite.Animation == anim) return;
        if (_owner.Sprite.SpriteFrames?.HasAnimation(anim) == true)
            _owner.Sprite.Play(anim);
    }

    public void Exit() { }
}
