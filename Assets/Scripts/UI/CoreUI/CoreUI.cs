using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreUI : MonoBehaviour
{
    public void GoToMenu()
    {
        PlayerPrefs.SetString("LastLevel", "");
        SceneManager.LoadScene("Startup", LoadSceneMode.Single);
    }
}
