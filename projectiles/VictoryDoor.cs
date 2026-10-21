// res://projectiles/VictoryDoor.cs
using Godot;

/// <summary>
/// Puerta de victoria del Nivel 3.
/// Solo permite cruzarla si se cumplen SIMULTÁNEAMENTE:
/// 1. TacoMayor ha sido derrotado
/// 2. GameManager.HasCure == true
/// 3. GameManager.CurrentLives >= 1
///
/// Si se cumplen, emite EventBus.LevelCompleted() y transiciona al próximo nivel.
/// Para abrirla hay que MANTENER pulsada la tecla de interactuar hasta llenar la barra.
/// </summary>
public partial class VictoryDoor : HoldInteractDoor
{
    private TacoMayor? _boss;
    private bool _isBossDefeated = false;

    public override void _Ready()
    {
        base._Ready();
        EventBus.Instance.EnemyKilled += OnEnemyKilled;

        // Buscar TacoMayor en la escena padre
        _boss = GetTree().Root.GetChild(GetTree().Root.GetChildCount() - 1)
            .FindChild("TacoMayor", false, false) as TacoMayor;
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        if (enemy is TacoMayor)
        {
            _isBossDefeated = true;
        }
    }

    protected override void OnHoldCompleted()
    {
        if (ValidateVictoryConditions())
        {
            TriggerVictory();
        }
        else
        {
            TriggerRejection();
        }
    }

    private bool ValidateVictoryConditions()
    {
        bool bossDefeated = _isBossDefeated;
        bool hasCure = GameManager.Instance.HasCure;
        bool hasLives = GameManager.Instance.CurrentLives >= 1;

        GD.PrintRich($"[color=yellow]VictoryDoor Check:[/color] Boss={bossDefeated}, Cure={hasCure}, Lives={hasLives}");

        return bossDefeated && hasCure && hasLives;
    }

    private void TriggerVictory()
    {
        GD.PrintRich("[color=green]✓ VICTORIA ALCANZADA[/color]");
        var sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (sprite?.SpriteFrames?.HasAnimation("open") == true) sprite.Play("open");
        AudioManager.Instance?.PlaySfx("recoger", -4f);
        Resolved = true;
        // GameManager cambia a la pantalla de victoria al recibir LevelCompleted(3)
        GetTree().CreateTimer(0.8).Timeout += () => EventBus.Instance.EmitLevelCompleted(3);
    }

    private void TriggerRejection()
    {
        GD.PrintRich("[color=red]✗ No se cumplen condiciones de victoria[/color]");

        // Flash rojo de rechazo (sin await)
        var tween = CreateTween();
        var originalModulate = Modulate;

        Modulate = Colors.Red.Lerp(Colors.White, 0.5f);
        tween.TweenCallback(Callable.From(() =>
        {
            Modulate = originalModulate;
        })).SetDelay(0.3);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (EventBus.Instance != null)
            EventBus.Instance.EnemyKilled -= OnEnemyKilled;
    }
}
