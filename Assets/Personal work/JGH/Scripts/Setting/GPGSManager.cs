using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GPGSManager : MonoBehaviour
{
    public static GPGSManager Instance { get; private set; }
    public static string PlayerID;
    public static string PlayerName;

    private int retryCount = 0;
    private const int maxRetryCount = 3;   // 최대 재시도 횟수
    private const float retryDelay = 2f;   // 재시도 간격 (초 단위)

    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 로그인 시도
        //AuthenticateUser();
        // PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
    }


    public void AuthenticateUser()
    {
        Debug.Log("GPGS 로그인 시도...");
        PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
    }


    /// <summary>
    /// 로그인 여부
    /// </summary>
    /// <param name="status"></param>
    private void OnAuthenticated(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("GPGS 로그인 성공");

            // 로그인 성공 시점에 값 초기화
            PlayerID = PlayGamesPlatform.Instance.localUser.id;
            PlayerName = PlayGamesPlatform.Instance.localUser.userName;


            DatabaseSystem.Instance.UserIntoSave();
            SceneManager.LoadScene(2/*로비씬*/);
        }
        else
        {
            Debug.Log("GPGS 로그인 실패");

            // 재시도 로직
            if (retryCount < maxRetryCount)
            {
                retryCount++;
                Debug.Log($"로그인 재시도... ({retryCount}/{maxRetryCount})");
                StartCoroutine(RetryAuthenticate());
            }
            else
            {
                Debug.LogError("GPGS 로그인 재시도 횟수 초과");
                //Application.Quit();
            }
        }
    }

    private IEnumerator RetryAuthenticate()
    {
        yield return new WaitForSeconds(retryDelay);
        AuthenticateUser();
    }

    /// <summary>
    /// UID 
    /// </summary>
    /// <returns></returns>
    public string GetPlayerId()
    {
        return PlayerID;
    }

    /// <summary>
    /// 닉네임
    /// </summary>
    /// <returns></returns>
    public string GetPlayerName()
    {
        return PlayerName;
    }

}