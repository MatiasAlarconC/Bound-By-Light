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

    void Start()
    {
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

    void Continuar()
    {
        string escena = PlayerPrefs.GetString("LastScene", "Nivel2");
        SceneManager.LoadScene(escena);
    }

    void IrAlUltimoCheckpoint()
    {
        // Borrar la posición de salida para que GameManager use el checkpoint guardado
        PlayerPrefs.DeleteKey("HasExitPos");
        PlayerPrefs.Save();

        string escena = PlayerPrefs.GetString("LastScene", "Nivel2");
        SceneManager.LoadScene(escena);
    }

    void NuevaPartida()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        string primeraEscena = "Nivel2";   // cambiar si hay Nivel1 u otra
        SceneManager.LoadScene(primeraEscena);
    }

    void AbrirConfiguracion()
    {
        SceneManager.LoadScene(escenaConfiguracion);
    }
}
