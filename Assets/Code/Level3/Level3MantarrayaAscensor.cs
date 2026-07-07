using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Level 3 mechanic: Brother Light transforms into a manta ray and lifts the babosa.
/// The ascent starts when the babosa is standing on the manta ray.
/// </summary>
public class Level3MantarrayaAscensor : MonoBehaviour
{
    [Header("References")]
    public Transform babosa;
    public GameObject hermanoLuzBase;
    public Transform mantarraya;
    public Level3RideableSurface rideableSurface;
    public Level3BallenaAscenso ballenaSiguiente;

    [Header("Activation")]
    public Transform puntoTransformacion;
    public KeyCode teclaTransformar = KeyCode.C;
    public float radioActivacion = 2.5f;
    public bool iniciarOculta = true;
    public bool requiereBabosaEncima = true;
    public float radioMontajeAutomatico = 4f;
    public bool permitirBabosaComoReferencia = true;
    public bool bloquearBabosaEnMantarraya = true;

    [Header("Ascent")]
    public Transform[] puntosAscenso;
    public float velocidadAscenso = 3.2f;
    public float distanciaCambioPunto = 0.08f;

    [Header("Events")]
    public UnityEvent onTransformada;
    public UnityEvent onAscensoCompleto;

    private int indicePunto;
    private bool transformada;
    private bool ascendiendo;
    private bool completada;

    private void Awake()
    {
        if (mantarraya != null && iniciarOculta)
        {
            mantarraya.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!transformada && Input.GetKeyDown(teclaTransformar) && EstaCercaDelPunto())
        {
            ActivarTransformacion();
        }

        if (transformada && !ascendiendo && !completada)
        {
            if (!requiereBabosaEncima || rideableSurface == null || rideableSurface.HasRider)
            {
                ascendiendo = true;
            }
        }

        if (ascendiendo)
        {
            MantenerBabosaMontada();
            MoverPorRuta();
        }
    }

    private bool EstaCercaDelPunto()
    {
        if (puntoTransformacion == null) return true;

        Transform referencia = hermanoLuzBase != null ? hermanoLuzBase.transform : transform;
        bool hermanoCerca = Vector2.Distance(referencia.position, puntoTransformacion.position) <= radioActivacion;
        bool babosaCerca = permitirBabosaComoReferencia
            && babosa != null
            && Vector2.Distance(babosa.position, puntoTransformacion.position) <= radioActivacion;

        return hermanoCerca || babosaCerca;
    }

    public void ActivarTransformacion()
    {
        if (transformada) return;

        transformada = true;
        indicePunto = 0;

        if (hermanoLuzBase != null) hermanoLuzBase.SetActive(false);
        if (mantarraya != null)
        {
            mantarraya.gameObject.SetActive(true);
            if (puntoTransformacion != null)
            {
                mantarraya.position = puntoTransformacion.position;
            }
        }

        if (babosa != null && rideableSurface != null && EstaBabosaCercaDeMantarraya())
        {
            MontarBabosaEnMantarraya();
        }

        onTransformada?.Invoke();
    }

    private bool EstaBabosaCercaDeMantarraya()
    {
        if (babosa == null || mantarraya == null) return false;
        return Vector2.Distance(babosa.position, mantarraya.position) <= radioMontajeAutomatico;
    }

    private void MoverPorRuta()
    {
        if (mantarraya == null || puntosAscenso == null || puntosAscenso.Length == 0)
        {
            CompletarAscenso();
            return;
        }

        Transform destino = puntosAscenso[Mathf.Clamp(indicePunto, 0, puntosAscenso.Length - 1)];
        mantarraya.position = Vector3.MoveTowards(
            mantarraya.position,
            destino.position,
            velocidadAscenso * Time.deltaTime
        );

        if (Vector2.Distance(mantarraya.position, destino.position) <= distanciaCambioPunto)
        {
            indicePunto++;
            if (indicePunto >= puntosAscenso.Length)
            {
                CompletarAscenso();
            }
        }
    }

    private void MantenerBabosaMontada()
    {
        if (!bloquearBabosaEnMantarraya || babosa == null || rideableSurface == null) return;

        rideableSurface.ForceAttachAndLock(babosa);
    }

    private void MontarBabosaEnMantarraya()
    {
        if (babosa == null || rideableSurface == null) return;

        if (bloquearBabosaEnMantarraya)
        {
            rideableSurface.ForceAttachAndLock(babosa);
            return;
        }

        rideableSurface.ForceAttach(babosa);
    }

    private void CompletarAscenso()
    {
        if (completada) return;
        ascendiendo = false;
        completada = true;
        if (ballenaSiguiente != null)
        {
            ballenaSiguiente.ActivarBallena();
        }
        onAscensoCompleto?.Invoke();
    }
}
