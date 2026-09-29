// res://entities/Character.cs
using Godot;

/// <summary>
/// Clase base abstracta para TODOS los personajes del juego.
/// NO instanciar directamente. Usar subclases concretas.
/// Incluye ventana de invulnerabilidad (iframes) tras recibir daño.
/// </summary>
public abstract partial class Character : CharacterBody2D
{
    // ── Exportados al Inspector de Godot ──────────────────────────
    [Export] public float Speed     { get; protected set; } = 150f;
    [Export] public int   MaxHealth { get; protected set; } = 100;
    [Export] public float InvulnerabilityDuration { get; set; } = 0.5f;

    // ── Estado en tiempo de ejecución ─────────────────────────────
    public int CurrentHealth { get; protected set; }
    public bool IsInvulnerable { get; protected set; } = false;
    private float _invulnerabilityTimer = 0f;

    // ── Dependencias internas ──────────────────────────────────────
    public StateMachine  StateMachine { get; protected set; } = null!;
    public AnimatedSprite2D Sprite    { get; protected set; } = null!;
    public Hitbox? Hitbox            { get; protected set; }
    public Hurtbox? Hurtbox          { get; protected set; }

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        StateMachine  = GetNode<StateMachine>("StateMachine");
        Sprite        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Intentar obtener Hitbox y Hurtbox de forma opcional si existen en la jerarquía del nodo
        Hitbox  = GetNodeOrNull<Hitbox>("Hitbox");
        Hurtbox = GetNodeOrNull<Hurtbox>("Hurtbox");

        InitializeStates();
        StateMachine.Start();
    }

    public override void _Process(double delta)
    {
        if (IsInvulnerable)
        {
            _invulnerabilityTimer -= (float)delta;
            if (_invulnerabilityTimer <= 0f)
            {
                IsInvulnerable = false;
                Sprite.Modulate = Colors.White;
            }
            else
            {
                float alpha = Mathf.Sin((float)Time.GetTicksMsec() * 0.05f) > 0 ? 1f : 0.4f;
                Sprite.Modulate = new Color(1f, 1f, 1f, alpha);
            }
        }
    }

    /// <summary>Cada subclase registra sus propios estados en la FSM.</summary>
    protected abstract void InitializeStates();

    public virtual void Move(Vector2 direction)
    {
        Velocity = direction * Speed;
        MoveAndSlide();
    }

    /// <summary>
    /// Recibe daño con verificación de invulnerabilidad (iframes).
    /// Emite HealthChanged vía EventBus. Llama a Die() si CurrentHealth llega a 0.
    /// </summary>
    /// <param name="amount">Daño a aplicar (valor positivo).</param>
    public virtual void TakeDamage(int amount)
    {
        if (IsInvulnerable) return;

        int finalDamage = ModifyIncomingDamage(amount);
        if (finalDamage <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - finalDamage);
        EventBus.Instance.EmitHealthChanged(this, CurrentHealth, MaxHealth);

        IsInvulnerable = true;
        _invulnerabilityTimer = InvulnerabilityDuration;

        if (CurrentHealth <= 0)
            Die();
        else
            StateMachine.TransitionTo("Hurt");
    }

    /// <summary>
    /// Permite a subclases o estados (como BlockState) modificar o reducir el daño entrante.
    /// </summary>
    protected virtual int ModifyIncomingDamage(int amount)
    {
        return amount;
    }

    protected virtual void Die()
    {
        StateMachine.TransitionTo("Dead");
    }
}
