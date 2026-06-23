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

    private Transform objetivoPatrulla;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    void Start()
    {
        objetivoPatrulla = puntoB;

        spriteRenderer = GetComponent<SpriteRenderer>();

        gameManager = FindFirstObjectByType<GameManager>();

        if (babosa == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");

            if (jugador != null)
            {
                babosa = jugador.transform;
            }
        }
    }

    void Update()
    {
        if (babosa == null) return;

        float distancia = Vector2.Distance(
            transform.position,
            babosa.position
        );

        if (distancia <= rangoDeteccion)
        {
            Perseguir();
        }
        else
        {
            Patrullar();
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
        Gizmos.DrawWireSphere(
            transform.position,
            rangoDeteccion
        );
    }
}