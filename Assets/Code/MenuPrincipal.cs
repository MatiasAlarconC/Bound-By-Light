using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Adjuntar a cualquier GameObject en la escena "Main Menu".
// Asignar los botones desde el Inspector.
public class MenuPrincipal : MonoBehaviour
{
    [Header("Botones principales")]
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonNuevaPartida;
    [SerializeField] private Button botonConfiguracion;

    [Header("Botón rewind (solo se muestra si hay checkpoint)")]
    [SerializeField] private Button botonRewound;
    [Tooltip("Nombre de la escena de configuración")]
    [SerializeField] private string escenaConfiguracion = "Configuracion";

    [Header("Sonido de botones")]
    [Tooltip("Arrastra aquí el clip de sonido para los botones del menú")]
    [SerializeField] private AudioClip sonidoBoton;

    private AudioSource fuenteAudio;

    void Start()
    {
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null)
            fuenteAudio = gameObject.AddComponent<AudioSource>();

        bool haySave       = PlayerPrefs.GetInt("SaveExists", 0) == 1;
        bool hayCheckpoint = PlayerPrefs.HasKey("CheckpointX") && PlayerPrefs.HasKey("CheckpointY");

        // "Continuar" solo visible si hay partida guardada
        if (botonContinuar != null)
        {
            botonContinuar.interactable = haySave;
            botonContinuar.onClick.AddListener(Continuar);
        }

        // "Rewind" visible solo si hay checkpoint activo
        if (botonRewound != null)
        {
            botonRewound.gameObject.SetActive(haySave && hayCheckpoint);
            botonRewound.onClick.AddListener(IrAlUltimoCheckpoint);
        }

        if (botonNuevaPartida != null)
            botonNuevaPartida.onClick.AddListener(NuevaPartida);

        if (botonConfiguracion != null)
            botonConfiguracion.onClick.AddListener(AbrirConfiguracion);
    }

    void PlayClick()
    {
        if (fuenteAudio != null && sonidoBoton != null)
            fuenteAudio.PlayOneShot(sonidoBoton);
    }

    void Continuar()
    {
        PlayClick();
        string escena = PlayerPrefs.GetString("LastScene", "Nivel2");
        SceneManager.LoadScene(escena);
    }

    void IrAlUltimoCheckpoint()
    {
        PlayClick();
        PlayerPrefs.DeleteKey("HasExitPos");
        PlayerPrefs.Save();
        string escena = PlayerPrefs.GetString("LastScene", "Nivel2");
        SceneManager.LoadScene(escena);
    }

    void NuevaPartida()
    {
        PlayClick();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene("Nivel2");
    }

    void AbrirConfiguracion()
    {
        PlayClick();
        SceneManager.LoadScene(escenaConfiguracion);
    }
}
