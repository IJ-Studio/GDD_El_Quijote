// res://projectiles/CurePickup.cs
using Godot;

/// <summary>
/// Pickup de la Cura de la Sazón sobre el pedestal.
/// Solo es interactuable si la plataforma de reversión fue activada previamente.
/// Al recogerla, marca GameManager.HasCure = true y emite señal.
/// </summary>
public partial class CurePickup : Area2D
{
    [Export] public float BobbingHeight { get; set; } = 30f;
    [Export] public float BobbingSpeed { get; set; } = 2.0f;
    [Export] public float RotationSpeed { get; set; } = 2.0f;

    private Vector2 _startPosition;
    private bool _collected = false;
    private ReversionPlatform? _requiredPlatform;
    private ColorRect? _visual;
    private float _bobbingTime = 0f;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _visual = GetNodeOrNull<ColorRect>("ColorRect");

        // Buscar plataforma de reversión en la escena (hermana o en padre)
        var parent = GetParent();
        _requiredPlatform = parent?.FindChild("ReversionPlatform", false, false) as ReversionPlatform;

        // Si no la encuentra, buscar en la escena raíz
        if (_requiredPlatform == null)
        {
            var root = GetTree().Root.GetChild(GetTree().Root.GetChildCount() - 1);
            _requiredPlatform = root?.FindChild("ReversionPlatform", false, false) as ReversionPlatform;
        }

        // Conectar colisión
        AreaEntered += OnAreaEntered;

        // Si no hay plataforma requerida, hacer visible siempre
        if (_requiredPlatform == null)
        {
            GD.PrintErr("[CurePickup] ReversionPlatform no encontrada - Cura siempre visible");
            Modulate = Colors.White;
        }
        else
        {
            // Iniciar invisible hasta que plataforma se active
            Modulate = Colors.Gray;
        }
    }

    public override void _Process(double delta)
    {
        if (_collected) return;

        // Incrementar tiempo para el efecto bobbing
        _bobbingTime += (float)delta;

        // Efecto bobbing (movimiento oscilante)
        var newPos = _startPosition;
        newPos.Y += Mathf.Sin(_bobbingTime * BobbingSpeed) * BobbingHeight;
        GlobalPosition = newPos;

        // Rotación continua
        Rotation += RotationSpeed * (float)delta;

        // Validar si plataforma se activó
        if (_requiredPlatform != null && _requiredPlatform.IsActivated && Modulate != Colors.White)
        {
            Modulate = Colors.White;
            GD.Print("[CurePickup] Plataforma activada - Cura desbloqueada");
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (_collected) return;

        // Validar que sea Hurtbox del jugador
        if (area is not Hurtbox) return;

        // Validar que la plataforma haya sido activada
        if (_requiredPlatform != null && !_requiredPlatform.IsActivated)
        {
            GD.Print("[CurePickup] Intento de recoger Cura sin activar plataforma - RECHAZADO");
            return;
        }

        Collect();
    }

    private void Collect()
    {
        _collected = true;

        // Marcar en GameManager
        GameManager.Instance.HasCure = true;

        // Emitir señales a través del EventBus (HUD y diálogo escuchan)
        EventBus.Instance.EmitCureCollected();
        EventBus.Instance.EmitCureObtained();
        AudioManager.Instance?.PlaySfx("curar", -4f);

        // Feedback visual: el pedestal se queda vacío (o desaparece si no hay sprite)
        var sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite?.SpriteFrames?.HasAnimation("empty") == true)
        {
            sprite.Play("empty");
        }
        else
        {
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad);
            tween.SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "modulate", Colors.Transparent, 0.3f);
            tween.TweenCallback(Callable.From(() => Hide()));
        }

        GD.Print("[CurePickup] ¡Cura de la Sazón obtenida! GameManager.HasCure = true");
    }

    public bool IsCollected => _collected;
}
