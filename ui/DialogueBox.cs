// res://ui/DialogueBox.cs
using Godot;

/// <summary>
/// Caja de diálogo para desplegar las líneas de historia.
/// Muestra réplicas de El Patrón y el Fantasma del Chef Fundador antes del nivel.
/// Presionar Enter/Espacio avanza al siguiente diálogo.
/// </summary>
public partial class DialogueBox : Control
{
    private Label? _labelCharacter;
    private Label? _labelDialogue;
    private Panel? _dialoguePanel;
    private int _currentDialogueIndex = 0;
    private bool _dialogueComplete = false;

    private static readonly string[] DialogueLines =
    {
        "El Patrón: ¡La Cura de la Sazón ha desaparecido!",
        "El Patrón: Don Quijote, debes recuperarla de la bóveda mágica.",
        "El Fantasma del Chef: ¡Cuidado! La oleada de Adrenalina despertará a mis sirvientes.",
        "El Fantasma del Chef: Vence al Taco Mayor y la Cura será tuya.",
        "El Patrón: ¡Que la suerte te acompañe, valiente caballero!"
    };

    public override void _Ready()
    {
        _labelCharacter = GetNode<Label>("DialoguePanel/VBoxContainer/LabelCharacter");
        _labelDialogue = GetNode<Label>("DialoguePanel/VBoxContainer/LabelDialogue");
        _dialoguePanel = GetNode<Panel>("DialoguePanel");

        _currentDialogueIndex = 0;
        DisplayNextDialogue();

        GD.PrintRich("[color=yellow]DialogueBox: Caja de diálogo inicializada[/color]");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("ui_select"))
        {
            if (!_dialogueComplete)
            {
                DisplayNextDialogue();
            }
            else
            {
                CloseDialogueBox();
            }
            GetTree().Root.SetInputAsHandled();
        }
    }

    private void DisplayNextDialogue()
    {
        if (_currentDialogueIndex >= DialogueLines.Length)
        {
            _dialogueComplete = true;
            if (_labelDialogue is not null)
                _labelDialogue.Text = "[Presiona Enter para continuar]";
            return;
        }

        string line = DialogueLines[_currentDialogueIndex];
        string[] parts = line.Split(": ", 2);

        if (_labelCharacter is not null && parts.Length > 0)
            _labelCharacter.Text = parts[0];

        if (_labelDialogue is not null && parts.Length > 1)
            _labelDialogue.Text = parts[1];

        _currentDialogueIndex++;
    }

    private void CloseDialogueBox()
    {
        GD.PrintRich("[color=lime]DialogueBox: Diálogo completado, cerrando[/color]");
        Visible = false;
        GetTree().Root.SetInputAsHandled();
        QueueFree();
    }
}
