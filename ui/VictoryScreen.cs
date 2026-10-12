// res://ui/VictoryScreen.cs
using Godot;

/// <summary>
/// Pantalla de victoria mostrando "¡Cura Recuperada!" y créditos del equipo.
/// Se muestra al completar Level 3 (derrotar TacoMayor con Cura + Lives >= 1).
/// </summary>
public partial class VictoryScreen : Control
{
    private Label? _labelTitle;
    private Label? _labelCredits;
    private Button? _buttonMainMenu;
    private Button? _buttonExit;

    private static readonly string Credits = @"
╔══════════════════════════════════════════════╗
║          ¡CURA RECUPERADA - VICTORIA!        ║
╚══════════════════════════════════════════════╝

🎮 EQUIPO DE DESARROLLO
━━━━━━━━━━━━━━━━━━━━━━━━━
Game Designer & Architect
  → [YourName]

Programador C# & Godot
  → [YourName]

Artista 2D & Animator
  → [YourName]

Audio Engineer & Composer
  → [YourName]

QA & Testing
  → [YourName]

┌──────────────────────────────────────────────┐
│    Gracias por jugar El Quijote              │
│    Una aventura de Polimorfismo y Destino    │
└──────────────────────────────────────────────┘

Presiona 'Menú Principal' para volver.
";

    public override void _Ready()
    {
        _labelTitle = GetNode<Label>("VBoxContainer/LabelTitle");
        _labelCredits = GetNode<Label>("VBoxContainer/ScrollContainer/LabelCredits");
        _buttonMainMenu = GetNode<Button>("VBoxContainer/ButtonMainMenu");
        _buttonExit = GetNode<Button>("VBoxContainer/ButtonExit");

        if (_labelTitle is not null)
            _labelTitle.Text = "¡CURA RECUPERADA!";

        if (_labelCredits is not null)
            _labelCredits.Text = Credits;

        _buttonMainMenu.Pressed += OnMainMenuPressed;
        _buttonExit.Pressed += OnExitPressed;

        GD.PrintRich("[color=gold]VictoryScreen: ¡VICTORIA DEL JUEGO![/color]");
    }

    private void OnMainMenuPressed()
    {
        GD.PrintRich("[color=cyan]VictoryScreen: Volviendo a menú principal[/color]");
        GetTree().ChangeSceneToFile("res://ui/MainMenu.tscn");
    }

    private void OnExitPressed()
    {
        GD.PrintRich("[color=red]VictoryScreen: Saliendo del juego[/color]");
        GetTree().Quit();
    }

    public override void _ExitTree()
    {
        if (_buttonMainMenu is not null) _buttonMainMenu.Pressed -= OnMainMenuPressed;
        if (_buttonExit is not null)     _buttonExit.Pressed -= OnExitPressed;
    }
}
