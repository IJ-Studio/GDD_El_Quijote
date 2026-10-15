// res://entities/enemies/TacoFuria.cs
using Godot;

/// <summary>
/// Enemigo que lanza salsa en arco que requiere bloqueo para evitar daño.
/// Hereda de Enemy y sobrescribe la lógica de ataque mediante polimorfismo.
/// </summary>
public partial class TacoFuria : Enemy
{
    /// <summary>Cuántas gotas de salsa lanza en cada ataque.</summary>
    [Export] public int DropsPerAttack { get; set; } = 2;

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this));
        StateMachine.RegisterState("EnemyAttack", new SprayState(this));
        StateMachine.RegisterState("Dead", new EnemyDeadState(this));

        // El estado Alert se activa por la Oleada de Adrenalina
        StateMachine.RegisterState("Alert", new GuardState(this, "EnemyAttack"));

        StateMachine.InitialState = "Guard";
    }

    /// <summary>
    /// Estado de ataque: lanza salsa en arco hacia el jugador.
    /// Requiere que el jugador bloquee (o se aleje) para evitar daño.
    /// </summary>
    private class SprayState : EnemyAttackState
    {
        private readonly TacoFuria _taco;

        public SprayState(TacoFuria owner) : base(owner, "attack")
        {
            _taco = owner;
        }

        public override void Enter()
        {
            base.Enter();
            for (int i = 0; i < _taco.DropsPerAttack; i++)
            {
                var p = ProjectilePool.Instance.Get(ProjectileType.SalsaPicante);
                // cada gota sale con un ángulo distinto para cubrir más espacio
                float lift = 1.1f - i * 0.35f;
                p?.Launch(_taco.MuzzlePosition, new Vector2(_taco.Facing, -lift));
            }
            AudioManager.Instance?.PlaySfx("disparo", -10f);
        }
    }
}
