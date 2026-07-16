using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SonidoGeiser : MonoBehaviour
{
    private AudioSource audioSource;
    private Camera camaraPrincipal;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        camaraPrincipal = Camera.main;

        audioSource.playOnAwake = false;
    }

    public void Reproducir()
    {
        if (!EstaDentroDeCamara())
            return;

        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.Stop();
        audioSource.Play();
    }

    public void Detener()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    private bool EstaDentroDeCamara()
    {
        if (camaraPrincipal == null)
            camaraPrincipal = Camera.main;

        if (camaraPrincipal == null)
            return false;

        Vector3 posicionViewport =
            camaraPrincipal.WorldToViewportPoint(transform.position);

        return posicionViewport.z > 0f &&
               posicionViewport.x >= 0f &&
               posicionViewport.x <= 1f &&
               posicionViewport.y >= 0f &&
               posicionViewport.y <= 1f;
    }
}