// res://levels/LevelController.cs
using Godot;

/// <summary>
/// Orquesta la escena de un nivel (capa NIVEL del GDD): quita a Sancho en modo
/// de 1 jugador (y a quien ya no tenga vidas), registra a los jugadores y lanza
/// los diálogos del nivel.
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
        string[] playerNames = { "DonQuijote", "SanchoPanza" };
        for (int i = 0; i < playerNames.Length; i++)
        {
            var player = GetNodeOrNull<Node>(playerNames[i]);
            if (player == null) continue;

            // Sancho solo juega en modo de 2 jugadores; quien se quedó sin vidas no vuelve a entrar
            bool absent = (i == 1 && GameManager.Instance.PlayerCount < 2) || GameManager.Instance.GetLives(i) <= 0;
            if (absent)
            {
                player.QueueFree();
                continue;
            }
            player.AddToGroup("players");
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
