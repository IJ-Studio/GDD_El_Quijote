// res://entities/enemies/PastaDemon.cs
using Godot;

/// <summary>
/// Enemigo que lanza albóndigas rebotadoras con daño en área.
/// Hereda de Enemy y sobrescribe la lógica de ataque mediante polimorfismo.
/// </summary>
public partial class PastaDemon : Enemy
{
    /// <summary>Inclinación del lanzamiento (0 = horizontal, 1 = 45° hacia arriba).</summary>
    [Export] public float TossArc { get; set; } = 0.9f;

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this));
        StateMachine.RegisterState("EnemyAttack", new TossState(this));
        StateMachine.RegisterState("Dead", new EnemyDeadState(this));

        // El estado Alert se activa por la Oleada de Adrenalina
        StateMachine.RegisterState("Alert", new GuardState(this, "EnemyAttack"));

        StateMachine.InitialState = "Guard";
    }

    /// <summary>
    /// Estado de ataque: lanza una albóndiga en arco cuando el brazo llega al frente.
    /// </summary>
    private class TossState : EnemyAttackState
    {
        private readonly PastaDemon _demon;
        private bool _thrown;

        public TossState(PastaDemon owner) : base(owner, "attack")
        {
            _demon = owner;
        }

        public override void Enter()
        {
            base.Enter();
            _thrown = false;
            _demon.Sprite.FrameChanged += OnFrameChanged;
        }

        public override void Exit()
        {
            _demon.Sprite.FrameChanged -= OnFrameChanged;
            base.Exit();
        }

        private void OnFrameChanged()
        {
            if (_thrown || _demon.Sprite.Frame < 1) return;
            _thrown = true;
            var p = ProjectilePool.Instance.Get(ProjectileType.Albondiga);
            p?.Launch(_demon.MuzzlePosition, new Vector2(_demon.Facing, -_demon.TossArc));
            AudioManager.Instance?.PlaySfx("disparo", -10f);
        }
    }
}
