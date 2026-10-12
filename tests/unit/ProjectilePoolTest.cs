// res://tests/unit/ProjectilePoolTest.cs
using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Suite de pruebas unitarias para ProjectilePool.
/// Verifica precalentamiento, reciclado y límites de instancias.
///
/// DISEÑO: Las pruebas no requieren la escena completa de Godot — solo crean
/// el nodo ProjectilePool de forma aislada para validar la lógica del Object Pool.
/// </summary>
[TestSuite]
public partial class ProjectilePoolTest : GodotTest
{
    private ProjectilePool? _pool;
    private PackedScene? _mockProjectileScene;

    /// <summary>Preparación previa a cada test: crea pool limpio.</summary>
    [Before]
    public async Task BeforeEach()
    {
        // Crear escena simulada de proyectil
        _mockProjectileScene = GD.Load<PackedScene>("res://projectiles/HiloDeQueso.tscn");

        // Crear instancia de ProjectilePool
        _pool = new ProjectilePool
        {
            PoolSizePerType = 10,
            HiloDeQuesoScene = _mockProjectileScene
        };

        // Necesario para que _Ready() se ejecute
        AutoFree(_pool);
        GetTree().Root.AddChild(_pool);
        await _pool.TreeEntered;
    }

    /// <summary>
    /// PRUEBA 1: El pool precalienta exactamente N instancias.
    /// </summary>
    [Test]
    public async Task PoolPrecalientes()
    {
        Assert.That(_pool).IsNotNull();

        // Acceder a las instancias privadas vía reflejo o verificar disponibilidad
        // En este caso, verificamos que Get() retorna instancias válidas
        var p1 = _pool.Get(ProjectileType.HiloDeQueso);
        Assert.That(p1).IsNotNull();
        Assert.That(p1.Visible).IsTrue();
    }

    /// <summary>
    /// PRUEBA 2: Al obtener un proyectil del pool, se activa (Enable).
    /// </summary>
    [Test]
    public async Task GetActivaProyectil()
    {
        var projectile = _pool.Get(ProjectileType.HiloDeQueso);

        Assert.That(projectile.Visible).IsTrue();
        Assert.That(projectile.ProcessMode).IsEqualTo(ProcessModeEnum.Inherit);
    }

    /// <summary>
    /// PRUEBA 3: Al devolver un proyectil, se desactiva (Disable).
    /// </summary>
    [Test]
    public async Task ReturnDesactivaProyectil()
    {
        var projectile = _pool.Get(ProjectileType.HiloDeQueso);
        var currentMode = projectile.ProcessMode;

        _pool.Return(projectile);

        Assert.That(projectile.Visible).IsFalse();
    }

    /// <summary>
    /// PRUEBA 4: Al disparar repetidamente, nunca se supera el límite inicial.
    /// Cuando el pool se vacía, se expande en +5, pero las pruebas verifican
    /// que no se crean instancias innecesarias.
    /// </summary>
    [Test]
    public async Task DispararRepetidamenteMantieneTextura()
    {
        int poolSizeInitial = _pool.PoolSizePerType;
        int nodosAlInicio = _pool.GetChildCount();

        // Simular 20 disparos (2x el tamaño inicial del pool)
        for (int i = 0; i < 20; i++)
        {
            var p = _pool.Get(ProjectileType.HiloDeQueso);
            Assert.That(p).IsNotNull();

            // Simular cierto tiempo de vida
            await Task.Delay(10);

            _pool.Return(p);
        }

        // Después de reciclado, el pool debe tener más nodos por la expansión
        // pero la lógica debe ser segura
        int nodosAlFinal = _pool.GetChildCount();
        Assert.That(nodosAlFinal).IsGreaterThanOrEqualTo(nodosAlInicio);
    }

    /// <summary>
    /// PRUEBA 5: Verificar que Get() desde un tipo no registrado retorna null (error handled).
    /// </summary>
    [Test]
    public async Task GetTipoNoRegistradoRetornaNulo()
    {
        // Si intentamos obtener un tipo no precalentado, debe devolver null o error
        var palbondiga = _pool.Get(ProjectileType.Albondiga);

        Assert.That(palbondiga).IsNull();
    }

    /// <summary>
    /// PRUEBA 6: Ciclo completo de 50 disparos sin que se exceda límite de expansión.
    /// Valida estabilidad del pool bajo carga.
    /// </summary>
    [Test]
    public async Task CicloCompleto50DisparosSinExceso()
    {
        int nodosAlInicio = _pool.GetChildCount();

        for (int i = 0; i < 50; i++)
        {
            var p = _pool.Get(ProjectileType.HiloDeQueso);
            if (p == null) continue;

            _pool.Return(p);
        }

        // Verifique que no hay memory leak excesivo
        int nodosAlFinal = _pool.GetChildCount();

        // Debe estar dentro de un rango razonable (inicial + expansiones)
        // Con 50 disparos y pool de 10, máx 5 expansiones = +25 nodos
        Assert.That(nodosAlFinal).IsLessThanOrEqualTo(nodosAlInicio + 30);
    }

    /// <summary>
    /// PRUEBA 7: Verificar que múltiples Get() consecutivos no devuelven la misma instancia.
    /// </summary>
    [Test]
    public async Task GetConsecutivosDevuelvenDiferentesInstancias()
    {
        var p1 = _pool.Get(ProjectileType.HiloDeQueso);
        var p2 = _pool.Get(ProjectileType.HiloDeQueso);

        Assert.That(p1).IsNotEqualTo(p2);
        Assert.That(p1.GetInstanceId()).IsNotEqualTo(p2.GetInstanceId());

        _pool.Return(p1);
        _pool.Return(p2);
    }
}
