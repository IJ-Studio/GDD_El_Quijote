// res://entities/Character.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Clase base abstracta para TODOS los personajes del juego.
/// NO instanciar directamente. Usar subclases concretas.
/// Incluye ventana de invulnerabilidad (iframes) tras recibir daño
/// y la orientación (Facing) que voltea sprite y Hitbox a la vez.
/// </summary>
public abstract partial class Character : CharacterBody2D
{
    // ── Exportados al Inspector de Godot ──────────────────────────
    [Export] public float Speed     { get; protected set; } = 150f;
    [Export] public int   MaxHealth { get; protected set; } = 100;
    [Export] public float InvulnerabilityDuration { get; set; } = 0.5f;
    [Export] public string InputPrefix { get; set; } = "player1_";

    /// <summary>Alto aproximado del cuerpo; sirve para centrar efectos y proyectiles.</summary>
    [Export] public float BodyHeight { get; set; } = 70f;

    // ── Estado en tiempo de ejecución ─────────────────────────────
    public int CurrentHealth { get; protected set; }
    public bool IsInvulnerable { get; protected set; } = false;
    private float _invulnerabilityTimer = 0f;

    /// <summary>1 = mira a la derecha, -1 = mira a la izquierda.</summary>
    public int Facing { get; private set; } = 1;

    /// <summary>Centro del cuerpo en coordenadas globales (el origen del nodo está en los pies).</summary>
    public Vector2 Center => GlobalPosition + new Vector2(0, -BodyHeight / 2f);

    public static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    // ── Dependencias internas ──────────────────────────────────────
    public StateMachine  StateMachine { get; protected set; } = null!;
    public AnimatedSprite2D Sprite    { get; protected set; } = null!;
    public Hitbox? Hitbox            { get; protected set; }
    public Hurtbox? Hurtbox          { get; protected set; }

    private readonly List<(CollisionShape2D Shape, float BaseX)> _hitboxShapes = new();

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        StateMachine  = GetNode<StateMachine>("StateMachine");
        Sprite        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Intentar obtener Hitbox y Hurtbox de forma opcional si existen en la jerarquía del nodo
        Hitbox  = GetNodeOrNull<Hitbox>("Hitbox");
        Hurtbox = GetNodeOrNull<Hurtbox>("Hurtbox");

        if (Hitbox is not null)
        {
            foreach (Node child in Hitbox.GetChildren())
            {
                if (child is CollisionShape2D shape)
                    _hitboxShapes.Add((shape, shape.Position.X));
            }
        }

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

    /// <summary>
    /// Voltea al personaje: el sprite y las formas de la Hitbox se reflejan juntos,
    /// así el ataque siempre sale hacia donde mira.
    /// </summary>
    public void SetFacing(int direction)
    {
        if (direction == 0) return;
        Facing = direction > 0 ? 1 : -1;
        Sprite.FlipH = Facing < 0;
        foreach (var (shape, baseX) in _hitboxShapes)
        {
            shape.Position = new Vector2(baseX * Facing, shape.Position.Y);
        }
    }

    // ── Input con perfiles player1_ / player2_ ────────────────────
    public bool IsActionPressed(string action)
    {
        foreach (string name in CandidateActions(action))
        {
            if (Input.IsActionPressed(name)) return true;
        }
        return false;
    }

    public bool IsActionJustPressed(string action)
    {
        foreach (string name in CandidateActions(action))
        {
            if (Input.IsActionJustPressed(name)) return true;
        }
        return false;
    }

    public float GetAxis(string negAction, string posAction)
    {
        float negative = IsActionPressed(negAction) ? 1f : 0f;
        float positive = IsActionPressed(posAction) ? 1f : 0f;
        return positive - negative;
    }

    /// <summary>
    /// Devuelve las acciones del InputMap que corresponden a este personaje
    /// (ej. "attack" → "player1_attack"). Solo devuelve las que existen,
    /// para no llenar la consola de errores.
    /// </summary>
    private IEnumerable<string> CandidateActions(string action)
    {
        string clean = action.StartsWith("player_") ? action.Substring(7) : action;
        string prefixed = InputPrefix + clean;
        if (InputMap.HasAction(prefixed)) yield return prefixed;
        if (clean.StartsWith("ui_") && InputMap.HasAction(clean)) yield return clean;
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
    /// Emite HealthChanged y DamageDealt vía EventBus. Llama a Die() si CurrentHealth llega a 0.
    /// </summary>
    /// <param name="amount">Daño a aplicar (valor positivo).</param>
    public virtual void TakeDamage(int amount)
    {
        if (IsInvulnerable || CurrentHealth <= 0) return;

        int finalDamage = ModifyIncomingDamage(amount);
        if (finalDamage < amount)
        {
            EventBus.Instance.EmitAttackBlocked(this);
        }
        if (finalDamage <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - finalDamage);
        EventBus.Instance.EmitHealthChanged(this, CurrentHealth, MaxHealth);
        EventBus.Instance.EmitDamageDealt(this, finalDamage);

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

    /// <summary>Recupera vida (comida). No supera MaxHealth.</summary>
    public virtual void Heal(int amount)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        EventBus.Instance.EmitHealthChanged(this, CurrentHealth, MaxHealth);
    }

    protected virtual void Die()
    {
        IsInvulnerable = false;
        Sprite.Modulate = Colors.White;
        StateMachine.TransitionTo("Dead");
    }
}
