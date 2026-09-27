using Godot;

/// <summary>
/// Clase base abstracta para TODOS los personajes del juego.
/// NO instanciar directamente. Usar subclases concretas.
/// </summary>
public abstract partial class Character : CharacterBody2D
{
    // ── Exportados al Inspector de Godot ──────────────────────────
    [Export] public float Speed     { get; protected set; } = 150f;
    [Export] public int   MaxHealth { get; protected set; } = 100;

    // ── Estado en tiempo de ejecución ─────────────────────────────
    public int CurrentHealth { get; protected set; }

    // ── Dependencias internas ──────────────────────────────────────
    protected StateMachine  StateMachine = null!;
    protected AnimatedSprite2D Sprite    = null!;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        StateMachine  = GetNode<StateMachine>("StateMachine");
        Sprite        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        InitializeStates();
        StateMachine.Start();
    }

    /// <summary>Cada subclase registra sus propios estados en la FSM.</summary>
    protected abstract void InitializeStates();

    public virtual void Move(Vector2 direction)
    {
        Velocity = direction * Speed;
        MoveAndSlide();
    }

    /// <summary>
    /// Recibe daño. Subclases pueden sobreescribir para resistencias o efectos.
    /// Emite HealthChanged vía EventBus. Llama a Die() si CurrentHealth llega a 0.
    /// </summary>
    /// <param name="amount">Daño a aplicar (valor positivo).</param>
    public virtual void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        EventBus.Instance.EmitHealthChanged(this, CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
            Die();
        else
            StateMachine.TransitionTo("Hurt");
    }

    protected virtual void Die()
    {
        StateMachine.TransitionTo("Dead");
    }
}
