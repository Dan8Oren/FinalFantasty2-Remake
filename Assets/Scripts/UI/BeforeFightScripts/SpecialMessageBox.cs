using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * A special message box for the end of the game event.
 */
public class SpecialMessageBox : MonoBehaviour
{
    public GameObject pressStart;
    [SerializeField] private List<string> dialogs;
    [SerializeField] private TextMeshPro textDisplay;
    [SerializeField] private float timeBetweenChars;
    [SerializeField] private int spaceCount;
    private bool _mainMessageFinished;

    // Update is called once per frame
    private void Update()
    {
        if (spaceCount > 3)
        {
            Destroy(SoundManager.Instance.gameObject);
            SceneManager.LoadScene("MainMenu");
            Destroy(GameManager.Instance.gameObject);
            Destroy(MySceneManager.Instance.gameObject);
            Destroy(InventoryManager.Instance.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.Space)) spaceCount++;
    }

    public void ShowDialog(string newDialogs, bool toAdd)
    {
        if (toAdd)
        {
            _mainMessageFinished = false;
            dialogs.Add(newDialogs);
        }
        else
        {
            StartCoroutine(AnimateDialog(newDialogs));
        }
    }

    private IEnumerator AnimateDialog(string dialog)
    {
        var chars = dialog.Replace("\\n", "\n").Replace("\\t", "\t").ToCharArray();
        var tempToShow = new string("");
        var isOnHtml = false;
        var closing = 0;
        foreach (var c in chars)
        {
            if (closing == 2) isOnHtml = false;

            tempToShow += c;
            textDisplay.SetText(tempToShow);
            if (c == '<' && !isOnHtml)
            {
                closing = 0;
                isOnHtml = true;
            }

            if (!isOnHtml)
            {
                yield return new WaitForSeconds(timeBetweenChars);
            }
            else
            {
                if (c == '>') closing++;
            }
        }

        _mainMessageFinished = true;
    }

    /**
     * Loops through all cakes text display's
     */
    public IEnumerator StartLoop(float time)
    {
        yield return new WaitUntil(() => _mainMessageFinished);
        foreach (var str in dialogs)
        {
            textDisplay.SetText(str);
            yield return new WaitForSeconds(time);
        }

        pressStart.SetActive(true);
        StartCoroutine(StartLoop(time));
    }
}