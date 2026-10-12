// res://states/enemy_states/GuardState.cs
using Godot;

/// <summary>
/// Estado de guardia para enemigos. El enemigo permanece en espera hasta que
/// un jugador entra en su zona de guardia (GuardZone).
/// </summary>
public class GuardState : IState
{
    private readonly Enemy _owner;
    private readonly string _attackState;

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
        if (_owner.GuardZone == null) return;

        foreach (var body in _owner.GuardZone.GetOverlappingBodies())
        {
            // Detecta si es un jugador (Character que no sea Enemy)
            if (body is Character and not Enemy)
            {
                _owner.StateMachine.TransitionTo(_attackState);
                return;
            }
        }
    }

    public void Exit() { }
}
