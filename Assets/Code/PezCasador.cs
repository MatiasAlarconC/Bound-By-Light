using UnityEngine;

public class PezPatrulla : MonoBehaviour
{
    [Header("Patrulla")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;
    [SerializeField] private float velocidadPatrulla = 2f;

    [Header("Persecución")]
    [SerializeField] private Transform babosa;
    [SerializeField] private float velocidadPersecucion = 4f;
    [SerializeField] private float rangoDeteccion = 5f;

    [Header("Sonido")]
    [Tooltip("Suena en loop cuando la Babosa o el Angel están dentro del radio de detección")]
    [SerializeField] private AudioClip sonidoPez;
    [Tooltip("Radio en el que se activa el sonido (puede ser distinto al de persecución)")]
    [SerializeField] private float radioSonido = 8f;
    private AudioSource audioSource;
    private Transform angelTransform;

    private Transform objetivoPatrulla;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    void Start()
    {
        objetivoPatrulla = puntoB;

        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager    = FindFirstObjectByType<GameManager>();

        if (babosa == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null) babosa = jugador.transform;
        }

        PulpoColumpio angel = FindFirstObjectByType<PulpoColumpio>();
        if (angel != null) angelTransform = angel.transform;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip        = sonidoPez;
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (babosa == null) return;

        float distancia = Vector2.Distance(
            transform.position,
            babosa.position
        );

        if (distancia <= rangoDeteccion)
            Perseguir();
        else
            Patrullar();

        // Sonido: activar si Babosa o Angel están dentro del radioSonido
        if (sonidoPez != null)
        {
            bool cerca = (babosa        != null && Vector2.Distance(transform.position, babosa.position)        <= radioSonido)
                      || (angelTransform != null && Vector2.Distance(transform.position, angelTransform.position) <= radioSonido);

            if (cerca && !audioSource.isPlaying) audioSource.Play();
            else if (!cerca && audioSource.isPlaying) audioSource.Stop();
        }
    }

    void Patrullar()
    {
        if (puntoA == null || puntoB == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            objetivoPatrulla.position,
            velocidadPatrulla * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, objetivoPatrulla.position) < 0.1f)
        {
            objetivoPatrulla = (objetivoPatrulla == puntoA)
                ? puntoB
                : puntoA;
        }

        Girar(objetivoPatrulla.position.x);
    }

    void Perseguir()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            babosa.position,
            velocidadPersecucion * Time.deltaTime
        );

        Girar(babosa.position.x);
    }

    void Girar(float objetivoX)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.flipX = objetivoX > transform.position.x;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") ||
            other.CompareTag("Babosa") ||
            other.CompareTag("Pulpo"))
        {
            if (gameManager != null)
            {
                gameManager.MuerteYRespawnCooperativo();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (puntoA != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(puntoA.position, 0.2f);
        }

        if (puntoB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(puntoB.position, 0.2f);
        }

        if (puntoA != null && puntoB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                puntoA.position,
                puntoB.position
            );
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Radio de sonido (naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radioSonido);
    }
}