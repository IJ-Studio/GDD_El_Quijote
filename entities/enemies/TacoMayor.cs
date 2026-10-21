// res://entities/enemies/TacoMayor.cs
using Godot;

/// <summary>
/// Jefe final del juego. Hereda de Enemy e implementa un sistema de 3 fases
/// de ataque que transiciona según el porcentaje de vida restante.
///
/// Fases:
/// - Fase 1 (70-100% HP): embestida pesada
/// - Fase 2 (30-69% HP): alterna embestida y salsa en arco
/// - Fase 3 (0-29% HP): ráfaga de salsa y embestidas más rápidas
/// </summary>
public partial class TacoMayor : Enemy
{
    [Export] public float AttackCooldownPhase1 { get; set; } = 2.4f;
    [Export] public float AttackCooldownPhase2 { get; set; } = 1.6f;
    [Export] public float AttackCooldownPhase3 { get; set; } = 0.9f;
    [Export] public float ChargeSpeedPhase3 { get; set; } = 280f;

    private TacoMayorPhase _currentPhase = TacoMayorPhase.Phase1;
    private float _timeSinceAttack = 0f;
    private int _attackCount = 0;

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this, "EnemyAttack"));

        // Selector: decide el ataque según la fase (polimorfismo de estados)
        StateMachine.RegisterState("EnemyAttack", new PhaseSelectorState(this));
        StateMachine.RegisterState("RangedAttack", new RangedAttackState(this));
        StateMachine.RegisterState("Charge", new ChargeAttackState(this));

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
    /// Elige el siguiente ataque según la fase y salta inmediatamente a ese estado.
    /// </summary>
    private class PhaseSelectorState : IState
    {
        private readonly TacoMayor _boss;
        public PhaseSelectorState(TacoMayor owner) => _boss = owner;

        public void Enter()
        {
            _boss._attackCount++;
            string next = _boss._currentPhase switch
            {
                TacoMayorPhase.Phase1 => "Charge",
                TacoMayorPhase.Phase2 => _boss._attackCount % 2 == 0 ? "Charge" : "RangedAttack",
                _ => _boss._attackCount % 3 == 0 ? "Charge" : "RangedAttack",
            };
            _boss.StateMachine.CallDeferred(StateMachine.MethodName.TransitionTo, next);
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
            int drops = barrage ? 4 : 2;
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

    /// <summary>
    /// Embestida frontal: corre hacia donde mira hasta chocar o agotar el tiempo.
    /// </summary>
    private class ChargeAttackState : EnemyAttackState
    {
        private readonly TacoMayor _boss;
        private const float ChargeAttackDuration = 1.1f;

        public ChargeAttackState(TacoMayor owner) : base(owner, "charge")
        {
            _boss = owner;
        }

        public override void Enter()
        {
            base.Enter();
            _boss.ResetAttackTimer();
        }

        public override void PhysicsUpdate(double delta)
        {
            Vector2 velocity = _boss.Velocity;
            float speed = _boss._currentPhase == TacoMayorPhase.Phase3 ? _boss.ChargeSpeedPhase3 : _boss.ChargeSpeedPhase3 * 0.75f;
            velocity.X = _boss.Facing * speed;

            if (!_boss.IsOnFloor())
            {
                velocity.Y += Character.Gravity * (float)delta;
            }

            _boss.Velocity = velocity;
            _boss.MoveAndSlide();

            // Terminar embestida al chocar con una pared o después de la duración
            if (_boss.IsOnWall() || _boss.GetTimeSinceAttack() > ChargeAttackDuration)
            {
                _boss.StateMachine.TransitionTo("Guard");
            }
        }

        // la animación de embestida es en bucle: no termina el ataque por animación
        protected override void OnAnimationFinished() { }
    }

    public enum TacoMayorPhase { Phase1, Phase2, Phase3 }
}
