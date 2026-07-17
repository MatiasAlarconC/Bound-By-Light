using UnityEngine;

public class MantarrayaControl : MonoBehaviour
{
    [Header("Montura")]
    [SerializeField] private float distanciaMaxima = 4f;
    [SerializeField] private Vector2 offsetBabosa = new Vector2(0f, 2.5f);

    [Header("Indicador visual (opcional)")]
    [SerializeField] private Sprite spriteIndicador;

    [Tooltip("Color del indicador si no hay sprite asignado")]
    [SerializeField] private Color colorIndicador =
        new Color(0.2f, 0.8f, 1f, 0.9f);

    [Header("Sonidos")]
    [Tooltip("Suena una vez al momento de la transformación")]
    [SerializeField] private AudioClip sonidoTransformacion;

    [Tooltip("Suena cuando el Angel salta estando transformado")]
    [SerializeField] private AudioClip sonidoVuelo;

    [Tooltip("Suena una vez al destransformarse")]
    [SerializeField] private AudioClip sonidoDestransformacion;

    private AudioSource audioSource;

    private BabosaControl babosa;
    private Rigidbody2D rbBabosa;
    private Collider2D colBabosa;
    private SpriteRenderer srBabosa;
    private Transform padreOriginalBabosa;
    private int sortingOrderOriginalBabosa;
    private Animator animatorAngel;

    private bool montado = false;
    private bool montando = false;

    private GameObject indicadorGO;
    private SpriteRenderer indicadorSR;

    private void Start()
    {
        babosa = FindFirstObjectByType<BabosaControl>();

        if (babosa == null)
        {
            Debug.LogError(
                "MantarrayaControl: No se encontró un objeto con BabosaControl.",
                gameObject
            );

            return;
        }

        rbBabosa = babosa.GetComponent<Rigidbody2D>();
        colBabosa = babosa.GetComponent<Collider2D>();
        srBabosa = babosa.GetComponent<SpriteRenderer>();

        padreOriginalBabosa = babosa.transform.parent;

        if (srBabosa != null)
        {
            sortingOrderOriginalBabosa =
                srBabosa.sortingOrder;
        }

        animatorAngel = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        CrearIndicador();
    }

    private void Update()
    {
        if (babosa == null)
        {
            return;
        }

        /*
         * Si el combinado está activo y la babosa
         * está siendo controlada, el salto desmonta.
         */
        if (montado && Input.GetButtonDown("Jump"))
        {
            if (babosa.EstaControlado)
            {
                Desmontar();
                return;
            }

            // Sonido de movimiento/salto del combinado.
            if (audioSource != null &&
                sonidoVuelo != null)
            {
                audioSource.PlayOneShot(sonidoVuelo);
            }
        }

        if (!Input.GetKeyDown(KeyCode.Z))
        {
            return;
        }

        if (!montado)
        {
            float distancia = Vector2.Distance(
                transform.position,
                babosa.transform.position
            );

            if (distancia <= distanciaMaxima)
            {
                /*
                 * Mantiene la condición original:
                 * solo permite montar cuando está
                 * dentro del rango de un géiser.
                 */
                if (!GeiserControl.AngelEnRangoDeAlgunGeiser(
                        babosa.transform.position))
                {
                    return;
                }

                Montar();
            }
        }
        else
        {
            Desmontar();
        }
    }

    private void Montar()
    {
        if (babosa == null || montado)
        {
            return;
        }

        montado = true;
        montando = true;

        /*
         * Esta llamada activa:
         * estaConectadaMantarraya = true
         *
         * Eso provoca la transición hacia
         * Babosa_mantaraya en el Animator.
         */
        babosa.ConectarConMantarraya();

        if (rbBabosa != null)
        {
            rbBabosa.linearVelocity = Vector2.zero;
            rbBabosa.bodyType =
                RigidbodyType2D.Kinematic;
        }

        // Ignorar barreras de géiser.
        PulpoColumpio pulpoControl =
            FindFirstObjectByType<PulpoColumpio>();

        Collider2D physicsCol = null;

        if (pulpoControl != null)
        {
            physicsCol =
                pulpoControl.GetPhysicsCollider();

            pulpoControl.SetModoCombinado(true);
        }

        foreach (
            Collider2D barrera
            in GeiserControl.todasLasBarreras
        )
        {
            if (barrera == null)
            {
                continue;
            }

            if (colBabosa != null)
            {
                Physics2D.IgnoreCollision(
                    colBabosa,
                    barrera,
                    true
                );
            }

            if (physicsCol != null)
            {
                Physics2D.IgnoreCollision(
                    physicsCol,
                    barrera,
                    true
                );
            }

            foreach (
                Collider2D colliderMantarraya
                in GetComponents<Collider2D>()
            )
            {
                if (colliderMantarraya != null)
                {
                    Physics2D.IgnoreCollision(
                        colliderMantarraya,
                        barrera,
                        true
                    );
                }
            }
        }

        if (animatorAngel != null)
        {
            animatorAngel.SetBool(
                "estaMontado",
                true
            );
        }

        if (indicadorGO != null)
        {
            indicadorGO.SetActive(true);
        }

        if (audioSource != null &&
            sonidoTransformacion != null)
        {
            audioSource.PlayOneShot(
                sonidoTransformacion
            );
        }

        PulpoColumpio pulpo =
            FindFirstObjectByType<PulpoColumpio>();

        if (pulpo != null)
        {
            pulpo.SetMaxSaltosEnModo(1);
            pulpo.SetEsMantarraya(true);
        }

        GameManager gm =
            FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            gm.SetModoCombinado(true);
        }

