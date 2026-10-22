// res://entities/enemies/TacoMayor.cs
using Godot;

/// <summary>
/// Jefe final del juego. Hereda de Enemy e implementa un sistema de 3 fases
/// de ataque que transiciona según el porcentaje de vida restante.
///
/// Su ataque principal es lanzar salsa picante en arco; cuando un jugador se le pega
/// además puede soltar un puñetazo (más seguido en la fase 3):
/// - Fase 1 (70-100% HP): 2 gotas de salsa
/// - Fase 2 (30-69% HP): 3 gotas, ataca más seguido
/// - Fase 3 (0-29% HP): ráfaga de 4 gotas, aún más seguido
/// </summary>
public partial class TacoMayor : Enemy
{
    [Export] public float AttackCooldownPhase1 { get; set; } = 1.5f;
    [Export] public float AttackCooldownPhase2 { get; set; } = 1.0f;
    [Export] public float AttackCooldownPhase3 { get; set; } = 0.7f;

    /// <summary>Multiplicadores del daño de su salsa por fase (la salsa común hace muy poco; el jefe pega fuerte).</summary>
    [Export] public float SauceDamagePhase1 { get; set; } = 2.5f;
    [Export] public float SauceDamagePhase2 { get; set; } = 3.0f;
    [Export] public float SauceDamagePhase3 { get; set; } = 3.5f;

    /// <summary>Distancia (px) a la que un jugador queda al alcance del puñetazo.</summary>
    [Export] public float PunchRange { get; set; } = 170f;

    /// <summary>Impulso hacia delante (px/s) que da al soltar el puñetazo.</summary>
    [Export] public float PunchLungeSpeed { get; set; } = 190f;

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
        StateMachine.RegisterState("Punch", new PunchAttackState(this));

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

    /// <summary>Distancia horizontal al jugador vivo más cercano (infinito si no hay ninguno).</summary>
    private float NearestPlayerDistance()
    {
        float nearest = float.MaxValue;
        foreach (Node node in GetTree().GetNodesInGroup("players"))
        {
            if (node is Character c && c.CurrentHealth > 0)
                nearest = Mathf.Min(nearest, Mathf.Abs(c.GlobalPosition.X - GlobalPosition.X));
        }
        return nearest;
    }

    /// <summary>
    /// Elige el ataque: casi siempre salsa; con un jugador pegado suelta un puñetazo
    /// (1 de cada 3 ataques en fases 1 y 2, 1 de cada 2 en la fase 3).
    /// </summary>
    private class PhaseSelectorState : IState
    {
        private readonly TacoMayor _boss;
        public PhaseSelectorState(TacoMayor owner) => _boss = owner;

        public void Enter()
        {
            _boss._attackCount++;
            bool close = _boss.NearestPlayerDistance() <= _boss.PunchRange;
            int every = _boss._currentPhase == TacoMayorPhase.Phase3 ? 2 : 3;
            string next = close && _boss._attackCount % every == 0 ? "Punch" : "RangedAttack";
            _boss.StateMachine.CallDeferred(StateMachine.MethodName.TransitionTo, next);
        }

        public void Update(double delta) { }
        public void PhysicsUpdate(double delta) { }
        public void Exit() { }
    }

    /// <summary>
    /// Puñetazo: animación de ataque con un pequeño impulso hacia el jugador; la Hitbox del jefe
    /// (que se activa en el estado base) es la que hace el daño.
    /// </summary>
    private class PunchAttackState : EnemyAttackState
    {
        private readonly TacoMayor _boss;

        public PunchAttackState(TacoMayor owner) : base(owner, "attack")
        {
            _boss = owner;
        }

        public override void Enter()
        {
            base.Enter();
            _boss.Velocity = new Vector2(_boss.Facing * _boss.PunchLungeSpeed, _boss.Velocity.Y);
            _boss.ResetAttackTimer();
            AudioManager.Instance?.PlaySfx("golpe", -4f);
        }
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
            float damageMultiplier = _boss._currentPhase switch
            {
                TacoMayorPhase.Phase1 => _boss.SauceDamagePhase1,
                TacoMayorPhase.Phase2 => _boss.SauceDamagePhase2,
                _ => _boss.SauceDamagePhase3
            };
            for (int i = 0; i < drops; i++)
            {
                var projectile = ProjectilePool.Instance.Get(ProjectileType.SalsaPicante);
                float lift = 1.3f - i * 0.3f;
                if (projectile != null) projectile.DamageMultiplier = damageMultiplier;
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
