using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] GameObject _enterPanel;
        [SerializeField] GameObject _loginPanel;

        void Awake()
        {
            _enterBtn.onClick.AddListener(CheckBefore);
            _googlePlayBtn.onClick.AddListener(Manager.GPGS.AuthenticateUser);
            _guestBtn.onClick.AddListener(LoginAsGuest);
        }

        private void CheckBefore()
        {
            _enterPanel.SetActive(false);
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                if (auth.CurrentUser.IsAnonymous)  //JWJ 수정함
                {
                    Debug.Log($"이미 게스트 로그인 상태: {auth.CurrentUser.UserId}");
                    Manager.DB.user = auth.CurrentUser;
                    Manager.Stage.LoadStage();
                    //DontDSystem.StatGame();
                    SceneManager.LoadScene(2/*로비씬*/);
                }
                else
                {
                    Debug.Log($"이미 일반 로그인 상태: {auth.CurrentUser.UserId}");
                    Manager.DB.user = auth.CurrentUser;
                    Manager.Stage.LoadStage();
                    //DontDSystem.StatGame();
                    SceneManager.LoadScene(2/*로비씬*/);
                }
                return;
            }
            // else
            _loginPanel.SetActive(true);
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
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
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

                Firebase.Auth.FirebaseUser newUser = task.Result.User;
                string uid = newUser.UserId;

                Debug.LogFormat($"게스트 로그인 성공 : {newUser.UserId}");

                Manager.DB.GetUserPath(uid)
                .Child("heart")
                .Child("currentHeart")
                .SetValueAsync(Manager.Heart.GetMaxHearts());

                Manager.DB.GetUserPath(uid)
                .Child("heart")
                .Child("lastSaveTime")
                .SetValueAsync(DateTime.Now.ToString("O"));

                Manager.DB.GetUserPath(uid)
                .Child("heart")
                .Child("remainingSeconds")
                .SetValueAsync(0);

                Manager.DB.GetUserPath(uid)
                .Child("playerName")
                .SetValueAsync("Guest");

                Manager.Stage.LoadStage();
                //DontDSystem.StatGame();
                SceneManager.LoadScene(2/*로비씬*/);

            });
            
        }
    }
}

