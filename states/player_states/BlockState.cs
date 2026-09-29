// res://states/player_states/BlockState.cs
using Godot;

/// <summary>
/// Estado de bloqueo con escudo del jugador.
/// Soporta bloqueo normal y bloqueo en arco / aéreo (K sostenido + Flecha Arriba / W).
/// Reduce o anula el daño recibido mientras está activo.
/// </summary>
public class BlockState : IState
{
    private const string ANIM_BLOCK = "block";
    private const string ANIM_BLOCK_HIGH = "block_high";
    private readonly Character _owner;

    public bool IsHighBlock { get; private set; } = false;

    public BlockState(Character owner) => _owner = owner;

    public void Enter()
    {
        UpdateBlockAnimation();
        _owner.Velocity = new Vector2(0, _owner.Velocity.Y);
    }

    public void Update(double delta)
    {
        // Actualizar tipo de bloqueo si presiona arriba/W manteniendo bloqueado
        bool wasHigh = IsHighBlock;
        IsHighBlock = Input.IsActionPressed("player_jump") || Input.IsActionPressed("ui_up");
        if (wasHigh != IsHighBlock)
        {
            UpdateBlockAnimation();
        }

        // Salir del bloqueo al soltar K o clic derecho
        if (!Input.IsActionPressed("player_block") && !Input.IsMouseButtonPressed(MouseButton.Right))
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

    /// <summary>
    /// Calcula el daño modificado mientras se bloquea.
    /// Bloqueo normal reduce un 50%. Bloqueo alto/aéreo reduce un 100% contra proyectiles o ataques superiores.
    /// </summary>
    public int ModifyDamage(int incomingDamage)
    {
        return IsHighBlock ? 0 : incomingDamage / 2;
    }
}
