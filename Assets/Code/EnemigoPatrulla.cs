using UnityEngine;

public class EnemigoPatrulla : MonoBehaviour
{
    [Header("Puntos de Patrulla")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float distanciaCambio = 0.2f;

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
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            objetivoActual.position,
            velocidad * Time.deltaTime
        );

        float distancia = Vector2.Distance(
            transform.position,
            objetivoActual.position
        );

        if (distancia <= distanciaCambio)
        {
            if (objetivoActual == puntoA)
            {
                objetivoActual = puntoB;
            }
            else
            {
                objetivoActual = puntoA;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (puntoA == null || puntoB == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(puntoA.position, 0.25f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(puntoB.position, 0.25f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(puntoA.position, puntoB.position);
    }
}