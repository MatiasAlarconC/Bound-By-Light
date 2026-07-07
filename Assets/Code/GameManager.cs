using UnityEngine;
using Unity.Cinemachine; // Recuerda cambiar a 'using Cinemachine;' si usas versión antigua de Unity

public class GameManager : MonoBehaviour
{
    [Header("Personajes")]
    [SerializeField] private BabosaControl hermanoMenorBabosa;
    [SerializeField] private PulpoColumpio hermanoMayorPulpo;

    [Header("Configuración de Control")]
    [SerializeField] private KeyCode teclaCambio = KeyCode.C;

    [Header("Interfaz de Usuario (HUD)")]
    [SerializeField] private HUDControlador hudUI; 

    [Header("Cerebro de Cinemachine")]
    [SerializeField] private CinemachineBrain cerebroCinemachine; 

    [Header("Efectos de Sonido (SFX)")]
    [Tooltip("Arrastra aquí el sonido de muerte de la Babosa")]
    [SerializeField] private AudioClip sonidoMuerteBabosa;

    [Tooltip("Arrastra aquí el sonido de muerte del Pulpo")]
    [SerializeField] private AudioClip sonidoMuertePulpo;

    [Tooltip("Arrastra aquí el sonido de enganche entre la Babosa y el Pulpo")]
    [SerializeField] private AudioClip sonidoEnganchePersonajes;

    // Este componente se encargará de reproducir todos los clips del GameManager
    private AudioSource miLectorDeAudio;

    [Header("Sistema de Checkpoints")]
    private Vector3 puntoDeReaparicion;

    private bool controlandoAlPulpo = false;

    void Start()
    {
        controlandoAlPulpo = false;
        ActualizarControlesEstrictos();

        if (hermanoMenorBabosa != null)
        {
            puntoDeReaparicion = hermanoMenorBabosa.transform.position;
        }

        // Agregamos u obtenemos el componente de audio al iniciar
        miLectorDeAudio = GetComponent<AudioSource>();
        if (miLectorDeAudio == null)
        {
            miLectorDeAudio = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaCambio))
        {
            controlandoAlPulpo = !controlandoAlPulpo;
            ActualizarControlesEstrictos();
            ActualizarCuartoActual();
            ForzarSaltoDeCamara();
        }
    }

    void ActualizarControlesEstrictos()
    {
        if (controlandoAlPulpo)
        {
            if (hermanoMayorPulpo != null) hermanoMayorPulpo.SetControlActivo(true);
            if (hermanoMenorBabosa != null) hermanoMenorBabosa.SetControlActivo(false);
            Debug.Log("<color=cyan>--- CONTROL: PULPO ACTIVO ---</color>");
        }
        else
        {
            if (hermanoMayorPulpo != null) hermanoMayorPulpo.SetControlActivo(false);
            if (hermanoMenorBabosa != null) hermanoMenorBabosa.SetControlActivo(true);
            Debug.Log("<color=green>--- CONTROL: BABOSA ACTIVA ---</color>");
        }

        if (hudUI != null)
        {
            hudUI.ActualizarIndicador(controlandoAlPulpo);
        }
    }

    void ActualizarCuartoActual()
    {
        Vector2 posicionPersonajeActual = controlandoAlPulpo ? 
            (Vector2)hermanoMayorPulpo.transform.position : 
            (Vector2)hermanoMenorBabosa.transform.position;

        Room[] todosLosCuartos = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (Room cuarto in todosLosCuartos)
        {
            cuarto.DesactivarCamaraManualmente();
        }

        Collider2D[] colisionadoresEncontrados = Physics2D.OverlapCircleAll(posicionPersonajeActual, 0.6f);

        bool cuartoEncontrado = false;
        foreach (Collider2D col in colisionadoresEncontrados)
        {
            Room cuartoActual = col.GetComponent<Room>();
            if (cuartoActual != null)
            {
                cuartoActual.ActivarCamaraManualmente();
                cuartoEncontrado = true;
                break; 
            }
        }

        if (!cuartoEncontrado)
        {
            foreach (Room cuarto in todosLosCuartos)
            {
                Collider2D colCuarto = cuarto.GetComponent<Collider2D>();
                if (colCuarto != null && colCuarto.OverlapPoint(posicionPersonajeActual))
                {
                    cuarto.ActivarCamaraManualmente();
                    break;
                }
            }
        }
    }

    void ForzarSaltoDeCamara()
    {
        if (cerebroCinemachine != null)
        {
            ICinemachineCamera camaraGenerica = cerebroCinemachine.ActiveVirtualCamera;
            CinemachineCamera camaraActiva = camaraGenerica as CinemachineCamera;
            
            if (camaraActiva != null)
            {
                camaraActiva.ForceCameraPosition(camaraActiva.State.RawPosition, camaraActiva.State.RawOrientation);
            }
        }
    }

    // ====================================================================
    // NUEVO: FUNCIÓN PÚBLICA PARA REPRODUCIR EL ENGANCHE
    // ====================================================================
    public void ReproducirSonidoEnganche()
    {
        if (miLectorDeAudio != null && sonidoEnganchePersonajes != null)
        {
            miLectorDeAudio.PlayOneShot(sonidoEnganchePersonajes);
            Debug.Log("<color=magenta>¡SFX: Sonido de enganche reproducido!</color>");
        }
    }

    // --- FUNCIONES DE CHECKPOINTS Y MUERTE ---
    public void GuardarNuevoCheckpoint(Vector3 nuevaPosicion)
    {
        puntoDeReaparicion = nuevaPosicion;
    }

    public void MuerteYRespawnCooperativo()
    {
        Debug.Log("<color=red>¡Muerte detectada! Reproduciendo sonido y reapareciendo...</color>");

        if (miLectorDeAudio != null)
        {
            if (controlandoAlPulpo && sonidoMuertePulpo != null)
            {
                miLectorDeAudio.PlayOneShot(sonidoMuertePulpo);
            }
            else if (!controlandoAlPulpo && sonidoMuerteBabosa != null)
            {
                miLectorDeAudio.PlayOneShot(sonidoMuerteBabosa);
            }
        }

        if (hermanoMenorBabosa != null) hermanoMenorBabosa.transform.position = puntoDeReaparicion;
        if (hermanoMayorPulpo != null) hermanoMayorPulpo.transform.position = puntoDeReaparicion + new Vector3(1.5f, 0f, 0f);
        
        FrenarRigidbody(hermanoMenorBabosa.gameObject);
        FrenarRigidbody(hermanoMayorPulpo.gameObject);
        ForzarSaltoDeCamara();
    }

    private void FrenarRigidbody(GameObject objeto)
    {
        Rigidbody2D rb = objeto.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero; 
    }
}