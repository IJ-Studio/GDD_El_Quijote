// res://states/player_states/HurtState.cs
using Godot;

/// <summary>
/// Estado de retroceso (Hurt) al recibir daño.
/// Empuja al personaje hacia atrás y regresa a Idle al terminar la animación.
/// </summary>
public partial class HurtState : Node, IState
{
    private Character _owner = null!;
    private float _timer = 0f;
    public HurtState() { }
    public HurtState(Character owner) => _owner = owner;

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
        if (_owner.Sprite.SpriteFrames?.HasAnimation("hurt") == true)
        {
            _owner.Sprite.Play("hurt");
        }
        else
        {
            _owner.Sprite.Play("idle");
        }
        _owner.Sprite.AnimationFinished += OnAnimationFinished;
        // Retroceso lejos de quien pegó (antes empujaba hacia atrás de donde miraba,
        // y un golpe por la espalda lo lanzaba contra el enemigo)
        _owner.Velocity = new Vector2(_owner.KnockbackDirection * 170f, -160f);
        _timer = 0.45f;
    }

    public void Update(double delta)
    {
        // Respaldo por si la animación es en bucle: sale por tiempo
        _timer -= (float)delta;
        if (_timer <= 0f) _owner.StateMachine.TransitionTo(_owner.IsOnFloor() ? "Idle" : "Fall");
    }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, 0, 400f * (float)delta);
        if (!_owner.IsOnFloor())
        {
            velocity.Y += Character.Gravity * (float)delta;
        }
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit()
    {
        _owner.Sprite.AnimationFinished -= OnAnimationFinished;
    }

    private void OnAnimationFinished()
    {
        if (_owner.Sprite.Animation == "hurt" && _owner.IsOnFloor())
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }
}
