// res://autoload/ControlBindings.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Teclas de ataque, defensa e interacción de cada personaje (0 = Quijote, 1 = Sancho).
/// Lógica estática, sin nodos: lee y modifica el InputMap y guarda los cambios en
/// user://controles.cfg para que se conserven entre partidas.
/// GameManager llama a LoadAndApply() al arrancar.
/// </summary>
public static class ControlBindings
{
    private const string SavePath = "user://controles.cfg";

    private static readonly string[] Prefixes = { "player1_", "player2_" };
    private static readonly string[] PlayerNames = { "Don Quijote", "Sancho Panza" };

    /// <summary>Acciones que el jugador puede reasignar desde el menú de controles.</summary>
    public static readonly string[] Rebindable = { "attack", "block", "interact" };

    private static readonly string[] AllActions = { "left", "right", "jump", "crouch", "attack", "block", "interact" };

    private static readonly Dictionary<string, string> ActionLabels = new()
    {
        ["attack"]   = "Ataque",
        ["block"]    = "Defensa (escudo)",
        ["interact"] = "Interactuar"
    };

    private static readonly Dictionary<string, Godot.Collections.Array<InputEvent>> Defaults = new();
    private static bool _initialized;

    public static string PlayerName(int player) => PlayerNames[Mathf.Clamp(player, 0, 1)];

    public static string ActionLabel(string action) => ActionLabels.TryGetValue(action, out string? label) ? label : action;

    public static string ActionName(int player, string action) => Prefixes[Mathf.Clamp(player, 0, 1)] + action;

    /// <summary>Guarda los valores por defecto del proyecto y aplica lo que el jugador cambió antes.</summary>
    public static void LoadAndApply()
    {
        if (_initialized) return;
        _initialized = true;

        for (int p = 0; p < Prefixes.Length; p++)
        {
            foreach (string action in Rebindable)
            {
                string name = ActionName(p, action);
                if (!InputMap.HasAction(name)) continue;
                var copy = new Godot.Collections.Array<InputEvent>();
                foreach (InputEvent ev in InputMap.ActionGetEvents(name)) copy.Add((InputEvent)ev.Duplicate());
                Defaults[name] = copy;
            }
        }

        var config = new ConfigFile();
        if (config.Load(SavePath) != Error.Ok) return;

        for (int p = 0; p < Prefixes.Length; p++)
        {
            foreach (string action in Rebindable)
            {
                string name = ActionName(p, action);
                if (!config.HasSectionKey("teclas", name)) continue;
                var key = (Key)(long)config.GetValue("teclas", name);
                if (key != Key.None && InputMap.HasAction(name)) SetKey(name, key);
            }
        }
    }

    /// <summary>Texto de la tecla asignada (ej. "E", "Kp 1", "Space").</summary>
    public static string GetKeyLabel(int player, string action)
    {
        string name = ActionName(player, action);
        if (!InputMap.HasAction(name)) return "?";
        foreach (InputEvent ev in InputMap.ActionGetEvents(name))
        {
            if (ev is InputEventKey key) return KeyText(key);
        }
        return "—";
    }

    /// <summary>
    /// Intenta asignar una tecla. Rechaza las que ya usa cualquier control de cualquier
    /// jugador o la pausa, para que no haya acciones pisándose.
    /// </summary>
    public static bool TryRebind(int player, string action, Key key, out string? error)
    {
        error = null;
        string target = ActionName(player, action);
        if (!InputMap.HasAction(target))
        {
            error = "Esa acción no existe.";
            return false;
        }

        for (int p = 0; p < Prefixes.Length; p++)
        {
            foreach (string other in AllActions)
            {
                string name = ActionName(p, other);
                if (name == target || !InputMap.HasAction(name)) continue;
                if (UsesKey(name, key))
                {
                    error = $"{OS.GetKeycodeString(key)} ya la usa {PlayerName(p)} ({other}).";
                    return false;
                }
            }
        }

        if (InputMap.HasAction("pause_menu") && UsesKey("pause_menu", key))
        {
            error = $"{OS.GetKeycodeString(key)} está reservada para la pausa.";
            return false;
        }

        SetKey(target, key);
        Save();
        return true;
    }

    /// <summary>Vuelve a las teclas originales del proyecto y borra lo guardado.</summary>
    public static void RestoreDefaults()
    {
        foreach (var (name, events) in Defaults)
        {
            if (!InputMap.HasAction(name)) continue;
            InputMap.ActionEraseEvents(name);
            foreach (InputEvent ev in events) InputMap.ActionAddEvent(name, (InputEvent)ev.Duplicate());
        }

        if (FileAccess.FileExists(SavePath)) DirAccess.RemoveAbsolute(SavePath);
    }

    /// <summary>Resumen de controles de un jugador para mostrar en pantalla.</summary>
    public static string Describe(int player)
    {
        string move = player == 0 ? "A/D mover  W/ESPACIO saltar  S agacharse" : "FLECHAS mover/saltar/agacharse";
        return $"{move}  {GetKeyLabel(player, "attack")} ataque  {GetKeyLabel(player, "block")} escudo  {GetKeyLabel(player, "interact")} interactuar";
    }

    private static void SetKey(string actionName, Key key)
    {
        InputMap.ActionEraseEvents(actionName);
        InputMap.ActionAddEvent(actionName, new InputEventKey { PhysicalKeycode = key });
    }

    private static bool UsesKey(string actionName, Key key)
    {
        foreach (InputEvent ev in InputMap.ActionGetEvents(actionName))
        {
            if (ev is InputEventKey k && (k.PhysicalKeycode == key || (k.PhysicalKeycode == Key.None && k.Keycode == key)))
                return true;
        }
        return false;
    }

    private static string KeyText(InputEventKey key)
    {
        Key code = key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode;
        return OS.GetKeycodeString(code);
    }

    private static void Save()
    {
        var config = new ConfigFile();
        for (int p = 0; p < Prefixes.Length; p++)
        {
            foreach (string action in Rebindable)
            {
                string name = ActionName(p, action);
                if (!InputMap.HasAction(name)) continue;
                foreach (InputEvent ev in InputMap.ActionGetEvents(name))
                {
                    if (ev is InputEventKey key)
                    {
                        Key code = key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode;
                        config.SetValue("teclas", name, (long)code);
                        break;
                    }
                }
            }
        }
        config.Save(SavePath);
    }
}
