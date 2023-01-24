using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Update()
    {
        if (!SoundManager.Instance.IsPlaying) SoundManager.Instance.PlayThemeByScene();
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (Input.GetKeyDown(KeyCode.Space)) SceneManager.LoadScene("Respawn");
    }
}