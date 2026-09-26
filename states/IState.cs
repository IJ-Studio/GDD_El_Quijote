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
