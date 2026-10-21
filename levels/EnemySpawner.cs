// res://levels/EnemySpawner.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Generador de hordas de un nivel. Al empezar crea una horda inicial y después añade enemigos
/// de 2 en 2 cada cierto tiempo (mientras no se supere MaxAlive) hasta agotar TotalEnemies:
/// la cantidad de enemigos que aparecen en el nivel es contada, no infinita.
///
/// - Los tipos salen de EnemyScenes y nunca se repite el mismo tipo dos veces seguidas,
///   así cada horda (y cada pareja) es variada.
/// - Aparecen en los Marker2D hijos de este nodo, prefiriendo los más lejanos a los jugadores.
/// - Se pausa durante una Oleada de Adrenalina para no alterar al grupo disparador del cofre.
/// </summary>
public partial class EnemySpawner : Node2D
{
    private const string PlayersGroup = "players";

    /// <summary>Escenas de enemigo que pueden aparecer (cuantos más tipos, más variada la horda).</summary>
    [Export] public Godot.Collections.Array<PackedScene> EnemyScenes { get; set; } = new();

    /// <summary>Enemigos que aparecen al empezar el nivel (siempre, sin tope).</summary>
    [Export] public int InitialHorde { get; set; } = 3;

    /// <summary>Total de enemigos que aparecerán en el nivel (horda inicial incluida). Al llegar a este número ya no salen más.</summary>
    [Export] public int TotalEnemies { get; set; } = 8;

    /// <summary>Cuántos enemigos aparecen en cada oleada periódica.</summary>
    [Export] public int WaveSize { get; set; } = 2;

    /// <summary>Segundos entre una oleada periódica y la siguiente.</summary>
    [Export] public float WaveInterval { get; set; } = 7f;

    /// <summary>Tope de enemigos vivos a la vez; si se alcanza, la oleada espera.</summary>
    [Export] public int MaxAlive { get; set; } = 8;

    /// <summary>Distancia mínima (px) a los jugadores al elegir dónde aparece cada enemigo.</summary>
    [Export] public float MinDistanceFromPlayers { get; set; } = 280f;

    /// <summary>Segundos que tarda el enemigo en hacerse visible al aparecer.</summary>
    [Export] public float FadeInTime { get; set; } = 0.35f;

    private readonly List<Marker2D> _points = new();
    private readonly RandomNumberGenerator _rng = new();
    private float _timer;
    private bool _rushActive;
    private int _lastIndex = -1;
    private int _spawned;

    /// <summary>Enemigos que aún faltan por aparecer en este nivel.</summary>
    public int Remaining => Mathf.Max(0, TotalEnemies - _spawned);

    public override void _Ready()
    {
        _rng.Randomize();
        foreach (Node child in GetChildren())
        {
            if (child is Marker2D marker) _points.Add(marker);
        }

        _timer = WaveInterval;
        EventBus.Instance.AdrenalineStarted += OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded   += OnAdrenalineEnded;

        // Esperar a que el nivel termine de montarse (jugadores registrados en su grupo)
        CallDeferred(MethodName.SpawnInitialHorde);
    }

    public override void _Process(double delta)
    {
        if (_rushActive || _points.Count == 0 || EnemyScenes.Count == 0 || Remaining <= 0) return;

        _timer -= (float)delta;
        if (_timer > 0f) return;

        _timer = WaveInterval;
        int room = MaxAlive - CountAlive();
        if (room > 0) SpawnEnemies(Mathf.Min(WaveSize, room));
    }

    private void SpawnInitialHorde() => SpawnEnemies(InitialHorde);

    private void SpawnEnemies(int count)
    {
        if (_points.Count == 0 || EnemyScenes.Count == 0) return;
        count = Mathf.Min(count, Remaining);

        for (int i = 0; i < count; i++)
        {
            PackedScene? scene = PickScene();
            if (scene is null) return;

            Node instance = scene.Instantiate();
            if (instance is not Enemy enemy)
            {
                GD.PushWarning($"[EnemySpawner] La escena {scene.ResourcePath} no es un Enemy; se ignora.");
                instance.Free();
                continue;
            }

            // La posición se fija ANTES de añadirlo: Enemy._Ready guarda su origen de patrulla.
            Node2D parent = GetParent<Node2D>();
            enemy.Position = parent.ToLocal(PickSpawnPoint());
            enemy.Modulate = new Color(1f, 1f, 1f, 0f);
            parent.AddChild(enemy);
            _spawned++;
            enemy.CreateTween().TweenProperty(enemy, "modulate:a", 1f, FadeInTime);
        }
    }

    /// <summary>Elige un tipo al azar, distinto del anterior cuando hay más de uno.</summary>
    private PackedScene? PickScene()
    {
        int count = EnemyScenes.Count;
        if (count == 0) return null;

        int index = _rng.RandiRange(0, count - 1);
        if (count > 1 && index == _lastIndex)
            index = (index + 1 + _rng.RandiRange(0, count - 2)) % count;

        _lastIndex = index;
        return EnemyScenes[index];
    }

    /// <summary>Punto de aparición lejos de los jugadores vivos (o el más lejano si todos están cerca).</summary>
    private Vector2 PickSpawnPoint()
    {
        var players = new List<Character>();
        foreach (Node node in GetTree().GetNodesInGroup(PlayersGroup))
        {
            if (node is Character c && c.CurrentHealth > 0 && !c.IsQueuedForDeletion())
                players.Add(c);
        }

        var candidates = new List<Marker2D>();
        Marker2D farthest = _points[0];
        float farthestDistance = -1f;

        foreach (Marker2D point in _points)
        {
            float nearest = float.MaxValue;
            foreach (Character player in players)
                nearest = Mathf.Min(nearest, point.GlobalPosition.DistanceTo(player.GlobalPosition));

            if (nearest >= MinDistanceFromPlayers) candidates.Add(point);
            if (nearest > farthestDistance)
            {
                farthestDistance = nearest;
                farthest = point;
            }
        }

        Marker2D chosen = candidates.Count > 0
            ? candidates[_rng.RandiRange(0, candidates.Count - 1)]
            : farthest;
        return chosen.GlobalPosition;
    }

    private int CountAlive()
    {
        int alive = 0;
        foreach (Node node in GetTree().GetNodesInGroup(Enemy.GroupName))
        {
            if (node is Enemy e && e.CurrentHealth > 0 && !e.IsQueuedForDeletion()) alive++;
        }
        return alive;
    }

    private void OnAdrenalineStarted() => _rushActive = true;

    private void OnAdrenalineEnded()
    {
        _rushActive = false;
        _timer = WaveInterval;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnded;
    }
}
