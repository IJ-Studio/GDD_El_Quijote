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
    [Export] private Control?       _bossPanel;
    [Export] private ProgressBar?   _bossBar;
    [Export] private Texture2D?     _heartFull;
    [Export] private Texture2D?     _heartEmpty;

    private Tween? _adrenalineTween;

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
                _healthBar.Value = percent;
                break;
            case SanchoPanza when _healthBarP2 is not null:
                _healthBarP2.Value = percent;
                break;
            case TacoMayor when _bossBar is not null && _bossPanel is not null:
                _bossPanel.Visible = current > 0;
                _bossBar.Value = percent;
                break;
        }
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
