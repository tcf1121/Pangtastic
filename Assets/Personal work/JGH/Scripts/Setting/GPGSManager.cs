using Firebase.Auth;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DatabaseSystem;

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
                            //Manager.DB.UserIntoSave();

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

        //string authJson = Manager.DB.GetAuthInfo();
        //DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);

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

    public void LinkGuestToGoogle(System.Action<bool> onDone) // 게스트 구글 링크
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated == false)
        {
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status != SignInStatus.Success)
                {
                    Debug.LogError($"GPGS 로그인 실패 {status.ToString()}");
                    onDone?.Invoke(false);
                    return;
                }
                Debug.Log("GPGS로그인 성공");
                RequestAndLink(onDone);
            });
            return;
        }
        Debug.Log("GPGS 로그인 이미 되어있음");
        RequestAndLink(onDone);
    }

    private void RequestAndLink(System.Action<bool> onDone)
    {
        Debug.Log("게스트 > 구글 전환 시작");
        PlayGamesPlatform.Instance.RequestServerSideAccess(true, authCode =>
        {
            if (string.IsNullOrEmpty(authCode) == true)
            {
                Debug.LogError("Link 실패: authCode 없음");
                onDone?.Invoke(false);
                return;
            }

            Credential cred = PlayGamesAuthProvider.GetCredential(authCode);
            FirebaseUser curUser = Manager.DB.auth.CurrentUser;

            if (curUser == null)
            {
                Debug.LogError("Link 실패: 유저 없음");
                onDone?.Invoke(false);
                return;
            }

            if (!curUser.IsAnonymous)
            {
                Debug.Log("이미 영구 계정임");
                onDone?.Invoke(true);
                return;
            }

            curUser.LinkWithCredentialAsync(cred).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"Link 실패: {task.Exception}");
                    onDone?.Invoke(false);
                    return;
                }

                Debug.Log("게스트 > 구글 링크 성공. ");

                string gpgsName = PlayGamesPlatform.Instance.localUser.userName;
                Debug.Log($"GPGS 이름 : {gpgsName}");



                if (string.IsNullOrEmpty(gpgsName))
                {
                    Debug.Log("GPGS 이름이 공백임");
                    gpgsName = string.IsNullOrEmpty(Manager.DB.user.DisplayName) ? "Player" : Manager.DB.user.DisplayName;
                    Debug.Log($"대체 이름: {gpgsName}");
                }

                var profile = new UserProfile { DisplayName = gpgsName };

                Manager.DB.user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.LogError($"프로필 업데이트 실패: {task.Exception}");
                        return;
                    }

                    Debug.Log("프로필 업데이트 성공");

                    UserData cur = Manager.User.GetCurrentUserData();
                    cur.PlayerName = string.IsNullOrEmpty(gpgsName) ? "Player" : gpgsName; //이름 있으면 이름, 없으면 Player

                    Manager.User.SetUser(cur); //저장
                                               //Manager.DB.ChangeNickname(gpgsName);

                    Debug.Log($"DB.DisplayName : {Manager.DB.user.DisplayName}");
                    Debug.Log($"profile.DisplayName : {profile.DisplayName}");

                    string uid = Manager.DB.auth.CurrentUser.UserId;
                    //Manager.DB.MigrateGuestDataToUser(uid); //마이그레이션
                    //Manager.DB.UserIntoSave(); //이름 설정
                    onDone?.Invoke(true);
                });
            });
        });
    }
}