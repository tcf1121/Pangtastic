using Firebase.Auth;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GPGSManager : Singleton<GPGSManager>
{
    //public static GPGSManager Instance { get; private set; }
    // public static string PlayerID;
    // public static string PlayerName;

    private int retryCount = 0;
    private const int maxRetryCount = 3;   // 최대 재시도 횟수
    private const float retryDelay = 2f;   // 재시도 간격 (초 단위)

    private bool _deleting;

    protected override void Awake()
    {
        base.Awake();
    }


    public void AuthenticateUser()
    {
        Debug.Log("GPGS 로그인 시도...");

        PlayGamesPlatform.Instance.Authenticate(OnPlayAuthenticated);

        //SceneManager.LoadScene("OutGame Test Scene"); //JWJ 주석처리함
    }


    /// <summary>
    /// 회원가입(구글 로그인 auth에 넣기)
    /// </summary>
    /// <param name="status"></param>
    private void OnPlayAuthenticated(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("GPGS 로그인 성공");


            PlayGamesPlatform.Instance.RequestServerSideAccess(
                false,
                authCode =>
                {
                    var credential = PlayGamesAuthProvider.GetCredential(authCode);

                    Debug.Log($"credential : {credential}");

                    Manager.DB.auth.SignInAndRetrieveDataWithCredentialAsync(credential)
                        .ContinueWithOnMainThread(task =>
                        {
                            // DB 저장 호출
                            Manager.DB.UserIntoSave();

                            //JWJ 추가
                            Manager.DB.user = Manager.DB.auth.CurrentUser;
                            if (Manager.Stage != null)
                            {
                                //Manager.Stage.LoadStage();
                            }
                            //SceneManager.LoadScene("OutGame Test Scene");
                            SceneManager.LoadScene(2/*로비씬*/);
                        });
                });


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
                FindObjectOfType<GuestLogin>()?.OnGuestLoginButtonClicked(); //게스트로 로그인 시킴
            }
        }
    }

    /// <summary>
    /// 구글 로그인 회원탈퇴
    /// </summary>
    /// <returns></returns>
    public void UserDelete()
    {
        if (_deleting) { Debug.LogWarning("삭제 진행 중입니다."); return; }
        _deleting = true;

        string authJson = Manager.DB.GetAuthInfo();
        DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

        if (Manager.DB.user == null) { Fail("로그인 안됨"); return; }

        // 재인증
        PlayGamesPlatform.Instance.RequestServerSideAccess(true, authCode =>
        {
            if (string.IsNullOrEmpty(authCode)) { Fail("authCode 없음"); return; }

            var cred = PlayGamesAuthProvider.GetCredential(authCode);
            Manager.DB.user.ReauthenticateAsync(cred).ContinueWithOnMainThread(reauthTask =>
            {
                if (reauthTask.IsFaulted || reauthTask.IsCanceled) { Fail("재인증 실패: " + reauthTask.Exception); return; }

                // 2) 사용자 데이터 삭제(/users/{uid})
                // if (DatabaseSystem.Instance.dbRef == null) { Fail("DB Reference가 null"); return; }

                // DatabaseSystem.Instance.dbRef.Child("users").Child(info.uid).RemoveValueAsync().ContinueWithOnMainThread(dbTask =>
                // {
                // if (dbTask.IsFaulted) { Fail("DB 삭제 실패: " + dbTask.Exception); return; }

                // 3) Firebase 계정 삭제
                Manager.DB.user.DeleteAsync().ContinueWithOnMainThread(delTask =>
                {
                    if (delTask.IsFaulted || delTask.IsCanceled) { Fail("계정 삭제 실패: " + delTask.Exception); return; }

                    // 4) 로그아웃 + 자동로그인 차단 플래그
                    try { Manager.DB.auth.SignOut(); } catch { }
                    PlayerPrefs.SetInt("SkipPGS", 1);  // 다음 실행에서 PGS 자동 인증 막기
                    PlayerPrefs.Save();

                    _deleting = false;
                    Debug.Log("회원 탈퇴 완료");

                    // Application.Quit();
                    // onDone?.Invoke();
                });
                // });
            });
        });

        void Fail(string msg)
        {
            _deleting = false;
            Debug.LogError(msg);
            // onError?.Invoke(msg);
        }
    }

    private IEnumerator RetryAuthenticate()
    {
        yield return new WaitForSeconds(retryDelay);
        AuthenticateUser();
    }

}