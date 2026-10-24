// res://autoload/FeedbackManager.cs
using Godot;

/// <summary>
/// Retroalimentación de combate ("game feel"): destellos, partículas, números de daño,
/// chispas de bloqueo, vibración de cámara y sonidos.
/// Patrón Observer puro: solo escucha señales del EventBus; ninguna entidad lo llama directamente.
/// Registrado como Autoload "FeedbackManager" en project.godot.
/// </summary>
public partial class FeedbackManager : Node
{
    public static FeedbackManager Instance { get; private set; } = null!;

    private const string FontPath = "res://assets/fonts/PressStart2P-Regular.ttf";
    private const string SparkFolder = "res://assets/sprites/objetos/";

    private static readonly Color PlayerHitColor = new(0.95f, 0.25f, 0.2f);
    private static readonly Color BlockedDamageColor = new(0.3f, 0.6f, 1f);
    private static readonly Color EnemyHitColor  = new(1f, 0.72f, 0.25f);
    private static readonly Color SauceColor     = new(0.85f, 0.12f, 0.08f);

    private Font? _font;
    private SpriteFrames? _sparkFrames;
    private SpriteFrames? _meatballBurstFrames;

    public override void _Ready()
    {
        Instance = this;
        _font = ResourceLoader.Exists(FontPath) ? GD.Load<Font>(FontPath) : null;
        _sparkFrames = BuildFrames("chispa_bloqueo", 4, 14f);
        _meatballBurstFrames = BuildFrames("albondiga_explota", 3, 10f);

        EventBus.Instance.DamageDealt       += OnDamageDealt;
        EventBus.Instance.AttackBlocked     += OnAttackBlocked;
        EventBus.Instance.ProjectileBurst   += OnProjectileBurst;
        EventBus.Instance.AdrenalineStarted += OnAdrenalineStarted;
        EventBus.Instance.KeyCollected      += OnKeyCollected;
    }

    // ── Reacciones a señales ──────────────────────────────────────

    private void OnDamageDealt(Character target, int amount)
    {
        if (!IsInstanceValid(target)) return;
        bool isPlayer = target is not Enemy;
        bool killed = target.CurrentHealth <= 0;

        Flash(target);
        Burst(target.Center, isPlayer ? PlayerHitColor : EnemyHitColor, killed ? 26 : 10, killed ? 1.4f : 0.8f);
        FloatingText(target.Center + new Vector2(0, -target.BodyHeight * 0.55f), $"-{amount}",
            !isPlayer ? Colors.White : target.LastDamageWasBlocked ? BlockedDamageColor : PlayerHitColor, 10);

        if (isPlayer)
        {
            Shake(killed ? 0.7f : 0.45f);
            HitStop(target.LastDamageWasBlocked ? 0.03f : 0.06f);
            AudioManager.Instance?.PlaySfx("dano", -4f);
        }
        else
        {
            Shake(killed ? 0.4f : 0.18f);
            HitStop(killed ? 0.09f : 0.05f);
            AudioManager.Instance?.PlaySfx(killed ? "enemigo_muere" : "golpe", -4f);
        }
    }

    // ── Hitstop: congelar el juego una fracción de segundo al conectar un golpe ──
    private ulong _hitStopUntilMs;
    private int _hitStopId;

    /// <summary>
    /// Casi detiene el tiempo unos milisegundos para que cada golpe "pese".
    /// Si llegan varios golpes seguidos, manda el que termina más tarde; solo ese
    /// devuelve el tiempo a la normalidad.
    /// </summary>
    private async void HitStop(float seconds)
    {
        if (GetTree().Paused) return;
        ulong until = Time.GetTicksMsec() + (ulong)(seconds * 1000f);
        if (until <= _hitStopUntilMs) return;
        _hitStopUntilMs = until;
        int id = ++_hitStopId;
        Engine.TimeScale = 0.05;

        // Temporizador que ignora la escala de tiempo (si no, tardaría 20 veces más)
        await ToSignal(GetTree().CreateTimer(seconds, true, false, true), SceneTreeTimer.SignalName.Timeout);
        if (id == _hitStopId) Engine.TimeScale = 1.0;
    }

    private void OnAttackBlocked(Character target)
    {
        if (!IsInstanceValid(target)) return;
        Vector2 at = target.Center + new Vector2(target.Facing * 22f, -6f);
        PlayOneShot(_sparkFrames, at, target.Sprite.FlipH);
        FloatingText(target.Center + new Vector2(0, -target.BodyHeight * 0.6f), "¡BLOQUEO!", new Color(1f, 0.9f, 0.5f), 8);
        Shake(0.15f);
        AudioManager.Instance?.PlaySfx("bloqueo", -6f);
    }

