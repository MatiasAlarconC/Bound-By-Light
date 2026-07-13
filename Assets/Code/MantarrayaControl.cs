using UnityEngine;

public class MantarrayaControl : MonoBehaviour
{
    [Header("Montura")]
    [SerializeField] private float distanciaMaxima = 4f;
    [SerializeField] private Vector2 offsetBabosa  = new Vector2(0f, 2.5f);

    [Header("Indicador visual (opcional)")]
    [SerializeField] private Sprite spriteIndicador;
    [Tooltip("Color del indicador si no hay sprite asignado")]
    [SerializeField] private Color colorIndicador = new Color(0.2f, 0.8f, 1f, 0.9f);

    private BabosaControl babosa;
    private Rigidbody2D  rbBabosa;
    private Collider2D   colBabosa;
    private SpriteRenderer srBabosa;
    private Transform    padreOriginalBabosa;
    private int          sortingOrderOriginalBabosa;
    private Animator     animatorAngel;

    private bool montado   = false;
    private bool montando  = false;   // true mientras Babosa sube flotando
    private GameObject indicadorGO;
    private SpriteRenderer indicadorSR;

    void Start()
    {
        babosa    = FindFirstObjectByType<BabosaControl>();
        if (babosa == null) return;

        rbBabosa  = babosa.GetComponent<Rigidbody2D>();
        colBabosa = babosa.GetComponent<Collider2D>();
        srBabosa  = babosa.GetComponent<SpriteRenderer>();
        padreOriginalBabosa = babosa.transform.parent;
        if (srBabosa != null) sortingOrderOriginalBabosa = srBabosa.sortingOrder;

        animatorAngel = GetComponent<Animator>();

        CrearIndicador();
    }

    void Update()
    {
        if (babosa == null) return;

        // Salto desmonta cuando la Babosa tiene el control
        if (montado && babosa.EstaControlado && Input.GetButtonDown("Jump"))
        {
            Desmontar();
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Z)) return;

