using UnityEngine;
using UnityEngine.UI; // Para la Image
using TMPro;         // Para TextMeshPro (si usas el texto moderno)

public class HUDControlador : MonoBehaviour
{
    [Header("Componentes de UI")]
    [SerializeField] private Image iconoPersonajeActual; 
    [SerializeField] private TextMeshProUGUI textoPersonajeActual; // <--- NUEVO: Para el texto dinámico

    [Header("Sprites de los Personajes")]
    [SerializeField] private Sprite fotoBabosa; 
    [SerializeField] private Sprite fotoPulpo;  

    public void ActualizarIndicador(bool esPulpoActivo)
    {
        // 1. Cambiar la foto y el texto si está activo el Pulpo
        if (esPulpoActivo)
        {
            if (iconoPersonajeActual != null) iconoPersonajeActual.sprite = fotoPulpo;
            if (textoPersonajeActual != null) textoPersonajeActual.text = "Pulpo"; // <--- NUEVO
        }
        // 2. Cambiar la foto y el texto si está activa la Babosa
        else
        {
            if (iconoPersonajeActual != null) iconoPersonajeActual.sprite = fotoBabosa;
            if (textoPersonajeActual != null) textoPersonajeActual.text = "Babosa"; // <--- NUEVO
        }
    }
}
