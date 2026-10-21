// res://levels/HoldTimer.cs

/// <summary>
/// Cronómetro de "mantener pulsado": se llena mientras el jugador sostiene la tecla y se
/// vacía más rápido cuando la suelta. Lógica pura (sin nodos) para poder probarla headless.
/// </summary>
public sealed class HoldTimer
{
    /// <summary>Qué tan rápido se vacía respecto a lo que tarda en llenarse.</summary>
    private const float DrainSpeedMultiplier = 2f;

    public float Duration { get; }

    /// <summary>Avance de 0 (vacío) a 1 (lleno).</summary>
    public float Progress { get; private set; }

    public HoldTimer(float duration)
    {
        Duration = duration > 0.01f ? duration : 0.01f;
    }

    /// <summary>
    /// Avanza el cronómetro. Devuelve true solo en el fotograma en que se completa.
    /// </summary>
    public bool Update(bool holding, float delta)
    {
        float step = delta / Duration;
        if (holding)
        {
            Progress += step;
            if (Progress >= 1f)
            {
                Progress = 1f;
                return true;
            }
        }
        else
        {
            Progress -= step * DrainSpeedMultiplier;
            if (Progress < 0f) Progress = 0f;
        }
        return false;
    }

    public void Reset() => Progress = 0f;
}