        if (!montado)
        {
            float dist = Vector2.Distance(transform.position, babosa.transform.position);
            if (dist <= distanciaMaxima)
                Montar();
        }
        else
        {
            Desmontar();
        }
    }

    void Montar()
    {
        montado = true;
        babosa.EstaMontada = true;

        if (rbBabosa != null)
        {
            rbBabosa.linearVelocity = Vector2.zero;
            rbBabosa.bodyType = RigidbodyType2D.Kinematic;
        }

        // Ignorar barreras de geiser: collider físico de PulpoColumpio + colBabosa
        PulpoColumpio pulpoControl = FindFirstObjectByType<PulpoColumpio>();
        Collider2D physicsCol = pulpoControl?.GetPhysicsCollider();
        pulpoControl?.SetModoCombinado(true);

        foreach (Collider2D barrera in GeiserControl.todasLasBarreras)
        {
            if (barrera == null) continue;
            if (colBabosa   != null) Physics2D.IgnoreCollision(colBabosa, barrera, true);
            if (physicsCol  != null) Physics2D.IgnoreCollision(physicsCol, barrera, true);
            foreach (Collider2D ca in GetComponents<Collider2D>())
                if (ca != null) Physics2D.IgnoreCollision(ca, barrera, true);
        }

        if (animatorAngel != null) animatorAngel.SetBool("estaMontado", true);
        if (indicadorGO != null) indicadorGO.SetActive(true);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null) gm.SetModoCombinado(true);

        StartCoroutine(LerpMontar());
    }

    System.Collections.IEnumerator LerpMontar()
    {
        montando = true;
        Vector3 posInicial = babosa.transform.position;
        float duracion = 0.75f;
        float elapsed  = 0f;

        while (elapsed < duracion && montado)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracion);
            t = t * t * (3f - 2f * t);   // smoothstep: acelera y desacelera suave
            Vector3 objetivo = transform.TransformPoint(new Vector3(offsetBabosa.x, offsetBabosa.y, 0f));
            babosa.transform.position = Vector3.Lerp(posInicial, objetivo, t);
            yield return null;
        }

        if (montado)
        {
            babosa.transform.SetParent(transform);
            babosa.transform.localPosition = new Vector3(offsetBabosa.x, offsetBabosa.y, 0f);
            SpriteRenderer srAngel = GetComponent<SpriteRenderer>();
            if (srBabosa != null && srAngel != null)
                srBabosa.sortingOrder = srAngel.sortingOrder + 2;
        }

        montando = false;
    }

    void Desmontar()
    {
        montado  = false;
        montando = false;
        babosa.EstaMontada = false;

        babosa.transform.SetParent(padreOriginalBabosa);
        if (srBabosa != null) srBabosa.sortingOrder = sortingOrderOriginalBabosa;
        if (rbBabosa != null) rbBabosa.bodyType = RigidbodyType2D.Dynamic;

        PulpoColumpio pulpoControl = FindFirstObjectByType<PulpoColumpio>();
        Collider2D physicsCol = pulpoControl?.GetPhysicsCollider();
        pulpoControl?.SetModoCombinado(false);

        foreach (Collider2D barrera in GeiserControl.todasLasBarreras)
        {
            if (barrera == null) continue;
            if (colBabosa  != null) Physics2D.IgnoreCollision(colBabosa, barrera, false);
            if (physicsCol != null) Physics2D.IgnoreCollision(physicsCol, barrera, false);
            foreach (Collider2D ca in GetComponents<Collider2D>())
                if (ca != null) Physics2D.IgnoreCollision(ca, barrera, false);
        }

        if (animatorAngel != null) animatorAngel.SetBool("estaMontado", false);
        if (indicadorGO != null) indicadorGO.SetActive(false);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null) gm.SetModoCombinado(false);
    }

    // Llamado desde GameManager al morir — separa sin llamar de vuelta a GameManager
    public void ForceDetach()
    {
        if (!montado && babosa != null && !babosa.EstaMontada) return;
        montado  = false;
        montando = false;
        if (babosa == null) return;

        babosa.EstaMontada = false;
        babosa.transform.SetParent(padreOriginalBabosa);
        if (srBabosa != null) srBabosa.sortingOrder = sortingOrderOriginalBabosa;
        if (rbBabosa != null) rbBabosa.bodyType = RigidbodyType2D.Dynamic;

        PulpoColumpio pulpoControl = FindFirstObjectByType<PulpoColumpio>();
        Collider2D physicsCol = pulpoControl?.GetPhysicsCollider();
        pulpoControl?.SetModoCombinado(false);

        foreach (Collider2D barrera in GeiserControl.todasLasBarreras)
        {
            if (barrera == null) continue;
            if (colBabosa  != null) Physics2D.IgnoreCollision(colBabosa, barrera, false);
            if (physicsCol != null) Physics2D.IgnoreCollision(physicsCol, barrera, false);
            foreach (Collider2D ca in GetComponents<Collider2D>())
                if (ca != null) Physics2D.IgnoreCollision(ca, barrera, false);
        }

        if (animatorAngel != null) animatorAngel.SetBool("estaMontado", false);
        if (indicadorGO != null) indicadorGO.SetActive(false);
    }

    void CrearIndicador()
    {
        indicadorGO = new GameObject("IndicadorMantarraya");
        indicadorGO.transform.SetParent(transform);
        indicadorGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        indicadorGO.transform.localScale    = Vector3.one * 0.5f;

        indicadorSR = indicadorGO.AddComponent<SpriteRenderer>();
        indicadorSR.sortingLayerName = "Player";
        indicadorSR.sortingOrder     = 10;
        indicadorSR.color            = colorIndicador;

        if (spriteIndicador != null)
        {
            indicadorSR.sprite = spriteIndicador;
        }
        else
        {
            // Crear un sprite de círculo simple como indicador
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            Vector2 center = new Vector2(16f, 16f);
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 32, y = i / 32;
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[i] = dist < 14f ? colorIndicador : Color.clear;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            indicadorSR.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        indicadorGO.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaMaxima);
    }
}
