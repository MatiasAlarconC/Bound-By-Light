using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SetupHUDBotonesAuto
{
    const string KEY = "BBL_HUDSetupV3";

    static SetupHUDBotonesAuto()
    {
        if (EditorPrefs.GetBool(KEY, false)) return;
        EditorApplication.delayCall += RunSetup;
    }

    static void RunSetup()
    {
        EditorApplication.delayCall -= RunSetup;

        var escenaActual = EditorSceneManager.GetActiveScene();
        if (escenaActual.isDirty)
            EditorSceneManager.SaveScene(escenaActual);
        string rutaActual = escenaActual.path;

        SetupHUDBotones.Setup();

        if (!string.IsNullOrEmpty(rutaActual))
            EditorSceneManager.OpenScene(rutaActual, OpenSceneMode.Single);

        EditorPrefs.SetBool(KEY, true);
    }
}
