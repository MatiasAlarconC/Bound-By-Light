using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar el nivel

public class MenuPausa : MonoBehaviour
{
    [Header("UI Paneles")]
    [SerializeField] private GameObject botonPausaUI;  // El botón de la esquina
    [SerializeField] private GameObject menuPausaPanel; // El cuadro del medio con Reanudar/Reiniciar

    private bool juegoPausado = false;

    void Start()
    {
        // Al empezar, nos aseguramos de que el juego corra y el menú del medio esté oculto
        Time.timeScale = 1f;
        if (menuPausaPanel != null) menuPausaPanel.SetActive(false);
        if (botonPausaUI != null) botonPausaUI.SetActive(true);
    }

    void Update()
    {
        // OPCIONAL: También puedes pausar/despausar presionando la tecla Escape (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        juegoPausado = true;
        Time.timeScale = 0f; // CONGELA el tiempo y las físicas del juego

        if (menuPausaPanel != null) menuPausaPanel.SetActive(true); // Muestra el menú del medio
        if (botonPausaUI != null) botonPausaUI.SetActive(false);    // Oculta el botón de la esquina
    }

    public void Reanudar()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // DEVUELVE el tiempo a la normalidad

        if (menuPausaPanel != null) menuPausaPanel.SetActive(false); // Oculta el menú del medio
        if (botonPausaUI != null) botonPausaUI.SetActive(true);      // Muestra el botón de la esquina
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; // ¡IMPORTANTE! Despausar el tiempo antes de recargar
        
        // Consigue el nombre de la escena actual en la que estás jugando y la vuelve a cargar
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(nombreEscenaActual);
    }
}
