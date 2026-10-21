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

        public ChargeState(BurgerBashing owner) : base(owner, "attack")
        {
            _burger = owner;
        }

        public override void PhysicsUpdate(double delta)
        {
            Vector2 velocity = _burger.Velocity;
            if (!_burger.IsOnFloor())
            {
                velocity.Y += Character.Gravity * (float)delta;
            }

            // Embestida hacia donde mira; se detiene si choca con una pared
            velocity.X = _burger.IsOnWall() ? 0f : _burger.Facing * _burger.ChargeSpeed;

            _burger.Velocity = velocity;
            _burger.MoveAndSlide();
        }
    }
}
