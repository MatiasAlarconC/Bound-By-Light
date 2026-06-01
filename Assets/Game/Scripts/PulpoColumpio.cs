using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PulpoColumpio : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadSuelo = 8f;
    [SerializeField] private float velocidadTecho = 6f;
    [SerializeField] private float fuerzaSalto = 18f; 

    [Header("Configuración del Tentáculo")]
    [SerializeField] private float longitudTentaculo = 5f;
    [SerializeField] private int numeroDeEslabones = 5; 

    private Rigidbody2D rbPulpo;
    private Collider2D colliderPulpo;
    private LineRenderer lineaVisual;
    private PilarRuta pilarActual;

    private List<GameObject> eslabones = new List<GameObject>();
    private GameObject puntaTentaculo;
    private bool tentaculoDesplegado = false;
    private bool pegadoAlTecho = false;
    private float tiempoSiguienteEnganche = 0f;
    private bool estaControlado = false;

    void Awake()
    {
        rbPulpo = GetComponent<Rigidbody2D>();
        colliderPulpo = GetComponent<Collider2D>();
        lineaVisual = GetComponent<LineRenderer>();
    }

    void Start()
    {
        if (lineaVisual != null) lineaVisual.enabled = false;
        PrepararTentaculoCadena();
    }

    void OnEnable()
    {
        if (rbPulpo == null) rbPulpo = GetComponent<Rigidbody2D>();
        
        rbPulpo.bodyType = RigidbodyType2D.Dynamic;
        rbPulpo.gravityScale = 1f;
        rbPulpo.constraints = RigidbodyConstraints2D.FreezeRotation; 
        rbPulpo.WakeUp();
    }

    void PrepararTentaculoCadena()
    {
        float distanciaEntreEslabones = longitudTentaculo / numeroDeEslabones;
        Rigidbody2D rbAnterior = rbPulpo;

        for (int i = 0; i < numeroDeEslabones; i++)
        {
            GameObject eslabon = new GameObject("EslabonTentaculo_" + i);
            eslabon.transform.SetParent(this.transform);

            Rigidbody2D rbEslabon = eslabon.AddComponent<Rigidbody2D>();
            rbEslabon.angularDamping = 0.1f;
            rbEslabon.linearDamping = 0.05f;
            rbEslabon.gravityScale = 1.2f; 

            HingeJoint2D articulacion = eslabon.AddComponent<HingeJoint2D>();
            articulacion.connectedBody = rbAnterior;
            articulacion.autoConfigureConnectedAnchor = false;
            
            if (i == 0)
                articulacion.connectedAnchor = Vector2.zero;
            else
                articulacion.connectedAnchor = new Vector2(0, -distanciaEntreEslabones);

            articulacion.anchor = Vector2.zero;

            articulacion.useLimits = true;
            JointAngleLimits2D limitesCuerda = new JointAngleLimits2D();
            limitesCuerda.min = -90f; 
            limitesCuerda.max = 90f;  
            articulacion.limits = limitesCuerda;

            if (i == numeroDeEslabones - 1)
            {
                puntaTentaculo = eslabon;
                CircleCollider2D col = puntaTentaculo.AddComponent<CircleCollider2D>();
                col.radius = 0.5f; 
                col.isTrigger = true;
            }

            eslabon.SetActive(false);
            eslabones.Add(eslabon);
            rbAnterior = rbEslabon;
        }

        if (lineaVisual != null) lineaVisual.positionCount = numeroDeEslabones + 1;
    }

    public void SetControlActivo(bool activo)
    {
        estaControlado = activo;
        if (rbPulpo != null) rbPulpo.WakeUp();
    }

    void Update()
    {
        if (!estaControlado)
        {
            if (pegadoAlTecho) rbPulpo.linearVelocity = Vector2.zero;
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float inputH = 0f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) inputH = 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) inputH = -1f;

        bool jumpPressed = keyboard.spaceKey.wasPressedThisFrame;

        if (inputH != 0f || jumpPressed)
        {
            rbPulpo.WakeUp();
        }

        if (!pegadoAlTecho)
        {
            rbPulpo.linearVelocity = new Vector2(inputH * velocidadSuelo, rbPulpo.linearVelocity.y);

            if (jumpPressed)
            {
                rbPulpo.linearVelocity = new Vector2(rbPulpo.linearVelocity.x, 0f);
                rbPulpo.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            }
        }
        else
        {
            rbPulpo.linearVelocity = new Vector2(inputH * velocidadTecho, 0f);

            if (jumpPressed)
            {
                SoltarYAvanzar();
            }
        }
    }

    void LateUpdate()
    {
        if (lineaVisual == null || !lineaVisual.enabled || !tentaculoDesplegado) return;

        lineaVisual.SetPosition(0, transform.position);
        for (int i = 0; i < eslabones.Count; i++)
        {
            lineaVisual.SetPosition(i + 1, eslabones[i].transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // MODIFICACIÓN: Agregamos la validación del tiempo de cooldown (Time.time > tiempoSiguienteEnganche)
        if (other.CompareTag("PilarColumpio") && pilarActual == null && Time.time > tiempoSiguienteEnganche)
        {
            pilarActual = other.GetComponent<PilarRuta>();
            if (pilarActual != null)
            {
                pegadoAlTecho = true; 
                rbPulpo.bodyType = RigidbodyType2D.Kinematic;
                rbPulpo.linearVelocity = Vector2.zero;

                if (colliderPulpo != null) colliderPulpo.enabled = false;

                float distanciaEntreEslabones = longitudTentaculo / numeroDeEslabones;
                for (int i = 0; i < eslabones.Count; i++)
                {
                    eslabones[i].transform.position = transform.position + Vector3.down * (distanciaEntreEslabones * (i + 1));
                    eslabones[i].SetActive(true);
                    
                    Rigidbody2D rb = eslabones[i].GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                }
                
                tentaculoDesplegado = true;
                if (lineaVisual != null) lineaVisual.enabled = true;
            }
        }
    }

    public void SoltarYAvanzar()
    {
        pegadoAlTecho = false; 
        tentaculoDesplegado = false;
        if (lineaVisual != null) lineaVisual.enabled = false;

        // LE DECIMOS AL SCRIPT QUE NO PERMITA ENGANCHARSE POR LOS PRÓXIMOS 0.4 SEGUNDOS
        tiempoSiguienteEnganche = Time.time + 0.4f;

        for (int i = 0; i < eslabones.Count; i++)
        {
            eslabones[i].SetActive(false);
        }

        if (colliderPulpo != null) colliderPulpo.enabled = true;

        rbPulpo.bodyType = RigidbodyType2D.Dynamic;
        rbPulpo.WakeUp();
        pilarActual = null;
        Debug.Log("Pulpo desenganchado y cayendo de forma segura.");
    }

    public GameObject ObtenerPuntaTentaculo() { return puntaTentaculo; }
    public bool IsTentaculoDesplegado() { return tentaculoDesplegado; }
}