// res://levels/TestHazard.cs
using Godot;

/// <summary>
/// Peligro o enemigo de prueba que inflige daño al entrar en contacto con la Hurtbox del personaje.
/// Utilizado para probar el sistema de vida, HUD y game over en el Nivel 1.
/// </summary>
public partial class TestHazard : Area2D
{
    [Export] public int Damage { get; set; } = 30;

    public override void _Ready()
    {
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
