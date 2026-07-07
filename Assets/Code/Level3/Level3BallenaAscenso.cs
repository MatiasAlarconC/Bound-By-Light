using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Moves the whale through the vortex after the babosa and manta ray reach it.
/// The final light/surface section is revealed only after the vortex point.
/// </summary>
public class Level3BallenaAscenso : MonoBehaviour
{
    [Header("References")]
    public Transform ballena;
    public Transform babosa;
    public Transform mantarraya;
    public Level3RideableSurface whaleRideableSurface;
    public Level3TransformacionTortuga tortugaAlFinal;
    public Level3MantarrayaAscensor controladorMantarraya;
    public Vector2 babosaOffsetSobreBallena = new Vector2(-0.4f, 1.55f);
    public Vector2 mantarrayaOffsetSobreBallena = new Vector2(1.25f, 1.35f);
    public bool activarTortugaAlCompletarRuta = true;
    public bool activarPorProximidad = true;
    public float radioActivacionPorProximidad = 5.5f;
    public bool activarPorAlturaDeEncuentro = true;
    public float alturaEncuentroBallena = 7.2f;
    public bool mantenerPasajerosMontados = true;
    public KeyCode teclaDebugActivar = KeyCode.B;

    [Header("Route")]
    public Transform[] puntosRuta;
    public int indicePuntoDespuesDelVortice = 1;
    public float velocidad = 2.6f;
    public float distanciaCambioPunto = 0.1f;

    [Header("Preview Motion")]
    public bool movimientoIdleAntesDeActivar = true;
    public float amplitudIdle = 0.18f;
    public float amplitudHorizontalIdle = 0.15f;
    public float velocidadIdle = 1.4f;

    [Header("Reveal After Vortex")]
    public GameObject[] objetosOcultosHastaPasarVortice;

    [Header("Events")]
    public UnityEvent onBallenaActivada;
    public UnityEvent onVorticeSuperado;
    public UnityEvent onRutaCompleta;

    private int indicePunto;
    private bool activa;
    private bool vorticeSuperado;
    private Vector3 posicionBaseIdle;

    private void Awake()
    {
        if (ballena == null) ballena = transform;
        posicionBaseIdle = ballena != null ? ballena.position : transform.position;
        SetPostVortexObjects(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activa) return;
        if (other.CompareTag("Player") || other.CompareTag("Pulpo") || other.CompareTag("Babosa"))
        {
            ActivarBallena();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (activa) return;
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Pulpo") || collision.collider.CompareTag("Babosa"))
        {
            ActivarBallena();
        }
    }

    public void ActivarBallena()
    {
        if (activa) return;

        activa = true;
        indicePunto = 0;

        if (controladorMantarraya != null)
        {
            controladorMantarraya.enabled = false;
        }

        MontarPasajerosEnBallena();

        onBallenaActivada?.Invoke();
    }

    private void Update()
    {
        if (ballena == null) return;

        if (!activa)
        {
            if (movimientoIdleAntesDeActivar)
            {
                ballena.position = posicionBaseIdle + new Vector3(
                    Mathf.Cos(Time.time * velocidadIdle * 0.7f) * amplitudHorizontalIdle,
                    Mathf.Sin(Time.time * velocidadIdle) * amplitudIdle,
                    0f
                );
            }

            if (Input.GetKeyDown(teclaDebugActivar)
                || (activarPorProximidad && PasajerosCercaDeBallena())
                || (activarPorAlturaDeEncuentro && PasajerosEnAlturaDeEncuentro()))
            {
                ActivarBallena();
            }

            return;
        }

        if (puntosRuta == null || puntosRuta.Length == 0) return;

        Transform destino = puntosRuta[Mathf.Clamp(indicePunto, 0, puntosRuta.Length - 1)];
        ballena.position = Vector3.MoveTowards(ballena.position, destino.position, velocidad * Time.deltaTime);
        if (mantenerPasajerosMontados)
        {
            MontarPasajerosEnBallena();
        }

        if (Vector2.Distance(ballena.position, destino.position) <= distanciaCambioPunto)
        {
            indicePunto++;

            if (!vorticeSuperado && indicePunto >= indicePuntoDespuesDelVortice)
            {
                vorticeSuperado = true;
                SetPostVortexObjects(true);
                onVorticeSuperado?.Invoke();
            }

            if (indicePunto >= puntosRuta.Length)
            {
                activa = false;
                if (activarTortugaAlCompletarRuta && tortugaAlFinal != null)
                {
                    tortugaAlFinal.ActivarTortuga();
                }
                onRutaCompleta?.Invoke();
            }
        }
    }

    private bool PasajerosCercaDeBallena()
    {
        if (ballena == null) return false;

        bool babosaCerca = babosa != null
            && Vector2.Distance(babosa.position, ballena.position) <= radioActivacionPorProximidad;
        bool mantarrayaCerca = mantarraya != null
            && mantarraya.gameObject.activeInHierarchy
            && Vector2.Distance(mantarraya.position, ballena.position) <= radioActivacionPorProximidad;

        return babosaCerca || mantarrayaCerca;
    }

    private bool PasajerosEnAlturaDeEncuentro()
    {
        bool babosaArriba = babosa != null && babosa.position.y >= alturaEncuentroBallena;
        bool mantarrayaArriba = mantarraya != null
            && mantarraya.gameObject.activeInHierarchy
            && mantarraya.position.y >= alturaEncuentroBallena;

        return babosaArriba || mantarrayaArriba;
    }

    private void MontarPasajerosEnBallena()
    {
        if (ballena == null) return;

        if (whaleRideableSurface != null && babosa != null)
        {
            whaleRideableSurface.ForceAttach(babosa);
        }
        else if (babosa != null)
        {
            babosa.position = ballena.position + new Vector3(babosaOffsetSobreBallena.x, babosaOffsetSobreBallena.y, 0f);
            babosa.SetParent(ballena, true);
        }

        SnapPassengerPhysics(babosa);

        if (mantarraya != null)
        {
            mantarraya.position = ballena.position + new Vector3(mantarrayaOffsetSobreBallena.x, mantarrayaOffsetSobreBallena.y, 0f);
            mantarraya.SetParent(ballena, true);
            SnapPassengerPhysics(mantarraya);
        }
    }

    private void SnapPassengerPhysics(Transform passenger)
    {
        if (passenger == null) return;

        Rigidbody2D rb = passenger.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void SetPostVortexObjects(bool active)
    {
        if (objetosOcultosHastaPasarVortice == null) return;

        foreach (GameObject obj in objetosOcultosHastaPasarVortice)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
}
