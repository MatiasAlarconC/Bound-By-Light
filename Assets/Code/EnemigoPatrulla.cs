using UnityEngine;

public class EnemigoPatrulla : MonoBehaviour
{
    [Header("Puntos de Patrulla")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float distanciaCambio = 0.2f;

    [Header("Sonido")]
    [Tooltip("Sonido que suena en loop cuando un personaje entra al radio de detección")]
    [SerializeField] private AudioClip sonidoPatrulla;
    [Tooltip("Radio de detección: si la Babosa o el Angel están dentro, el sonido se activa")]
    [SerializeField] private float radioDeteccion = 8f;
    private AudioSource audioSource;
    private Transform babosaTransform;
    private Transform angelTransform;

    private Transform objetivoActual;

    void Start()
    {
        if (puntoA == null || puntoB == null)
        {
            Debug.LogWarning("EnemigoPatrulla: PuntoA o PuntoB sin asignar en " + gameObject.name + " — el enemigo no patrullará.");
            enabled = false;
            return;
        }

        objetivoActual = puntoB;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip        = sonidoPatrulla;
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        BabosaControl babosa = FindFirstObjectByType<BabosaControl>();
        if (babosa != null) babosaTransform = babosa.transform;

        PulpoColumpio angel = FindFirstObjectByType<PulpoColumpio>();
        if (angel != null) angelTransform = angel.transform;
    }

    void Update()
    {
        // Movimiento de patrulla
        transform.position = Vector2.MoveTowards(
            transform.position,
            objetivoActual.position,
            velocidad * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, objetivoActual.position) <= distanciaCambio)
            objetivoActual = (objetivoActual == puntoA) ? puntoB : puntoA;

        // Sonido por radio
        if (sonidoPatrulla != null)
        {
            bool cerca = (babosaTransform != null && Vector2.Distance(transform.position, babosaTransform.position) <= radioDeteccion)
                      || (angelTransform  != null && Vector2.Distance(transform.position, angelTransform.position)  <= radioDeteccion);

            if (cerca && !audioSource.isPlaying)
                audioSource.Play();
            else if (!cerca && audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    private void OnDrawGizmos()
    {
        if (puntoA != null && puntoB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(puntoA.position, 0.25f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(puntoB.position, 0.25f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(puntoA.position, puntoB.position);
        }

        // Radio de detección (naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}