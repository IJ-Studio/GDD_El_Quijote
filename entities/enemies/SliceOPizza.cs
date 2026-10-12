// res://entities/enemies/SliceOPizza.cs
using Godot;

/// <summary>
/// Enemigo que ataca a distancia disparando un hilo de queso.
/// </summary>
public partial class SliceOPizza : Enemy
{
    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Guard", new GuardState(this));
        StateMachine.RegisterState("EnemyAttack", new ShootState(this));
        StateMachine.RegisterState("Dead", new EnemyDeadState(this));
        
        // El estado Alert se activa por la Oleada de Adrenalina
        StateMachine.RegisterState("Alert", new GuardState(this, "EnemyAttack"));
        
        StateMachine.InitialState = "Guard";
    }

    private class ShootState : EnemyAttackState
    {
        public ShootState(Enemy owner) : base(owner, "attack") { }

        public override void Enter()
        {
            base.Enter();
            
            // Disparar proyectil desde el pool
            var p = ProjectilePool.Instance.Get(ProjectileType.HiloDeQueso);
            
            // Determinar dirección basada en el flip del sprite
            Vector2 direction = Owner.Sprite.FlipH ? Vector2.Left : Vector2.Right;
            
            // Ajustar posición de salida si es necesario
            p.Launch(Owner.GlobalPosition, direction);
        }
    }
}
