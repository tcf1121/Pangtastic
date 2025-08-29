using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class PlayGamesManager : MonoBehaviour
{
    public void Start()
    {
        // GPGS 초기화 
        // PlayGamesPlatform.Activate();
        
        // 로그인 시도
        PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
    }

    private void OnAuthenticated(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("GPGS 로그인 성공");
            // DatabaseSystem.Instance.CheckSignupUser();
        }
        else
        {
            Debug.Log("GPGS 로그인 실패");
        }
    }
    

}