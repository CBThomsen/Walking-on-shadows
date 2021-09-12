using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelect : MonoBehaviour
{

    public GameObject levelButtonPrefab;

    private string levelResourcePath = "Assets/Resources/Levels";

    void Start()
    {
        LevelController.instance.UnloadCurrentLevel();

        string[] files = Directory.GetFiles(levelResourcePath);

        for (var i = 0; i < files.Length; i++)
        {
            if (files[i].Contains(".meta"))
                continue;

            GameObject btn = GameObject.Instantiate(levelButtonPrefab);
            btn.transform.SetParent(levelButtonPrefab.transform.parent);

            TextMeshProUGUI text = btn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                string name = files[i].Substring(files[i].LastIndexOf("\\") + 1, files[i].LastIndexOf(".") - files[i].LastIndexOf("\\") - 1);
                text.SetText(name);

                btn.GetComponent<Button>().onClick.AddListener(() => OnClickedLevelButton(name));
            }
        }

        levelButtonPrefab.SetActive(false);
    }

    private void OnClickedLevelButton(string levelName)
    {
        StartCoroutine(LevelController.instance.LoadLevel(levelName));
    }
}