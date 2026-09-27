using UnityEngine;

/// <summary>
/// Drives an audio-reactive CRT flicker on a screen material, in the style of the
/// Kid A Mnesia Exhibition "Idioteque" room: a green-tinted CRT that flickers, jitters
/// and gets noisier as the track's intensity rises.
///
/// Attach this to the CRT screen object (the one whose renderer uses CRT_IdiotequeScreen.shader,
/// or feeds into your existing CRT_Full / CRT_AnalogNoise chain). Assign the AudioSource playing
/// the track. No audio needed either -- you can drive Intensity manually (see below).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class IdiotequeCRTFlicker : MonoBehaviour
{
    [Header("Audio Source")]
    [Tooltip("AudioSource playing the track. If left null, drive Intensity manually via SetIntensity().")]
    public AudioSource audioSource;

    [Tooltip("Number of frequency bins sampled per channel. Must be a power of two.")]
    public int spectrumSize = 256;

    [Tooltip("Which FFT window to use for spectrum sampling.")]
    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;

    [Header("Intensity Shaping")]
    [Tooltip("How much of the spectrum (from the bottom) counts as 'bass/mid' drive. 0-1 fraction of spectrumSize.")]
    [Range(0.05f, 1f)] public float bandFraction = 0.35f;

    [Tooltip("Multiplies the raw band energy before clamping to 0-1.")]
    public float gain = 18f;

    [Tooltip("How quickly the smoothed intensity rises to meet a loud peak.")]
    public float attackSpeed = 18f;

    [Tooltip("How quickly the smoothed intensity falls off after a peak.")]
    public float decaySpeed = 4f;

    [Header("Flicker Look")]
    [Tooltip("Base flicker amount even at silence, so the screen never looks perfectly static.")]
    [Range(0f, 1f)] public float idleFlicker = 0.08f;

    [Tooltip("How strongly Intensity scales the flicker/jitter/noise on top of idleFlicker.")]
    public float intensityToFlicker = 1.2f;

    [Tooltip("Frequency of the fast flicker sine, in Hz-ish units.")]
    public float flickerSpeed = 40f;

    [Tooltip("Chance per second, scaled by Intensity, of a hard dropout/glitch frame.")]
    public float glitchFrequency = 1.5f;

    [Tooltip("How long a glitch dropout lasts, in seconds.")]
    public float glitchDuration = 0.05f;

    [Header("Green Tint")]
    public Color lowIntensityColor = new Color(0.05f, 0.35f, 0.10f);
    public Color highIntensityColor = new Color(0.25f, 1.0f, 0.35f);

    [Header("Shader Property Names")]
    public string flickerProp = "_FlickerIntensity";
    public string noiseProp = "_NoiseAmount";
    public string brightnessProp = "_Brightness";
    public string tintProp = "_ScreenTint";
    public string jitterProp = "_JitterAmount";

    /// <summary>Current smoothed 0-1 intensity value. Read-only from outside; drive it via SetIntensity() when not using audio.</summary>
    public float Intensity { get; private set; }

    float[] _spectrumL;
    float[] _spectrumR;
    float _rawTarget;
    Material _mat;
    MaterialPropertyBlock _mpb;
    float _glitchTimer;
    float _nextGlitchAt;
    bool _manualDrive;

    static readonly int FlickerID = Shader.PropertyToID("_FlickerIntensity");

    void Awake()
    {
        var renderer = GetComponent<Renderer>();
        _mat = renderer.material; // instance, safe to tweak per-object
        _mpb = new MaterialPropertyBlock();
        _spectrumL = new float[Mathf.NextPowerOfTwo(spectrumSize)];
        _spectrumR = new float[_spectrumL.Length];
        ScheduleNextGlitch();
    }

    void Update()
    {
        if (!_manualDrive)
        {
            SampleAudio();
        }

        // Attack/decay smoothing so the flicker breathes instead of jittering frame to frame.
        float speed = _rawTarget > Intensity ? attackSpeed : decaySpeed;
        Intensity = Mathf.MoveTowards(Intensity, _rawTarget, speed * Time.deltaTime);
        Intensity = Mathf.Clamp01(Intensity);

        UpdateGlitch();
        ApplyToMaterial();
    }

    void SampleAudio()
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            _rawTarget = 0f;
            return;
        }

        audioSource.GetSpectrumData(_spectrumL, 0, fftWindow);
        int bandCount = Mathf.Max(1, Mathf.RoundToInt(_spectrumL.Length * bandFraction));

        float sum = 0f;
        for (int i = 0; i < bandCount; i++)
        {
            sum += _spectrumL[i];
        }
        float average = sum / bandCount;

        _rawTarget = Mathf.Clamp01(average * gain);
    }

    void UpdateGlitch()
    {
        _glitchTimer -= Time.deltaTime;
        if (_glitchTimer <= 0f)
        {
            float chance = glitchFrequency * (0.15f + Intensity) * Time.deltaTime;
            if (Random.value < chance)
            {
                _glitchTimer = glitchDuration;
            }
        }
    }

    void ApplyToMaterial()
    {
        float flickerWave = 1f + Mathf.Sin(Time.time * flickerSpeed) * 0.5f
                             + (Mathf.PerlinNoise(Time.time * flickerSpeed * 0.5f, 0f) - 0.5f);

        float flickerAmount = idleFlicker + Intensity * intensityToFlicker * flickerWave;
        flickerAmount = Mathf.Clamp01(flickerAmount);

        // Hard glitch dropout: briefly slams flicker/noise to a spike.
        bool glitching = _glitchTimer > 0f;
        float noiseAmount = idleFlicker * 0.5f + Intensity * 0.9f + (glitching ? 0.6f : 0f);
        float jitterAmount = Intensity * 0.02f + (glitching ? 0.05f : 0f);
        float brightness = Mathf.Lerp(0.6f, 1.4f, Intensity) * (glitching ? Random.Range(0.3f, 1.6f) : 1f);

        Color tint = Color.Lerp(lowIntensityColor, highIntensityColor, Intensity);

        var renderer = GetComponent<Renderer>();
        renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(flickerProp, flickerAmount);
        _mpb.SetFloat(noiseProp, Mathf.Clamp01(noiseAmount));
        _mpb.SetFloat(brightnessProp, brightness);
        _mpb.SetFloat(jitterProp, jitterAmount);
        _mpb.SetColor(tintProp, tint);
        renderer.SetPropertyBlock(_mpb);
    }

    void ScheduleNextGlitch()
    {
        _nextGlitchAt = Time.time + Random.Range(0.5f, 2.5f);
    }

    /// <summary>
    /// Call this instead of assigning an AudioSource if you want to drive the flicker
    /// from your own song-intensity value (e.g. a beat-mapped curve, an analysis track, etc).
    /// Values should be 0-1; smoothing/attack/decay is still applied automatically.
    /// </summary>
    public void SetIntensity(float value)
    {
        _manualDrive = true;
        _rawTarget = Mathf.Clamp01(value);
    }

    /// <summary>Switch back to automatic audio-spectrum sampling.</summary>
    public void UseAudioSource()
    {
        _manualDrive = false;
    }
}
