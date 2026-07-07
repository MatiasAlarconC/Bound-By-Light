using UnityEngine;

public class Geiser : MonoBehaviour
{
    [SerializeField] private float fuerza = 20f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(Vector2.up * fuerza * Time.deltaTime, ForceMode2D.Force);
        }
    }
}