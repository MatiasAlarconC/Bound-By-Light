using UnityEngine;

public class AplicadorVolumen : MonoBehaviour
{
    private AudioSource _source;

    void Start()
    {
        _source = GetComponent<AudioSource>();
        if (_source == null) return;

        float master = PlayerPrefs.GetFloat("MasterVolume", 80f) / 100f;
        float music  = PlayerPrefs.GetFloat("MusicVolume",  70f) / 100f;
        _source.volume = master * music;
    }
}
