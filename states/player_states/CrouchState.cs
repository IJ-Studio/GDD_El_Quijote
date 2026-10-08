// res://states/player_states/CrouchState.cs
using Godot;

/// <summary>
/// Estado de agachado (Crouch) del jugador.
/// </summary>
public partial class CrouchState : Node, IState
{
    private const string ANIM_CROUCH = "crouch";
    private Character _owner = null!;

    public CrouchState() { }
    public CrouchState(Character owner) => _owner = owner;

    public override void _Ready()
    {
        if (_owner == null)
        {
            Node p = GetParent();
            while (p != null && p is not Character) p = p.GetParent();
            if (p is Character c) _owner = c;
        }
    }

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_CROUCH);
        _owner.Velocity = Vector2.Zero;
    }

    public void Update(double delta)
    {
        if (!_owner.IsActionPressed("crouch") || !_owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }

    public void PhysicsUpdate(double delta) { }
    public void Exit() { }
}
