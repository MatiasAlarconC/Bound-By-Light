using UnityEngine;

/// <summary>
/// Dedicated audio layer for Level 3. It replaces the copied level music and
/// reacts to the manta, whale, vortex and final light beats.
/// </summary>
public class Level3AudioDirector : MonoBehaviour
{
    [Header("References")]
    public Transform babosa;
    public Transform mantarraya;
    public Transform ballena;

    [Header("Clips")]
    public AudioClip[] ambienteProfundo;
    public AudioClip[] ambienteAscenso;
    public AudioClip[] transformacionMantarraya;
    public AudioClip[] movimientoMantarraya;
    public AudioClip[] entradaBallena;
    public AudioClip[] vortice;
    public AudioClip[] luzFinal;

    [Header("Mix")]
    [Range(0f, 1f)] public float volumenAmbiente = 0.42f;
    [Range(0f, 1f)] public float volumenMovimiento = 0.32f;
    [Range(0f, 1f)] public float volumenSfx = 0.72f;
    public float alturaBallenaActiva = 12.5f;
    public float alturaVortice = 24f;
    public float alturaLuzFinal = 70f;

    private AudioSource ambienteSource;
    private AudioSource movimientoSource;
    private AudioSource sfxSource;
    private bool transformacionReproducida;
    private bool ballenaReproducida;
    private bool vorticeReproducido;
    private bool luzFinalReproducida;

    private void Awake()
    {
        ambienteSource = CreateSource("Ambiente", volumenAmbiente, true);
        movimientoSource = CreateSource("Movimiento", volumenMovimiento, true);
        sfxSource = CreateSource("SFX", volumenSfx, false);
    }

    private void Start()
    {
        PlayLoop(ambienteSource, Pick(ambienteProfundo), volumenAmbiente);
    }

    private void Update()
    {
        if (!transformacionReproducida && mantarraya != null && mantarraya.gameObject.activeInHierarchy)
        {
            transformacionReproducida = true;
            PlayOneShot(transformacionMantarraya);
            PlayLoop(ambienteSource, Pick(ambienteAscenso), volumenAmbiente);
            PlayLoop(movimientoSource, Pick(movimientoMantarraya), volumenMovimiento);
        }

        if (!ballenaReproducida && ballena != null && ballena.position.y >= alturaBallenaActiva)
        {
            ballenaReproducida = true;
            PlayOneShot(entradaBallena);
            PlayLoop(movimientoSource, Pick(entradaBallena), volumenMovimiento);
        }

        if (!vorticeReproducido && ballena != null && ballena.position.y >= alturaVortice)
        {
            vorticeReproducido = true;
            PlayOneShot(vortice);
        }

        if (!luzFinalReproducida && GetHighestPassengerY() >= alturaLuzFinal)
        {
            luzFinalReproducida = true;
            PlayOneShot(luzFinal);
            movimientoSource.Stop();
        }
    }

    private AudioSource CreateSource(string sourceName, float volume, bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.priority = sourceName == "SFX" ? 64 : 128;
        return source;
    }

    private void PlayLoop(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null) return;

        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        source.Play();
    }

    private void PlayOneShot(AudioClip[] clips)
    {
        AudioClip clip = Pick(clips);
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip, volumenSfx);
    }

    private static AudioClip Pick(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    private float GetHighestPassengerY()
    {
        float highest = float.MinValue;
        if (babosa != null) highest = Mathf.Max(highest, babosa.position.y);
        if (mantarraya != null) highest = Mathf.Max(highest, mantarraya.position.y);
        if (ballena != null) highest = Mathf.Max(highest, ballena.position.y);
        return highest;
    }
}
