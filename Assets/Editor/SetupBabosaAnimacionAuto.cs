using UnityEditor;

[InitializeOnLoad]
public static class SetupBabosaAnimacionAuto
{
    const string KEY = "BBL_BabosaAnimV1";

    static SetupBabosaAnimacionAuto()
    {
        if (EditorPrefs.GetBool(KEY, false)) return;
        EditorApplication.delayCall += RunSetup;
    }

    static void RunSetup()
    {
        EditorApplication.delayCall -= RunSetup;
        SetupBabosaAnimacion.Setup();
        EditorPrefs.SetBool(KEY, true);
    }
}
