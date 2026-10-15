// res://ui/MainMenu.cs
using Godot;

/// <summary>
/// Controlador de la pantalla principal (MainMenu).
/// Gestiona botones: Iniciar (1P/2P), Salir.
/// </summary>
public partial class MainMenu : Control
{
    private Button? _buttonStart1P;
    private Button? _buttonStart2P;
    private Button? _buttonExit;
    private VBoxContainer? _modeSelector;

    public override void _Ready()
    {
        _buttonStart1P = GetNode<Button>("VBoxContainer/ButtonStart1P");
        _buttonStart2P = GetNode<Button>("VBoxContainer/ButtonStart2P");
        _buttonExit = GetNode<Button>("VBoxContainer/ButtonExit");
        _modeSelector = GetNode<VBoxContainer>("VBoxContainer");

        _buttonStart1P.Pressed += OnStart1PPressed;
        _buttonStart2P.Pressed += OnStart2PPressed;
        _buttonExit.Pressed += OnExitPressed;
        _buttonStart1P.GrabFocus();

        GD.PrintRich("[color=yellow]MainMenu: Pantalla de inicio cargada[/color]");
    }

    private void OnStart1PPressed()
    {
        GD.PrintRich("[color=lime]MainMenu: Iniciando juego en Modo 1 Jugador[/color]");
        GameManager.Instance.StartGame(1);
    }

    private void OnStart2PPressed()
    {
        GD.PrintRich("[color=lime]MainMenu: Iniciando juego en Modo 2 Jugadores[/color]");
        GameManager.Instance.StartGame(2);
    }

    private void OnExitPressed()
    {
        GD.PrintRich("[color=red]MainMenu: Saliendo del juego[/color]");
        GetTree().Quit();
    }

    public override void _ExitTree()
    {
        if (_buttonStart1P is not null) _buttonStart1P.Pressed -= OnStart1PPressed;
        if (_buttonStart2P is not null) _buttonStart2P.Pressed -= OnStart2PPressed;
        if (_buttonExit is not null)    _buttonExit.Pressed -= OnExitPressed;
    }
}
