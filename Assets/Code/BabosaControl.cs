using UnityEngine;

public class BabosaControl : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadSuelo = 8f;
    [SerializeField] private float fuerzaFisicaBalanceo = 180f; 
    [SerializeField] private float fuerzaSaltoX = 5f;          
    [SerializeField] private float fuerzaSaltoY = 7f;   

    [Header("Efectos de Sonido")]
    [Tooltip("Arrastra aquí los diferentes clips de sonido para cuando la babosa sale impulsada del pulpo")]
    [SerializeField] private AudioClip[] sonidosSaltoBabosa;
    private AudioSource miLectorDeAudio;        

    private Rigidbody2D rbBabosa;
    private HingeJoint2D agarreActual; 
    private bool colgadaActualmente = false;
    private float cooldownEnganche = 0f;
    private PulpoColumpio pulpoCuerdaActual; 

    private bool estaControlado = false;

    // Renderizador de la imagen (Sprite)
    private SpriteRenderer miSprite;

    // COMPONENTE NUEVO: Control de animaciones
    private Animator miAnimator;

    void Start()
    {
        rbBabosa = GetComponent<Rigidbody2D>();
        if (rbBabosa != null)
        {
            rbBabosa.mass = 0.5f;
        }

        miSprite = GetComponent<SpriteRenderer>();
        miAnimator = GetComponent<Animator>();

        // Inicializamos el componente de audio de la Babosa
        miLectorDeAudio = GetComponent<AudioSource>();
        if (miLectorDeAudio == null)
        {
            miLectorDeAudio = gameObject.AddComponent<AudioSource>();
        }
    }

    public void SetControlActivo(bool activo)
    {
        estaControlado = activo;
        if (rbBabosa != null) rbBabosa.WakeUp();
    }

    void Update()
    {
        // Si estamos con el pulpo, la babosa mantiene su inercia física pero no recibe teclado
        if (!estaControlado) return;

        float inputH = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputH = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputH = -1f;

        if (!colgadaActualmente)
        {
            rbBabosa.linearVelocity = new Vector2(inputH * velocidadSuelo, rbBabosa.linearVelocity.y);
        }
        else
        {
            if (rbBabosa != null)
            {
                // Multiplicamos por Time.fixedDeltaTime para que la fuerza sea igual en cualquier PC o navegador
                rbBabosa.AddForce(new Vector2(inputH * fuerzaFisicaBalanceo * Time.fixedDeltaTime * 60f, 0f), ForceMode2D.Force);
            }

            if (Input.GetButtonDown("Jump"))
            {
                SoltarYSalirDisparada();
            }
        }

        // Controlamos hacia dónde mira la imagen según el input
        if (miSprite != null)
        {
            if (inputH < 0f) 
            {
                miSprite.flipX = true;  // Activa el modo espejo (mira a la izquierda)
            }
            else if (inputH > 0f) 
            {
                miSprite.flipX = false; // Desactiva el modo espejo (mira a la derecha)
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (colgadaActualmente || Time.time < cooldownEnganche) return;

        PulpoColumpio pulpoDetectado = other.GetComponentInParent<PulpoColumpio>();

        if (pulpoDetectado != null && pulpoDetectado.IsTentaculoDesplegado())
        {
            GameObject puntaTentaculo = pulpoDetectado.ObtenerPuntaTentaculo();
            
            if (other.gameObject == puntaTentaculo)
            {
                Rigidbody2D rbPunta = other.GetComponent<Rigidbody2D>();
                if (rbPunta != null)
                {
                    colgadaActualmente = true;
                    pulpoCuerdaActual = pulpoDetectado; 

                    agarreActual = gameObject.AddComponent<HingeJoint2D>();
                    agarreActual.connectedBody = rbPunta; 
                    
                    agarreActual.useLimits = true;
                    JointAngleLimits2D limitesSemicirculo = new JointAngleLimits2D();
                    limitesSemicirculo.min = -90f; 
                    limitesSemicirculo.max = 90f;  
                    agarreActual.limits = limitesSemicirculo;

                    // NUEVO: Activamos la animación en el Animator usando el parámetro booleano
                    if (miAnimator != null)
                    {
                        miAnimator.SetBool("estaEnganchada", true);
                    }

                    rbBabosa.linearVelocity = new Vector2(rbBabosa.linearVelocity.x * 1.2f, 4f);
                    Debug.Log("Enganchado dinámicamente al pulpo: " + pulpoDetectado.name);

                    // ====================================================================
                    // ¡NUEVO!: ENCONTRAR AL GAMEMANAGER Y REPRODUCIR EL SONIDO DE ENGANCHE
                    // ====================================================================
                    GameManager gm = FindFirstObjectByType<GameManager>();
                    if (gm != null)
                    {
                        gm.ReproducirSonidoEnganche();
                    }
                    // ====================================================================
                }
            }
        }
    }

    void SoltarYSalirDisparada()
    {
        colgadaActualmente = false;
        cooldownEnganche = Time.time + 0.5f; 

        if (miAnimator != null)
        {
            miAnimator.SetBool("estaEnganchada", false);
        }

        if (agarreActual != null)
        {
            Destroy(agarreActual);
        }

        pulpoCuerdaActual = null; 

        float direccionInercia = rbBabosa.linearVelocity.x >= 0 ? 1f : -1f;
        rbBabosa.linearVelocity = new Vector2(rbBabosa.linearVelocity.x * 0.7f, 0f);
        rbBabosa.AddForce(new Vector2(direccionInercia * fuerzaSaltoX, fuerzaSaltoY), ForceMode2D.Impulse);

        // ====================================================================
        // NUEVO: REPRODUCIR SONIDO ALEATORIO AL DESENGANCHARSE (SALTO)
        // ====================================================================
        if (miLectorDeAudio != null && sonidosSaltoBabosa != null && sonidosSaltoBabosa.Length > 0)
        {
            int indiceAleatorio = Random.Range(0, sonidosSaltoBabosa.Length);
            miLectorDeAudio.PlayOneShot(sonidosSaltoBabosa[indiceAleatorio]);
            Debug.Log("<color=green>¡Babosa: Sonido de impulso/desenganche reproducido!</color>");
        }
    }
}