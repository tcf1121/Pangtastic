using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class GPGSManager : MonoBehaviour
{
    public static GPGSManager Instance { get; private set; }
    public static string PlayerID;
    public static string PlayerName;
    
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
        }
        else
        {
            Debug.Log("GPGS 로그인 실패");
        }
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