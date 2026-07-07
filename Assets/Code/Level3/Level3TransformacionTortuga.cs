using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Final Level 3 transformation: after the vortex, the manta ray becomes a turtle
/// so the babosa and Brother Light can reach the surface together.
/// </summary>
public class Level3TransformacionTortuga : MonoBehaviour
{
    [Header("References")]
    public Transform babosa;
    public GameObject mantarraya;
    public GameObject tortuga;
    public Level3RideableSurface turtleRideableSurface;

    [Header("Exit Route")]
    public Transform[] puntosSalida;
    public float velocidadSalida = 2.8f;
    public float distanciaCambioPunto = 0.08f;
    public bool activarAlEntrar = true;
    public bool ocultarTortugaAlInicio = true;

    [Header("Events")]
    public UnityEvent onTransformacionTortuga;
    public UnityEvent onSalidaCompleta;

    private int indicePunto;
    private bool activa;

    private void Awake()
    {
        if (tortuga != null) tortuga.SetActive(!ocultarTortugaAlInicio);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activarAlEntrar || activa) return;
        if (other.CompareTag("Player") || other.CompareTag("Pulpo") || other.CompareTag("Babosa"))
        {
            ActivarTortuga();
        }
    }

    public void ActivarTortuga()
    {
        if (activa) return;

        activa = true;
        indicePunto = 0;

        Vector3 posicionInicial = mantarraya != null ? mantarraya.transform.position : transform.position;
        if (mantarraya != null) mantarraya.SetActive(false);

        if (tortuga != null)
        {
            tortuga.SetActive(true);
            tortuga.transform.position = posicionInicial;
        }

        if (turtleRideableSurface != null)
        {
            turtleRideableSurface.ForceAttach(babosa);
        }
        else if (babosa != null && tortuga != null)
        {
            babosa.SetParent(tortuga.transform, true);
        }

        onTransformacionTortuga?.Invoke();
    }

    private void Update()
    {
        if (!activa || tortuga == null || puntosSalida == null || puntosSalida.Length == 0) return;

        Transform destino = puntosSalida[Mathf.Clamp(indicePunto, 0, puntosSalida.Length - 1)];
        tortuga.transform.position = Vector3.MoveTowards(
            tortuga.transform.position,
            destino.position,
            velocidadSalida * Time.deltaTime
        );

        if (Vector2.Distance(tortuga.transform.position, destino.position) <= distanciaCambioPunto)
        {
            indicePunto++;
            if (indicePunto >= puntosSalida.Length)
            {
                activa = false;
                onSalidaCompleta?.Invoke();
            }
        }
    }
}