        StartCoroutine(LerpMontar());

        Debug.Log(
            "<color=cyan>Mantarraya: conexión iniciada y animación de la babosa activada.</color>",
            gameObject
        );
    }

    private System.Collections.IEnumerator LerpMontar()
    {
        if (babosa == null)
        {
            montando = false;
            yield break;
        }

        Vector3 posicionInicial =
            babosa.transform.position;

        float duracion = 0.75f;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion &&
               montado)
        {
            tiempoTranscurrido += Time.deltaTime;

            float t = Mathf.Clamp01(
                tiempoTranscurrido / duracion
            );

            // Movimiento suave.
            t = t * t * (3f - 2f * t);

            Vector3 posicionObjetivo =
                transform.TransformPoint(
                    new Vector3(
                        offsetBabosa.x,
                        offsetBabosa.y,
                        0f
                    )
                );

            babosa.transform.position =
                Vector3.Lerp(
                    posicionInicial,
                    posicionObjetivo,
                    t
                );

            yield return null;
        }

        if (montado && babosa != null)
        {
            babosa.transform.SetParent(transform);

            babosa.transform.localPosition =
                new Vector3(
                    offsetBabosa.x,
                    offsetBabosa.y,
                    0f
                );

            SpriteRenderer srAngel =
                GetComponent<SpriteRenderer>();

            if (srBabosa != null &&
                srAngel != null)
            {
                srBabosa.sortingOrder =
                    srAngel.sortingOrder + 2;
            }
        }

        montando = false;
    }

    public void Desmontar()
    {
        if (babosa == null)
        {
            return;
        }

        if (!montado &&
            !babosa.EstaMontada)
        {
            return;
        }

        montado = false;
        montando = false;

        StopAllCoroutines();

        /*
         * Esta llamada coloca:
         * estaConectadaMantarraya = false
         *
         * El Animator debe regresar a Babosa_Idle.
         */
        babosa.DesconectarDeMantarraya();

        babosa.transform.SetParent(
            padreOriginalBabosa
        );

        if (srBabosa != null)
        {
            srBabosa.sortingOrder =
                sortingOrderOriginalBabosa;
        }

        if (rbBabosa != null)
        {
            rbBabosa.bodyType =
                RigidbodyType2D.Dynamic;
        }

        PulpoColumpio pulpoControl =
            FindFirstObjectByType<PulpoColumpio>();

        Collider2D physicsCol = null;

        if (pulpoControl != null)
        {
            physicsCol =
                pulpoControl.GetPhysicsCollider();

            pulpoControl.SetModoCombinado(false);
        }

        foreach (
            Collider2D barrera
            in GeiserControl.todasLasBarreras
        )
        {
            if (barrera == null)
            {
                continue;
            }

            if (colBabosa != null)
            {
                Physics2D.IgnoreCollision(
                    colBabosa,
                    barrera,
                    false
                );
            }

            if (physicsCol != null)
            {
                Physics2D.IgnoreCollision(
                    physicsCol,
                    barrera,
                    false
                );
            }

            foreach (
                Collider2D colliderMantarraya
                in GetComponents<Collider2D>()
            )
            {
                if (colliderMantarraya != null)
                {
                    Physics2D.IgnoreCollision(
                        colliderMantarraya,
                        barrera,
                        false
                    );
                }
            }
        }

        if (animatorAngel != null)
        {
            animatorAngel.SetBool(
                "estaMontado",
                false
            );
        }

        if (indicadorGO != null)
        {
            indicadorGO.SetActive(false);
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;

            if (sonidoDestransformacion != null)
            {
                audioSource.PlayOneShot(
                    sonidoDestransformacion
                );
            }
        }

        PulpoColumpio pulpo =
            FindFirstObjectByType<PulpoColumpio>();

        if (pulpo != null)
        {
            pulpo.SetMaxSaltosEnModo(2);
            pulpo.SetEsMantarraya(false);
        }

        GameManager gm =
            FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            gm.SetModoCombinado(false);
        }

        Debug.Log(
            "<color=cyan>Mantarraya: conexión terminada y babosa devuelta a Idle.</color>",
            gameObject
        );
    }

    /*
     * Llamado desde GameManager al morir.
     * Separa los personajes sin provocar otra muerte.
     */
    public void ForceDetach()
    {
        if (babosa == null)
        {
            return;
        }

        if (!montado &&
            !babosa.EstaMontada)
        {
            /*
             * Incluso si los estados físicos dicen que
             * no está montada, apagamos la animación
             * por seguridad.
             */
            babosa.DesconectarDeMantarraya();
            return;
        }

        montado = false;
        montando = false;

        StopAllCoroutines();

        /*
         * Evita que Babosa_mantaraya quede activa
         * después de morir o reaparecer.
         */
        babosa.DesconectarDeMantarraya();

        babosa.transform.SetParent(
            padreOriginalBabosa
        );

        if (srBabosa != null)
        {
            srBabosa.sortingOrder =
                sortingOrderOriginalBabosa;
        }

        if (rbBabosa != null)
        {
            rbBabosa.bodyType =
                RigidbodyType2D.Dynamic;

            rbBabosa.linearVelocity =
                Vector2.zero;
        }

        PulpoColumpio pulpoControl =
            FindFirstObjectByType<PulpoColumpio>();

        Collider2D physicsCol = null;

        if (pulpoControl != null)
        {
            physicsCol =
                pulpoControl.GetPhysicsCollider();

            pulpoControl.SetModoCombinado(false);
        }

        foreach (
            Collider2D barrera
            in GeiserControl.todasLasBarreras
        )
        {
            if (barrera == null)
            {
                continue;
            }

            if (colBabosa != null)
            {
                Physics2D.IgnoreCollision(
                    colBabosa,
                    barrera,
                    false
                );
            }

            if (physicsCol != null)
            {
                Physics2D.IgnoreCollision(
                    physicsCol,
                    barrera,
                    false
                );
            }

            foreach (
                Collider2D colliderMantarraya
                in GetComponents<Collider2D>()
            )
            {
                if (colliderMantarraya != null)
                {
                    Physics2D.IgnoreCollision(
                        colliderMantarraya,
                        barrera,
                        false
                    );
                }
            }
        }

        if (animatorAngel != null)
        {
            animatorAngel.SetBool(
                "estaMontado",
                false
            );
        }

        if (indicadorGO != null)
        {
            indicadorGO.SetActive(false);
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;

            if (sonidoDestransformacion != null)
            {
                audioSource.PlayOneShot(
                    sonidoDestransformacion
                );
            }
        }

        PulpoColumpio pulpo =
            FindFirstObjectByType<PulpoColumpio>();

        if (pulpo != null)
        {
            pulpo.SetMaxSaltosEnModo(2);
            pulpo.SetEsMantarraya(false);
        }

        GameManager gm =
            FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            gm.SetModoCombinado(false);
        }
    }

    private void CrearIndicador()
    {
        indicadorGO =
            new GameObject("IndicadorMantarraya");

        indicadorGO.transform.SetParent(transform);

        indicadorGO.transform.localPosition =
            new Vector3(0f, 1.8f, 0f);

        indicadorGO.transform.localScale =
            Vector3.one * 0.5f;

        indicadorSR =
            indicadorGO.AddComponent<SpriteRenderer>();

        indicadorSR.sortingLayerName = "Player";
        indicadorSR.sortingOrder = 10;
        indicadorSR.color = colorIndicador;

        if (spriteIndicador != null)
        {
            indicadorSR.sprite = spriteIndicador;
        }
        else
        {
            Texture2D textura =
                new Texture2D(32, 32);

            Color[] pixeles =
                new Color[32 * 32];

            Vector2 centro =
                new Vector2(16f, 16f);

            for (int i = 0;
                 i < pixeles.Length;
                 i++)
            {
                int x = i % 32;
                int y = i / 32;

                float distancia =
                    Vector2.Distance(
                        new Vector2(x, y),
                        centro
                    );

                pixeles[i] =
                    distancia < 14f
                        ? colorIndicador
                        : Color.clear;
            }

            textura.SetPixels(pixeles);
            textura.Apply();

            indicadorSR.sprite =
                Sprite.Create(
                    textura,
                    new Rect(0, 0, 32, 32),
                    new Vector2(0.5f, 0.5f)
                );
        }

        indicadorGO.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            distanciaMaxima
        );
    }
}