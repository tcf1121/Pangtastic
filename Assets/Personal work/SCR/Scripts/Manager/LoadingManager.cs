using SCR_B;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static int _nextScene;
    [SerializeField] TMP_Text _loadingText;
    [SerializeField] Image _fillBar;
    [SerializeField] StringSO _readyOpen;
    [SerializeField] StringSO _makingDonut;

    private void Start()
    {
        _loadingText.text = _readyOpen.GetText(Manager.Language.GetLanguage());
        StartCoroutine(LoadScene());
    }

    public static void LoadScene(int sceneNum)
    {

        _nextScene = sceneNum;
        SceneManager.LoadScene(1/*로딩씬*/);
    }

    IEnumerator LoadScene()
    {
        yield return null;
        UserInfoUI.Instance.SetActive(false);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            if (_fillBar != null)
            {
                _fillBar.fillAmount = asyncLoad.progress;
            }
            yield return null;
        }

        _fillBar.fillAmount = 0f;
        _loadingText.text = _makingDonut.GetText(Manager.Language.GetLanguage());

        Scene gameScene = SceneManager.GetSceneByName("Game Scene");
        // 씬의 모든 루트 오브젝트를 순회하며 BoardManager를 찾습니다.
        GameObject[] rootObjects = gameScene.GetRootGameObjects();
        KDJ.BoardManager boardManager = null;

        // 이 시점에서 rootObjects 배열의 길이를 확인하여 디버깅에 도움을 받을 수 있습니다.
        Debug.Log($"씬 '{_nextScene}'의 루트 오브젝트 개수: {rootObjects.Length}");

        foreach (GameObject rootObj in rootObjects)
        {
            boardManager = rootObj.GetComponent<KDJ.BoardManager>();
            if (boardManager != null)
            {
                break;
            }
        }

        yield return StartCoroutine(boardManager.StageInit(_fillBar));

        yield return new WaitForEndOfFrame();

        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.SetActiveScene(gameScene);
    }
}
