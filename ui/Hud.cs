// res://ui/Hud.cs
using Godot;

/// <summary>
/// HUD principal del juego.
/// Escucha exclusivamente las señales del EventBus para actualizar la interfaz (vidas, salud, coleccionables)
/// manteniendo un desacoplamiento estricto de la lógica interna de los personajes.
/// </summary>
public partial class Hud : CanvasLayer
{
    [Export] private Label?       _livesLabel;
    [Export] private TextureRect? _keyIcon;
    [Export] private TextureRect? _cureIcon;
    [Export] private ProgressBar? _healthBar;

    public override void _Ready()
    {
        EventBus.Instance.HealthChanged     += OnHealthChanged;
        EventBus.Instance.KeyCollected      += OnKeyCollected;
        EventBus.Instance.CureCollected     += OnCureCollected;
        EventBus.Instance.PlayerDied        += OnPlayerDied;
        EventBus.Instance.AdrenalineStarted += OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded   += OnAdrenalineEnded;

        UpdateLivesDisplay();
        if (_keyIcon  is not null) _keyIcon.Visible  = GameManager.Instance.HasKey;
        if (_cureIcon is not null) _cureIcon.Visible = GameManager.Instance.HasCure;
    }

    private void OnHealthChanged(Character c, int current, int max)
    {
        if (c is DonQuijote && _healthBar is not null)
        {
            _healthBar.MaxValue = max;
            _healthBar.Value = current;
        }
    }

    private void OnPlayerDied(Character _)
    {
        UpdateLivesDisplay();
    }

    private void OnKeyCollected()
    {
        if (_keyIcon is not null) _keyIcon.Visible = true;
    }

    private void OnCureCollected()
    {
        if (_cureIcon is not null) _cureIcon.Visible = true;
    }

    private void OnAdrenalineStarted()
    {
        // Indicador visual de adrenalina en HUD
    }

    private void OnAdrenalineEnded()
    {
        // Revertir indicador visual de adrenalina en HUD
    }

    private void UpdateLivesDisplay()
    {
        if (_livesLabel is not null)
        {
            _livesLabel.Text = $"Vidas: {GameManager.Instance.Lives}";
        }
    }

    public override void _ExitTree()
    {
        EventBus.Instance.HealthChanged     -= OnHealthChanged;
        EventBus.Instance.KeyCollected      -= OnKeyCollected;
        EventBus.Instance.CureCollected     -= OnCureCollected;
        EventBus.Instance.PlayerDied        -= OnPlayerDied;
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnded;
    }
}
