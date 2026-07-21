using System.Collections;
using UnityEngine;

/// <summary>
/// Reproduce un audio de narrador una sola vez por partida.
/// Usa un token de sesión en PlayerPrefs: no requiere registro ni HashSet.
/// En nueva partida llama NarratorHint.NewSession() para incrementar el token.
/// </summary>
public class NarratorHint : MonoBehaviour
{
    public enum TriggerMode { OnSceneStart, OnProximity }

    [Header("Audio")]
    [SerializeField] private AudioClip clip;
    [Tooltip("Fuente de audio opcional. Si está vacío se crea una automáticamente.")]
    [SerializeField] private AudioSource audioSourceOverride;

    [Header("Trigger")]
    [SerializeField] private TriggerMode mode = TriggerMode.OnProximity;
    [Tooltip("Radio de detección (OnProximity). Visible como esfera amarilla en Scene.")]
    [SerializeField] private float triggerRadius = 4f;
    [Tooltip("Segundos de espera antes de reproducir el audio.")]
    [SerializeField] private float delay = 0f;

    [Header("Persistencia")]
    [Tooltip("Clave única. Debe ser diferente para cada pista.")]
    [SerializeField] private string uniqueKey = "hint_";

    private const string SESSION_KEY = "nrt_session";

    // ── API pública ──────────────────────────────────────────────────
    /// <summary>Llama esto en nueva partida. Incrementa el token → todos los hints se resetean.</summary>
    public static void NewSession()
    {
        int s = PlayerPrefs.GetInt(SESSION_KEY, 0) + 1;
        PlayerPrefs.SetInt(SESSION_KEY, s);
        PlayerPrefs.Save();
    }

    // ── Internos ─────────────────────────────────────────────────────
    private AudioSource _source;
    private bool _triggered;
    private Transform _playerA;  // Babosa
    private Transform _playerB;  // Pulpo / Angel

    private bool AlreadyPlayed()
    {
        int session = PlayerPrefs.GetInt(SESSION_KEY, 0);
        return PlayerPrefs.GetInt(uniqueKey, -1) == session;
    }

    private void MarkPlayed()
    {
        int session = PlayerPrefs.GetInt(SESSION_KEY, 0);
        PlayerPrefs.SetInt(uniqueKey, session);
        PlayerPrefs.Save();
    }

    void Start()
    {
        if (AlreadyPlayed()) { _triggered = true; return; }

        _source = audioSourceOverride;
        if (_source == null)
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake  = false;
            _source.spatialBlend = 0f;
            _source.loop         = false;
        }

        if (mode == TriggerMode.OnSceneStart)
        {
            TriggerHint();
            return;
        }

        var babosa = FindFirstObjectByType<BabosaControl>();
        var pulpo  = FindFirstObjectByType<PulpoColumpio>();
        if (babosa != null) _playerA = babosa.transform;
        if (pulpo  != null) _playerB = pulpo.transform;
    }

    void Update()
    {
        if (_triggered || mode != TriggerMode.OnProximity) return;
        if (_playerA == null && _playerB == null) return;

        float distA = _playerA != null
            ? Vector2.Distance(transform.position, _playerA.position) : float.MaxValue;
        float distB = _playerB != null
            ? Vector2.Distance(transform.position, _playerB.position) : float.MaxValue;

        if (Mathf.Min(distA, distB) <= triggerRadius)
            TriggerHint();
    }

    void TriggerHint()
    {
        if (_triggered) return;
        _triggered = true;
        MarkPlayed();
        if (clip != null) StartCoroutine(PlayDelayed());
    }

    IEnumerator PlayDelayed()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (_source != null && clip != null)
            _source.PlayOneShot(clip);
    }

    void OnDrawGizmos()
    {
        if (mode != TriggerMode.OnProximity) return;
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.2f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
