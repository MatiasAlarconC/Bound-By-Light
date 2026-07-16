using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GeiserControl : MonoBehaviour
{
    public static List<Collider2D> todasLasBarreras = new List<Collider2D>();
    public static bool modoCombinadoActivo = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LimpiarBarrerasAlIniciar() { todasLasBarreras.Clear(); modoCombinadoActivo = false; }

    [Header("Referencias (auto-detectadas si no se asignan)")]
    [SerializeField] private ParticleSystem sistemaParticulas;
    [Tooltip("Arrastra aquí el AreaEffector2D (WindEffect) que está sobre este geiser")]
    [SerializeField] private AreaEffector2D areaEffector;

    [Header("Audio")]
    [Tooltip("AudioSource del sonido del géiser. Si no se asigna, se busca automáticamente en este objeto.")]
    [SerializeField] private AudioSource audioSourceGeiser;

    [Header("Fuerza del impulso")]
    [Tooltip("Fuerza continua aplicada a la Babosa mientras está en el chorro (default = 15). " +
             "El largo del chorro de partículas escala con este valor.")]
    [SerializeField] private float fuerzaImpulso = 15f;

    [Header("Ciclo")]
    [SerializeField] private float tiempoActivo   = 5f;
    [SerializeField] private float tiempoInactivo = 5f;
    [SerializeField] private float desfase        = 0f;

    [Header("Barrera física (world-space, sin herencia de escala)")]
    [Tooltip("Ancho del canal de agua (ajustar para que coincida con el ancho del WindEffect)")]
    [SerializeField] private float anchoBarrera  = 5f;
    [Tooltip("Alto de la barrera (debe cubrir desde el suelo hasta arriba del chorro)")]
    [SerializeField] private float alturaBarrera = 25f;
    [Tooltip("Offset en WORLD SPACE desde el centro del geiser")]
    [SerializeField] private Vector2 offsetBarreraWorld = new Vector2(0f, 0f);

    private BoxCollider2D paredIzquierda;
    private BoxCollider2D paredDerecha;
    private GameObject barreraGO;
    private Collider2D effectorCollider;
    private BabosaControl babosaRef;
    private Rigidbody2D  rbBabosa;
    private PulpoColumpio angelRef;
    private Rigidbody2D  rbAngel;
    private GameManager  gmRef;
    private bool activo = true;
    private bool combinadoEnZona         = false;
    private bool combinadoEnZonaAnterior = false;
    private bool soloEnZona              = false;
    private float temporizador;
    private ParticleSystem sistemaParticulasCarga;
    private bool psCreatedByUs = false;   // true si creamos el PS en código (no venía del artista)

    void Awake()
    {
        // Destruir todos los scripts Geiser.cs viejos en este objeto y sus hijos
        Geiser[] viejos = GetComponentsInChildren<Geiser>(true);
        if (viejos.Length > 0) Debug.Log($"[{gameObject.name}] Destruyendo {viejos.Length} Geiser.cs viejo(s)");
        foreach (Geiser g in viejos) Destroy(g);

        if (sistemaParticulas == null)
            sistemaParticulas = GetComponentInChildren<ParticleSystem>();

        // Si ningún hijo tiene ParticleSystem (geisers sin PS pre-existente), crear uno
        if (sistemaParticulas == null)
        {
            GameObject goPS = new GameObject("Particle System");
            goPS.transform.SetParent(transform);
            goPS.transform.localPosition = Vector3.zero;
            sistemaParticulas = goPS.AddComponent<ParticleSystem>();
            psCreatedByUs = true;
        }

        if (areaEffector != null)
            effectorCollider = areaEffector.GetComponent<Collider2D>();

        if (audioSourceGeiser == null)
            audioSourceGeiser = GetComponent<AudioSource>();

        if (audioSourceGeiser != null)
        {
            audioSourceGeiser.playOnAwake = false;
            audioSourceGeiser.loop = true;
            audioSourceGeiser.Stop();
        }

        CrearBarreraWorldSpace();
        CrearSistemaParticulasCarga();
    }

    void CrearBarreraWorldSpace()
    {
        // Crear en world space para evitar herencia de escala del sprite del geiser
        Vector3 centroMundo = transform.position + new Vector3(offsetBarreraWorld.x, offsetBarreraWorld.y, 0f);
        float mitad   = anchoBarrera / 2f;
        float grosor  = 0.15f;

        barreraGO = new GameObject("BarreraGeiser");

        // Pared izquierda
        GameObject leftGO = new GameObject("ParedIzquierda");
        leftGO.transform.SetParent(barreraGO.transform);
        leftGO.transform.position = new Vector3(centroMundo.x - mitad, centroMundo.y, 0f);
        leftGO.transform.localScale = Vector3.one;
        leftGO.layer = 0;
        paredIzquierda = leftGO.AddComponent<BoxCollider2D>();
        paredIzquierda.size = new Vector2(grosor, alturaBarrera);

        // Pared derecha
        GameObject rightGO = new GameObject("ParedDerecha");
        rightGO.transform.SetParent(barreraGO.transform);
        rightGO.transform.position = new Vector3(centroMundo.x + mitad, centroMundo.y, 0f);
        rightGO.transform.localScale = Vector3.one;
        rightGO.layer = 0;
        paredDerecha = rightGO.AddComponent<BoxCollider2D>();
        paredDerecha.size = new Vector2(grosor, alturaBarrera);

        todasLasBarreras.Add(paredIzquierda);
        todasLasBarreras.Add(paredDerecha);
    }

    void OnDestroy()
    {
        if (paredIzquierda != null) todasLasBarreras.Remove(paredIzquierda);
        if (paredDerecha   != null) todasLasBarreras.Remove(paredDerecha);
        if (barreraGO      != null) Destroy(barreraGO);
    }

    void Start()
    {
        babosaRef = FindFirstObjectByType<BabosaControl>();
        if (babosaRef != null) rbBabosa = babosaRef.GetComponent<Rigidbody2D>();

        angelRef = FindFirstObjectByType<PulpoColumpio>();
        if (angelRef != null) rbAngel = angelRef.GetComponent<Rigidbody2D>();

        gmRef = FindFirstObjectByType<GameManager>();

        // Deshabilitar TODOS los AreaEffectors de la escena — la fuerza la maneja solo GeiserControl vía código
        foreach (AreaEffector2D ae in FindObjectsByType<AreaEffector2D>(FindObjectsSortMode.None))
        {
            ae.enabled = false;
            Collider2D col = ae.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // Reubicar barreras si el objeto fue movido después de Awake
        if (barreraGO != null)
        {
            Vector3 centro = transform.position + new Vector3(offsetBarreraWorld.x, offsetBarreraWorld.y, 0f);
            float mitad = anchoBarrera / 2f;
            if (paredIzquierda != null)
                paredIzquierda.transform.position = new Vector3(centro.x - mitad, centro.y, 0f);
            if (paredDerecha != null)
                paredDerecha.transform.position = new Vector3(centro.x + mitad, centro.y, 0f);
        }

        ConfigurarParticulasAgua();   // copia material artístico en el PS principal si es nuestro

        // Propagar ese mismo material al PS de carga (ahora el artístico ya está disponible)
        if (psCreatedByUs && sistemaParticulasCarga != null)
        {
            var rendPrincipal = sistemaParticulas?.GetComponent<ParticleSystemRenderer>();
            var rendCarga     = sistemaParticulasCarga.GetComponent<ParticleSystemRenderer>();
            if (rendPrincipal != null && rendCarga != null && rendPrincipal.sharedMaterial != null)
                rendCarga.sharedMaterial = rendPrincipal.sharedMaterial;
        }

        temporizador = tiempoActivo + desfase;
        ActualizarEstado(true);
    }

    void Update()
    {
        temporizador -= Time.deltaTime;
        if (temporizador <= 0f)
        {
            activo = !activo;
            ActualizarEstado(activo);
            temporizador = activo ? tiempoActivo : tiempoInactivo;
        }

        ActualizarSonidoPorCamara();

        combinadoEnZona = false;
        soloEnZona      = false;

        if (activo && babosaRef != null)
        {
            Vector2 centro  = (Vector2)transform.position + offsetBarreraWorld;
            float halfW = anchoBarrera / 2f;
            float halfH = alturaBarrera / 2f;

            if (babosaRef.EstaMontada && angelRef != null)
            {
                // COMBINADO: detectar posición de Angel → impulso seguro, sin flag de muerte
                Vector2 angelPos = (Vector2)angelRef.transform.position;
                bool dentroX = Mathf.Abs(angelPos.x - centro.x) <= halfW;
                bool dentroY = angelPos.y >= centro.y - halfH && angelPos.y <= centro.y + halfH;
                if (dentroX && dentroY) combinadoEnZona = true;
            }
            else if (!babosaRef.EstaMontada && !babosaRef.EstaColgada)
            {
                // BABOSA SOLA: si el geiser activo la atrapa → empujarla y marcar para muerte al aterrizar
                Vector2 babosaPos = (Vector2)babosaRef.transform.position;
                bool dentroX = Mathf.Abs(babosaPos.x - centro.x) <= halfW;
                bool dentroY = babosaPos.y >= centro.y - halfH && babosaPos.y <= centro.y + halfH;
                if (dentroX && dentroY)
                {
                    babosaRef.FueLanzadaPorGeiser = true;   // muere cuando aterrice
                    soloEnZona = true;
                }

                // ANGEL SOLO (Babosa no montada): detectar Angel aislado → muerte directa
                if (angelRef != null)
                {
                    Vector2 angelPos = (Vector2)angelRef.transform.position;
                    bool axIn = Mathf.Abs(angelPos.x - centro.x) <= halfW;
                    bool ayIn = angelPos.y >= centro.y - halfH && angelPos.y <= centro.y + halfH;
                    if (axIn && ayIn) gmRef?.MuerteYRespawnCooperativo();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!activo) return;

        // COMBINADO: impulsar Angel
        if (rbAngel != null)
        {
            bool acabaDeEntrar = combinadoEnZona && !combinadoEnZonaAnterior;
            if (acabaDeEntrar)
            {
                rbAngel.linearVelocity = new Vector2(rbAngel.linearVelocity.x, 0f);
                rbAngel.AddForce(Vector2.up * fuerzaImpulso, ForceMode2D.Impulse);
            }
            else if (combinadoEnZona)
            {
                rbAngel.AddForce(Vector2.up * fuerzaImpulso * 3f, ForceMode2D.Force);
            }
            combinadoEnZonaAnterior = combinadoEnZona;
        }

        // BABOSA SOLA: lanzarla hacia arriba (morirá al aterrizar via FueLanzadaPorGeiser)
        if (rbBabosa != null && soloEnZona)
        {
            rbBabosa.AddForce(Vector2.up * fuerzaImpulso * 2f, ForceMode2D.Force);
        }
    }

    void ActualizarSonidoPorCamara()
    {
        if (audioSourceGeiser == null || audioSourceGeiser.clip == null)
            return;

        bool debeSonar = activo && EstaGeiserDentroDeCamara();

        if (debeSonar)
        {
            if (!audioSourceGeiser.isPlaying)
                audioSourceGeiser.Play();
        }
        else
        {
            if (audioSourceGeiser.isPlaying)
                audioSourceGeiser.Stop();
        }
    }

    bool EstaGeiserDentroDeCamara()
    {
        Camera camara = Camera.main;
        if (camara == null)
            return false;

        Vector3 posicionViewport = camara.WorldToViewportPoint(transform.position);

        return posicionViewport.z > 0f &&
               posicionViewport.x >= 0f && posicionViewport.x <= 1f &&
               posicionViewport.y >= 0f && posicionViewport.y <= 1f;
    }

    void ActualizarEstado(bool encendido)
    {
        activo = encendido;

        // Barrera física — activa solo cuando ON y no hay modo combinado
        if (barreraGO != null) barreraGO.SetActive(encendido && !modoCombinadoActivo);

        // AreaEffector permanece deshabilitado siempre; la fuerza la aplica FixedUpdate solo sobre Babosa.

        if (!encendido)
        {
            combinadoEnZona         = false;
            combinadoEnZonaAnterior = false;
            soloEnZona              = false;
        }

        if (encendido)
        {
            // Apagar partículas de carga
            if (sistemaParticulasCarga != null)
            {
                sistemaParticulasCarga.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                sistemaParticulasCarga.Clear();
            }
            // Encender chorro principal
            if (sistemaParticulas != null)
            {
                sistemaParticulas.Clear();
                sistemaParticulas.Play();
            }
        }
        else
        {
            // Apagar chorro principal
            if (sistemaParticulas != null)
            {
                sistemaParticulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                sistemaParticulas.Clear();
            }
            // Encender partículas de carga (burbujeo previo)
            if (sistemaParticulasCarga != null)
            {
                sistemaParticulasCarga.Clear();
                sistemaParticulasCarga.Play();
            }
        }

        ActualizarSonidoPorCamara();
    }

    void CrearSistemaParticulasCarga()
    {
        GameObject go = new GameObject("ParticulasCarga");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        sistemaParticulasCarga = go.AddComponent<ParticleSystem>();

        var main = sistemaParticulasCarga.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.duration        = 999f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0f, 0f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.85f, 0.97f, 1f, 1f), Color.white
        );
        main.gravityModifier = -0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 1200;

        var emission = sistemaParticulasCarga.emission;
        emission.rateOverTime = 160f;

        var shape = sistemaParticulasCarga.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 1.5f;

        var vel = sistemaParticulasCarga.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-4f, 4f);
        vel.y = new ParticleSystem.MinMaxCurve(1.2f, 3.5f);
        vel.z = new ParticleSystem.MinMaxCurve(0f,    0f);

        var noise = sistemaParticulasCarga.noise;
        noise.enabled     = true;
        noise.strength    = new ParticleSystem.MinMaxCurve(0.7f);
        noise.frequency   = 0.8f;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.6f);
        noise.quality     = ParticleSystemNoiseQuality.Medium;

        var col = sistemaParticulasCarga.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white,                  0f),
                new GradientColorKey(new Color(0.85f, 0.97f, 1f), 0.3f),
                new GradientColorKey(new Color(0.5f,  0.85f, 1f), 0.7f),
                new GradientColorKey(new Color(0.3f,  0.7f,  1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.7f, 0.4f),
                new GradientAlphaKey(0.2f, 0.8f),
                new GradientAlphaKey(0f,   1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var size = sistemaParticulasCarga.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve curva = new AnimationCurve(
            new Keyframe(0f,   1f),
            new Keyframe(0.5f, 0.6f),
            new Keyframe(1f,   0f)
        );
        size.size = new ParticleSystem.MinMaxCurve(1f, curva);

        var rend = sistemaParticulasCarga.GetComponent<ParticleSystemRenderer>();
        if (rend != null)
        {
            rend.renderMode = ParticleSystemRenderMode.Billboard;

            // Intentar copiar material del PS principal (tiene el del artista si no fue creado por código)
            Material mat = null;
            if (sistemaParticulas != null && !psCreatedByUs)
            {
                var srcRend = sistemaParticulas.GetComponent<ParticleSystemRenderer>();
                if (srcRend != null) mat = srcRend.sharedMaterial;
            }
            // Si el PS principal también fue creado por código, buscar en otros geisers (en Start, no aquí)
            // Por ahora fallback a Sprites/Default para evitar el magenta
            if (mat == null)
            {
                Shader sh = Shader.Find("Sprites/Default");
                if (sh != null) mat = new Material(sh);
            }
            if (mat != null) rend.sharedMaterial = mat;

            var sr = GetComponent<SpriteRenderer>();
            rend.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
            rend.sortingOrder     = sr != null ? sr.sortingOrder + 1 : 1;
        }

        sistemaParticulasCarga.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void ConfigurarParticulasAgua()
    {
        if (sistemaParticulas == null) return;

        var main = sistemaParticulas.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.duration        = 999f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(1.0f, 2.2f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0f, 0f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.85f, 0.97f, 1f, 1f), Color.white
        );
        main.gravityModifier = 1.8f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 3000;

        var emission = sistemaParticulas.emission;
        emission.rateOverTime = 900f;

        var shape = sistemaParticulas.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.15f;

        var vel = sistemaParticulas.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-2.5f, 2.5f);
        vel.y = new ParticleSystem.MinMaxCurve(fuerzaImpulso * 1.5f, fuerzaImpulso * 2.5f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var noise = sistemaParticulas.noise;
        noise.enabled     = true;
        noise.strength    = new ParticleSystem.MinMaxCurve(1.0f);
        noise.frequency   = 0.7f;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.5f);
        noise.quality     = ParticleSystemNoiseQuality.High;

        var col = sistemaParticulas.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white,                  0f),
                new GradientColorKey(new Color(0.85f, 0.97f, 1f), 0.2f),
                new GradientColorKey(new Color(0.45f, 0.82f, 1f), 0.6f),
                new GradientColorKey(new Color(0.2f,  0.65f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f,    0f),
                new GradientAlphaKey(0.95f, 0.25f),
                new GradientAlphaKey(0.6f,  0.65f),
                new GradientAlphaKey(0f,    1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var size = sistemaParticulas.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f,    0.1f),
            new Keyframe(0.08f, 1f),
            new Keyframe(0.5f,  0.65f),
            new Keyframe(1f,    0.05f)
        );
        size.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var renderer = sistemaParticulas.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode    = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.06f;
            renderer.lengthScale   = 2.0f;
            var sr = GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
            renderer.sortingOrder     = sr != null ? sr.sortingOrder + 1 : 1;

            // Si el PS fue creado por código, copiar el material del PS artístico de otro geiser
            if (psCreatedByUs)
            {
                Material matArtistico = BuscarMaterialArtistaDeOtroGeiser();
                if (matArtistico != null)
                    renderer.sharedMaterial = matArtistico;
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Escalonar todos los geisers de la escena")]
    void EscalonarTodosLosGeisers()
    {
        var todos = FindObjectsByType<GeiserControl>(FindObjectsSortMode.None);

        // Ordenar por posición X para que el escalonado sea lógico izq→der
        System.Array.Sort(todos, (a, b) =>
            a.transform.position.x.CompareTo(b.transform.position.x));

        float ciclo = tiempoActivo + tiempoInactivo;
        float intervalo = ciclo / Mathf.Max(todos.Length, 1);

        for (int i = 0; i < todos.Length; i++)
        {
            Undo.RecordObject(todos[i], "Escalonar geisers");
            todos[i].desfase = intervalo * i;
            EditorUtility.SetDirty(todos[i].gameObject);
        }

        Debug.Log($"[GeiserControl] {todos.Length} geisers escalonados — intervalo: {intervalo:F2}s por cada uno.");
    }

    [ContextMenu("Inicializar Particle System (Editor)")]
    void InicializarPSDesdeEditor()
    {
        // Buscar PS hijo existente
        ParticleSystem psExistente = GetComponentInChildren<ParticleSystem>();
        if (psExistente != null)
        {
            sistemaParticulas = psExistente;
            Debug.Log($"[{gameObject.name}] Particle System ya existe — referencia asignada.");
        }
        else
        {
            // Crear uno nuevo
            GameObject goPS = new GameObject("Particle System");
            Undo.RegisterCreatedObjectUndo(goPS, "Crear PS Geiser");
            goPS.transform.SetParent(transform);
            goPS.transform.localPosition = Vector3.zero;
            sistemaParticulas = goPS.AddComponent<ParticleSystem>();
            psCreatedByUs = true;
            Debug.Log($"[{gameObject.name}] Particle System creado.");
        }

        // Copiar material del primer geiser que ya lo tenga configurado
        var rendDst = sistemaParticulas.GetComponent<ParticleSystemRenderer>();
        if (rendDst != null && (rendDst.sharedMaterial == null ||
            rendDst.sharedMaterial.name.Contains("Default-Particle") ||
            rendDst.sharedMaterial.name.Contains("Default-ParticleSystem")))
        {
            Material mat = BuscarMaterialArtistaDeOtroGeiser();
            if (mat != null)
            {
                rendDst.sharedMaterial = mat;
                Debug.Log($"[{gameObject.name}] Material '{mat.name}' copiado de otro geiser.");
            }
        }

        EditorUtility.SetDirty(gameObject);
    }
#endif

    // Busca el material que el artista asignó al PS de cualquier otro geiser en la escena
    Material BuscarMaterialArtistaDeOtroGeiser()
    {
        foreach (var gc in FindObjectsByType<GeiserControl>(FindObjectsSortMode.None))
        {
            if (gc == this) continue;

            // Buscar en todos los PS hijos (no solo en el primero)
            ParticleSystem[] todos = gc.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in todos)
            {
                // Ignorar el ParticulasCarga (lo nombramos así)
                if (ps.gameObject.name == "ParticulasCarga") continue;

                var rend = ps.GetComponent<ParticleSystemRenderer>();
                if (rend == null || rend.sharedMaterial == null) continue;

                string nombre = rend.sharedMaterial.name;
                // Descartar materiales default/sprites-default si hay otros disponibles
                if (!nombre.Contains("Default-Particle") &&
                    !nombre.Contains("Sprites-Default") &&
                    !nombre.Contains("Default-ParticleSystem"))
                    return rend.sharedMaterial;
            }
        }

        // Segunda pasada: aceptar cualquier material no nulo (incluso Sprites-Default)
        foreach (var gc in FindObjectsByType<GeiserControl>(FindObjectsSortMode.None))
        {
            if (gc == this) continue;
            ParticleSystem[] todos = gc.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in todos)
            {
                if (ps.gameObject.name == "ParticulasCarga") continue;
                var rend = ps.GetComponent<ParticleSystemRenderer>();
                if (rend != null && rend.sharedMaterial != null)
                    return rend.sharedMaterial;
            }
        }

        return null;
    }

    public void ActualizarBarreras()
    {
        if (barreraGO != null) barreraGO.SetActive(activo && !modoCombinadoActivo);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 centro = transform.position + new Vector3(offsetBarreraWorld.x, offsetBarreraWorld.y, 0f);

        // Paredes (cyan)
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireCube(centro, new Vector3(anchoBarrera, alturaBarrera, 0.1f));
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(centro + Vector3.left  * anchoBarrera / 2f + Vector3.down * alturaBarrera / 2f,
                        centro + Vector3.left  * anchoBarrera / 2f + Vector3.up   * alturaBarrera / 2f);
        Gizmos.DrawLine(centro + Vector3.right * anchoBarrera / 2f + Vector3.down * alturaBarrera / 2f,
                        centro + Vector3.right * anchoBarrera / 2f + Vector3.up   * alturaBarrera / 2f);

    }
}