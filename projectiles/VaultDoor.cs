// res://projectiles/VaultDoor.cs
using Godot;

/// <summary>
/// Puerta de salida de un nivel. Se abre si el jugador cumple la condición
/// (llave en el Nivel 1, Cura de la Sazón en el Nivel 2) y avisa al GameManager
/// mediante EventBus.LevelCompleted. Si no la cumple, parpadea en rojo.
/// Para abrirla hay que MANTENER pulsada la tecla de interactuar hasta llenar la barra.
/// </summary>
public partial class VaultDoor : HoldInteractDoor
{
    [Export] public float RejectFlashDuration { get; set; } = 0.3f;

    /// <summary>Número del nivel que termina al cruzar esta puerta.</summary>
    [Export] public int LevelNumber { get; set; } = 1;

    /// <summary>Si es verdadero, además de la llave pide la Cura de la Sazón.</summary>
    [Export] public bool RequiresCure { get; set; } = false;

    private Control? _flashOverlay;
    private AnimatedSprite2D? _sprite;
    private float _flashTimer = 0f;
    private bool _isFlashing = false;

    public override void _Ready()
    {
        base._Ready();
        _flashOverlay = GetNodeOrNull<Control>("FlashOverlay");
        _sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_isFlashing) return;
        _flashTimer -= (float)delta;
        if (_flashTimer <= 0)
        {
            _isFlashing = false;
            if (_flashOverlay != null) _flashOverlay.Visible = false;
        }
    }

    private bool CanOpen()
    {
        bool keyOk = GameManager.Instance.HasKey || LevelNumber > 1;
        bool cureOk = !RequiresCure || GameManager.Instance.HasCure;
        return keyOk && cureOk;
    }

    protected override void OnHoldCompleted()
    {
        if (CanOpen())
            OnDoorUnlocked();
        else
            OnDoorLocked();
    }

    private void OnDoorUnlocked()
    {
        Resolved = true;
        GD.Print($"[VaultDoor] Puerta abierta. Terminando nivel {LevelNumber}...");
        if (_sprite?.SpriteFrames?.HasAnimation("open") == true) _sprite.Play("open");
        AudioManager.Instance?.PlaySfx("recoger", -4f);

        // Pequeña pausa para ver la puerta abrirse; luego GameManager cambia de nivel
        GetTree().CreateTimer(0.7).Timeout += () => EventBus.Instance.EmitLevelCompleted(LevelNumber);
    }

    private void OnDoorLocked()
    {
        GD.Print("[VaultDoor] Bloqueada: falta la llave o la Cura.");
        _isFlashing = true;
        _flashTimer = RejectFlashDuration;
        if (_flashOverlay != null) _flashOverlay.Visible = true;
        AudioManager.Instance?.PlaySfx("bloqueo", -10f);
    }

}
