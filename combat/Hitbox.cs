// res://combat/Hitbox.cs
using Godot;

/// <summary>
/// Área que INFLIGE daño. Solo se activa durante AttackState.Enter()
/// y se desactiva en AttackState.Exit(). Nunca permanece activa en reposo.
/// </summary>
public partial class Hitbox : Area2D
{
    [Export] public int Damage { get; set; } = 10;

    public override void _Ready()
    {
        Monitoring = false;   // Inactiva por defecto — la activa AttackState
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is Hurtbox hurtbox)
        {
            hurtbox.ReceiveDamage(Damage);
        }
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
    }
}
