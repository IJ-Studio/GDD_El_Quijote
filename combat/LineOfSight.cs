// res://combat/LineOfSight.cs
using Godot;

/// <summary>
/// Comprueba si hay una estructura (suelo, pared o plataforma) entre dos puntos.
/// Los ataques de los enemigos lo usan para no atravesar el escenario.
/// Un rayo trata las plataformas finas como sólidas en cualquier dirección.
/// </summary>
public static class LineOfSight
{
    /// <summary>Capa física 1 = "mundo" (suelo, paredes y plataformas).</summary>
    private const uint WorldMask = 1;

    /// <summary>True si no hay ninguna estructura entre from y to.</summary>
    public static bool IsClear(Node2D context, Vector2 from, Vector2 to)
    {
        if (!context.IsInsideTree()) return true;
        var query = PhysicsRayQueryParameters2D.Create(from, to, WorldMask);
        return context.GetWorld2D().DirectSpaceState.IntersectRay(query).Count == 0;
    }
}
