// res://levels/LevelController.cs
using Godot;

/// <summary>
/// Orquesta la escena de un nivel (capa NIVEL del GDD): quita a Sancho en modo
/// de 1 jugador, registra a los jugadores y lanza los diálogos del nivel.
/// No contiene lógica de combate.
/// </summary>
public partial class LevelController : Node2D
{
    [Export] public int LevelNumber { get; set; } = 1;

    /// <summary>Diálogo al entrar al nivel por primera vez (vacío = ninguno).</summary>
    [Export] public string IntroDialogue { get; set; } = "";

    /// <summary>Diálogo al obtener la Cura de la Sazón (vacío = ninguno).</summary>
    [Export] public string CureDialogue { get; set; } = "";

    public override void _Ready()
    {
        var sancho = GetNodeOrNull<Node>("SanchoPanza");
        if (sancho != null && GameManager.Instance.PlayerCount < 2)
        {
            sancho.QueueFree();
        }

        foreach (string name in new[] { "DonQuijote", "SanchoPanza" })
        {
            var player = GetNodeOrNull<Node>(name);
            if (player != null && !player.IsQueuedForDeletion()) player.AddToGroup("players");
        }

        EventBus.Instance.CureObtained += OnCureObtained;

        if (!string.IsNullOrEmpty(IntroDialogue))
            CallDeferred(MethodName.RequestDialogue, IntroDialogue);
    }

    private void OnCureObtained()
    {
        if (!string.IsNullOrEmpty(CureDialogue))
            RequestDialogue(CureDialogue);
    }

    private void RequestDialogue(string id)
    {
        if (GameManager.Instance.SeenDialogues.Contains(id)) return;
        GameManager.Instance.SeenDialogues.Add(id);
        EventBus.Instance.EmitDialogueRequested(id);
    }

    public override void _ExitTree()
    {
        EventBus.Instance.CureObtained -= OnCureObtained;
    }
}
