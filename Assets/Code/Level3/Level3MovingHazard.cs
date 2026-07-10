using UnityEngine;
using UnityEngine.SceneManagement;

public class Level3MovingHazard : MonoBehaviour
{
    public float distanciaHorizontal = 4f;
    public float velocidad = 1.6f;
    public float fase = 0f;

    private Vector3 puntoInicial;
    private Rigidbody2D rb;
    private bool reiniciando;

    private void Awake()
    {
        puntoInicial = transform.position;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        float offsetX = Mathf.Sin((Time.time * velocidad) + fase) * distanciaHorizontal;
        Vector2 destino = new Vector2(puntoInicial.x + offsetX, puntoInicial.y);

        if (rb != null)
        {
            rb.MovePosition(destino);
        }
        else
        {
            transform.position = destino;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryRestart(other.transform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryRestart(collision.transform);
    }

    private void TryRestart(Transform other)
    {
        if (reiniciando || other == null) return;
        if (!IsLevel3PlayerObject(other)) return;

        reiniciando = true;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private bool IsLevel3PlayerObject(Transform other)
    {
        Transform current = other;
        while (current != null)
        {
            string objectName = current.name;
            if (objectName.Contains("Babosa") ||
                objectName.Contains("Mantarraya") ||
                objectName.Contains("Pulpo") ||
                objectName.Contains("Hermano"))
            {
                return true;
            }

            if (current.CompareTag("Player") ||
                current.CompareTag("Babosa") ||
                current.CompareTag("Pulpo"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}
