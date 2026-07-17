using UnityEngine;

// Amplifica el audio del VideoPlayer más allá de 1.0 via DSP
[RequireComponent(typeof(AudioSource))]
public class AudioAmplificador : MonoBehaviour
{
    public float ganancia = 1f;

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = Mathf.Clamp(data[i] * ganancia, -1f, 1f);
    }
}
