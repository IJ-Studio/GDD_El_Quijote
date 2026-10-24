// res://entities/enemies/SliceOPizza.cs
using Godot;

/// <summary>
/// Enemigo que ataca a distancia disparando un hilo de queso.
/// </summary>
public partial class SliceOPizza : Enemy
{
    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this));
        StateMachine.RegisterState("EnemyAttack", new ShootState(this));
        StateMachine.RegisterState("Dead", new EnemyDeadState(this));

        // El estado Alert se activa por la Oleada de Adrenalina
        StateMachine.RegisterState("Alert", new GuardState(this, "EnemyAttack"));

        StateMachine.InitialState = "Guard";
    }

    private class ShootState : EnemyAttackState
    {
        public ShootState(Enemy owner) : base(owner, "attack") { }

        protected override void Strike()
        {
            base.Strike();

            // Disparar proyectil desde el pool, hacia donde mira
            var p = ProjectilePool.Instance.Get(ProjectileType.HiloDeQueso);
            // Sale de la orilla de la pizza, a la altura de la cabeza del jugador:
            // agachado, el queso le pasa por encima.
            p?.Launch(Owner.MuzzleAtHeight(64f), new Vector2(Owner.Facing, 0));
            AudioManager.Instance?.PlaySfx("disparo", -10f);
        }
    }
}
