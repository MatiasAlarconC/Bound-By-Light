using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjetoCheckpoint1 : MonoBehaviour
{
    [SerializeField] public bool esElUltimo = false;
    [SerializeField] public string escenaDestino = "Nivel2";
    [SerializeField] private float radioDeteccion = 1.5f;

    private bool _activado = false;
    private Animator _miAnimator;

    private void Start()
    {
        _miAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!esElUltimo || _activado) return;

        // Detectar cualquier collider dentro del radio sin depender de capas de física
        Collider2D[] cercanos = Physics2D.OverlapCircleAll(transform.position, radioDeteccion);
        foreach (Collider2D col in cercanos)
        {
            if (col.gameObject == gameObject) continue;
            if (col.CompareTag("Player") || col.CompareTag("Babosa"))
            {
                _activado = true;
                PlayerPrefs.SetString("LastScene", escenaDestino);
                PlayerPrefs.SetInt("SaveExists", 1);
                PlayerPrefs.DeleteKey("CheckpointX");
                PlayerPrefs.DeleteKey("CheckpointY");
                PlayerPrefs.DeleteKey("HasExitPos");
                PlayerPrefs.Save();
                SceneManager.LoadScene(escenaDestino);
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (esElUltimo || _activado) return;
        if (!other.CompareTag("Player") && !other.CompareTag("Babosa")) return;

        _activado = true;
        GameManager elCerebro = FindFirstObjectByType<GameManager>();
        if (elCerebro != null)
            elCerebro.GuardarNuevoCheckpoint(transform.position, _miAnimator);
    }

    private void OnDrawGizmosSelected()
    {
        if (!esElUltimo) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
