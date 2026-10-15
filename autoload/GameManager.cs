// res://autoload/GameManager.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Singleton global: vidas, llave, Cura, nivel activo y flujo de pantallas.
/// Registrado como Autoload "GameManager" en project.godot.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    // ── Estado persistente entre escenas ──────────────────────────
    public int  Lives        { get; private set; } = 5;
    public int  CurrentLives => Lives;
    public bool HasKey       { get; set; } = false;
    public bool HasCure      { get; set; } = false;
    public int  CurrentLevel { get; private set; } = 1;
    public int  PlayerCount  { get; private set; } = 1;

    /// <summary>Segundos entre la muerte del jugador y el reinicio del nivel (deja ver la animación).</summary>
    [Export] public float RespawnDelay { get; set; } = 1.6f;

    private bool _respawnPending = false;

    /// <summary>Diálogos ya vistos en esta partida (no se repiten al reintentar un nivel).</summary>
    public HashSet<string> SeenDialogues { get; } = new();

    public GameState State { get; private set; } = GameState.MainMenu;

    public override void _Ready()
    {
        Instance = this;
        EventBus.Instance.KeyCollected   += OnKeyCollected;
        EventBus.Instance.CureCollected  += OnCureCollected;
        EventBus.Instance.PlayerDied     += OnPlayerDied;
        EventBus.Instance.LevelCompleted += OnLevelCompleted;
    }

    public void StartGame() => StartGame(1);

    /// <summary>Inicia una partida nueva con 1 o 2 jugadores.</summary>
    public void StartGame(int players)
    {
        PlayerCount = Mathf.Clamp(players, 1, 2);
        _respawnPending = false;
        SeenDialogues.Clear();
        Lives = 5;
        HasKey = false;
        HasCure = false;
        CurrentLevel = 1;
        TransitionState(GameState.Playing);
        GD.PrintRich($"[color=lime]GameManager: Iniciando juego - Nivel {CurrentLevel}[/color]");
        GetTree().ChangeSceneToFile(GetLevelScenePath(CurrentLevel));
    }

    /// <summary>Reinicia el estado de partida sin cambiar de escena (pruebas).</summary>
    public void GoToMainMenu()
    {
        TransitionState(GameState.MainMenu);
        GD.PrintRich("[color=cyan]GameManager: Volviendo a Main Menu[/color]");
        GetTree().ChangeSceneToFile("res://ui/MainMenu.tscn");
    }

    public void PauseGame()
    {
        TransitionState(GameState.Paused);
        GetTree().Paused = true;
        GD.PrintRich("[color=yellow]GameManager: Juego pausado[/color]");
    }

    public void ResumeGame()
    {
        TransitionState(GameState.Playing);
        GetTree().Paused = false;
        GD.PrintRich("[color=lime]GameManager: Juego reanudado[/color]");
    }

    /// <summary>
    /// Pierde una vida. Transiciona a GameOver si llega a 0, o recarga la escena actual.
    /// </summary>
    public void LoseLife()
    {
        Lives = Mathf.Max(0, Lives - 1);
        if (Lives <= 0)
        {
            TransitionState(GameState.GameOver);
            GetTree().ChangeSceneToFile("res://ui/GameOverScreen.tscn");
        }
        else
        {
            GetTree().ReloadCurrentScene();
        }
    }

    private void OnPlayerDied(Character _)
    {
        // Espera a que se vea la animación de derrota antes de reiniciar
        if (_respawnPending) return;
        _respawnPending = true;
        GetTree().CreateTimer(RespawnDelay).Timeout += () =>
        {
            _respawnPending = false;
            LoseLife();
        };
    }

    private void OnKeyCollected()          => HasKey  = true;
    private void OnCureCollected()         => HasCure = true;

    private void OnLevelCompleted(int lvl)
    {
        CurrentLevel = lvl + 1;
        if (CurrentLevel > 3)
        {
            TransitionState(GameState.Victory);
            GD.PrintRich("[color=gold]GameManager: ¡VICTORIA! Mostrando pantalla de victoria[/color]");
            GetTree().ChangeSceneToFile("res://ui/VictoryScreen.tscn");
        }
        else
        {
            GD.PrintRich($"[color=cyan]GameManager: Transición a Nivel {CurrentLevel}[/color]");
            GetTree().ChangeSceneToFile(GetLevelScenePath(CurrentLevel));
        }
    }

    private void TransitionState(GameState next) => State = next;

    /// <summary>
    /// Obtiene la ruta del archivo de la escena correspondiente a cada nivel.
    /// Resuelve de forma robusta la estructura canónica de carpetas del proyecto.
    /// </summary>
    private static string GetLevelScenePath(int level) => level switch
    {
        1 => "res://levels/Level1Almacen.tscn",
        2 => "res://levels/Level2BovedaDeLaCura.tscn",
        3 => "res://levels/Level3Cocina.tscn",
        _ => "res://levels/Level1Almacen.tscn"
    };

    public override void _ExitTree()
    {
        EventBus.Instance.KeyCollected   -= OnKeyCollected;
        EventBus.Instance.CureCollected  -= OnCureCollected;
        EventBus.Instance.PlayerDied     -= OnPlayerDied;
        EventBus.Instance.LevelCompleted -= OnLevelCompleted;
    }
}

public enum GameState { MainMenu, Playing, Paused, Dialogue, Victory, GameOver }