    private void OnProjectileBurst(Vector2 position, int projectileType)
    {
        if ((ProjectileType)projectileType == ProjectileType.Albondiga)
        {
            PlayOneShot(_meatballBurstFrames, position, false);
            Shake(0.12f);
        }
        else
        {
            Burst(position, SauceColor, 12, 0.7f);
        }
    }

    private void OnAdrenalineStarted()
    {
        Shake(0.5f);
        AudioManager.Instance?.PlaySfx("dash", -2f);
    }

    private void OnKeyCollected()
    {
        Shake(0.12f);
    }

    // ── Efectos reutilizables ─────────────────────────────────────

    /// <summary>Destello blanco del sprite (usa SelfModulate para no pelear con el parpadeo de invulnerabilidad).</summary>
    private static void Flash(Character target)
    {
        target.Sprite.SelfModulate = new Color(4f, 4f, 4f);
        var tween = target.CreateTween();
        tween.TweenProperty(target.Sprite, "self_modulate", Colors.White, 0.16f);
    }

    /// <summary>Estallido de partículas cuadradas (pixel) en todas direcciones.</summary>
    private void Burst(Vector2 position, Color color, int amount, float strength)
    {
        var parent = EffectsParent();
        if (parent == null) return;

        var particles = new CpuParticles2D
        {
            Emitting = false,
            OneShot = true,
            Amount = amount,
            Lifetime = 0.55f,
            Explosiveness = 0.95f,
            Spread = 180f,
            Gravity = new Vector2(0, 520),
            InitialVelocityMin = 60f * strength,
            InitialVelocityMax = 190f * strength,
            ScaleAmountMin = 2.5f,
            ScaleAmountMax = 5f,
            Color = color,
            ZIndex = 20,
        };
        var ramp = new Gradient();
        ramp.SetColor(0, Colors.White);
        ramp.SetColor(1, new Color(1, 1, 1, 0));
        particles.ColorRamp = ramp;
        parent.AddChild(particles);
        particles.GlobalPosition = position;
        particles.Emitting = true;
        particles.Finished += particles.QueueFree;
    }

    /// <summary>Texto que sube y se desvanece ("-10", "¡BLOQUEO!").</summary>
    private void FloatingText(Vector2 position, string text, Color color, int size)
    {
        var parent = EffectsParent();
        if (parent == null) return;

        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(140, 0),
            ZIndex = 50,
        };
        if (_font != null) label.AddThemeFontOverride("font", _font);
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 4);
        parent.AddChild(label);
        label.GlobalPosition = position - new Vector2(70, 8);

        var tween = label.CreateTween();
        tween.TweenProperty(label, "position:y", label.Position.Y - 34f, 0.7f)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.Parallel().TweenProperty(label, "modulate:a", 0f, 0.3f).SetDelay(0.4f);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    /// <summary>Reproduce una animación una sola vez y la elimina al terminar.</summary>
    private void PlayOneShot(SpriteFrames? frames, Vector2 position, bool flip)
    {
        var parent = EffectsParent();
        if (parent == null || frames == null) return;

        var sprite = new AnimatedSprite2D
        {
            SpriteFrames = frames,
            FlipH = flip,
            ZIndex = 25,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        };
        parent.AddChild(sprite);
        sprite.GlobalPosition = position;
        sprite.AnimationFinished += sprite.QueueFree;
        sprite.Play("default");
    }

    /// <summary>Pide a la cámara del nivel que vibre (grupo "camara").</summary>
    private void Shake(float amount)
    {
        GetTree().CallGroup("camara", "Shake", amount);
    }

    private Node? EffectsParent() => GetTree().CurrentScene;

    private static SpriteFrames? BuildFrames(string baseName, int count, float fps)
    {
        var frames = new SpriteFrames();
        frames.SetAnimationLoopMode("default", SpriteFrames.LoopMode.None);
        frames.SetAnimationSpeed("default", fps);
        for (int i = 0; i < count; i++)
        {
            string path = $"{SparkFolder}{baseName}_{i}.png";
            if (!ResourceLoader.Exists(path)) return null;
            frames.AddFrame("default", GD.Load<Texture2D>(path));
        }
        return frames;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.DamageDealt       -= OnDamageDealt;
        EventBus.Instance.AttackBlocked     -= OnAttackBlocked;
        EventBus.Instance.ProjectileBurst   -= OnProjectileBurst;
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineStarted;
        EventBus.Instance.KeyCollected      -= OnKeyCollected;
    }
}
