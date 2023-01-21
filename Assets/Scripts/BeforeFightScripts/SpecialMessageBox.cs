using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class SpecialMessageBox : MonoBehaviour
{
    public bool enableSpace;
    [SerializeField] private List<String> dialogs;
    [SerializeField] private TextMeshPro textDisplay;
    [SerializeField] private float timeBetweenChars;
    [SerializeField] private int spaceCount;

    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (spaceCount > 3)
        {
            Destroy(GameManager.Instance);
            Destroy(MySceneManager.Instance);
            Destroy(InventoryManager.Instance);
            SceneManager.LoadScene("MainMenu");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceCount++;
        }
    }

    public void ShowDialog(string newDialogs, bool toAdd)
    {
        if (toAdd)
        {
            dialogs.Add(newDialogs);
        }
        else
        {
            StartCoroutine(AnimateDialog(newDialogs));
        }
    }

    private IEnumerator AnimateDialog(string dialog)
    {
        char[] chars = dialog.Replace("\\n", "\n").Replace("\\t", "\t").ToCharArray();
        String tempToShow = new string("");
        bool isOnHtml = false;
        int closing = 0;
        foreach (var c in chars)
        {
            if (closing == 2)
            {
                isOnHtml = false;
            }

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
                if (c == '>')
                {
                    closing++;
                }
            }

        }
    }

    public IEnumerator StartLoop(float time)
    {
        foreach (var str in dialogs)
        {
            textDisplay.SetText(str);
            yield return new WaitForSeconds(time);
        }
        StartCoroutine(StartLoop(time));
    }

}


