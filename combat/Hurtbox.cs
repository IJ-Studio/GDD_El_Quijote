// res://combat/Hurtbox.cs
using Godot;

/// <summary>
/// Área que RECIBE daño. Delega a TakeDamage del Character dueño.
/// </summary>
public partial class Hurtbox : Area2D
{
    [Export] public NodePath OwnerPath { get; set; } = new NodePath("..");
    private Character? _owner;

    public override void _Ready()
    {
        _owner = GetNode<Character>(OwnerPath);
    }

    public void ReceiveDamage(int amount)
    {
        _owner?.TakeDamage(amount);
    }
}
