// res://projectiles/HoldInteractDoor.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Base de las puertas que se abren manteniendo pulsada la tecla de interactuar.
/// Mientras un personaje esté dentro de la puerta y sostenga su tecla, una barra se llena;
/// al completarse se llama a OnHoldCompleted(). Si suelta, la barra se vacía.
/// El tiempo es corto a propósito: los enemigos siguen apareciendo mientras tanto.
/// </summary>
public abstract partial class HoldInteractDoor : Area2D
{
    /// <summary>Segundos que hay que mantener la tecla para abrir.</summary>
    [Export] public float HoldSeconds { get; set; } = 1.5f;

    /// <summary>Posición del indicador respecto a los pies de la puerta.</summary>
    [Export] public Vector2 PromptOffset { get; set; } = new(0f, -148f);

    /// <summary>Cuando es true la puerta ya se resolvió y no pide más interacción.</summary>
    protected bool Resolved { get; set; }

    private readonly List<Character> _inside = new();
    private HoldTimer _timer = new(1.5f);
    private HoldPrompt? _prompt;

    public override void _Ready()
    {
        _timer = new HoldTimer(HoldSeconds);
        _prompt = new HoldPrompt { Position = PromptOffset, Visible = false, ZIndex = 20 };
        AddChild(_prompt);
        AreaEntered += OnHoldAreaEntered;
        AreaExited += OnHoldAreaExited;
    }

    /// <summary>Se llama al llenarse la barra. La puerta decide si abre o rechaza.</summary>
    protected abstract void OnHoldCompleted();

    public override void _Process(double delta)
    {
        if (_prompt is null) return;
        if (Resolved)
        {
            _prompt.Visible = false;
            return;
        }

        _inside.RemoveAll(c => !IsInstanceValid(c));

        bool holding = false;
        foreach (Character c in _inside)
        {
            if (c.CurrentHealth > 0 && c.IsActionPressed("interact"))
            {
                holding = true;
                break;
            }
        }

        bool completed = _timer.Update(holding, (float)delta);
        _prompt.Progress = _timer.Progress;
        _prompt.Visible = _inside.Count > 0 || _timer.Progress > 0f;

        if (completed)
        {
            _timer.Reset();
            _prompt.Progress = 0f;
            OnHoldCompleted();
        }
    }

    private void OnHoldAreaEntered(Area2D area)
    {
        if (area is Hurtbox { Fighter: { } fighter } && !_inside.Contains(fighter))
        {
            _inside.Add(fighter);
            RefreshKeyLabel();
        }
    }

    private void OnHoldAreaExited(Area2D area)
    {
        if (area is Hurtbox { Fighter: { } fighter } && _inside.Remove(fighter))
            RefreshKeyLabel();
    }

    /// <summary>Muestra la tecla de interactuar de quien está dentro (ej. "E" o "E / Kp 0").</summary>
    private void RefreshKeyLabel()
    {
        if (_prompt is null || _inside.Count == 0) return;
        var labels = new List<string>();
        foreach (Character c in _inside)
        {
            string label = ControlBindings.GetKeyLabel(GameManager.PlayerIndexOf(c), "interact");
            if (!labels.Contains(label)) labels.Add(label);
        }
        _prompt.KeyLabel = string.Join(" / ", labels);
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnHoldAreaEntered;
        AreaExited -= OnHoldAreaExited;
    }
}
