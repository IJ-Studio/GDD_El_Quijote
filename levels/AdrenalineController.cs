// res://levels/AdrenalineController.cs
using Godot;
using System.Collections.Generic;

/// <summary>
/// Gestiona la Oleada de Adrenalina en un nivel.
/// FSM de dos estados: Normal y Adrenaline.
/// Detonador: un jugador se acerca al cofre (TriggerZone). Temporizador: 15-30 segundos.
/// La oleada termina cuando se agota el tiempo o cuando caen todos los enemigos
/// que estaban dentro de la zona al detonarse (el "grupo disparador").
/// </summary>
public partial class AdrenalineController : Node2D
{
    // ── Exportados al Inspector ────────────────────────────────────
    [Export] public Area2D TriggerZone { get; set; } = null!;
    [Export] public float MinDuration { get; set; } = 15.0f;
    [Export] public float MaxDuration { get; set; } = 30.0f;

    /// <summary>Escena de comida que suelta el cofre al terminar la oleada (opcional).</summary>
    [Export] public PackedScene? RewardScene { get; set; }

    // ── Estado interno ─────────────────────────────────────────────
    private AdrenalineState _currentState = AdrenalineState.Normal;
    private float _stateTimer = 0f;
    private bool _used = false;
    private readonly RandomNumberGenerator _rng = new();
    private AnimatedSprite2D? _chest;

    // Enemigos activos dentro de la zona de influencia
    private readonly HashSet<Enemy> _enemiesInZone = new();

    public bool IsActive => _currentState == AdrenalineState.Adrenaline;

    public override void _Ready()
    {
        if (TriggerZone == null)
            TriggerZone = GetNode<Area2D>("TriggerZone");
        _chest = GetNodeOrNull<AnimatedSprite2D>("Cofre");

        TriggerZone.BodyEntered += OnBodyEntered;
        TriggerZone.BodyExited += OnBodyExited;

        // Limpiar referencias a enemigos eliminados
        EventBus.Instance.EnemyKilled += OnEnemyKilled;

        // Modo permanente: Si MinDuration == 0 && MaxDuration == 0, oleada siempre activa (Level 3)
        if (MinDuration == 0f && MaxDuration == 0f)
        {
            CallDeferred(MethodName.StartPermanentAdrenaline);
        }
    }

    public override void _Process(double delta)
    {
        if (_currentState != AdrenalineState.Adrenaline || _stateTimer == float.MaxValue)
            return;

        _stateTimer -= (float)delta;
        RemoveDeadEnemies();

        // Terminar oleada si se agota el tiempo o ya no quedan enemigos del grupo
        if (_stateTimer <= 0f || _enemiesInZone.Count == 0)
        {
            EndAdrenalineRush();
        }
    }

    /// <summary>
    /// Inicia la Oleada de Adrenalina con temporizador aleatorio [Min-Max]s.
    /// </summary>
    private void StartAdrenalineRush()
    {
        if (_currentState != AdrenalineState.Normal || _used)
            return;

        // El grupo disparador son los enemigos que están en la zona en este momento
        foreach (var body in TriggerZone.GetOverlappingBodies())
        {
            if (body is Enemy enemy && enemy.CurrentHealth > 0)
                _enemiesInZone.Add(enemy);
        }
        if (_enemiesInZone.Count == 0)
            return;

        _used = true;
        _currentState = AdrenalineState.Adrenaline;
        _stateTimer = _rng.Randf() * (MaxDuration - MinDuration) + MinDuration;
        if (_chest?.SpriteFrames?.HasAnimation("shake") == true) _chest.Play("shake");

        GD.Print($"[ADRENALINE] Oleada iniciada. Duración: {_stateTimer:F2}s, enemigos: {_enemiesInZone.Count}");
        EventBus.Instance.EmitAdrenalineStarted();
    }

    /// <summary>
    /// Termina la Oleada de Adrenalina y abre el cofre.
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
        EventBus.Instance.EmitEnemyGroupDefeated();
        OpenChest();
    }

    private void OpenChest()
    {
        if (_chest?.SpriteFrames?.HasAnimation("open") == true) _chest.Play("open");
        if (RewardScene == null || _chest == null) return;
        for (int i = 0; i < 2; i++)
        {
            var reward = RewardScene.Instantiate<Node2D>();
            GetParent().CallDeferred(Node.MethodName.AddChild, reward);
            reward.GlobalPosition = _chest.GlobalPosition + new Vector2(-24 + i * 48, -46);
        }
    }

    /// <summary>
    /// Un jugador se acerca al cofre: detona la oleada. Los enemigos que entran se rastrean.
    /// </summary>
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            if (_currentState == AdrenalineState.Adrenaline)
                _enemiesInZone.Add(enemy);
            return;
        }

        if (body is Character && _currentState == AdrenalineState.Normal)
        {
            StartAdrenalineRush();
        }
    }

    private void OnBodyExited(Node2D body)
    {
        // Los enemigos del grupo siguen contando aunque salgan persiguiendo al jugador
    }

    /// <summary>
    /// Al eliminar un enemigo, notificación global.
    /// </summary>
    private void OnEnemyKilled(Enemy enemy)
    {
        _enemiesInZone.Remove(enemy);
    }

    /// <summary>
    /// Limpia referencias a enemigos que ya no existen (fueron QueueFree'd) o están muriendo.
    /// </summary>
    private void RemoveDeadEnemies()
    {
        _enemiesInZone.RemoveWhere(e => !GodotObject.IsInstanceValid(e) || e.CurrentHealth <= 0);
    }

    /// <summary>
    /// Inicia la Oleada de Adrenalina de forma permanente sin temporizador.
    /// Utilizado en Level 3 (Jefe Final) donde MinDuration == 0 && MaxDuration == 0.
    /// </summary>
    private void StartPermanentAdrenaline()
    {
        _currentState = AdrenalineState.Adrenaline;
        _stateTimer = float.MaxValue; // Nunca expira por tiempo
        _used = true;

        GD.Print("[ADRENALINE] Oleada permanente iniciada (Nivel 3 - Jefe Final)");
        EventBus.Instance.EmitAdrenalineStarted();
    }

    public override void _ExitTree()
    {
        if (TriggerZone != null)
        {
            TriggerZone.BodyEntered -= OnBodyEntered;
            TriggerZone.BodyExited -= OnBodyExited;
        }
        EventBus.Instance.EnemyKilled -= OnEnemyKilled;
    }

    private enum AdrenalineState { Normal, Adrenaline }
}
