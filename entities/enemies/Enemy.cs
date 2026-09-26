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
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineRush;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnd;
    }
}
