// res://combat/Hitbox.cs
using Godot;

/// <summary>
/// Área que INFLIGE daño. Solo se activa durante AttackState.Enter()
/// y se desactiva en AttackState.Exit(). Nunca permanece activa en reposo.
/// </summary>
public partial class Hitbox : Area2D
{
    [Export] public int Damage { get; set; } = 10;

    /// <summary>Si es true, no daña a un personaje que esté agachado (el golpe pasa por encima).</summary>
    [Export] public bool DodgedByCrouch { get; set; } = false;

    public override void _Ready()
    {
        Monitoring = false;   // Inactiva por defecto — la activa AttackState
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is Hurtbox hurtbox)
        {
            if (DodgedByCrouch && hurtbox.Fighter is { IsCrouching: true }) return;

            // Los golpes cuerpo a cuerpo de un enemigo no atraviesan paredes ni plataformas
            if (GetParent() is Enemy attacker && hurtbox.Fighter is { } target
                && !LineOfSight.IsClear(this, attacker.Center, target.Center))
                return;
            // El retroceso empuja lejos del atacante (o del proyectil)
            Vector2 source = GetParent() is Node2D owner ? owner.GlobalPosition : GlobalPosition;
            hurtbox.ReceiveDamage(Damage, source);
        }
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
    }
}
