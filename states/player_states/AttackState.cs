// res://states/player_states/AttackState.cs
using Godot;

/// <summary>
/// Estado de ataque del jugador. Activa la Hitbox solo durante el ataque
/// y vuelve a Idle (o Fall, si está en el aire) cuando termina la animación.
/// </summary>
public partial class AttackState : Node, IState
{
    private Character _owner = null!;

    public AttackState() { }
    public AttackState(Character owner) => _owner = owner;

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
        _owner.Sprite.Play("attack");
        _owner.Sprite.AnimationFinished += OnAnimationFinished;
        if (_owner.Hitbox is not null)
        {
            _owner.Hitbox.SetDeferred(Area2D.PropertyName.Monitoring, true);
        }
        if (_owner.IsOnFloor())
        {
            // pequeño impulso hacia adelante en la estocada
            _owner.Velocity = new Vector2(_owner.Facing * 60f, _owner.Velocity.Y);
        }
        AudioManager.Instance?.PlaySfx("ataque", -12f);
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        if (_owner.IsOnFloor())
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, 500f * (float)delta);
        }
        else
        {
            velocity.Y += Character.Gravity * (float)delta;
        }
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit()
    {
        _owner.Sprite.AnimationFinished -= OnAnimationFinished;
        if (_owner.Hitbox is not null)
        {
            _owner.Hitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        }
    }

    private void OnAnimationFinished()
    {
        if (_owner.Sprite.Animation == "attack")
        {
            _owner.StateMachine.TransitionTo(_owner.IsOnFloor() ? "Idle" : "Fall");
        }
    }
}
