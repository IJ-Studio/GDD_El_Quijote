// res://ui/VictoryScreen.cs
using Godot;

/// <summary>
/// Pantalla de victoria: ilustración final, mensaje del Patrón y créditos del equipo.
/// </summary>
public partial class VictoryScreen : Control
{
    private Label? _labelTitle;
    private Label? _labelCredits;
    private Button? _buttonMainMenu;
    private Button? _buttonExit;

    private const string Credits =
        "IJ-STUDIO\n\n" +
        "Irving Aldahir Angeles Romero\n" +
        "José Alfredo Hernández Arellano\n\n" +
        "Optativa 1: Creación de Videojuegos\n" +
        "Docente: Juvencio Mendoza Castelán\n\n" +
        "Gracias por jugar El Quijote";

    public override void _Ready()
    {
        _labelTitle = GetNode<Label>("Layout/LabelTitle");
        _labelCredits = GetNode<Label>("Layout/LabelCredits");
        _buttonMainMenu = GetNode<Button>("Layout/Buttons/ButtonMainMenu");
        _buttonExit = GetNode<Button>("Layout/Buttons/ButtonExit");

        var quote = GetNodeOrNull<Label>("Layout/LabelQuote");
        if (quote is not null && DialogueLibrary.TryGet("victoria", out var scene))
        {
            quote.Text = string.Join(" ", System.Linq.Enumerable.Select(scene.Lines, l => l.Text));
        }

        _labelTitle.Text = "¡CURA RECUPERADA!";
        _labelCredits.Text = Credits;

        _buttonMainMenu.Pressed += OnMainMenuPressed;
        _buttonExit.Pressed += OnExitPressed;
        _buttonMainMenu.GrabFocus();
        AudioManager.Instance?.PlaySfx("curar", -2f);
        GD.PrintRich("[color=gold]VictoryScreen: ¡VICTORIA DEL JUEGO![/color]");
    }

    private void OnMainMenuPressed() => GameManager.Instance.GoToMainMenu();

    private void OnExitPressed() => GetTree().Quit();

    public override void _ExitTree()
    {
        if (_buttonMainMenu is not null) _buttonMainMenu.Pressed -= OnMainMenuPressed;
        if (_buttonExit is not null)     _buttonExit.Pressed -= OnExitPressed;
    }
}
