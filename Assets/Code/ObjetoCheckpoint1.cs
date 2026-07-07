using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjetoCheckpoint1 : MonoBehaviour
{
    [SerializeField] private bool esElUltimo = false;
    [SerializeField] private string escenaDestino = "Nivel2";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (esElUltimo)
        {
            SceneManager.LoadScene(escenaDestino);
            return;
        }

        GameManager elCerebro = FindFirstObjectByType<GameManager>();
        if (elCerebro != null)
        {
            Animator miAnimator = GetComponent<Animator>();
            elCerebro.GuardarNuevoCheckpoint(transform.position, miAnimator);
        }
    }
}
