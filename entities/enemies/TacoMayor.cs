// res://entities/enemies/TacoMayor.cs
using Godot;

/// <summary>
/// Jefe final del juego. Hereda de Enemy e implementa un sistema de 3 fases
/// de ataque que transiciona según el porcentaje de vida restante.
///
/// Fases:
/// Su único ataque es lanzar salsa picante en arco (sin embestidas ni golpes cuerpo a cuerpo):
/// - Fase 1 (70-100% HP): 2 gotas de salsa
/// - Fase 2 (30-69% HP): 3 gotas, ataca más seguido
/// - Fase 3 (0-29% HP): ráfaga de 4 gotas, aún más seguido
/// </summary>
public partial class TacoMayor : Enemy
{
    [Export] public float AttackCooldownPhase1 { get; set; } = 2.4f;
    [Export] public float AttackCooldownPhase2 { get; set; } = 1.6f;
    [Export] public float AttackCooldownPhase3 { get; set; } = 0.9f;

    /// <summary>El jefe no se aturde ni pierde su ataque por un golpe: mantiene su ritmo por fases.</summary>
    protected override bool ReactsToHits => false;

    private TacoMayorPhase _currentPhase = TacoMayorPhase.Phase1;
    private float _timeSinceAttack = 0f;
    private int _attackCount = 0;

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this, "EnemyAttack"));

        // Selector: decide el ataque según la fase (polimorfismo de estados)
        StateMachine.RegisterState("EnemyAttack", new PhaseSelectorState(this));
        StateMachine.RegisterState("RangedAttack", new RangedAttackState(this));

        // Estado Alert para Oleada de Adrenalina
        StateMachine.RegisterState("Alert", new GuardState(this, "EnemyAttack"));

        StateMachine.RegisterState("Dead", new EnemyDeadState(this));
        StateMachine.InitialState = "Guard";
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        _timeSinceAttack += (float)delta;
        UpdatePhaseTransition();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        UpdatePhaseTransition();
    }

    /// <summary>
    /// Transiciona entre fases según el porcentaje de vida actual.
    /// </summary>
    private void UpdatePhaseTransition()
    {
        float hpPercentage = (float)CurrentHealth / MaxHealth * 100f;
        TacoMayorPhase newPhase = GetPhaseFromHealth(hpPercentage);

        if (newPhase != _currentPhase)
        {
            _currentPhase = newPhase;
            _timeSinceAttack = 0f;
            GuardCooldown = GetCurrentAttackCooldown();
        }
    }

    private static TacoMayorPhase GetPhaseFromHealth(float hpPercentage)
    {
        if (hpPercentage >= 70f) return TacoMayorPhase.Phase1;
        if (hpPercentage >= 30f) return TacoMayorPhase.Phase2;
        return TacoMayorPhase.Phase3;
    }

    public float GetCurrentAttackCooldown()
    {
        float baseCooldown = _currentPhase switch
        {
            TacoMayorPhase.Phase1 => AttackCooldownPhase1,
            TacoMayorPhase.Phase2 => AttackCooldownPhase2,
            TacoMayorPhase.Phase3 => AttackCooldownPhase3,
            _ => 2.0f
        };
        return baseCooldown * AttackCooldownScale;
    }

    public float GetTimeSinceAttack() => _timeSinceAttack;
    public void ResetAttackTimer() => _timeSinceAttack = 0f;
    public TacoMayorPhase GetCurrentPhase() => _currentPhase;

    /// <summary>
    /// Pasa de la guardia al ataque de salsa (el jefe solo tiene este ataque).
    /// </summary>
    private class PhaseSelectorState : IState
    {
        private readonly TacoMayor _boss;
        public PhaseSelectorState(TacoMayor owner) => _boss = owner;

        public void Enter()
        {
            _boss._attackCount++;
            _boss.StateMachine.CallDeferred(StateMachine.MethodName.TransitionTo, "RangedAttack");
        }

        public void Update(double delta) { }
        public void PhysicsUpdate(double delta) { }
        public void Exit() { }
    }

    /// <summary>
    /// Lanza salsa en arco (fase 2) o ráfaga de salsa (fase 3).
    /// </summary>
    private class RangedAttackState : EnemyAttackState
    {
        private readonly TacoMayor _boss;
        private bool _barrageListening;

        public RangedAttackState(TacoMayor owner) : base(owner, "attack")
        {
            _boss = owner;
        }

        public override void Enter()
        {
            bool barrage = _boss._currentPhase == TacoMayorPhase.Phase3;
            if (barrage && _boss.Sprite.SpriteFrames?.HasAnimation("barrage") == true)
            {
                Owner.Velocity = new Vector2(0, Owner.Velocity.Y);
                Owner.Sprite.Play("barrage");
                Owner.Sprite.AnimationFinished += OnBarrageFinished;
                _barrageListening = true;
            }
            else
            {
                base.Enter();
            }
            int drops = _boss._currentPhase switch
            {
                TacoMayorPhase.Phase1 => 2,
                TacoMayorPhase.Phase2 => 3,
                _ => 4
            };
            for (int i = 0; i < drops; i++)
            {
                var projectile = ProjectilePool.Instance.Get(ProjectileType.SalsaPicante);
                float lift = 1.3f - i * 0.3f;
                projectile?.Launch(_boss.MuzzlePosition, new Vector2(_boss.Facing, -lift));
            }
            _boss.ResetAttackTimer();
            AudioManager.Instance?.PlaySfx("disparo", -8f);
        }

        public override void Exit()
        {
            if (_barrageListening)
            {
                Owner.Sprite.AnimationFinished -= OnBarrageFinished;
                _barrageListening = false;
            }
            base.Exit();
        }

        private void OnBarrageFinished()
        {
            Owner.StateMachine.TransitionTo("Guard");
        }
    }

    public enum TacoMayorPhase { Phase1, Phase2, Phase3 }
}
