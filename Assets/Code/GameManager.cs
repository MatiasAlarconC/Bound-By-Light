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

    [Header("Triángulos Indicadores de Control")]
    [Tooltip("Arrastra aquí el objeto del triángulo que está dentro de la Babosa")]
    [SerializeField] private GameObject trianguloIndicadorBabosa;
    [Tooltip("Arrastra aquí el objeto del triángulo que está dentro del Pulpo")]
    [SerializeField] private GameObject trianguloIndicadorPulpo;

    [Header("UI de Teclas de Personajes")]
    [SerializeField] private GameObject cartelTeclasBabosa;
    [SerializeField] private GameObject cartelTeclasPulpo;

    [Header("Sistema de Checkpoints")]
    private Vector3 puntoDeReaparicion;
    private bool controlandoAlPulpo  = false;
    private bool estaEnModoCombinado = false;
    private float tiempoSiguienteMuerte = 0f;
    private float _switchCooldown = 0f;

    // Sorting de personajes: el controlado se renderiza encima del otro
    private SpriteRenderer[] _rendsBabosa;
    private SpriteRenderer[] _rendsPulpo;
    private int[] _ordenOrigBabosa;
    private int[] _ordenOrigPulpo;
    private const int SORTING_BOOST = 10;

    void Start()
    {
        if (hudUI == null)
            hudUI = FindFirstObjectByType<HUDControlador>();

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
                {
                    float px = PlayerPrefs.GetFloat("ExitPosPulpoX", ex + 1.5f);
                    float py = PlayerPrefs.GetFloat("ExitPosPulpoY", ey);
                    hermanoMayorPulpo.transform.position = new Vector3(px, py, hermanoMayorPulpo.transform.position.z);
                }
                // Reanudar música desde donde se quedó
                if (PlayerPrefs.HasKey("MusicTimeSamples"))
                {
                    GameObject soundsGO = GameObject.Find("Sounds");
                    if (soundsGO != null)
                    {
                        AudioSource musicAudio = soundsGO.GetComponent<AudioSource>();
                        if (musicAudio != null)
                            musicAudio.timeSamples = PlayerPrefs.GetInt("MusicTimeSamples");
                    }
                    PlayerPrefs.DeleteKey("MusicTimeSamples");
                }
                PlayerPrefs.DeleteKey("HasExitPos");
                PlayerPrefs.DeleteKey("ExitPosPulpoX");
                PlayerPrefs.DeleteKey("ExitPosPulpoY");
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

        CrearTriangulosIndicadores();
        ActivarCartelUI(false);
        StartCoroutine(CachearSortingOrders());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IrAlMenuPrincipal();
            return;
        }

        // Modo combinado: no se permite cambiar de personaje
        if (estaEnModoCombinado) return;

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

        // Triángulos indicadores de control
        if (trianguloIndicadorBabosa != null) trianguloIndicadorBabosa.SetActive(!controlandoAlPulpo);
        if (trianguloIndicadorPulpo != null) trianguloIndicadorPulpo.SetActive(controlandoAlPulpo);

        if (hudUI != null)
        {
            hudUI.ActualizarIndicador(controlandoAlPulpo);
        }

        ActivarCartelUI(controlandoAlPulpo);
        ActualizarOrdenCapas();
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
            Debug.Log($"<color=cyan>CHECKPOINT GUARDADO en X={nuevaPosicion.x:F2}  Y={nuevaPosicion.y:F2}</color>");

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

        // Si estaban combinados, forzar separación antes de respawnear
        if (estaEnModoCombinado)
        {
            MantarrayaControl mc = FindFirstObjectByType<MantarrayaControl>();
            if (mc != null) mc.ForceDetach();
            estaEnModoCombinado = false;
            controlandoAlPulpo  = false;
            ActualizarControlesEstrictos();
        }

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
            Debug.Log($"<color=magenta>SPAWN BABOSA en X={puntoDeReaparicion.x:F2}  Y={puntoDeReaparicion.y:F2}</color>");
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

    // ── Sorting de personajes ─────────────────────────────────────────────────

    System.Collections.IEnumerator CachearSortingOrders()
    {
        yield return null; // esperar un frame para que todos los Start() creen sus renderers
        if (hermanoMenorBabosa != null)
        {
            _rendsBabosa     = hermanoMenorBabosa.GetComponentsInChildren<SpriteRenderer>(true);
            _ordenOrigBabosa = System.Array.ConvertAll(_rendsBabosa, r => r.sortingOrder);
        }
        if (hermanoMayorPulpo != null)
        {
            _rendsPulpo     = hermanoMayorPulpo.GetComponentsInChildren<SpriteRenderer>(true);
            _ordenOrigPulpo = System.Array.ConvertAll(_rendsPulpo, r => r.sortingOrder);
        }
        ActualizarOrdenCapas();
    }

    void ActualizarOrdenCapas()
    {
        if (estaEnModoCombinado) return;
        if (_rendsBabosa != null && _ordenOrigBabosa != null)
            for (int i = 0; i < _rendsBabosa.Length; i++)
                if (_rendsBabosa[i] != null)
                    _rendsBabosa[i].sortingOrder = _ordenOrigBabosa[i] + (controlandoAlPulpo ? 0 : SORTING_BOOST);
        if (_rendsPulpo != null && _ordenOrigPulpo != null)
            for (int i = 0; i < _rendsPulpo.Length; i++)
                if (_rendsPulpo[i] != null)
                    _rendsPulpo[i].sortingOrder = _ordenOrigPulpo[i] + (controlandoAlPulpo ? SORTING_BOOST : 0);
    }

    // ── Modo combinado (Babosa montada en Mantarraya) ─────────────────────────

    public void SetModoCombinado(bool combinado)
    {
        estaEnModoCombinado = combinado;
        GeiserControl.modoCombinadoActivo = combinado;

        // Actualizar todas las barreras de geisers inmediatamente
        foreach (var gc in FindObjectsByType<GeiserControl>(FindObjectsSortMode.None))
            gc.ActualizarBarreras();

        if (combinado)
        {
            // Forzar control al Pulpo/Mantarraya; Babosa queda pasiva
            controlandoAlPulpo = true;
            if (hermanoMenorBabosa != null) hermanoMenorBabosa.SetControlActivo(false);
            if (hermanoMayorPulpo  != null) hermanoMayorPulpo.SetControlActivo(true);

            // Ocultar triángulos individuales
            if (trianguloIndicadorBabosa != null) trianguloIndicadorBabosa.SetActive(false);
            if (trianguloIndicadorPulpo  != null) trianguloIndicadorPulpo.SetActive(false);

            // HUD: mostrar estado combinado
            if (hudUI != null) hudUI.MostrarCombinado();
        }
        else
        {
            // Volver a modo individual: Babosa activa
            controlandoAlPulpo = false;
            ActualizarControlesEstrictos();
            ActualizarCuartoActual();
            ForzarSaltoDeCamara();
        }
    }

    // ── Triángulo indicador ────────────────────────────────────────────────────

    void CrearTriangulosIndicadores()
    {
        if (hermanoMenorBabosa != null && trianguloIndicadorBabosa == null)
            trianguloIndicadorBabosa = CrearTriangulo("IndicadorBabosa", hermanoMenorBabosa.transform,
                                                       new Color(0f, 0.15f, 0.75f, 1f));   // azul oscuro

        if (hermanoMayorPulpo != null && trianguloIndicadorPulpo == null)
            trianguloIndicadorPulpo = CrearTriangulo("IndicadorPulpo", hermanoMayorPulpo.transform,
                                                      new Color(1f, 0.5f, 0.05f, 1f));   // naranja
    }

    GameObject CrearTriangulo(string nombre, Transform padre, Color color)
    {
        // Posición en world-space: 1 unidad por encima del personaje, sin heredar escala
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);

        // Compensar escala del padre para que el triángulo mida ~0.55 unidades en world space
        float sx = padre.lossyScale.x != 0f ? 0.55f / Mathf.Abs(padre.lossyScale.x) : 1f;
        float sy = padre.lossyScale.y != 0f ? 0.55f / Mathf.Abs(padre.lossyScale.y) : 1f;
        go.transform.localScale = new Vector3(sx, sy, 1f);

        // Posición: 1.9 unidades arriba en world-space
        float offsetY = padre.lossyScale.y != 0f ? 1.9f / Mathf.Abs(padre.lossyScale.y) : 2f;
        go.transform.localPosition = new Vector3(0f, offsetY, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CrearSpriteTrianguloInvertido(color);
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 10;

        return go;
    }

    Sprite CrearSpriteTrianguloInvertido(Color color)
    {
        int tam = 32;
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[tam * tam];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        // Triángulo invertido ▼: ancho arriba, punta abajo
        for (int y = 0; y < tam; y++)
        {
            float t = (float)y / (tam - 1);           // 0=abajo(punta) 1=arriba(base)
            int halfW = Mathf.RoundToInt(t * (tam / 2f));
            int cx = tam / 2;
            for (int x = cx - halfW; x <= cx + halfW; x++)
                if (x >= 0 && x < tam) pixels[y * tam + x] = color;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), tam);
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("SaveExists", 1);
        if (hermanoMenorBabosa != null)
        {
            PlayerPrefs.SetFloat("ExitPosX", hermanoMenorBabosa.transform.position.x);
            PlayerPrefs.SetFloat("ExitPosY", hermanoMenorBabosa.transform.position.y);
            PlayerPrefs.SetInt("HasExitPos", 1);
        }
        if (hermanoMayorPulpo != null)
        {
            PlayerPrefs.SetFloat("ExitPosPulpoX", hermanoMayorPulpo.transform.position.x);
            PlayerPrefs.SetFloat("ExitPosPulpoY", hermanoMayorPulpo.transform.position.y);
        }
        // Guardar posición exacta de la música para reanudarla al volver
        GameObject soundsGO = GameObject.Find("Sounds");
        if (soundsGO != null)
        {
            AudioSource musicAudio = soundsGO.GetComponent<AudioSource>();
            if (musicAudio != null && musicAudio.isPlaying)
                PlayerPrefs.SetInt("MusicTimeSamples", musicAudio.timeSamples);
        }
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main Menu");
    }
}
