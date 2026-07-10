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
        if (esPulpoActivo)
        {
            if (iconoPersonajeActual != null && fotoPulpo != null)
                iconoPersonajeActual.sprite = fotoPulpo;
            if (textoPersonajeActual != null)
                textoPersonajeActual.text = "Pulpo";
        }
        else
        {
            if (iconoPersonajeActual != null && fotoBabosa != null)
                iconoPersonajeActual.sprite = fotoBabosa;
            if (textoPersonajeActual != null)
                textoPersonajeActual.text = "Babosa";
        }
    }
}
