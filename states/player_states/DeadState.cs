// res://states/player_states/DeadState.cs
using Godot;

/// <summary>
/// Estado de muerte (Dead) del personaje.
/// Emite PlayerDied al EventBus.
/// </summary>
public partial class DeadState : Node, IState
{
    private const string ANIM_DEAD = "death";
    private Character _owner = null!;

    public DeadState() { }
    public DeadState(Character owner) => _owner = owner;

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
        _owner.Sprite.Play(ANIM_DEAD);
        _owner.Velocity = Vector2.Zero;
        EventBus.Instance.EmitPlayerDied(_owner);
    }

    public void Update(double delta) { }
    public void PhysicsUpdate(double delta) { }
    public void Exit() { }
}
