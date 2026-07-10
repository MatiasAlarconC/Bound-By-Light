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

        // Guardar posición exacta para que "continuar" retome desde aquí
        BabosaControl babosa = FindFirstObjectByType<BabosaControl>();
        if (babosa != null)
        {
            PlayerPrefs.SetFloat("ExitPosX", babosa.transform.position.x);
            PlayerPrefs.SetFloat("ExitPosY", babosa.transform.position.y);
            PlayerPrefs.SetInt("HasExitPos", 1);
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("Main Menu");
    }
}
