using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CargadorPorVideo : MonoBehaviour
{
    private VideoPlayer miVideoPlayer;

    [Header("Configuración")]
    [Tooltip("Escribe el nombre exacto de la escena a la que irá al terminar")]
    [SerializeField] private string nombreEscenaGameplay = "Nivel1";

    [Tooltip("Nombre del archivo de video en StreamingAssets/Cinematics/ (ej: Cinematica1.mp4)")]
    [SerializeField] private string nombreArchivoVideo = "";

    [Tooltip("Marcar si esta es la cinemática final del juego — borra el save y va al Main Menu")]
    [SerializeField] private bool esFinalDelJuego = false;

    void Start()
    {
        miVideoPlayer = GetComponent<VideoPlayer>();

        if (miVideoPlayer != null)
        {
            if (!string.IsNullOrEmpty(nombreArchivoVideo))
            {
                miVideoPlayer.source = VideoSource.Url;
                miVideoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "Cinematics", nombreArchivoVideo);
                miVideoPlayer.Play();
            }
            miVideoPlayer.loopPointReached += AlTerminarVideo;
        }
        else
        {
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
            miVideoPlayer.loopPointReached -= AlTerminarVideo;

        if (esFinalDelJuego)
        {
            PlayerPrefs.DeleteKey("SaveExists");
            PlayerPrefs.DeleteKey("LastScene");
            PlayerPrefs.DeleteKey("CheckpointX");
            PlayerPrefs.DeleteKey("CheckpointY");
            PlayerPrefs.DeleteKey("HasExitPos");
            PlayerPrefs.Save();
            SceneManager.LoadScene("Main Menu");
        }
        else
        {
            SceneManager.LoadScene(nombreEscenaGameplay);
        }
    }
}
