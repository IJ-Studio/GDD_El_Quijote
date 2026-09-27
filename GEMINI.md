# Instructions & Guidelines — *El Quijote: La Búsqueda de la Sazón Perdida*

> **Propósito:** Guía de referencia técnica y directivas de comportamiento para Gemini CLI.
> Cubre reglas de generación de código, arquitectura, patrones, estándares C#, pruebas,
> CI/CD, flujo Git y todo lo que el asistente debe saber antes de escribir, modificar
> o revisar cualquier línea de código de este proyecto.
>
> **Motor:** Godot 4.7.2 · **Lenguaje:** C# 14 / .NET 10 · **Equipo:** 2 personas (bina)
> **Renderizador:** Compatibilidad (OpenGL 3 — 2D Side-Scroller)
> **Asignatura:** Optativa 1: Creación de Videojuegos

---

## ⚠️ REGLAS ESTRICTAS PARA GEMINI CLI

> Estas reglas tienen **prioridad absoluta** sobre cualquier otra consideración.
> Aplicarlas en cada respuesta, sin excepción.

### R1 — Lenguaje exclusivo: C# moderno

Todo código generado debe ser estrictamente **C# 14 para Godot 4.7.2 (.NET 10)**.

```
❌ PROHIBIDO: GDScript, Mono legacy (Godot 3), sintaxis de C# < 10
✅ REQUERIDO: C# 14, nullable reference types, pattern matching, target-typed new(), records donde apliquen
```

Solo se puede generar GDScript si el usuario lo pide **explícitamente** en su mensaje.

### R2 — Código completo, sin stubs

```
❌ PROHIBIDO: // TODO: implementar aquí...
❌ PROHIBIDO: // ... resto del código
❌ PROHIBIDO: // implementación omitida por brevedad
✅ REQUERIDO: código compilable y listo para usar, incluyendo imports y using statements
```

### R3 — Sin boilerplate innecesario

El código debe ser limpio, fuertemente tipado y modular. No generar código que compile
con warnings. Ejecutar mentalmente `-warnaserror` antes de entregar cualquier fragmento.

### R4 — Desacoplamiento obligatorio

Separar siempre la lógica de negocio (calculadoras de daño, FSM, timers) de los nodos
gráficos de Godot para que las pruebas unitarias con gdUnit4 puedan correr **headless**,
sin instanciar el árbol de escenas.

### R5 — Respetar la arquitectura por capas

Antes de generar cualquier clase, verificar en qué capa vive y qué capas puede conocer
(ver Sección 4). Nunca generar código que viole las reglas de dependencia entre capas.

### R6 — Rutas explícitas siempre

Indicar en cada respuesta la ruta exacta donde debe guardarse el archivo:

```
// res://entities/enemies/SliceOPizza.cs
// res://states/player_states/AttackState.cs
// res://tests/unit/StateMachineTest.cs
```

### R7 — Prueba unitaria incluida cuando aplique

Si la solución contiene lógica no trivial (FSM, cálculo de daño, timer, pool), incluir
automáticamente el script de test gdUnit4 correspondiente en `res://tests/unit/`.

---

## 📋 FORMATO DE RESPUESTAS DE GEMINI CLI

Toda respuesta que genere código debe seguir este formato en orden:

```
1. [Decisión técnica]  Una línea explicando el patrón o decisión arquitectónica aplicada.
2. [Ruta del archivo]  res://ruta/exacta/Clase.cs
3. [Código C# completo]  Compilable, con using statements, sin omisiones.
4. [Registro en Godot]  Si aplica: cómo registrar en project.godot, Inspector o .csproj.
5. [Test gdUnit4]      Si la lógica es no trivial, el script completo en res://tests/unit/.
```

**Ejemplo de respuesta bien formateada:**

```
Decisión: Se implementa como IState para mantener el comportamiento encapsulado
en la FSM genérica y evitar flags booleanos en Character.

// res://states/player_states/CrouchState.cs
using Godot;
public class CrouchState : IState { ... }

// res://tests/unit/CrouchStateTest.cs
[TestSuite]
public class CrouchStateTest { ... }
```

---

## Tabla de contenido

