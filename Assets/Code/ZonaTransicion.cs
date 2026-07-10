using UnityEngine;
using UnityEngine.SceneManagement;

public class ZonaTransicion : MonoBehaviour
{
    [Header("Configuracion de la Transicion")]
    public string tagDelJugador = "Player";
    public string nombreEscenaSiguiente = "Escena_Cinematica2";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagDelJugador))
        {
            Debug.Log("¡Meta alcanzada! Cargando cinematica...");
            SceneManager.LoadScene(nombreEscenaSiguiente);
        }
    }
}
