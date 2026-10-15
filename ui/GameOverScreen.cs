// res://ui/GameOverScreen.cs
using Godot;

/// <summary>
/// Pantalla de fin del juego (sin vidas). Permite reintentar con el mismo
/// número de jugadores o volver al menú principal.
/// </summary>
public partial class GameOverScreen : Control
{
    private Button? _buttonRetry;
    private Button? _buttonMenu;

    public override void _Ready()
    {
        _buttonRetry = GetNode<Button>("Layout/Buttons/ButtonRetry");
        _buttonMenu = GetNode<Button>("Layout/Buttons/ButtonMenu");
        _buttonRetry.Pressed += OnRetryPressed;
        _buttonMenu.Pressed += OnMenuPressed;
        _buttonRetry.GrabFocus();
    }

    private void OnRetryPressed() => GameManager.Instance.StartGame(GameManager.Instance.PlayerCount);

    private void OnMenuPressed() => GameManager.Instance.GoToMainMenu();

    public override void _ExitTree()
    {
        if (_buttonRetry is not null) _buttonRetry.Pressed -= OnRetryPressed;
        if (_buttonMenu is not null)  _buttonMenu.Pressed -= OnMenuPressed;
    }
}
