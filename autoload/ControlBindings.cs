// res://autoload/ControlBindings.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Teclas de cada personaje (0 = Quijote, 1 = Sancho): moverse, saltar, agacharse, atacar,
/// defender e interactuar. Lógica estática, sin nodos: lee y modifica el InputMap y guarda
/// los cambios en user://controles.cfg para que se conserven entre partidas.
/// GameManager llama a LoadAndApply() al arrancar.
/// </summary>
public static class ControlBindings
{
    private const string SavePath = "user://controles.cfg";
    private const string Section = "teclas";

    private static readonly string[] Prefixes = { "player1_", "player2_" };
    private static readonly string[] PlayerNames = { "Don Quijote", "Sancho Panza" };

    /// <summary>Acciones que el jugador puede reasignar desde el menú de controles (en este orden).</summary>
    public static readonly string[] Rebindable = { "left", "right", "jump", "crouch", "attack", "block", "interact" };

    private static readonly Dictionary<string, string> ActionLabels = new()
    {
        ["left"]     = "Izquierda",
        ["right"]    = "Derecha",
        ["jump"]     = "Saltar",
        ["crouch"]   = "Agacharse",
        ["attack"]   = "Ataque",
        ["block"]    = "Defensa (escudo)",
        ["interact"] = "Interactuar"
    };

    /// <summary>Nombres cortos en español para las teclas especiales.</summary>
    private static readonly Dictionary<Key, string> KeyNames = new()
    {
        [Key.Escape] = "ESC", [Key.Space] = "ESPACIO", [Key.Enter] = "ENTER", [Key.KpEnter] = "NUM ENTER",
        [Key.Left] = "←", [Key.Right] = "→", [Key.Up] = "↑", [Key.Down] = "↓",
        [Key.Shift] = "SHIFT", [Key.Ctrl] = "CTRL", [Key.Alt] = "ALT", [Key.Tab] = "TAB",
        [Key.Backspace] = "BORRAR", [Key.Capslock] = "MAYÚS",
        [Key.Kp0] = "NUM 0", [Key.Kp1] = "NUM 1", [Key.Kp2] = "NUM 2", [Key.Kp3] = "NUM 3",
        [Key.Kp4] = "NUM 4", [Key.Kp5] = "NUM 5", [Key.Kp6] = "NUM 6", [Key.Kp7] = "NUM 7",
        [Key.Kp8] = "NUM 8", [Key.Kp9] = "NUM 9"
    };

    /// <summary>Teclas que no se pueden asignar: ESC cancela en los menús y la pausa es global.</summary>
    private static readonly Key[] Reserved = { Key.Escape };

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

        foreach (string name in AllRebindableNames())
        {
            var copy = new Godot.Collections.Array<InputEvent>();
            foreach (InputEvent ev in InputMap.ActionGetEvents(name)) copy.Add((InputEvent)ev.Duplicate());
            Defaults[name] = copy;
        }

        var config = new ConfigFile();
        if (config.Load(SavePath) != Error.Ok) return;

