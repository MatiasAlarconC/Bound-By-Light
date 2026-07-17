using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SetupCinematicasAuto
{
    const string KEY = "BBL_CinematicasSetupV3";

    static SetupCinematicasAuto()
    {
        if (EditorPrefs.GetBool(KEY, false)) return;
        EditorApplication.delayCall += RunSetup;
    }

    static void RunSetup()
    {
        EditorApplication.delayCall -= RunSetup;

        // Guardar y recordar la escena actual para volver después
        var escenaActual = EditorSceneManager.GetActiveScene();
        if (escenaActual.isDirty)
            EditorSceneManager.SaveScene(escenaActual);
        string rutaActual = escenaActual.path;

        SetupCinematicas.Setup();

        // Volver a la escena que tenía abierta el usuario
        if (!string.IsNullOrEmpty(rutaActual))
            EditorSceneManager.OpenScene(rutaActual, OpenSceneMode.Single);

        EditorPrefs.SetBool(KEY, true);
        UnityEngine.Debug.Log("<color=lime>Setup cinemáticas completado automáticamente.</color>");
    }
}
