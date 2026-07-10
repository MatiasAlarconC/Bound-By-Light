using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CargadorPorVideo : MonoBehaviour
{
    private VideoPlayer miVideoPlayer;
    
    [Header("Configuración")]
    [Tooltip("Escribe el nombre exacto de la escena a la que irá al terminar")]
    [SerializeField] private string nombreEscenaGameplay = "Nivel1";

    void Start()
    {
        miVideoPlayer = GetComponent<VideoPlayer>();

        if (miVideoPlayer != null)
        {
            // Le indicamos a Unity que escuche únicamente cuando el video termine por sí solo
            miVideoPlayer.loopPointReached += AlTerminarVideo;
        }
        else
        {
            // Si por algún error extraño el video no carga, pasa a la escena para no congelar el juego
            CargarJuego();
        }
    }

    // Eliminamos por completo el "void Update" para que el teclado no haga nada

    private void AlTerminarVideo(VideoPlayer vp)
    {
        CargarJuego();
    }

    private void CargarJuego()
    {
        if (miVideoPlayer != null)
        {
            miVideoPlayer.loopPointReached -= AlTerminarVideo;
        }

        // Carga la siguiente escena de forma limpia
        SceneManager.LoadScene(nombreEscenaGameplay);
    }
}
