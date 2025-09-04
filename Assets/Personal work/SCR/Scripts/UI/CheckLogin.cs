using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
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

        private void CheckBefore()
        {

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
            _enterPanel.SetActive(false);
            _loginPanel.SetActive(true);
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

                FirebaseUser newUser = task.Result.User;
                string uid = newUser.UserId;

                Debug.LogFormat($"게스트 로그인 성공 : {newUser.UserId}");

                NewUser(uid, DateTime.Now.ToString("O"));

                Manager.Stage.LoadStage();
                //DontDSystem.StatGame();
                //SceneManager.LoadScene("Lobby Scene");

            });

        }

        public async void NewUser(string uid, string now)
        {
            // 1. 단일 데이터 모델 객체 생성 및 값 설정
            var newUserData = new UserData();
            newUserData.UserInfo.Heart.currentHeart = 5;
            newUserData.UserInfo.Heart.lastSaveTime = now;
            // 나머지 속성들은 기본값으로 자동 초기화됩니다.
            // 2. 단일 쓰기 작업을 실행하고 완료를 기다림

            string json = JsonConvert.SerializeObject(newUserData);
            try
            {
                Debug.Log("유저 생성 시작3");
                await Manager.DB.GetUserPath(uid).SetRawJsonValueAsync(json);
                Debug.Log("새로운 유저 데이터가 성공적으로 생성되었습니다.");
            }
            catch (Exception ex)
            {
                // 3. 오류 발생 시 예외 처리
                Debug.LogError("유저 데이터 생성 실패: " + ex.Message);
            }
        }
    }
}

