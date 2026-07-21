using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CargadorPorVideo : MonoBehaviour
{
    private VideoPlayer miVideoPlayer;
    private AudioSource miAudioSource;

    [Header("Configuración")]
    [SerializeField] private string nombreEscenaGameplay = "Nivel1";
    [SerializeField] private string nombreArchivoVideo = "";
    [SerializeField] private bool esFinalDelJuego = false;

    [Header("Volumen del video (1 = normal, 2 = doble, 3 = triple...)")]
    [Range(0f, 5f)]
    [SerializeField] private float volumenVideo = 1f;

    [Header("Pantalla de carga")]
    [SerializeField] private GameObject pantallaEspera;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null) cam.backgroundColor = Color.black;

        pantallaEspera ??= GameObject.Find("PantallaEspera");

        if (pantallaEspera != null) pantallaEspera.SetActive(true);

        miVideoPlayer  = GetComponent<VideoPlayer>();
        miAudioSource  = GetComponent<AudioSource>();

        if (miVideoPlayer != null)
        {
            // Salida por AudioSource para poder amplificar con AudioAmplificador
            if (miAudioSource != null)
            {
                miVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                miVideoPlayer.SetTargetAudioSource(0, miAudioSource);
                miAudioSource.volume = 1f;

                AudioAmplificador amp = GetComponent<AudioAmplificador>();
                if (amp != null) amp.ganancia = volumenVideo;
            }

            if (!string.IsNullOrEmpty(nombreArchivoVideo))
            {
                miVideoPlayer.source            = VideoSource.Url;
                miVideoPlayer.url               = System.IO.Path.Combine(Application.streamingAssetsPath, "Cinematics", nombreArchivoVideo);
                miVideoPlayer.playOnAwake       = false;
                miVideoPlayer.waitForFirstFrame = true;

                miVideoPlayer.prepareCompleted += OnVideoPreparado;
                miVideoPlayer.loopPointReached += AlTerminarVideo;
                miVideoPlayer.Prepare();
            }
            else
            {
                miVideoPlayer.loopPointReached += AlTerminarVideo;
                CargarJuego();
            }
        }
        else
        {
            CargarJuego();
        }
    }

    private void OnVideoPreparado(VideoPlayer vp)
    {
        miVideoPlayer.prepareCompleted -= OnVideoPreparado;
        if (pantallaEspera != null) pantallaEspera.SetActive(false);
        miVideoPlayer.Play();
    }

    private void AlTerminarVideo(VideoPlayer vp) => CargarJuego();

    private void CargarJuego()
    {
        if (miVideoPlayer != null)
        {
            miVideoPlayer.loopPointReached -= AlTerminarVideo;
            miVideoPlayer.prepareCompleted -= OnVideoPreparado;
        }

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
