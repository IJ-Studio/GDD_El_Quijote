// res://entities/enemies/Enemy.cs
using Godot;

/// <summary>
/// Base abstracta para todos los enemigos. Suscribe y desuscribe automáticamente
/// los eventos de la Oleada de Adrenalina, y ofrece utilidades comunes:
/// detección del jugador en la GuardZone, patrulla corta y tiempo de espera entre ataques.
/// </summary>
public abstract partial class Enemy : Character
{
    /// <summary>Grupo al que pertenecen todos los enemigos (lo usa el EnemySpawner para contarlos).</summary>
    public const string GroupName = "enemies";

    /// <summary>
    /// Ajuste global de balance: multiplica el tiempo de espera entre ataques de TODOS los enemigos
    /// (1.6 = atacan con un 60 % más de pausa, es decir, más lento).
    /// </summary>
    public static float AttackCooldownScale { get; set; } = 1.6f;

    /// <summary>
    /// Ajuste global de balance: multiplica el daño por ataque de TODOS los enemigos
    /// (cuerpo a cuerpo, proyectiles y explosiones). 0.6 = hacen un 40 % menos de daño.
    /// </summary>
    public static float DamageScale { get; set; } = 0.6f;

    /// <summary>Aplica <see cref="DamageScale"/> a un daño base; nunca baja de 1.</summary>
    public static int ScaleDamage(int baseDamage) => Mathf.Max(1, Mathf.RoundToInt(baseDamage * DamageScale));

    [Export] public Area2D? GuardZone { get; private set; }

    /// <summary>Segundos que espera en guardia entre un ataque y el siguiente.</summary>
    [Export] public float GuardCooldown { get; set; } = 1.4f;

    /// <summary>Distancia (px) que patrulla a cada lado de su posición inicial. 0 = no patrulla.</summary>
    [Export] public float PatrolDistance { get; set; } = 50f;

    public float CooldownLeft { get; set; } = 0f;
    public float PatrolOriginX { get; private set; }
    public bool IsAdrenalineActive { get; private set; } = false;

    public override void _Ready()
    {
        base._Ready();
        AddToGroup(GroupName);

        // Balance: todos los enemigos atacan más despacio y hacen menos daño por golpe
        GuardCooldown *= AttackCooldownScale;
        if (Hitbox is not null)
            Hitbox.Damage = ScaleDamage(Hitbox.Damage);

        PatrolOriginX = GlobalPosition.X;
        SetFacing(-1);
        CooldownLeft = GuardCooldown * 0.5f;
        EventBus.Instance.AdrenalineStarted += OnAdrenalineRush;
        EventBus.Instance.AdrenalineEnded   += OnAdrenalineEnd;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CooldownLeft > 0f) CooldownLeft -= (float)delta;
    }

    /// <summary>Primer jugador vivo dentro de la GuardZone (o null).</summary>
    public Character? FindTarget()
    {
        if (GuardZone == null) return null;
        foreach (var body in GuardZone.GetOverlappingBodies())
        {
            if (body is Character c and not Enemy && c.CurrentHealth > 0)
                return c;
        }
        return null;
    }

    /// <summary>Gira hacia el personaje indicado.</summary>
    public void FaceTowards(Node2D target)
    {
        SetFacing(target.GlobalPosition.X < GlobalPosition.X ? -1 : 1);
    }

    /// <summary>Punto desde donde salen los proyectiles (frente a la cara).</summary>
    public Vector2 MuzzlePosition => Center + new Vector2(Facing * BodyHeight * 0.35f, -BodyHeight * 0.1f);

    /// <summary>
    /// Respuesta base ante la Oleada de Adrenalina: duplica velocidad y
    /// transiciona a Alert. Las subclases pueden sobreescribir.
    /// </summary>
    protected virtual void OnAdrenalineRush()
    {
        if (IsAdrenalineActive || CurrentHealth <= 0) return;
        IsAdrenalineActive = true;
        Speed *= 2f;
        Sprite.SpeedScale *= 2f;
        GuardCooldown *= 0.5f;
        StateMachine.TransitionTo("Alert");
    }

    protected virtual void OnAdrenalineEnd()
    {
        if (!IsAdrenalineActive) return;
        IsAdrenalineActive = false;
        Speed /= 2f;
        Sprite.SpeedScale /= 2f;
        GuardCooldown *= 2f;
        if (CurrentHealth > 0) StateMachine.TransitionTo("Guard");
    }

    public override void TakeDamage(int amount)
    {
        bool wasAlive = CurrentHealth > 0;
        base.TakeDamage(amount);
        // pequeño retroceso al recibir el golpe
        if (wasAlive && CurrentHealth > 0)
            Velocity = new Vector2(-Facing * 90f, -60f);
    }

    public override void _ExitTree()
    {
        // Desuscribirse siempre para evitar memory leaks y callbacks en instancias muertas
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineRush;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnd;
    }
}
