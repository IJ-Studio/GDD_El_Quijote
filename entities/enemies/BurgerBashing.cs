// res://entities/enemies/BurgerBashing.cs
using Godot;

/// <summary>
/// Enemigo que ataca cuerpo a cuerpo mediante una embestida frontal.
/// </summary>
public partial class BurgerBashing : Enemy
{
    [Export] public float ChargeSpeed { get; set; } = 300f;

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this));
        StateMachine.RegisterState("EnemyAttack", new ChargeState(this));
        StateMachine.RegisterState("Dead", new EnemyDeadState(this));
        
        // El estado Alert se activa por la Oleada de Adrenalina
        StateMachine.RegisterState("Alert", new GuardState(this, "EnemyAttack"));

        StateMachine.InitialState = "Guard";
    }

    private class ChargeState : EnemyAttackState
    {
        private readonly BurgerBashing _burger;
        private Vector2 _chargeDir;

        public ChargeState(BurgerBashing owner) : base(owner, "attack") 
        {
            _burger = owner;
        }

        public override void Enter()
        {
            base.Enter();
            // Determinar dirección de la embestida
            _chargeDir = _burger.Sprite.FlipH ? Vector2.Left : Vector2.Right;
        }

        public override void PhysicsUpdate(double delta)
        {
            // Aplicar gravedad
            Vector2 velocity = _burger.Velocity;
            if (!_burger.IsOnFloor())
            {
                velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
            }
            
            // Aplicar velocidad de embestida
            velocity.X = _chargeDir.X * _burger.ChargeSpeed;
            
            _burger.Velocity = velocity;
            _burger.MoveAndSlide();
        }
    }
}
