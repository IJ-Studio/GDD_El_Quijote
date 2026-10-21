// res://ui/Hud.cs
using Godot;

/// <summary>
/// El HUD solo ESCUCHA señales del EventBus (patrón Observer).
/// Muestra la vida de cada jugador, las vidas restantes (corazones), la llave,
/// la Cura, el aviso de Oleada de Adrenalina y la barra del jefe.
/// </summary>
public partial class Hud : CanvasLayer
{
    [Export] private ProgressBar?   _healthBar;
    [Export] private ProgressBar?   _healthBarP2;
    [Export] private Control?       _panelP2;
    [Export] private HBoxContainer? _hearts;
    [Export] private Label?         _livesLabel;
    [Export] private HBoxContainer? _heartsP2;
    [Export] private Label?         _livesLabelP2;
    [Export] private Label?         _killsLabel;
    [Export] private TextureRect?   _keyIcon;
    [Export] private TextureRect?   _cureIcon;
    [Export] private Control?       _adrenalineBanner;
    [Export] private TextureRect?   _adrenalineFrame;
    [Export] private TextureRect?   _damageFrame;
    [Export] private Control?       _bossPanel;
    [Export] private ProgressBar?   _bossBar;
    [Export] private Texture2D?     _heartFull;
    [Export] private Texture2D?     _heartEmpty;

    private Tween? _adrenalineTween;
    private Tween? _damageTween;

    private static readonly Color BlockedLossColor = new(0.3f, 0.6f, 1f);
    private float _lastPercentP1 = 100f;
    private float _lastPercentP2 = 100f;

    public override void _Ready()
    {
        EventBus.Instance.HealthChanged     += OnHealthChanged;
        EventBus.Instance.KeyCollected      += OnKeyCollected;
        EventBus.Instance.CureCollected     += OnCureCollected;
        EventBus.Instance.LivesChanged           += OnLivesChanged;
        EventBus.Instance.EnemiesDefeatedChanged += OnEnemiesDefeatedChanged;
        EventBus.Instance.AdrenalineStarted      += OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded        += OnAdrenalineEnded;

        // Instanciar PauseMenu como child de este HUD
        var pauseMenuScene = GD.Load<PackedScene>("res://ui/PauseMenu.tscn");
        if (pauseMenuScene is not null) AddChild(pauseMenuScene.Instantiate<Control>());

        // Estado inicial
        if (_panelP2 is not null)    _panelP2.Visible = GameManager.Instance.PlayerCount > 1;
        if (_keyIcon is not null)    _keyIcon.Modulate = GameManager.Instance.HasKey ? Colors.White : new Color(0, 0, 0, 0.45f);
        if (_cureIcon is not null)   _cureIcon.Modulate = GameManager.Instance.HasCure ? Colors.White : new Color(0, 0, 0, 0.45f);
        if (_adrenalineBanner is not null) _adrenalineBanner.Visible = false;
        if (_adrenalineFrame is not null)  _adrenalineFrame.Visible = false;
        if (_damageFrame is not null)      _damageFrame.Visible = false;
        if (_bossPanel is not null)  _bossPanel.Visible = false;
        UpdateLivesDisplay(_hearts, _livesLabel, GameManager.Instance.GetLives(0));
        UpdateLivesDisplay(_heartsP2, _livesLabelP2, GameManager.Instance.GetLives(1));
        OnEnemiesDefeatedChanged(GameManager.Instance.EnemiesDefeated);
    }

    private void OnHealthChanged(Character c, int current, int max)
    {
        float percent = max > 0 ? (float)current / max * 100f : 0f;
        switch (c)
        {
            case DonQuijote when _healthBar is not null:
                ShowLostChunk(_healthBar, _lastPercentP1, percent, c.LastDamageWasBlocked);
                FlashDamageFrame(_lastPercentP1, percent, c.LastDamageWasBlocked);
                _lastPercentP1 = percent;
                _healthBar.Value = percent;
                break;
            case SanchoPanza when _healthBarP2 is not null:
                ShowLostChunk(_healthBarP2, _lastPercentP2, percent, c.LastDamageWasBlocked);
                FlashDamageFrame(_lastPercentP2, percent, c.LastDamageWasBlocked);
                _lastPercentP2 = percent;
                _healthBarP2.Value = percent;
                break;
            case TacoMayor when _bossBar is not null && _bossPanel is not null:
                _bossPanel.Visible = current > 0;
                _bossBar.Value = percent;
                break;
        }
    }

