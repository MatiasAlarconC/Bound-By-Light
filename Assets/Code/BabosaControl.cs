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
    private float _inputCooldown;

    private SpriteRenderer miSprite;
    private Animator miAnimator;
    private SpriteRenderer _outlineRenderer;

    void Start()
    {
        rbBabosa = GetComponent<Rigidbody2D>();
        if (rbBabosa != null)
        {
            rbBabosa.mass = 0.5f;
        }

        miSprite = GetComponent<SpriteRenderer>();
        miAnimator = GetComponent<Animator>();

        miLectorDeAudio = GetComponent<AudioSource>();
        if (miLectorDeAudio == null)
        {
            miLectorDeAudio = gameObject.AddComponent<AudioSource>();
        }

        // Outline exterior: shader detecta borde y pinta blanco solo fuera del sprite
        Shader outlineShader = Shader.Find("Custom/SpriteSilhouette");
        GameObject outlineGO = new GameObject("Outline");
        outlineGO.transform.SetParent(transform);
        outlineGO.transform.localPosition = Vector3.zero;
        outlineGO.transform.localScale = Vector3.one * 1.05f; // extiende el mesh levemente para dar espacio al borde
        _outlineRenderer = outlineGO.AddComponent<SpriteRenderer>();
        if (miSprite != null)
        {
            _outlineRenderer.sprite = miSprite.sprite;
            _outlineRenderer.sortingLayerName = miSprite.sortingLayerName;
            _outlineRenderer.sortingOrder = miSprite.sortingOrder - 1;
        }
        if (outlineShader != null)
        {
            Material mat = new Material(outlineShader);
            mat.SetColor("_Color", Color.white);
            mat.SetFloat("_OutlineSize", 1.5f);
            _outlineRenderer.material = mat;
        }
        _outlineRenderer.enabled = false;
    }

    public void SetControlActivo(bool activo)
    {
        estaControlado = activo;
        if (rbBabosa != null)
        {
            if (!activo) rbBabosa.linearVelocity = Vector2.zero;
            rbBabosa.WakeUp();
        }
    }

    public void BloquearInput(float duracion)
    {
        _inputCooldown = Time.time + duracion;
    }

    public void SetOutlineActivo(bool activo)
    {
        if (_outlineRenderer != null) _outlineRenderer.enabled = activo;
    }

    public void ResetearEstado()
    {
        colgadaActualmente = false;
        cooldownEnganche = 0f;
        if (agarreActual != null)
        {
            Destroy(agarreActual);
            agarreActual = null;
        }
        pulpoCuerdaActual = null;
        if (miAnimator != null) miAnimator.SetBool("estaEnganchada", false);
        if (rbBabosa != null) rbBabosa.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!estaControlado) return;
        if (Time.time < _inputCooldown) return;

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
                rbBabosa.AddForce(new Vector2(inputH * fuerzaFisicaBalanceo * Time.fixedDeltaTime * 60f, 0f), ForceMode2D.Force);
            }

            if (Input.GetButtonDown("Jump"))
            {
                SoltarYSalirDisparada();
            }
        }

        if (miSprite != null)
        {
            if (inputH < 0f)
            {
                miSprite.flipX = true;
            }
            else if (inputH > 0f)
            {
                miSprite.flipX = false;
            }
        }
    }

    void LateUpdate()
    {
        if (_outlineRenderer == null || !_outlineRenderer.enabled || miSprite == null) return;
        _outlineRenderer.sprite = miSprite.sprite;
        _outlineRenderer.flipX  = miSprite.flipX;
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

                    if (miAnimator != null)
                    {
                        miAnimator.SetBool("estaEnganchada", true);
                    }

                    rbBabosa.linearVelocity = new Vector2(rbBabosa.linearVelocity.x * 1.2f, 4f);
                    Debug.Log("Enganchado dinámicamente al pulpo: " + pulpoDetectado.name);

                    GameManager gm = FindFirstObjectByType<GameManager>();
                    if (gm != null)
                    {
                        gm.ReproducirSonidoEnganche();
                    }
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

        if (miLectorDeAudio != null && sonidosSaltoBabosa != null && sonidosSaltoBabosa.Length > 0)
        {
            int indiceAleatorio = Random.Range(0, sonidosSaltoBabosa.Length);
            miLectorDeAudio.PlayOneShot(sonidosSaltoBabosa[indiceAleatorio]);
            Debug.Log("<color=green>¡Babosa: Sonido de impulso/desenganche reproducido!</color>");
        }
    }
}
