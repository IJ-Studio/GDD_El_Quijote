// res://states/player_states/BlockState.cs
using Godot;

/// <summary>
/// Estado de bloqueo con escudo del jugador.
/// Soporta bloqueo normal y bloqueo en arco / aéreo (K sostenido + Flecha Arriba / W).
/// Reduce o anula el daño recibido mientras está activo.
/// </summary>
public partial class BlockState : Node, IState
{
    private const string ANIM_BLOCK = "block";
    private const string ANIM_BLOCK_HIGH = "block_high";
    private Character _owner = null!;

    public bool IsHighBlock { get; private set; } = false;

    public BlockState() { }
    public BlockState(Character owner) => _owner = owner;

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
        UpdateBlockAnimation();
        _owner.Velocity = new Vector2(0, _owner.Velocity.Y);
    }

    public void Update(double delta)
    {
        bool wasHigh = IsHighBlock;
        IsHighBlock = _owner.IsActionPressed("jump") || _owner.IsActionPressed("ui_up");
        if (wasHigh != IsHighBlock)
        {
            UpdateBlockAnimation();
        }

        if (!_owner.IsActionPressed("block") && !(_owner.InputPrefix == "player1_" && Input.IsMouseButtonPressed(MouseButton.Right)))
        {
            _owner.StateMachine.TransitionTo("Idle");
        }
    }

    private void UpdateBlockAnimation()
    {
        string anim = IsHighBlock ? ANIM_BLOCK_HIGH : ANIM_BLOCK;
        if (_owner.Sprite.SpriteFrames?.HasAnimation(anim) == true)
        {
            _owner.Sprite.Play(anim);
        }
        else
        {
            _owner.Sprite.Play(ANIM_BLOCK);
        }
    }

    public void PhysicsUpdate(double delta)
    {
        Vector2 velocity = _owner.Velocity;
        if (!_owner.IsOnFloor())
        {
            velocity.Y += (float)(ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle() * delta);
        }
        _owner.Velocity = velocity;
        _owner.MoveAndSlide();
    }

    public void Exit()
    {
        IsHighBlock = false;
    }

    public int ModifyDamage(int incomingDamage)
    {
        return IsHighBlock ? 0 : incomingDamage / 2;
    }
}
