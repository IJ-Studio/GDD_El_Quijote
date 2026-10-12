// res://entities/enemies/TacoFuria.cs
using Godot;

/// <summary>
/// Enemigo que lanza salsa en arco que requiere bloqueo para evitar daño.
/// Hereda de Enemy y sobrescribe la lógica de ataque mediante polimorfismo.
/// </summary>
public partial class TacoFuria : Enemy
{
    [Export] public float AttackCooldown { get; set; } = 2.0f;
    [Export] public float AttackDuration { get; set; } = 1.0f;

    private float _timeSinceAttack = 0f;
    private float _attackTimer = 0f;

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this));
        StateMachine.RegisterState("EnemyAttack", new SprayState(this));
        StateMachine.RegisterState("Dead", new EnemyDeadState(this));

        // El estado Alert se activa por la Oleada de Adrenalina
        StateMachine.RegisterState("Alert", new GuardState(this, "EnemyAttack"));

        StateMachine.InitialState = "Guard";
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _timeSinceAttack += (float)delta;
    }

    /// <summary>
    /// Estado de ataque: lanzar salsa en arco.
    /// Requiere que el jugador bloquee para evitar daño.
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
            _taco._attackTimer = 0f;
            _taco._timeSinceAttack = 0f;
        }

        public override void PhysicsUpdate(double delta)
        {
            // Aplicar gravedad
            Vector2 velocity = _taco.Velocity;
            if (!_taco.IsOnFloor())
            {
                velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
            }

            velocity.X = 0f; // Quieto durante ataque
            _taco.Velocity = velocity;
            _taco.MoveAndSlide();

            // Contador de duración del ataque
            _taco._attackTimer += (float)delta;

            // Disparar salsa cada cooldown durante la duración
            if (_taco._timeSinceAttack >= _taco.AttackCooldown)
            {
                SpraySalsa();
                _taco._timeSinceAttack = 0f;
            }

            // Terminar ataque después de la duración
            if (_taco._attackTimer >= _taco.AttackDuration)
            {
                _taco.StateMachine.TransitionTo("Guard");
            }
        }

        private void SpraySalsa()
        {
            var p = ProjectilePool.Instance.Get(ProjectileType.SalsaPicante);

            // Lanzar en arco hacia el jugador
            Vector2 direction = _taco.Sprite.FlipH ? Vector2.Left : Vector2.Right;

            // Ajustar posición de salida
            p.Launch(_taco.GlobalPosition, direction);
        }
    }
}
