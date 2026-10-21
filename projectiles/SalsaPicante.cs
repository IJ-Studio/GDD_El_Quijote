// res://projectiles/SalsaPicante.cs
using Godot;

/// <summary>
/// Proyectil de arco lanzado por TacoFuria.
/// Sigue una trayectoria parabólica y explota en área.
/// Requiere bloqueo para evitar daño.
/// </summary>
public partial class SalsaPicante : BaseProjectile
{
    [Export] public float GravityScale { get; set; } = 1.5f;
    [Export] public int AreaDamage { get; set; } = 8;
    [Export] public float ExplosionRadius { get; set; } = 80f;

    private Vector2 _velocity = Vector2.Zero;
    private bool _hasExploded = false;

    public override void Launch(Vector2 position, Vector2 direction)
    {
        base.Launch(position, direction);

        // Velocidad inicial en la dirección pedida (los enemigos apuntan hacia arriba para hacer el arco)
        _velocity = Direction * Speed;

        _hasExploded = false;
    }

    public override void _Process(double delta)
    {
        // Aplicar gravedad personalizada
        float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        _velocity.Y += gravity * GravityScale * (float)delta;

        // Mover proyectil
        GlobalPosition += _velocity * (float)delta;

        // Actualizar rotación según velocidad
        Rotation = _velocity.Angle();

        // Incrementar lifetime
        CurrentLifetime += (float)delta;
        if (CurrentLifetime >= Lifetime && !_hasExploded)
        {
            Explode();
            ReturnToPool();
        }
    }

    protected override void OnBodyEntered(Node2D body)
    {
        if (body is Character) return;

        // Explotar al tocar suelo/plataforma
        if (!_hasExploded)
        {
            Explode();
            ReturnToPool();
        }
    }

    protected override void OnAreaEntered(Area2D area)
    {
        if (area is Hurtbox)
        {
            Explode();
            ReturnToPool();
        }
    }

    private void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;
        EventBus.Instance.EmitProjectileBurst(GlobalPosition, (int)Type);

        // Buscar todas las Hurtbox en el área de explosión
        var space = GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D();
        query.Shape = new CircleShape2D { Radius = ExplosionRadius };
        query.Transform = GlobalTransform;
        query.CollisionMask = 8; // Capa "hurtbox_jugador"
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;

        var results = space.IntersectShape(query);
        foreach (var result in results)
        {
            if (result["collider"].Obj is Hurtbox hurtbox)
            {
                hurtbox.ReceiveDamage(Enemy.ScaleDamage(AreaDamage));
            }
        }
    }
}
