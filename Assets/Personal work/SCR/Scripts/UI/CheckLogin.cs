using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SCR
{
    public enum LoginType
    {
        Guest,
        Google
    }
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
            _logoutBtn.onClick.AddListener(Logout);
            _enterBtn.onClick.AddListener(CheckBefore);
            _googlePlayBtn.onClick.AddListener(Manager.GPGS.AuthenticateUser);
            _guestBtn.onClick.AddListener(LoginAsGuest);
        }

        private async void CheckBefore()
        {

            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                if (auth.CurrentUser.IsAnonymous)  //JWJ 수정함
                {
                    Debug.Log($"이미 게스트 로그인 상태: {auth.CurrentUser.UserId}");
                    Manager.DB.user = auth.CurrentUser;
                    //Manager.Stage.LoadStage();
                    await Manager.User.SetUser(auth.CurrentUser.UserId);
                    //DontDSystem.StatGame();
                    SceneManager.LoadScene(2/*로비씬*/);
                }
                else
                {
                    Debug.Log($"이미 구글 플레이 로그인 상태: {auth.CurrentUser.UserId}");
                    Manager.DB.user = auth.CurrentUser;
                    //Manager.Stage.LoadStage();
                    //DontDSystem.StatGame();
                    SceneManager.LoadScene(2/*로비씬*/);
                }
                return;
            }
            // else
            LoginAsGuest();
        }

        private void Logout()
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
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

        private void HowToLogin(LoginType loginType)
        {
            // 게스트로 로그인 했을 경우
            if (loginType == LoginType.Guest)
            {

            }
            // 구글로 로그인 했을 경우
            else
            {
                Manager.GPGS.AuthenticateUser();
            }
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

                Manager.User.NewUser(uid, DateTime.Now.ToString("O"));

                //Manager.Stage.LoadStage();

                SceneManager.LoadScene("Lobby Scene");
            });

        }
    }
}

