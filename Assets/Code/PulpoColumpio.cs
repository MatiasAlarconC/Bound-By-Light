using UnityEngine;
using System.Collections.Generic;

public class PulpoColumpio : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadSuelo = 8f;
    [SerializeField] private float velocidadTecho = 6f;
    [SerializeField] private float fuerzaSalto = 18f; 

    [Header("Configuración del Tentáculo (Automático)")]
    [SerializeField] private string tagDeLaBabosa = "Player"; 
    [SerializeField] private int numeroDeEslabones = 5;    
    [SerializeField] private float longitudMaximaTentaculo = 12f; 
    
    [Header("Ajuste de Altura")]
    [Tooltip("Cuánto más abajo de la babosa quieres que baje el tentáculo para asegurar el enganche")]
    [SerializeField] private float margenExtraBajar = 0.8f; 

    private Rigidbody2D rbPulpo;
    private Collider2D colliderPulpo;
    private LineRenderer lineaVisual;
    private Animator animatorPulpo; 
    private PilarRuta pilarActual;

    private List<GameObject> eslabones = new List<GameObject>();
    private GameObject puntaTentaculo;
    private bool tentaculoDesplegado = false;
    private bool pegadoAlTecho = false;
    private float tiempoSiguienteEnganche = 0f;
    private bool estaControlado = false;

    // VARIABLE NUEVA: Para recordar hacia dónde miraba originalmente
    private bool mirandoDerecha = true;

    void Awake()
    {
        rbPulpo = GetComponent<Rigidbody2D>();
        colliderPulpo = GetComponent<Collider2D>();
        lineaVisual = GetComponent<LineRenderer>();
        animatorPulpo = GetComponent<Animator>(); 
    }

    void Start()
    {
        if (lineaVisual != null) {
            lineaVisual.enabled = false;
            lineaVisual.useWorldSpace = false; 
        }
        PrepararTentaculoCadena();

        rbPulpo.bodyType = RigidbodyType2D.Dynamic;
        rbPulpo.gravityScale = 1f;
        rbPulpo.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void SetControlActivo(bool activo)
    {
        estaControlado = activo;
        if (rbPulpo != null) rbPulpo.WakeUp();
    }

    void PrepararTentaculoCadena()
    {
        float distanciaBase = 5f / numeroDeEslabones;
        Rigidbody2D rbAnterior = rbPulpo;

        for (int i = 0; i < numeroDeEslabones; i++)
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
                articulacion.connectedAnchor = new Vector2(0, -distanciaBase);

            articulacion.anchor = Vector2.zero;
            articulacion.useLimits = false; 

            if (i == numeroDeEslabones - 1)
            {
                puntaTentaculo = eslabon;
                CircleCollider2D col = puntaTentaculo.AddComponent<CircleCollider2D>();
                col.radius = 0.6f; 
                col.isTrigger = true;
                rbEslabon.gravityScale = 1.2f; 
            }

            eslabon.SetActive(false);
            eslabones.Add(eslabon);
            rbAnterior = rbEslabon;
        }

        if (lineaVisual != null) lineaVisual.positionCount = numeroDeEslabones + 1;
    }

    void Update()
    {
        if (!estaControlado) 
        {
            if (pegadoAlTecho) rbPulpo.linearVelocity = Vector2.zero;
            return; 
        }

        float inputH = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputH = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputH = -1f;

        if (inputH != 0f || Input.GetButtonDown("Jump")) rbPulpo.WakeUp();

        // ====================================================================
        // NUEVO: SISTEMA DE VOLTEO (FLIP) AL CAMINAR
        // ====================================================================
        if (inputH > 0f && !mirandoDerecha)
        {
            VoltearPersonaje();
        }
        else if (inputH < 0f && mirandoDerecha)
        {
            VoltearPersonaje();
        }

        if (!pegadoAlTecho)
        {
            rbPulpo.linearVelocity = new Vector2(inputH * velocidadSuelo, rbPulpo.linearVelocity.y);

            if (Input.GetButtonDown("Jump"))
            {
                rbPulpo.linearVelocity = new Vector2(rbPulpo.linearVelocity.x, 0f);
                rbPulpo.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            }
        }
        else
        {
            rbPulpo.linearVelocity = new Vector2(inputH * velocidadTecho, 0f);

            if (Input.GetButtonDown("Jump")) SoltarYAvanzar();
        }
    }

    // FUNCIÓN NUEVA: Invierte la escala en X para que mire al otro lado de forma real
    void VoltearPersonaje()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escalaEscena = transform.localScale;
        escalaEscena.x *= -1; // Multiplica por -1 para hacer el efecto espejo
        transform.localScale = escalaEscena;
    }

    void LateUpdate()
    {
        if (lineaVisual == null || !lineaVisual.enabled || !tentaculoDesplegado) return;

        lineaVisual.SetPosition(0, Vector3.zero);
        for (int i = 0; i < eslabones.Count; i++)
        {
            lineaVisual.SetPosition(i + 1, eslabones[i].transform.localPosition);
        }

        if (eslabones.Count > 0 && puntaTentaculo != null)
        {
            float distanciaTotal = Vector2.Distance(transform.position, puntaTentaculo.transform.position);
            lineaVisual.material.mainTextureScale = new Vector2(distanciaTotal, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PilarColumpio") && pilarActual == null && Time.time > tiempoSiguienteEnganche)
        {
            pilarActual = other.GetComponent<PilarRuta>();
            if (pilarActual != null)
            {
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

                float distanciaEntreEslabones = longitudFinal / numeroDeEslabones;

                for (int i = 0; i < eslabones.Count; i++)
                {
                    eslabones[i].transform.localPosition = Vector3.down * (distanciaEntreEslabones * (i + 1));
                    eslabones[i].transform.localRotation = Quaternion.identity;
                    
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
                            articulacion.connectedAnchor = new Vector2(0, -distanciaEntreEslabones);
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

        for (int i = 0; i < eslabones.Count; i++)
        {
            Rigidbody2D rbEslabon = eslabones[i].GetComponent<Rigidbody2D>();
            if (rbEslabon != null)
            {
                rbEslabon.linearVelocity = Vector2.zero;
                rbEslabon.angularVelocity = 0f;
            }
            eslabones[i].SetActive(false);
        }

        if (colliderPulpo != null) colliderPulpo.enabled = true;

        rbPulpo.bodyType = RigidbodyType2D.Dynamic;
        rbPulpo.WakeUp();
        pilarActual = null;
    }

    public GameObject ObtenerPuntaTentaculo() { return puntaTentaculo; }
    public bool IsTentaculoDesplegado() { return tentaculoDesplegado; }
}