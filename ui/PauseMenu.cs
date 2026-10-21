// res://ui/PauseMenu.cs
using Godot;

/// <summary>
/// Controlador de la pantalla de pausa.
/// Accesible con tecla Escape/P. Permite reanudar o volver al menú principal.
/// </summary>
public partial class PauseMenu : Control
{
    private Button? _buttonResume;
    private Button? _buttonControls;
    private Button? _buttonMainMenu;
    private bool _isPauseMenuVisible = false;

    public override void _Ready()
    {
        _buttonResume = GetNode<Button>("VBoxContainer/ButtonResume");
        _buttonControls = GetNode<Button>("VBoxContainer/ButtonControls");
        _buttonMainMenu = GetNode<Button>("VBoxContainer/ButtonMainMenu");

        _buttonResume.Pressed += OnResumePressed;
        _buttonControls.Pressed += OnControlsPressed;
        _buttonMainMenu.Pressed += OnMainMenuPressed;

        // Inicialmente oculto; debe seguir recibiendo input con el juego en pausa
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        GD.PrintRich("[color=yellow]PauseMenu: Script de pausa inicializado[/color]");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // No abrir la pausa encima de un diálogo
        bool dialogueOpen = GetTree().Paused && !_isPauseMenuVisible;
        if ((@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("pause_menu")) && !dialogueOpen)
        {
            TogglePauseMenu();
            GetTree().Root.SetInputAsHandled();
        }
    }

    private void TogglePauseMenu()
    {
        _isPauseMenuVisible = !_isPauseMenuVisible;
        Visible = _isPauseMenuVisible;

        if (_isPauseMenuVisible)
        {
            GetTree().Paused = true;
            _buttonResume?.GrabFocus();
            GD.PrintRich("[color=yellow]PauseMenu: Juego pausado[/color]");
        }
        else
        {
            GetTree().Paused = false;
            GD.PrintRich("[color=lime]PauseMenu: Juego reanudado[/color]");
        }
    }

    private void OnResumePressed()
    {
        GetTree().Paused = false;
        Visible = false;
        _isPauseMenuVisible = false;
        GD.PrintRich("[color=lime]PauseMenu: Reanudando juego[/color]");
    }

    private void OnControlsPressed()
    {
        var menu = GD.Load<PackedScene>("res://ui/ControlsMenu.tscn").Instantiate<ControlsMenu>();
        menu.Closed += () => _buttonControls?.GrabFocus();
        AddChild(menu);
    }

    private void OnMainMenuPressed()
    {
        GetTree().Paused = false;
        GD.PrintRich("[color=orange]PauseMenu: Volviendo al menú principal[/color]");
        GameManager.Instance.GoToMainMenu();
    }

    public override void _ExitTree()
    {
        if (_buttonResume is not null)    _buttonResume.Pressed -= OnResumePressed;
        if (_buttonControls is not null)  _buttonControls.Pressed -= OnControlsPressed;
        if (_buttonMainMenu is not null)  _buttonMainMenu.Pressed -= OnMainMenuPressed;
    }
}
