// res://entities/enemies/PastaDemon.cs
using Godot;

/// <summary>
/// Enemigo que lanza albóndigas rebotadoras con daño en área.
/// Hereda de Enemy y sobrescribe la lógica de ataque mediante polimorfismo.
/// </summary>
public partial class PastaDemon : Enemy
{
    [Export] public float AttackCooldown { get; set; } = 1.5f;

    private float _timeSinceAttack = 0f;

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this));
        StateMachine.RegisterState("EnemyAttack", new TossState(this));
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
    /// Estado de ataque: lanzar albóndigas.
    /// Implementa la lógica de toss con cooldown.
    /// </summary>
    private class TossState : EnemyAttackState
    {
        private readonly PastaDemon _demon;

        public TossState(PastaDemon owner) : base(owner, "attack")
        {
            _demon = owner;
        }

        public override void Enter()
        {
            base.Enter();
            _demon._timeSinceAttack = 0f;
        }

        public override void PhysicsUpdate(double delta)
        {
            // Aplicar gravedad
            Vector2 velocity = _demon.Velocity;
            if (!_demon.IsOnFloor())
            {
                velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
            }

            velocity.X = 0f; // Quieto durante ataque
            _demon.Velocity = velocity;
            _demon.MoveAndSlide();

            // Disparar si tiempo suficiente
            if (_demon._timeSinceAttack >= _demon.AttackCooldown)
            {
                TossAlbondiga();
                _demon._timeSinceAttack = 0f;
            }

            // Terminar ataque después de algunos disparos
            if (_demon._timeSinceAttack > _demon.AttackCooldown * 3)
            {
                _demon.StateMachine.TransitionTo("Guard");
            }
        }

        private void TossAlbondiga()
        {
            var p = ProjectilePool.Instance.Get(ProjectileType.Albondiga);

            // Lanzar en arco hacia el jugador
            Vector2 direction = _demon.Sprite.FlipH ? Vector2.Left : Vector2.Right;

            // Ajustar posición de salida (similar a SliceOPizza)
            p.Launch(_demon.GlobalPosition, direction);
        }
    }
}
