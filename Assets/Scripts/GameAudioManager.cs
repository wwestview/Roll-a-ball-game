using UnityEngine;

/// <summary>
/// Procedural audio manager that generates all game sounds mathematically.
/// No external audio files needed — everything is created via AudioClip.Create().
/// Singleton pattern for easy access from other scripts.
/// </summary>
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.15f;
    [Range(0f, 1f)] public float sfxVolume = 0.6f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private AudioClip pickupClip;
    private AudioClip wallHitClip;
    private AudioClip ambientClip;

    private const int SAMPLE_RATE = 44100;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Create audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        // Generate all audio clips
        GenerateAllClips();

        // Start ambient music
        musicSource.clip = ambientClip;
        musicSource.Play();
    }

    void GenerateAllClips()
    {
        pickupClip = GeneratePickupSound();
        wallHitClip = GenerateWallHitSound();
        ambientClip = GenerateAmbientMusic();
    }

    /// <summary>
    /// Play the "ding" pickup collection sound.
    /// </summary>
    public void PlayPickupSound()
    {
        if (sfxSource != null && pickupClip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.2f); // Slight variation
            sfxSource.PlayOneShot(pickupClip, sfxVolume);
        }
    }

    /// <summary>
    /// Play the wall collision impact sound.
    /// </summary>
    public void PlayWallHitSound()
    {
        if (sfxSource != null && wallHitClip != null)
        {
            sfxSource.pitch = Random.Range(0.8f, 1.1f);
            sfxSource.PlayOneShot(wallHitClip, sfxVolume * 0.7f);
        }
    }

    /// <summary>
    /// Generates a bright, rising "ding" sound for pickup collection.
    /// Two overlapping sine tones with rising pitch.
    /// </summary>
    AudioClip GeneratePickupSound()
    {
        float duration = 0.35f;
        int sampleCount = (int)(SAMPLE_RATE * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float normalizedT = (float)i / sampleCount;

            // Rising frequency chirp (880 Hz -> 1760 Hz)
            float freq1 = 880f + normalizedT * 880f;
            // Second harmonic
            float freq2 = 1320f + normalizedT * 660f;

            // Sine waves
            float wave1 = Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.5f;
            float wave2 = Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.3f;

            // Envelope: quick attack, sustain, then decay
            float envelope;
            if (normalizedT < 0.05f)
                envelope = normalizedT / 0.05f; // Attack
            else
                envelope = Mathf.Exp(-3f * (normalizedT - 0.05f)); // Decay

            samples[i] = (wave1 + wave2) * envelope;
        }

        AudioClip clip = AudioClip.Create("PickupDing", sampleCount, 1, SAMPLE_RATE, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generates a short impact/thud sound for wall collisions.
    /// Noise burst with rapid exponential decay.
    /// </summary>
    AudioClip GenerateWallHitSound()
    {
        float duration = 0.2f;
        int sampleCount = (int)(SAMPLE_RATE * duration);
        float[] samples = new float[sampleCount];

        System.Random rng = new System.Random(123);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float normalizedT = (float)i / sampleCount;

            // Low-frequency thump (base tone ~100Hz decaying)
            float baseTone = Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.4f;

            // Add some mid-frequency content
            float midTone = Mathf.Sin(2f * Mathf.PI * 250f * t) * 0.2f;

            // Filtered noise
            float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.3f;

            // Sharp exponential decay envelope
            float envelope = Mathf.Exp(-15f * normalizedT);

            // Extra initial click
            float click = normalizedT < 0.01f ? (1f - normalizedT / 0.01f) * 0.5f : 0f;

            samples[i] = (baseTone + midTone + noise + click) * envelope;
        }

        AudioClip clip = AudioClip.Create("WallHit", sampleCount, 1, SAMPLE_RATE, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generates ambient background music — a slowly evolving pad/drone.
    /// Multiple detuned sine waves creating a spacey, atmospheric sound.
    /// </summary>
    AudioClip GenerateAmbientMusic()
    {
        float duration = 30f; // 30 seconds, loops
        int sampleCount = (int)(SAMPLE_RATE * duration);
        float[] samples = new float[sampleCount];

        // Base frequencies for a Cm chord (C, Eb, G) spread across octaves
        float[] freqs = { 65.41f, 77.78f, 98.0f, 130.81f, 155.56f, 196.0f };
        float[] amps = { 0.15f, 0.12f, 0.10f, 0.08f, 0.06f, 0.05f };

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            float normalizedT = (float)i / sampleCount;
            float sample = 0f;

            for (int j = 0; j < freqs.Length; j++)
            {
                // Slowly modulate each frequency for organic feel
                float freqMod = 1f + 0.002f * Mathf.Sin(2f * Mathf.PI * (0.05f + j * 0.01f) * t);
                float freq = freqs[j] * freqMod;

                // Sine wave with slight detuning
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);

                // Add subtle overtone
                wave += 0.3f * Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t);

                // Amplitude modulation (slow breathing)
                float ampMod = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * (0.1f + j * 0.02f) * t);

                sample += wave * amps[j] * ampMod;
            }

            // Add very quiet high-frequency shimmer
            float shimmer = Mathf.Sin(2f * Mathf.PI * 523.25f * t) *
                           Mathf.Sin(2f * Mathf.PI * 0.25f * t) * 0.02f;
            sample += shimmer;

            // Soft fade in/out at loop boundaries
            float fadeTime = 2f; // 2 seconds fade
            float fadeSamples = fadeTime * SAMPLE_RATE;
            if (i < fadeSamples)
                sample *= (float)i / fadeSamples;
            else if (i > sampleCount - fadeSamples)
                sample *= (float)(sampleCount - i) / fadeSamples;

            // Soft clamp
            samples[i] = Mathf.Clamp(sample, -0.8f, 0.8f);
        }

        AudioClip clip = AudioClip.Create("AmbientMusic", sampleCount, 1, SAMPLE_RATE, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
