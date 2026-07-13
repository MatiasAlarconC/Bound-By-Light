using UnityEngine;

// Adjuntar a un GameObject con BoxCollider2D trigger posicionado bajo el nivel.
// Mata a cualquier personaje que caiga ahí y respawnea en el último checkpoint.
public class ZonaMuerte : MonoBehaviour
{
    private GameManager gm;

    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();

        // Asegurar que el collider sea trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Babosa") || other.CompareTag("Pulpo"))
        {
            if (gm == null) gm = FindFirstObjectByType<GameManager>();
            gm?.MuerteYRespawnCooperativo();
        }
    }
}
