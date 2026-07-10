using UnityEngine;
using UnityEngine.SceneManagement;

public class DetectorColisiones : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameManager == null) return;

        // 1. Checkpoint
        if (collision.CompareTag("Checkpoint"))
        {
            if (!gameObject.CompareTag("Player"))
                return;

            ObjetoCheckpoint1 datos = collision.GetComponent<ObjetoCheckpoint1>();

            if (datos != null && datos.esElUltimo)
            {
                // Último checkpoint — ir al siguiente nivel
                PlayerPrefs.SetString("LastScene", datos.escenaDestino);
                PlayerPrefs.SetInt("SaveExists", 1);
                PlayerPrefs.DeleteKey("CheckpointX");
                PlayerPrefs.DeleteKey("CheckpointY");
                PlayerPrefs.DeleteKey("HasExitPos");
                PlayerPrefs.Save();
                SceneManager.LoadScene(datos.escenaDestino);
                return;
            }

            // Checkpoint normal — guardar posición
            gameManager.GuardarNuevoCheckpoint(collision.transform.position);

            Animator animBurbuja = collision.GetComponent<Animator>();
            if (animBurbuja != null)
                animBurbuja.Play("Burbuja_Explotar");

            collision.enabled = false;
        }

        // 2. Zona de muerte
        if (collision.CompareTag("DeadZone"))
        {
            gameManager.MuerteYRespawnCooperativo();
        }
    }
}
