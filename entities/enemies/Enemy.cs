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

    /// <summary>Segundos que se tambalea (sin patrullar ni atacar) tras recibir un golpe.</summary>
    [Export] public float HitStunDuration { get; set; } = 0.35f;

    /// <summary>
    /// Pausa mínima (segundos) antes de poder atacar después de recibir daño,
    /// para que no devuelva el golpe al instante.
    /// </summary>
    [Export] public float PostHitAttackDelay { get; set; } = 0.8f;

    /// <summary>
    /// Segundos que el enemigo se prepara (quieto y parpadeando en rojo) antes de que su ataque
    /// haga daño. Si el jugador le pega en ese momento, el ataque se cancela.
    /// </summary>
    [Export] public float AttackWindup { get; set; } = 0.35f;

    public float CooldownLeft { get; set; } = 0f;

    /// <summary>Tiempo restante de aturdimiento por golpe (lo respeta GuardState).</summary>
    public float StunLeft { get; set; } = 0f;

    /// <summary>Si es falso (jefe), los golpes no lo aturden, no interrumpen su ataque ni retrasan el siguiente.</summary>
    protected virtual bool ReactsToHits => true;
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
        if (StunLeft > 0f) StunLeft -= (float)delta;
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

    /// <summary>
    /// Punto desde donde salen los proyectiles (frente a la cara). Si hay una estructura entre el
    /// centro y ese punto (enemigo pegado a una pared), sale del centro para que choque con ella
    /// en lugar de aparecer al otro lado.
    /// </summary>
    public Vector2 MuzzlePosition
    {
        get
        {
            Vector2 muzzle = Center + new Vector2(Facing * BodyHeight * 0.35f, -BodyHeight * 0.1f);
            return LineOfSight.IsClear(this, Center, muzzle) ? muzzle : Center;
        }
    }

    /// <summary>Punto de disparo frente al enemigo a una altura dada (px sobre sus pies).</summary>
    public Vector2 MuzzleAtHeight(float height)
    {
        Vector2 muzzle = GlobalPosition + new Vector2(Facing * BodyHeight * 0.35f, -height);
        return LineOfSight.IsClear(this, Center, muzzle) ? muzzle : Center;
    }

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
        int healthBefore = CurrentHealth;
        base.TakeDamage(amount);

        // Solo reacciona si de verdad perdió vida y sigue vivo
        if (healthBefore <= 0 || CurrentHealth <= 0 || CurrentHealth >= healthBefore) return;

        // pequeño retroceso al recibir el golpe
        Velocity = new Vector2(KnockbackDirection * 90f, -60f);

        if (!ReactsToHits) return;

        // El golpe interrumpe un ataque en curso y le impone una pausa antes del siguiente
        if (StateMachine.CurrentStateName == "EnemyAttack")
            StateMachine.TransitionTo(IsAdrenalineActive ? "Alert" : "Guard");

        StunLeft = Mathf.Max(StunLeft, HitStunDuration);
        CooldownLeft = Mathf.Max(CooldownLeft, PostHitAttackDelay);
    }

    public override void _ExitTree()
    {
        // Desuscribirse siempre para evitar memory leaks y callbacks en instancias muertas
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineRush;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnd;
    }
}
