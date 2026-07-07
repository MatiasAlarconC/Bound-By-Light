using UnityEngine;
using System.Collections.Generic;

public class PulpoColumpio : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadSuelo = 8f;
    [SerializeField] private float velocidadTecho = 6f;
    [SerializeField] private float fuerzaSalto = 18f; 

    [Header("Control de Salto (Suelo)")]
    [Tooltip("Selecciona la capa (Layer) que representa el suelo de tu juego")]
    [SerializeField] private LayerMask capaSuelo;

    [Header("Configuración del Tentáculo (Automático)")]
    [SerializeField] private string tagDeLaBabosa = "Player"; 
    [Tooltip("Distancia ideal entre cada pedazo de tentáculo (ej: 1.5 unidades por eslabón)")]
    [SerializeField] private float distanciaPorEslabon = 1.2f; 
    [SerializeField] private float longitudMaximaTentaculo = 25f; 
    
    [Header("Ajuste de Altura")]
    [Tooltip("Cuánto más abajo de la babosa quieres que baje el tentáculo para asegurar el enganche")]
    [SerializeField] private float margenExtraBajar = 0.8f; 

    [Header("Efectos de Sonido del Tentáculo")]
    [Tooltip("Arrastra aquí los diferentes clips de sonido que sonarán al engancharse")]
    [SerializeField] private AudioClip[] sonidosEngancheTecho;

    [Tooltip("Arrastra aquí los diferentes clips de sonido para el salto del pulpo")]
    [SerializeField] private AudioClip[] sonidosSaltoPulpo;
    private AudioSource miLectorDeAudio;

    private Rigidbody2D rbPulpo;
    private Collider2D colliderPulpo;
    private LineRenderer lineaVisual;
    private Animator animatorPulpo; 
    private SpriteRenderer spritePulpo; 
    private PilarRuta pilarActual;
    private Collider2D colliderDelCuadradoOculto; 

    private List<GameObject> eslabones = new List<GameObject>();
    private GameObject puntaTentaculo;
    private bool tentaculoDesplegado = false;
    private bool pegadoAlTecho = false;
    private float tiempoSiguienteEnganche = 0f;
    private bool estaControlado = false;
    private bool mirandoDerecha = true;

    // YA NO USAMOS EL OBJETO VACÍO. Ahora usamos un candado de saltos.
    private int saltosDisponibles = 1;

    void Awake()
    {
        rbPulpo = GetComponent<Rigidbody2D>();
        colliderPulpo = GetComponent<Collider2D>();
        lineaVisual = GetComponent<LineRenderer>();
        animatorPulpo = GetComponent<Animator>();
        spritePulpo = GetComponent<SpriteRenderer>();

        // Inicializamos el lector de audio para este personaje
        miLectorDeAudio = GetComponent<AudioSource>();
        if (miLectorDeAudio == null)
        {
            miLectorDeAudio = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (lineaVisual != null) {
            lineaVisual.enabled = false;
            lineaVisual.useWorldSpace = true; 
        }

        rbPulpo.bodyType = RigidbodyType2D.Dynamic;
        rbPulpo.gravityScale = 1f;
        rbPulpo.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void SetControlActivo(bool activo)
    {
        estaControlado = activo;
        if (rbPulpo != null) rbPulpo.WakeUp();
    }

    void GenerarCadenaDinamica(int cantidadEslabones, float distanciaEntreEllos)
    {
        DestruirCadenaActual();

        Rigidbody2D rbAnterior = rbPulpo;

        for (int i = 0; i < cantidadEslabones; i++)
        {
            GameObject eslabon = new GameObject("EslabonTentaculo_" + i);
            eslabon.transform.SetParent(this.transform); 
            eslabon.layer = LayerMask.NameToLayer("Default");

            Rigidbody2D rbEslabon = eslabon.AddComponent<Rigidbody2D>();
            rbEslabon.angularDamping = 1.2f;  
            rbEslabon.linearDamping = 0.2f;   
            rbEslabon.gravityScale = 0.8f;   

            HingeJoint2D articulacion = eslabon.AddComponent<HingeJoint2D>();
            articulacion.connectedBody = rbAnterior;
            articulacion.autoConfigureConnectedAnchor = false;
            
            if (i == 0)
                articulacion.connectedAnchor = Vector2.zero;
            else
                articulacion.connectedAnchor = new Vector2(0, -distanciaEntreEllos);

            articulacion.anchor = Vector2.zero;

            if (i == cantidadEslabones - 1)
            {
                puntaTentaculo = eslabon;
                CircleCollider2D col = puntaTentaculo.AddComponent<CircleCollider2D>();
                col.radius = 0.6f; 
                col.isTrigger = true;
                rbEslabon.gravityScale = 1.0f; 
            }

            eslabon.SetActive(false);
            eslabones.Add(eslabon);
            rbAnterior = rbEslabon;
        }

        if (lineaVisual != null) lineaVisual.positionCount = cantidadEslabones + 1;
    }

    void DestruirCadenaActual()
    {
        foreach (GameObject eslabon in eslabones)
        {
            if (eslabon != null) Destroy(eslabon);
        }
        eslabones.Clear();
        puntaTentaculo = null;
    }

    void Update()
    {
        if (!estaControlado) 
        {
            if (pegadoAlTecho) rbPulpo.linearVelocity = Vector2.zero;
            return;
        }

        // NUEVO: Verificamos si el propio collider del pulpo está tocando el suelo.
        if (colliderPulpo != null && colliderPulpo.IsTouchingLayers(capaSuelo) && rbPulpo.linearVelocity.y <= 0.1f)
        {
            saltosDisponibles = 1;
        }

        float inputH = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputH = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputH = -1f;

        if (inputH != 0f || Input.GetButtonDown("Jump")) rbPulpo.WakeUp();

        if (inputH > 0f && !mirandoDerecha) VoltearPersonaje();
        else if (inputH < 0f && mirandoDerecha) VoltearPersonaje();

        if (!pegadoAlTecho)
        {
            rbPulpo.linearVelocity = new Vector2(inputH * velocidadSuelo, rbPulpo.linearVelocity.y);

            // ================================================================
            // MODIFICADO: DETECCIÓN DEL SALTO CON AUDIO ALEATORIO
            // ================================================================
            if (Input.GetButtonDown("Jump") && saltosDisponibles > 0)
            {
                saltosDisponibles = 0;
                rbPulpo.linearVelocity = new Vector2(rbPulpo.linearVelocity.x, 0f);
                rbPulpo.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

                // === NUEVO: Elegimos un efecto de sonido al azar del Array ===
                if (miLectorDeAudio != null && sonidosSaltoPulpo != null && sonidosSaltoPulpo.Length > 0)
                {
                    int indiceAleatorio = Random.Range(0, sonidosSaltoPulpo.Length);
                    miLectorDeAudio.PlayOneShot(sonidosSaltoPulpo[indiceAleatorio]);
                    Debug.Log("<color=yellow>¡Pulpo: Sonido de salto aleatorio reproducido!</color>");
                }
            }
        }
        else
        {
            rbPulpo.linearVelocity = new Vector2(inputH * velocidadTecho, 0f);

            if (colliderDelCuadradoOculto != null)
            {
                float limiteIzquierdo = colliderDelCuadradoOculto.bounds.min.x;
                float limiteDerecho = colliderDelCuadradoOculto.bounds.max.x;

                if (transform.position.x < limiteIzquierdo || transform.position.x > limiteDerecho)
                {
                    SoltarYAvanzar();
                }
            }

            if (Input.GetButtonDown("Jump")) SoltarYAvanzar();
        }
    }

    void VoltearPersonaje()
    {
        mirandoDerecha = !mirandoDerecha;

        if (spritePulpo != null)
        {
            spritePulpo.flipX = !mirandoDerecha;
        }
        else
        {
            // CORREGIDO: Modificamos la escala usando una variable local temporal
            Vector3 escalaEscena = transform.localScale;
            escalaEscena.x *= -1; 
            transform.localScale = escalaEscena;
        }
    }

    private Vector3 getEscalaEscena() => transform.localScale;

    void LateUpdate()
    {
        if (lineaVisual == null || !lineaVisual.enabled || !tentaculoDesplegado || eslabones.Count == 0) return;

        lineaVisual.SetPosition(0, transform.position);
        
        for (int i = 0; i < eslabones.Count; i++)
        {
            if (eslabones[i] != null)
                lineaVisual.SetPosition(i + 1, eslabones[i].transform.position);
        }

        if (eslabones.Count > 0 && puntaTentaculo != null)
        {
            float distanciaTotal = Vector2.Distance(transform.position, puntaTentaculo.transform.position);
            lineaVisual.material.mainTextureScale = new Vector2(distanciaTotal, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificamos que sea un pilar válido, que no estemos colgados ya y que haya pasado el tiempo de enfriamiento
        if (other.CompareTag("PilarColumpio") && pilarActual == null && Time.time > tiempoSiguienteEnganche)
        {
            pilarActual = other.GetComponent<PilarRuta>();
            colliderDelCuadradoOculto = other;

            if (pilarActual != null)
            {
                // ================================================================
                // NUEVO: SELECCIÓN Y REPRODUCCIÓN ALEATORIA DE EFECTO DE SONIDO
                // ================================================================
                if (miLectorDeAudio != null && sonidosEngancheTecho != null && sonidosEngancheTecho.Length > 0)
                {
                    // Elegimos un casillero al azar de tu arreglo de Unity
                    int indiceAleatorio = Random.Range(0, sonidosEngancheTecho.Length);
                    
                    // Reproducimos el clip de sonido seleccionado sin interrumpir otros canales
                    miLectorDeAudio.PlayOneShot(sonidosEngancheTecho[indiceAleatorio], 0.3f);
                    Debug.Log("<color=cyan>¡PulpoColumpio: Sonido de gancho aleatorio reproducido!</color>");
                }

                // ================================================================
                // LOGICA ORIGINAL DE ENGANCHE Y GENERACIÓN DEL TENTÁCULO
                // ================================================================
                // Al engancharse al techo reseteamos el salto por si acaso para cuando se suelte
                saltosDisponibles = 1;
                pegadoAlTecho = true;
                if (animatorPulpo != null) animatorPulpo.SetBool("estaPegado", true);

                rbPulpo.bodyType = RigidbodyType2D.Kinematic;
                rbPulpo.linearVelocity = Vector2.zero;
                
                if (colliderPulpo != null) colliderPulpo.enabled = false;

                float longitudFinal = 5f;

                GameObject babosa = GameObject.FindGameObjectWithTag(tagDeLaBabosa);
                
                if (babosa != null)
                {
                    float distanciaAlCentroY = transform.position.y - babosa.transform.position.y;
                    longitudFinal = distanciaAlCentroY + margenExtraBajar;
                }

                if (longitudFinal > longitudMaximaTentaculo) longitudFinal = longitudMaximaTentaculo;
                if (longitudFinal < 2f) longitudFinal = 2f;

                int calculoEslabones = Mathf.CeilToInt(longitudFinal / distanciaPorEslabon);
                if (calculoEslabones < 3) calculoEslabones = 3;

                float distanciaExactaEntreEslabones = longitudFinal / calculoEslabones;

                GenerarCadenaDinamica(calculoEslabones, distanciaExactaEntreEslabones);

                for (int i = 0; i < eslabones.Count; i++)
                {
                    Vector3 posicionMundo = transform.position + Vector3.down * (distanciaExactaEntreEslabones * (i + 1));
                    eslabones[i].transform.position = posicionMundo;
                    eslabones[i].transform.rotation = Quaternion.identity;
                    
                    eslabones[i].SetActive(true);

                    Rigidbody2D rbEslabon = eslabones[i].GetComponent<Rigidbody2D>();
                    if (rbEslabon != null)
                    {
                        rbEslabon.bodyType = RigidbodyType2D.Dynamic;
                        rbEslabon.linearVelocity = Vector2.zero;
                        rbEslabon.angularVelocity = 0f;
                    }

                    HingeJoint2D articulacion = eslabones[i].GetComponent<HingeJoint2D>();
                    if (articulacion != null)
                    {
                        if (i == 0)
                            articulacion.connectedAnchor = Vector2.zero;
                        else
                            articulacion.connectedAnchor = new Vector2(0, -distanciaExactaEntreEslabones);
                    }
                }
                
                tentaculoDesplegado = true;
                if (lineaVisual != null) lineaVisual.enabled = true;
            }
        }
    }

    public void SoltarYAvanzar()
    {
        pegadoAlTecho = false; 
        if (animatorPulpo != null) animatorPulpo.SetBool("estaPegado", false);

        tentaculoDesplegado = false;
        if (lineaVisual != null) lineaVisual.enabled = false;

        tiempoSiguienteEnganche = Time.time + 0.4f;

        DestruirCadenaActual();

        if (colliderPulpo != null) colliderPulpo.enabled = true;

        rbPulpo.bodyType = RigidbodyType2D.Dynamic;
        rbPulpo.WakeUp();
        pilarActual = null;
        colliderDelCuadradoOculto = null; 
    }

    public GameObject ObtenerPuntaTentaculo() { return puntaTentaculo; }
    public bool IsTentaculoDesplegado() { return tentaculoDesplegado; }
}