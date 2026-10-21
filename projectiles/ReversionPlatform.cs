// res://projectiles/ReversionPlatform.cs
using Godot;

/// <summary>
/// Plataforma de reversión. Al activarse through Area2D, emite una señal
/// via EventBus para despejar el camino hacia el pedestal de la Cura.
/// Muestra feedback visual cuando es activada.
/// </summary>
public partial class ReversionPlatform : StaticBody2D
{
    [Export] public float FlashDuration { get; set; } = 0.3f;

    private bool _activated = false;
    private ColorRect? _visual;
    private Tween? _flashTween;

    public override void _Ready()
    {
        _visual = GetNode<ColorRect>("ColorRect");

        var triggerZone = GetNode<Area2D>("TriggerZone");
        triggerZone.AreaEntered += OnTriggerEntered;
    }

    private void OnTriggerEntered(Area2D area)
    {
        if (_activated) return;

        // Validar que sea Hurtbox del jugador
        if (area is not Hurtbox) return;

        Activate();
    }

    private void Activate()
    {
        _activated = true;

        // Emitir señal a través del EventBus
        EventBus.Instance.EmitRevisionPlatformActivated();

        // Feedback visual: flash blanco
        if (_visual != null)
        {
            _flashTween?.Kill();
            var originalColor = _visual.Color;

            _flashTween = CreateTween();
            _flashTween.SetTrans(Tween.TransitionType.Quad);
            _flashTween.SetEase(Tween.EaseType.Out);
            _flashTween.TweenProperty(_visual, "color", Colors.White, FlashDuration * 0.5f);
            _flashTween.TweenProperty(_visual, "color", originalColor, FlashDuration * 0.5f);
        }

        // Log para debugging
        GD.Print("[ReversionPlatform] Activada - Camino hacia Cura desbloqueado");
    }

    public bool IsActivated => _activated;
}
