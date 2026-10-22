// res://projectiles/BaseProjectile.cs
using Godot;
using System;

/// <summary>
/// Clase base para todos los proyectiles gestionados por el ProjectilePool.
/// </summary>
public abstract partial class BaseProjectile : Area2D
{
    [Export] public float Speed { get; set; } = 300f;
    [Export] public int Damage { get; set; } = 10;
    [Export] public float Lifetime { get; set; } = 5f;

    /// <summary>
    /// Multiplicador de daño de este lanzamiento (el jefe lo sube). Vuelve a 1 cada vez que el
    /// proyectil sale del pool; hay que fijarlo justo antes de llamar a Launch.
    /// </summary>
    public float DamageMultiplier { get; set; } = 1f;

    public ProjectilePool? Pool { get; set; }
    public ProjectileType Type { get; set; }

    protected Vector2 Direction = Vector2.Zero;
    protected float CurrentLifetime = 0f;
    protected Hitbox? Hitbox;

    public override void _Ready()
    {
        Hitbox = GetNodeOrNull<Hitbox>("Hitbox");
        if (Hitbox != null)
        {
            Hitbox.Damage = Enemy.ScaleDamage(Damage);
        }
        
        // Conectar señal de impacto si es necesario, o dejar que la Hitbox lo maneje
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
    }

    public virtual void Launch(Vector2 position, Vector2 direction)
    {
        if (Hitbox != null) Hitbox.Damage = Enemy.ScaleDamage(Mathf.RoundToInt(Damage * DamageMultiplier));
        GlobalPosition = position;
        Direction = direction.Normalized();
        Rotation = Direction.Angle();
        CurrentLifetime = 0f;
        
        // Activar procesamiento
        SetProcess(true);
        Show();
    }

    public override void _Process(double delta)
    {
        Position += Direction * Speed * (float)delta;
        
        CurrentLifetime += (float)delta;
        if (CurrentLifetime >= Lifetime)
        {
            ReturnToPool();
        }
    }

    public virtual void Enable()
    {
        DamageMultiplier = 1f;
        Show();
        SetProcess(true);
        SetDeferred(Area2D.PropertyName.Monitoring, true);
        SetDeferred(Area2D.PropertyName.Monitorable, true);
        Hitbox?.SetDeferred(Area2D.PropertyName.Monitoring, true);
    }

    public virtual void Disable()
    {
        Hide();
        SetProcess(false);
        SetDeferred(Area2D.PropertyName.Monitoring, false);
        SetDeferred(Area2D.PropertyName.Monitorable, false);
        Hitbox?.SetDeferred(Area2D.PropertyName.Monitoring, false);
    }

    protected virtual void OnAreaEntered(Area2D area)
    {
        // Si impacta con algo relevante, se devuelve al pool
        if (area is Hurtbox hurtbox)
        {
            // Si el objetivo lo esquiva agachándose, el proyectil sigue de largo
            if (Hitbox is { DodgedByCrouch: true } && hurtbox.Fighter is { IsCrouching: true }) return;
            ReturnToPool();
        }
    }

    protected virtual void OnBodyEntered(Node2D body)
    {
        // Si impacta con el escenario (TileMap, etc)
        if (body is not Character)
        {
            ReturnToPool();
        }
    }

    protected void ReturnToPool()
    {
        Pool?.Return(this);
    }
}
