using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SetupCinematicas
{
    [MenuItem("Tools/Setup Cinemáticas (pantalla negra)")]
    public static void Setup()
    {
        string[] escenas = {
            "Assets/Scenes/Escena_Cinematica.unity",
            "Assets/Scenes/Escena_Cinematica2.unity",
            "Assets/Scenes/Escena_Cinematica3.unity"
        };

        foreach (string ruta in escenas)
        {
            var scene = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            ConfigurarCineScene();
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Configurada: {ruta}");
        }

        Debug.Log("Listo — todas las cinemáticas configuradas.");
    }

    static void ConfigurarCineScene()
    {
        CargadorPorVideo cargador = Object.FindFirstObjectByType<CargadorPorVideo>();
        if (cargador == null) { Debug.LogError("No se encontró CargadorPorVideo."); return; }

        var previo = GameObject.Find("Canvas_Cine");
        if (previo != null) Object.DestroyImmediate(previo);

        // Canvas
        GameObject canvasGO = new GameObject("Canvas_Cine");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel negro
        GameObject panelGO = new GameObject("PantallaEspera");
        panelGO.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = panelGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = panelGO.AddComponent<Image>();
        img.color          = Color.black;
        img.raycastTarget  = false;

        // AudioSource + AudioAmplificador en el mismo GameObject que CargadorPorVideo
        GameObject vpGO = cargador.gameObject;
        if (vpGO.GetComponent<AudioSource>() == null)
            vpGO.AddComponent<AudioSource>();
        if (vpGO.GetComponent<AudioAmplificador>() == null)
            vpGO.AddComponent<AudioAmplificador>();

        // Asignar al CargadorPorVideo
        SerializedObject so = new SerializedObject(cargador);
        so.FindProperty("pantallaEspera").objectReferenceValue = panelGO;
        so.ApplyModifiedProperties();
    }
}
