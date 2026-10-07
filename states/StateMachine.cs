// res://states/StateMachine.cs
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Motor genérico de la FSM. Se registra como hijo del nodo Character en la escena.
/// Los estados se registran por código o se descubren automáticamente como nodos hijos.
/// </summary>
public partial class StateMachine : Node
{
    private IState?                     _current;
    private Dictionary<string, IState>  _states = new();

    [Export] public string InitialState { get; set; } = "Idle";

    public override void _Ready()
    {
        // Auto-registrar nodos hijos que implementen IState
        foreach (Node child in GetChildren())
        {
            if (child is IState state)
            {
                RegisterState(child.Name, state);
            }
        }
    }

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
