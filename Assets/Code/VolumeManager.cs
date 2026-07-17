using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VolumeManager : MonoBehaviour
{
    private static VolumeManager _instance;
    private int _pendingFrames = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (_instance != null) return;
        var go = new GameObject("[VolumeManager]");
        _instance = go.AddComponent<VolumeManager>();
        DontDestroyOnLoad(go);
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Aplicar ahora y durante los próximos 10 frames para capturar
        // los AudioSource creados dinámicamente en Start()
        Apply();
        _pendingFrames = 10;
    }

    void LateUpdate()
    {
        if (_pendingFrames <= 0) return;
        _pendingFrames--;
        Apply();
    }

    // Llamado desde MainMenuController al mover cualquier slider
    public static void Apply()
    {
        float music = PlayerPrefs.GetFloat("MusicVolume", 70f) / 100f;
        float sfx   = PlayerPrefs.GetFloat("SFXVolume",   60f) / 100f;

        AudioListener.volume = 1f;

        // FindObjectsInactive.Include captura objetos inactivos también
        foreach (var src in FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            // El objeto "Sounds" en cada nivel contiene la música de fondo
            if (src.gameObject.name == "Sounds")
                src.volume = music;
            else
                src.volume = sfx;
        }
    }
}
