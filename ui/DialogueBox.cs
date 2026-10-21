// res://ui/DialogueBox.cs
using Godot;

/// <summary>
/// Caja de diálogo con ilustración. Escucha EventBus.DialogueRequested, pausa el juego,
/// muestra las líneas con efecto de máquina de escribir y al terminar emite DialogueFinished.
/// Avanza con Enter, Espacio, J o clic.
/// </summary>
public partial class DialogueBox : CanvasLayer
{
    [Export] public float CharactersPerSecond { get; set; } = 48f;

    private TextureRect? _image;
    private Label? _labelCharacter;
    private Label? _labelDialogue;
    private Label? _hint;

    private DialogueScene? _scene;
    private string _currentId = "";
    private int _lineIndex = 0;
    private float _visibleChars = 0f;

    public bool IsOpen => Visible;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _image = GetNodeOrNull<TextureRect>("Root/Image");
        _labelCharacter = GetNodeOrNull<Label>("Root/DialoguePanel/Margin/VBox/LabelCharacter");
        _labelDialogue = GetNodeOrNull<Label>("Root/DialoguePanel/Margin/VBox/LabelDialogue");
        _hint = GetNodeOrNull<Label>("Root/DialoguePanel/Hint");
        Visible = false;
        EventBus.Instance.DialogueRequested += Open;
    }

    /// <summary>Abre el diálogo indicado (si existe en DialogueLibrary).</summary>
    public void Open(string dialogueId)
    {
        if (!DialogueLibrary.TryGet(dialogueId, out var scene)) return;

        _scene = scene;
        _currentId = dialogueId;
        _lineIndex = 0;
        if (_image is not null && ResourceLoader.Exists(scene.ImagePath))
            _image.Texture = GD.Load<Texture2D>(scene.ImagePath);

        Visible = true;
        GetTree().Paused = true;
        ShowLine();
    }

    private void ShowLine()
    {
        if (_scene is null) return;
        var line = _scene.Lines[_lineIndex];
        if (_labelCharacter is not null) _labelCharacter.Text = line.Speaker;
        if (_labelDialogue is not null)
        {
            _labelDialogue.Text = line.Text;
            _labelDialogue.VisibleCharacters = 0;
        }
        _visibleChars = 0f;
    }

    public override void _Process(double delta)
    {
        if (!Visible || _labelDialogue is null) return;
        int total = _labelDialogue.Text.Length;
        if (_labelDialogue.VisibleCharacters < total)
        {
            _visibleChars += CharactersPerSecond * (float)delta;
            _labelDialogue.VisibleCharacters = Mathf.Min(total, (int)_visibleChars);
        }
        if (_hint is not null)
        {
            _hint.Visible = _labelDialogue.VisibleCharacters >= total
                            && Mathf.Sin((float)Time.GetTicksMsec() * 0.008f) > -0.3f;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        bool advance = @event.IsActionPressed("ui_accept")
                       || @event.IsActionPressed("player1_attack")
                       || @event.IsActionPressed("player2_attack")
                       || (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left });
        if (!advance) return;

        GetViewport().SetInputAsHandled();

        if (_labelDialogue is not null && _labelDialogue.VisibleCharacters < _labelDialogue.Text.Length)
        {
            _labelDialogue.VisibleCharacters = _labelDialogue.Text.Length;
            return;
        }

        _lineIndex++;
        if (_scene is not null && _lineIndex < _scene.Lines.Count)
            ShowLine();
        else
            Close();
    }

    private void Close()
    {
        Visible = false;
        GetTree().Paused = false;
        EventBus.Instance.EmitDialogueFinished(_currentId);
        GD.PrintRich($"[color=lime]DialogueBox: diálogo '{_currentId}' terminado[/color]");
    }

    public override void _ExitTree()
    {
        EventBus.Instance.DialogueRequested -= Open;
    }
}
