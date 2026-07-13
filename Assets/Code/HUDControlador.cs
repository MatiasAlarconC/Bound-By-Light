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
    [Tooltip("Sprite para cuando Babosa está montada en la Mantarraya (modo combinado)")]
    [SerializeField] private Sprite fotoCombinado;

    void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = true;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        if (iconoPersonajeActual != null)
        {
            float size = Mathf.Min(Screen.width, Screen.height) * 0.14f;
            iconoPersonajeActual.rectTransform.sizeDelta = new Vector2(size, size);
        }

    }

    public void ActualizarIndicador(bool esPulpoActivo)
    {
        Sprite foto   = esPulpoActivo ? fotoPulpo : fotoBabosa;
        string nombre = esPulpoActivo ? "Pulpo"   : "Babosa";

        if (iconoPersonajeActual != null)
        {
            if (foto != null) iconoPersonajeActual.sprite = foto;
            iconoPersonajeActual.color   = Color.white;
            iconoPersonajeActual.enabled = true;
        }

        if (textoPersonajeActual != null)
        {
            textoPersonajeActual.text    = nombre;
            textoPersonajeActual.color   = Color.white;
            textoPersonajeActual.enabled = true;
        }
    }

    public void MostrarCombinado()
    {
        if (iconoPersonajeActual != null)
        {
            if (fotoCombinado != null) iconoPersonajeActual.sprite = fotoCombinado;
            iconoPersonajeActual.color   = Color.white;
            iconoPersonajeActual.enabled = true;
        }

        if (textoPersonajeActual != null)
        {
            textoPersonajeActual.text    = "Combinados";
            textoPersonajeActual.color   = Color.white;
            textoPersonajeActual.enabled = true;
        }
    }
}
