// res://entities/players/SanchoPanza.cs
using Godot;

/// <summary>
/// Personaje jugable 2: Sancho Panza.
/// Hereda de Character, utiliza la misma FSM genérica y emplea un esquema de input independiente (player2_).
/// Cuenta con mayor salud y daño con su Maza/Hacha en comparación con Don Quijote.
/// </summary>
public partial class SanchoPanza : Character
{
    private BlockState? _blockState;

    protected override void InitializeStates()
    {
        Speed = 160f;
        MaxHealth = 150;
        InputPrefix = "player2_";

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
        if (StateMachine.CurrentStateName == "Block" && _blockState is not null)
        {
            return _blockState.ModifyDamage(amount);
        }
        return base.ModifyIncomingDamage(amount);
    }
}
