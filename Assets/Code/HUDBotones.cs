using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDBotones : MonoBehaviour
{
    [SerializeField] private Button btnPausa;
    [SerializeField] private Button btnReiniciar;

    void Start()
    {
        btnPausa?.onClick.AddListener(OnPausa);
        btnReiniciar?.onClick.AddListener(OnReiniciar);
    }

    void OnPausa()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null) { gm.IrAlMenuPrincipal(); return; }
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    void OnReiniciar()
    {
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("HasExitPos");
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
