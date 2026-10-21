// res://ui/HoldPrompt.cs
using Godot;

/// <summary>
/// Indicador sobre un objeto interactuable: texto "Mantén [E]" y una barra que se llena
/// con el progreso. Se dibuja con _Draw, así no depende del layout de Controls.
/// </summary>
public partial class HoldPrompt : Node2D
{
    private const float BarWidth = 56f;
    private const float BarHeight = 8f;
    private const int FontSize = 12;

    private float _progress;
    private string _keyLabel = "E";

    public float Progress
    {
        get => _progress;
        set
        {
            float clamped = Mathf.Clamp(value, 0f, 1f);
            if (Mathf.IsEqualApprox(_progress, clamped)) return;
            _progress = clamped;
            QueueRedraw();
        }
    }

    public string KeyLabel
    {
        get => _keyLabel;
        set
        {
            if (_keyLabel == value) return;
            _keyLabel = value;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        Font font = ThemeDB.FallbackFont;
        string text = $"Mantén [{_keyLabel}]";
        Vector2 size = font.GetStringSize(text, HorizontalAlignment.Left, -1, FontSize);

        // Texto centrado con contorno para que se lea sobre cualquier fondo
        Vector2 textPos = new(-size.X / 2f, -BarHeight - 6f);
        DrawString(font, textPos + new Vector2(1, 1), text, HorizontalAlignment.Left, -1, FontSize, Colors.Black);
        DrawString(font, textPos, text, HorizontalAlignment.Left, -1, FontSize, new Color(1f, 0.95f, 0.8f));

        // Barra: fondo, relleno (amarillo → verde) y borde
        var back = new Rect2(-BarWidth / 2f, -BarHeight, BarWidth, BarHeight);
        DrawRect(back, new Color(0f, 0f, 0f, 0.7f));
        Color fill = new Color(1f, 0.8f, 0.2f).Lerp(new Color(0.45f, 0.9f, 0.3f), _progress);
        DrawRect(new Rect2(back.Position, new Vector2(BarWidth * _progress, BarHeight)), fill);
        DrawRect(back, Colors.Black, false, 1f);
    }
}
