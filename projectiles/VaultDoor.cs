// res://projectiles/VaultDoor.cs
using Godot;

/// <summary>
/// Puerta de salida del Nivel 1 que valida GameManager.HasKey.
/// Al ser tocada por el jugador CON la llave, transiciona al Nivel 2.
/// Muestra feedback visual si se toca sin llave.
/// </summary>
public partial class VaultDoor : Area2D
{
    /// <summary>Duración de la animación de rechazo (segundos).</summary>
    [Export] public float RejectFlashDuration { get; set; } = 0.3f;

    private Control? _flashOverlay;
    private float _flashTimer = 0f;
    private bool _isFlashing = false;

    public override void _Ready()
    {
        // Buscar el overlay visual para feedback (si existe en la escena)
        _flashOverlay = GetNode<Control>("FlashOverlay") as Control;

        // Conectar señal de colisión
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        // Actualizar flash de rechazo
        if (_isFlashing)
        {
            _flashTimer -= (float)delta;
            if (_flashTimer <= 0)
            {
                _isFlashing = false;
                if (_flashOverlay != null)
                    _flashOverlay.Visible = false;
            }
        }
    }

    /// <summary>
    /// Reacciona cuando el jugador toca la puerta.
    /// Si tiene llave: transiciona al Nivel 2.
    /// Si no tiene llave: anima rechazo.
    /// </summary>
    private void OnAreaEntered(Node2D area)
    {
        // Solo reaccionar a Hurtbox del jugador
        if (area is not Hurtbox)
            return;

        if (GameManager.Instance.HasKey)
        {
            // ✓ Tiene llave → permitir paso al siguiente nivel
            OnDoorUnlocked();
        }
        else
        {
            // ✗ Sin llave → mostrar rechazo visual y sonoro
            OnDoorLocked();
        }
    }

    /// <summary>Secuencia de apertura: transición al Nivel 2.</summary>
    private void OnDoorUnlocked()
    {
        GD.Print("[VaultDoor] ¡Llave usada! Transicionando a Nivel 2...");

        // Opcional: reproducir sonido de apertura (futuro)
        // AudioPlayer.Play("door_unlock")

        // Emitir evento de nivel completado
        EventBus.Instance.EmitLevelCompleted(1);

        // Transicionar a siguiente nivel (GameManager lo maneja automáticamente)
        // Se ejecutará en OnLevelCompleted() del GameManager
    }

    /// <summary>Feedback visual y sonoro cuando se intenta abrir sin llave.</summary>
    private void OnDoorLocked()
    {
        GD.Print("[VaultDoor] ¡Requiere llave! Bloqueada.");

        // Animar flash rojo
        _isFlashing = true;
        _flashTimer = RejectFlashDuration;

        if (_flashOverlay != null)
            _flashOverlay.Visible = true;

        // Opcional: reproducir sonido de rechazo (futuro)
        // AudioPlayer.Play("door_locked_reject")
    }
}