    /// <summary>
    /// Marca en la barra el trozo de vida que se acaba de perder y lo desvanece.
    /// Si el golpe llegó con el escudo arriba, el trozo es azul.
    /// </summary>
    private void ShowLostChunk(ProgressBar bar, float previous, float current, bool blocked)
    {
        if (!blocked || current >= previous) return;
        float width = bar.Size.X;
        var chunk = new ColorRect
        {
            Color = BlockedLossColor,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(width * current / 100f, 0f),
            Size = new Vector2(width * (previous - current) / 100f, bar.Size.Y)
        };
        bar.AddChild(chunk);
        var tween = chunk.CreateTween();
        tween.TweenInterval(0.35);
        tween.TweenProperty(chunk, "modulate:a", 0f, 0.6f);
        tween.TweenCallback(Callable.From(chunk.QueueFree));
    }

    /// <summary>Destello de viñeta roja cuando un personaje recibe daño (no si el escudo lo bloqueó).</summary>
    private void FlashDamageFrame(float previous, float current, bool blocked)
    {
        if (_damageFrame is null || blocked || current >= previous) return;
        TextureRect frame = _damageFrame;
        _damageTween?.Kill();
        frame.Visible = true;
        frame.Modulate = Colors.White;
        _damageTween = CreateTween();
        _damageTween.TweenProperty(frame, "modulate:a", 0f, 0.6f);
        _damageTween.TweenCallback(Callable.From(() => frame.Visible = false));
    }

    private void OnLivesChanged(int playerIndex, int lives)
    {
        // Cada personaje tiene sus propias vidas: se actualiza solo su fila
        if (playerIndex == 0) UpdateLivesDisplay(_hearts, _livesLabel, lives);
        else                  UpdateLivesDisplay(_heartsP2, _livesLabelP2, lives);
    }

    private void OnEnemiesDefeatedChanged(int total)
    {
        if (_killsLabel is not null) _killsLabel.Text = $"DERROTADOS: {total}";
    }

    private void OnKeyCollected()
    {
        if (_keyIcon is null) return;
        _keyIcon.Modulate = Colors.White;
        Pop(_keyIcon);
    }

    private void OnCureCollected()
    {
        if (_cureIcon is null) return;
        _cureIcon.Modulate = Colors.White;
        Pop(_cureIcon);
    }

    private void OnAdrenalineStarted()
    {
        if (_adrenalineBanner is not null) { _adrenalineBanner.Visible = true; Pop(_adrenalineBanner); }
        if (_adrenalineFrame is null) return;
        _adrenalineFrame.Visible = true;
        _adrenalineTween?.Kill();
        _adrenalineTween = CreateTween().SetLoops();
        _adrenalineTween.TweenProperty(_adrenalineFrame, "modulate:a", 0.25f, 0.45f);
        _adrenalineTween.TweenProperty(_adrenalineFrame, "modulate:a", 1f, 0.45f);
    }

    private void OnAdrenalineEnded()
    {
        _adrenalineTween?.Kill();
        if (_adrenalineBanner is not null) _adrenalineBanner.Visible = false;
        if (_adrenalineFrame is not null)  _adrenalineFrame.Visible = false;
    }

    /// <summary>Dibuja un corazón lleno por cada vida del personaje al que pertenece la fila.</summary>
    private void UpdateLivesDisplay(HBoxContainer? hearts, Label? label, int livesRemaining)
    {
        int lives = Mathf.Max(0, livesRemaining);
        if (label is not null) label.Text = $"x{lives}";
        if (hearts is null) return;
        for (int i = 0; i < hearts.GetChildCount(); i++)
        {
            if (hearts.GetChild(i) is TextureRect heart)
                heart.Texture = i < lives ? _heartFull : _heartEmpty;
        }
    }

    private void Pop(Control control)
    {
        control.PivotOffset = control.Size / 2f;
        control.Scale = new Vector2(1.5f, 1.5f);
        var tween = CreateTween();
        tween.TweenProperty(control, "scale", Vector2.One, 0.25f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    public override void _ExitTree()
    {
        EventBus.Instance.HealthChanged     -= OnHealthChanged;
        EventBus.Instance.KeyCollected      -= OnKeyCollected;
        EventBus.Instance.CureCollected     -= OnCureCollected;
        EventBus.Instance.LivesChanged           -= OnLivesChanged;
        EventBus.Instance.EnemiesDefeatedChanged -= OnEnemiesDefeatedChanged;
        EventBus.Instance.AdrenalineStarted      -= OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded        -= OnAdrenalineEnded;
    }
}
