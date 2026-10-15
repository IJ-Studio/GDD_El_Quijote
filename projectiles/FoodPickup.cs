// res://projectiles/FoodPickup.cs
using Godot;

/// <summary>
/// Comida buena (pollo, pan, queso, manzana) que recupera vida al tocarla.
/// Se coloca en los niveles y la sueltan los cofres al terminar la Oleada de Adrenalina.
/// </summary>
public partial class FoodPickup : Area2D
{
    [Export] public int HealAmount { get; set; } = 30;
    [Export] public float BobAmplitude { get; set; } = 3f;

    private Vector2 _startPosition;
    private float _time = 0f;
    private bool _taken = false;

    public override void _Ready()
    {
        _startPosition = Position;
        _time = (float)GD.RandRange(0.0, 3.0);
        var sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite?.SpriteFrames != null)
        {
            int count = sprite.SpriteFrames.GetFrameCount(sprite.Animation);
            sprite.Frame = GD.RandRange(0, Mathf.Max(0, count - 1));
        }
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        Position = _startPosition + new Vector2(0, Mathf.Sin(_time * 4f) * BobAmplitude);
    }

    private void OnAreaEntered(Area2D area)
    {
        if (_taken || area is not Hurtbox || area.GetParent() is not Character player || player is Enemy)
            return;
        if (player.CurrentHealth >= player.MaxHealth || player.CurrentHealth <= 0)
            return;

        _taken = true;
        player.Heal(HealAmount);
        AudioManager.Instance?.PlaySfx("curar", -6f);
        QueueFree();
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
    }
}
