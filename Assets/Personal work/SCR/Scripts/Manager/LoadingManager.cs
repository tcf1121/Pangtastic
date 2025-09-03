using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static int _nextScene;

    [SerializeField] Image _fillBar;

    private void Start()
    {
        //StartCoroutine
    }

    public static void LoadScene(int sceneNum)
    {
        _nextScene = sceneNum;
        SceneManager.LoadScene(1/*로딩씬*/);
    }

    private IEnumerator LoadScene()
    {
        yield return null;
    }
}
