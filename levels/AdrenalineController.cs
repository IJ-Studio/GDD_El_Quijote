// res://levels/AdrenalineController.cs
using Godot;
using System.Collections.Generic;

/// <summary>
/// Gestiona la Oleada de Adrenalina en un nivel.
/// FSM de dos estados: Normal y Adrenaline.
/// Detonador: Area2D sobre un cofre. Temporizador: 15-30 segundos.
/// La oleada termina cuando NO hay enemigos en la zona de influencia.
/// </summary>
public partial class AdrenalineController : Node
{
    // ── Exportados al Inspector ────────────────────────────────────
    [Export] public Area2D TriggerZone { get; private set; } = null!;
    [Export] public float MinDuration { get; set; } = 15.0f;
    [Export] public float MaxDuration { get; set; } = 30.0f;

    // ── Estado interno ─────────────────────────────────────────────
    private AdrenalineState _currentState = AdrenalineState.Normal;
    private float _stateTimer = 0f;
    private RandomNumberGenerator _rng = new();

    // Enemigos activos dentro de la zona de influencia
    private HashSet<Enemy> _enemiesInZone = new();

    public override void _Ready()
    {
        if (TriggerZone == null)
            TriggerZone = GetNode<Area2D>("TriggerZone");

        // Suscribir a detectar enemigos
        TriggerZone.AreaEntered += OnAreaEntered;
        TriggerZone.AreaExited += OnAreaExited;

        // Limpiar referencias a enemigos eliminados
        EventBus.Instance.EnemyKilled += OnEnemyKilled;
    }

    public override void _Process(double delta)
    {
        if (_currentState == AdrenalineState.Adrenaline)
        {
            _stateTimer -= (float)delta;

            // Verificar si quedan enemigos en la zona
            RemoveDeadEnemies();

            // Terminar oleada si:
            // 1. Timer agota, O
            // 2. No hay enemigos en la zona
            if (_stateTimer <= 0f || _enemiesInZone.Count == 0)
            {
                EndAdrenalineRush();
            }
        }
    }

    /// <summary>
    /// Inicia la Oleada de Adrenalina con temporizador aleatorio [15-30]s.
    /// </summary>
    private void StartAdrenalineRush()
    {
        if (_currentState != AdrenalineState.Normal)
            return;

        _currentState = AdrenalineState.Adrenaline;
        _stateTimer = _rng.Randf() * (MaxDuration - MinDuration) + MinDuration;

        GD.Print($"[ADRENALINE] Oleada iniciada. Duración: {_stateTimer:F2}s");
        EventBus.Instance.EmitAdrenalineStarted();
    }

    /// <summary>
    /// Termina la Oleada de Adrenalina.
    /// </summary>
    private void EndAdrenalineRush()
    {
        if (_currentState != AdrenalineState.Adrenaline)
            return;

        _currentState = AdrenalineState.Normal;
        _stateTimer = 0f;
        _enemiesInZone.Clear();

        GD.Print("[ADRENALINE] Oleada finalizada.");
        EventBus.Instance.EmitAdrenalineEnded();
    }

    /// <summary>
    /// Cuando un nodo entra en la zona de influencia del cofre.
    /// </summary>
    private void OnAreaEntered(Node2D body)
    {
        // Detectar enemies
        if (body is Enemy enemy)
        {
            _enemiesInZone.Add(enemy);
            GD.Print($"[ADRENALINE] Enemigo detectado en zona: {enemy.Name}");

            // Iniciar oleada si es la primera vez
            if (_currentState == AdrenalineState.Normal)
            {
                StartAdrenalineRush();
            }
        }
    }

    /// <summary>
    /// Cuando un nodo sale de la zona de influencia del cofre.
    /// </summary>
    private void OnAreaExited(Node2D body)
    {
        if (body is Enemy enemy)
        {
            _enemiesInZone.Remove(enemy);
            GD.Print($"[ADRENALINE] Enemigo salió de la zona: {enemy.Name}");
        }
    }

    /// <summary>
    /// Al eliminar un enemigo, notificación global.
    /// </summary>
    private void OnEnemyKilled(Enemy enemy)
    {
        _enemiesInZone.Remove(enemy);
        GD.Print($"[ADRENALINE] Enemigo eliminado: {enemy.Name}");
    }

    /// <summary>
    /// Limpia referencias a enemigos que ya no existen (fueron QueueFree'd).
    /// </summary>
    private void RemoveDeadEnemies()
    {
        var deadEnemies = new List<Enemy>();
        foreach (var enemy in _enemiesInZone)
        {
            if (!GodotObject.IsInstanceValid(enemy))
                deadEnemies.Add(enemy);
        }

        foreach (var dead in deadEnemies)
            _enemiesInZone.Remove(dead);
    }

    public override void _ExitTree()
    {
        if (TriggerZone != null)
        {
            TriggerZone.AreaEntered -= OnAreaEntered;
            TriggerZone.AreaExited -= OnAreaExited;
        }
        EventBus.Instance.EnemyKilled -= OnEnemyKilled;
    }

    private enum AdrenalineState { Normal, Adrenaline }
}
