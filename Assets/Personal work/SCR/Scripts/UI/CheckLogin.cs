using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SCR
{

    public class CheckLogin : MonoBehaviour
    {
        [SerializeField] Button _enterBtn;
        [SerializeField] Button _googlePlayBtn;
        [SerializeField] Button _guestBtn;
        [SerializeField] Button _logoutBtn;
        [SerializeField] GameObject _enterPanel;
        [SerializeField] GameObject _loginPanel;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _logoutBtn.onClick.AddListener(Logout);
            _enterBtn.onClick.AddListener(CheckBeforeAsync);
            _googlePlayBtn.onClick.AddListener(Manager.GPGS.AuthenticateUser);
            _guestBtn.onClick.AddListener(LoginAsGuest);
        }

        private async void CheckBeforeAsync()
        {
            Manager.Data.SetUser();
            await Manager.DB.InitFirebase();
            SceneManager.LoadScene("Lobby Scene");
        }

        private void Logout()
        {
            DeleteFB();
            Manager.Data.DeleteSaveData();
        }

        private void DeleteFB()
        {
            FirebaseAuth auth = Manager.DB.auth;
            if (auth != null)
            {
                DatabaseReference dataToRemove = Manager.DB.GetUserPath(auth.CurrentUser.UserId);
                dataToRemove.RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("데이터 삭제 성공!");
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.LogError("데이터 삭제 실패: " + task.Exception);
                    }
                });
                auth.SignOut();
            }
        }

        private async void HowToLoginAsync(bool isGuest)
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            // 게스트로 로그인 했을 경우
            if (isGuest)
            {
                Debug.Log($"이미 게스트 로그인 상태: {auth.CurrentUser.UserId}");
            }
            // 구글로 로그인 했을 경우
            else
            {
                Debug.Log($"이미 구글 플레이 로그인 상태: {auth.CurrentUser.UserId}");
            }
            //Manager.DB.user = auth.CurrentUser;
            //await Manager.Data.SetUser(auth.CurrentUser.UserId);
            SceneManager.LoadScene(2/*로비씬*/);
        }

        private void LoginAsGuest()
        {
            Debug.Log("버튼눌림");
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(async task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("익명 로그인 취소");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"익명 로그인 실패 : {task.Exception}");
                    return;
                }
                FirebaseUser newUser = task.Result.User;
                string uid = newUser.UserId;
                Debug.LogFormat($"게스트 로그인 성공 : {newUser.UserId}");

                //Manager.Data.NewUser(uid, );
                // Manager.User.NewUser(uid, DateTime.Now.ToString("O"));


            });

        }
    }
}

