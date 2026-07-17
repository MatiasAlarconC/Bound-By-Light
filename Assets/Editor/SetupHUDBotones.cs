using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SetupHUDBotones
{
    const string CLICK_SOUND = "Assets/Sounds/UI_click.wav";

    [MenuItem("Tools/Setup HUD Botones en Niveles")]
    public static void Setup()
    {
        string[] escenas = {
            "Assets/Scenes/MecanicaPulpo.unity",
            "Assets/Scenes/Nivel2.unity"
        };

        foreach (string ruta in escenas)
        {
            string fullPath = Application.dataPath + ruta.Substring("Assets".Length);
            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogWarning($"Escena no encontrada, omitiendo: {ruta}");
                continue;
            }
            var scene = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            AgregarHUD();
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"HUD actualizado: {ruta}");
        }

        Debug.Log("<color=lime>HUD Botones configurado en niveles.</color>");
    }

    static void AgregarHUD()
    {
        var previo = GameObject.Find("HUD_Botones");
        if (previo != null) Object.DestroyImmediate(previo);

        // Eliminar panel de pausa legacy si quedó de versión anterior
        var previoPanel = GameObject.Find("Panel_Pausa");
        if (previoPanel != null) Object.DestroyImmediate(previoPanel);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No se encontró Canvas en la escena."); return; }

        AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(CLICK_SOUND);
        Sprite spritePausa     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Interface/Pausa_Boton.png");
        Sprite spriteReiniciar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Interface/Reiniciar_Boton.png");

        // Contenedor HUD anclado a esquina superior izquierda
        GameObject hudGO = new GameObject("HUD_Botones");
        hudGO.transform.SetParent(canvas.transform, false);
        RectTransform hudRT = hudGO.AddComponent<RectTransform>();
        hudRT.anchorMin        = new Vector2(0f, 1f);
        hudRT.anchorMax        = new Vector2(0f, 1f);
        hudRT.pivot            = new Vector2(0f, 1f);
        hudRT.anchoredPosition = Vector2.zero;
        hudRT.sizeDelta        = Vector2.zero;

        AudioSource audio = hudGO.AddComponent<AudioSource>();
        audio.playOnAwake = false;

        HUDBotones script = hudGO.AddComponent<HUDBotones>();

        Button btnPausa     = CrearBoton("Btn_Pausa",     hudGO.transform, spritePausa,     new Vector2(48f,  -52f), 60f);
        Button btnReiniciar = CrearBoton("Btn_Reiniciar", hudGO.transform, spriteReiniciar, new Vector2(116f, -52f), 60f);

        SerializedObject so = new SerializedObject(script);
        so.FindProperty("btnPausa").objectReferenceValue     = btnPausa;
        so.FindProperty("btnReiniciar").objectReferenceValue = btnReiniciar;
        so.FindProperty("sonidoClick").objectReferenceValue  = clickClip;
        so.ApplyModifiedProperties();
    }

    static Button CrearBoton(string nombre, Transform padre, Sprite sprite, Vector2 pos, float tamaño)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(tamaño, tamaño);
        Image img = go.AddComponent<Image>();
        img.sprite         = sprite;
        img.preserveAspect = true;
        img.color          = Color.white;
        Button btn = go.AddComponent<Button>();
        Navigation nav = btn.navigation; nav.mode = Navigation.Mode.None; btn.navigation = nav;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        cb.pressedColor     = new Color(0.6f, 0.6f, 0.6f, 1f);
        btn.colors = cb;
        return btn;
    }
}
