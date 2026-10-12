// res://projectiles/VaultKey.cs
using Godot;

/// <summary>
/// Recolectable: Llave de la bóveda del Nivel 1.
/// Al colisionar con el jugador, emite la señal KeyCollected via EventBus.
/// GameManager escucha esta señal y marca HasKey = true.
/// </summary>
public partial class VaultKey : Area2D
{
    /// <summary>Rotación base de la llave (grados por segundo).</summary>
    [Export] public float RotationSpeed { get; set; } = 180f;

    /// <summary>Amplitud del movimiento vertical oscilante.</summary>
    [Export] public float BobAmplitude { get; set; } = 10f;

    /// <summary>Velocidad del movimiento oscilante (Hz).</summary>
    [Export] public float BobSpeed { get; set; } = 2f;

    private Vector2 _startPosition;
    private float _elapsedTime = 0f;

    public override void _Ready()
    {
        _startPosition = Position;

        // Conectar señal de detección de cuerpo
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        _elapsedTime += (float)delta;

        // Rotación continua
        Rotation += Mathf.DegToRad(RotationSpeed) * (float)delta;

        // Movimiento oscilante vertical (bobbing)
        float yOffset = Mathf.Sin(_elapsedTime * Mathf.Tau * BobSpeed) * BobAmplitude;

        Position = _startPosition + Vector2.Up * yOffset;
    }

    /// <summary>
    /// Reacciona cuando el jugador (o un Hurtbox) toca la llave.
    /// Emite señal global y se desactiva.
    /// </summary>
    private void OnAreaEntered(Node2D area)
    {
        // Validar que es un Hurtbox del jugador
        if (area is not Hurtbox)
            return;

        // Emitir señal global — GameManager escucha y marca HasKey = true
        EventBus.Instance.EmitKeyCollected();

        // Desaparecer (sin eliminar, solo ocultar)
        Visible = false;
        Monitorable = false;
        CollisionMask = 0;
    }
}
