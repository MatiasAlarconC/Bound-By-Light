using UnityEngine;

public class DetectorColisiones : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        // Buscamos el GameManager automáticamente en la escena al iniciar
        gameManager = Object.FindFirstObjectByType<GameManager>(); // En versiones antiguas de Unity usa: FindObjectOfType<GameManager>();
    }

    // Se ejecuta cuando el personaje entra en un objeto que es "Is Trigger"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameManager == null) return;

        // 1. Si chocamos con la burbuja checkpoint
        if (collision.CompareTag("Checkpoint"))
        {
            // Le enviamos la posición de la burbuja al GameManager para que la guarde
            gameManager.GuardarNuevoCheckpoint(collision.transform.position);

            // Buscamos la animación de la burbuja y la hacemos explotar
            Animator animBurbuja = collision.GetComponent<Animator>();
            if (animBurbuja != null)
            {
                animBurbuja.Play("Burbuja_Explotar");
            }

            // Desactivamos el colisionador de la burbuja para que no vuelva a procesarse
            collision.enabled = false;
        }

        // 2. Si chocamos con pinchos o vacío
        if (collision.CompareTag("DeadZone"))
        {
            // Le ordenamos al GameManager que active la reaparición doble
            gameManager.MuerteYRespawnCooperativo();
        }
    }
}