1. [Contexto del proyecto](#1-contexto-del-proyecto)
2. [Stack tecnológico](#2-stack-tecnológico)
3. [Estructura de carpetas canónica](#3-estructura-de-carpetas-canónica)
4. [Arquitectura general](#4-arquitectura-general)
5. [Jerarquía de clases y herencia](#5-jerarquía-de-clases-y-herencia)
6. [Patrones de diseño aplicados](#6-patrones-de-diseño-aplicados)
7. [Máquina de estados (State Machine)](#7-máquina-de-estados-state-machine)
8. [Estado global y Autoloads](#8-estado-global-y-autoloads)
9. [Sistema de combate](#9-sistema-de-combate)
10. [Mecánica Oleada de Adrenalina](#10-mecánica-oleada-de-adrenalina)
11. [Sistema de proyectiles (Object Pool)](#11-sistema-de-proyectiles-object-pool)
12. [HUD y patrón Observer](#12-hud-y-patrón-observer)
13. [Estándares de código C#](#13-estándares-de-código-c)
14. [Convenciones de Godot 4 con C#](#14-convenciones-de-godot-4-con-c)
15. [Estrategia de pruebas (TDD + gdUnit4)](#15-estrategia-de-pruebas-tdd--gdunit4)
16. [CI/CD con GitHub Actions](#16-cicd-con-github-actions)
17. [Flujo Git (GitHub Flow)](#17-flujo-git-github-flow)
18. [Gestión del proyecto: GitHub Projects + PowerShell](#18-gestión-del-proyecto-github-projects--powershell)
19. [Plan de sprints y Definición de Terminado](#19-plan-de-sprints-y-definición-de-terminado)
20. [Riesgos técnicos y mitigaciones](#20-riesgos-técnicos-y-mitigaciones)
21. [Buenas prácticas de vibecoding con IA](#21-buenas-prácticas-de-vibecoding-con-ia)
22. [Anti-patrones y qué NO hacer](#22-anti-patrones-y-qué-no-hacer)
23. [Checklist de entrega por sprint](#23-checklist-de-entrega-por-sprint)

---

## 1. Contexto del proyecto

| Campo | Valor |
|---|---|
| **Nombre** | *El Quijote: La Búsqueda de la Sazón Perdida* |
| **Género** | Beat 'em up / Plataformas 2D |
| **Equipo** | Irving Aldahir Angeles Romero (`irvingaldahirangelesromero`) + José Alfredo Hernández Arellano |
| **Docente** | Juvencio Mendoza Castelán |
| **Org. GitHub** | IJ-Studio |
| **Periodo** | 28 sep 2026 – ~dic 2026 (Sprints 0–5, ~12 semanas) |
| **Niveles** | 3 (Almacén → Bóveda de la Cura → Cocina) |
| **Jugadores** | 1–2 locales (Don Quijote + Sancho Panza) |
| **Enemigos** | Slice-O-Pizza, Burger-Bashing, Pasta-Demon, Taco-Furia + Jefe: Taco Mayor |

### Controles del juego

| Input | Acción |
|---|---|
| `A` / `D` | Movimiento lateral |
| `W` | Saltar |
| `S` | Agacharse |
| `J` (o clic izquierdo) | Ataque cuerpo a cuerpo (estocada con Lanza / Maza) |
| `K` (o clic derecho) | Bloqueo con escudo |
| `E` | Interacción con elementos del entorno |
| `Esc` / `P` | Pausa |

Estos controles se configuran en **InputMap** de Godot (`project.godot`), con perfiles
separados `Player1_*` y `Player2_*` para el modo de 2 jugadores.

### Objetivos pedagógicos que DEBEN reflejarse en el código

El GDD exige demostrar explícitamente:

- **Herencia y polimorfismo** — clase base `Character` con subclases reales.
- **Patrón State** — FSM genérica para personajes y enemigos.
- **Patrón Observer** — señales de Godot como mecanismo de desacoplamiento.
- **Singleton (Autoload)** — estado persistente entre escenas.
- **Object Pool** — proyectiles en tiempo de alto rendimiento.

Nunca omitas estos patrones por "simplicidad". Son criterios de evaluación.

---

## 2. Stack tecnológico

| Herramienta | Versión exacta | Propósito |
|---|---|---|
| **Godot** | **4.7.2** (soporte .NET) | Motor de juego |
| **C#** | **C# 14** | Lenguaje de scripting |
| **.NET** | **.NET 10** | Runtime |
| **Renderizador** | **Compatibilidad (OpenGL 3)** | Render pipeline — 2D Side-Scroller |
| **gdUnit4** | última versión compatible con Godot 4.7.x + C# | Framework de pruebas unitarias |
| **GitHub Actions** | — | CI (build + test headless) |
| **GitHub CLI** | ≥ 2.94 | Importador de backlog (`importar_videojuego_el_quijote.ps1`) |
| **GitHub Projects v2** | — | Tablero Scrumban (Kanban + Sprints) |
| **Git** | ≥ 2.40 | Control de versiones |
| **PowerShell** | ≥ 7 | Script de importación del backlog |

> **⚠️ Nota de versión para Gemini:** Godot 4.7.2 usa el binding C# moderno (no Mono).
> Toda la sintaxis debe ser compatible con C# 14 / .NET 10. No generar código con
> patrones de Godot 3 / Mono (ej. `OS.GetTicksMsec()` vs `Time.GetTicksMsec()`).

### `.csproj` — referencias mínimas

```xml
<!-- ElQuijote.csproj -->
<Project Sdk="Godot.NET.Sdk/4.7.2">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="GdUnit4" Version="*"
                      Condition="'$(Configuration)'=='Debug'" />
  </ItemGroup>
</Project>
```

---

## 3. Estructura de carpetas canónica

Toda generación de código DEBE respetar esta estructura. No crear archivos en rutas
fuera de ella sin justificación explícita.

```
res://   (ElQuijote-LaBusquedaDeLaSazonPerdida/)
│
├── project.godot                        # Configuración del motor
├── ElQuijote.csproj                     # Proyecto C# (.NET 10 / C# 14)
├── .github/
│   └── workflows/
│       ├── ci.yml                       # Build + test en cada push/PR
│       └── export.yml                   # Export automático al cerrar sprint
│
├── autoload/                            # ← Singletons: NUNCA instanciar manualmente
│   ├── GameManager.cs                   # Vidas, llave, Cura, nivel, estado de partida
│   ├── AudioManager.cs                  # Música/SFX; reacciona a EventBus
│   └── EventBus.cs                      # Canal de señales globales (Observer hub)
│
├── entities/
│   ├── Character.cs                     # Clase base ABSTRACTA — NO instanciar directamente
│   ├── players/
│   │   ├── DonQuijote.cs / .tscn
│   │   └── SanchoPanza.cs / .tscn
│   ├── enemies/
│   │   ├── Enemy.cs                     # Base abstracta de enemigos
│   │   ├── SliceOPizza.cs / .tscn
│   │   ├── BurgerBashing.cs / .tscn
│   │   ├── PastaDemon.cs  / .tscn
│   │   └── TacoFuria.cs  / .tscn
│   └── bosses/
│       ├── TacoMayor.cs
│       └── TacoMayor.tscn
│
├── states/                              # Patrón State — FSM desacoplada
│   ├── IState.cs                        # Interfaz que todo estado implementa
│   ├── StateMachine.cs                  # Motor genérico de la FSM
│   ├── player_states/
│   │   ├── IdleState.cs
│   │   ├── RunState.cs
│   │   ├── JumpState.cs
│   │   ├── CrouchState.cs
│   │   ├── AttackState.cs
│   │   ├── BlockState.cs
│   │   ├── HurtState.cs
│   │   └── DeadState.cs
│   └── enemy_states/
│       ├── GuardState.cs
│       ├── AlertState.cs                # Se activa con la Oleada de Adrenalina
│       ├── EnemyAttackState.cs
│       └── EnemyDeadState.cs
│
├── combat/
│   ├── Hitbox.cs                        # Área que INFLIGE daño
│   ├── Hurtbox.cs                       # Área que RECIBE daño
│   └── ProjectilePool.cs                # Object Pool — NO instanciar proyectiles directamente
│
├── projectiles/
│   ├── BaseProjectile.cs
│   ├── HiloDeQueso.cs / .tscn
│   ├── Albondiga.cs   / .tscn
│   └── SalsaPicante.cs / .tscn
│
├── levels/
│   ├── Level1Almacen.tscn / .cs
│   ├── Level2BovedaDeLaCura.tscn / .cs
│   └── Level3Cocina.tscn / .cs
│
├── ui/
│   ├── Hud.cs                           # Solo escucha señales — no lee estado directamente
│   ├── MainMenu.cs
│   ├── PauseMenu.cs
│   ├── DialogueBox.cs
│   └── VictoryScreen.cs
│
├── resources/                           # .tres / .res — datos serializados
│   └── CharacterStats.tres
│
├── assets/
│   ├── sprites/
│   ├── audio/
│   │   ├── music/
│   │   └── sfx/
│   └── fonts/
│
└── tests/                               # gdUnit4 — espejo de entities/states/combat
    ├── unit/
    │   ├── StateMachineTest.cs
    │   ├── CharacterTest.cs
    │   ├── DamageCalculationTest.cs
    │   ├── AdrenalineTimerTest.cs
    │   └── ProjectilePoolTest.cs
    └── integration/
        └── README.md                    # Validación manual por sprint (documentada)
```

---

## 4. Arquitectura general

### Decisión central: Capas sobre el árbol de nodos de Godot

```
┌─────────────────────────────────────────────────────────┐
│  CAPA UI          Hud, Menus, DialogueBox               │
│                   Solo escuchan señales. Sin lógica.     │
├─────────────────────────────────────────────────────────┤
│  CAPA AUTOLOAD    GameManager │ AudioManager │ EventBus  │
│                   Estado global. Vive entre escenas.     │
├─────────────────────────────────────────────────────────┤
│  CAPA ENTIDADES   Character → jugables / enemigos / jefe │
│                   Lógica de personaje + delegación a FSM │
├─────────────────────────────────────────────────────────┤
│  CAPA ESTADOS     StateMachine + IState implementations  │
│                   Comportamiento puro, sin nodos de UI   │
├─────────────────────────────────────────────────────────┤
│  CAPA COMBAT      Hitbox / Hurtbox / ProjectilePool      │
│                   Física y detección de colisiones       │
├─────────────────────────────────────────────────────────┤
│  CAPA NIVEL       LevelX.cs — orquesta la escena         │
│                   Spawnea enemigos, gestiona checkpoint   │
└─────────────────────────────────────────────────────────┘
```

### Reglas de dependencia entre capas

- La UI **nunca** llama métodos de `Character` directamente → solo lee señales de `EventBus`/`GameManager`.
- Los estados (`IState`) **no** referencian nodos de UI.
- `GameManager` **no** conoce la escena concreta que está cargada; la escena le notifica vía señales.
- Los niveles **no** contienen lógica de combate; solo instancian y configuran entidades.
- `ProjectilePool` es la única ruta para crear proyectiles en runtime.

---

## 5. Jerarquía de clases y herencia

### Clase base `Character` (abstracta)

```csharp
// res://entities/Character.cs
using Godot;

/// <summary>
/// Clase base abstracta para TODOS los personajes del juego.
/// NO instanciar directamente. Usar subclases concretas.
/// </summary>
public abstract partial class Character : CharacterBody2D
{
    // ── Exportados al Inspector de Godot ──────────────────────────
    [Export] public float Speed     { get; protected set; } = 150f;
    [Export] public int   MaxHealth { get; protected set; } = 100;

    // ── Estado en tiempo de ejecución ─────────────────────────────
    public int CurrentHealth { get; protected set; }

    // ── Dependencias internas ──────────────────────────────────────
    protected StateMachine  StateMachine = null!;
    protected AnimatedSprite2D Sprite    = null!;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        StateMachine  = GetNode<StateMachine>("StateMachine");
        Sprite        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        InitializeStates();
        StateMachine.Start();
    }

    /// <summary>Cada subclase registra sus propios estados en la FSM.</summary>
    protected abstract void InitializeStates();

    public virtual void Move(Vector2 direction)
    {
        Velocity = direction * Speed;
        MoveAndSlide();
    }

    /// <summary>
    /// Recibe daño. Subclases pueden sobreescribir para resistencias o efectos.
    /// Emite HealthChanged vía EventBus. Llama a Die() si CurrentHealth llega a 0.
    /// </summary>
    /// <param name="amount">Daño a aplicar (valor positivo).</param>
    public virtual void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        EventBus.Instance.EmitHealthChanged(this, CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
            Die();
        else
            StateMachine.TransitionTo("Hurt");
    }

    protected virtual void Die()
    {
        StateMachine.TransitionTo("Dead");
    }
}
```

### Tabla de herencia

| Clase | Hereda de | Propósito |
|---|---|---|
| `DonQuijote` | `Character` | Jugador 1. Lanza/Ropera + Tapa de Olla |
| `SanchoPanza` | `Character` | Jugador 2. Maza/Hacha + Tabla de Madera |
| `Enemy` | `Character` | Base abstracta de enemigos. Agrega `GuardZone` y respuesta a Oleada |
| `SliceOPizza` | `Enemy` | Proyectil: hilo de queso a distancia |
| `BurgerBashing` | `Enemy` | Embestida frontal cuerpo a cuerpo |
| `PastaDemon` | `Enemy` | Albóndiga rebotadora en área |
| `TacoFuria` | `Enemy` | Salsa en arco |
| `TacoMayor` | `Enemy` | Jefe. Embestida + proyectiles + ráfaga combinadas |

### Clase base `Enemy`

```csharp
// res://entities/enemies/Enemy.cs
using Godot;

/// <summary>
/// Base abstracta para todos los enemigos. Suscribe y desuscribe automáticamente
/// los eventos de la Oleada de Adrenalina.
/// </summary>
public abstract partial class Enemy : Character
{
    [Export] public Area2D? GuardZone { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        EventBus.Instance.AdrenalineStarted += OnAdrenalineRush;
        EventBus.Instance.AdrenalineEnded   += OnAdrenalineEnd;
    }

    /// <summary>
    /// Respuesta base ante la Oleada de Adrenalina: duplica velocidad y
    /// transiciona a Alert. Las subclases pueden sobreescribir.
    /// </summary>
    protected virtual void OnAdrenalineRush()
    {
        Speed *= 2f;
        Sprite.SpeedScale *= 2f;
        StateMachine.TransitionTo("Alert");
    }

    protected virtual void OnAdrenalineEnd()
    {
        Speed /= 2f;
        Sprite.SpeedScale /= 2f;
        StateMachine.TransitionTo("Guard");
    }

    public override void _ExitTree()
    {
        // Desuscribirse siempre para evitar memory leaks y callbacks en instancias muertas
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineRush;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnd;
    }
}
```

---

## 6. Patrones de diseño aplicados

### 6.1 Herencia + Polimorfismo (`patron-herencia`)

**Dónde:** `Character` → jugables / `Enemy` → tipos de enemigo / `TacoMayor`

**Regla de Gemini:** toda nueva entidad hereda de la clase correcta en la jerarquía.
Nunca duplicar código de movimiento, colisión o daño fuera de `Character`.

### 6.2 State — Máquina de estados (`patron-state`)

**Dónde:** `StateMachine.cs` + `IState.cs` + `states/player_states/` + `states/enemy_states/`

La lógica de animación y movimiento **NO** usa árboles de `if/else` sobre flags booleanos.
Cada estado es una clase separada. Los estados no se comunican entre sí directamente; solo
a través de `StateMachine.TransitionTo(string)`.

### 6.3 Observer — Señales de Godot (`patron-observer`)

**Dónde:** `autoload/EventBus.cs` como hub central.

**Regla:** si la señal cruza el límite de escena o va de una entidad a la UI, **siempre**
pasa por `EventBus`. El HUD nunca consulta estado mediante polling en `_Process`.

```csharp
// res://autoload/EventBus.cs
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
```

### 6.4 Singleton / Autoload (`patron-singleton`)

**Dónde:** `GameManager`, `AudioManager`, `EventBus`

**Registro en `project.godot`:**

```ini
[autoload]
EventBus="*res://autoload/EventBus.cs"
GameManager="*res://autoload/GameManager.cs"
AudioManager="*res://autoload/AudioManager.cs"
```

**Acceso correcto:**

```csharp
// ✅ Propiedad estática tipada
GameManager.Instance.Lives
EventBus.Instance.EmitKeyCollected()

// ❌ GetNode para acceder a Autoloads — frágil y no testeable
GetNode<GameManager>("/root/GameManager")
```

### 6.5 Object Pool (`patron-object-pool`)

**Dónde:** `combat/ProjectilePool.cs`

Durante la Oleada de Adrenalina hay picos de proyectiles simultáneos a ×2 velocidad.
Instanciar/destruir genera GC pressure y caídas de FPS justo en el momento más exigente.
Ver Sección 11 para la implementación completa.

### 6.6 Factory Method (dentro del Pool)

**Dónde:** `ProjectilePool.Get(ProjectileType type)`

Cada enemigo llama `ProjectilePool.Instance.Get(ProjectileType.HiloDeQueso)` sin conocer
la ruta de la escena. El pool resuelve qué `PackedScene` cargar.

---

## 7. Máquina de estados (State Machine)

### `IState.cs`

```csharp
// res://states/IState.cs

/// <summary>
/// Contrato que todo estado de la FSM debe implementar.
/// Los estados son clases C# puras — no heredan de Node.
/// </summary>
public interface IState
{
    /// <summary>Se llama una vez al entrar en este estado.</summary>
    void Enter();

    /// <summary>Equivalente a _Process. delta en segundos.</summary>
    void Update(double delta);

    /// <summary>Equivalente a _PhysicsProcess.</summary>
    void PhysicsUpdate(double delta);

    /// <summary>Se llama una vez al salir del estado.</summary>
    void Exit();
}
```

### `StateMachine.cs`

```csharp
// res://states/StateMachine.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Motor genérico de la FSM. Se registra como hijo del nodo Character en la escena.
/// Los estados se registran por código en Character._Ready() → InitializeStates().
/// </summary>
public partial class StateMachine : Node
{
    private IState?                     _current;
    private Dictionary<string, IState>  _states = new();

    [Export] public string InitialState { get; set; } = "Idle";

    public void RegisterState(string name, IState state) => _states[name] = state;

    public void Start()
    {
        if (!_states.TryGetValue(InitialState, out var initial)) return;
        _current = initial;
        _current.Enter();
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    public void TransitionTo(string stateName)
    {
        if (!_states.TryGetValue(stateName, out var next)) return;
        if (next == _current) return;

        _current?.Exit();
        _current = next;
        _current.Enter();
    }

    public override void _Process(double delta)          => _current?.Update(delta);
    public override void _PhysicsProcess(double delta)   => _current?.PhysicsUpdate(delta);

    public string CurrentStateName =>
        _states.TryGetValue(_current?.GetType().Name ?? "", out _)
            ? _states.First(kv => kv.Value == _current).Key
            : "Unknown";
}
```

### Ejemplo: `AttackState`

```csharp
// res://states/player_states/AttackState.cs
using Godot;

/// <summary>
/// Estado de ataque del jugador. Activa la Hitbox solo durante los frames de ataque
/// y transiciona automáticamente a Idle cuando termina la animación.
/// </summary>
public class AttackState : IState
{
    private const string ANIM_ATTACK = "attack";

    private readonly Character _owner;

    public AttackState(Character owner) => _owner = owner;

    public void Enter()
    {
        _owner.Sprite.Play(ANIM_ATTACK);
        _owner.Sprite.AnimationFinished += OnAnimationFinished;
        _owner.Hitbox.Monitoring = true;
    }

    public void Update(double delta) { }
    public void PhysicsUpdate(double delta) { }

    public void Exit()
    {
        _owner.Sprite.AnimationFinished -= OnAnimationFinished;
        _owner.Hitbox.Monitoring = false;
    }

    private void OnAnimationFinished()
    {
        if (_owner.Sprite.Animation == ANIM_ATTACK)
            _owner.StateMachine.TransitionTo("Idle");
    }
}
```

### Tabla de transiciones válidas — Personaje jugable

```
Idle      ──────────► Run / Jump / Crouch / Attack / Block
Run       ──────────► Idle / Jump / Attack
Jump      ──────────► Idle (aterrizar) / Attack (en el aire)
Crouch    ──────────► Idle (soltar S)
Attack    ──────────► Idle (fin de animación)
Block     ──────────► Idle (soltar K)
[Cualquier estado] ► Hurt (al recibir daño, si no está en HurtState)
Hurt      ──────────► Idle (fin de animación de daño)
[Cualquier estado] ► Dead (CurrentHealth == 0)
```

### Tabla de transiciones válidas — Enemigo

```
Guard     ──────────► Alert (Oleada de Adrenalina activa)
Guard     ──────────► EnemyAttack (jugador en rango de ataque)
Alert     ──────────► EnemyAttack (jugador en rango)
Alert     ──────────► Guard (Oleada termina)
EnemyAttack ────────► Guard (fin de ataque, jugador fuera de rango)
[Cualquier estado] ► EnemyDead (CurrentHealth == 0)
```

---

## 8. Estado global y Autoloads

### `GameManager.cs`

```csharp
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
```

### Flujo de estados del juego

```
[*] ──────────► MainMenu
MainMenu ──────► Playing      (StartGame)
Playing ───────► Paused       (Esc / P)
Paused ────────► Playing      (Resume)
Playing ───────► Dialogue     (trigger narrativo)
Dialogue ──────► Playing      (confirmar)
Playing ───────► Victory      (derrota Taco Mayor + cruza puerta)
Playing ───────► GameOver     (Lives == 0)
GameOver ──────► Playing      (reiniciar partida)
Victory ───────► [*]
```

---

## 9. Sistema de combate

### Hitbox / Hurtbox

```csharp
// res://combat/Hitbox.cs
using Godot;

/// <summary>
/// Área que INFLIGE daño. Solo se activa durante AttackState.Enter()
/// y se desactiva en AttackState.Exit(). Nunca permanece activa en reposo.
/// </summary>
public partial class Hitbox : Area2D
{
    [Export] public int Damage { get; set; } = 10;

    public override void _Ready()
    {
        Monitoring = false;   // Inactiva por defecto — la activa AttackState
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is Hurtbox hurtbox)
            hurtbox.ReceiveDamage(Damage);
    }
}

// res://combat/Hurtbox.cs
using Godot;

/// <summary>
/// Área que RECIBE daño. Delega a TakeDamage del Character dueño.
/// </summary>
public partial class Hurtbox : Area2D
{
    [Export] public NodePath OwnerPath { get; set; } = new NodePath("..");
    private Character? _owner;

    public override void _Ready() => _owner = GetNode<Character>(OwnerPath);

    public void ReceiveDamage(int amount) => _owner?.TakeDamage(amount);
}
```

### Reglas del sistema de combate

- `Hitbox.Monitoring` se activa **solo** en `AttackState.Enter()` y se desactiva en `AttackState.Exit()`.
- Los proyectiles tienen su propia `Hitbox`. Al impactar llaman `Pool.Return(this)`, no `QueueFree()`.
- `BlockState` sobrescribe `TakeDamage` para reducir el daño al 50%. No hacerlo en `Character` directamente.
- `HurtState` ignora llamadas a `TakeDamage` durante su duración fija (frames de invencibilidad).

---

## 10. Mecánica Oleada de Adrenalina

> **Mecánica estrella. Pair Programming obligatorio en Sprint 4.**

### Especificaciones

| Propiedad | Valor |
|---|---|
| **Disparador** | Acercarse a un contenedor de suministros / cofre |
| **Duración** | 15–30 s (configurable por nivel via `[Export]`) |
| **Efecto velocidad** | `Speed *= 2f` en cada entidad activa |
| **Efecto animaciones** | `Sprite.SpeedScale *= 2f` en cada entidad activa |
| **Efecto proyectiles** | Velocidad de proyectiles activos en pool × 2 |
| **Nivel 3** | Oleada activa **todo el nivel**. TacoMayor arranca en `AdrenalineState`. |
| **Fin (N1 y N2)** | Al eliminar el **último** enemigo del grupo disparador |

### Regla crítica de implementación

```csharp
// ❌ PROHIBIDO — altera UI, timers del sistema y menús
Engine.TimeScale = 2.0f;

// ✅ CORRECTO — escalar propiedad por propiedad en cada entidad
foreach (var entity in _managedEntities)
{
    entity.Speed             *= 2f;
    entity.Sprite.SpeedScale *= 2f;
}
```

### `AdrenalineController.cs`

```csharp
// res://autoload/AdrenalineController.cs   (o como nodo hijo del nivel)
using System.Collections.Generic;
using Godot;

/// <summary>
/// Gestiona el ciclo Normal → Adrenaline → Normal.
/// PermanentMode = true en Nivel 3 (nunca desactiva automáticamente).
/// </summary>
public partial class AdrenalineController : Node
{
    [Export] public float Duration      { get; set; } = 20f;
    [Export] public bool  PermanentMode { get; set; } = false;

    private bool  _active   = false;
    private float _timeLeft = 0f;
    private int   _enemiesAlive = 0;
    private readonly List<Character> _managedEntities = new();

    public void Activate(List<Character> entities)
    {
        if (_active) return;
        _active    = true;
        _timeLeft  = Duration;
        _enemiesAlive = entities.Count;
        _managedEntities.AddRange(entities);

        foreach (var e in _managedEntities) ApplyEffect(e);
        EventBus.Instance.EmitAdrenalineStarted();
        EventBus.Instance.EnemyKilled += OnEnemyKilled;
    }

    public override void _Process(double delta)
    {
        if (!_active || PermanentMode) return;
        _timeLeft -= (float)delta;
        if (_timeLeft <= 0f) Deactivate();
    }

    private void OnEnemyKilled(Enemy _)
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        if (_enemiesAlive <= 0) Deactivate();
    }

    private void Deactivate()
    {
        if (!_active) return;
        _active = false;
        EventBus.Instance.EnemyKilled -= OnEnemyKilled;

        foreach (var e in _managedEntities) RemoveEffect(e);
        _managedEntities.Clear();
        EventBus.Instance.EmitAdrenalineEnded();
        EventBus.Instance.EmitEnemyGroupDefeated();
    }

    private static void ApplyEffect(Character e)
    {
        e.Speed              *= 2f;
        e.Sprite.SpeedScale  *= 2f;
    }

    private static void RemoveEffect(Character e)
    {
        e.Speed              /= 2f;
        e.Sprite.SpeedScale  /= 2f;
    }
}
```

---

## 11. Sistema de proyectiles (Object Pool)

```csharp
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
    [Export] public PackedScene? HiloDeQuesoScene  { get; set; }
    [Export] public PackedScene? AlbondigaScene    { get; set; }
    [Export] public PackedScene? SalsaPicanteScene { get; set; }

    private Dictionary<ProjectileType, Queue<BaseProjectile>> _pools = new();

    public override void _Ready()
    {
        Instance = this;
        PreWarm(ProjectileType.HiloDeQueso,  HiloDeQuesoScene!);
        PreWarm(ProjectileType.Albondiga,     AlbondigaScene!);
        PreWarm(ProjectileType.SalsaPicante,  SalsaPicanteScene!);
    }

    private void PreWarm(ProjectileType type, PackedScene scene)
    {
        _pools[type] = new Queue<BaseProjectile>();
        for (int i = 0; i < PoolSizePerType; i++)
        {
            var p = scene.Instantiate<BaseProjectile>();
            p.Pool = this;
            p.Type = type;
            AddChild(p);
            p.Disable();
            _pools[type].Enqueue(p);
        }
    }

    /// <summary>
    /// Obtiene un proyectil activo del pool.
    /// Si el pool está vacío, lo expande en 5 unidades (solo en casos excepcionales).
    /// </summary>
    public BaseProjectile Get(ProjectileType type)
    {
        if (_pools[type].Count == 0) Expand(type, 5);
        var p = _pools[type].Dequeue();
        p.Enable();
        return p;
    }

    public void Return(BaseProjectile projectile)
    {
        projectile.Disable();
        _pools[projectile.Type].Enqueue(projectile);
    }

    private void Expand(ProjectileType type, int count)
    {
        GD.PushWarning($"[ProjectilePool] Pool vacío para {type}. Expandiendo en {count}.");
        // Instanciar los `count` proyectiles adicionales del tipo correspondiente
    }
}
```

---

## 12. HUD y patrón Observer

```csharp
// res://ui/Hud.cs
using Godot;

/// <summary>
/// El HUD solo ESCUCHA señales del EventBus.
/// Nunca lee estado directamente de Character, GameManager o EventBus polling.
/// </summary>
public partial class Hud : CanvasLayer
{
    [Export] private Label?       _livesLabel;
    [Export] private TextureRect? _keyIcon;
    [Export] private TextureRect? _cureIcon;
    [Export] private ProgressBar? _healthBar;

    public override void _Ready()
    {
        EventBus.Instance.HealthChanged      += OnHealthChanged;
        EventBus.Instance.KeyCollected       += OnKeyCollected;
        EventBus.Instance.CureCollected      += OnCureCollected;
        EventBus.Instance.PlayerDied         += OnPlayerDied;
        EventBus.Instance.AdrenalineStarted  += OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded    += OnAdrenalineEnded;

        // Estado inicial
        if (_livesLabel is not null) _livesLabel.Text = $"Vidas: {GameManager.Instance.Lives}";
        if (_keyIcon   is not null) _keyIcon.Visible  = false;
        if (_cureIcon  is not null) _cureIcon.Visible  = false;
    }

    private void OnHealthChanged(Character c, int current, int max)
    {
        if (c is DonQuijote or SanchoPanza && _healthBar is not null)
            _healthBar.Value = (float)current / max * 100f;
    }

    private void OnKeyCollected()              => _keyIcon!.Visible  = true;
    private void OnCureCollected()             => _cureIcon!.Visible = true;
    private void OnPlayerDied(Character _)     => _livesLabel!.Text  = $"Vidas: {GameManager.Instance.Lives}";
    private void OnAdrenalineStarted()         { /* Efecto visual — flash rojo, pulso */ }
    private void OnAdrenalineEnded()           { /* Revertir efecto visual */ }

    public override void _ExitTree()
    {
        EventBus.Instance.HealthChanged     -= OnHealthChanged;
        EventBus.Instance.KeyCollected      -= OnKeyCollected;
        EventBus.Instance.CureCollected     -= OnCureCollected;
        EventBus.Instance.PlayerDied        -= OnPlayerDied;
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnded;
    }
}
```

---

## 13. Estándares de código C#

### Nomenclatura

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases | `PascalCase` | `SliceOPizza`, `StateMachine` |
| Interfaces | `IPascalCase` | `IState`, `IInteractable` |
| Métodos | `PascalCase` | `TakeDamage()`, `OnAdrenalineRush()` |
| Propiedades públicas | `PascalCase` | `CurrentHealth`, `MaxHealth` |
| Campos privados | `_camelCase` | `_current`, `_timeLeft` |
| Constantes | `UPPER_SNAKE_CASE` | `MAX_PROJECTILES`, `ANIM_ATTACK` |
| Enums (tipo y valores) | `PascalCase` | `GameState.Playing` |
| Parámetros / vars locales | `camelCase` | `delta`, `damageAmount` |
| Señales Godot | sufijo `EventHandler` | `HealthChangedEventHandler` |
| Nodos en escena | `PascalCase` | `AnimatedSprite2D`, `Hitbox` |

### Reglas de calidad

```csharp
// ✅ Nullable habilitado — siempre anotar tipos nullable con ?
private Character? _owner;

// ✅ Setter protegido cuando solo el dueño modifica
public int CurrentHealth { get; protected set; }

// ✅ Pattern matching sobre casting explícito
if (area is Hurtbox hurtbox) hurtbox.ReceiveDamage(Damage);

// ✅ Constantes para nombres de animaciones — nunca strings literales dispersos
private const string ANIM_IDLE = "idle";
Sprite.Play(ANIM_IDLE);

// ✅ XML Docs en todo método y clase pública
/// <summary>Aplica daño. Emite HealthChanged. Llama Die() en HP=0.</summary>
/// <param name="amount">Daño positivo a aplicar.</param>
public virtual void TakeDamage(int amount) { ... }

// ❌ Magic strings para nodos — usar [Export] o rutas relativas cortas
GetNode("/root/Level1/Enemies/SliceOPizza/Hitbox")  // ← frágil

// ❌ Múltiples clases en un mismo archivo
// SliceOPizza.cs debe contener solo public partial class SliceOPizza
```

### Una clase, un archivo

- El nombre del archivo coincide exactamente con la clase: `SliceOPizza.cs` → `public partial class SliceOPizza`.
- Usar `partial` en todas las clases que heredan de `Node` (requerido por el generador de Godot).
- Las clases de estados (`IState`) son POCO — no heredan de `Node` y no llevan `partial`.

---

## 14. Convenciones de Godot 4.7.2 con C# 14

### Referencias a nodos — solo en `_Ready()`

```csharp
// ✅ En _Ready()
public override void _Ready()
{
    _sprite  = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    _hitbox  = GetNode<Hitbox>("Hitbox");
}

// ❌ En el constructor o como inicializador de campo — el árbol no está listo
private AnimatedSprite2D _sprite = GetNode<AnimatedSprite2D>("...");  // crash
```

### Señales — siempre desconectar en `_ExitTree()`

```csharp
public override void _Ready()    => EventBus.Instance.AdrenalineStarted += OnRush;
public override void _ExitTree() => EventBus.Instance.AdrenalineStarted -= OnRush;
// Sin desconectar: instancias destruidas siguen recibiendo callbacks → crash / memory leak
```

### `[Export]` — solo configuración, no estado de runtime

```csharp
[ExportGroup("Stats")]
[Export] public float Speed     { get; set; } = 150f;   // ✅ Configurable en Inspector
[Export] public int   MaxHealth { get; set; } = 100;    // ✅ Configurable en Inspector

// ❌ No exportar estado de runtime
[Export] public int CurrentHealth { get; set; }   // ← no tiene sentido en Inspector
```

### Física de personajes

```
✅ CharacterBody2D + MoveAndSlide() → todo personaje jugable y enemigo
✅ Area2D con movimiento por código  → proyectiles
❌ RigidBody2D para entidades controladas por lógica → pierde control determinístico
```

### API exclusiva de Godot 4 (no Godot 3 / Mono)

```csharp
// ✅ Godot 4.7.2
Time.GetTicksMsec()
GetTree().ChangeSceneToFile("res://...")
AnimatedSprite2D (no AnimatedSprite)

// ❌ Godot 3 / Mono — no usar
OS.GetTicksMsec()
GetTree().ChangeScene("res://...")
AnimatedSprite
```

---

## 15. Estrategia de pruebas (TDD + gdUnit4)

### Filosofía TDD en este proyecto

1. **Escribe la prueba primero** para `StateMachine`, cálculo de daño y timer de Adrenalina.
2. **Pruebas unitarias headless** — lógica desacoplada del árbol de escenas (gdUnit4).
3. **Pruebas de integración manuales** por sprint, documentadas en `tests/integration/README.md`.
4. **Definición de Terminado:** toda historia con lógica no trivial tiene al menos una prueba pasando.

### Qué probar con gdUnit4 (headless)

| Clase / Lógica | Casos clave |
|---|---|
| `StateMachine` | Transiciones válidas, `Enter()`/`Exit()` llamados, transición al mismo estado ignorada, estado desconocido ignorado |
| `Character.TakeDamage()` | Reducción correcta de HP, señal emitida, `Die()` en HP=0, daño no reduce por debajo de 0 |
| `BlockState.TakeDamage()` | Reducción al 50% del daño |
| `AdrenalineController` | Timer decrece correctamente, `Deactivate()` al llegar a 0, modo permanente no desactiva, efecto aplicado y removido |
| `ProjectilePool` | `Get()` devuelve instancia activa, `Return()` la desactiva y reencola, expansión automática |
| `GameManager` | `Lives` decrementa, `GameOver` a `Lives==0`, `HasKey` y `HasCure` persisten |

### Ejemplo completo de prueba gdUnit4

```csharp
// res://tests/unit/StateMachineTest.cs
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class StateMachineTest
{
    private StateMachine _sm = null!;
    private MockState    _idle = null!;
    private MockState    _run  = null!;

    [Before]
    public void Setup()
    {
        _sm   = new StateMachine();
        _idle = new MockState();
        _run  = new MockState();

        _sm.RegisterState("Idle", _idle);
        _sm.RegisterState("Run",  _run);
        _sm.InitialState = "Idle";
        _sm.Start();
    }

    [TestCase]
    public void StartsInInitialState()
    {
        AssertThat(_sm.CurrentStateName).IsEqual("Idle");
        AssertThat(_idle.EnterCalled).IsTrue();
    }

    [TestCase]
    public void TransitionCallsExitThenEnter()
    {
        _sm.TransitionTo("Run");
        AssertThat(_idle.ExitCalled).IsTrue();
        AssertThat(_run.EnterCalled).IsTrue();
        AssertThat(_sm.CurrentStateName).IsEqual("Run");
    }

    [TestCase]
    public void TransitionToSameStateIsIgnored()
    {
        _sm.TransitionTo("Idle");
        AssertThat(_idle.ExitCalled).IsFalse();   // No debe llamar Exit si ya está en Idle
    }

    [TestCase]
    public void TransitionToUnknownStateIsIgnored()
    {
        _sm.TransitionTo("FlyingPig");
        AssertThat(_sm.CurrentStateName).IsEqual("Idle");
    }
}

/// <summary>Stub mínimo de IState para pruebas.</summary>
public class MockState : IState
{
    public bool EnterCalled { get; private set; }
    public bool ExitCalled  { get; private set; }
    public void Enter()                => EnterCalled = true;
    public void Exit()                 => ExitCalled  = true;
    public void Update(double d)       { }
    public void PhysicsUpdate(double d){ }
}
```

### Cobertura mínima por sprint

| Sprint | Pruebas mínimas nuevas |
|---|---|
| 0 | `StateMachineTest` — transiciones, enter/exit |
| 1 | `CharacterMovementTest` — velocidad, física |
| 2 | `TakeDamageTest`, `BlockDamageReductionTest` |
| 3 | `ProjectilePoolTest` — get, return, expand |
| 4 | `AdrenalineTimerTest`, `AdrenalineEffectTest` |
| 5 | `GameManagerLivesTest`, `VictoryConditionTest` |

---

## 16. CI/CD con GitHub Actions

### `.github/workflows/ci.yml`

```yaml
name: CI — Build + Test

on:
  push:
    branches: [main, "feature/**"]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    name: Build & Test (headless)
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.x"

      - name: Restore
        run: dotnet restore ElQuijote.csproj

      - name: Build (warnings = errores)
        run: dotnet build ElQuijote.csproj --no-restore -warnaserror

      - name: Test
        run: dotnet test --no-build --verbosity normal

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: "**/TestResults/*.xml"
```

### `.github/workflows/export.yml` — cierre de sprint

```yaml
name: Export — Sprint Build

on:
  workflow_dispatch:
    inputs:
      sprint:
        description: "Número de sprint (ej: 3)"
        required: true

jobs:
  export:
    runs-on: ubuntu-latest
    container:
      image: barichello/godot-ci:4.3   # Imagen con Godot headless

    steps:
      - uses: actions/checkout@v4

      - name: Export Windows
        run: |
          mkdir -p exports/windows
          godot --headless --export-release "Windows Desktop" exports/windows/ElQuijote.exe

      - name: Export Web
        run: |
          mkdir -p exports/web
          godot --headless --export-release "Web" exports/web/index.html

      - name: Upload
        uses: actions/upload-artifact@v4
        with:
          name: sprint-${{ github.event.inputs.sprint }}-build
          path: exports/
```

### Reglas de CI

- `main` está **protegida**: solo merges de PR con CI en verde. Cero pushes directos.
- Build fallido = PR bloqueado, sin excepciones.
- Test fallido = PR bloqueado, sin excepciones.
- `-warnaserror` activo siempre — ningún warning se acepta en `main`.

---

## 17. Flujo Git (GitHub Flow)

### Ramas

```
main                          ← Siempre deployable. CI en verde.
feature/hu00-setup-ci         ← formato: feature/<HU>-<slug>
feature/hu04-oleada-adrenalina
hotfix/fix-hitbox-overlap     ← Solo para bugs críticos post-merge
```

### Proceso por historia

```bash
# 1. Crear rama
git checkout -b feature/hu03-hud-vidas

# 2. Commits atómicos con Conventional Commits
git commit -m "feat(hud): add health bar connected to EventBus"
git commit -m "test(hud): add unit test for HealthChanged signal"

# 3. Push y PR hacia main
gh pr create --title "HU-03: HUD de vidas (Observer)" --body "Closes #3"

# 4. CI pasa → compañero revisa → merge → eliminar rama
```

### Conventional Commits — scope válidos

```
feat(combat)      fix(pool)         test(statemachine)
refactor(character)  docs(gemini)   chore(ci)
feat(adrenaline)  fix(hitbox)       test(gamemanager)
```

---

## 18. Gestión del proyecto: GitHub Projects + PowerShell

### Prerrequisitos del script `importar_videojuego_el_quijote.ps1`

```powershell
gh auth login
gh auth refresh -s project

# Crear campo Sprint si no existe
gh project field-create <NUM> --owner IJ-Studio --name "Sprint" \
  --data-type SINGLE_SELECT \
  --single-select-options "Sprint 0,Sprint 1,Sprint 2,Sprint 3,Sprint 4,Sprint 5"
```

**Ejecución:**

```powershell
# PowerShell 7+, raíz del repo
./importar_videojuego_el_quijote.ps1
```

**Etiquetas creadas automáticamente:**

```
historia  tarea  pair-programming  tdd  refactor  devops
patron-herencia  patron-singleton  patron-state
patron-observer  patron-object-pool  estado-global
```

> Estas etiquetas son exclusivas del proyecto videojuego. Si ambos proyectos
> (videojuego + app móvil) comparten organización, fusionar los `$BaseLabels`
> de ambos scripts antes de ejecutar.

---

## 19. Plan de sprints y Definición de Terminado

### Sprints

| Sprint | Fechas | Objetivo central |
|---|---|---|
| **0** | 28 sep – 4 oct | Repo, CI, gdUnit4, `Character`, `StateMachine`, `EventBus`, `GameManager` |
| **1** | 5 – 18 oct | Movimiento (WASD), plataformas, FSM Idle/Run/Jump/Crouch en Don Quijote |
| **2** | 19 oct – 1 nov | Combate (J/K), Hitbox/Hurtbox, HUD Observer, 5 vidas, Sancho Panza, 2J |
| **3** | 2 – 15 nov | Slice-O-Pizza + Burger-Bashing (polimorfismo), Object Pool, Nivel 1 completo |
| **4** | 16 – 29 nov | **Oleada de Adrenalina** (PP), Pasta-Demon, Taco-Furia, Nivel 2 completo |
| **5** | 30 nov – 13 dic | Taco Mayor, Nivel 3, flujo completo, sonido, pulido, build final |

### Definición de Terminado (por historia)

Una historia está **Terminada** cuando:

- [ ] `dotnet build -warnaserror` pasa sin errores.
- [ ] CI en verde (GitHub Actions).
- [ ] Al menos una prueba unitaria gdUnit4 para lógica no trivial.
- [ ] PR aprobado por el compañero (o Pair Programming documentado).
- [ ] Criterio de aceptación demostrable en el build del sprint.
- [ ] Si introduce un patrón nuevo, `GEMINI.md` o un `/// <summary>` lo documenta.
- [ ] Rama mergeada y eliminada.

---

## 20. Riesgos técnicos y mitigaciones

| Riesgo | Impacto | Mitigación |
|---|---|---|
| **Física + Oleada simultáneas** — duplicar velocidad sin desincronizar colisiones / animaciones / spawns | Alto | Probar `AdrenalineController` en nivel aislado (Sprint 0/1) antes de integrar al gameplay (Sprint 4) |
| **Nivel 3 con Oleada permanente** — carga sostenida de proyectiles todo el nivel | Alto | Object Pool pre-warmed ≥ 20 proyectiles/tipo; medir FPS en build de Sprint 4 antes de Sprint 5 |
| **Input 2 jugadores** — dos esquemas de teclado simultáneos | Medio | Configurar `InputMap` con perfiles `Player1_*` / `Player2_*` en Sprint 2 |
| **GC pressure en combate** — `new()` o `Instantiate()` por disparo | Medio | `ProjectilePool` desde Sprint 3; cero `new()` en el camino caliente |
| **Acoplamiento UI–Lógica** | Bajo | Observer estricto via `EventBus`; HUD sin referencias directas a `Character` |
| **Animaciones desincronizadas con estados** | Bajo | Cada estado controla su animación en `Enter()`/`Exit()`; nunca en `_Process()` |

---

## 21. Buenas prácticas de vibecoding con IA

### Antes de pedir código a Gemini

1. **Indica ruta y clase exacta:**
   > "Crea `AlertState` en `res://states/enemy_states/AlertState.cs`, implementa `IState`,
   > duplica `Speed` y `SpeedScale` del enemigo al entrar."

2. **Referencia el patrón existente:**
   > "Sigue el mismo patrón que `AttackState.cs` para conectar y desconectar la señal
   > de animación."

3. **Especifica las dependencias:**
   > "El constructor recibe `Enemy` como parámetro, igual que `EnemyAttackState`."

4. **Pide el test junto con la implementación:**
   > "Incluye el test gdUnit4 en `res://tests/unit/AlertStateTest.cs`."

### Durante la revisión del código generado

5. Verificar que no se violan las **reglas de capas** (UI accediendo directamente a lógica).
6. Confirmar que las señales se **desconectan en `_ExitTree()`**.
7. Confirmar que **no se usa `Engine.TimeScale`** para velocidad.
8. Confirmar que los proyectiles llaman **`Pool.Return(this)`**, no `QueueFree()`.
9. Confirmar que los nombres de animaciones usan **constantes**, no strings literales.
10. Confirmar que el código compila con **`-warnaserror`** (nullable, tipos, etc.).

### Después de recibir el código

```bash
dotnet build -warnaserror   # Debe pasar sin errores
dotnet test                 # Las nuevas pruebas deben pasar y las anteriores no romperse
git diff --stat             # Revisar el diff completo antes de git add
git commit -m "feat(...)"   # Un commit atómico por cambio lógico
```

### Prompts de referencia para Gemini CLI

```
# Nuevo estado de enemigo
"Crea EnemyDeadState.cs en res://states/enemy_states/.
Implementa IState. Al entrar, reproduce la animación 'death' del Sprite del Enemy
y al terminar la animación llama EventBus.Instance.EmitEnemyKilled(enemy) y luego
QueueFree() sobre el nodo. El constructor recibe Enemy.
Incluye el test gdUnit4 en res://tests/unit/EnemyDeadStateTest.cs."

# Nuevo enemigo
"Crea TacoFuria.cs en res://entities/enemies/.
Hereda de Enemy. Su ataque lanza SalsaPicante en arco de 30°.
Usa ProjectilePool.Instance.Get(ProjectileType.SalsaPicante).
En InitializeStates() registra GuardState, AlertState, EnemyAttackState y EnemyDeadState."

# Nueva señal en EventBus
"Agrega en EventBus.cs la señal CheckpointReached que emite el número de checkpoint (int).
Incluye el método de emisión tipado EmitCheckpointReached(int checkpoint)."
```

---

## 22. Anti-patrones y qué NO hacer

### Capas y acoplamiento

```csharp
// ❌ UI accede directamente al personaje
var hp = GetNode<DonQuijote>("/root/Level1/DonQuijote").CurrentHealth;

// ✅ UI solo escucha señales
EventBus.Instance.HealthChanged += OnHealthChanged;
```

```csharp
// ❌ Un estado conoce a otro estado directamente
public class IdleState : IState {
    private RunState _run = new RunState();  // acoplamiento directo
    public void Update(double d) { if (...) _run.Enter(); }
}

// ✅ Transición vía StateMachine
public void Update(double d) {
    if (...) _owner.StateMachine.TransitionTo("Run");
}
```

```csharp
// ❌ Proyectil instanciado directamente
var p = GD.Load<PackedScene>("res://projectiles/HiloDeQueso.tscn").Instantiate();
AddChild(p);

// ✅ Siempre el pool
var p = ProjectilePool.Instance.Get(ProjectileType.HiloDeQueso);
p.Launch(GlobalPosition, direction);
```

### Escenas de Godot

```
❌ Lógica de negocio en nodos de UI (.tscn de menú con código de combate)
❌ Un .tscn monolítico con todos los elementos del nivel hardcoded
❌ GetNode() con rutas largas absolutas → usar [Export] o rutas relativas
❌ Conectar señales en el editor (.tscn) para lógica que puede cambiar
   → Conectar por código en _Ready() para documentación y testabilidad
```

### Rendimiento

```
❌ Instanciar / destruir nodos en _PhysicsProcess() o _Process()
❌ GD.Load() dentro del gameplay — pre-cargar en _Ready() o al iniciar el nivel
❌ get_tree().get_nodes_in_group() en el camino caliente
   → mantener listas propias de referencias
❌ Engine.TimeScale para efectos de velocidad — escalar por entidad
```

### Testing

```
❌ Prueba que solo verifica "no truena" sin aserciones significativas
❌ Prueba acoplada al árbol de escenas (no corre headless)
❌ Un test gigante que cubre toda la clase → un test por comportamiento / caso borde
❌ Stubs que replican la lógica real → deben ser mínimos (solo registrar llamadas)
```

---

## 23. Checklist de entrega por sprint

### Sprint 0 — Infraestructura
- [ ] Repo en `IJ-Studio`, `main` protegida con regla de PR obligatorio
- [ ] `.github/workflows/ci.yml` corriendo (build + test headless)
- [ ] gdUnit4 configurado, `StateMachineTest` pasando en CI
- [ ] `Character.cs` (abstracta) + `StateMachine.cs` + `IState.cs` en `main`
- [ ] `EventBus.cs`, `GameManager.cs`, `AudioManager.cs` registrados en `project.godot`
- [ ] Estructura de carpetas canónica creada
- [ ] Script PowerShell ejecutado, backlog en GitHub Projects

### Sprint 1 — Movimiento y plataformas
- [ ] Don Quijote se mueve (A/D), salta (W), se agacha (S)
- [ ] FSM: Idle, Run, Jump, Crouch funcionando
- [ ] Cada estado controla su propia animación
- [ ] Colisiones con plataformas y desniveles
- [ ] `InputMap` con perfiles `Player1_*` configurado
- [ ] Build de prueba exportado (Windows o Web)

### Sprint 2 — Combate y 2 jugadores
- [ ] Ataque (J): `Hitbox.Monitoring` activa solo en `AttackState`
- [ ] Bloqueo (K): daño reducido al 50% en `BlockState`
- [ ] Sistema de vidas: 5 vidas, HUD reactivo via Observer
- [ ] `InputMap` `Player2_*` configurado
- [ ] Sancho Panza jugable con su propia FSM
- [ ] Modo 2 jugadores local sin conflicto de inputs

### Sprint 3 — Enemigos y Nivel 1
- [ ] SliceOPizza y BurgerBashing con polimorfismo (`Enemy` → subclase)
- [ ] `ProjectilePool` pre-warmed y operativo (cero `new()` en disparo)
- [ ] Nivel 1 (Almacén) completo: llave obtenible, puerta a Nivel 2
- [ ] `ProjectilePoolTest` pasando en CI

### Sprint 4 — Oleada de Adrenalina y Nivel 2
- [ ] `AdrenalineController` implementado sin `Engine.TimeScale`
- [ ] PastaDemon y TacoFuria con sus ataques
- [ ] Nivel 2 (Bóveda) con puzzle de plataforma + recolección de Cura
- [ ] Oleada se desactiva al eliminar último enemigo del grupo
- [ ] `AdrenalineTimerTest`, `AdrenalineEffectTest` pasando en CI

### Sprint 5 — Jefe, Nivel 3 y build final
- [ ] TacoMayor: fases embestida + proyectiles + ráfaga
- [ ] Nivel 3 (Cocina): Oleada activa todo el nivel, TacoMayor arranca en `AdrenalineState`
- [ ] Flujo completo: MainMenu → Niveles → Victory / GameOver → Créditos
- [ ] `AudioManager` con música diferenciada en Oleada
- [ ] Pantallas de Pausa, Diálogo y Victoria funcionales
- [ ] Build final exportado (Windows + Web) con CI en verde
- [ ] Todas las pruebas unitarias pasando
- [ ] Documento de evidencia por historia (capturas o video)

---

## Referencias cruzadas

| Documento | Propósito |
|---|---|
| `GDD_El_Quijote_APA7.docx` | Documento de diseño del juego — fuente de verdad para mecánicas y narrativa |
| `planificacion_videojuego_el_quijote.md` | Plan técnico: arquitectura, sprints y metodología |
| `plan_videojuego_el_quijote.json` | Backlog estructurado (consumido por el script PowerShell) |
| `importar_videojuego_el_quijote.ps1` | Script para poblar GitHub Projects |
| `GEMINI.md` (este archivo) | Fuente de verdad para Gemini CLI y el equipo |

---

*Última actualización: Sprint 0 (28 sep 2026).
Actualizar al final de cada sprint si cambian patrones, clases base o convenciones.*