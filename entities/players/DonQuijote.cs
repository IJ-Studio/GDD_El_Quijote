using Godot;

// res://entities/players/DonQuijote.cs
public partial class DonQuijote : Character
{
    public override void _Ready()
    {
        // Don Quijote tiene estadísticas base específicas si se desea
        Speed = 200f;
        MaxHealth = 120;
        
        base._Ready();
    }

    protected override void InitializeStates()
    {
        StateMachine.RegisterState("Idle", new IdleState(this));
        StateMachine.RegisterState("Run", new RunState(this));
        StateMachine.RegisterState("Jump", new JumpState(this));
        StateMachine.RegisterState("Crouch", new CrouchState(this));
        
        // El estado inicial se configura en el inspector del StateMachine (default: Idle)
    }

    public override void _PhysicsProcess(double delta)
    {
        // La gravedad se maneja dentro de los estados que la requieren (JumpState)
        // Pero si estamos en Idle/Run y por alguna razón no estamos en el suelo,
        // deberíamos caer. El StateMachine se encarga de la delegación.
        
        // Nota: MoveAndSlide() se llama dentro de PhysicsUpdate de cada estado.
    }
}
