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
    private bool conectadaMantarraya = false;

    private float cooldownEnganche = 0f;
    private PulpoColumpio pulpoCuerdaActual;

    private bool estaControlado = false;
    private float _inputCooldown;
    private float _inputHFisica = 0f;

    public bool EstaMontada { get; set; } = false;
    public bool FueLanzadaPorGeiser { get; set; } = false;

    public bool EstaControlado => estaControlado;
    public bool EstaColgada => colgadaActualmente;
    public bool EstaConectadaMantarraya => conectadaMantarraya;

    private SpriteRenderer miSprite;
    private Animator miAnimator;
    private SpriteRenderer _outlineRenderer;

    private static readonly int EstaEnganchadaHash =
        Animator.StringToHash("estaEnganchada");

    private static readonly int EstaConectadaMantarrayaHash =
        Animator.StringToHash("estaConectadaMantarraya");

    private void Start()
    {
        rbBabosa = GetComponent<Rigidbody2D>();

        if (rbBabosa != null)
        {
            rbBabosa.mass = 0.5f;
            rbBabosa.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        else
        {
            Debug.LogError(
                "BabosaControl: no se encontró Rigidbody2D.",
                gameObject
            );
        }

        miSprite = GetComponent<SpriteRenderer>();
        miAnimator = GetComponent<Animator>();

        if (miAnimator != null)
        {
            miAnimator.SetBool(EstaEnganchadaHash, false);
            miAnimator.SetBool(EstaConectadaMantarrayaHash, false);
        }
        else
        {
            Debug.LogError(
                "BabosaControl: no se encontró Animator.",
                gameObject
            );
        }

        miLectorDeAudio = GetComponent<AudioSource>();

        if (miLectorDeAudio == null)
        {
            miLectorDeAudio = gameObject.AddComponent<AudioSource>();
        }

        CrearOutline();
    }

    private void CrearOutline()
    {
        Shader outlineShader =
            Shader.Find("Custom/SpriteSilhouette");

        GameObject outlineGO = new GameObject("Outline");

        outlineGO.transform.SetParent(transform);
        outlineGO.transform.localPosition = Vector3.zero;
        outlineGO.transform.localRotation = Quaternion.identity;
        outlineGO.transform.localScale = Vector3.one * 1.05f;

        _outlineRenderer =
            outlineGO.AddComponent<SpriteRenderer>();

        if (miSprite != null)
        {
            _outlineRenderer.sprite = miSprite.sprite;
            _outlineRenderer.sortingLayerName =
                miSprite.sortingLayerName;
            _outlineRenderer.sortingOrder =
                miSprite.sortingOrder - 1;
        }

        if (outlineShader != null)
        {
            Material materialOutline =
                new Material(outlineShader);

            materialOutline.SetColor("_Color", Color.white);
            materialOutline.SetFloat("_OutlineSize", 1.5f);

            _outlineRenderer.material = materialOutline;
        }
        else
        {
            Debug.LogWarning(
                "BabosaControl: no se encontró el shader Custom/SpriteSilhouette.",
                gameObject
            );
        }

        _outlineRenderer.enabled = false;
    }

    public void SetControlActivo(bool activo)
    {
        estaControlado = activo;

        if (rbBabosa != null)
        {
            if (!activo)
            {
                rbBabosa.linearVelocity = Vector2.zero;
            }

            rbBabosa.WakeUp();
        }
    }

    public void BloquearInput(float duracion)
    {
        _inputCooldown = Time.time + duracion;
    }

    public void SetOutlineActivo(bool activo)
    {
        if (_outlineRenderer != null)
        {
            _outlineRenderer.enabled = activo;
        }
    }

    public void ResetearEstado()
    {
        colgadaActualmente = false;
        conectadaMantarraya = false;
        EstaMontada = false;

        cooldownEnganche = 0f;
        FueLanzadaPorGeiser = false;
        _inputHFisica = 0f;

        if (agarreActual != null)
        {
            Destroy(agarreActual);
            agarreActual = null;
        }

        pulpoCuerdaActual = null;

        if (miAnimator != null)
        {
            miAnimator.SetBool(EstaEnganchadaHash, false);
            miAnimator.SetBool(
                EstaConectadaMantarrayaHash,
                false
            );
        }

        if (rbBabosa != null)
        {
            rbBabosa.linearVelocity = Vector2.zero;
        }
    }

    public void ConectarConMantarraya()
    {
        conectadaMantarraya = true;
        EstaMontada = true;

        /*
         * Apagamos el estado del pulpo para que las dos
         * animaciones no compitan entre sí.
         */
        colgadaActualmente = false;
        _inputHFisica = 0f;

        if (agarreActual != null)
        {
            Destroy(agarreActual);
            agarreActual = null;
        }

        pulpoCuerdaActual = null;

        if (miAnimator != null)
        {
            miAnimator.SetBool(EstaEnganchadaHash, false);

            miAnimator.SetBool(
                EstaConectadaMantarrayaHash,
                true
            );
        }

        Debug.Log(
            "<color=cyan>Babosa conectada con la mantarraya.</color>",
            gameObject
        );
    }

    public void DesconectarDeMantarraya()
    {
        conectadaMantarraya = false;
        EstaMontada = false;

        if (miAnimator != null)
        {
            miAnimator.SetBool(
                EstaConectadaMantarrayaHash,
                false
            );
        }

        Debug.Log(
            "<color=cyan>Babosa desconectada de la mantarraya.</color>",
            gameObject
        );
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!FueLanzadaPorGeiser)
        {
            return;
        }

        foreach (ContactPoint2D contacto in col.contacts)
        {
            if (contacto.normal.y > 0.5f)
            {
                FueLanzadaPorGeiser = false;

                GameManager gameManager =
                    FindFirstObjectByType<GameManager>();

                if (gameManager != null)
                {
                    gameManager.MuerteYRespawnCooperativo();
                }

                return;
            }
        }
    }

    private void Update()
    {
        if (!estaControlado || EstaMontada)
        {
            return;
        }

        if (Time.time < _inputCooldown)
        {
            return;
        }

        if (rbBabosa == null)
        {
            return;
        }

        float inputH = 0f;

        if (Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow))
        {
            inputH = 1f;
        }

        if (Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow))
        {
            inputH = -1f;
        }

        if (!colgadaActualmente)
        {
            rbBabosa.linearVelocity = new Vector2(
                inputH * velocidadSuelo,
                rbBabosa.linearVelocity.y
            );
        }
        else
        {
            _inputHFisica = inputH;

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

    private void FixedUpdate()
    {
        if (EstaMontada)
        {
            return;
        }

        if (colgadaActualmente &&
            rbBabosa != null &&
            _inputHFisica != 0f)
        {
            rbBabosa.AddForce(
                new Vector2(
                    _inputHFisica * fuerzaFisicaBalanceo,
                    0f
                ),
                ForceMode2D.Force
            );
        }
    }

    private void LateUpdate()
    {
        if (_outlineRenderer == null ||
            !_outlineRenderer.enabled ||
            miSprite == null)
        {
            return;
        }

        _outlineRenderer.sprite = miSprite.sprite;
        _outlineRenderer.flipX = miSprite.flipX;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        /*
         * Si está conectada con la mantarraya,
         * no puede engancharse al pulpo.
         */
        if (colgadaActualmente ||
            conectadaMantarraya ||
            EstaMontada ||
            Time.time < cooldownEnganche)
        {
            return;
        }

        PulpoColumpio pulpoDetectado =
            other.GetComponentInParent<PulpoColumpio>();

        if (pulpoDetectado == null ||
            !pulpoDetectado.IsTentaculoDesplegado())
        {
            return;
        }

        GameObject puntaTentaculo =
            pulpoDetectado.ObtenerPuntaTentaculo();

        if (puntaTentaculo == null ||
            other.gameObject != puntaTentaculo)
        {
            return;
        }

        Rigidbody2D rbPunta =
            other.GetComponent<Rigidbody2D>();

        if (rbPunta == null)
        {
            return;
        }

        colgadaActualmente = true;
        conectadaMantarraya = false;
        pulpoCuerdaActual = pulpoDetectado;

        agarreActual =
            gameObject.AddComponent<HingeJoint2D>();

        agarreActual.connectedBody = rbPunta;
        agarreActual.useLimits = true;

        JointAngleLimits2D limitesSemicirculo =
            new JointAngleLimits2D
            {
                min = -90f,
                max = 90f
            };

        agarreActual.limits = limitesSemicirculo;

        if (miAnimator != null)
        {
            miAnimator.SetBool(
                EstaConectadaMantarrayaHash,
                false
            );

            miAnimator.SetBool(
                EstaEnganchadaHash,
                true
            );
        }

        if (rbBabosa != null)
        {
            rbBabosa.linearVelocity = new Vector2(
                rbBabosa.linearVelocity.x * 1.2f,
                4f
            );
        }

        Debug.Log(
            "Enganchado dinámicamente al pulpo: " +
            pulpoDetectado.name
        );

        GameManager gameManager =
            FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.ReproducirSonidoEnganche();
        }
    }

    private void SoltarYSalirDisparada()
    {
        colgadaActualmente = false;
        cooldownEnganche = Time.time + 0.5f;
        _inputHFisica = 0f;

        if (miAnimator != null)
        {
            miAnimator.SetBool(
                EstaEnganchadaHash,
                false
            );
        }

        if (agarreActual != null)
        {
            Destroy(agarreActual);
            agarreActual = null;
        }

        pulpoCuerdaActual = null;

        if (rbBabosa != null)
        {
            float direccionInercia =
                rbBabosa.linearVelocity.x >= 0f
                    ? 1f
                    : -1f;

            rbBabosa.linearVelocity = new Vector2(
                rbBabosa.linearVelocity.x * 0.7f,
                0f
            );

            rbBabosa.AddForce(
                new Vector2(
                    direccionInercia * fuerzaSaltoX,
                    fuerzaSaltoY
                ),
                ForceMode2D.Impulse
            );
        }

        ReproducirSonidoSaltoBabosa();
    }

    private void ReproducirSonidoSaltoBabosa()
    {
        if (miLectorDeAudio == null ||
            sonidosSaltoBabosa == null ||
            sonidosSaltoBabosa.Length == 0)
        {
            return;
        }

        int indiceAleatorio =
            Random.Range(0, sonidosSaltoBabosa.Length);

        AudioClip clipSeleccionado =
            sonidosSaltoBabosa[indiceAleatorio];

        if (clipSeleccionado == null)
        {
            return;
        }

        miLectorDeAudio.PlayOneShot(clipSeleccionado);

        Debug.Log(
            "<color=green>¡Babosa: sonido de impulso/desenganche reproducido!</color>"
        );
    }
}