// res://combat/ProjectilePool.cs
using System.Collections.Generic;
using Godot;

public enum ProjectileType { HiloDeQueso, Albondiga, SalsaPicante }

/// <summary>
/// Object Pool de proyectiles. Pre-calienta instancias al inicio del nivel.
/// Es la ÚNICA forma de crear proyectiles en runtime — nunca usar new() ni Instantiate() directo.
/// </summary>
public partial class ProjectilePool : Node
{
    public static ProjectilePool Instance { get; private set; } = null!;

    [Export] public int PoolSizePerType { get; set; } = 20;

    [ExportGroup("Scenes")]
    [Export] public PackedScene? HiloDeQuesoScene { get; set; }
    [Export] public PackedScene? AlbondigaScene   { get; set; }
    [Export] public PackedScene? SalsaPicanteScene { get; set; }

    private readonly Dictionary<ProjectileType, Queue<BaseProjectile>> _pools = new();

    public override void _Ready()
    {
        Instance = this;
        
        if (HiloDeQuesoScene != null) PreWarm(ProjectileType.HiloDeQueso, HiloDeQuesoScene);
        if (AlbondigaScene != null)   PreWarm(ProjectileType.Albondiga, AlbondigaScene);
        if (SalsaPicanteScene != null) PreWarm(ProjectileType.SalsaPicante, SalsaPicanteScene);
    }

    private void PreWarm(ProjectileType type, PackedScene scene)
    {
        _pools[type] = new Queue<BaseProjectile>();
        for (int i = 0; i < PoolSizePerType; i++)
        {
            var p = CreateInstance(type, scene);
            _pools[type].Enqueue(p);
        }
    }

    private BaseProjectile CreateInstance(ProjectileType type, PackedScene scene)
    {
        var p = scene.Instantiate<BaseProjectile>();
        p.Pool = this;
        p.Type = type;
        AddChild(p);
        p.Disable();
        return p;
    }

    /// <summary>
    /// Obtiene un proyectil activo del pool.
    /// Si el pool está vacío, lo expande en 5 unidades.
    /// </summary>
    public BaseProjectile Get(ProjectileType type)
    {
        if (!_pools.ContainsKey(type))
        {
            GD.PushError($"[ProjectilePool] No hay pool registrado para {type}");
            return null!;
        }

        if (_pools[type].Count == 0)
        {
            Expand(type);
        }

        var p = _pools[type].Dequeue();
        p.Enable();
        return p;
    }

    public void Return(BaseProjectile projectile)
    {
        projectile.Disable();
        _pools[projectile.Type].Enqueue(projectile);
    }

    private void Expand(ProjectileType type)
    {
        GD.PushWarning($"[ProjectilePool] Pool vacío para {type}. Expandiendo en 5.");
        PackedScene? scene = type switch
        {
            ProjectileType.HiloDeQueso => HiloDeQuesoScene,
            ProjectileType.Albondiga => AlbondigaScene,
            ProjectileType.SalsaPicante => SalsaPicanteScene,
            _ => null
        };

        if (scene != null)
        {
            for (int i = 0; i < 5; i++)
            {
                var p = CreateInstance(type, scene);
                _pools[type].Enqueue(p);
            }
        }
    }
}
