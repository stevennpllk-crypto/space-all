using UnityEngine;

/// <summary>Creates original lightweight engine and ambient audio procedurally.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class ProceduralSpaceAudio : MonoBehaviour
{
    private AudioSource engineSource;
    private AudioSource ambienceSource;
    private Rigidbody body;
    public float maximumEnginePitch = 1.65f;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        engineSource = GetComponent<AudioSource>();
        engineSource.clip = CreateEngineHum();
        engineSource.loop = true;
        engineSource.volume = 0.18f;
        engineSource.spatialBlend = 0.8f;
        engineSource.minDistance = 4f;
        engineSource.maxDistance = 90f;
        engineSource.playOnAwake = false;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.clip = CreateAmbientDrone();
        ambienceSource.loop = true;
        ambienceSource.volume = 0.07f;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.playOnAwake = false;
    }

    private void Start()
    {
        engineSource.Play();
        ambienceSource.Play();
    }

    private void Update()
    {
        float speedRatio = body != null ? Mathf.Clamp01(body.velocity.magnitude / 42f) : 0f;
        engineSource.pitch = Mathf.Lerp(0.82f, maximumEnginePitch, speedRatio);
        engineSource.volume = Mathf.Lerp(0.1f, 0.23f, speedRatio);
    }

    private static AudioClip CreateEngineHum()
    {
        const int rate = 22050;
        const int seconds = 2;
        int count = rate * seconds;
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / rate;
            float hum = Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.42f
                      + Mathf.Sin(2f * Mathf.PI * 73f * t) * 0.2f
                      + Mathf.Sin(2f * Mathf.PI * 96f * t) * 0.12f;
            float pulse = 0.82f + 0.18f * Mathf.Sin(2f * Mathf.PI * 2f * t);
            samples[i] = hum * pulse * 0.32f;
        }
        AudioClip clip = AudioClip.Create("Original Procedural Ship Engine", count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateAmbientDrone()
    {
        const int rate = 22050;
        const int seconds = 6;
        int count = rate * seconds;
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / rate;
            float fade = 0.72f + 0.28f * Mathf.Sin(2f * Mathf.PI * 0.1666667f * t);
            samples[i] = (Mathf.Sin(2f * Mathf.PI * 36f * t) * 0.5f
                        + Mathf.Sin(2f * Mathf.PI * 54f * t) * 0.25f) * fade * 0.12f;
        }
        AudioClip clip = AudioClip.Create("Original Procedural Deep Space Ambience", count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
