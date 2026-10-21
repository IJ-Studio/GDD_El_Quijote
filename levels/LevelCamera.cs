// res://levels/LevelCamera.cs
using Godot;

/// <summary>
/// Cámara del nivel: sigue el punto medio entre los jugadores vivos y vibra
/// cuando FeedbackManager se lo pide (grupo "camara", método Shake).
/// Con 1 jugador hace más zoom sobre su personaje; con 2 se aleja para que quepan los dos.
/// </summary>
public partial class LevelCamera : Camera2D
{
    [Export] public float FollowSpeed { get; set; } = 6f;
    [Export] public float Decay { get; set; } = 2.4f;
    [Export] public Vector2 MaxShakeOffset { get; set; } = new(14, 10);

    /// <summary>Zoom con un solo jugador (más cerca del personaje).</summary>
    [Export] public float SinglePlayerZoom { get; set; } = 1.6f;

    /// <summary>Zoom con dos jugadores (más lejos, para ver a ambos).</summary>
    [Export] public float CoopZoom { get; set; } = 1.0f;

    /// <summary>Qué tan rápido se acerca o aleja la cámara al cambiar el zoom.</summary>
    [Export] public float ZoomSpeed { get; set; } = 4f;

    private float _trauma = 0f;

    public override void _Ready()
    {
        AddToGroup("camara");
        Zoom = Vector2.One * DesiredZoom();
        GlobalPosition = TargetPosition() ?? GlobalPosition;
        ResetSmoothing();
    }

    /// <summary>Agrega "trauma" (0 a 1). Se calma solo con el tiempo.</summary>
    public void Shake(float amount)
    {
        _trauma = Mathf.Min(_trauma + amount, 1f);
    }

    private float DesiredZoom() => GameManager.Instance.PlayerCount > 1 ? CoopZoom : SinglePlayerZoom;

    public override void _Process(double delta)
    {
        float zoom = Mathf.Lerp(Zoom.X, DesiredZoom(), 1f - Mathf.Exp(-ZoomSpeed * (float)delta));
        Zoom = Vector2.One * zoom;

        Vector2? target = TargetPosition();
        if (target.HasValue)
        {
            GlobalPosition = GlobalPosition.Lerp(target.Value, 1f - Mathf.Exp(-FollowSpeed * (float)delta));
        }

        if (_trauma > 0f)
        {
            _trauma = Mathf.Max(_trauma - Decay * (float)delta, 0f);
            float strength = _trauma * _trauma;
            Offset = new Vector2(
                MaxShakeOffset.X * strength * (float)GD.RandRange(-1.0, 1.0),
                MaxShakeOffset.Y * strength * (float)GD.RandRange(-1.0, 1.0));
        }
        else
        {
            Offset = Vector2.Zero;
        }
    }

    private Vector2? TargetPosition()
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;
        foreach (Node node in GetTree().GetNodesInGroup("players"))
        {
            if (node is Character c && c.CurrentHealth > 0)
            {
                sum += c.Center;
                count++;
            }
        }
        return count > 0 ? sum / count : null;
    }
}
