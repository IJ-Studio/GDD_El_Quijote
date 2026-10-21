// res://ui/DialogueLibrary.cs
using System.Collections.Generic;

/// <summary>Una línea de diálogo: quién habla y qué dice.</summary>
public record DialogueLine(string Speaker, string Text);

/// <summary>Una escena de diálogo: ilustración de fondo y sus líneas.</summary>
public record DialogueScene(string ImagePath, IReadOnlyList<DialogueLine> Lines);

/// <summary>
/// Textos de la historia. Cada id corresponde a un momento del juego
/// (lo pide LevelController o la pantalla de victoria por EventBus).
/// </summary>
public static class DialogueLibrary
{
    private static readonly Dictionary<string, DialogueScene> Scenes = new()
    {
        ["intro"] = new("res://assets/dialogos/escena_1.png", new DialogueLine[]
        {
            new("El Patrón", "¡Don Quijote, Sancho! Un brote de hiperprocesamiento mutó los insumos del sótano."),
            new("El Patrón", "¡Se apoderaron de la Cura de la Sazón, la fórmula secreta del restaurante!"),
            new("Don Quijote", "¡Por mi lanza que la recuperaremos! Sancho, busquemos la llave de la Bóveda."),
        }),
        ["boveda"] = new("res://assets/dialogos/escena_2.png", new DialogueLine[]
        {
            new("Fantasma del Chef", "¡Avanzan bien, valientes guerreros! Tienen la llave..."),
            new("Fantasma del Chef", "...pero en la Bóveda les espera una horda implacable de Pastas y Tacos."),
            new("Fantasma del Chef", "Pisen la losa de reversión para liberar la Cura del pedestal."),
        }),
        ["cura"] = new("res://assets/dialogos/escena_3.png", new DialogueLine[]
        {
            new("Don Quijote", "¡Lo logramos, Sancho! La Cura de la Sazón está a salvo en nuestras manos."),
            new("Don Quijote", "¡Mírala resplandecer! Ahora, a la salida de la Bóveda."),
        }),
        ["jefe"] = new("res://assets/dialogos/escena_4.png", new DialogueLine[]
        {
            new("Taco Mayor", "¡Nadie sale de esta cocina con la Cura!"),
            new("Taco Mayor", "¡El Restaurante El Quijote ahora le pertenece al Gran Taco!"),
        }),
        ["victoria"] = new("res://assets/dialogos/escena_5.png", new DialogueLine[]
        {
            new("El Patrón", "¡Increíble! Han derrotado al Taco Mayor y devuelto la Cura."),
            new("El Patrón", "¡Nuestra receta distintiva vuelve a brillar!"),
        }),
    };

    public static bool TryGet(string id, out DialogueScene scene) => Scenes.TryGetValue(id, out scene!);
}
