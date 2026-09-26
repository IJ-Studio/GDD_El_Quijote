using Godot;

/// <summary>
/// Hub de señales globales. Autoload registrado en project.godot como "EventBus".
/// </summary>
public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; } = null!;

    // ── Señales de vida ────────────────────────────────────────────
    [Signal] public delegate void HealthChangedEventHandler(Character character, int current, int max);
    [Signal] public delegate void PlayerDiedEventHandler(Character player);

    // ── Señales de coleccionables ──────────────────────────────────
    [Signal] public delegate void KeyCollectedEventHandler();
    [Signal] public delegate void CureCollectedEventHandler();

    // ── Señales de Oleada de Adrenalina ───────────────────────────
    [Signal] public delegate void AdrenalineStartedEventHandler();
    [Signal] public delegate void AdrenalineEndedEventHandler();
    [Signal] public delegate void EnemyGroupDefeatedEventHandler();

    // ── Señales de nivel ──────────────────────────────────────────
    [Signal] public delegate void LevelCompletedEventHandler(int levelNumber);
    [Signal] public delegate void EnemyKilledEventHandler(Enemy enemy);

    public override void _Ready() => Instance = this;

    // Métodos de emisión tipados — evitan SignalName strings dispersos en el código
    public void EmitHealthChanged(Character c, int cur, int max)
        => EmitSignal(SignalName.HealthChanged, c, cur, max);
    public void EmitAdrenalineStarted()    => EmitSignal(SignalName.AdrenalineStarted);
    public void EmitAdrenalineEnded()      => EmitSignal(SignalName.AdrenalineEnded);
    public void EmitEnemyGroupDefeated()   => EmitSignal(SignalName.EnemyGroupDefeated);
    public void EmitKeyCollected()         => EmitSignal(SignalName.KeyCollected);
    public void EmitCureCollected()        => EmitSignal(SignalName.CureCollected);
    public void EmitLevelCompleted(int n)  => EmitSignal(SignalName.LevelCompleted, n);
    public void EmitEnemyKilled(Enemy e)   => EmitSignal(SignalName.EnemyKilled, e);
    public void EmitPlayerDied(Character p) => EmitSignal(SignalName.PlayerDied, p);
}