        foreach (string name in AllRebindableNames())
        {
            if (!config.HasSectionKey(Section, name)) continue;
            Variant value = config.GetValue(Section, name);
            var keys = new List<Key>();
            if (value.VariantType == Variant.Type.Int)
            {
                keys.Add((Key)(long)value); // formato anterior: una sola tecla
            }
            else
            {
                foreach (long code in value.AsInt64Array()) keys.Add((Key)code);
            }
            keys.RemoveAll(k => k == Key.None);
            if (keys.Count > 0) SetKeys(name, keys);
        }
    }

    /// <summary>Texto de las teclas asignadas (ej. "E", "W/ESPACIO", "NUM 1").</summary>
    public static string GetKeyLabel(int player, string action)
    {
        string name = ActionName(player, action);
        if (!InputMap.HasAction(name)) return "?";
        List<Key> keys = KeysOf(name);
        if (keys.Count == 0) return "—";
        var parts = new List<string>();
        foreach (Key key in keys)
        {
            parts.Add(KeyText(key));
            if (parts.Count == 2) break;
        }
        return string.Join("/", parts);
    }

    /// <summary>
    /// Asigna una tecla a una acción. Si otra acción (de cualquier jugador) ya la usaba,
    /// las dos se intercambian las teclas para que nunca haya dos acciones con la misma.
    /// </summary>
    /// <param name="message">Error si no se pudo, o aviso del intercambio si lo hubo.</param>
    public static bool TryRebind(int player, string action, Key key, out string? message)
    {
        message = null;
        string target = ActionName(player, action);
        if (!InputMap.HasAction(target))
        {
            message = "Esa acción no existe.";
            return false;
        }

        if (System.Array.IndexOf(Reserved, key) >= 0
            || (InputMap.HasAction("pause_menu") && KeysOf("pause_menu").Contains(key)))
        {
            message = $"{KeyText(key)} está reservada para la pausa y los menús.";
            return false;
        }

        List<Key> previous = KeysOf(target);
        if (previous.Count == 1 && previous[0] == key) return true;

        // ¿Otra acción usa esa tecla? Se queda con las teclas que tenía esta.
        foreach (string other in AllRebindableNames())
        {
            if (other == target || !KeysOf(other).Contains(key)) continue;

            List<Key> otherKeys = KeysOf(other);
            otherKeys.Remove(key);
            if (otherKeys.Count == 0) otherKeys.AddRange(previous);
            SetKeys(other, otherKeys);
            (int p, string a) = Split(other);
            message = $"{KeyText(key)} era de {PlayerName(p)} ({ActionLabel(a)}); ahora esa acción usa {DescribeKeys(otherKeys)}.";
        }

        SetKeys(target, new List<Key> { key });
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
        string L(string a) => GetKeyLabel(player, a);
        string left = L("left"), right = L("right");
        string move = left.Contains('/') || right.Contains('/') ? $"{left} {right}" : $"{left}/{right}";
        return $"{move} mover  {L("jump")} saltar  {L("crouch")} agacharse  " +
               $"{L("attack")} ataque  {L("block")} escudo  {L("interact")} interactuar";
    }

    // ── Utilidades internas ───────────────────────────────────────

    private static IEnumerable<string> AllRebindableNames()
    {
        for (int p = 0; p < Prefixes.Length; p++)
        {
            foreach (string action in Rebindable)
            {
                string name = ActionName(p, action);
                if (InputMap.HasAction(name)) yield return name;
            }
        }
    }

    private static (int Player, string Action) Split(string actionName)
    {
        for (int p = 0; p < Prefixes.Length; p++)
        {
            if (actionName.StartsWith(Prefixes[p])) return (p, actionName.Substring(Prefixes[p].Length));
        }
        return (0, actionName);
    }

    /// <summary>Teclas de teclado de una acción (sin repetir). Los botones del ratón se conservan aparte.</summary>
    private static List<Key> KeysOf(string actionName)
    {
        var keys = new List<Key>();
        foreach (InputEvent ev in InputMap.ActionGetEvents(actionName))
        {
            if (ev is not InputEventKey k) continue;
            Key code = k.PhysicalKeycode != Key.None ? k.PhysicalKeycode : k.Keycode;
            if (code != Key.None && !keys.Contains(code)) keys.Add(code);
        }
        return keys;
    }

    /// <summary>Reemplaza las teclas de una acción, dejando intactos otros eventos (ratón, mando).</summary>
    private static void SetKeys(string actionName, List<Key> keys)
    {
        foreach (InputEvent ev in InputMap.ActionGetEvents(actionName))
        {
            if (ev is InputEventKey) InputMap.ActionEraseEvent(actionName, ev);
        }
        foreach (Key key in keys)
            InputMap.ActionAddEvent(actionName, new InputEventKey { PhysicalKeycode = key });
    }

    private static string KeyText(Key key) =>
        KeyNames.TryGetValue(key, out string? name) ? name : OS.GetKeycodeString(key).ToUpperInvariant();

    private static string DescribeKeys(List<Key> keys) =>
        keys.Count == 0 ? "ninguna tecla" : string.Join("/", keys.ConvertAll(KeyText));

    private static void Save()
    {
        var config = new ConfigFile();
        foreach (string name in AllRebindableNames())
        {
            var codes = new List<long>();
            foreach (Key key in KeysOf(name)) codes.Add((long)key);
            config.SetValue(Section, name, codes.ToArray());
        }
        config.Save(SavePath);
    }
}
