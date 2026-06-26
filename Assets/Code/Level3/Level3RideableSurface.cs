using UnityEngine;

/// <summary>
/// Parents the babosa to a moving creature/platform while she is standing on it.
/// This keeps the prototype stable for the manta ray, whale and turtle rides.
/// </summary>
public class Level3RideableSurface : MonoBehaviour
{
    [SerializeField] private string babosaTag = "Player";
    [SerializeField] private Transform mountRoot;
    [SerializeField] private Vector2 riderWorldOffset = new Vector2(0f, 0.8f);
    [SerializeField] private bool snapRiderOnForceAttach = true;
    [SerializeField] private bool snapRiderOnCollisionAttach = false;
    [SerializeField] private bool lockRiderUntilReleased = false;
    [SerializeField] private bool snapLockedRiderEveryFrame = false;

    public Transform CurrentRider { get; private set; }
    public bool HasRider => CurrentRider != null;

    private void Reset()
    {
        mountRoot = transform;
    }

    private void Awake()
    {
        if (mountRoot == null) mountRoot = transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryAttach(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryAttach(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (lockRiderUntilReleased) return;

        if (CurrentRider != null && collision.transform == CurrentRider)
        {
            CurrentRider.SetParent(null, true);
            CurrentRider = null;
        }
    }

    private void LateUpdate()
    {
        if (CurrentRider == null || !lockRiderUntilReleased) return;

        if (snapLockedRiderEveryFrame)
        {
            SnapRiderToMount(CurrentRider);
        }

        CurrentRider.SetParent(mountRoot, true);
    }

    private void TryAttach(Collider2D other)
    {
        if (!other.CompareTag(babosaTag)) return;

        CurrentRider = other.transform;
        if (snapRiderOnCollisionAttach)
        {
            SnapRiderToMount(CurrentRider);
        }
        CurrentRider.SetParent(mountRoot, true);
    }

    public void ForceAttach(Transform rider)
    {
        if (rider == null) return;
        CurrentRider = rider;
        if (snapRiderOnForceAttach)
        {
            SnapRiderToMount(CurrentRider);
        }
        CurrentRider.SetParent(mountRoot, true);
    }

    public void ForceAttachAndLock(Transform rider)
    {
        lockRiderUntilReleased = true;
        snapLockedRiderEveryFrame = true;
        ForceAttach(rider);
    }

    public void ConfigureMountOffset(
        Vector2 offset,
        bool snapOnForce = true,
        bool snapOnCollision = false,
        bool lockRider = false,
        bool snapLockedEveryFrame = false)
    {
        riderWorldOffset = offset;
        snapRiderOnForceAttach = snapOnForce;
        snapRiderOnCollisionAttach = snapOnCollision;
        lockRiderUntilReleased = lockRider;
        snapLockedRiderEveryFrame = snapLockedEveryFrame;
    }

    private void SnapRiderToMount(Transform rider)
    {
        if (rider == null || mountRoot == null) return;

        rider.position = mountRoot.position + new Vector3(riderWorldOffset.x, riderWorldOffset.y, 0f);

        Rigidbody2D rb = rider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void Detach()
    {
        if (lockRiderUntilReleased) return;

        ForceDetach();
    }

    public void ForceDetach()
    {
        if (CurrentRider == null) return;
        CurrentRider.SetParent(null, true);
        CurrentRider = null;
    }

    public void ReleaseLock()
    {
        lockRiderUntilReleased = false;
        snapLockedRiderEveryFrame = false;
    }
}
