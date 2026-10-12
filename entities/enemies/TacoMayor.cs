// res://entities/enemies/TacoMayor.cs
using Godot;

/// <summary>
/// Jefe final del juego. Hereda de Enemy e implementa un sistema de 3 fases
/// de ataque que transiciona según el porcentaje de vida restante.
///
/// Fases:
/// - Fase 1 (70-100% HP): Guard - Patrullaje pasivo
/// - Fase 2 (30-69% HP): RangedAttack - Lanza salsa en arco
/// - Fase 3 (0-29% HP): ChargeAttack - Embestida frontal
/// </summary>
public partial class TacoMayor : Enemy
{
    [Export] public float AttackCooldownPhase1 { get; set; } = 3.0f;
    [Export] public float AttackCooldownPhase2 { get; set; } = 1.8f;
    [Export] public float AttackCooldownPhase3 { get; set; } = 0.7f;
    [Export] public float ChargeSpeedPhase3 { get; set; } = 280f;

    private TacoMayorPhase _currentPhase = TacoMayorPhase.Phase1;
    private float _timeSinceAttack = 0f;

    protected override void InitializeStates()
    {
        // Fase 1: Guard pasivo
        StateMachine.RegisterState("Guard", new GuardState(this));

        // Fase 2: Lanzador de salsa
        StateMachine.RegisterState("EnemyAttack", new RangedAttackState(this));

        // Fase 3: Embestidor
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
        }
    }

    private TacoMayorPhase GetPhaseFromHealth(float hpPercentage)
    {
        if (hpPercentage >= 70f) return TacoMayorPhase.Phase1;
        if (hpPercentage >= 30f) return TacoMayorPhase.Phase2;
        return TacoMayorPhase.Phase3;
    }

    public float GetCurrentAttackCooldown() => _currentPhase switch
    {
        TacoMayorPhase.Phase1 => AttackCooldownPhase1,
        TacoMayorPhase.Phase2 => AttackCooldownPhase2,
        TacoMayorPhase.Phase3 => AttackCooldownPhase3,
        _ => 2.0f
    };

    public float GetTimeSinceAttack() => _timeSinceAttack;
    public void ResetAttackTimer() => _timeSinceAttack = 0f;
    public TacoMayorPhase GetCurrentPhase() => _currentPhase;

    /// <summary>
    /// Fase 2: Lanza salsa en arco.
    /// </summary>
    private class RangedAttackState : EnemyAttackState
    {
        private readonly TacoMayor _boss;

        public RangedAttackState(TacoMayor owner) : base(owner, "attack")
        {
            _boss = owner;
        }

        public override void Enter()
        {
            base.Enter();
            _boss.Sprite.Play("attack");
        }

        public override void Exit() { }

        public override void PhysicsUpdate(double delta)
        {
            // Aplicar gravedad
            Vector2 velocity = _boss.Velocity;
            if (!_boss.IsOnFloor())
            {
                velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
            }
            velocity.X = 0f;
            _boss.Velocity = velocity;
            _boss.MoveAndSlide();

            // Disparar si tiempo suficiente
            if (_boss.GetTimeSinceAttack() >= _boss.GetCurrentAttackCooldown())
            {
                ShootSalsa();
                _boss.ResetAttackTimer();
            }

            // Terminar ataque después de 1.5s
            if (_boss.GetTimeSinceAttack() > 1.5f)
            {
                _boss.StateMachine.TransitionTo("Guard");
            }
        }

        private void ShootSalsa()
        {
            var projectile = ProjectilePool.Instance.Get(ProjectileType.SalsaPicante);
            if (projectile != null)
            {
                Vector2 direction = _boss.Sprite.FlipH ? Vector2.Left : Vector2.Right;
                projectile.Launch(_boss.GlobalPosition, direction);
            }
        }
    }

    /// <summary>
    /// Fase 3: Embestida frontal agresiva.
    /// </summary>
    private class ChargeAttackState : EnemyAttackState
    {
        private readonly TacoMayor _boss;
        private const float ChargeAttackDuration = 2.0f;

        public ChargeAttackState(TacoMayor owner) : base(owner, "attack")
        {
            _boss = owner;
        }

        public override void Enter()
        {
            base.Enter();
            _boss.Sprite.Play("attack");
            _boss.ResetAttackTimer();
        }

        public override void Exit() { }

        public override void PhysicsUpdate(double delta)
        {
            Vector2 velocity = _boss.Velocity;

            // Embestir hacia la derecha (o izquierda si está volteado)
            float chargeDir = _boss.Sprite.FlipH ? -1f : 1f;
            velocity.X = chargeDir * _boss.ChargeSpeedPhase3;

            // Aplicar gravedad
            if (!_boss.IsOnFloor())
            {
                velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
            }

            _boss.Velocity = velocity;
            _boss.MoveAndSlide();

            // Terminar embestida después de duración
            if (_boss.GetTimeSinceAttack() > ChargeAttackDuration)
            {
                _boss.ResetAttackTimer();
                _boss.StateMachine.TransitionTo("Guard");
            }
        }
    }

    public enum TacoMayorPhase { Phase1, Phase2, Phase3 }
}
