using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDBotones : MonoBehaviour
{
    [SerializeField] private Button btnPausa;
    [SerializeField] private Button btnReiniciar;
    [SerializeField] private AudioClip sonidoClick;

    private AudioSource fuenteAudio;

    void Start()
    {
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null)
            fuenteAudio = gameObject.AddComponent<AudioSource>();

        btnPausa?.onClick.AddListener(OnPausa);
        btnReiniciar?.onClick.AddListener(OnReiniciar);
    }

    void PlayClick()
    {
        if (fuenteAudio != null && sonidoClick != null)
            fuenteAudio.PlayOneShot(sonidoClick);
    }

    void OnPausa()
    {
        PlayClick();
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null) { gm.IrAlMenuPrincipal(); return; }
        Time.timeScale = 1f;
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("SaveExists", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main Menu");
    }

    void OnReiniciar()
    {
        PlayClick();
        Time.timeScale = 1f;
        string escena = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", escena);
        PlayerPrefs.DeleteKey("HasExitPos");
        PlayerPrefs.Save();
        SceneManager.LoadScene(escena);
    }
}
