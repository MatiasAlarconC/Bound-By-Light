using UnityEngine;

/// <summary>
/// Level 3 only helper: lets the babosa climb/jump through block platforms
/// without changing the original Level 1 BabosaControl script.
/// </summary>
public class Level3BabosaPlatformerAssist : MonoBehaviour
{
    public float fuerzaSalto = 8.6f;
    public float impulsoEscalada = 5.2f;
    public float distanciaSuelo = 0.78f;
    public float distanciaBloqueLateral = 0.55f;
    public float enfriamientoSalto = 0.16f;
    public bool permitirEscaladaLateral = false;
    public LayerMask capasSolidas = ~0;

    private Rigidbody2D rb;
    private Collider2D col;
    private float siguienteSalto;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = Mathf.Max(rb.gravityScale, 2.4f);
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
    }

    private void Update()
    {
        if (rb == null) return;

        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        bool climbPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        if (jumpPressed && Time.time >= siguienteSalto && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            siguienteSalto = Time.time + enfriamientoSalto;
        }

        if (permitirEscaladaLateral && climbPressed && IsTouchingSideBlock())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, impulsoEscalada));
        }
    }

    private bool IsGrounded()
    {
        Vector2 origin = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;
        float halfWidth = col != null ? col.bounds.extents.x * 0.75f : 0.35f;
        float halfHeight = col != null ? col.bounds.extents.y : 0.45f;
        float rayDistance = halfHeight + distanciaSuelo * 0.35f;

        return RayHitsSolid(origin + Vector2.left * halfWidth, Vector2.down, rayDistance)
            || RayHitsSolid(origin, Vector2.down, rayDistance)
            || RayHitsSolid(origin + Vector2.right * halfWidth, Vector2.down, rayDistance);
    }

    private bool IsTouchingSideBlock()
    {
        Vector2 origin = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;
        float halfWidth = col != null ? col.bounds.extents.x : 0.4f;
        float rayDistance = halfWidth + distanciaBloqueLateral;

        return RayHitsSolid(origin, Vector2.left, rayDistance)
            || RayHitsSolid(origin, Vector2.right, rayDistance);
    }

    private bool RayHitsSolid(Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, capasSolidas);
        return hit.collider != null
            && hit.collider.attachedRigidbody != rb
            && !hit.collider.isTrigger;
    }
}
