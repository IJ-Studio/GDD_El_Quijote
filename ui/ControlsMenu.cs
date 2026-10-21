// res://ui/ControlsMenu.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Menú de controles: una columna por personaje (Don Quijote y Sancho Panza) donde se
/// elige la tecla de ataque, defensa e interacción. Se abre como superposición desde el
/// menú principal y el de pausa; los cambios se guardan solos (ControlBindings).
/// </summary>
public partial class ControlsMenu : Control
{
    [Signal] public delegate void ClosedEventHandler();

    private readonly List<(int Player, string Action, Button Button)> _rows = new();
    private Label? _status;
    private Button? _closeButton;
    private (int Player, string Action, Button Button)? _listening;

    public override void _Ready()
    {
        // Debe seguir recibiendo input cuando el juego está en pausa
        ProcessMode = ProcessModeEnum.Always;
        SetAnchorsPreset(LayoutPreset.FullRect);

        var dim = new ColorRect { Color = new Color(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Stop };
        dim.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(dim);

        var center = new CenterContainer();
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var root = new VBoxContainer();
        root.AddThemeConstantOverride("separation", 16);
        center.AddChild(root);

        var title = new Label { Text = "CONTROLES", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.82f, 0.3f));
        root.AddChild(title);

        var columns = new HBoxContainer();
        columns.AddThemeConstantOverride("separation", 48);
        root.AddChild(columns);

        columns.AddChild(BuildColumn(0, new Color(1f, 0.82f, 0.3f)));
        columns.AddChild(BuildColumn(1, new Color(1f, 0.7f, 0.3f)));

        _status = new Label
        {
            Text = "Elige un control y pulsa la tecla nueva (ESC cancela).",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _status.AddThemeFontSizeOverride("font_size", 10);
        root.AddChild(_status);

        var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        buttons.AddThemeConstantOverride("separation", 16);
        root.AddChild(buttons);

        var restore = new Button { Text = "RESTAURAR" };
        restore.Pressed += OnRestorePressed;
        buttons.AddChild(restore);

        _closeButton = new Button { Text = "VOLVER" };
        _closeButton.Pressed += Close;
        buttons.AddChild(_closeButton);

        if (_rows.Count > 0) _rows[0].Button.GrabFocus();
    }

    private VBoxContainer BuildColumn(int player, Color nameColor)
    {
        var column = new VBoxContainer { CustomMinimumSize = new Vector2(300f, 0f) };
        column.AddThemeConstantOverride("separation", 10);

        var header = new Label
        {
            Text = player == 0 ? "DON QUIJOTE (J1)" : "SANCHO PANZA (J2)",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        header.AddThemeFontSizeOverride("font_size", 14);
        header.AddThemeColorOverride("font_color", nameColor);
        column.AddChild(header);

        foreach (string action in ControlBindings.Rebindable)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);

            var label = new Label { Text = ControlBindings.ActionLabel(action), SizeFlagsHorizontal = SizeFlags.ExpandFill };
            label.AddThemeFontSizeOverride("font_size", 11);
            row.AddChild(label);

            var button = new Button { CustomMinimumSize = new Vector2(120f, 0f) };
            int capturedPlayer = player;
            string capturedAction = action;
            button.Pressed += () => StartListening(capturedPlayer, capturedAction, button);
            row.AddChild(button);

            column.AddChild(row);
            _rows.Add((player, action, button));
        }

        RefreshLabels();
        return column;
    }

    private void RefreshLabels()
    {
        foreach (var (player, action, button) in _rows)
            button.Text = ControlBindings.GetKeyLabel(player, action);
    }

    private void StartListening(int player, string action, Button button)
    {
        _listening = (player, action, button);
        button.Text = "Pulsa una tecla...";
        if (_status is not null) _status.Text = $"{ControlBindings.PlayerName(player)}: elige la tecla para «{ControlBindings.ActionLabel(action)}».";
    }

    private void StopListening()
    {
        _listening = null;
        RefreshLabels();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;

        if (_listening is { } listening)
        {
            GetViewport().SetInputAsHandled();
            Key code = key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode;

            if (code == Key.Escape)
            {
                if (_status is not null) _status.Text = "Cambio cancelado.";
                StopListening();
                return;
            }

            if (ControlBindings.TryRebind(listening.Player, listening.Action, code, out string? error))
            {
                if (_status is not null)
                    _status.Text = $"{ControlBindings.PlayerName(listening.Player)}: {ControlBindings.ActionLabel(listening.Action)} = {OS.GetKeycodeString(code)}";
            }
            else if (_status is not null)
            {
                _status.Text = error ?? "No se pudo asignar esa tecla.";
            }
            StopListening();
            return;
        }

        if (key.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
    }

    private void OnRestorePressed()
    {
        ControlBindings.RestoreDefaults();
        StopListening();
        if (_status is not null) _status.Text = "Controles restaurados.";
    }

    private void Close()
    {
        EmitSignal(SignalName.Closed);
        QueueFree();
    }
}
