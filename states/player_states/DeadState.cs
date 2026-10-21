// res://states/player_states/DeadState.cs
using Godot;

/// <summary>
/// Estado de muerte (Dead) del personaje.
/// Reproduce la animación de derrota y emite PlayerDied al EventBus.
/// </summary>
public partial class DeadState : Node, IState
{
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
        _owner.Sprite.Play("death");
        _owner.Velocity = new Vector2(-_owner.Facing * 120f, -200f);
        if (_owner.Hurtbox is not null) _owner.Hurtbox.SetDeferred(Area2D.PropertyName.Monitorable, false);
        if (_owner.Hitbox is not null) _owner.Hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        AudioManager.Instance?.PlaySfx("muerte", -6f);
        EventBus.Instance.EmitPlayerDied(_owner);
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 300f * (float)delta);
        if (!_owner.IsOnFloor()) velocity.Y += Character.Gravity * (float)delta;
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit() { }
}
