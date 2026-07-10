using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

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
    [Tooltip("Arrastra aquí todos los sonidos de muerte de la Babosa")]
    [SerializeField] private AudioClip[] sonidosMuerteBabosa;

    [Tooltip("Arrastra aquí todos los sonidos de muerte del Pulpo")]
    [SerializeField] private AudioClip[] sonidosMuertePulpo;

    [Tooltip("Arrastra aquí el sonido de enganche entre la Babosa y el Pulpo")]
    [SerializeField] private AudioClip sonidoEnganchePersonajes;

    [Tooltip("Arrastra aquí el sonido que sonará al activar un Checkpoint")]
    [SerializeField] private AudioClip sonidoCheckpoint;

    private AudioSource miLectorDeAudio;

    [Header("UI de Teclas de Personajes")]
    [SerializeField] private GameObject cartelTeclasBabosa;
    [SerializeField] private GameObject cartelTeclasPulpo;

    [Header("Sistema de Checkpoints")]
    private Vector3 puntoDeReaparicion;
    private bool controlandoAlPulpo = false;
    private float tiempoSiguienteMuerte = 0f;
    private float _switchCooldown = 0f;

    void Start()
    {
        controlandoAlPulpo = false;
        ActualizarControlesEstrictos();

        if (hermanoMenorBabosa != null)
        {
            string thisScene  = SceneManager.GetActiveScene().name;
            string savedScene = PlayerPrefs.GetString("LastScene", "");
            float z = hermanoMenorBabosa.transform.position.z;

            bool hasExitPos = savedScene == thisScene
                && PlayerPrefs.GetInt("HasExitPos", 0) == 1;

            bool hasCheckpoint = savedScene == thisScene
                && PlayerPrefs.HasKey("CheckpointX")
                && PlayerPrefs.HasKey("CheckpointY");

            if (hasExitPos)
            {
                float ex = PlayerPrefs.GetFloat("ExitPosX");
                float ey = PlayerPrefs.GetFloat("ExitPosY");
                puntoDeReaparicion = new Vector3(ex, ey, z);
                hermanoMenorBabosa.transform.position = puntoDeReaparicion;
                if (hermanoMayorPulpo != null)
                    hermanoMayorPulpo.transform.position = puntoDeReaparicion + new Vector3(1.5f, 0f, 0f);
                PlayerPrefs.DeleteKey("HasExitPos");
                PlayerPrefs.Save();
            }
            else if (hasCheckpoint)
            {
                float cx = PlayerPrefs.GetFloat("CheckpointX");
                float cy = PlayerPrefs.GetFloat("CheckpointY");
                puntoDeReaparicion = new Vector3(cx, cy, z);
                hermanoMenorBabosa.transform.position = puntoDeReaparicion;
                if (hermanoMayorPulpo != null)
                    hermanoMayorPulpo.transform.position = puntoDeReaparicion + new Vector3(1.5f, 0f, 0f);
            }
            else
            {
                puntoDeReaparicion = hermanoMenorBabosa.transform.position;
            }

            // Siempre limpiamos velocidad al cargar escena para evitar el boost
            hermanoMenorBabosa.ResetearEstado();
            if (hermanoMayorPulpo != null) hermanoMayorPulpo.ResetearEstado();
        }

        miLectorDeAudio = GetComponent<AudioSource>();
        if (miLectorDeAudio == null)
        {
            miLectorDeAudio = gameObject.AddComponent<AudioSource>();
        }

        ActivarCartelUI(false);
    }

    void Update()
    {
        if (Time.time > _switchCooldown &&
            (Input.GetKeyDown(teclaCambio) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
        {
            _switchCooldown = Time.time + 0.15f;
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
            if (hermanoMenorBabosa != null) hermanoMenorBabosa.SetControlActivo(false);
            if (hermanoMayorPulpo != null)
            {
                hermanoMayorPulpo.SetControlActivo(true);
                hermanoMayorPulpo.BloquearInput(0.12f);
            }
            Debug.Log("<color=cyan>--- CONTROL: PULPO ACTIVO ---</color>");
        }
        else
        {
            if (hermanoMayorPulpo != null) hermanoMayorPulpo.SetControlActivo(false);
            if (hermanoMenorBabosa != null)
            {
                hermanoMenorBabosa.SetControlActivo(true);
                hermanoMenorBabosa.BloquearInput(0.12f);
            }
            Debug.Log("<color=green>--- CONTROL: BABOSA ACTIVA ---</color>");
        }

        // Actualizar outline en ambos personajes
        if (hermanoMenorBabosa != null) hermanoMenorBabosa.SetOutlineActivo(!controlandoAlPulpo);
        if (hermanoMayorPulpo != null) hermanoMayorPulpo.SetOutlineActivo(controlandoAlPulpo);

        if (hudUI != null)
        {
            hudUI.ActualizarIndicador(controlandoAlPulpo);
        }

        ActivarCartelUI(controlandoAlPulpo);
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

    public void ReproducirSonidoEnganche()
    {
        if (miLectorDeAudio != null && sonidoEnganchePersonajes != null)
        {
            miLectorDeAudio.PlayOneShot(sonidoEnganchePersonajes);
        }
    }

    public void GuardarNuevoCheckpoint(Vector3 nuevaPosicion, Animator animatorDelCheckpoint = null)
    {
        if (controlandoAlPulpo)
        {
            Debug.Log("<color=orange>¡Checkpoint ignorado! El Pulpo no puede activarlo.</color>");
            return;
        }

        if (puntoDeReaparicion != nuevaPosicion)
        {
            puntoDeReaparicion = nuevaPosicion;
            PlayerPrefs.SetFloat("CheckpointX", nuevaPosicion.x);
            PlayerPrefs.SetFloat("CheckpointY", nuevaPosicion.y);
            PlayerPrefs.Save();

            if (miLectorDeAudio != null && sonidoCheckpoint != null)
            {
                miLectorDeAudio.PlayOneShot(sonidoCheckpoint);
                Debug.Log("<color=yellow>¡Checkpoint guardado y SFX reproducido!</color>");
            }

            if (animatorDelCheckpoint != null)
            {
                animatorDelCheckpoint.SetBool("activado", true);
                Debug.Log("<color=green>¡Burbuja activada con éxito!</color>");
            }
        }
    }

    public void MuerteYRespawnCooperativo()
    {
        if (Time.time < tiempoSiguienteMuerte)
        {
            return;
        }

        tiempoSiguienteMuerte = Time.time + 0.4f;

        Debug.Log("<color=red>¡Muerte detectada! Reproduciendo sonido aleatorio y reapareciendo...</color>");

        if (miLectorDeAudio != null)
        {
            if (controlandoAlPulpo && sonidosMuertePulpo != null && sonidosMuertePulpo.Length > 0)
            {
                int indiceAleatorio = Random.Range(0, sonidosMuertePulpo.Length);
                miLectorDeAudio.PlayOneShot(sonidosMuertePulpo[indiceAleatorio]);
            }
            else if (!controlandoAlPulpo && sonidosMuerteBabosa != null && sonidosMuerteBabosa.Length > 0)
            {
                int indiceAleatorio = Random.Range(0, sonidosMuerteBabosa.Length);
                miLectorDeAudio.PlayOneShot(sonidosMuerteBabosa[indiceAleatorio]);
            }
        }

        if (hermanoMenorBabosa != null)
        {
            hermanoMenorBabosa.transform.position = puntoDeReaparicion;
            hermanoMenorBabosa.ResetearEstado();
        }
        if (hermanoMayorPulpo != null)
        {
            hermanoMayorPulpo.transform.position = puntoDeReaparicion + new Vector3(1.5f, 0f, 0f);
            hermanoMayorPulpo.ResetearEstado();
        }

        ForzarSaltoDeCamara();
    }

    private void FrenarRigidbody(GameObject objeto)
    {
        Rigidbody2D rb = objeto.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void ActivarCartelUI(bool esPulpo)
    {
        if (cartelTeclasBabosa != null && cartelTeclasPulpo != null)
        {
            cartelTeclasPulpo.SetActive(esPulpo);
            cartelTeclasBabosa.SetActive(!esPulpo);
        }
    }
}
