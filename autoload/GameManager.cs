// res://autoload/GameManager.cs
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
    public bool HasKey       { get; private set; } = false;
    public bool HasCure      { get; private set; } = false;
    public int  CurrentLevel { get; private set; } = 1;

    public GameState State { get; private set; } = GameState.MainMenu;

    public override void _Ready()
    {
        Instance = this;
        EventBus.Instance.KeyCollected   += OnKeyCollected;
        EventBus.Instance.CureCollected  += OnCureCollected;
        EventBus.Instance.PlayerDied     += OnPlayerDied;
        EventBus.Instance.LevelCompleted += OnLevelCompleted;
    }

    public void StartGame()
    {
        Lives = 5; HasKey = false; HasCure = false; CurrentLevel = 1;
        TransitionState(GameState.Playing);
        GetTree().ChangeSceneToFile("res://levels/Level1Almacen.tscn");
    }

    public void PauseGame()  => TransitionState(GameState.Paused);
    public void ResumeGame() => TransitionState(GameState.Playing);

    private void OnPlayerDied(Character _)
    {
        Lives = Mathf.Max(0, Lives - 1);
        if (Lives <= 0) TransitionState(GameState.GameOver);
        else            GetTree().ReloadCurrentScene();
    }

    private void OnKeyCollected()          => HasKey  = true;
    private void OnCureCollected()         => HasCure = true;

    private void OnLevelCompleted(int lvl)
    {
        CurrentLevel = lvl + 1;
        if (CurrentLevel > 3)
            TransitionState(GameState.Victory);
        else
            GetTree().ChangeSceneToFile($"res://levels/Level{CurrentLevel}Almacen.tscn");
    }

    private void TransitionState(GameState next) => State = next;

    public override void _ExitTree()
    {
        EventBus.Instance.KeyCollected   -= OnKeyCollected;
        EventBus.Instance.CureCollected  -= OnCureCollected;
        EventBus.Instance.PlayerDied     -= OnPlayerDied;
        EventBus.Instance.LevelCompleted -= OnLevelCompleted;
    }
}

public enum GameState { MainMenu, Playing, Paused, Dialogue, Victory, GameOver }
