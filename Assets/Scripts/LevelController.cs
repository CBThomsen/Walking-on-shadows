using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    private GameObject activeLevel;

    public static LevelController instance;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator LoadLevel(string levelName)
    {
        ResourceRequest levelLoadRequest = Resources.LoadAsync<GameObject>("Levels/" + levelName);
        AsyncOperation coreSceneLoadOperation = SceneManager.LoadSceneAsync("Core", LoadSceneMode.Single);

        while (!levelLoadRequest.isDone && !coreSceneLoadOperation.isDone)
        {
            yield return null;
        }

        activeLevel = GameObject.Instantiate(levelLoadRequest.asset) as GameObject;
        activeLevel.transform.SetParent(transform);
    }

    public void UnloadCurrentLevel()
    {
        Debug.Log(activeLevel);
        if (activeLevel != null)
        {
            Destroy(activeLevel);
        }
    }
}
