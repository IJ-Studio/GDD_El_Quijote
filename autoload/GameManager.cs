// res://autoload/GameManager.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Singleton global: vidas por personaje, llave, Cura, nivel activo y flujo de pantallas.
/// Registrado como Autoload "GameManager" en project.godot.
///
/// Cuando un personaje muere NO se recarga el nivel: pierde una de SUS vidas y reaparece
/// en su punto de inicio, así los enemigos derrotados siguen derrotados. El otro personaje
/// no se ve afectado. La partida termina cuando todos los jugadores se quedan sin vidas.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    private const int StartingLives = 5;

    // ── Estado persistente entre escenas ──────────────────────────
    private readonly int[] _lives = { StartingLives, StartingLives };

    /// <summary>Vidas restantes sumadas de los jugadores en partida.</summary>
    public int  Lives        => GetLives(0) + (PlayerCount > 1 ? GetLives(1) : 0);
    public int  CurrentLives => Lives;
    public bool HasKey       { get; set; } = false;
    public bool HasCure      { get; set; } = false;
    public int  CurrentLevel { get; private set; } = 1;
    public int  PlayerCount  { get; private set; } = 1;

    /// <summary>Enemigos derrotados en la partida. Se conserva al reaparecer y entre niveles.</summary>
    public int EnemiesDefeated { get; private set; } = 0;

    /// <summary>Segundos entre la muerte del jugador y su reaparición (deja ver la animación).</summary>
    [Export] public float RespawnDelay { get; set; } = 1.6f;

    private readonly HashSet<Character> _respawning = new();

    /// <summary>Diálogos ya vistos en esta partida (no se repiten al reintentar un nivel).</summary>
    public HashSet<string> SeenDialogues { get; } = new();

    public GameState State { get; private set; } = GameState.MainMenu;

    public override void _Ready()
    {
        Instance = this;
        ControlBindings.LoadAndApply(); // aplica las teclas elegidas en el menú de controles
        EventBus.Instance.KeyCollected   += OnKeyCollected;
        EventBus.Instance.CureCollected  += OnCureCollected;
        EventBus.Instance.PlayerDied     += OnPlayerDied;
        EventBus.Instance.EnemyKilled    += OnEnemyKilled;
        EventBus.Instance.LevelCompleted += OnLevelCompleted;
    }

    /// <summary>0 = Don Quijote, 1 = Sancho Panza.</summary>
    public static int PlayerIndexOf(Character character) => character is SanchoPanza ? 1 : 0;

    /// <summary>Vidas restantes del jugador indicado (0 = Quijote, 1 = Sancho).</summary>
    public int GetLives(int playerIndex) => _lives[Mathf.Clamp(playerIndex, 0, 1)];

    public void StartGame() => StartGame(1);

    /// <summary>Inicia una partida nueva con 1 o 2 jugadores.</summary>
    public void StartGame(int players)
    {
        PlayerCount = Mathf.Clamp(players, 1, 2);
        _respawning.Clear();
        SeenDialogues.Clear();
        _lives[0] = StartingLives;
        _lives[1] = StartingLives;
        EnemiesDefeated = 0;
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

    /// <summary>Resta una vida al jugador indicado (0 = Quijote, 1 = Sancho) y avisa al HUD.</summary>
    public void LoseLife(int playerIndex = 0)
    {
        int index = Mathf.Clamp(playerIndex, 0, 1);
        _lives[index] = Mathf.Max(0, _lives[index] - 1);
        EventBus.Instance.EmitLivesChanged(index, _lives[index]);
    }

    private void OnPlayerDied(Character player)
    {
        // Una sola gestión por muerte (la señal podría repetirse)
        if (!_respawning.Add(player)) return;

        LoseLife(PlayerIndexOf(player));

        // Espera a que se vea la animación de derrota antes de reaparecer / terminar
        GetTree().CreateTimer(RespawnDelay).Timeout += () => FinishDeath(player);
    }

    private void FinishDeath(Character player)
    {
        _respawning.Remove(player);
        if (State == GameState.GameOver || State == GameState.Victory) return;

        if (AllPlayersOutOfLives())
        {
            GameOver();
            return;
        }

        // Reaparece solo si aún le quedan vidas; los enemigos y objetos del nivel no se tocan
        if (GodotObject.IsInstanceValid(player) && player.IsInsideTree() && GetLives(PlayerIndexOf(player)) > 0)
            player.Respawn();
    }

    private bool AllPlayersOutOfLives()
    {
        for (int i = 0; i < PlayerCount; i++)
        {
            if (GetLives(i) > 0) return false;
        }
        return true;
    }

    private void GameOver()
    {
        TransitionState(GameState.GameOver);
        GetTree().ChangeSceneToFile("res://ui/GameOverScreen.tscn");
    }

    private void OnKeyCollected()          => HasKey  = true;
    private void OnCureCollected()         => HasCure = true;

    private void OnEnemyKilled(Enemy _)
    {
        EnemiesDefeated++;
        EventBus.Instance.EmitEnemiesDefeatedChanged(EnemiesDefeated);
    }

    private void OnLevelCompleted(int lvl)
    {
        _respawning.Clear();
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
        EventBus.Instance.EnemyKilled    -= OnEnemyKilled;
        EventBus.Instance.LevelCompleted -= OnLevelCompleted;
    }
}

public enum GameState { MainMenu, Playing, Paused, Dialogue, Victory, GameOver }
