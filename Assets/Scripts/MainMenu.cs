using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Update()
    {
        if (!SoundManager.Instance.IsPlaying)
        {
            SoundManager.Instance.PlayThemeByScene();
        }
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("Respawn");
        }
    }
}
