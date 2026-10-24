# El Quijote: La Búsqueda de la Sazón Perdida

Beat 'em up / plataformas 2D en **Godot 4.7.2 (.NET) + C# 14 / .NET 10**.
IJ-STUDIO · Optativa 1: Creación de Videojuegos · Docente: Juvencio Mendoza Castelán

---

## 1. Instalación (solo la primera vez)

1. **.NET 10 SDK** → <https://dotnet.microsoft.com/download/dotnet/10.0>
   Descarga el instalador **SDK x64 para Windows** e instálalo. Verifica en una terminal:
   `dotnet --version` → debe mostrar `10.x.x`.
2. **Godot 4.7.2 – versión .NET** → <https://godotengine.org/download/windows/>
   Elige **"Godot Engine – .NET"** (NO la versión normal; esa no ejecuta C#).
   Es un .zip: descomprímelo en una carpeta (ej. `C:\Godot`) y abre `Godot_v4.7.2-stable_mono_win64.exe`.
3. En Godot: **Importar** → selecciona `El-Quijote\project.godot` → **Importar y editar**.
   La primera vez tarda un poco importando sprites.
4. Pulsa el botón **Build** (martillo, arriba a la derecha) para compilar el C#.
   Debe terminar sin errores.
5. Pulsa **F5** (▶) para jugar. La escena principal es `ui/MainMenu.tscn`.

> Si Godot dice *".NET SDK not found"*: reinicia la PC después de instalar el SDK.
> Si sale *"Godot.NET.Sdk/4.7.2 not found"*: revisa que estés usando Godot **4.7.2 .NET** exactamente.

## 2. Controles

| Acción | Jugador 1 (Don Quijote) | Jugador 2 (Sancho) |
|---|---|---|
| Moverse | A / D | ← / → |
| Saltar | W o Espacio | ↑ |
| Agacharse | S | ↓ |
| Atacar | J o clic izquierdo | M o Num 1 |
| Escudo (bloqueo) | K (+ W = escudo alto) | N o Num 2 |
| Interactuar | E | Num 0 |
| Pausa | P / Esc | P / Esc |
| Avanzar diálogo | Enter / Espacio / atacar / clic | — |

Escudo normal: recibe muy poco daño. Escudo alto: bloquea todo.

**Todas las teclas se pueden cambiar** en *Menú principal → Controles* (o desde la pausa).
Si eliges una tecla que ya usa otra acción, las dos se intercambian. Los cambios se guardan
solos (`user://controles.cfg`) y *Restaurar* vuelve a las de la tabla.

**Mecánicas de combate:**
- Los enemigos **avisan antes de atacar** (se quedan quietos y parpadean en rojo). Si les pegas
  en ese momento, su ataque se cancela y se tambalean.
- Al recibir un golpe te empuja **lejos de quien te pegó** (antes podía lanzarte contra el enemigo).
- **Agachado** esquivas el hilo de queso de la pizza (pasa por encima). Lo que cae en arco
  (albóndigas, salsa) sí te alcanza.
- Pulsar atacar al final de un golpe **encadena** el siguiente; si mantienes una dirección,
  el golpe sale hacia ese lado.
- Salto más justo: puedes saltar un instante después de dejar el borde y el salto pulsado
  justo antes de aterrizar no se pierde.

## 3. Flujo del juego

**Menú** (1 o 2 jugadores) → **Nivel 1: Almacén** (llave → puerta) →
**Nivel 2: Bóveda de la Cura** (pisar la plataforma de reversión → tomar la Cura → puerta) →
**Nivel 3: Cocina** (jefe Taco Mayor, 3 fases, adrenalina permanente) → **Victoria / créditos**.
Al perder las 5 vidas → **Game Over** (reintentar o menú).

Cada nivel tiene diálogo de introducción con las ilustraciones de `assets/dialogos/`,
cofre de **Oleada de Adrenalina** (enemigos ×2 de velocidad; al vencerlos suelta comida) y comida que cura.

## 4. Arquitectura (patrones del GDD)

| Patrón | Dónde |
|---|---|
| Herencia abstracta | `entities/Character.cs` → `DonQuijote`, `SanchoPanza`, `Enemy` → `SliceOPizza`, `BurgerBashing`, `PastaDemon`, `TacoFuria`, `TacoMayor` |
| State (FSM) | `states/StateMachine.cs` + `IState`; estados en `states/player_states` y `states/enemy_states` |
| Observer | `autoload/EventBus.cs` (señales). `Hud`, `FeedbackManager`, `AudioManager`, `GameManager` solo escuchan |
| Singleton (Autoload) | `EventBus`, `GameManager`, `AudioManager`, `FeedbackManager` |
| Object Pool | `projectiles/ProjectilePool.cs` (hilo de queso, albóndiga, salsa) |

Capas de colisión: 1 mundo · 2 jugadores · 3 enemigos · 4 hurtbox jugador · 5 hurtbox enemigo.

## 5. Qué se integró en esta versión

**Del prototipo de José (GDScript → C#):** todos los sprites y animaciones (Quijote, Sancho, pizza,
hamburguesa, albóndiga, tacos, objetos, UI), sonidos, fuente Press Start 2P, vibración de cámara,
destello al recibir golpe, partículas, números de daño, chispa de bloqueo y fondo del almacén.

**Nuevo:** `FeedbackManager` (game feel por Observer), `LevelCamera`, `LevelController`,
`DialogueBox` + `DialogueLibrary` (diálogos de las 5 escenas), `GameOverScreen`, `FoodPickup`,
HUD con corazones, llave/cura, barra del jefe y aviso de adrenalina; modo 2 jugadores.

**Errores del repo original que se corrigieron:**
- Escenas `.tscn` de UI con sintaxis inválida y `ext_resource type="Scene"`; faltaba el InputMap completo.
- Capas/máscaras de colisión inconsistentes; la explosión de proyectiles no detectaba hurtboxes.
- `StateMachine.CurrentStateName` siempre devolvía "Unknown" (el bloqueo nunca funcionaba).
- PastaDemon y TacoFuria nunca atacaban; los enemigos no tenían gravedad.
- La Hitbox no se volteaba con el personaje (el ataque salía hacia atrás).
- `VaultDoor` siempre terminaba el nivel 1; `VictoryDoor` cambiaba de escena dos veces;
  `CureCollected` nunca se emitía.
- Cambios de `Monitoring` dentro de señales de física (ahora con `SetDeferred`).

## 6. Pendiente / ideas

- Los fondos de Bóveda y Cocina son provisionales (generados); se pueden reemplazar en
  `assets/backgrounds/boveda.png` y `cocina.png` con la misma medida (1440×660).
- Doble salto y dash del prototipo GDScript no están en el GDD; se pueden añadir como estados nuevos.
- Animaciones extra de Gemini (caminata de pizza mejorada, etc.) se cambian en `assets/sprites/<personaje>/`.

## 7. Subir a GitHub

Copia el contenido de esta carpeta dentro de tu clon de `IJ-Studio/GDD_El_Quijote`
(reemplazando archivos), luego:

```bash
git checkout -b integracion-arte
git add .
git commit -m "feat: integra arte, efectos, diálogos y corrige mecánicas"
git push -u origin integracion-arte
```

No subas la carpeta `.godot/` (ya está en `.gitignore`).
