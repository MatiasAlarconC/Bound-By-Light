using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            IrAlMenuPrincipal();
    }

    private void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("SaveExists", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main Menu");
    }
}
