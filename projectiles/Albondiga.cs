// res://projectiles/Albondiga.cs
using Godot;

/// <summary>
/// Proyectil rebotador lanzado por PastaDemon.
/// Rebota en paredes y plataformas, causando daño en área al impactar.
/// </summary>
public partial class Albondiga : BaseProjectile
{
    [Export] public float BounceElasticity { get; set; } = 0.8f;
    [Export] public int AreaDamage { get; set; } = 5;
    [Export] public float ExplosionRadius { get; set; } = 60f;

    private Vector2 _velocity = Vector2.Zero;
    private int _bounceCount = 0;
    private const int MAX_BOUNCES = 5;

    public override void Launch(Vector2 position, Vector2 direction)
    {
        base.Launch(position, direction);
        _velocity = Direction * Speed;
        _bounceCount = 0;
    }

    public override void _Process(double delta)
    {
        // Aplicar gravedad
        float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        _velocity.Y += gravity * (float)delta;

        // Mover proyectil
        GlobalPosition += _velocity * (float)delta;

        // Incrementar lifetime y devolver si expira
        CurrentLifetime += (float)delta;
        if (CurrentLifetime >= Lifetime)
        {
            ReturnToPool();
        }
    }

    protected override void OnBodyEntered(Node2D body)
    {
        if (body is Character) return;

        // Rebotar en plataformas
        if (_bounceCount < MAX_BOUNCES)
        {
            _velocity.Y *= -BounceElasticity;
            _velocity.X *= BounceElasticity;
            _bounceCount++;
        }
        else
        {
            // Si se agotaron rebotes, explotar en área
            ExplodeInArea();
            ReturnToPool();
        }
    }

    protected override void OnAreaEntered(Area2D area)
    {
        if (area is Hurtbox)
        {
            ExplodeInArea();
            ReturnToPool();
        }
    }

    private void ExplodeInArea()
    {
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
