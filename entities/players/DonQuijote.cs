// res://entities/players/DonQuijote.cs
using Godot;

/// <summary>
/// Personaje jugable 1: Don Quijote.
/// Hereda de Character e implementa la inicialización de estados para la FSM (Sprint 1 y 2).
/// </summary>
public partial class DonQuijote : Character
{
    private BlockState? _blockState;

    protected override void InitializeStates()
    {
        _blockState = new BlockState(this);

        StateMachine.RegisterState("Idle",   new IdleState(this));
        StateMachine.RegisterState("Run",    new RunState(this));
        StateMachine.RegisterState("Jump",   new JumpState(this));
        StateMachine.RegisterState("Crouch", new CrouchState(this));
        StateMachine.RegisterState("Attack", new AttackState(this));
        StateMachine.RegisterState("Block",  _blockState);
        StateMachine.RegisterState("Hurt",   new HurtState(this));
        StateMachine.RegisterState("Dead",   new DeadState(this));

        StateMachine.InitialState = "Idle";
    }

    protected override int ModifyIncomingDamage(int amount)
    {
        // Si el estado actual es BlockState, delegar la reducción de daño
        if (StateMachine.CurrentStateName == "Block" && _blockState is not null)
        {
            return _blockState.ModifyDamage(amount);
        }
        return base.ModifyIncomingDamage(amount);
    }
}
