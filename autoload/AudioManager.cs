// res://autoload/AudioManager.cs
using System.Collections.Generic;
using Godot;

/// <summary>
/// Singleton para gestionar música y SFX.
/// Acelera/normaliza audio durante la Oleada de Adrenalina.
/// Registrado como Autoload "AudioManager" en project.godot.
/// </summary>
public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; } = null!;

    [Export] public float NormalPitchScale { get; set; } = 1.0f;
    [Export] public float AdrenalinePitchScale { get; set; } = 1.5f;

    private AudioStreamPlayer? _musicPlayer;

    // ── Efectos de sonido (SFX) ───────────────────────────────────
    private const string SfxFolder = "res://assets/audio/sfx/";
    private const int SfxVoices = 12;
    private readonly Dictionary<string, AudioStream> _sfxCache = new();
    private readonly List<AudioStreamPlayer> _sfxPlayers = new();
    private int _nextVoice = 0;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        for (int i = 0; i < SfxVoices; i++)
        {
            var player = new AudioStreamPlayer();
            AddChild(player);
            _sfxPlayers.Add(player);
        }
        EventBus.Instance.AdrenalineStarted += OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded   += OnAdrenalineEnded;
    }

    /// <summary>
    /// Acelera la música al iniciar la oleada de Adrenalina.
    /// </summary>
    private void OnAdrenalineStarted()
    {
        if (_musicPlayer == null)
            _musicPlayer = GetNodeOrNull<AudioStreamPlayer>(".") ?? FindMusicPlayer();

        if (_musicPlayer != null)
        {
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Linear);
            tween.SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(_musicPlayer, "pitch_scale", AdrenalinePitchScale, 0.5f);
        }
    }

    /// <summary>
    /// Normaliza la música al terminar la oleada de Adrenalina.
    /// </summary>
    private void OnAdrenalineEnded()
    {
        if (_musicPlayer == null)
            _musicPlayer = FindMusicPlayer();

        if (_musicPlayer != null)
        {
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Linear);
            tween.SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(_musicPlayer, "pitch_scale", NormalPitchScale, 0.5f);
        }
    }

    /// <summary>
    /// Busca el reproductor de música en la escena actual de forma adaptativa.
    /// </summary>
    private AudioStreamPlayer? FindMusicPlayer()
    {
        var root = GetTree().CurrentScene;
        if (root == null) return null;

        // Buscar nodos con nombres comunes para música
        var candidates = new[] { "Music", "BGM", "Background", "MusicPlayer" };
        foreach (var name in candidates)
        {
            var node = root.FindChild(name, false, false) as AudioStreamPlayer;
            if (node != null) return node;
        }

        return null;
    }

    /// <summary>
    /// Reproduce un efecto de sonido por nombre (archivo en res://assets/audio/sfx/NOMBRE.wav).
    /// Usa un grupo fijo de voces para no crear nodos en cada golpe.
    /// </summary>
    public void PlaySfx(string name, float volumeDb = -6f)
    {
        if (!_sfxCache.TryGetValue(name, out var stream))
        {
            string path = SfxFolder + name + ".wav";
            if (!ResourceLoader.Exists(path)) return;
            stream = GD.Load<AudioStream>(path);
            _sfxCache[name] = stream;
        }

        var player = _sfxPlayers[_nextVoice];
        _nextVoice = (_nextVoice + 1) % _sfxPlayers.Count;
        player.Stream = stream;
        player.VolumeDb = volumeDb;
        player.PitchScale = (float)GD.RandRange(0.94, 1.06);
        player.Play();
    }

    public override void _ExitTree()
    {
        EventBus.Instance.AdrenalineStarted -= OnAdrenalineStarted;
        EventBus.Instance.AdrenalineEnded   -= OnAdrenalineEnded;
    }
}
