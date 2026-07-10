using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDControlador : MonoBehaviour
{
    [Header("Componentes de UI")]
    [SerializeField] private Image iconoPersonajeActual;
    [SerializeField] private TextMeshProUGUI textoPersonajeActual;

    [Header("Sprites de los Personajes")]
    [SerializeField] private Sprite fotoBabosa;
    [SerializeField] private Sprite fotoPulpo;

    void Start()
    {
        // Canvas escalable según resolución de pantalla
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        // Tamaño del icono proporcional a la pantalla (8% del lado menor)
        if (iconoPersonajeActual != null)
        {
            float size = Mathf.Min(Screen.width, Screen.height) * 0.08f;
            iconoPersonajeActual.rectTransform.sizeDelta = new Vector2(size, size);
        }
    }

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
